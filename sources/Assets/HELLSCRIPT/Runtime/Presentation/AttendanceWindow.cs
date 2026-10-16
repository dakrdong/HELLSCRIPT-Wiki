using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public static class AttendanceWindow
    {
        public static ContentWindowView Open(Transform parent,GameStore store,AttendancePopupSettings settings,
            AttendanceKind kind,Func<float> reading,Action<AttendanceKind> visited,Action closed,Action dailyQuests=null)
        {
            var session=new AttendanceSession(store,settings,kind,reading,visited,dailyQuests);
            var view=ContentWindowView.Open(parent,"출석 이벤트",EquipmentViewSource.Owned,reading,session.Render,closed,new Vector2(900,820));
            var swipe=view.gameObject.AddComponent<AttendanceSwipe>();session.Swipe=swipe;
            swipe.Initialize(view,session.MovePage);
            StoreViewBinding.Attach(view,store,view.Repaint,()=>!swipe.Dragging,session.DisplayKey);
            visited?.Invoke(kind);return view;
        }
    }
    // Landscape: featured reward on the altar beside the day grid. Portrait: the altar above the grid.
    // Tiles show reward definitions; grants stay in GameStore attendance transactions.
    sealed class AttendanceSession
    {
        const float Gap=6;
        // popup-altar.png: the altar's upper face, as a fraction of the art height from the top.
        const float AltarTop=.46f;
        readonly GameStore store;readonly AttendancePopupSettings settings;readonly Func<float> reading;readonly Action<AttendanceKind> visited;
        AttendanceKind kind;ContentWindowView view;string notice="",language=Loc.Language;Color noticeColor;int selected,observedEarned=-1;long selectedPeriod;
        public AttendanceSwipe Swipe;
        readonly Action dailyQuests;
        float Scale=>reading?.Invoke()??1;
        AttendanceTrack Track=>Attendance.Track(store.Data.attendance,kind);
        public string DisplayKey()=>string.Join("|",StoreViewBinding.Key(store.Data.attendance),
            DailyQuests.Pending(store.Data.dailyQuests),store.Data.Hero.id,Economy.FreeSlots(store.Data.Hero),LastDay);
        long LastDay=>Math.Max(Attendance.Day(store.AttendanceClock()),store.Data.attendance.lastDay);
        public AttendanceSession(GameStore store,AttendancePopupSettings settings,AttendanceKind kind,Func<float> reading,Action<AttendanceKind> visited,Action dailyQuests)
        {this.store=store;this.settings=settings;this.kind=kind;this.reading=reading;this.visited=visited;this.dailyQuests=dailyQuests;}
        public void MovePage(int direction)
        {
            var target=(AttendanceKind)Mathf.Clamp((int)kind+direction,0,1);if(target==kind)return;
            kind=target;selected=0;notice="";visited?.Invoke(kind);view.Repaint();view.Scroll.verticalNormalizedPosition=1;
            Swipe?.Animate(direction);
        }
        public void Render(ContentWindowView window)
        {
            view=window;if(language!=Loc.Language){language=Loc.Language;notice="";}
            float s=Scale,w=view.Width-24,h=view.Frame.rect.height;bool landscape=view.Frame.rect.width>h;
            float navH=30*s,actionH=landscape?40*s:74*s,bodyTop=UiTheme.Header+navH+6,bodyH=h-bodyTop-actionH-16;
            UiLayout.Place(view.Navigation,12,UiTheme.Header,w,navH);
            UiLayout.Place(view.Actions,12,h-actionH-8,w,actionH);
            UiLayout.Place(view.Scroll.viewport,12,bodyTop,w,bodyH);
            view.Frame.gameObject.name="Attendance "+kind;
            int total=Attendance.Length(kind);var track=Track;
            if(selectedPeriod!=track.period||observedEarned!=track.earned){selected=0;selectedPeriod=track.period;observedEarned=track.earned;}
            if(selected<1||selected>total)selected=Enumerable.Range(1,total).FirstOrDefault(d=>d<=track.earned&&!track.Claimed(d));
            if(selected==0)selected=Math.Min(total,track.earned+1);
            Tabs(w,navH,landscape);
            bool chests=store.Data.attendance.chests.Count>0;float stashH=44*s;
            if(landscape)
            {
                float featureW=Mathf.Clamp(w*.36f,220,320),gridX=featureW+10,gridW=w-gridX;
                var row=Row("Attendance columns",w,1);
                float gridH=Grid(row,gridX,0,gridW,bodyH-(chests?stashH+Gap:0),true);
                if(chests){Stash(row,gridX,gridH+Gap,gridW,stashH);gridH+=stashH+Gap;}
                SetHeight(row,Feature(row,0,0,featureW,Mathf.Max(bodyH,gridH)));
            }
            else
            {
                var feature=Row("Attendance feature row",w,1);SetHeight(feature,Feature(feature,0,0,w,0));
                var grid=Row("Attendance grid",w,1);SetHeight(grid,Grid(grid,0,0,w,0,false));
                if(chests)Stash(Row("Attendance chest stash",w,stashH),0,0,w,stashH);
            }
            var rules=Row("Attendance rules",w,1);
            var text=Fit(rules,Loc.T("출석은 계정의 모든 캐릭터가 함께 쌓습니다. 매일 00:00 KST에 갱신되며 접속한 날만 누적됩니다. 받지 않은 보상은 기간 초기화 때 사라집니다.\n전설 상자는 수령한 직업과 단계로 보관됩니다. 열린 장비는 현재 캐릭터의 가방으로 이동합니다."),w-16,UiTheme.Caption,UiTheme.Muted,TextAnchor.UpperLeft);
            Move(text,8,4);SetHeight(rules,text.rectTransform.rect.height+8);
            DrawActions(w,landscape);
            Swipe?.BindViewport();
        }
        void Tabs(float w,float h,bool landscape)
        {
            float s=Scale;int count=dailyQuests==null?2:3;float tab=landscape?Mathf.Min(170*s,(w-(count-1)*6)/count):(w-(count-1)*6)/count;
            foreach(var page in new[]{AttendanceKind.Weekly,AttendanceKind.Monthly})
            {
                bool weekly=page==AttendanceKind.Weekly;
                var b=B(view.Navigation,weekly?"7일 출석":"28일 출석",(int)page*(tab+6),0,tab,h,()=>MovePage(page-kind),weekly?"attendance-weekly":"attendance-monthly");
                UiTheme.Choice(b,kind==page);
                b.GetComponentInChildren<Text>().fontSize=Mathf.RoundToInt(UiTheme.Caption*s);
                if(Attendance.Pending(Attendance.Track(store.Data.attendance,page))>0)Dot(b.transform,tab-13*s,5*s,8*s);
            }
            if(dailyQuests!=null)
            {
                var daily=B(view.Navigation,"일일 퀘스트",2*(tab+6),0,tab,h,dailyQuests,"attendance-daily-quests");
                daily.GetComponentInChildren<Text>().fontSize=Mathf.RoundToInt(UiTheme.Caption*s);
                if(DailyQuests.Pending(store.Data.dailyQuests)>0)Dot(daily.transform,tab-13*s,5*s,8*s);
            }
        }
        // Monthly: a 7×4 calendar. Weekly: days 1-6 in 3×2 with the chest day as a tall (landscape) or wide (portrait) card.
        float Grid(Transform parent,float x,float y,float w,float available,bool landscape)
        {
            if(kind==AttendanceKind.Monthly)
            {
                float gap=5,cell=(w-6*gap)/7,cellH=Mathf.Max(TileHeight(cell,true),(available-3*gap)/4);
                for(int d=1;d<=28;d++)Tile(parent,d,x+(d-1)%7*(cell+gap),y+(d-1)/7*(cellH+gap),cell,cellH,true);
                return 4*cellH+3*gap;
            }
            if(landscape)
            {
                float small=(w-3*Gap)/4.4f,big=w-3*Gap-3*small,cellH=Mathf.Max(TileHeight(small,false),(available-Gap)/2);
                for(int d=1;d<=6;d++)Tile(parent,d,x+(d-1)%3*(small+Gap),y+(d-1)/3*(cellH+Gap),small,cellH,false);
                Tile(parent,7,x+3*(small+Gap),y,big,2*cellH+Gap,false);
                return 2*cellH+Gap;
            }
            float c=(w-2*Gap)/3,ch=Mathf.Max(TileHeight(c,false),c*.95f);
            for(int d=1;d<=6;d++)Tile(parent,d,x+(d-1)%3*(c+Gap),y+(d-1)/3*(ch+Gap),c,ch,false);
            Tile(parent,7,x,y+2*(ch+Gap),w,ch,false);
            return 3*ch+2*Gap;
        }
        float TileHeight(float w,bool compact)=>compact?8+28*Scale+22+4:14+36*Scale+Mathf.Max(34,w*.42f)+8;
        void Tile(Transform parent,int day,float x,float y,float w,float h,bool compact)
        {
            float s=Scale,pad=compact?4:7,labelH=(compact?13:16)*s,amountH=(compact?15:20)*s;var track=Track;var reward=Attendance.Reward(kind,day);
            bool claimed=track.Claimed(day),ready=day<=track.earned&&!claimed,chest=reward.kind==AttendanceRewardKind.LegendaryChest,wide=chest&&!compact&&w>h*1.6f;
            float rewardH=h-(ready?(compact?22:26)*s:0);
            var b=B(parent,"",x,y,w,h,()=>{selected=day;notice="";view.Repaint();},"attendance-day-"+day);UiTheme.Choice(b,selected==day,false);var t=b.transform;
            Surface(t,"Tile card",4,4,w-8,h-8,claimed?UiTheme.RaisedHex:ready?"203e29":chest?UiTheme.BorderHex:UiTheme.HoverHex,claimed?UiTheme.BackgroundHex:ready?"102017":chest?UiTheme.BackgroundHex:UiTheme.BackgroundHex);
            if(chest)Surface(t,"Milestone accent",4,4,w-8,2,UiTheme.GoldHex,UiTheme.GoldHex);
            T(t,compact?day.ToString():Loc.F("{0}일차",day),pad+1,pad-1,w-pad*2-10*s,labelH,UiTheme.Caption,chest?UiTheme.Gold:claimed?UiTheme.Muted:UiTheme.Text);
            string amount="×"+JewelerSession.Compact(reward.amount);float icon,cx,cy;
            if(compact&&ready)
            {
                float area=rewardH-pad*2-labelH-2;icon=Mathf.Max(8,Mathf.Min(12*s,area));
                cx=pad+icon/2;cy=pad+labelH+area/2;
                T(t,amount,pad+icon+2,pad+labelH,w-pad*2-icon-2,area,UiTheme.Caption,UiTheme.Gold,TextAnchor.MiddleCenter);
            }
            else if(wide)
            {
                icon=Mathf.Min(rewardH-pad*2-labelH,w*.3f);cx=w*.3f;cy=pad+labelH+(rewardH-pad*2-labelH)/2;
                float tx=cx+icon/2+14,tw=w-tx-pad-4;var name=Fit(t,reward.Name,tw,UiTheme.Body,claimed?UiTheme.Muted:UiTheme.Text,TextAnchor.MiddleLeft);
                float nh=name.rectTransform.rect.height;Move(name,tx,cy-(nh+amountH)/2);
                T(t,amount,tx,cy-(nh+amountH)/2+nh,tw,amountH,UiTheme.Heading,claimed?UiTheme.Muted:UiTheme.Gold);
            }
            else
            {
                float nameH=0;
                if(chest&&!compact)
                {var name=Fit(t,reward.Name,w-12,UiTheme.Caption,claimed?UiTheme.Muted:UiTheme.Text);nameH=name.rectTransform.rect.height;Move(name,6,rewardH-pad-amountH-nameH);}
                float area=rewardH-pad*2-labelH-amountH-nameH-4;icon=Mathf.Max(8,Mathf.Min(w*(chest&&!compact?.7f:compact?.62f:.58f),area));
                cx=w/2;cy=pad+labelH+2+area/2;
                T(t,amount,2,rewardH-pad-amountH,w-4,amountH,compact?UiTheme.Caption:UiTheme.Heading,claimed?UiTheme.Muted:UiTheme.Gold,TextAnchor.MiddleCenter);
            }
            if(ready||chest)Glow(t,cx,cy,Mathf.Min(icon*1.05f,Mathf.Min(w,h)/2-3),ready?UiTheme.Claimable:UiTheme.Gold,ready);
            Icon(t,reward,cx-icon/2,cy-icon/2,icon);
            if(claimed){Surface(t,"Claimed shade",4,4,w-8,h-8,UiTheme.InsetHex+"80",UiTheme.InsetHex+"b0");Seal(t,cx,cy,Mathf.Min(icon*1.05f,Mathf.Min(w,h)*.62f));}
            if(ready)
            {
                // Keep the eligibility border fully visible even when animation is reduced.
                Surface(t,"Claimable frame",0,0,w,h,"00000000","00000000",UiTheme.ClaimableHex);
                Surface(t,"Claimable inner frame",1,1,w-2,h-2,"00000000","00000000",UiTheme.ClaimableHex+"99");
                Dot(t,w-pad-9*s,pad,8*s);
                long period=track.period;
                var collect=B(t,"수집",pad,h-(compact?20:24)*s-pad/2,w-pad*2,(compact?20:24)*s,
                    ()=>Claim(()=>store.ClaimAttendance(kind,period,day)),"attendance-collect-"+day);
                var label=collect.GetComponentInChildren<Text>();label.fontSize=Mathf.RoundToInt(UiTheme.Caption*s);label.color=UiTheme.Claimable;
            }
        }
        // The reward rests on the altar: the art is cropped so the altar's face meets the icon above the text block.
        float Feature(Transform parent,float x,float y,float w,float h)
        {
            float s=Scale,pad=10*s,iw=w-pad*2,label=20*s,barH=5*s;var track=Track;int total=Attendance.Length(kind);var reward=Attendance.Reward(kind,selected);
            bool claimed=track.Claimed(selected),ready=selected<=track.earned&&!claimed;
            var panel=UiLayout.Rect("Attendance feature",parent);panel.gameObject.AddComponent<RectMask2D>();
            var altar=UiLayout.Rect("Altar art",panel);var art=altar.gameObject.AddComponent<RawImage>();art.texture=AttendanceArt.Altar;art.raycastTarget=false;
            var topShade=Surface(panel,"Label shade",0,0,1,1,UiTheme.BackgroundHex+"c0",UiTheme.BackgroundHex+"00");var shade=Surface(panel,"Text shade",0,0,1,1,UiTheme.BackgroundHex+"00",UiTheme.BackgroundHex+"f4");
            var glow=UiLayout.Rect("Glow layer",panel);UiLayout.Stretch(glow);var icons=UiLayout.Rect("Icon layer",panel);UiLayout.Stretch(icons);
            var day=T(panel,Loc.F("{0}일차",selected),pad,pad,iw,label,UiTheme.Heading,UiTheme.Gold);
            var state=State(selected,ready,claimed);
            var name=Fit(panel,reward.Name,iw,UiTheme.Heading,UiTheme.Text);
            var amount=Fit(panel,"× "+reward.amount.ToString("N0"),iw,UiTheme.Title,UiTheme.Gold);
            var status=Fit(panel,state.text,iw,UiTheme.Body,state.color);
            var period=Fit(panel,Loc.F("출석 {0} / {1}일",track.earned,total)+"\n"+Loc.F("초기화 {0:yyyy-MM-dd} 00:00 KST",Attendance.Date(Attendance.ResetDay(kind,LastDay))),iw,UiTheme.Caption,UiTheme.Muted);
            foreach(var shadow in new[]{day,name,amount,status})shadow.gameObject.AddComponent<Shadow>().effectColor=new Color(0,0,0,.85f);
            float block=H(name)+H(amount)+H(status)+barH+8*s+H(period);
            float want=Mathf.Clamp(w*.34f,64,112);h=Mathf.Max(h,pad+label+10*s+(h>0?64:want)+12*s+block+pad);UiLayout.Place(panel,x,y,w,h);UiLayout.Stretch(altar);
            float r=w/h,uvW=Mathf.Min(1,r),uvH=Mathf.Min(1,1/r),blockTop=h-pad-block;
            float top=Mathf.Clamp(AltarTop-(blockTop-8*s)/h*uvH,0,1-uvH);art.uvRect=new Rect((1-uvW)/2,1-top-uvH,uvW,uvH);
            float iconBottom=Mathf.Min((AltarTop-top)/uvH*h+2*s,blockTop-4*s),icon=Mathf.Min(want,iconBottom-pad-label-6*s);
            UiLayout.Place((RectTransform)topShade.transform,0,0,w,pad+label+16*s);UiLayout.Place((RectTransform)shade.transform,0,blockTop-28*s,w,h-blockTop+28*s);
            float cy=iconBottom-icon/2;
            Glow(glow,w/2,cy,icon*.95f,ready?UiTheme.Claimable:UiTheme.Gold,ready);
            Icon(icons,reward,(w-icon)/2,iconBottom-icon,icon);
            if(claimed)Seal(icons,w/2+icon*.3f,cy+icon*.2f,icon*.6f);
            float ty=blockTop;
            foreach(var line in new[]{name,amount,status}){Move(line,pad,ty);ty+=H(line);}
            ty+=4*s;float g=total>7?1:2,seg=(iw-(total-1)*g)/total;
            for(int d=1;d<=total;d++)Surface(panel,"Progress segment",pad+(d-1)*(seg+g),ty,seg,barH,track.Claimed(d)?UiTheme.GoldHex:d<=track.earned?UiTheme.DropHex:UiTheme.BorderHex,track.Claimed(d)?UiTheme.GoldDeepHex:d<=track.earned?UiTheme.GoldHex:UiTheme.HoverHex);
            Move(period,pad,ty+barH+4*s);
            Surface(panel,"Feature frame",0,0,w,h,"00000000","00000000",ready?UiTheme.ClaimableHex:reward.kind==AttendanceRewardKind.LegendaryChest?UiTheme.GoldHex:UiTheme.BorderHex);
            return h;
        }
        (string text,Color color) State(int day,bool ready,bool claimed)
        {
            if(notice!="")return(notice,noticeColor);
            if(claimed)return(Loc.T("수령 완료"),UiTheme.Success);
            if(ready)return(Loc.T("수령 가능"),UiTheme.Claimable);
            // Only calendar days after today can still add check-ins before the period resets.
            int needed=day-Track.earned;long left=Attendance.ResetDay(kind,LastDay)-LastDay-1;
            return needed>left?(Loc.T("이번 기간에는 받을 수 없습니다"),UiTheme.Muted):(Loc.F("남은 출석 {0}일",needed),UiTheme.Text);
        }
        void Stash(Transform parent,float x,float y,float w,float h)
        {
            float s=Scale,bw=Mathf.Min(140*s,w*.42f),art=Mathf.Min(30*s,h-8);var chest=store.Data.attendance.chests[0];
            var bar=Surface(parent,"Attendance chest stash",x,y,w,h,UiTheme.SelectedHex,UiTheme.PanelHex,UiTheme.BorderHex).transform;
            var image=UiLayout.Rect("Stash chest",bar);UiLayout.Place(image,8,(h-art)/2,art,art);var picture=image.gameObject.AddComponent<Image>();
            picture.sprite=AttendanceArt.For(AttendanceRewardKind.LegendaryChest);picture.preserveAspect=true;picture.raycastTarget=false;
            T(bar,Loc.F("보관 중인 전설 상자 {0}개",store.Data.attendance.chests.Count),art+16,2,w-art-bw-30,h-4,UiTheme.Body,UiTheme.Gold);
            var open=B(bar,"상자 1개 열기",w-bw-6,(h-32*s)/2,bw,32*s,()=>
            {bool ok=store.OpenAttendanceChest(chest.id);notice=ok?Loc.T("전설 장비를 가방에 보관했습니다."):Loc.T(store.Error);noticeColor=ok?UiTheme.Gold:UiTheme.Danger;view.Repaint();},"attendance-open-chest");
            open.interactable=Economy.FreeSlots(store.Data.Hero)>0;
        }
        void DrawActions(float w,bool landscape)
        {
            float s=Scale,ctaH=40*s,ctaW=landscape?Mathf.Min(280*s,w*.46f):w,checkH=30*s,checkY=landscape?(ctaH-checkH)/2:0;bool hidden=settings.Hidden(kind,store.AttendanceClock());
            var toggle=B(view.Actions,"",0,checkY,1,checkH,()=>
            {notice=settings.SetHidden(kind,!settings.Hidden(kind,store.AttendanceClock()),store.AttendanceClock())?"":settings.Error;noticeColor=UiTheme.Danger;view.Repaint();},"attendance-hide-today");
            UiTheme.Skin(toggle,"quiet");toggle.gameObject.AddComponent<AttendanceCheckbox>().Checked=hidden;
            float box=18*s;var mark=UiLayout.Rect("Checkbox",toggle.transform);UiLayout.Place(mark,8,(checkH-box)/2,box,box);mark.gameObject.AddComponent<StorageSurface>().Paint(UiTheme.InsetHex,UiTheme.InsetHex,UiTheme.GoldHex);
            if(hidden){var check=UiLayout.Rect("Checkbox mark",mark);UiLayout.Stretch(check);var glyph=check.gameObject.AddComponent<StorageGlyph>();glyph.symbol="check";glyph.color=UiTheme.Gold;glyph.raycastTarget=false;}
            var label=T(toggle.transform,Loc.T("하루 동안 보여주지 않기"),box+16,0,1000,checkH,UiTheme.Body,UiTheme.Text);
            float tw=Mathf.Min(label.preferredWidth+4,(landscape?w-ctaW-12:w)-box-26);label.rectTransform.sizeDelta=new Vector2(tw,checkH);
            ((RectTransform)toggle.transform).sizeDelta=new Vector2(box+tw+26,checkH);
            long period=Track.period;
            var claim=B(view.Actions,"모든 보상 받기",w-ctaW,landscape?0:34*s,ctaW,ctaH,
                ()=>Claim(()=>store.ClaimAllAttendance(kind,period)),"attendance-claim-all",true);
            claim.interactable=Attendance.Pending(Track)>0;
        }
        void Claim(Func<bool> claim)
        {
            bool ok=claim();notice=ok?Loc.T("출석 보상을 받았습니다."):Loc.T(store.Error);
            noticeColor=ok?UiTheme.Gold:UiTheme.Danger;view.Repaint();
        }
        public static void Icon(Transform parent,AttendanceReward reward,float x,float y,float size)
        {
            if(reward.kind==AttendanceRewardKind.Gems)
            {
                foreach(var pair in new[]{("G03",0f),("G02",.34f),("G01",.66f)})JewelerArt.Gem(parent,pair.Item1,reward.tier,x+pair.Item2*size*.55f,y+pair.Item2*size*.15f,size*.62f);
                return;
            }
            var r=UiLayout.Rect("Reward art",parent);UiLayout.Place(r,x,y,size,size);
            var image=r.gameObject.AddComponent<Image>();image.sprite=AttendanceArt.For(reward.kind);image.preserveAspect=true;image.raycastTarget=false;
        }
        static void Seal(Transform parent,float cx,float cy,float size)
        {
            var r=UiLayout.Rect("Claimed seal",parent);UiLayout.Place(r,cx,cy,size,size);r.pivot=new Vector2(.5f,.5f);r.localRotation=Quaternion.Euler(0,0,12);
            var image=r.gameObject.AddComponent<Image>();image.sprite=AttendanceArt.Seal;image.preserveAspect=true;image.raycastTarget=false;
        }
        static void Glow(Transform parent,float cx,float cy,float radius,Color color,bool pulse)
        {
            if(radius<=0)return;var root=UiLayout.Rect("Reward glow",parent);UiLayout.Place(root,cx-radius,cy-radius,radius*2,radius*2);
            if(pulse)
            {
                var light=root.gameObject.AddComponent<AttendanceClaimGlow>();light.color=color;light.raycastTarget=false;
                return;
            }
            for(int n=0;n<6;n++)
            {
                float size=1-n*.14f;var ring=UiLayout.Rect("Glow disc",root);UiLayout.Place(ring,radius*(1-size),radius*(1-size),radius*2*size,radius*2*size);
                var disc=ring.gameObject.AddComponent<TownCircleGraphic>();disc.color=new Color(color.r,color.g,color.b,pulse?.045f:.032f);disc.raycastTarget=false;
            }
            if(pulse)root.gameObject.AddComponent<ForgeWorkingPulse>();
        }
        static void Dot(Transform parent,float x,float y,float size)
        {var r=UiLayout.Rect("Claim dot",parent);UiLayout.Place(r,x,y,size,size);var dot=r.gameObject.AddComponent<TownCircleGraphic>();dot.color=UiTheme.Claimable;dot.raycastTarget=false;}
        static StorageSurface Surface(Transform parent,string name,float x,float y,float w,float h,string top,string bottom,string edge="00000000")
        {var r=UiLayout.Rect(name,parent);UiLayout.Place(r,x,y,w,h);var surface=r.gameObject.AddComponent<StorageSurface>();surface.Paint(top,bottom,edge);surface.raycastTarget=false;return surface;}
        static float H(Text t)=>t.rectTransform.rect.height;
        static void Move(Text t,float x,float y)=>t.rectTransform.anchoredPosition=new Vector2(x,-y);
        static void SetHeight(RectTransform row,float h){row.sizeDelta=new Vector2(row.sizeDelta.x,h);var e=row.GetComponent<LayoutElement>();e.minHeight=e.preferredHeight=h;}
        RectTransform Row(string name,float w,float h)
        {var r=UiLayout.Rect(name,view.Body);r.sizeDelta=new Vector2(w,h);var e=r.gameObject.AddComponent<LayoutElement>();e.minHeight=e.preferredHeight=h;return r;}
        Button B(Transform parent,string label,float x,float y,float w,float h,Action action,string name,bool primary=false)
        {var b=view.Button(parent,label,x,y,w,h,action,primary);b.name=name;return b;}
        Text T(Transform parent,string text,float x,float y,float w,float h,int size,Color color,TextAnchor align=TextAnchor.MiddleLeft)
        {var t=UiLayout.Text(parent,"Attendance text",text,x,y,w,h,Mathf.RoundToInt(size*Scale),color);t.alignment=align;return t;}
        // Wrapped text sized to its own preferred height; callers position it with Move.
        Text Fit(Transform parent,string text,float w,int size,Color color,TextAnchor align=TextAnchor.MiddleCenter)
        {var t=T(parent,text,0,0,w,1000,size,color,align);t.rectTransform.sizeDelta=new Vector2(w,t.preferredHeight+2);return t;}
    }
    public sealed class AttendanceCheckbox:MonoBehaviour
    {
        public bool Checked;
    }
}
