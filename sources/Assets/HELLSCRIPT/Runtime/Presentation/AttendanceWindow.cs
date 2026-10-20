using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using A=Hellscript.AttendanceCasualSkin;

namespace Hellscript
{
    public static class AttendanceWindow
    {
        public static ContentWindowView Open(Transform parent,GameStore store,AttendancePopupSettings settings,
            AttendanceKind kind,Func<float> reading,Action<AttendanceKind> visited,Action closed,Action dailyQuests=null)
        {
            var session=new AttendanceSession(store,settings,kind,reading,visited,dailyQuests);
            var view=ContentWindowView.Open(parent,"출석 이벤트",EquipmentViewSource.Owned,reading,session.Render,closed,skin:UiSkin.Casual,fit:EventsPlate.Fit);
            var swipe=view.gameObject.AddComponent<AttendanceSwipe>();session.Swipe=swipe;
            swipe.Initialize(view,session.MovePage);
            StoreViewBinding.Attach(view,store,view.Repaint,()=>!swipe.Dragging,session.DisplayKey);
            visited?.Invoke(kind);return view;
        }
    }
    // Landscape: featured reward on the altar beside the day grid. Portrait: the altar above the grid.
    // Tiles show reward definitions; grants stay in GameStore attendance transactions.
    sealed partial class AttendanceSession
    {
        const float Gap=6;
        // popup-altar.png: the altar's upper face, as a fraction of the art height from the top.
        const float AltarTop=.46f;
        readonly GameStore store;readonly AttendancePopupSettings settings;readonly Func<float> reading;readonly Action<AttendanceKind> visited;
        AttendanceKind kind;ContentWindowView view;string notice="",language=Loc.Language;Color noticeColor;int selected,observedEarned=-1;long selectedPeriod;
        public AttendanceSwipe Swipe;
        // Days claimed by the last collect press and when it happened: a seal drawn for them stamps down once, staggered for collect-all,
        // and a repaint during the stamp continues it instead of restarting it.
        readonly System.Collections.Generic.Dictionary<int,float> stamps=new System.Collections.Generic.Dictionary<int,float>();float stampClock;AttendanceKind stampKind;
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
        (string text,Color color) State(int day,bool ready,bool claimed)
        {
            if(notice!="")return(notice,noticeColor);
            if(claimed)return(Loc.T("수령 완료"),A.Skin.GreenInk);
            if(ready)return(Loc.T("수령 가능"),A.Skin.GreenInk);
            // Only calendar days after today can still add check-ins before the period resets.
            int needed=day-Track.earned;long left=Attendance.ResetDay(kind,LastDay)-LastDay-1;
            return needed>left?(Loc.T("이번 기간에는 받을 수 없습니다"),A.Skin.Muted):(Loc.F("남은 출석 {0}일",needed),A.Skin.Navy);
        }
        void Claim(Func<bool> claim)
        {
            var track=Track;var ready=Enumerable.Range(1,Attendance.Length(kind)).Where(d=>d<=track.earned&&!track.Claimed(d)).ToArray();
            bool ok=claim();notice=ok?Loc.T("출석 보상을 받았습니다."):Loc.T(store.Error);
            stamps.Clear();stampClock=Time.unscaledTime;stampKind=kind;
            if(ok)for(int i=0;i<ready.Length;i++)stamps[ready[i]]=Mathf.Min(i,7)*.07f;
            noticeColor=ok?A.GoldInk:A.Skin.Danger;view.Repaint();
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
        void Dim(Transform artwork)
        {
            if(!view.TryGetComponent<AttendanceRewardTone>(out var tone))tone=view.gameObject.AddComponent<AttendanceRewardTone>();
            foreach(var image in artwork.GetComponentsInChildren<Image>()){image.color=A.Alpha(A.Skin.White,.42f);image.material=tone.Claimed;}
        }
        static RectTransform Seal(Transform parent,float cx,float cy,float size)
        {
            var r=UiLayout.Rect("Claimed seal",parent);UiLayout.Place(r,cx,cy,size,size);r.pivot=new Vector2(.5f,.5f);r.localRotation=Quaternion.Euler(0,0,12);
            var image=r.gameObject.AddComponent<Image>();image.sprite=AttendanceArt.Seal;image.preserveAspect=true;image.raycastTarget=false;image.color=A.Alpha(A.Skin.White,.78f);
            return r;
        }
        void Stamp(RectTransform seal,int day)
        {
            if(kind!=stampKind||!stamps.TryGetValue(day,out float delay))return;
            float elapsed=Time.unscaledTime-stampClock;if(elapsed<delay+UiPopIn.Duration)UiPopIn.Play(seal,1.9f,delay,elapsed);
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
        {var r=UiLayout.Rect("Claim dot",parent);UiLayout.Place(r,x,y,size,size);var dot=r.gameObject.AddComponent<TownCircleGraphic>();dot.color=A.Skin.GreenShade;dot.raycastTarget=false;}
        static StorageSurface Surface(Transform parent,string name,float x,float y,float w,float h,Color top,Color bottom,Color edge,
            float radius=12,float border=2,float shadow=2)=>A.Surface(parent,name,x,y,w,h,top,bottom,edge,radius,border,shadow);
        Button B(Transform parent,string label,float x,float y,float w,float h,Action action,string name,bool primary=false)
        {var b=view.Button(parent,label,x,y,w,h,action,primary);b.name=name;if(primary)A.Primary(b);return b;}
        Text T(Transform parent,string text,float x,float y,float w,float h,int size,Color color,TextAnchor align=TextAnchor.MiddleLeft)
        {var t=UiLayout.Text(parent,"Attendance text",text,x,y,w,h,Mathf.RoundToInt(size*Scale),color,A.Skin.BodyFont);t.alignment=align;return t;}
        // Wrapped text sized to its own preferred height; callers position it with Move.
        Text Fit(Transform parent,string text,float w,int size,Color color,TextAnchor align=TextAnchor.MiddleCenter,bool display=false,bool emphasis=false)
        {var t=T(parent,text,0,0,w,1000,size,color,align);if(display)A.Display(t);if(emphasis)A.Emphasis(t);t.rectTransform.sizeDelta=new Vector2(w,t.preferredHeight+2);return t;}
    }
    public sealed class AttendanceCheckbox:MonoBehaviour
    {
        public bool Checked;
    }
}
