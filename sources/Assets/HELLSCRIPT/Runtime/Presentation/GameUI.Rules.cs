using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        int expandedRule;
        float buildScrollOffset;
        RunState decisionsPauseRun;
        bool decisionsWasPaused;
        void RestoreBuildScroll()
        {
            Canvas.ForceUpdateCanvases();var scroll=content.GetComponentInParent<UnityEngine.UI.ScrollRect>();
            scroll.StopMovement();content.anchoredPosition=new Vector2(0,Mathf.Clamp(buildScrollOffset,0,Mathf.Max(0,content.rect.height-scroll.viewport.rect.height)));
        }
        void FocusExpandedRule()
        {
            var title=content.GetComponentsInChildren<UnityEngine.UI.Button>().FirstOrDefault(b=>b.name.StartsWith($"{expandedRule+1:00}  "));
            if(title==null)return;var row=(RectTransform)title.transform.parent;buildScrollOffset=-(row.localPosition.y+row.rect.yMax);RestoreBuildScroll();
        }
        static readonly string[] UtilityNames={"스킬","기본 공격","장비 회수","상자 개봉","탐색·성소"};
        string NamedConditions(string value)
        {for(int i=0;i<game.catalog.skills.Count;i++)value=value.Replace(Loc.F("스킬 {0} / ", i+1),Loc.F("{0} / ",game.catalog.skills[i].name));return value;}
        string RuleName(Rule r)=>r.action==RuleAction.Skill&&r.skill>=0&&r.skill<game.catalog.skills.Count?game.catalog.skills[r.skill].name:UtilityNames[Mathf.Clamp((int)r.action,0,4)];
        bool ValidateEditing()
        {
            var errors=BehaviorRules.Validate(editing,EditingHero.heroClass);
            if(errors.Count==0)return true;ShowToast(errors[0]);return false;
        }
        void AddRule(Rule rule,int index=-1)
        {
            if(rule.enabled&&editing.rules.Count(r=>r.enabled)>=24){ShowToast("활성 규칙은 최대 24개입니다. 먼저 다른 규칙을 꺼 주세요.");return;}
            if(rule.action==RuleAction.Skill&&editing.rules.Count(r=>r.action==RuleAction.Skill&&r.skill==rule.skill)>=3){ShowToast("스킬 하나의 규칙은 최대 3개입니다.");return;}
            rule.id=Guid.NewGuid().ToString("N");
            if(index<0){index=editing.rules.FindIndex(r=>r.action!=RuleAction.Skill);if(index<0)index=editing.rules.Count;}
            editing.rules.Insert(index,rule);expandedRule=index;RenderBuild();FocusExpandedRule();
        }
        void RulePicker()
        {
            pageRepaint=()=>RulePicker();Base("rule-picker","행동 추가","장착 상태와 행동 규칙은 따로 관리합니다.");
            foreach(int skill in editing.activeSkills){int id=skill;BigButton(content,Loc.F("{0} 규칙 추가", game.catalog.skills[id].name),()=>AddRule(BehaviorRules.Make(id)));}
            foreach(var type in new[]{RuleAction.Basic,RuleAction.Loot,RuleAction.Chest,RuleAction.Explore})
            {var action=type;BigButton(content,Loc.F("{0} 규칙 추가", UtilityNames[(int)action]),()=>AddRule(BehaviorRules.Utility(action)));}
            FooterButton(0,1,"편집으로 돌아가기",RenderBuild);
        }
        void RenderRules()
        {
            Note(content,Loc.F("활성 행동 {0}/24개 · 총 {1}행 · 액티브당 최대 3규칙", editing.rules.Count(r=>r.enabled), editing.rules.Count),20,82,pale);
            BigButton(content,"+ 행동 규칙 추가",RulePicker);
            for(int n=0;n<editing.rules.Count;n++)
            {
                int index=n;var rule=editing.rules[index];var header=Row(content,70);
                var title=Button(header,Loc.F("{0:00}  {1}{2}", index+1, RuleName(rule), (rule.enabled?"":" · 꺼짐")),()=>{expandedRule=expandedRule==index?-1:index;RenderBuild();});Span((RectTransform)title.transform,8,10,234,50);
                var toggle=Button(header,rule.enabled?"ON":"OFF",()=>{if(!rule.enabled&&editing.rules.Count(r=>r.enabled)>=24){ShowToast("활성 규칙은 최대 24개입니다.");return;}rule.enabled=!rule.enabled;RenderBuild();});Right((RectTransform)toggle.transform,156,10,70,50);
                var up=Button(header,"↑",()=>{if(index<=0)return;editing.rules.RemoveAt(index);editing.rules.Insert(index-1,rule);expandedRule=index-1;RenderBuild();});Right((RectTransform)up.transform,82,10,66,50);
                var down=Button(header,"↓",()=>{if(index>=editing.rules.Count-1)return;editing.rules.RemoveAt(index);editing.rules.Insert(index+1,rule);expandedRule=index+1;RenderBuild();});Right((RectTransform)down.transform,8,10,66,50);
                if(expandedRule!=index)continue;
                string summary=NamedConditions(BehaviorRules.Summary(rule));Note(content,summary,19,Mathf.Max(70,Mathf.Ceil(summary.Length/38f)*26+18),pale);
                if(rule.action==RuleAction.Skill)
                {int unlock=game.catalog.skills[rule.skill].unlock;if(unlock>EditingLevel)Note(content,Loc.F("Lv.{0} 해금 전에는 실행하지 않습니다. 규칙은 보존됩니다.", unlock),19,68,gold);}
                Cycle(content,"이 규칙의 목표",BehaviorRules.Targets,(int)rule.target,i=>{rule.target=(RuleTarget)i;RenderBuild();});
                if(rule.target==RuleTarget.Dense)Cycle(content,"밀집 판정 반경",BehaviorRules.Radii.Select(r=>$"{r}m").ToArray(),Mathf.Max(0,Array.IndexOf(BehaviorRules.Radii,rule.targetRadius)),i=>{rule.targetRadius=BehaviorRules.Radii[i];RenderBuild();});
                if(rule.action==RuleAction.Skill&&(rule.skill==8||rule.skill==13))
                {
                    Cycle(content,"설치 위치",new[]{"목표 위치","무리 중심","자신 아래","관측한 이동 경로"},(int)rule.areaAim,i=>{rule.areaAim=(AreaAim)i;RenderBuild();});
                    if(rule.areaAim==AreaAim.ObservedPath)Note(content,"관측한 이동 방향으로 0.75초 앞을 조준합니다.\n관측이 없거나 오래되면 현재 목표 위치를 사용합니다.",18,82,pale);
                    if(rule.skill==13&&(game.Active?game.Combat.Stats:new HeroStats(EditingHero)).specials.Contains("LM01"))
                        Cycle(content,"눈보라 이동",new[]{"목표 추적","생성 위치 고정"},(int)rule.blizzardMode,i=>{rule.blizzardMode=(BlizzardMode)i;RenderBuild();});
                    Note(content,"설치 위치는 준비 시작에 결정합니다.\n이미 생성된 장판은 이후 설정 변경의 영향을 받지 않습니다.",18,82,pale);
                }
                BigButton(content,rule.overrideMovement?"✓ 이 규칙의 이동 설정 사용":"전체 이동 설정 사용",()=>{rule.overrideMovement=!rule.overrideMovement;rule.movement=editing.movement;rule.distance=editing.distance;RenderBuild();});
                if(rule.overrideMovement)
                {Cycle(content,"전투 이동",BehaviorRules.Movements,(int)rule.movement,i=>{rule.movement=(MovementMode)i;RenderBuild();});SliderRow(content,"이 규칙의 거리",rule.distance,.5f,10,v=>rule.distance=Mathf.Round(v*2)/2,"m");}
                if(rule.action==RuleAction.Skill&&(rule.skill==1||rule.skill==9||rule.skill==15))
                {
                    BigButton(content,rule.escape?"✓ 생존용 이동 · 허용 구간 중단 가능":"공격용 이동",()=>{rule.escape=!rule.escape;RenderBuild();});
                    Cycle(content,"이동 위치의 목적",BehaviorRules.Purposes,(int)rule.positionPurpose,i=>{rule.positionPurpose=(PositionPurpose)i;RenderBuild();});
                }
                BigButton(content,rule.always?"✓ 항상 검사":"조건 묶음 검사",()=>
                {rule.always=!rule.always;rule.groups.Clear();if(!rule.always)rule.groups.Add(new ConditionGroup{conditions={BehaviorRules.All[0].Create()}});RenderBuild();});
                if(!rule.always)
                {
                    for(int g=0;g<rule.groups.Count;g++)
                    {
                        int groupIndex=g;var group=rule.groups[g];Note(content,Loc.F("OR {0} · 이 묶음의 조건을 모두 충족하면 실행합니다.", g+1),19,60,gold);
                        for(int c=0;c<group.conditions.Count;c++)RenderCondition(rule,group,c);
                        if(group.conditions.Count<3)BigButton(content,"+ AND 조건 추가",()=>PickCondition(rule,group,-1));
                        BigButton(content,Loc.F("OR {0} 묶음 삭제", g+1),()=>{rule.groups.RemoveAt(groupIndex);RenderBuild();});
                    }
                    if(rule.groups.Count<2)BigButton(content,"+ OR 묶음 추가",()=>{rule.groups.Add(new ConditionGroup());RenderBuild();});
                }
                BigButton(content,"이 규칙 복제",()=>AddRule(rule.Copy(),index+1));
                BigButton(content,"이 규칙 삭제 · 장착 유지",()=>{editing.rules.RemoveAt(index);expandedRule=Mathf.Min(index,editing.rules.Count-1);RenderBuild();});
            }
            foreach(int id in editing.activeSkills.Where(s=>!editing.rules.Any(r=>r.action==RuleAction.Skill&&r.skill==s)))Note(content,Loc.F("{0}은 장착되어 있지만 실행 규칙이 없습니다.", game.catalog.skills[id].name),19,68,gold);
            var errors=BehaviorRules.Validate(editing,EditingHero.heroClass);
            foreach(string error in errors.Take(5))Note(content,error,19,88,new Color(1,.5f,.4f));
        }
        void PickCondition(Rule rule,ConditionGroup group,int index)
        {
            pageRepaint=()=>PickCondition(rule,group,index);Base("condition-picker","조건 선택","22종 · 조건 판정과 CD·자원·사거리 검사는 별도로 처리합니다.");
            foreach(var d in BehaviorRules.All)
            {
                var definition=d;var button=BigButton(content,d.name,()=>
                {
                    var c=definition.Create();c.skill=rule.action==RuleAction.Skill?rule.skill:(editing.activeSkills.Count>0?editing.activeSkills[0]:-1);
                    if(index<0)group.conditions.Add(c);else group.conditions[index]=c;RenderBuild();
                });button.gameObject.name=Loc.F("{0}  {1}",d.id,d.name);
            }
            FooterButton(0,1,"취소",RenderBuild);
        }
        void RenderCondition(Rule rule,ConditionGroup group,int index)
        {
            var c=group.conditions[index];var d=BehaviorRules.Definition(c.id);
            BigButton(content,Loc.F("AND {0}  {1}", index+1, (d?.name??"알 수 없는 조건")),()=>PickCondition(rule,group,index));
            if(d==null){Note(content,"조건을 다시 선택하세요.",19,62,gold);return;}
            Cycle(content,"비교",d.comparisons.Select(BehaviorRules.CompareText).ToArray(),Mathf.Max(0,Array.IndexOf(d.comparisons,c.comparison)),i=>{c.comparison=d.comparisons[i];RenderBuild();});
            if(BehaviorRules.UsesValue(c))SliderRow(content,"조건 값",c.value,d.min,d.max,v=>c.value=d.min+Mathf.Round((v-d.min)/d.step)*d.step,d.unit);
            if(c.id=="SC04")Cycle(content,"확인할 버프",BehaviorRules.Buffs,Mathf.Clamp(c.choice,0,BehaviorRules.Buffs.Length-1),i=>{c.choice=i;RenderBuild();});
            if(c.id=="SC05")Cycle(content,"피해 기록 범위",BehaviorRules.Windows.Select(v=>Loc.F("최근 {0}초", v)).ToArray(),Mathf.Max(0,Array.IndexOf(BehaviorRules.Windows,c.window)),i=>{c.window=BehaviorRules.Windows[i];RenderBuild();});
            if(c.id=="SC06")
            {
                if(rule.action==RuleAction.Skill)BigButton(content,c.predicted?"선택 스킬의 예상 명중 영역":"자신을 중심으로 감지",()=>{c.predicted=!c.predicted;RenderBuild();});
                if(!c.predicted)Cycle(content,"반경",BehaviorRules.Radii.Select(v=>$"{v}m").ToArray(),Mathf.Max(0,Array.IndexOf(BehaviorRules.Radii,c.radius)),i=>{c.radius=BehaviorRules.Radii[i];RenderBuild();});
            }
            string[] choices=c.id=="SC07"?new[]{"정예 또는 보스","정예","보스"}:c.id=="SC11"?new[]{"근접","돌진","원거리","장판","지원","폭발"}:c.id=="SC12"?new[]{"일반","정예","보스"}:c.id=="SC13"?BehaviorRules.States:c.id=="SC20"?new[]{"미등장","교전","처치"}:null;
            if(choices!=null)Cycle(content,"선택",choices,Mathf.Clamp(c.choice,0,choices.Length-1),i=>{c.choice=i;RenderBuild();});
            if(c.id=="SC09"&&c.comparison==Comparison.Between)SliderRow(content,"거리 끝값",c.upper,c.value,12,v=>c.upper=Mathf.Round(v*2)/2,"m");
            if(c.id=="SC15"||c.id=="SC22")
            {
                var skills=editing.activeSkills;if(skills.Count==0)Note(content,"참조할 스킬을 먼저 장착하세요.",19,64,gold);
                else Cycle(content,"참조 스킬",skills.Select(s=>game.catalog.skills[s].name).ToArray(),Mathf.Max(0,skills.IndexOf(c.skill)),i=>{c.skill=skills[i];RenderBuild();});
            }
            BigButton(content,Loc.F("AND {0} 조건 삭제", index+1),()=>{group.conditions.RemoveAt(index);RenderBuild();});
        }
        public void ShowRuleDecisions()
        {
            if(game.Combat==null)return;var run=game.Combat.State;
            if(Page!="decisions"||decisionsPauseRun!=run){decisionsPauseRun=run;decisionsWasPaused=run.paused;}
            bool paused=decisionsWasPaused;run.paused=true;
            pageRepaint=()=>ShowRuleDecisions();Base("decisions","행동 판단 기록","최근 80건 · 같은 사유는 묶어 표시합니다.");
            game.RecordGuide(()=>FirstPlayGuide.ReadDecision(game.Store.Data,run));
            var selected=FirstPlayGuide.SelectedDecision(run);int selectedRow=selected==null?-1:run.build.rules.FindIndex(r=>r.id==selected.ruleId);
            if(selectedRow>=0&&!(game.ComparisonRun&&game.Active))BigButton(content,"선택된 행동의 규칙 보기",()=>{run.paused=paused;ShowBuild();expandedRule=selectedRow;RenderBuild();FocusExpandedRule();});
            BigButton(content,"첫 플레이 안내",()=>{run.paused=paused;ShowOnboarding();});
            foreach(var record in run.decisions.OrderByDescending(d=>d.lastTime).ThenBy(d=>d.row).Take(80))
            {
                string code=record.code=="SELECTED"?"실행 선택":record.code=="PRIORITY"?"상위 규칙 우선":record.code=="CONDITION"?"조건 미충족":record.code=="COOLDOWN"?"CD 대기":record.code=="RESOURCE"?"자원 부족":record.code=="RANGE"?"사거리 밖":record.code=="LINE_OF_FIRE"?"사선 차단":record.code=="LANDING"?"착지 불가":record.code=="ACTION_LOCK"?"현재 동작 잠금":record.code=="LEVEL_LOCK"?"레벨 미해금":record.code=="DISABLED"?"규칙 꺼짐":record.code=="CHANNELING"?"유지 중":record.code=="NOT_EQUIPPED"?"미장착":record.code=="EFFECT_ACTIVE"?"효과 유지 중":record.code=="NO_TARGET"?"대상 없음":record.code=="NO_LOOT"?"회수 대상 없음":record.code=="NO_CHEST"?"개봉 대상 없음":record.code;
                if(record.code=="EDICT_AIM")code="사냥 칙령 사용 조건 대기";
                else if(record.code=="EDICT_OFF")code="사냥 칙령 자동 사용 꺼짐";
                else if(record.code=="RESOURCE_NOT_NEEDED")code="자원 보충 불필요";
                var enemy=run.enemies.Find(e=>e.id==record.targetId);string target=enemy==null?"대상 없음":enemy.boss?GameCatalog.BossNames[enemy.pattern]:GameCatalog.EnemyNames[enemy.kind];
                string heading=EdictRuleOrder.IsCompiledRule(record.ruleId)
                    ?Loc.F("{0} 사냥 칙령 · {1} ×{2}\n{3:0.0}–{4:0.0}초 / {5}",EdictRuleOrder.CompiledSkill(record.ruleId)<0?"기본 공격":game.catalog.skills[EdictRuleOrder.CompiledSkill(record.ruleId)].name,code,record.count,record.firstTime,record.lastTime,target)
                    :Loc.F("{0:00}번 · {1} ×{2}\n{3:0.0}–{4:0.0}초 / {5}",record.row+1,code,record.count,record.firstTime,record.lastTime,target);
                Note(content,heading,20,94,gold);
                string detail=NamedConditions(record.detail??"");Note(content,detail,18,Mathf.Max(70,Mathf.Ceil(detail.Length/40f)*25+20),pale);
            }
            if(run.decisions.Count==0)Note(content,"다음 전투 판단부터 기록됩니다.",21,90,pale);
            FooterButton(0,1,"전투로 돌아가기",()=>{run.paused=paused;ShowBattle();});
        }
    }
}
