using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // A small tactical illustration, not a second combat simulation. Recipes and policy definitions
    // supply the conditions; these authored silhouettes explain their movement, target and effect.
    public sealed class SkillPresetExampleView : MonoBehaviour
    {
        public struct Scenario
        {
            public string skill,preset,effect,motion,target,rule;
            public int enemies;
            public float value;
        }
        static readonly string[][] Effects={
            new[]{"spin","leap","cleave","stomp","shield","shout","mark","pull","bleed","reap","dash","counter","gather","wave","resource","cleanse","summon","strike"},
            new[]{"pierce","fan","trap","retreat","mark","empower","volley","snipe","trap","ice-trap","decoy","retreat","poison","split","turret","resource","rain","pursuit"},
            new[]{"fireball","blizzard","chain","blink","shield","freeze","pierce","wall","ice-spear","orb","lightning-orb","beam","resource","barrier","elements","gather","collapse","avatar"}
        };
        public Scenario Example {get;private set;}
        public string Scope {get;private set;}
        SkillTreeGraphic g;Font font;float scale;RectTransform root;
        Color enemy=>UiTheme.Danger;
        Color effect=>UiTheme.Resource;
        static Color A(Color color,float alpha){color.a=alpha;return color;}
        public static Scenario Describe(string scope,string preset)
        {
            var recipe=HuntEdictQuickPresets.For(scope).Single(p=>p.id==preset);
            var s=new Scenario{skill=scope.StartsWith("skill/",StringComparison.Ordinal)?scope.Substring(6):"BASIC",preset=preset,effect="strike",motion="none",target="single",rule="NATIVE",enemies=1};
            if(scope=="order"){s.effect="order";return s;}
            if(s.skill=="BASIC")
            {
                s.effect=scope=="basic/WARRIOR"?"strike":"pierce";
                if(preset=="resource")s.rule="LOW_RESOURCE";else if(preset=="finish"){s.rule="FINISH";s.target="low-health";}return s;
            }
            var skill=ClassSkills.Find(s.skill);int n=int.Parse(s.skill.Substring(1))-1;
            s.effect=Effects[skill.heroClass][n];
            if(new[]{"spin","cleave","stomp","gather","wave","fan","trap","ice-trap","split","rain","blizzard","chain","freeze","wall","orb","collapse"}.Contains(s.effect)){s.enemies=4;s.target="pack";}
            if(new[]{"shield","shout","resource","cleanse","barrier","elements","avatar","empower"}.Contains(s.effect)){s.target="self";s.enemies=2;}
            if(new[]{"leap","dash","pursuit"}.Contains(s.effect))s.motion="approach";
            if(new[]{"retreat","blink"}.Contains(s.effect))s.motion="retreat";
            if(new[]{"pull","gather"}.Contains(s.effect))s.motion="pull";
            // Expanded skills reuse the executable rule and threshold from their selected choice.
            var option=ClassSkillOptions.Find(s.skill);
            var pick=recipe.choices.FirstOrDefault(p=>p.id==s.skill);
            var choice=option?.choices.FirstOrDefault(c=>c.id==(pick?.choice??option.initial));
            if(choice!=null){s.rule=choice.rule;s.value=choice.value;}
            switch(s.rule)
            {
                case "ANY":s.enemies=1;s.target=s.target=="self"?"self":"single";break;
                case "GROUP":case "CONTROLLED_GROUP":s.enemies=Mathf.Max(3,(int)s.value);s.target=s.rule=="GROUP"?"pack":"controlled";break;
                case "ELITE":s.enemies=1;s.target="elite";break;
                case "MARKED":case "MARKED_FAR":s.enemies=1;s.target="marked";break;
                case "DISTANT":s.target="far";break;
                case "NEAR":s.target="near";break;
                case "CONTROLLED":case "SLOW_TARGET":s.target="controlled";break;
                case "NEW_DOT":case "POISONED":case "OWN_BLEED":case "BLEED_FINISH":s.target="status";break;
            }
            if(!skill.legacy&&s.skill!="M13")return s;
            // Legacy presets own several options at once. Keep their authored scene choices explicit;
            // labels and numbers still come from the recipe/rule rather than guessed combat timings.
            string key=s.skill+"/"+preset;
            switch(key)
            {
                case "W01/center":s.motion="center";s.rule="CENTER";break;
                case "W01/edge":s.motion="edge";s.rule="EDGE";break;
                case "W01/stand":s.motion="none";s.rule="STAND";s.value=3;break;
                case "W02/dense":s.motion="center";s.enemies=4;s.rule="CENTER";break;
                case "W02/escape":case "A04/escape":case "M04/survival":s.motion="retreat";s.rule="EMERGENCY";break;
                case "W02/balanced":s.motion="approach";s.rule="DISTANT";s.value=4;break;
                case "W03/group":case "A01/pack":case "A02/pack":case "M01/pack":s.enemies=4;s.target="pack";s.rule="GROUP";s.value=s.skill=="A01"?2:0;break;
                case "W03/elite":case "A01/elite":case "A02/focus":case "A05/elite":case "A06/elite":case "M01/elite":s.target="elite";s.enemies=1;s.rule="ELITE";break;
                case "W03/charge":case "M03/charge":case "M06/charge":s.rule="CHARGE";break;
                case "W04/interrupt":s.rule="WINDUP";s.target="casting";break;
                case "W04/retreat":s.motion="retreat";s.rule="RETREAT";break;
                case "W05/chain":case "M05/chain":s.rule="NO_SHIELD";break;
                case "W05/emergency":case "M05/emergency":s.rule="EMERGENCY";break;
                case "W06/resource":s.rule="LOW_RESOURCE";break;
                case "W06/boss":s.target="elite";s.enemies=1;s.rule="ELITE";break;
                case "A01/steady":case "M03/steady":s.enemies=1;s.target="single";s.rule="ANY";break;
                case "A02/safe":s.rule="DISTANT";s.target="far";break;
                case "A03/renew":s.enemies=1;s.target="single";s.rule="REPLACE";break;
                case "A03/defend":case "M02/defend":s.rule="PATH";s.target="approaching";break;
                case "A04/safe":case "M04/distance":s.rule="DISTANCE";break;
                case "A04/trap":s.rule="OWN_TRAP";s.effect="trap";break;
                case "A05/switch":s.rule="NEW_TARGET";s.enemies=3;break;
                case "A05/health":s.rule="HIGH_HP";s.target="high-health";s.enemies=3;break;
                case "A06/poison":s.rule="POISONED";s.target="status";s.enemies=3;break;
                case "M02/keep":s.rule="FOLLOW";s.motion="follow";break;
                case "M03/linked":s.rule="LINKED";s.enemies=4;break;
                case "M04/sparse":s.rule="SPARSE";s.motion="retreat";break;
                case "M06/here":s.rule="STAND";s.motion="none";break;
                case "M06/approach":s.rule="GROUP";s.motion="center";s.enemies=4;break;
                case "M13/hold":s.rule="HOLD";s.motion="none";s.value=.5f;break;
                case "M13/retreat":s.rule="RETREAT_CHARGE";s.motion="retreat";s.value=.9f;break;
            }
            return s;
        }
        public static SkillPresetExampleView Create(Transform parent,string scope,string preset,HuntEdictLoadout load,Font font,float x,float y,float w,float h,float reading)
        {
            var rect=UiLayout.Rect("Preset example "+scope+"/"+preset,parent);UiLayout.Place(rect,x,y,w,h);
            var v=rect.gameObject.AddComponent<SkillPresetExampleView>();v.root=rect;v.font=font;v.scale=w/400;v.Scope=scope;v.Example=Describe(scope,preset);
            v.g=SkillTreeGraphic.Create(rect,"Tactical diagram",0,0,w,h);v.Draw(load);return v;
        }
        void Line(Vector2 a,Vector2 b,Color c,float width=2)=>g.Line(a*scale,b*scale,width*scale,c);
        void Arrow(Vector2 a,Vector2 b,Color c)
        {Line(a,b,c,2);var d=(b-a).normalized;var n=new Vector2(-d.y,d.x);Line(b-d*9+n*5,b,c);Line(b-d*9-n*5,b,c);}
        void Ring(Vector2 p,float radius,Color c,float width=2)=>g.Ring(p*scale,radius*scale,width*scale,c);
        void Disc(Vector2 p,float radius,Color c)=>g.Disc(p*scale,radius*scale,c);
        void Label(string value,float x,float y,float w,float h,int size,Color color)
        {
            var r=UiLayout.Rect("Example label",root);UiLayout.Place(r,x*scale,y*scale,w*scale,h*scale);
            var t=r.gameObject.AddComponent<Text>();t.font=font;t.text=Loc.T(value);t.fontSize=Mathf.Max(8,Mathf.RoundToInt(size*scale));t.color=color;t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;
        }
        void Draw(HuntEdictLoadout load)
        {
            var s=Example;float h=root.rect.height/scale;
            g.Box(0,0,400*scale,h*scale,A(UiTheme.Background,.8f),UiTheme.Panel);
            for(int x=16;x<400;x+=24)Line(new Vector2(x,30),new Vector2(x,h-33),A(UiTheme.Border,.15f),.6f);
            for(int y=40;y<h-33;y+=24)Line(new Vector2(10,y),new Vector2(390,y),A(UiTheme.Border,.15f),.6f);
            Line(new Vector2(8,28),new Vector2(392,28),A(UiTheme.Gold,.4f),1);
            Label("전투 방식 예시",8,2,384,23,13,UiTheme.Gold);
            if(s.effect=="order"){DrawOrder(load);return;}
            Vector2 start=new Vector2(76,154),hero=start,target=new Vector2(s.target=="near"?155:292,132);
            if(s.motion=="center")hero=new Vector2(284,145);
            else if(s.motion=="edge")hero=new Vector2(246,174);
            else if(s.motion=="approach")hero=new Vector2(246,151);
            else if(s.motion=="retreat"){start=new Vector2(219,163);hero=new Vector2(75,192);}
            if(s.motion!="none"&&s.motion!="pull"&&s.motion!="follow")
            {Ring(start,14,A(UiTheme.Success,.45f));Arrow(start+(hero-start).normalized*19,hero-(hero-start).normalized*24,UiTheme.Success);}
            if(new[]{"spin","freeze","stomp"}.Contains(s.effect)&&s.motion!="edge")target=hero;
            var enemies=Enumerable.Range(0,s.enemies).Select(i=>target+(s.enemies==1?Vector2.zero:new Vector2(Mathf.Cos(i*Mathf.PI*2/s.enemies)*39,Mathf.Sin(i*Mathf.PI*2/s.enemies)*34))).ToArray();
            Color ink=s.skill.StartsWith("M",StringComparison.Ordinal)?UiTheme.Resource:UiTheme.Gold;
            if(s.effect=="spin")
            {
                Ring(hero,36,A(ink,.7f));Ring(hero,45,A(ink,.25f),5);
                for(int i=0;i<3;i++){float a=i*2.094f;Arrow(hero+new Vector2(Mathf.Cos(a)*36,Mathf.Sin(a)*36),hero+new Vector2(Mathf.Cos(a+.6f)*36,Mathf.Sin(a+.6f)*36),ink);}
            }
            else if(new[]{"shield","cleanse","counter","barrier","avatar"}.Contains(s.effect))
            {g.Glow(hero*scale,25*scale,48*scale,A(effect,.3f));Ring(hero,32,effect,3);Ring(hero,38,A(effect,.3f));}
            else if(new[]{"resource","shout","empower","elements"}.Contains(s.effect))
            {for(int i=0;i<3;i++)Arrow(hero+new Vector2(-30+i*30,36),hero+new Vector2(-30+i*30,5),UiTheme.Resource);Ring(hero,46,A(ink,.6f));}
            else if(new[]{"gather","pull"}.Contains(s.effect))
            {foreach(var e in enemies)Arrow(e-(e-hero).normalized*17,hero+(e-hero).normalized*36,ink);Ring(hero,45,A(ink,.4f));}
            else if(new[]{"trap","ice-trap","blizzard","wall","rain","freeze","stomp","collapse"}.Contains(s.effect))
            {
                var area=s.effect=="freeze"||s.effect=="stomp"||s.rule=="OWN_TRAP"?hero:s.rule=="PATH"?(hero+target)/2:target;
                g.Glow(area*scale,33*scale,62*scale,A(ink,.23f));Ring(area,52,A(ink,.7f));
                for(int i=0;i<6;i++){var p=area+new Vector2(Mathf.Cos(i)*27,Mathf.Sin(i)*27);g.Diamond(p*scale,4*scale,ink);}
                if(s.motion=="follow")Arrow(target,target+new Vector2(42,28),ink);
                if(s.effect=="rain")for(int i=0;i<4;i++)Arrow(area+new Vector2(-38+i*25,-64),area+new Vector2(-24+i*18,-12),ink);
            }
            else if(new[]{"summon","turret","decoy","lightning-orb"}.Contains(s.effect))
            {
                var ally=hero+new Vector2(74,-39);g.DiamondRing(ally*scale,19*scale,3*scale,UiTheme.Success);Ring(ally,29,A(UiTheme.Success,.3f));Arrow(ally+new Vector2(23,3),target-new Vector2(22,0),ink);
            }
            else if(s.effect=="mark")Ring(target,23,ink,3);
            else if(s.effect=="chain")
            {var p=hero+new Vector2(20,0);foreach(var e in enemies){var m=(p+e)/2;Line(p,m+new Vector2(0,-9),ink,3);Line(m+new Vector2(0,-9),m+new Vector2(8,8),ink,3);Line(m+new Vector2(8,8),e,ink,3);p=e;}}
            else if(s.effect=="fan"||s.effect=="split"||s.effect=="volley")
            {foreach(var e in enemies)Arrow(hero+new Vector2(20,-3),e-new Vector2(17,0),ink);}
            else if(s.effect!="retreat"&&s.effect!="blink")
            {Arrow(hero+new Vector2(23,0),target-new Vector2(20,0),ink);if(new[]{"fireball","orb","strike","cleave","leap"}.Contains(s.effect))Ring(target,28,A(ink,.65f),4);}
            for(int i=0;i<enemies.Length;i++)
            {
                var e=enemies[i];bool elite=s.target=="elite"&&i==0;float size=elite?18:12;
                g.Diamond(e*scale,(size+3)*scale,UiTheme.Background).DiamondRing(e*scale,size*scale,2*scale,enemy).Diamond(e*scale,(size-4)*scale,A(enemy,.65f));
                if(elite)for(int k=0;k<3;k++)g.Diamond((e+new Vector2(-9+k*9,-26))*scale,3*scale,UiTheme.Gold);
                if(s.target=="controlled"||s.target=="status")Ring(e,18,effect);
                if(s.target=="marked"){Ring(e,23,ink);Line(e-new Vector2(30,0),e+new Vector2(30,0),ink,1);Line(e-new Vector2(0,30),e+new Vector2(0,30),ink,1);}
                if(s.target=="casting"||s.rule=="WINDUP")Arrow(e+new Vector2(-18,5),hero+new Vector2(22,-5),A(enemy,.75f));
                float hp=s.target=="low-health"?.22f:s.target=="high-health"?(i==0?1:.3f):.7f;
                g.Box((e.x-14)*scale,(e.y-size-10)*scale,28*scale,3*scale,UiTheme.Background,UiTheme.Background);
                g.Box((e.x-14)*scale,(e.y-size-10)*scale,28*hp*scale,3*scale,enemy,enemy);
            }
            Disc(hero,19,UiTheme.Background);Ring(hero,19,UiTheme.Success,3);Disc(hero,13,A(UiTheme.Success,.7f));
            Arrow(hero+new Vector2(-6,4),hero+new Vector2(9,-3),UiTheme.Text);
            if(s.value>0&&new[]{"LOW_RESOURCE","LOW_HEALTH","RESERVE","HOLD","RETREAT_CHARGE"}.Contains(s.rule))
            {
                var color=s.rule=="LOW_HEALTH"?enemy:UiTheme.Resource;
                g.Box(66*scale,52*scale,90*scale,8*scale,UiTheme.Background,UiTheme.Background);
                g.Box(66*scale,52*scale,90*s.value*scale,8*scale,color,color);
                Line(new Vector2(66+90*s.value,48),new Vector2(66+90*s.value,64),UiTheme.Text,1);
            }
            if(s.motion=="retreat"||s.rule=="EMERGENCY")Ring(start,42,A(enemy,.5f));
            string cue=Cue(s);
            Label(cue,8,h-31,384,26,13,UiTheme.Text);
            if(s.skill!="BASIC")
            {
                var icon=SkillIconView.Create(root,false);UiLayout.Place(icon.Rect,12*scale,40*scale,34*scale,34*scale);icon.SetSkill(s.skill);
            }
        }
        static string Cue(Scenario s)
        {
            switch(s.rule)
            {
                case "CENTER":return Loc.T("중심으로 이동");case "EDGE":return Loc.T("가장자리 유지");
                case "STAND":return s.value>0?Loc.F("제자리 · {0}초",s.value):Loc.T("제자리에서 사용");
                case "DISTANT":case "MARKED_FAR":return s.value>0?Loc.F("거리 {0}m 이상",s.value):Loc.T("먼 거리에서 사용");
                case "GROUP":case "CONTROLLED_GROUP":return s.value>0?Loc.F("적 {0}명 이상",s.value):Loc.T("무리를 함께 공격");
                case "LOW_RESOURCE":return s.value>0?Loc.F("자원 {0}% 이하",s.value*100):Loc.T("자원 보충");
                case "LOW_HEALTH":return Loc.F("HP {0}% 이하",s.value*100);
                case "RESERVE":return Loc.F("사용 후 자원 {0}% 보존",s.value*100);
                case "RETREAT_CHARGE":return Loc.T("거리 확보 → 충분히 충전");case "HOLD":return Loc.T("제자리 → 짧게 충전");
                case "EMERGENCY":return Loc.T("위급할 때 사용");case "NO_SHIELD":return Loc.T("보호막이 끝난 뒤 사용");
                case "WINDUP":return Loc.T("적의 공격 준비에 대응");case "ELITE":return Loc.T("정예·보스 우선");
                case "PATH":return Loc.T("접근 경로에 설치");case "FOLLOW":return Loc.T("대상을 따라 유지");
                case "ANY":return Loc.T("기회가 오면 바로 사용");case "NEW_TARGET":return Loc.T("새 대상으로 이전");
                case "HIGH_HP":return Loc.T("HP가 높은 적 우선");case "FINISH":return Loc.T("HP가 낮은 적 마무리");
                case "NEAR":return Loc.F("거리 {0}m 이내",s.value);
                case "CONTROLLED":return Loc.T("제어된 적에게 사용");case "UNCONTROLLED":return Loc.T("제어되지 않은 적에게 사용");
                case "SLOW_TARGET":return Loc.T("둔화된 적을 빙결");case "SLOWED":return Loc.T("내 둔화를 해제");
                case "NEW_DOT":return Loc.T("지속 피해를 새 대상에 분배");case "POISONED":return Loc.T("중독된 적과 연계");
                case "OWN_BLEED":return Loc.T("내 출혈을 회수");case "BLEED_FINISH":return Loc.T("출혈 종료·회복에 맞춰 사용");
                case "MARKED":return Loc.T("표식 대상 집중");case "SHOUT":return Loc.T("함성 강화와 연계");
                case "PAID_READY":return Loc.T("자원을 쓰는 스킬 직전에 사용");case "CHAIN_READY":return Loc.T("연쇄 번개 준비 후 사용");
                case "CHAIN_HIT":return Loc.T("연쇄 번개 적중 후 사용");case "CHARGE":return Loc.T("충전과 연계");
                case "ELEMENTS_READY":case "CYCLE_READY":return Loc.T("세 원소 준비 후 사용");case "ELEMENT_BUFF":return Loc.T("원소 강화와 연계");
                case "SPARSE":case "DISTANCE":case "RETREAT":return Loc.T("안전 거리 확보");case "OWN_TRAP":return Loc.T("자기 덫으로 유도");
                case "LINKED":return Loc.T("적 사이로 연결");case "REPLACE":return Loc.T("위치를 바꾸어 설치");
                case "NATIVE":return Loc.T("기본 조건을 만족하면 사용");default:return Loc.T("조건을 갖춘 뒤 연계");
            }
        }
        void DrawOrder(HuntEdictLoadout load)
        {
            var ids=HuntEdictQuickPresets.Apply(load,Scope,Example.preset).classSkills.order;
            float cell=360f/Mathf.Max(1,ids.Length);
            for(int i=0;i<ids.Length;i++)
            {
                var c=new Vector2(20+cell*(i+.5f),130);Ring(c,22,UiTheme.Gold);
                if(ids[i]!="BASIC"){var icon=SkillIconView.Create(root,false);UiLayout.Place(icon.Rect,(c.x-18)*scale,(c.y-18)*scale,36*scale,36*scale);icon.SetSkill(ids[i]);}
                else Label("기본",c.x-22,c.y-14,44,28,11,UiTheme.Text);
                Label((i+1).ToString(),c.x-15,167,30,24,15,UiTheme.Gold);
                if(i<ids.Length-1)Arrow(c+new Vector2(26,0),c+new Vector2(cell-26,0),UiTheme.Success);
            }
            Label("왼쪽부터 사용 가능 여부를 확인합니다.",12,226,376,31,12,UiTheme.Text);
        }
    }
}
