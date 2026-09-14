using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hellscript
{
    // Resolve columns after the parent assigns width, before the vertical layout measures rows.
    public sealed class SettingsChoiceGrid : GridLayoutGroup
    {
        public override void CalculateLayoutInputHorizontal(){base.CalculateLayoutInputHorizontal();SetLayoutInputForAxis(0,-1,0,-1,0);}
        public override void SetLayoutHorizontal()
        {
            float width=rectTransform.rect.width;constraint=Constraint.FixedColumnCount;constraintCount=Mathf.Clamp(Mathf.FloorToInt((width+8)/92),2,5);
            spacing=new Vector2(8,8);cellSize=new Vector2((width-(constraintCount-1)*8)/constraintCount,46);base.SetLayoutHorizontal();
        }
    }
    public sealed class SettingsGearGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var rect=rectTransform.rect;float size=Mathf.Min(rect.width,rect.height)*.5f;const int samples=80;
            for(int i=0;i<=samples;i++)
            {
                float angle=i*Mathf.PI*2/samples;var direction=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                float radius=i%8>=2&&i%8<=5?1:.77f;
                vh.AddVert(rect.center+direction*size*radius,color,Vector2.zero);vh.AddVert(rect.center+direction*size*.36f,color,Vector2.zero);
                if(i==0)continue;int n=i*2;vh.AddTriangle(n-2,n,n-1);vh.AddTriangle(n,n+1,n-1);
            }
        }
    }
    // The track represents integer 5% steps; dragging never resizes the track underneath the pointer.
    public sealed class SettingsStepSlider : Slider,IScrollHandler
    {
        public bool Dragging {get;private set;}
        public Action Released;
        public override void OnPointerDown(PointerEventData e){Dragging=true;base.OnPointerDown(e);}
        public override void OnPointerUp(PointerEventData e){base.OnPointerUp(e);Dragging=false;Released?.Invoke();}
        public void OnScroll(PointerEventData e)
        {if(!IsInteractable()||Mathf.Abs(e.scrollDelta.y)<.01f)return;value+=Mathf.Sign(e.scrollDelta.y);e.Use();}
        protected override void OnDisable(){Dragging=false;base.OnDisable();}
    }
}
