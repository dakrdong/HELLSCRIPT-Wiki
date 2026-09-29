using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class InventoryWindow
    {
        float WalletSummaryHeight=>landscape?24:40;
        // Read account-owned balances only. This view never grants or transfers resources.
        IEnumerable<(string id,string label,int amount,string icon)> WalletRows()
        {
            var a=store.Data;
            yield return ("gold",Loc.T("골드"),a.gold,"coin");
            yield return ("premium",Loc.T("심연 주화"),a.premium,"abyssal-coin");
            yield return ("materials",Loc.T("일반 재료"),a.materials,"vault");
            yield return ("stones",Loc.T("강화석"),a.enhancementStones,"enhancement-stone");
            for(int i=0;i<a.cores.Length;i++)yield return ("core-"+i,Loc.F("{0} 코어",GameCatalog.Slots[i]),a.cores[i],CurrencyIconView.CoreIcon(i));
        }
        void WalletFit(Text text)
        {
            text.horizontalOverflow=HorizontalWrapMode.Wrap;text.resizeTextForBestFit=true;
            text.resizeTextMinSize=8;text.resizeTextMaxSize=text.fontSize;
        }
        void DrawWalletSummary(RectTransform strip)
        {
            int columns=landscape?4:2,index=0;float cell=(strip.rect.width-16)/columns,rowHeight=WalletSummaryHeight/(4/columns);
            foreach(var row in WalletRows().Take(4))
            {
                float x=8+(index%columns)*cell,y=(index/columns)*rowHeight;index++;
                var label=Txt(strip,row.label,x+3,y,cell*.53f,rowHeight,9,muted);WalletFit(label);label.name="wallet-label-"+row.id;
                // Reserve for the longest balance; changing an amount never moves its icon or label.
                float labelWidth=Mathf.Min(label.preferredWidth,cell*.53f);
                label.rectTransform.sizeDelta=new Vector2(labelWidth,rowHeight);
                float iconX=x+labelWidth+7;
                WalletIcon(strip,row.icon,iconX,y+(rowHeight-16)/2,16);
                var value=Txt(strip,row.amount.ToString("N0"),iconX+20,y,cell-labelWidth-32,rowHeight,11,gold);WalletFit(value);value.name="wallet-value-"+row.id;
            }
        }
        void WalletIcon(Transform parent,string icon,float x,float y,float size)
        {
            if(CurrencyIconView.Create(parent,icon,x,y,size)!=null)return;
            Glyph(parent,icon,x,y,size,icon=="coin"?gold:muted);
        }
        public void ShowWallet()
        {
            float w=Mathf.Min(width-16,424),h=Mathf.Min(height-24,538);Modal("wallet","전체 재화",w,h);
            Txt(dialog,"모든 캐릭터가 함께 사용하는 보유 수량입니다.",14,44,w-28,38,10,muted);
            Scroll(dialog,"Account balances",10,86,w-20,h-137,out var content);float y=3;
            foreach(var row in WalletRows())
            {
                var cell=Panel(content,"wallet-row-"+row.id,UiTheme.HoverHex,UiTheme.RaisedHex,UiTheme.EdgeHex);Place(cell,4,y,w-38,42);
                WalletIcon(cell,row.icon,4,3,36);
                var name=Txt(cell,row.label,44,0,(w-88)*.56f,42,11,pale);WalletFit(name);
                var value=Txt(cell,row.amount.ToString("N0"),44+(w-88)*.56f,0,(w-88)*.44f,42,12,gold,TextAnchor.MiddleRight);
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
