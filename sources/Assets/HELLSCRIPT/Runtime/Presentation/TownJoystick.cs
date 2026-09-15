using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hellscript
{
    // Pointer ownership keeps a second thumb on the interaction button from stealing movement.
    public sealed class TownJoystick : MonoBehaviour,IPointerDownHandler,IDragHandler,IPointerUpHandler
    {
        public RectTransform Knob;
        public Vector2 Value {get;private set;}
        int? pointer;
        public void OnPointerDown(PointerEventData e){if(pointer.HasValue)return;pointer=e.pointerId;OnDrag(e);}
        public void OnDrag(PointerEventData e)
        {
            if(pointer!=e.pointerId)return;var rect=(RectTransform)transform;
            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,e.position,e.pressEventCamera,out var point))return;
            float radius=rect.rect.width*.32f;Value=Vector2.ClampMagnitude((point-rect.rect.center)/radius,1);
            if(Value.magnitude<.12f)Value=Vector2.zero;Knob.anchoredPosition=Value*radius;
        }
        public void OnPointerUp(PointerEventData e){if(pointer==e.pointerId)ResetInput();}
        public void ResetInput(){pointer=null;Value=Vector2.zero;if(Knob!=null)Knob.anchoredPosition=Vector2.zero;}
        void OnDisable()=>ResetInput();
        void OnApplicationFocus(bool focused){if(!focused)ResetInput();}
        void OnApplicationPause(bool paused){if(paused)ResetInput();}
    }
    // Vector UI circles remain crisp at every resolution; no generated bitmap is needed.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class TownCircleGraphic : MaskableGraphic
    {
        public float InnerRatio;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var rect=rectTransform.rect;var center=rect.center;float radius=Mathf.Min(rect.width,rect.height)/2;
            const int segments=64;
            for(int i=0;i<=segments;i++)
            {
                float a=i*Mathf.PI*2/segments;var dir=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                vh.AddVert(center+dir*radius,color,Vector2.zero);vh.AddVert(center+dir*radius*InnerRatio,color,Vector2.zero);
                if(i==0)continue;int n=i*2;vh.AddTriangle(n-2,n,n-1);vh.AddTriangle(n,n+1,n-1);
            }
        }
    }
}
