using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class InventoryWindow
    {
        int potionSlotIndex;
        void DrawPotionSlots(Transform parent,float w,Rect weapon)
        {
            var slots=PotionPolicy.Resolve(Hero).Slots;float size=landscape?28:32,gap=3,cog=22;
            float left=w-12-(size*3+gap*2+4+cog),y=weapon.center.y-size/2;
            for(int i=0;i<PotionLoadout.SlotCount;i++)
            {
                int index=i;var slot=slots[i];var root=EquipmentSlotView.Create(parent,null,left+i*(size+gap),y,size,font);
                root.name="inventory-potion-"+i;var target=root.gameObject.AddComponent<InventoryDropTarget>();target.window=this;target.slot=-3;
                var button=root.gameObject.AddComponent<Button>();button.targetGraphic=root.GetComponent<StorageSurface>();UiTheme.Button(button);button.onClick.AddListener(()=>ShowPotionPicker(index));
                if(!string.IsNullOrEmpty(slot.id))
                {
                    var def=PotionCatalog.Get(slot.id);var art=PotionArt.Draw(root,def,3,3,size-6);art.color=Hero.potions.Count(slot.id)>0?Color.white:new Color(1,1,1,.35f);
                    var count=Txt(root,"×"+Hero.potions.Count(slot.id),1,size-11,size-3,10,8,pale,TextAnchor.MiddleRight);
                    count.resizeTextForBestFit=true;count.resizeTextMinSize=6;count.resizeTextMaxSize=count.fontSize;
                }
                else Txt(root,"+",2,2,size-4,size-4,14,muted,TextAnchor.MiddleCenter);
                Txt(root,(i+1).ToString(),2,0,9,11,7,muted);
            }
            var settings=Btn(parent,"",w-12-cog,y,cog,size,ShowPotionSettings);settings.name="inventory-potion-settings";
            Glyph(settings.transform,"gear",2,(size-18)/2,18,gold);
        }
        public void ShowPotionSettings()
        {
            Dismiss();dialogKind="potion";
            var shade=Btn(overlays,"",0,0,width,height,Dismiss);shade.name="potion-settings-backdrop";shade.GetComponent<StorageSurface>().Paint("00000000","00000000");
            float w=Mathf.Min(352,width-16),h=310;
            var anchor=(RectTransform)Find("inventory-potion-settings").transform;var corners=new Vector3[4];anchor.GetWorldCorners(corners);
            var point=frame.InverseTransformPoint((corners[0]+corners[3])*.5f);float ax=point.x-frame.rect.xMin,ay=frame.rect.yMax-point.y;
            bool below=ay+h+12<=height;float top=below?ay+8:Mathf.Clamp(ay-anchor.rect.height-h-8,8,height-h-8);
            dialog=Panel(overlays,"Potion settings bubble","302b1f","1b1e15","a68b59");Place(dialog,Mathf.Clamp(ax-w/2,8,width-w-8),top,w,h);
            var tail=Panel(dialog,"Speech bubble tail","302b1f","302b1f","a68b59");float tx=Mathf.Clamp(ax-dialog.anchoredPosition.x,12,w-12);
            Place(tail,tx-4,below?-4:h-4,8,8);tail.localRotation=Quaternion.Euler(0,0,45);tail.SetAsFirstSibling();tail.GetComponent<Image>().raycastTarget=false;
            Txt(dialog,"물약 사용 순서",12,2,w-53,25,12,gold);
            Btn(dialog,"×",w-32,3,26,25,Dismiss,false,17).name="potion-settings-close";
            Txt(dialog,"지정 물약을 모두 소진 했을 시",12,30,w-24,39,12,gold);
            for(int n=0;n<PotionLoadout.FallbackLabels.Length;n++)
            {
                var choice=(PotionFallback)n;bool chosen=Hero.potions.SharedFallback==choice;
                var row=Btn(dialog,"",10,73+n*43,w-20,40,()=>ChoosePotionFallback(choice),chosen,11);row.name="potion-fallback-"+n;
                Txt(row.transform,PotionLoadout.FallbackLabels[n],33,2,w-68,36,11,chosen?gold:pale);
                Check(row.transform,9,12,15,chosen);
            }
            Txt(dialog,"세 슬롯에 공통 적용됩니다. 같은 효과의 물약만 사용하며, 대체 물약이 없으면 멈춥니다.",12,249,w-24,50,9,muted);
        }
        void ChoosePotionFallback(PotionFallback choice)
        {
            if(!store.SetPotionFallback(Guid.NewGuid().ToString("N"),Hero.id,choice)){ShowError(store.Error);return;}
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
            potionSlotIndex=index;float w=landscape?410:width-16,h=height-24;Modal("potion-picker",Loc.F("물약 슬롯 {0}",index+1),w,h);
            var slots=PotionPolicy.Resolve(Hero).Slots;
            Scroll(dialog,"Owned potions",10,47,w-20,h-96,out var content);float y=4;
            foreach(var def in PotionCatalog.All)
            {
                string id=def.id;int count=Hero.potions.Count(id);bool occupied=slots.Where((s,n)=>n!=index).Any(s=>s.id==id),chosen=slots[index].id==id;
                float rowHeight=def.Crafted?128:80;
                var row=Btn(content,"",5,y,w-42,rowHeight,()=>AssignPotion(index,id),chosen);row.name="potion-pick-"+id;row.interactable=count>0&&!occupied;
                PotionArt.Draw(row.transform,def,8,12,48);
                Txt(row.transform,GemElixirs.Name(def)+" · ×"+count,66,4,w-123,def.Crafted?42:27,12,chosen?gold:pale);
                Txt(row.transform,occupied?"다른 슬롯에 지정됨":count==0?"보유하지 않음":GemElixirs.Description(def),66,def.Crafted?48:32,w-123,rowHeight-39,10,muted);
                if(chosen)Glyph(row.transform,"check",w-66,5,14,gold);y+=rowHeight+6;
            }
            content.sizeDelta=new Vector2(0,y);
            Btn(dialog,"해제",12,h-41,87,30,()=>AssignPotion(index,""),false,11).name="potion-clear";
            Btn(dialog,"닫기",w-101,h-41,87,30,Dismiss,false,11).name="potion-picker-back";
        }
        void AssignPotion(int index,string id)
        {
            if(!store.SetPotionSlot(Guid.NewGuid().ToString("N"),Hero.id,index,id)){ShowError(store.Error);return;}
            Repaint();ShowPotionPicker(index);
        }
    }
}
