using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // A closed, inset rectangle. Unlike Outline, it never depends on pixels outside a clipped slot.
    [RequireComponent(typeof(Graphic))]
    public sealed class UIRectBorder : BaseMeshEffect
    {
        public Color color = new Color(.40f, .39f, .31f, 1);
        public float width = 1;

        public override void ModifyMesh(VertexHelper mesh)
        {
            if (!IsActive()) return;
            var rect = graphic.GetPixelAdjustedRect();
            float scale = graphic.canvas != null ? graphic.canvas.scaleFactor : 1;
            float edge = Mathf.Min(Mathf.Max(1, Mathf.Round(width * scale)) / scale, Mathf.Min(rect.width, rect.height) * .5f);
            Add(mesh, rect.xMin, rect.yMin, rect.xMax, rect.yMin + edge);
            Add(mesh, rect.xMin, rect.yMax - edge, rect.xMax, rect.yMax);
            Add(mesh, rect.xMin, rect.yMin + edge, rect.xMin + edge, rect.yMax - edge);
            Add(mesh, rect.xMax - edge, rect.yMin + edge, rect.xMax, rect.yMax - edge);
        }

        void Add(VertexHelper mesh, float left, float bottom, float right, float top)
        {
            int first = mesh.currentVertCount;
            mesh.AddVert(new Vector3(left, bottom), color, Vector2.zero);
            mesh.AddVert(new Vector3(left, top), color, Vector2.zero);
            mesh.AddVert(new Vector3(right, top), color, Vector2.zero);
            mesh.AddVert(new Vector3(right, bottom), color, Vector2.zero);
            mesh.AddTriangle(first, first + 1, first + 2);
            mesh.AddTriangle(first, first + 2, first + 3);
        }
    }
}
