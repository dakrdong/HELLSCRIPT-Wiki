using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class InventoryWindow
    {
        const float WalletSummaryHeight=34;
        // Read account-owned balances only. This view never grants or transfers resources.
        IEnumerable<(string id,string label,int amount,string icon)> WalletRows()
        {
            var a=store.Data;
            yield return ("gold",Loc.T("골드"),a.gold,"coin");
            yield return ("premium",Loc.T("심연 주화"),a.premium,"abyssal-coin");
            yield return ("materials",Loc.T("일반 재료"),a.materials,"vault");
            yield return ("stones",Loc.T("강화석"),a.enhancementStones,"gem-solid");
            for(int i=0;i<a.cores.Length;i++)yield return ("core-"+i,Loc.F("{0} 코어",GameCatalog.Slots[i]),a.cores[i],"orb");
        }
        void WalletFit(Text text)
        {
            text.horizontalOverflow=HorizontalWrapMode.Wrap;text.resizeTextForBestFit=true;
            text.resizeTextMinSize=8;text.resizeTextMaxSize=text.fontSize;
        }
        void DrawWalletSummary(RectTransform footer)
        {
            float cell=(footer.rect.width-16)/4;int index=0;
            foreach(var row in WalletRows().Take(4))
            {
                float x=8+index++*cell;
                var label=Txt(footer,row.label,x+4,0,cell-8,14,8,muted);WalletFit(label);label.name="wallet-label-"+row.id;
                WalletIcon(footer,row.icon,x+3,17,13);
                var value=Txt(footer,row.amount.ToString("N0"),x+21,14,cell-26,19,11,gold);WalletFit(value);value.name="wallet-value-"+row.id;
            }
        }
        void WalletIcon(Transform parent,string icon,float x,float y,float size)
        {
            if(icon!="abyssal-coin"){Glyph(parent,icon,x,y,size,icon=="coin"?gold:muted);return;}
            var rect=Rect("Abyssal Coin",parent);Place(rect,x-2,y-2,size+4,size+4);
            var art=rect.gameObject.AddComponent<Image>();art.sprite=Resources.Load<Sprite>("Art/GlobalHUD/currency-abyssal-coin");
            art.preserveAspect=true;art.raycastTarget=false;
        }
        public void ShowWallet()
        {
            float w=Mathf.Min(width-16,424),h=Mathf.Min(height-24,538);Modal("wallet","전체 재화",w,h);
            Txt(dialog,"모든 캐릭터가 함께 사용하는 보유 수량입니다.",14,44,w-28,38,10,muted);
            Scroll(dialog,"Account balances",10,86,w-20,h-137,out var content);float y=3;
            foreach(var row in WalletRows())
            {
                var cell=Panel(content,"wallet-row-"+row.id,"28291e","1c2016","534a32");Place(cell,4,y,w-38,42);
                WalletIcon(cell,row.icon,8,13,16);
                var name=Txt(cell,row.label,32,0,(w-76)*.56f,42,11,pale);WalletFit(name);
                var value=Txt(cell,row.amount.ToString("N0"),32+(w-76)*.56f,0,(w-76)*.44f,42,12,gold,TextAnchor.MiddleRight);
                WalletFit(value);value.name="wallet-detail-value-"+row.id;y+=46;
            }
            content.sizeDelta=new Vector2(0,y);
            Btn(dialog,"닫기",w-101,h-41,87,30,Dismiss,false,11).name="inventory-wallet-close";
        }
        void RefreshCommittedView()
        {
            Repaint();if(dialogKind!="wallet"||dialog==null)return;
            // A successful save updates counts in place, retaining the open sheet and its scroll offset.
            foreach(var row in WalletRows())
            {
                var value=dialog.GetComponentsInChildren<Text>().FirstOrDefault(t=>t.name=="wallet-detail-value-"+row.id);
                if(value!=null)value.text=row.amount.ToString("N0");
            }
        }
    }
}
