using System.Collections.Generic;
using UnityEngine;

namespace Hellscript
{
    // All native allocations are owned by the dungeon root and released when it is rebuilt.
    public sealed class RiftFogView : MonoBehaviour
    {
        public RiftVisibility Visibility { get; private set; }
        public Texture2D Texture { get; private set; }
        readonly Dictionary<Material, Material> objects = new Dictionary<Material, Material>();
        readonly Dictionary<Material, Material> terrain = new Dictionary<Material, Material>();
        readonly Dictionary<Material, Material> originals = new Dictionary<Material, Material>();
        Shader shader;
        int uploaded = -1;
        public void Initialize(RiftVisibility visibility)
        {
            Visibility = visibility; obstacleProperties = new MaterialPropertyBlock();
            shader = Resources.Load<Shader>("RiftTerrain");
            if (shader == null || !shader.isSupported) throw new System.InvalidOperationException("Rift visibility shader is unavailable.");
            Texture = new Texture2D(visibility.Width, visibility.Height, TextureFormat.RGBA32, false, true)
            { name = "Rift explored and visible mask", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            Present();
        }
        public Material Resolve(Material original, bool remember = false)
        {
            if (originals.TryGetValue(original, out var resolved)) original = resolved;
            var cache = remember ? terrain : objects;
            if (cache.TryGetValue(original, out var found)) return found;
            var material = new Material(shader) { name = original.name + (remember ? " explored terrain" : " current sight") };
            var color = original.HasProperty("_BaseColor") ? original.GetColor("_BaseColor") : original.color;
            material.SetColor("_BaseColor", color);
            material.SetColor("_EmissionColor", original.IsKeywordEnabled("_EMISSION") && original.HasProperty("_EmissionColor") ? original.GetColor("_EmissionColor") : Color.black);
            if (original.mainTexture != null) material.SetTexture("_BaseMap", original.mainTexture);
            material.SetFloat("_RememberTerrain", remember ? 1 : 0);
            material.SetTexture("_FogTex", Texture);
            material.SetVector("_FogBounds", new Vector4(Visibility.Origin.x, Visibility.Origin.y, Visibility.Width * RiftVisibility.CellSize, Visibility.Height * RiftVisibility.CellSize));
            material.SetVector("_FogSize", new Vector4(Visibility.Width, Visibility.Height, 0, 0));
            cache.Add(original, material); originals.Add(material, original); return material;
        }
        public void Bind(Renderer renderer, bool remember = false)
        {
            var source = renderer.sharedMaterials;
            for (int i = 0; i < source.Length; i++) if (source[i] != null) source[i] = Resolve(source[i], remember);
            renderer.sharedMaterials = source;
        }
        readonly List<(Renderer renderer, RiftObstacle obstacle)> obstacles = new List<(Renderer, RiftObstacle)>();
        MaterialPropertyBlock obstacleProperties;
        public void ObserveObstacle(Renderer renderer, RiftObstacle obstacle)
        { obstacles.Add((renderer, obstacle)); PresentObstacle(renderer, obstacle); }
        void PresentObstacle(Renderer renderer, RiftObstacle obstacle)
        {
            if (renderer == null) return;
            var origin = Visibility.Run.position; var delta = origin - obstacle.position;
            var near = obstacle.radius > 0 ? obstacle.position + delta.normalized * obstacle.radius : obstacle.position + new Vector2(Mathf.Clamp(delta.x, -obstacle.halfSize.x, obstacle.halfSize.x), Mathf.Clamp(delta.y, -obstacle.halfSize.y, obstacle.halfSize.y));
            near += (origin - near).normalized * .04f;
            // The first face of a sight-blocking pillar is visible even though its centre is not.
            obstacleProperties.SetFloat("_ObjectVisibility", Visibility.Visible(near) ? 1 : 0); renderer.SetPropertyBlock(obstacleProperties);
        }
        public void Present()
        {
            Visibility.Update();
            if (uploaded == Visibility.Revision) return;
            foreach (var entry in obstacles) PresentObstacle(entry.renderer, entry.obstacle);
            Texture.SetPixels32(Visibility.Pixels); Texture.Apply(false, false); uploaded = Visibility.Revision;
        }
        void OnDestroy()
        {
            if (Texture != null) Destroy(Texture);
            foreach (var material in objects.Values) Destroy(material);
            foreach (var material in terrain.Values) Destroy(material);
        }
    }
}
