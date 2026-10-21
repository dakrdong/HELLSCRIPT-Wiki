using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // Storage artwork follows the shared button state. Selection and transactions remain in StorageWindow.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class StorageChestGraphic : MaskableGraphic
    {
        static Sprite closedArt, openArt;
        UiButton button;
        StorageChestOverlay overlay;
        int lastState = -1;
        public bool Owned { get; private set; }
        public bool Purchasable { get; private set; }
        public bool IsOpen => Owned && button != null && button.Chosen;
        public float TransferHover { get; private set; }
        public Sprite ArtSprite
        {
            get
            {
                if (IsOpen) return openArt != null ? openArt : openArt = Resources.Load<Sprite>("Art/Storage/chest-open");
                return closedArt != null ? closedArt : closedArt = Resources.Load<Sprite>("Art/Storage/chest-closed");
            }
        }
        public override Texture mainTexture => ArtSprite != null ? ArtSprite.texture : Texture2D.whiteTexture;

        // Approved replacement canvases already include their transparent margin.
        public Rect ArtUv => new Rect(0, 0, 1, 1);
        public Rect ArtRect
        {
            get
            {
                var r = GetPixelAdjustedRect();
                float aspect = ArtSprite != null ? (float)ArtSprite.texture.width / ArtSprite.texture.height : 1;
                float width = Mathf.Min(r.width, r.height * aspect);
                float height = width / aspect;
                return new Rect(r.center.x - width / 2, r.center.y - height / 2 - PressOffset, width, height);
            }
        }
        internal float PressOffset => button != null && button.Face != null && button.Face.Pressed ? rectTransform.rect.height * 1.6f / 52 : 0;

        public void ShowTransferHover(float progress)
        {
            progress = Mathf.Clamp01(progress);
            if (Mathf.Approximately(progress, TransferHover)) return;
            TransferHover = progress;
            if (overlay != null) overlay.SetVerticesDirty();
        }
        public void Bind(UiButton control, bool owned, bool purchasable, bool badges = true)
        {
            button = control; Owned = owned; Purchasable = purchasable; raycastTarget = false;
            if (overlay == null && badges)
            {
                var r = UiLayout.Rect("Chest status", transform);
                r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
                overlay = r.gameObject.AddComponent<StorageChestOverlay>(); overlay.Bind(this);
            }
            lastState = -1;
            SetVerticesDirty(); SetMaterialDirty(); if (overlay != null) overlay.SetVerticesDirty();
        }
        void LateUpdate()
        {
            if (button == null || button.Face == null) return;
            int state = (IsOpen ? 1 : 0) | (button.Face.Hovered ? 2 : 0) | (button.Face.Pressed ? 4 : 0)
                | (button.Face.Focused ? 8 : 0) | (button.Face.Available ? 16 : 0);
            if (state == lastState) return;
            lastState = state; SetVerticesDirty(); SetMaterialDirty();
            if (overlay != null) overlay.SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (button == null || button.Face == null || ArtSprite == null) return;
            float brightness = Owned ? (IsOpen ? .95f : .86f) : Purchasable ? .72f : .42f;
            if (button.Face.Hovered || button.Face.Focused) brightness = Mathf.Min(1, brightness + .12f);
            if (!button.Face.Available) brightness = .35f;
            var tint = new Color(brightness, brightness, brightness, 1);
            var r = ArtRect; var uv = ArtUv;
            mesh.AddVert(new Vector3(r.xMin, r.yMin), tint, new Vector2(uv.xMin, uv.yMin));
            mesh.AddVert(new Vector3(r.xMin, r.yMax), tint, new Vector2(uv.xMin, uv.yMax));
            mesh.AddVert(new Vector3(r.xMax, r.yMax), tint, new Vector2(uv.xMax, uv.yMax));
            mesh.AddVert(new Vector3(r.xMax, r.yMin), tint, new Vector2(uv.xMax, uv.yMin));
            mesh.AddTriangle(0, 1, 2); mesh.AddTriangle(2, 3, 0);
        }
    }

    // Untextured badges stay above the raster art and never intercept pointer input.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class StorageChestOverlay : MaskableGraphic
    {
        StorageChestGraphic chest;
        public void Bind(StorageChestGraphic owner) { chest = owner; raycastTarget = false; SetVerticesDirty(); }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); if (chest == null) return;
            // Casual badges: a navy ring around a green plus (purchasable) or a grey padlock (locked).
            var skin = UiSkin.Casual;
            if (chest.TransferHover > 0 && chest.Owned && !chest.IsOpen)
                Stroke(mesh, skin.Gold, 2, 10,50, 10+44*chest.TransferHover,50);
            if (!chest.Owned)
            {
                // Purchase badge and padlock occupy the same position but have distinct shapes.
                Poly(mesh, skin.Navy, 41,30, 54,30, 59,35, 59,46, 44,48, 39,43, 39,35);
                if (chest.Purchasable)
                {
                    Poly(mesh, skin.Green, 43,32, 54,32, 57,36, 57,43, 45,46, 41,42, 41,36);
                    Stroke(mesh, skin.White, 2, 45,39, 53,39); Stroke(mesh, skin.White, 2, 49,35, 49,43);
                }
                else
                {
                    Poly(mesh, skin.Disabled, 43,32, 54,32, 57,36, 57,43, 45,46, 41,42, 41,36);
                    Stroke(mesh, skin.Muted, 2, 46,38, 46,35, 48,33, 51,33, 53,35, 53,38);
                    Poly(mesh, skin.Muted, 44,38, 55,38, 55,44, 44,44);
                    Stroke(mesh, skin.Disabled, 1.5f, 49,40, 49,42);
                }
            }
        }
        Vector3 Point(float x, float y)
        {
            var r = GetPixelAdjustedRect();
            return new Vector3(r.xMin + x * r.width / 64, r.yMax - y * r.height / 52 - chest.PressOffset);
        }
        void Poly(VertexHelper mesh, Color tint, params float[] points)
        {
            int first = mesh.currentVertCount;
            for (int n = 0; n < points.Length; n += 2) mesh.AddVert(Point(points[n],points[n+1]), tint, Vector2.zero);
            for (int n = 1; n < points.Length / 2 - 1; n++) mesh.AddTriangle(first,first+n,first+n+1);
        }
        void Stroke(VertexHelper mesh, Color tint, float width, params float[] points)
        {
            for (int n = 0; n < points.Length - 2; n += 2)
            {
                var a = new Vector2(points[n],points[n+1]); var b = new Vector2(points[n+2],points[n+3]);
                var normal = new Vector2(-(b-a).y,(b-a).x).normalized * width / 2;
                Poly(mesh,tint,a.x-normal.x,a.y-normal.y,a.x+normal.x,a.y+normal.y,b.x+normal.x,b.y+normal.y,b.x-normal.x,b.y-normal.y);
            }
        }
    }
}
