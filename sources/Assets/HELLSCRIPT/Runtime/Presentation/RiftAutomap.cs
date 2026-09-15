using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class RiftAutomap : MaskableGraphic
    {
        GameController game;
        RiftFogView fog;
        Material ownedMaterial;
        Vector2 axisX, axisY, lastSize;
        float scale;
        public void Initialize(GameController controller)
        {
            game = controller; fog = game.World.RiftFog; raycastTarget = false;
            ownedMaterial = CreateMapMaterial(fog); material = ownedMaterial;
        }
        public static Material CreateMapMaterial(RiftFogView fog)
        {
            var shader = Resources.Load<Shader>("RiftMap");
            if (shader == null || !shader.isSupported) throw new System.InvalidOperationException("Explored map shader is unavailable.");
            var material = new Material(shader) { name = "Explored map mask" };
            material.SetTexture("_FogTex", fog.Texture);
            material.SetVector("_FogSize", new Vector4(fog.Visibility.Width, fog.Visibility.Height, 0, 0));
            return material;
        }
        public static Vector2 UV(RiftVisibility view, Vector2 p) => new Vector2((p.x - view.Origin.x) / (view.Width * RiftVisibility.CellSize), (p.y - view.Origin.y) / (view.Height * RiftVisibility.CellSize));
        Vector2 Point(Vector2 p) => new Vector2(Vector2.Dot(p, axisX), Vector2.Dot(p, axisY)) * scale;
        void LateUpdate()
        {
            if (game == null || fog == null || game.DisplayDimmed) return;
            var size = ((RectTransform)transform.parent).rect.size; var camera = Camera.main;
            Vector2 x = new Vector2(camera.transform.right.x, camera.transform.right.z), y = new Vector2(camera.transform.up.x, camera.transform.up.z);
            if (size != lastSize || x != axisX || y != axisY)
            {
                lastSize = size; axisX = x; axisY = y; scale = Mathf.Min(size.x / 58, size.y / 40);
                float diameter = fog.Visibility.Navigation.Surface.Bounds.size.magnitude * scale + 40;
                rectTransform.sizeDelta = Vector2.one * diameter; SetVerticesDirty();
            }
            var p = game.World.PresentedHeroPosition;
            rectTransform.anchoredPosition = Point(fog.Visibility.Navigation.Surface.Bounds.center - new Vector2(p.x, p.z));
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); if (fog == null) return;
            var visibility = fog.Visibility; var center = visibility.Navigation.Surface.Bounds.center;
            foreach (var edge in visibility.Boundary)
            {
                var a = new Vector2(edge.x, edge.y); var b = new Vector2(edge.z, edge.w);
                var p = Point(a - center); var q = Point(b - center); var side = new Vector2(-(q - p).y, (q - p).x).normalized * .8f;
                int n = vh.currentVertCount; var color = new Color(.82f, .84f, .75f, .60f);
                vh.AddVert(p - side, color, UV(visibility, a)); vh.AddVert(p + side, color, UV(visibility, a));
                vh.AddVert(q + side, color, UV(visibility, b)); vh.AddVert(q - side, color, UV(visibility, b));
                vh.AddTriangle(n, n + 1, n + 2); vh.AddTriangle(n, n + 2, n + 3);
            }
        }
        protected override void OnDestroy() { if (ownedMaterial != null) Destroy(ownedMaterial); base.OnDestroy(); }
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class RiftAutomapHero : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); var c = new Color(1, .82f, .4f, .95f);
            vh.AddVert(new Vector2(0, 7), c, Vector2.zero); vh.AddVert(new Vector2(-5, -5), c, Vector2.zero);
            vh.AddVert(new Vector2(0, -2), c, Vector2.zero); vh.AddVert(new Vector2(5, -5), c, Vector2.zero);
            vh.AddTriangle(0, 1, 2); vh.AddTriangle(0, 2, 3);
        }
    }
}
