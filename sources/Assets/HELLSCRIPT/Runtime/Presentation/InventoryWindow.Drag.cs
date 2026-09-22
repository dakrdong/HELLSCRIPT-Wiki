using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed class InventoryCell:MonoBehaviour,IPointerDownHandler,IPointerClickHandler,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        public InventoryWindow window;public string itemId;public bool bag,inspectOnly;
        float pressedAt;ScrollRect scrolling;
        public void OnPointerDown(PointerEventData data){pressedAt=Time.unscaledTime;}
        public void OnPointerClick(PointerEventData data){if(data.button!=PointerEventData.InputButton.Left||itemId==null)return;if(inspectOnly)window.Inspect(itemId);else window.SelectItem(itemId);}
        public void OnBeginDrag(PointerEventData data)
        {
            bool touchScroll=data is ExtendedPointerEventData extended&&extended.pointerType==UIPointerType.Touch&&Time.unscaledTime-pressedAt<.18f;
            if(inspectOnly||window.Selecting||itemId==null||touchScroll){scrolling=GetComponentInParent<ScrollRect>();scrolling?.OnBeginDrag(data);return;}
            window.BeginDrag(this,data);
        }
        public void OnDrag(PointerEventData data){if(scrolling!=null)scrolling.OnDrag(data);else if(!inspectOnly)window.Drag(data);}
        public void OnEndDrag(PointerEventData data){if(scrolling!=null){scrolling.OnEndDrag(data);scrolling=null;}else if(!inspectOnly)window.EndDrag(data);}
    }
    public sealed class InventoryDropTarget:MonoBehaviour
    {public InventoryWindow window;public int slot,index;}
    public sealed partial class InventoryWindow
    {
        RectTransform ghost;string draggingId;bool draggingEquipped;Vector2 dragOffset;float suppressClickUntil;
        internal void BeginDrag(InventoryCell cell,PointerEventData data)
        {
            if(selecting||dialog!=null||cell.itemId==null||data.button!=PointerEventData.InputButton.Left)return;
            var item=FindItem(cell.itemId);if(item==null)return;
            draggingId=item.id;draggingEquipped=item.equipped;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)cell.transform,data.position,null,out var local);
            var rect=((RectTransform)cell.transform).rect;dragOffset=new Vector2(local.x-rect.xMin,rect.yMax-local.y);
            ghost=DrawCell(overlays,item,0,0,SlotSize,false);ghost.name="Inventory drag ghost";var group=ghost.gameObject.AddComponent<CanvasGroup>();group.blocksRaycasts=false;group.alpha=.88f;
            Drag(data);
        }
        internal void Drag(PointerEventData data)
        {
            if(ghost==null)return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(frame,data.position,null,out var point);
            Place(ghost,point.x-frame.rect.xMin-dragOffset.x,frame.rect.yMax-point.y-dragOffset.y,SlotSize,SlotSize);
        }
        internal void EndDrag(PointerEventData data)
        {
            if(ghost==null)return;var id=draggingId;bool equipped=draggingEquipped;
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
            var target=hits.Select(h=>h.gameObject.GetComponentInParent<InventoryDropTarget>()).FirstOrDefault(t=>t!=null&&t.window==this);
            CancelDrag();suppressClickUntil=Time.unscaledTime+.2f;
            if(target==null)return;
            var item=FindItem(id);if(item==null)return;
            if(equipped){if(target.slot==-2)Unequip(id);return;}
            if(target.slot==-1)Equip(id);else if(target.slot==item.slot)Equip(id,target.index);else Toast("이 장비를 해당 장착 위치에 넣을 수 없습니다.");
        }
        void CancelDrag(){if(ghost!=null){ghost.gameObject.SetActive(false);Destroy(ghost.gameObject);}ghost=null;draggingId=null;}
    }
}
