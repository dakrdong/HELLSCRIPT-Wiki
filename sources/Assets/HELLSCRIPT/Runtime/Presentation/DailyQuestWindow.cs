using System;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // Generated content entry; the shared event navigation retains the two attendance tracks.
    public static class DailyQuestWindow
    {
        public static ContentWindowView Open(Transform parent,GameStore store,Func<float> readingScale,
            Action<AttendanceKind> attendance,Action closed)
        {
            var session=new DailyQuestSession(store,readingScale,attendance);
            var view=ContentWindowView.Open(parent,"일일 퀘스트",EquipmentViewSource.Owned,readingScale,session.Render,closed,skin:UiSkin.Casual,fit:EventsPlate.Fit);
            session.Countdown=view.gameObject.AddComponent<DailyQuestCountdown>();session.BindCountdown();
            StoreViewBinding.Attach(view,store,view.Repaint,visibleState:()=>StoreViewBinding.Key(store.Data.dailyQuests));return view;
        }
    }
    // Draws the daily quest page of the casual events mockup in its CSS pixels: the board decides every size (portrait, landscape, PC).
    sealed partial class DailyQuestSession
    {
        readonly GameStore store;readonly Func<float> reading;readonly Action<AttendanceKind> attendance;
        ContentWindowView view;Text timer;string notice="",language=Loc.Language;
        public DailyQuestCountdown Countdown;
        public DailyQuestSession(GameStore store,Func<float> reading,Action<AttendanceKind> attendance)
        {this.store=store;this.reading=reading;this.attendance=attendance;}
        public void BindCountdown()=>Countdown?.Bind(timer,store.AttendanceClock,store.Data.dailyQuests.day,Refresh);
        void Refresh(){notice=store.RefreshDailyQuests()?"":store.Error;view.Repaint();}
        // The mockup's text styles: Jua labels, Noto regular/bold/black; sizes are already board pixels.
        Text Txt(Transform parent,string name,string value,float x,float y,float w,float h,int size,Color color,TextAnchor anchor=TextAnchor.MiddleLeft,int weight=400,bool jua=false)
        {
            var t=UiLayout.Text(parent,name,value,x,y,w,h,size,color,jua?UiSkin.Casual.LabelFont(value):UiFonts.CasualWeight(weight));
            t.alignment=anchor;t.horizontalOverflow=HorizontalWrapMode.Overflow;t.verticalOverflow=VerticalWrapMode.Overflow;return t;
        }
        float Measure(Text probe,string value){var settings=probe.GetGenerationSettings(Vector2.zero);settings.horizontalOverflow=HorizontalWrapMode.Overflow;return probe.cachedTextGeneratorForLayout.GetPreferredWidth(value,settings)/probe.pixelsPerUnit;}
        Text probe;
        // Width of a string in a style, for pills that wrap their content.
        float Width(string value,int size,int weight=400,bool jua=false)
        {
            if(probe==null){var host=UiLayout.Rect("Measure",view.Frame);UiLayout.Place(host,-400,-400,200,40);probe=host.gameObject.AddComponent<Text>();probe.color=Color.clear;probe.raycastTarget=false;}
            probe.font=jua?UiSkin.Casual.LabelFont(value):UiFonts.CasualWeight(weight);probe.fontSize=size;probe.text=value;return Measure(probe,value);
        }
        RectTransform Box(Transform parent,string name,float x,float y,float w,float h,Color fill,Color edge,float radius,float border=2,float shadow=0,Color? fillEnd=null)
        {
            var r=UiLayout.Rect(name,parent);UiLayout.Place(r,x,y,w,h);var face=r.gameObject.AddComponent<StorageSurface>();face.shadowOutside=true;face.raycastTarget=false;
            face.SetSkin(UiSkin.Casual,edge,radius,border,shadow,fill,fillEnd??fill);return r;
        }
        void Glyph(Transform parent,string symbol,float x,float y,float size,Color color)
        {
            var r=UiLayout.Rect("Daily glyph "+symbol,parent);UiLayout.Place(r,x,y,size,size);
            if(AcceptedSemanticArt.TryDraw(r,"daily/"+symbol,color.a))return;
            var glyph=r.gameObject.AddComponent<StorageGlyph>();glyph.symbol=symbol;glyph.color=color;glyph.raycastTarget=false;
        }
        // "have / need" as three runs, the way the mockup sets them: a Jua count, then a bold slash and total that touch each other.
        void Counts(Transform parent,float right,float haveY,float needY,string have,string need,int haveSize,int needSize,float gap)
        {
            float needW=Width(need,needSize,700),slashW=Width("/",needSize,700),haveW=Width(have,haveSize,jua:true),x=right-needW;
            float needH=needSize*1.45f;
            Txt(parent,"Daily need",need,x,needY,needW+2,needH,needSize,UiTheme.MockMutedInk,weight:700);
            Txt(parent,"Daily slash","/",x-slashW,needY,slashW+2,needH,needSize,UiTheme.MockMutedInk,weight:700);
            Txt(parent,"Daily have",have,x-slashW-gap-haveW,haveY,haveW+2,haveSize,haveSize,UiSkin.Casual.Navy,jua:true);
        }
        public void Render(ContentWindowView window)
        {
            view=window;if(language!=Loc.Language){language=Loc.Language;notice="";}
            var board=UiMock.Of(UiSafeArea.Current);var shell=new EventsPlate(view,board);
            var plate=view.Frame;plate.gameObject.name="Daily quests";
            shell.Draw("일일 퀘스트","log",2,Tabs,attendance);
            float pw=shell.Width,x=3+shell.ContentPadX,top=3+shell.Mast+shell.TabsHeight+shell.ContentPadY,w=pw-6-2*shell.ContentPadX;
            float summaryH=UiMock.Pick(board,78,48,92);Summary(shell,x,top,w,summaryH);
            float gridTop=top+summaryH+shell.ContentGap,gridBottom=shell.FooterTop-shell.ContentPadY,gap=UiMock.Pick(board,7,7,12);
            var quests=store.Data.dailyQuests.quests;
            if(quests.Count==0)
            {
                var empty=Box(plate,"Daily quest empty",x,gridTop,w,gridBottom-gridTop,UiTheme.MockInfoFill,UiTheme.MockInfoEdge,14);
                Glyph(empty,"info",12,12,17,UiTheme.D4SteelText);
                Txt(empty,"daily-quest-empty",Loc.T("오늘의 일일 퀘스트를 준비하지 못했습니다. 저장 상태와 기기 시간을 확인한 뒤 새로고침해 주세요."),12,36,w-24,60,12,UiTheme.MockBody,TextAnchor.UpperLeft);
            }
            else if(board==UiBoard.Portrait)
            {
                float rules=67,card=(gridBottom-gridTop-rules-5*gap)/5;
                for(int i=0;i<quests.Count;i++)Quest(plate,quests[i],board,x,gridTop+i*(card+gap),w,card);
                Rules(plate,board,x,gridTop+5*(card+gap),w,rules);
            }
            else
            {
                float card=(w-2*gap)/3,row=(gridBottom-gridTop-gap)/2;
                for(int i=0;i<quests.Count;i++)Quest(plate,quests[i],board,x+i%3*(card+gap),gridTop+i/3*(row+gap),card,row);
                Rules(plate,board,x+2*(card+gap),gridTop+row+gap,card,row);
            }
            if(notice!="")Toast(plate,pw,shell.FooterTop);
            Footer(shell);
            BindCountdown();
        }
        // Tab dots: something is waiting on that page (the shared navigation owns the order and the clicks).
        bool[] Tabs()=>new[]{Attendance.Pending(Attendance.Track(store.Data.attendance,AttendanceKind.Weekly))>0,Attendance.Pending(Attendance.Track(store.Data.attendance,AttendanceKind.Monthly))>0,DailyQuests.Pending(store.Data.dailyQuests)>0};
        void Summary(EventsPlate shell,float x,float y,float w,float h)
        {
            var board=shell.Board;var state=store.Data.dailyQuests;int claimed=DailyQuests.ClaimedCoins(state),goals=0,ready=0;
            bool today=state.day==DailyQuests.Day(store.AttendanceClock());
            foreach(var q in state.quests){if(q.progress>=q.target)goals++;if(today&&!q.claimed&&q.progress>=q.target)ready+=q.reward;}
            var plate=view.Frame;float inset=2,right=x+w-inset;
            float coin=UiMock.Pick(board,15,13,15),label=UiMock.Pick(board,11,10,12),have=UiMock.Pick(board,13,12,16);
            float labelY=UiMock.Pick(board,.7f,-.5f,6.1f),line=UiMock.Pick(board,33,28,44.4f);
            DailyQuestCasualSkin.Coin(plate,x+inset,y+UiMock.Pick(board,1.4f,.5f,7.5f),coin);
            DailyQuestCasualSkin.Emphasis(Txt(plate,"Daily claimed label",Loc.T("오늘 받은 심연 주화"),x+inset+coin+UiMock.Pick(board,7,7,7),y+labelY,UiMock.Pick(board,150,180,150),UiMock.Pick(board,16,14.5f,17.5f),(int)label,UiTheme.MockMutedInk,weight:700));
            float ly=y+line-2;int hs=(int)have;string claimedText=claimed.ToString(),totalText="/ "+DailyQuests.DailyReward;
            var claimedLabel=Txt(plate,"daily-quest-claimed-coins",claimedText,x+inset,ly,Width(claimedText,hs,jua:true)+2,hs+4,hs,UiTheme.MockMutedInk,jua:true);
            float afterClaimed=x+inset+Width(claimedText,hs,jua:true)+UiMock.Pick(board,7,5,7);
            Txt(plate,"Daily total",totalText,afterClaimed,ly,Width(totalText,hs,jua:true)+2,hs+4,hs,UiTheme.MockMutedInk,jua:true);
            float afterTotal=afterClaimed+Width(totalText,hs,jua:true)+UiMock.Pick(board,12,13,13);
            Txt(plate,"Daily total label",Loc.T("하루 총 보상"),afterTotal,ly,UiMock.Pick(board,80,60,80),hs+4,hs,UiTheme.MockMutedInk,jua:true);
            if(board!=UiBoard.Landscape)
            {
                float trackW=UiMock.Pick(board,150,0,180),trackH=UiMock.Pick(board,6,0,8);
                var track=Box(plate,"Daily claimed progress",x+inset,y+UiMock.Pick(board,58.8f,0,75.4f),trackW,trackH,UiTheme.MockTrack,Color.clear,trackH/2,0);
                float fraction=Mathf.Clamp01((float)claimed/DailyQuests.DailyReward);
                if(fraction>0)Box(track,"Daily claimed fill",0,0,trackW*fraction,trackH,UiTheme.D4RankGoldOn,Color.clear,trackH/2,0);
            }
            string status=claimed==DailyQuests.DailyReward?Loc.T("오늘 보상 수령 완료"):ready>0?Loc.F("수령 가능 {0}개",ready):Loc.T("수령할 보상이 없습니다");
            int goalSize=(int)UiMock.Pick(board,12,11,15),statusSize=(int)UiMock.Pick(board,11,11,12);
            var done=Txt(plate,"daily-quest-goals",Loc.F("목표 달성 {0} / {1}",goals,state.quests.Count),right-UiMock.Pick(board,130,130,170),y+UiMock.Pick(board,19.2f,-.3f,27.1f),UiMock.Pick(board,130,130,170),goalSize+3,goalSize,UiSkin.Casual.Navy,TextAnchor.MiddleRight,jua:true);
            DailyQuestCasualSkin.Emphasis(Txt(plate,"daily-quest-ready-coins",status,right-UiMock.Pick(board,130,150,170),y+UiMock.Pick(board,36.8f,-2.1f,49.1f),UiMock.Pick(board,130,150,170),UiMock.Pick(board,16,16,17.5f),statusSize,ready>0?DailyQuestCasualSkin.SuccessText:UiTheme.MockMutedInk,TextAnchor.MiddleRight,700));
            // Bottom rule of the summary band.
            if(board!=UiBoard.Landscape||true)Box(plate,"Daily summary rule",x,y+h-2,w,2,UiTheme.MockRule,Color.clear,0,0);
        }
        void Quest(Transform parent,DailyQuestProgress q,UiBoard board,float x,float y,float w,float h)
        {
            long day=store.Data.dailyQuests.day;
            bool ready=!q.claimed&&q.progress>=q.target&&day==DailyQuests.Day(store.AttendanceClock());
            var edge=q.claimed?DailyQuestCasualSkin.Quiet:UiSkin.Casual.Navy;
            var card=Box(parent,"Daily quest "+q.id,x,y,w,h,q.claimed?DailyQuestCasualSkin.Claimed:DailyQuestCasualSkin.Card,edge,14,2,3);
            if(ready)
            {
                // Claimable: a green left bar inside the border, a pale green wash and a soft green ring just outside.
                var glow=Box(parent,"Daily quest glow "+q.id,x-4,y-4,w+8,h+8,UiTheme.MockReadyGlow,Color.clear,18,0);glow.SetSiblingIndex(card.GetSiblingIndex());
                var wash=Box(card,"Daily quest wash",2,2,w-4,h-4,UiTheme.MockReadyWash,Color.clear,12,0,0,Color.white);
                Box(card,"Daily quest bar",2,10,4,h-20,UiTheme.MockBarReady,Color.clear,2,0);
            }
            string symbol=q.activity switch {DailyQuestActivity.SellEquipment=>"daily-sell",DailyQuestActivity.ClearRift=>"daily-rift",DailyQuestActivity.EnhanceEquipment=>"daily-enhance",DailyQuestActivity.SpendShopGold=>"daily-shop",_=>"daily-potion"};
            string description=q.activity switch
            {
                DailyQuestActivity.SellEquipment=>"실제 판매 수량 · 자동·창고 판매 포함",
                DailyQuestActivity.ClearRift=>"전리품 정리까지 완료 · 훈련·튜토리얼·중도 귀환 제외",
                DailyQuestActivity.EnhanceEquipment=>"완료한 강화 단계 수 · +10은 10단계",
                DailyQuestActivity.SpendShopGold=>"실제 결제 골드 · 무료 보급·강화·재련 제외",
                _=>"실제 소비 수량 · 훈련·튜토리얼·취소·실패 제외"
            };
            var ink=q.claimed?UiTheme.MockBody:UiSkin.Casual.Navy;var gold=q.claimed?DailyQuestCasualSkin.Quiet:DailyQuestCasualSkin.Gold;
            bool landscape=board==UiBoard.Landscape,portrait=board==UiBoard.Portrait;
            // Per-board card recipe (all board pixels, relative to the card).
            float pad=UiMock.Pick(board,12,11,20),icon=UiMock.Pick(board,32,0,37),iconPad=UiMock.Pick(board,12,13,20);
            float titleX=UiMock.Pick(board,54,42,67),titleY=UiMock.Pick(board,10.9f,11.1f,30.1f),titleH=UiMock.Pick(board,14,13,16);int titleSize=(int)titleH;
            if(landscape)Glyph(card,symbol,13,8,20,gold);
            else
            {
                var plate=Box(card,"Daily quest icon",iconPad,iconPad,icon,icon,q.claimed?UiTheme.MockTrack:UiTheme.MockIconTop,q.claimed?DailyQuestCasualSkin.Quiet:UiSkin.Casual.Navy,portrait?9:10,2,2,q.claimed?UiTheme.MockTrack:UiTheme.MockIconBottom);
                float glyph=UiMock.Pick(board,21,20,23);Glyph(plate,symbol,(icon-glyph)/2,(icon-glyph)/2,glyph,q.claimed?UiTheme.MockMutedInk:UiSkin.Casual.Navy);
            }
            DailyQuestCasualSkin.Display(Txt(card,"Daily title "+q.id,Loc.T(DailyQuests.Title(q.activity)),titleX,titleY,w-titleX-80,titleH,titleSize,q.claimed?UiTheme.MockBody:ink,jua:true));
            float descSize=UiMock.Pick(board,12,11,13),descX=UiMock.Pick(board,54,11,20),barY=UiMock.Pick(board,62.4f,61.5f,h-81),barH=UiMock.Pick(board,4,4,8);
            float barX=UiMock.Pick(board,54,11,20),barW=w-barX-UiMock.Pick(board,12,11,20);
            float progressTop=portrait?barY-16:landscape?barY-2:barY-27;
            float descH=landscape?30.3f:UiMock.Pick(board,17.5f,0,18.5f);
            float descTop=landscape?29.4f:portrait?29:(67+(progressTop-10)-descH)/2;
            var desc=Txt(card,"Daily description "+q.id,Loc.T(description),descX,descTop,w-descX-pad,descH,(int)descSize,UiTheme.MockBody,landscape?TextAnchor.UpperLeft:TextAnchor.MiddleLeft);
            desc.horizontalOverflow=HorizontalWrapMode.Wrap;
            string have=q.progress.ToString("N0"),need=q.target.ToString("N0");
            float right=w-UiMock.Pick(board,12,11,20);int haveSize=(int)UiMock.Pick(board,14,13,18),needSize=(int)UiMock.Pick(board,11,10,12);
            float haveY=UiMock.Pick(board,14,12,143.9f),needY=UiMock.Pick(board,12,11,144.4f);
            if(board==UiBoard.Pc)Txt(card,"Daily progress label "+q.id,Loc.T("진행"),20,145.4f,40,16,11,UiTheme.MockMutedInk,weight:700);
            Counts(card,right,haveY,needY,have,need,haveSize,needSize,UiMock.Pick(board,4.1f,4,5.4f));
            // Bar
            var track=Box(card,"Daily quest progress "+q.id,barX,barY,barW,barH,UiTheme.MockTrack,Color.clear,barH/2,0);
            float fraction=q.target<=0?0:Mathf.Clamp01((float)q.progress/q.target);
            if(fraction>0)Box(track,"Daily quest fill",0,0,barW*fraction,barH,q.claimed?UiTheme.MockBarClaimed:ready?UiTheme.MockBarReady:UiTheme.D4RankGoldOn,Color.clear,barH/2,0);
            // Reward pill
            float coinSize=UiMock.Pick(board,16,15,19),amountSize=UiMock.Pick(board,17,17,20),unitSize=10,padL=UiMock.Pick(board,4,4,5),padR=UiMock.Pick(board,10,9,12),gap=UiMock.Pick(board,5,4,6);
            string amountText=q.reward.ToString(),unitText=Loc.T("주화");float amountW=Width(amountText,(int)amountSize,jua:true),unitW=Width(unitText,(int)unitSize,700);
            float pillW=4+padL+coinSize+gap+amountW+gap+unitW+padR,pillH=UiMock.Pick(board,24.7f,24.7f,32);
            float pillY=UiMock.Pick(board,72.1f,73.2f,h-55);
            var pill=Box(card,"Daily reward "+q.id,barX,pillY,pillW,pillH,q.claimed?UiTheme.MockTrack:UiTheme.MockIconTop,q.claimed?UiTheme.MockDisabledEdge:UiTheme.MockRewardEdge,pillH/2,2,0);
            DailyQuestCasualSkin.Coin(pill,2+padL,(pillH-coinSize)/2,coinSize);
            DailyQuestCasualSkin.Display(Txt(pill,"Daily reward amount",amountText,2+padL+coinSize+gap,(pillH-amountSize-3)/2,amountW+2,amountSize+3,(int)amountSize,q.claimed?UiTheme.MockMutedInk:DailyQuestCasualSkin.GoldText,jua:true));
            DailyQuestCasualSkin.Emphasis(Txt(pill,"Daily reward unit",unitText,2+padL+coinSize+gap+amountW+gap,(pillH-14.5f)/2+.5f,unitW+2,14.5f,(int)unitSize,UiTheme.MockMutedInk,weight:700));
            // Claim button
            string claimText=Loc.T(q.claimed?"수령 완료":ready?"보상 수령":"진행 중");float claimPad=UiMock.Pick(board,10,8,12),claimMin=UiMock.Pick(board,100,98,104),claimH=UiMock.Pick(board,32,32,38);
            float claimW=Mathf.Max(claimMin,Width(claimText,14)+(q.claimed?19:0)+2*claimPad+(ready?6:4));
            float claimX=w-UiMock.Pick(board,12,11,20)-claimW,claimY=UiMock.Pick(board,68.4f,69.5f,h-58);
            var b=view.Button(card,q.claimed?"수령 완료":ready?"보상 수령":"진행 중",claimX,claimY,claimW,claimH,()=>
            {notice=store.ClaimDailyQuest(q.id,day)?Loc.T("보상을 받았습니다."):store.Error;view.Repaint();},primary:ready);
            b.name="daily-quest-claim-"+q.id;var button=(UiButton)b;button.SetCompleted(q.claimed);
            button.SoundCue="ui.click"; // Primary selects green chrome; keep the existing claim press sound.
            button.SetCasualAppearance(radius:12,borderWidth:ready?3:2,shadow:ready?3:0,edge:q.claimed?DailyQuestCasualSkin.Quiet:ready?UiSkin.Casual.Navy:UiTheme.MockDisabledEdge);
            var label=b.GetComponentInChildren<Text>();label.font=UiFonts.CasualWeight(400);label.fontSize=14;label.resizeTextForBestFit=false;label.fontStyle=FontStyle.Normal;
            label.color=ready?UiSkin.Casual.White:q.claimed?UiTheme.MockMutedInk:UiSkin.Casual.Navy;label.horizontalOverflow=HorizontalWrapMode.Overflow;
            UiLayout.Place(label.rectTransform,0,0,claimW,claimH);
            if(q.claimed)
            {
                float text=Width(Loc.T("수령 완료"),14),mark=14,left=(claimW-(mark+5+text))/2;
                Glyph(b.transform,"check",left,(claimH-mark)/2,mark,DailyQuestCasualSkin.SuccessText);
                label.alignment=TextAnchor.MiddleLeft;UiLayout.Place(label.rectTransform,left+mark+5,0,text+2,claimH);
            }
            b.interactable=ready;
        }
        void Rules(Transform parent,UiBoard board,float x,float y,float w,float h)
        {
            var card=Box(parent,"Daily quest rules",x,y,w,h,UiTheme.MockInfoFill,UiTheme.MockInfoEdge,14,2);
            bool portrait=board==UiBoard.Portrait,landscape=board==UiBoard.Landscape;
            float ix=portrait?12:landscape?14:22,icon=UiMock.Pick(board,14,15,17);int titleSize=(int)UiMock.Pick(board,11,12,13),line=(int)UiMock.Pick(board,11,11,12),note=(int)UiMock.Pick(board,10,10,11);
            float titleY=UiMock.Pick(board,19.3f,23,91.3f),firstY=UiMock.Pick(board,16.8f,40.6f,117.2f),noteY=UiMock.Pick(board,35,60,144.2f);
            Glyph(card,"info",ix,UiMock.Pick(board,18,22.3f,91.2f),icon,UiTheme.D4SteelText);
            DailyQuestCasualSkin.Display(Txt(card,"Daily rules title",Loc.T("매일 같은 목표"),ix+icon+UiMock.Pick(board,5,6,8),titleY,w-ix-icon-20,13,titleSize,UiTheme.D4SteelText,jua:true));
            float bx=portrait?104:ix;string strong=Loc.F("하루 총 {0}개",DailyQuests.DailyReward),extra=Loc.T("추가 전체 완료 보상 없음");
            float sw=Width(strong,line,900),dotW=Width("·",line);
            DailyQuestCasualSkin.Emphasis(Txt(card,"Daily reward rule total",strong,bx,firstY,sw+2,UiMock.Pick(board,16,16,17.5f),line,UiSkin.Casual.Navy,weight:900));
            Txt(card,"Daily reward rule dot","·",bx+sw+UiMock.Pick(board,5,5,5),firstY,dotW+2,16,line,UiTheme.MockBody);
            Txt(card,"Daily reward rule",extra,bx+sw+dotW+UiMock.Pick(board,10,10,12),firstY,w-bx-sw-dotW-24,16,line,UiTheme.MockBody);
            var unlock=Txt(card,"Daily unlock rule",Loc.T("강화는 기존 해금 조건 적용 · 저단계 장비도 가능"),bx,noteY,w-bx-12,landscape?28:16,note,UiTheme.MockMutedInk,TextAnchor.UpperLeft);unlock.horizontalOverflow=HorizontalWrapMode.Wrap;
        }
        void Toast(Transform parent,float pw,float footerTop)
        {
            string text=notice;float tw=Width(text,12,900)+30,th=UiMock.Pick(UiMock.Of(UiSafeArea.Current),35,31,38);
            var toast=Box(parent,"daily-quest-notice-card",(pw-tw)/2,footerTop-th-12,tw,th,UiSkin.Casual.White,UiSkin.Casual.Navy,14,3,4);
            var t=Txt(toast,"daily-quest-notice",text,0,0,tw,th,12,notice==Loc.T("보상을 받았습니다.")?DailyQuestCasualSkin.SuccessText:UiSkin.Casual.Navy,TextAnchor.MiddleCenter,900);
        }
        void Footer(EventsPlate shell)
        {
            var board=shell.Board;var plate=view.Frame;float y=shell.FooterTop,h=shell.Footer,pad=shell.FootPad;
            float clock=UiMock.Pick(board,16,17,17);
            Glyph(plate,"clock",3+pad,y+(h-clock)/2-.5f,clock,UiTheme.MockMutedInk);
            float tx=3+pad+clock+UiMock.Pick(board,6,8,8);
            if(board==UiBoard.Portrait)
            {
                DailyQuestCasualSkin.Emphasis(Txt(plate,"daily-quest-reset-label",Loc.T("매일 00:00 KST 초기화"),tx,y+10.8f,170,14.5f,10,UiTheme.MockMutedInk,weight:700));
                timer=Txt(plate,"daily-quest-reset","",tx,y+28.8f,80,12,10,UiSkin.Casual.Navy,jua:true);
            }
            else
            {
                int size=board==UiBoard.Pc?11:10;float labelW=Width(Loc.T("매일 00:00 KST 초기화"),size,700);
                DailyQuestCasualSkin.Emphasis(Txt(plate,"daily-quest-reset-label",Loc.T("매일 00:00 KST 초기화"),tx,y+(h-16)/2+UiMock.Pick(board,0,.4f,0),labelW+2,16,size,UiTheme.MockMutedInk,weight:700));
                timer=Txt(plate,"daily-quest-reset","",tx+labelW+UiMock.Pick(board,14,14,14),y+(h-size-4)/2+3,80,size+3,size,UiSkin.Casual.Navy,jua:true);
            }
            timer.fontStyle=FontStyle.Normal;
            float rw=UiMock.Pick(board,95.5f,97,99.5f),rh=UiMock.Pick(board,34,30,36);
            var refresh=view.Button(plate,"새로고침",view.Width-3-pad-rw,y+(h-2-rh)/2+2,rw,rh,Refresh);refresh.name="daily-quest-refresh";
            var rb=(UiButton)refresh;rb.SetCasualAppearance(radius:10,borderWidth:2,shadow:3,edge:UiSkin.Casual.Navy);
            var rl=refresh.GetComponentInChildren<Text>();rl.font=UiFonts.CasualWeight(400);rl.fontSize=14;rl.resizeTextForBestFit=false;rl.horizontalOverflow=HorizontalWrapMode.Overflow;rl.alignment=TextAnchor.MiddleLeft;
            float lw=Width(Loc.T("새로고침"),14),ic=14,group=ic+6+lw,left=(rw-group)/2;
            Glyph(refresh.transform,"repeat",left,(rh-ic)/2,ic,UiTheme.D4SteelText);UiLayout.Place(rl.rectTransform,left+ic+6,0,lw+2,rh);
        }
    }
    // Only updates presentation. Its controller owns the single midnight refresh attempt.
    sealed class DailyQuestCountdown:MonoBehaviour
    {
        Text label;Func<long> clock;Action changed;long day,lastSecond=-1,attemptedDay=-1;
        public void Bind(Text label,Func<long> clock,long day,Action changed)
        {this.label=label;this.clock=clock;this.day=day;this.changed=changed;lastSecond=-1;Tick();}
        void Update()=>Tick();
        void Tick()
        {
            if(label==null||clock==null)return;long now=clock(),second=now/1000;if(second==lastSecond)return;lastSecond=second;
            long current=DailyQuests.Day(now);
            if(current!=day&&current!=attemptedDay){attemptedDay=current;changed?.Invoke();return;}
            long left=Math.Max(0,(RiftEntryRules.Midnight(now)-now+999)/1000);
            label.text=string.Format("{0:00}:{1:00}:{2:00}",left/3600,left/60%60,left%60);
        }
    }
}
