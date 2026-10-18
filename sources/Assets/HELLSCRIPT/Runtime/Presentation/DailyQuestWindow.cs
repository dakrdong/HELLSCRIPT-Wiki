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
            var view=ContentWindowView.Open(parent,"일일 퀘스트",EquipmentViewSource.Owned,readingScale,session.Render,closed,new Vector2(1200,1600),skin:UiSkin.Casual);
            session.Countdown=view.gameObject.AddComponent<DailyQuestCountdown>();session.BindCountdown();
            StoreViewBinding.Attach(view,store,view.Repaint,visibleState:()=>StoreViewBinding.Key(store.Data.dailyQuests));return view;
        }
    }
    sealed class DailyQuestSession
    {
        readonly GameStore store;readonly Func<float> reading;readonly Action<AttendanceKind> attendance;
        ContentWindowView view;Text timer;string notice="",language=Loc.Language;
        public DailyQuestCountdown Countdown;
        float Scale=>reading?.Invoke()??1;
        public DailyQuestSession(GameStore store,Func<float> reading,Action<AttendanceKind> attendance)
        {this.store=store;this.reading=reading;this.attendance=attendance;}
        public void BindCountdown()=>Countdown?.Bind(timer,store.AttendanceClock,store.Data.dailyQuests.day,Refresh);
        void Refresh(){notice=store.RefreshDailyQuests()?"":store.Error;view.Repaint();}
        Text Text(Transform p,string name,string value,float x,float y,float w,float h,int size,Color color)
            =>UiLayout.Text(p,name,value,x,y,w,h,Mathf.RoundToInt(size*Scale),color,UiFonts.CasualBody());
        RectTransform Surface(Transform parent,string name,float x,float y,float w,float h,Color edge,Color? fill=null)
            =>DailyQuestCasualSkin.Surface(parent,name,x,y,w,h,fill??DailyQuestCasualSkin.Card,edge);
        void Glyph(Transform parent,string symbol,float x,float y,float size,Color color)
        {
            var r=UiLayout.Rect("Daily glyph "+symbol,parent);UiLayout.Place(r,x,y,size,size);
            if(AcceptedSemanticArt.TryDraw(r,"daily/"+symbol,color.a))return;
            var glyph=r.gameObject.AddComponent<StorageGlyph>();glyph.symbol=symbol;glyph.color=color;glyph.raycastTarget=false;
        }
        void Bar(Transform parent,string name,float x,float y,float w,float fraction,Color color)
        {
            var r=UiLayout.Rect(name,parent);UiLayout.Place(r,x,y,w,2);
            var background=r.gameObject.AddComponent<Image>();background.color=DailyQuestCasualSkin.Track;background.raycastTarget=false;
            if(fraction<=0)return;
            var fill=UiLayout.Rect(name+" fill",r);UiLayout.Place(fill,0,0,w*Mathf.Clamp01(fraction),2);
            var image=fill.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;
        }
        public void Render(ContentWindowView window)
        {
            view=window;if(language!=Loc.Language){language=Loc.Language;notice="";}
            float w=view.Width-24,tab=(w-12)/3;
            for(int i=0;i<3;i++)
            {
                int index=i;string title=i==0?"7일 출석":i==1?"28일 출석":"일일 퀘스트";
                var b=view.Tab(view.Navigation,title,i*(tab+6),0,tab,32,()=>{if(index<2)attendance?.Invoke((AttendanceKind)index);},i==2);
                b.name=i==2?"daily-quests-tab":"daily-attendance-"+i;
                DailyQuestCasualSkin.Display(b.GetComponentInChildren<Text>()).fontSize=Mathf.RoundToInt(UiTheme.Caption*Scale);
            }
            // Fixed-content adapter, as in RiftVictoryWindow. The shell still owns the safe area and actions.
            view.Body.GetComponent<VerticalLayoutGroup>().enabled=false;
            view.Body.GetComponent<ContentSizeFitter>().enabled=false;view.Scroll.enabled=false;UiLayout.Stretch(view.Body);
            bool wide=view.Frame.rect.width>view.Frame.rect.height;
            float available=view.Scroll.viewport.rect.height;
            float unit=Mathf.Min(w/(wide?760:381),available/(wide?318:720));
            var content=UiLayout.Rect("Daily quest fixed content",view.Body);
            float cw=w/unit,ch=available/unit;UiLayout.Place(content,0,0,cw,ch);content.localScale=Vector3.one*unit;
            var paper=content.gameObject.AddComponent<StorageSurface>();paper.top=paper.bottom=DailyQuestCasualSkin.Cream;paper.raycastTarget=false;
            float summaryH=wide?48:70,top=summaryH+8,noticeH=32,ruleH=60;
            Summary(content,cw,summaryH);
            if(store.Data.dailyQuests.quests.Count==0)
            {
                var empty=Surface(content,"Daily quest empty",0,top,cw,ch-top-noticeH,DailyQuestCasualSkin.Quiet);
                Glyph(empty,"info",12,12,22,DailyQuestCasualSkin.Warning);
                Text(empty,"daily-quest-empty",Loc.T("오늘의 일일 퀘스트를 준비하지 못했습니다. 저장 상태와 기기 시간을 확인한 뒤 새로고침해 주세요."),12,44,cw-24,80,UiTheme.Body,DailyQuestCasualSkin.Muted);
            }
            else
            {
                float cardW=wide?(cw-12)/3:cw;
                float cardH=wide?(ch-top-noticeH-6)/2:(ch-top-noticeH-ruleH-30)/5;
                for(int i=0;i<store.Data.dailyQuests.quests.Count;i++)
                    Quest(content,store.Data.dailyQuests.quests[i],wide?i%3*(cardW+6):0,top+(wide?i/3:i)*(cardH+6),cardW,cardH,wide);
                Rules(content,wide?2*(cardW+6):0,wide?top+cardH+6:ch-noticeH-ruleH,cardW,wide?cardH:ruleH);
            }
            Text(content,"daily-quest-notice",notice,0,ch-noticeH+4,cw,noticeH-4,UiTheme.Caption,notice==Loc.T("보상을 받았습니다.")?DailyQuestCasualSkin.SuccessText:DailyQuestCasualSkin.WarningText);
            Glyph(view.Actions,"clock",0,12,18,DailyQuestCasualSkin.Gold);
            timer=Text(view.Actions,"daily-quest-reset","",26,0,Mathf.Max(1,w-136),44,UiTheme.Caption,DailyQuestCasualSkin.Muted);timer.alignment=TextAnchor.MiddleLeft;
            var refresh=view.Button(view.Actions,"새로고침",w-104,4,104,36,Refresh);refresh.name="daily-quest-refresh";
            DailyQuestCasualSkin.Display(refresh.GetComponentInChildren<Text>());
            BindCountdown();
        }
        void Summary(Transform parent,float w,float h)
        {
            var state=store.Data.dailyQuests;int claimed=DailyQuests.ClaimedCoins(state),goals=0,ready=0;
            bool today=state.day==DailyQuests.Day(store.AttendanceClock());
            foreach(var q in state.quests){if(q.progress>=q.target)goals++;if(today&&!q.claimed&&q.progress>=q.target)ready+=q.reward;}
            DailyQuestCasualSkin.Coin(parent,0,3,18);
            DailyQuestCasualSkin.Emphasis(Text(parent,"Daily claimed label",Loc.T("오늘 받은 심연 주화"),24,0,w*.46f-24,18,UiTheme.Caption,DailyQuestCasualSkin.Muted));
            DailyQuestCasualSkin.Display(Text(parent,"daily-quest-claimed-coins",claimed+" / "+DailyQuests.DailyReward,0,18,w*.47f,30,UiTheme.Title,DailyQuestCasualSkin.GoldText));
            var done=Text(parent,"daily-quest-goals",Loc.F("목표 달성 {0} / {1}",goals,state.quests.Count),w*.48f,4,w*.52f,18,UiTheme.Body,DailyQuestCasualSkin.Body);done.alignment=TextAnchor.MiddleRight;
            string status=claimed==DailyQuests.DailyReward?Loc.T("오늘 보상 수령 완료"):ready>0?Loc.F("수령 가능 {0}개",ready):Loc.T("수령할 보상이 없습니다");
            var rewards=DailyQuestCasualSkin.Emphasis(Text(parent,"daily-quest-ready-coins",status,w*.48f,24,w*.52f,18,UiTheme.Caption,ready>0?DailyQuestCasualSkin.SuccessText:DailyQuestCasualSkin.Muted));rewards.alignment=TextAnchor.MiddleRight;
            Bar(parent,"Daily claimed progress",0,h-6,w,(float)claimed/DailyQuests.DailyReward,UiTheme.D4RankGoldOn);
        }
        void Quest(Transform parent,DailyQuestProgress q,float x,float y,float w,float h,bool wide)
        {
            long day=store.Data.dailyQuests.day;
            bool ready=!q.claimed&&q.progress>=q.target&&day==DailyQuests.Day(store.AttendanceClock());
            var row=Surface(parent,"Daily quest "+q.id,x,y,w,h,ready?DailyQuestCasualSkin.Ready:q.claimed?DailyQuestCasualSkin.Quiet:DailyQuestCasualSkin.Ink,
                q.claimed?DailyQuestCasualSkin.Claimed:DailyQuestCasualSkin.Card);
            string symbol=q.activity switch {DailyQuestActivity.SellEquipment=>"daily-sell",DailyQuestActivity.ClearRift=>"daily-rift",DailyQuestActivity.EnhanceEquipment=>"daily-enhance",DailyQuestActivity.SpendShopGold=>"daily-shop",_=>"daily-potion"};
            float left=wide?8:52;
            Glyph(row,symbol,wide?8:10,wide?6:10,wide?20:32,q.claimed?DailyQuestCasualSkin.Quiet:DailyQuestCasualSkin.Gold);
            DailyQuestCasualSkin.Display(Text(row,"Daily title "+q.id,Loc.T(DailyQuests.Title(q.activity)),wide?36:left,6,w-(wide?44:142),20,UiTheme.Heading,q.claimed?DailyQuestCasualSkin.Muted:DailyQuestCasualSkin.Ink));
            string description=q.activity switch
            {
                DailyQuestActivity.SellEquipment=>"실제 판매 수량 · 자동·창고 판매 포함",
                DailyQuestActivity.ClearRift=>"전리품 정리까지 완료 · 훈련·튜토리얼·중도 귀환 제외",
                DailyQuestActivity.EnhanceEquipment=>"완료한 강화 단계 수 · +10은 10단계",
                DailyQuestActivity.SpendShopGold=>"실제 결제 골드 · 무료 보급·강화·재련 제외",
                _=>"실제 소비 수량 · 훈련·튜토리얼·취소·실패 제외"
            };
            Text(row,"Daily description "+q.id,Loc.T(description),left,30,w-left-10,30,UiTheme.Body,DailyQuestCasualSkin.Muted);
            var progress=Text(row,"Daily progress "+q.id,wide?Loc.F("진행 {0:N0} / {1:N0}",q.progress,q.target):Loc.F("{0:N0} / {1:N0}",q.progress,q.target),wide?8:w-80,wide?62:8,wide?w-16:70,wide?14:16,UiTheme.Caption,DailyQuestCasualSkin.Body);
            progress.alignment=wide?TextAnchor.MiddleLeft:TextAnchor.MiddleRight;
            Bar(row,"Daily quest progress "+q.id,left,h-34,w-left-10,(float)q.progress/q.target,q.claimed||ready?DailyQuestCasualSkin.Ready:UiTheme.D4RankGoldOn);
            DailyQuestCasualSkin.Coin(row,left,h-25,18);
            Text(row,"Daily reward "+q.id,Loc.F("{0} 주화",q.reward),left+23,h-28,w-left-146,24,UiTheme.Body,q.claimed?DailyQuestCasualSkin.Muted:DailyQuestCasualSkin.GoldText).alignment=TextAnchor.MiddleLeft;
            var b=view.Button(row,q.claimed?"수령 완료":ready?"보상 수령":"진행 중",w-118,h-28,108,24,()=>
            {notice=store.ClaimDailyQuest(q.id,day)?Loc.T("보상을 받았습니다."):store.Error;view.Repaint();},primary:ready);
            b.name="daily-quest-claim-"+q.id;((UiButton)b).SetCompleted(q.claimed);
            ((UiButton)b).SoundCue="ui.click"; // Primary selects green chrome; keep the existing claim press sound.
            var label=DailyQuestCasualSkin.Display(b.GetComponentInChildren<Text>());label.fontSize=Mathf.RoundToInt(UiTheme.Caption*Scale);label.color=ready?view.Skin.Navy:view.Skin.Muted;
            if(q.claimed){Glyph(b.transform,"check",6,6,12,DailyQuestCasualSkin.SuccessText);UiLayout.Place(label.rectTransform,20,0,83,24);}
            b.interactable=ready;
        }
        void Rules(Transform parent,float x,float y,float w,float h)
        {
            var row=Surface(parent,"Daily quest rules",x,y,w,h,DailyQuestCasualSkin.InfoEdge,DailyQuestCasualSkin.Info);Glyph(row,"info",8,7,14,UiTheme.D4SteelText);
            DailyQuestCasualSkin.Display(Text(row,"Daily rules title",Loc.T("매일 같은 목표"),28,4,w-36,18,UiTheme.Caption,UiTheme.D4SteelText));
            Text(row,"Daily reward rule",Loc.T("하루 총 100개 · 추가 전체 완료 보상 없음"),8,24,w-16,h>70?28:16,UiTheme.Caption,DailyQuestCasualSkin.Body);
            Text(row,"Daily unlock rule",Loc.T("강화는 기존 해금 조건 적용 · 저단계 장비도 가능"),8,h>70?62:40,w-16,h>70?28:18,UiTheme.Caption,DailyQuestCasualSkin.Body);
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
            label.text=Loc.F("00:00 KST · 초기화까지 {0}",UiTime.Duration(Math.Max(0,(RiftEntryRules.Midnight(now)-now+999)/1000)));
        }
    }
}
