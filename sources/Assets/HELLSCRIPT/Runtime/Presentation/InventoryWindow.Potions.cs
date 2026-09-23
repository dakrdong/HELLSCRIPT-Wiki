using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class InventoryWindow
    {
        int potionSlotIndex;
        void DrawPotionSlots(Transform parent,float w,float y)
        {
            var slots=PotionPolicy.Resolve(Hero).Slots;float step=SlotSize+UiTheme.Gap,left=(w-(SlotSize*3+UiTheme.Gap*2))/2;
            for(int i=0;i<PotionLoadout.SlotCount;i++)
            {
                int index=i;var slot=slots[i];var root=EquipmentSlotView.Create(parent,null,left+i*step,y,SlotSize,font);
                root.name="inventory-potion-"+i;var target=root.gameObject.AddComponent<InventoryDropTarget>();target.window=this;target.slot=-3;
                var button=root.gameObject.AddComponent<Button>();button.targetGraphic=root.GetComponent<StorageSurface>();UiTheme.Button(button);button.onClick.AddListener(()=>ShowPotionSettings(index));
                Txt(root,(i+1).ToString(),3,1,11,13,8,muted);
                if(!string.IsNullOrEmpty(slot.id))
                {
                    var def=PotionCatalog.Get(slot.id);var art=PotionArt.Draw(root,def,5,5,SlotSize-10);art.color=Hero.potions.Count(slot.id)>0?Color.white:new Color(1,1,1,.35f);
                    Txt(root,"×"+Hero.potions.Count(slot.id),2,SlotSize-14,SlotSize-5,12,9,pale,TextAnchor.MiddleRight);
                }
                else Txt(root,"물약",4,16,SlotSize-8,22,10,muted,TextAnchor.MiddleCenter);
                var cog=Panel(root,"Potion settings icon","171b13e0","171b13e0");Place(cog,SlotSize-19,1,18,18);cog.GetComponent<Image>().raycastTarget=false;
                Glyph(cog,"gear",1,1,16,gold);
            }
        }
        public void ShowPotionSettings(int index)
        {
            if(index<0||index>=PotionLoadout.SlotCount)return;
            Dismiss();potionSlotIndex=index;dialogKind="potion";
            var shade=Btn(overlays,"",0,0,width,height,Dismiss);shade.name="potion-settings-backdrop";shade.GetComponent<StorageSurface>().Paint("00000000","00000000");
            float w=Mathf.Min(352,width-16),h=366;
            var anchor=(RectTransform)Find("inventory-potion-"+index).transform;var corners=new Vector3[4];anchor.GetWorldCorners(corners);
            var point=frame.InverseTransformPoint((corners[0]+corners[3])*.5f);float ax=point.x-frame.rect.xMin,ay=frame.rect.yMax-point.y;
            bool below=ay+h+12<=height;float top=below?ay+8:Mathf.Clamp(ay-anchor.rect.height-h-8,8,height-h-8);
            dialog=Panel(overlays,"Potion settings bubble","302b1f","1b1e15","a68b59");Place(dialog,Mathf.Clamp(ax-w/2,8,width-w-8),top,w,h);
            var tail=Panel(dialog,"Speech bubble tail","302b1f","302b1f","a68b59");float tx=Mathf.Clamp(ax-dialog.anchoredPosition.x,12,w-12);
            Place(tail,tx-4,below?-4:h-4,8,8);tail.localRotation=Quaternion.Euler(0,0,45);tail.SetAsFirstSibling();tail.GetComponent<Image>().raycastTarget=false;
            Txt(dialog,Loc.F("물약 슬롯 {0}",index+1),12,2,w-53,25,12,gold);
            Btn(dialog,"×",w-32,3,26,25,Dismiss,false,17).name="potion-settings-close";
            var slot=PotionPolicy.Resolve(Hero).Slots[index];
            if(!string.IsNullOrEmpty(slot.id))
            {
                var def=PotionCatalog.Get(slot.id);PotionArt.Draw(dialog,def,13,30,35);
                Txt(dialog,Loc.T(def.name)+" · ×"+Hero.potions.Count(def.id),57,29,w-69,37,12,pale);
            }
            else Txt(dialog,"빈 물약 슬롯",12,29,w-24,37,12,muted);
            Txt(dialog,"지정 물약을 모두 소진 했을 시",12,68,w-24,39,12,gold);
            for(int n=0;n<PotionLoadout.FallbackLabels.Length;n++)
            {
                var choice=(PotionFallback)n;bool chosen=slot.fallback==choice;
                var row=Btn(dialog,"",10,110+n*43,w-20,40,()=>ChoosePotionFallback(index,choice),chosen,11);row.name="potion-fallback-"+n;
                Txt(row.transform,PotionLoadout.FallbackLabels[n],33,2,w-68,36,11,chosen?gold:pale);
                Check(row.transform,9,12,15,chosen);
            }
            Txt(dialog,"같은 효과의 물약만 사용합니다. 대체 물약이 없으면 사용을 멈춥니다.",12,285,w-24,36,9,muted);
            Btn(dialog,"물약 변경",12,326,w-100,30,()=>ShowPotionPicker(index),true,11).name="potion-change";
            Btn(dialog,"해제",w-80,326,68,30,()=>AssignPotion(index,""),false,11).name="potion-clear";
        }
        void ChoosePotionFallback(int index,PotionFallback choice)
        {
            if(!store.SetPotionFallback(Guid.NewGuid().ToString("N"),Hero.id,index,choice)){ShowError(store.Error);return;}
            // Keep the bubble, hit areas and inventory layout stable while changing only selection styling.
            for(int n=0;n<PotionLoadout.FallbackLabels.Length;n++)
            {
                var row=Find("potion-fallback-"+n);bool chosen=n==(int)choice;UiTheme.Button(row,chosen);
                foreach(var text in row.GetComponentsInChildren<Text>())text.color=chosen?gold:pale;
                var check=row.transform.Find("Selection checkbox");check.GetComponent<StorageSurface>().Paint(chosen?"c4a774":"13180f",chosen?"c4a774":"13180f","c0a475");
                if(check.childCount==0&&chosen)Glyph(check,"check",1,1,13,ink);else if(check.childCount>0)check.GetChild(0).gameObject.SetActive(chosen);
            }
        }
        void ShowPotionPicker(int index)
        {
            potionSlotIndex=index;float w=landscape?410:width-16,h=height-24;Modal("potion-picker","물약 선택",w,h);
            var slots=PotionPolicy.Resolve(Hero).Slots;
            Scroll(dialog,"Owned potions",10,47,w-20,h-96,out var content);float y=4;
            foreach(var def in PotionCatalog.All)
            {
                string id=def.id;int count=Hero.potions.Count(id);bool occupied=slots.Where((s,n)=>n!=index).Any(s=>s.id==id),chosen=slots[index].id==id;
                var row=Btn(content,"",5,y,w-42,80,()=>AssignPotion(index,id),chosen);row.name="potion-pick-"+id;row.interactable=count>0&&!occupied;
                PotionArt.Draw(row.transform,def,8,12,48);
                Txt(row.transform,Loc.T(def.name)+" · ×"+count,66,4,w-123,27,12,chosen?gold:pale);
                Txt(row.transform,occupied?"다른 슬롯에 지정됨":count==0?"보유하지 않음":def.description,66,32,w-123,41,10,muted);
                if(chosen)Glyph(row.transform,"check",w-66,5,14,gold);y+=86;
            }
            content.sizeDelta=new Vector2(0,y);
            Btn(dialog,"뒤로",w-101,h-41,87,30,()=>ShowPotionSettings(index),false,11).name="potion-picker-back";
        }
        void AssignPotion(int index,string id)
        {
            if(!store.SetPotionSlot(Guid.NewGuid().ToString("N"),Hero.id,index,id)){ShowError(store.Error);return;}
            Repaint();ShowPotionSettings(index);
        }
    }
}
