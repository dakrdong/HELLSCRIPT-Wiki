"""Shared Blender helpers for HELLSCRIPT's procedural 3D art (runs inside `blender -b`).

Every model is built from seeded geometry and procedural shader graphs, then baked to a small
texture set Unity reads at runtime:

    <Name>_A.png  albedo RGB (ambient occlusion multiplied in), alpha = smoothness
    <Name>_N.png  tangent-space normal map (OpenGL / +Y, what Unity expects)

Parts whose object name ends in "_Glow" are exported untextured; Unity gives them an emissive
material. Empties named "Pivot_*" become transform pivots the runtime animates.

Conventions (Unity after import): 1 Blender unit = 1 m = 1 Unity unit, characters face Unity +Z,
feet at y=0. In Blender that means: face -Y, feet on z=0 (the FBX export converts the axes).
No third-party meshes, textures, scans or game assets are used anywhere.
"""
import hashlib
import json
import math
import os
import random
import sys
from pathlib import Path

import bmesh
import bpy
from mathutils import Euler, Matrix, Vector

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "Assets/HELLSCRIPT/Resources/World"
REVISION = "hellscript-art3d-v1"

# Game camera looks along this Unity vector (WorldView.CameraPosition offset (12,25,-18)).
# Unity (x, y, z) -> Blender (x, z, y) with Unity +Z = Blender -Y.
GAME_VIEW = Vector((-12, 18, -25)).normalized()  # Blender space: Unity (-12,-25,18)


# ---------------------------------------------------------------- scene

def seed_for(asset_id):
    """Recipe seed: int(sha256("hellscript-art3d-v1|<id>")[:8], 16), the formula ASSETS.md gives recipes."""
    return int(hashlib.sha256(f"{REVISION}|{asset_id}".encode()).hexdigest()[:8], 16)


def reset(seed=0):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.unit_settings.system = 'METRIC'
    scene.unit_settings.scale_length = 1
    random.seed(seed)
    return scene


def use_gpu(scene=None):
    scene = scene or bpy.context.scene
    scene.render.engine = 'CYCLES'
    prefs = bpy.context.preferences.addons['cycles'].preferences
    try:
        prefs.compute_device_type = 'METAL'
        prefs.get_devices()
        for d in prefs.devices:
            d.use = True
        scene.cycles.device = 'GPU'
    except Exception:  # CPU fallback keeps the build deterministic, only slower
        scene.cycles.device = 'CPU'


def link(obj, parent=None):
    if obj.name not in bpy.context.scene.collection.objects:
        bpy.context.scene.collection.objects.link(obj)
    if parent is not None:
        # Locations passed to the builders are world-space; keep them when parenting.
        bpy.context.view_layer.update()
        obj.parent = parent
        obj.matrix_parent_inverse = parent.matrix_world.inverted()
    return obj


def empty(name, location=(0, 0, 0), parent=None):
    e = bpy.data.objects.new(name, None)
    e.empty_display_size = .15
    e.location = location
    return link(e, parent)


def from_bmesh(name, bm, parent=None, location=(0, 0, 0), smooth=True):
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    for poly in me.polygons:
        poly.use_smooth = smooth
    obj = bpy.data.objects.new(name, me)
    obj.location = location
    return link(obj, parent)


def select_only(objs, active=None):
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = active or objs[0]


# ---------------------------------------------------------------- geometry

def box(name, size, location=(0, 0, 0), parent=None, bevel=0.0, segments=2):
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1)
    bmesh.ops.scale(bm, vec=Vector(size), verts=bm.verts)
    obj = from_bmesh(name, bm, parent, location, smooth=False)
    if bevel > 0:
        add_bevel(obj, bevel, segments)
    return obj


def cylinder(name, radius, depth, location=(0, 0, 0), parent=None, segments=16, radius2=None, cap=True, smooth=True):
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=cap, cap_tris=False, segments=segments,
                          radius1=radius, radius2=radius if radius2 is None else radius2, depth=depth)
    return from_bmesh(name, bm, parent, location, smooth)


def sphere(name, radius, location=(0, 0, 0), parent=None, u=16, v=10, scale=(1, 1, 1)):
    bm = bmesh.new()
    bmesh.ops.create_uvsphere(bm, u_segments=u, v_segments=v, radius=radius)
    bmesh.ops.scale(bm, vec=Vector(scale), verts=bm.verts)
    return from_bmesh(name, _fixed_face_order(bm), parent, location)


def _fixed_face_order(bm):
    """create_uvsphere's face and loop order varies between runs (same vertices); rebuild the faces in a fixed order,
    each starting at its lowest vertex with the winding kept, so UVs and exports are reproducible."""
    bm.verts.index_update()
    faces = []
    for f in bm.faces:
        idx = [v.index for v in f.verts]
        k = idx.index(min(idx))
        faces.append(idx[k:] + idx[:k])
    out = bmesh.new()
    verts = [out.verts.new(v.co) for v in bm.verts]
    for f in sorted(faces):
        out.faces.new([verts[i] for i in f])
    bm.free()
    return out


def lathe(name, profile, segments=16, location=(0, 0, 0), parent=None, smooth=True):
    """Revolve [(radius, z), ...] around Z. Radii of 0 close the ends."""
    bm = bmesh.new()
    rings = []
    for r, z in profile:
        ring = []
        for s in range(segments):
            a = 2 * math.pi * s / segments
            ring.append(bm.verts.new((math.cos(a) * r, math.sin(a) * r, z)))
        rings.append(ring)
    for a, b in zip(rings, rings[1:]):
        for s in range(segments):
            t = (s + 1) % segments
            quad = [a[s], a[t], b[t], b[s]]
            try:
                bm.faces.new(quad)
            except ValueError:
                pass
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-5)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    return from_bmesh(name, bm, parent, location, smooth)


def tube(name, points, radii, segments=10, parent=None, smooth=True):
    """Sweep a circle along a polyline with per-point radius (limbs, horns, chains, tentacles)."""
    bm = bmesh.new()
    pts = [Vector(p) for p in points]
    rings = []
    for i, p in enumerate(pts):
        d = (pts[min(i + 1, len(pts) - 1)] - pts[max(i - 1, 0)]).normalized()
        up = Vector((0, 0, 1)) if abs(d.z) < .95 else Vector((1, 0, 0))
        x = d.cross(up).normalized()
        y = d.cross(x).normalized()
        r = radii[i] if isinstance(radii, (list, tuple)) else radii
        rings.append([bm.verts.new(p + (x * math.cos(2 * math.pi * s / segments) + y * math.sin(2 * math.pi * s / segments)) * r)
                      for s in range(segments)])
    for a, b in zip(rings, rings[1:]):
        for s in range(segments):
            t = (s + 1) % segments
            bm.faces.new([a[s], a[t], b[t], b[s]])
    bm.faces.new(list(reversed(rings[0])))
    bm.faces.new(rings[-1])
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    return from_bmesh(name, bm, parent, smooth=smooth)


def add_bevel(obj, width, segments=2):
    m = obj.modifiers.new("Bevel", 'BEVEL')
    m.width = width
    m.segments = segments
    m.limit_method = 'ANGLE'
    return m


def subdivide(obj, levels=1):
    m = obj.modifiers.new("Subsurf", 'SUBSURF')
    m.levels = levels
    m.render_levels = levels
    return m


def displace(obj, strength=.05, scale=1.0, seed=0, kind='CLOUDS', mid=.5):
    tex = bpy.data.textures.new(f"{obj.name}_disp", kind)
    if hasattr(tex, 'noise_scale'):
        tex.noise_scale = scale
    if hasattr(tex, 'noise_depth'):
        tex.noise_depth = 2
    m = obj.modifiers.new("Displace", 'DISPLACE')
    m.texture = tex
    m.strength = strength
    m.mid_level = mid
    m.texture_coords = 'OBJECT'
    helper = bpy.data.objects.new(f"{obj.name}_dispcoords", None)
    helper.location = (seed * 13.37 % 97, seed * 7.77 % 89, seed * 3.1 % 83)
    link(helper)
    m.texture_coords_object = helper
    return m


def apply_modifiers(obj):
    select_only([obj])
    for m in list(obj.modifiers):
        bpy.ops.object.modifier_apply(modifier=m.name)


def join(objs, name):
    select_only(objs, objs[0])
    bpy.ops.object.join()
    obj = bpy.context.view_layer.objects.active
    obj.name = name
    obj.data.name = name
    return obj


def mirror_x(obj):
    m = obj.modifiers.new("Mirror", 'MIRROR')
    m.use_axis[0] = True
    return m


def decimate(obj, ratio):
    m = obj.modifiers.new("Decimate", 'DECIMATE')
    m.ratio = ratio
    return m


def triangles(objs):
    total = 0
    deps = bpy.context.evaluated_depsgraph_get()
    for o in objs:
        if o.type != 'MESH':
            continue
        me = o.evaluated_get(deps).to_mesh()
        total += sum(len(p.vertices) - 2 for p in me.polygons)
        o.evaluated_get(deps).to_mesh_clear()
    return total


# ---------------------------------------------------------------- materials (bake sources)

class Graph:
    """Tiny node-graph builder so recipes read as material descriptions, not node plumbing."""

    def __init__(self, name):
        self.mat = bpy.data.materials.new(name)
        self.mat.use_nodes = True
        self.nt = self.mat.node_tree
        self.bsdf = self.nt.nodes["Principled BSDF"]
        self.coord = self.node("ShaderNodeTexCoord")

    def node(self, kind, **inputs):
        n = self.nt.nodes.new(kind)
        for k, v in inputs.items():
            if k in n.inputs:
                self.set(n.inputs[k], v)
            else:
                setattr(n, k, v)
        return n

    def set(self, socket, value):
        if hasattr(value, 'bl_idname') or isinstance(value, bpy.types.NodeSocket):
            out = value if isinstance(value, bpy.types.NodeSocket) else value.outputs[0]
            self.nt.links.new(out, socket)
        else:
            if isinstance(value, tuple) and len(value) == 3 and len(socket.default_value) == 4:
                value = (*value, 1.0)
            socket.default_value = value

    def noise(self, scale=5, detail=6, rough=.55, space='Object', w=0.0, distortion=0.0):
        n = self.node("ShaderNodeTexNoise", Scale=scale, Detail=detail, Roughness=rough, Distortion=distortion)
        n.noise_dimensions = '4D'
        n.inputs['W'].default_value = w
        self.nt.links.new(self.coord.outputs[space], n.inputs['Vector'])
        return n

    def voronoi(self, scale=5, feature='F1', space='Object', randomness=1.0, w=0.0):
        n = self.node("ShaderNodeTexVoronoi", Scale=scale, Randomness=randomness)
        n.voronoi_dimensions = '4D'
        n.feature = feature
        n.inputs['W'].default_value = w
        self.nt.links.new(self.coord.outputs[space], n.inputs['Vector'])
        return n

    def ramp(self, fac, stops):
        """stops: [(position, (r,g,b)), ...] in linear colour."""
        n = self.node("ShaderNodeValToRGB")
        cr = n.color_ramp
        while len(cr.elements) > 1:
            cr.elements.remove(cr.elements[-1])
        cr.elements[0].position = stops[0][0]
        cr.elements[0].color = (*stops[0][1], 1)
        for pos, col in stops[1:]:
            e = cr.elements.new(pos)
            e.color = (*col, 1)
        self.set(n.inputs['Fac'], out(fac, 'Fac'))
        return n

    def math(self, op, a, b=0.0, clamp=False):
        n = self.node("ShaderNodeMath", operation=op, use_clamp=clamp)
        self.set(n.inputs[0], out(a, 'Value'))
        self.set(n.inputs[1], out(b, 'Value'))
        return n

    def mix(self, fac, a, b, blend='MIX'):
        n = self.nt.nodes.new("ShaderNodeMix")
        n.data_type = 'RGBA'
        n.blend_type = blend
        self.set(n.inputs['Factor'], out(fac, 'Value'))
        self.set(n.inputs[6], out(a, 'Color'))
        self.set(n.inputs[7], out(b, 'Color'))
        return n

    def bump(self, height, strength=.5, distance=.02, normal=None):
        n = self.node("ShaderNodeBump", Strength=strength, Distance=distance)
        self.set(n.inputs['Height'], out(height, 'Value'))
        if normal is not None:
            self.set(n.inputs['Normal'], out(normal, 'Normal'))
        return n

    def finish(self, color, roughness=.6, normal=None, metallic=0.0):
        self.set(self.bsdf.inputs['Base Color'], out(color, 'Color'))
        self.set(self.bsdf.inputs['Roughness'], out(roughness, 'Value') if not isinstance(roughness, float) else roughness)
        self.set(self.bsdf.inputs['Metallic'], metallic if isinstance(metallic, float) else out(metallic, 'Value'))
        if normal is not None:
            self.set(self.bsdf.inputs['Normal'], out(normal, 'Normal'))
        return self.mat


def out(node, preferred):
    """Pick a node's output socket by preferred name, falling back to the first output."""
    if isinstance(node, (int, float, tuple, list)):
        return node
    if isinstance(node, bpy.types.NodeSocket):
        return node
    for name in (preferred, 'Result', 'Color', 'Value', 'Fac', 'Distance', 'Normal'):
        if name in node.outputs and node.outputs[name].enabled:
            return node.outputs[name]
    return node.outputs[0]


def lin(hex_or_rgb):
    """sRGB hex/tuple -> linear tuple for node colours."""
    if isinstance(hex_or_rgb, str):
        h = hex_or_rgb.lstrip('#')
        c = tuple(int(h[i:i + 2], 16) / 255 for i in (0, 2, 4))
    else:
        c = hex_or_rgb
    return tuple(((v + .055) / 1.055) ** 2.4 if v > .04045 else v / 12.92 for v in c)


def surface(name, dark, light, scale=6, rough=(.55, .85), bump=.85, grime=.35, metallic=0.0, seed=0,
            pattern=None, pattern_scale=12, wear=None, bump_distance=.06):
    """General worn surface: two-tone noise, cavity grime, optional scratches/edge wear.

    pattern: None | 'scales' | 'rivets' | 'fibres' | 'cracks' | 'veins' | 'planks'
    """
    g = Graph(name)
    w = seed * .731
    base = g.noise(scale, 8, .6, w=w)
    fine = g.noise(scale * 7, 4, .7, w=w + 3)
    col = g.ramp(base, [(0.25, lin(dark)), (0.75, lin(light))])
    height = g.math('MULTIPLY', fine, .35)
    if pattern == 'scales':
        v = g.voronoi(pattern_scale, 'F1', w=w)
        height = g.math('ADD', height, g.math('POWER', v, .5))
    elif pattern == 'rivets':
        v = g.voronoi(pattern_scale, 'F1', randomness=0.0, w=w)
        height = g.math('ADD', height, g.math('LESS_THAN', v, .12))
    elif pattern == 'fibres':
        n = g.node("ShaderNodeTexWave", Scale=pattern_scale, Distortion=4, Detail=4)
        g.nt.links.new(g.coord.outputs['Object'], n.inputs['Vector'])
        height = g.math('ADD', height, g.math('MULTIPLY', n, .4))
    elif pattern in ('cracks', 'veins'):
        v = g.voronoi(pattern_scale, 'DISTANCE_TO_EDGE', w=w)
        line = g.math('LESS_THAN', v, .035 if pattern == 'cracks' else .06)
        height = g.math('SUBTRACT', height, g.math('MULTIPLY', line, .8))
        if pattern == 'veins':
            col = g.mix(line, col, lin(wear or '#b0301c'))
    elif pattern == 'planks':
        n = g.node("ShaderNodeTexWave", Scale=pattern_scale, Distortion=1.5, Detail=6, wave_type='BANDS')
        g.nt.links.new(g.coord.outputs['Object'], n.inputs['Vector'])
        height = g.math('ADD', height, g.math('MULTIPLY', n, .3))
        col = g.mix(g.math('MULTIPLY', n, .5), col, lin(dark), 'MULTIPLY')
    # Grime gathers in cavities (pointiness is geometric curvature).
    geo = g.node("ShaderNodeNewGeometry")
    cav = g.node("ShaderNodeMapRange", **{'From Min': .45, 'From Max': .55, 'To Min': 1.0, 'To Max': 0.0})
    g.set(cav.inputs['Value'], geo.outputs['Pointiness'])
    grime_mask = g.math('MULTIPLY', cav, grime, clamp=True)
    col = g.mix(grime_mask, col, lin('#0b0806'))
    if wear:
        edge = g.node("ShaderNodeMapRange", **{'From Min': .52, 'From Max': .6, 'To Min': 0.0, 'To Max': 1.0})
        g.set(edge.inputs['Value'], geo.outputs['Pointiness'])
        col = g.mix(g.math('MULTIPLY', edge, g.math('GREATER_THAN', fine, .45)), col, lin(wear))
    r = g.ramp(fine, [(0, (rough[0],) * 3), (1, (rough[1],) * 3)])
    b = g.bump(height, bump, bump_distance)
    return g.finish(col, r, b, metallic)


def glow(name, color, strength=6.0):
    g = Graph(name)
    em = g.node("ShaderNodeEmission", Strength=strength)
    em.inputs['Color'].default_value = (*lin(color), 1)
    g.nt.links.new(em.outputs[0], g.nt.nodes["Material Output"].inputs['Surface'])
    return g.mat


def assign(obj, mat):
    obj.data.materials.clear()
    obj.data.materials.append(mat)
    return obj


# ---------------------------------------------------------------- UV + bake

def uv_unwrap(objs, margin=.004, angle=66):
    meshes = [o for o in objs if o.type == 'MESH' and not o.name.endswith('_Glow')]
    for o in meshes:
        apply_modifiers(o)
    select_only(meshes)
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.smart_project(angle_limit=math.radians(angle), island_margin=margin, scale_to_bounds=False)
    bpy.ops.uv.pack_islands(margin=margin, rotate=True)
    bpy.ops.object.mode_set(mode='OBJECT')
    return meshes


def bake_set(objs, name, folder, size=512, samples=16, ao=True, smoothness=True, supersample=2, ao_distance=.3):
    """Bake albedo(+AO)/smoothness and normal for every non-glow mesh onto one shared atlas.

    Colour, roughness and normal are baked at `supersample` x the output size and box-filtered
    down, because Cycles bakes one point per texel and high-frequency procedural bumps alias
    badly at 512 px otherwise. AO is low-frequency and bakes at the output size. Texels no island
    covers are filled from their neighbours so mipmaps never bleed black gutters into islands.
    """
    scene = bpy.context.scene
    use_gpu(scene)
    scene.cycles.samples = samples
    ss = max(1, int(supersample))
    scene.render.bake.margin = 16 * ss
    meshes = uv_unwrap(objs)
    images = {}
    for kind in ('color', 'rough', 'normal', 'ao'):
        side = size if kind == 'ao' else size * ss
        img = bpy.data.images.new(f"{name}_{kind}", side, side, alpha=False, float_buffer=kind == 'normal')
        if kind in ('normal', 'rough', 'ao'):
            img.colorspace_settings.name = 'Non-Color'
        images[kind] = img

    def target(img):
        for o in meshes:
            for slot in o.material_slots:
                nt = slot.material.node_tree
                node = nt.nodes.get("HS_BAKE") or nt.nodes.new("ShaderNodeTexImage")
                node.name = "HS_BAKE"
                node.image = img
                nt.nodes.active = node

    select_only(meshes)
    target(images['color'])
    bpy.ops.object.bake(type='DIFFUSE', pass_filter={'COLOR'})
    target(images['rough'])
    bpy.ops.object.bake(type='ROUGHNESS')
    target(images['normal'])
    bpy.ops.object.bake(type='NORMAL', normal_space='TANGENT')
    if ao:
        # Cycles' AO bake reaches world.light_settings.distance (10 m by default), at which every
        # part of a character occludes every other and albedo loses 30-50% of its value.
        if scene.world is None:
            scene.world = bpy.data.worlds.new("hs3d bake")
        scene.world.light_settings.distance = ao_distance
        scene.cycles.samples = max(samples, 32)
        target(images['ao'])
        bpy.ops.object.bake(type='AO')

    folder.mkdir(parents=True, exist_ok=True)
    w = size
    # A baked tangent normal always has z > 0; the untouched image stays (0,0,0).
    normal_hi = pixels(images['normal'])
    covered = downsample(normal_hi[:, 2:3] > .05, size, ss)[:, 0] > .5
    color = fill_gutters(downsample(pixels(images['color']), size, ss), covered, size)
    rough = fill_gutters(downsample(pixels(images['rough']), size, ss), covered, size)
    normal = fill_gutters(downsample(normal_hi, size, ss), covered, size)
    normal[:, :3] = renormalize(normal[:, :3])
    px = color.copy()
    if ao:
        occl = fill_gutters(pixels(images['ao']), covered, size)
        px[:, :3] *= (.35 + .65 * occl[:, :1])
    px[:, 3] = 1 - rough[:, 0] if smoothness else 1.0
    albedo = bpy.data.images.new(f"{name}_A", w, w, alpha=True)
    albedo.pixels.foreach_set(px.ravel())
    save(albedo, folder / f"{name}_A.png")
    nrm = bpy.data.images.new(f"{name}_N", w, w, alpha=False)
    nrm.colorspace_settings.name = 'Non-Color'
    nrm.pixels.foreach_set(normal.ravel())
    save(nrm, folder / f"{name}_N.png", non_color=True)
    return meshes


def downsample(px, size, factor):
    """Box-filter a (side*side, C) pixel array baked at size*factor down to size*size."""
    import numpy as np
    px = np.asarray(px, dtype=np.float32)
    if factor == 1:
        return px.reshape(size * size, -1)
    c = px.shape[-1]
    img = px.reshape(size, factor, size, factor, c)
    return img.mean(axis=(1, 3)).reshape(size * size, c)


def fill_gutters(px, covered, size, passes=None):
    """Grow covered texels outward (4-neighbour average) until every texel has a value."""
    import numpy as np
    px = np.asarray(px, dtype=np.float32).reshape(size, size, -1).copy()
    mask = np.asarray(covered, dtype=bool).reshape(size, size).copy()
    if mask.all() or not mask.any():
        return px.reshape(size * size, -1)
    for _ in range(passes or size):
        if mask.all():
            break
        acc = np.zeros_like(px)
        cnt = np.zeros(mask.shape, dtype=np.float32)
        for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            m = np.roll(mask, (dy, dx), axis=(0, 1))
            acc += np.roll(px, (dy, dx), axis=(0, 1)) * m[..., None]
            cnt += m
        grow = (~mask) & (cnt > 0)
        px[grow] = acc[grow] / cnt[grow][:, None]
        mask |= grow
    return px.reshape(size * size, -1)


def renormalize(rgb):
    """Re-normalise encoded tangent normals after filtering (0..1 encoding in, 0..1 out)."""
    import numpy as np
    v = rgb * 2 - 1
    n = np.linalg.norm(v, axis=1, keepdims=True)
    return (v / np.maximum(n, 1e-6) + 1) * .5


def pixels(img):
    import numpy as np
    buf = np.empty(img.size[0] * img.size[1] * 4, dtype=np.float32)
    img.pixels.foreach_get(buf)
    return buf.reshape(-1, 4)


def save(img, path, non_color=False):
    img.filepath_raw = str(path)
    img.file_format = 'PNG'
    scene = bpy.context.scene
    settings = scene.render.image_settings
    settings.color_depth = '8'
    settings.compression = 90
    img.save()


# ---------------------------------------------------------------- export + preview

def export_fbx(root, path):
    """Export  and its children as one FBX; Unity assigns materials at runtime.

    Blender's FBX axis baking only handles direct children correctly, so deeper objects are
    flattened under the root and renamed "Name@Parent". WorldArt.Instantiate re-parents them
    (keeping the world pose) and restores the plain name, so pivots animate in Unity axes.
    """
    path.parent.mkdir(parents=True, exist_ok=True)
    bpy.context.view_layer.update()
    objs = [root] + list(root.children_recursive)
    nested = [(o, o.parent.name, o.matrix_world.copy()) for o in objs[1:] if o.parent is not root]
    for o, parent, world in nested:
        o.parent = root
        o.matrix_parent_inverse = root.matrix_world.inverted()
        o.matrix_world = world
    for o, parent, _ in nested:
        o["hs_parent"] = parent
    for o, parent, _ in nested:
        o.name = f"{o.name}@{parent}"
    select_only(objs, root)
    _stable_fbx()
    bpy.ops.export_scene.fbx(filepath=str(path), use_selection=True, apply_unit_scale=True,
                             apply_scale_options='FBX_SCALE_ALL', axis_forward='-Z', axis_up='Y',
                             bake_space_transform=True, object_types={'EMPTY', 'MESH'},
                             use_mesh_modifiers=True, mesh_smooth_type='FACE', add_leaf_bones=False,
                             bake_anim=False, path_mode='STRIP', embed_textures=False, use_tspace=True)


def _stable_fbx():
    """The FBX exporter stamps the wall clock into the header and derives object UIDs from Python's per-process
    randomised hash(); pin both so rebuilding the same scene writes the same bytes."""
    import datetime
    from io_scene_fbx import export_fbx_bin as fbx, fbx_utils
    if getattr(fbx.fbx_header_elements, "hs_fixed", False):
        return
    header, key_to_uuid = fbx.fbx_header_elements, fbx_utils._key_to_uuid

    def fixed_header(root, scene_data, time=None):
        return header(root, scene_data, datetime.datetime(2000, 1, 1))

    def stable_uuid(uuids, key):  # an int in [0, 2**63) is used as is; the exporter's shortening and collision loop stay
        if not isinstance(key, int):
            key = int.from_bytes(hashlib.sha1(repr(key).encode()).digest()[:8], "little") % 2**63
        return key_to_uuid(uuids, key)
    fixed_header.hs_fixed = True
    fbx.fbx_header_elements, fbx_utils._key_to_uuid = fixed_header, stable_uuid


def use_baked(objs, folder, name):
    """Swap the procedural bake sources for the baked atlas, so previews show what Unity receives."""
    g = Graph(name + "_baked")
    albedo = g.node("ShaderNodeTexImage")
    albedo.image = bpy.data.images.load(str(Path(folder) / f"{name}_A.png"))
    tex = g.node("ShaderNodeTexImage")
    tex.image = bpy.data.images.load(str(Path(folder) / f"{name}_N.png"))
    tex.image.colorspace_settings.name = 'Non-Color'
    normal = g.node("ShaderNodeNormalMap")
    g.set(normal.inputs['Color'], tex.outputs['Color'])
    g.finish(albedo.outputs['Color'], g.math('SUBTRACT', 1.0, albedo.outputs['Alpha']), normal)
    for o in objs:  # export_fbx may already have renamed parts to Name@Parent
        if o.type == 'MESH' and not o.name.split('@')[0].endswith('_Glow'):
            assign(o, g.mat)


def preview(path, objs, size=512, angles=(0,), ortho=None, key=(1, .62, .32), rim=(.45, .6, 1)):
    """Render the model from the game's camera direction (and optional extra yaw angles)."""
    scene = bpy.context.scene
    scene.render.engine = 'BLENDER_EEVEE' if 'BLENDER_EEVEE' in {e.identifier for e in bpy.types.RenderSettings.bl_rna.properties['engine'].enum_items} else 'BLENDER_EEVEE_NEXT'
    scene.render.resolution_x = size * len(angles)
    scene.render.resolution_y = size
    scene.render.film_transparent = False
    world = bpy.data.worlds.new("preview") if scene.world is None else scene.world
    scene.world = world
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs[0].default_value = (.012, .014, .02, 1)
    lo = Vector((1e9,) * 3)
    hi = Vector((-1e9,) * 3)
    for o in objs:
        if o.type != 'MESH':
            continue
        for c in o.bound_box:
            p = o.matrix_world @ Vector(c)
            lo = Vector(map(min, lo, p))
            hi = Vector(map(max, hi, p))
    center = (lo + hi) / 2
    extent = max((hi - lo).length, .5)
    cam_data = bpy.data.cameras.new("preview")
    cam_data.type = 'ORTHO'
    cam_data.ortho_scale = ortho or extent * 1.15
    cam = link(bpy.data.objects.new("preview camera", cam_data))
    scene.camera = cam
    sun = bpy.data.lights.new("key", 'SUN')
    sun.energy = 3.2
    sun.color = key
    sun_obj = link(bpy.data.objects.new("key", sun))
    sun_obj.rotation_euler = Euler((math.radians(50), 0, math.radians(35)))
    back = bpy.data.lights.new("rim", 'SUN')
    back.energy = 2.0
    back.color = rim
    back_obj = link(bpy.data.objects.new("rim", back))
    back_obj.rotation_euler = Euler((math.radians(60), 0, math.radians(200)))
    frames = []
    for yaw in angles:
        d = Matrix.Rotation(math.radians(yaw), 3, 'Z') @ GAME_VIEW
        cam.location = center - d * extent * 3
        cam.rotation_euler = (-d).to_track_quat('Z', 'Y').to_euler()
        cam.rotation_euler = d.to_track_quat('-Z', 'Y').to_euler()
        frame = str(path) + f".{yaw}.png"
        scene.render.resolution_x = size
        scene.render.filepath = frame
        bpy.ops.render.render(write_still=True)
        frames.append(frame)
    stitch(frames, path)
    for f in frames:
        os.remove(f)
    for o in (cam, sun_obj, back_obj):
        bpy.data.objects.remove(o)


def stitch(frames, path):
    import numpy as np
    imgs = [bpy.data.images.load(f) for f in frames]
    w, h = imgs[0].size
    rows = np.concatenate([pixels(img).reshape(h, w, 4) for img in imgs], axis=1)
    sheet = bpy.data.images.new("sheet", w * len(imgs), h)
    sheet.pixels.foreach_set(rows.ravel())
    save(sheet, Path(path))


# ---------------------------------------------------------------- manifest + metas

def guid(rel):
    return hashlib.md5(("hellscript-art3d:" + rel).encode()).hexdigest()


def write_meta(path):
    """Minimal meta with a deterministic GUID; the importer settings live in WorldArtImport.cs."""
    rel = Path(path).resolve().relative_to(ROOT).as_posix()
    meta = Path(str(path) + ".meta")
    if meta.exists():
        return
    folder = Path(path).is_dir()
    body = "folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n" if folder else ""
    meta.write_text(f"fileFormatVersion: 2\nguid: {guid(rel)}\n{body}")


def metas(folder):
    folder = Path(folder)
    p = folder
    while p != OUT.parent and p != ROOT:
        write_meta(p)
        p = p.parent
    for f in sorted(folder.rglob('*')):
        if f.suffix != '.meta' and not f.name.startswith('.') and '.preview' not in f.name:
            write_meta(f)


def record(folder, name, info):
    """Merge one model's facts into <folder>/manifest.json (tri counts, sizes, seed, revision)."""
    folder.mkdir(parents=True, exist_ok=True)
    path = folder / "manifest.json"
    data = json.loads(path.read_text()) if path.exists() else {"revision": REVISION, "models": {}}
    data["models"][name] = info
    data["models"] = dict(sorted(data["models"].items()))
    path.write_text(json.dumps(data, indent=2, ensure_ascii=False) + "\n")


def args():
    """Arguments after `--` on the blender command line."""
    return sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
