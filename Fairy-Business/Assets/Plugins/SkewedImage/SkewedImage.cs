using UnityEngine;
using UnityEngine.UI;

namespace UIExtensions
{
    public class SkewedImage : Image
    {
        public Vector2 SkewVector;
        public float TrapezSkewTop = 1.0f;
        public float TrapezSkewBottom = 1.0f;
        
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            base.OnPopulateMesh(vh);
            var r = GetPixelAdjustedRect();
            var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
            float topWidthMod = (TrapezSkewTop - 1.0f) * r.width / 2.0f;
            float botWidhtMod = (TrapezSkewBottom - 1.0f) * r.width / 2.0f;
            Color32 color32 = color;
            vh.Clear();
            vh.AddVert(new Vector3(v.x - SkewVector.x - topWidthMod, v.y - SkewVector.y), color32, new Vector2(0f, 0f));
            vh.AddVert(new Vector3(v.x + SkewVector.x - botWidhtMod, v.w - SkewVector.y), color32, new Vector2(0f, 1f));
            vh.AddVert(new Vector3(v.z + SkewVector.x + botWidhtMod, v.w + SkewVector.y), color32, new Vector2(1f, 1f));
            vh.AddVert(new Vector3(v.z - SkewVector.x + topWidthMod, v.y + SkewVector.y), color32, new Vector2(1f, 0f));
            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }
    }
}