using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        bool comparisonEditing;
        HeroSave comparisonHero;
        Text comparisonDifference;
        RectTransform comparisonLaidOutContent;
        Vector2 comparisonPageSize;
        HeroSave EditingHero=>comparisonEditing?comparisonHero:game.Active?game.Combat.Hero:game.Store.Data.Hero;
        int EditingLevel=>comparisonEditing?comparisonHero.level:game.Active?game.Combat.EffectiveLevel:game.Store.Data.Hero.level;
        int EditingPassiveSlots=>Math.Max(EditingHero.legacyPassiveSlots,ContentUnlocks.Rules.passiveLevels.Count(level=>EditingLevel>=level));
        static readonly string[] TrainingNames={"단일 표적","다수 표적","위험 패턴"};
        void ComparisonNote(string text,Color? color=null,int size=20)
        {Note(content,text,size,Mathf.Max(82,text.Split('\n').Sum(s=>Mathf.Max(1,Mathf.CeilToInt(s.Length/28f)))*(size+7)+30),color??pale);}
        public void ShowComparisonPicker()
        {
            if(!RequireContent(ContentUnlocks.Train))return;
            pageRepaint=()=>ShowComparisonPicker();Base("comparison-picker","훈련 A/B 비교","같은 캐릭터로 행동·소유 장비의 차이를 확인합니다");
            ComparisonNote("A는 현재 설정으로 훈련합니다. 결과를 읽고 B의 조건을 바꾸면, 같은 시작 상태와 배치에서 다시 훈련합니다. 한 번에 한 조건씩 바꾸면 차이를 이해하기 쉽습니다.");
            for(int i=0;i<3;i++){int training=i;BigButton(content,Loc.F("A/B {0:00} · {1}",i+1,TrainingNames[i]),()=>game.BeginComparison(training),i==0);}
            var saved=game.Store.Data.Hero.trainingComparison;if(saved!=null)BigButton(content,"최근 저장한 비교",()=>ShowComparisonRecord(saved));
            ComparisonNote("한 시도는 60전투초 또는 사망·표적 전멸까지 진행합니다. 진행 중인 비교는 앱을 종료하면 끝나며, 두 시도를 마친 결과는 직접 저장할 수 있습니다.",muted);
            FooterButton(0,1,"훈련장으로",ShowTraining);
        }
        public void ShowComparisonConditions()
        {
            if(!game.ComparisonRun)return;var run=game.Combat.State;bool paused=run.paused;run.paused=true;
            pageRepaint=()=>ShowComparisonConditions();Base("comparison-conditions","비교 중인 설정",Loc.F("{0} · {1} · 조건을 고정해 시험합니다", (game.Comparison.IsB?"B":"A"), TrainingNames[run.training]));
            ComparisonNote(Loc.F("{0} Lv.{1}",game.catalog.classNames[(int)game.Combat.Hero.heroClass],game.Combat.EffectiveLevel),gold);
            if(game.Combat.Hero.useEdict)ComparisonNote(Loc.F("사냥 칙령 · 장착 스킬\n{0}",string.Join(", ",game.Combat.Hero.edict.slots.Where(s=>s!="").Select(SkillName))));
            else ComparisonNote(Loc.F("{0}\n이동: {1} {2:0.#}m\n물약: HP {3:0}% 이하",run.build.name,BehaviorRules.Movements[(int)run.build.movement],run.build.distance,run.build.potionThreshold));
            ComparisonNote(Loc.F("이번 시도의 장비\n{0}",string.Join("\n",TrainingEquipmentSnapshot.Capture(game.Combat.Hero).Select(i=>Loc.F("{0}: {1} +{2}",GameCatalog.Slots[i.slot],i.DisplayName,i.enhancement)))));
            ComparisonNote("비교 중에는 설정을 유지합니다. 훈련을 마친 뒤 B 설정을 바꾸고 다시 실행할 수 있습니다. 이 화면을 닫으면 이전 일시정지 상태로 돌아갑니다.");
            FooterButton(0,1,"훈련으로 돌아가기",()=>{run.paused=paused;ShowBattle();});
        }
        public void ShowComparisonEditor()
        {
            if(game.Comparison==null||!game.Comparison.HasA||game.Active)return;
            comparisonEditing=true;comparisonHero=game.Comparison.Hero;editing=game.Comparison.A.build;BehaviorRules.Normalize(editing);
            comparisonEquipmentIds=TrainingEquipmentSnapshot.Capture(comparisonHero).Select(i=>i.id).ToList();
            comparisonEquipmentSlot=0;comparisonCandidateId=comparisonEquipmentIds.FirstOrDefault()??"";comparisonListOffset=comparisonDetailOffset=0;comparisonEquipmentDetail=false;
            buildWasPaused=false;expandedRule=0;buildScrollOffset=0;RenderBuild();
        }
        void AddComparisonDifference()
        {
            comparisonDifference=Note(content,"",19,100,gold);RefreshComparisonDifference();
        }
        void RefreshComparisonDifference()
        {
            if(!comparisonEditing||Page!="build"||comparisonDifference==null)return;
            comparisonHero.build=editing.Copy();var changes=TrainingComparisonChanges.Describe(game.Comparison.Hero,comparisonHero,game.catalog);
            string text=changes.Count==0?"A와 같은 설정입니다. 조건을 바꾸거나 같은 설정으로 재현성을 확인할 수 있습니다.":Loc.F("B에서 바꿀 내용\n{0}", string.Join("\n\n",changes.Select(NamedConditions)));
            comparisonDifference.text=Loc.T(text);var layout=comparisonDifference.transform.parent.GetComponent<LayoutElement>();
            layout.minHeight=layout.preferredHeight=Mathf.Max(100,text.Split('\n').Sum(s=>Mathf.Max(1,Mathf.CeilToInt(s.Length/30f)))*27+34);
        }
        public void ShowComparisonResult()
        {
            comparisonEditing=false;var pair=game.Comparison;if(pair==null){ShowTraining();return;}
            var a=pair.A;var b=pair.B;
            if(a!=null&&b!=null){ShowComparisonRecord(pair.Record());return;}
            pageRepaint=()=>ShowComparisonResult();Base("comparison-result","훈련 A/B 결과",Loc.F("{0} · {1}", TrainingNames[pair.Training], (a==null?"A 결과 확인":"A 완료 · B 설정을 바꿔 보세요")),true);
            if(!string.IsNullOrEmpty(game.ComparisonError))ComparisonNote(game.ComparisonError,gold);
            if(a==null)BigButton(content,"A 다시 실행",()=>game.RestartComparisonA(),true);
            else RenderComparisonMetrics(a,null,pair.Training);
            ComparisonNote("두 시도를 마치기 전에는 완성한 비교로 저장하지 않습니다. 실제 캐릭터의 경험치·장비·재화는 유지됩니다.",muted);
            FooterButton(0,a==null?1:2,"성소로",()=>game.ReturnTown());
            if(a!=null)FooterButton(1,2,"B 설정 만들기",ShowComparisonEditor,true);
        }
        public void ShowComparisonRecord(TrainingComparisonRecord record)
        {
            comparisonEditing=false;
            if(record==null||!record.Complete)
            {pageRepaint=()=>ShowComparisonRecord(record);Base("comparison-result","저장한 훈련 비교","이 결과는 현재 버전에서 비교할 수 없습니다");ComparisonNote("기록은 보존되어 있습니다. 새로운 비교를 시작하면 현재 버전의 지표를 확인할 수 있습니다.");FooterButton(0,1,"훈련장으로",ShowTraining);return;}
            var frozen=JsonUtility.FromJson<TrainingComparisonRecord>(JsonUtility.ToJson(record));var hero=JsonUtility.FromJson<HeroSave>(frozen.baselineHeroJson);
            pageRepaint=()=>ShowComparisonRecord(record);Base("comparison-result","훈련 A/B 결과",Loc.F("{0} Lv.{1} · {2} · {3}", game.catalog.classNames[(int)hero.heroClass], hero.level, TrainingNames[frozen.training],frozen.EquipmentChanged?"장비 변경 비교":"같은 기준 장비"),true);
            BigButton(content,"변경한 조건 보기",()=>ShowComparisonChanges(frozen));
            RenderComparisonMetrics(frozen.a,frozen.b,frozen.training);
            BigButton(content,"이 비교 결과 저장",()=>
            {
                void Save(){bool ok=game.Store.SaveTrainingComparison(frozen);ShowComparisonRecord(frozen);ShowToast(ok?"최근 훈련 비교로 저장했습니다.":game.Store.Error);}
                var previous=game.Store.Data.Hero.trainingComparison;
                if(previous!=null&&previous.id!=frozen.id)Confirm("이 캐릭터의 최근 비교 1개를 지금 결과로 바꿉니다. 저장할까요?",Save);else Save();
            },true);
            BigButton(content,"A 설정 활용",()=>ShowComparisonChoice(frozen,false));BigButton(content,"B 설정 활용",()=>ShowComparisonChoice(frozen,true));
            ComparisonNote("한 쌍의 시험 결과입니다. 피해·생존·행동 이유를 함께 보고 다음 설정을 고르세요. 장비와 현재 행동 설정은 아직 바뀌지 않았습니다.",muted);
            FooterButton(0,2,"성소로",()=>game.ReturnTown());
            FooterButton(1,2,"저장·활용으로",()=>
            {
                Canvas.ForceUpdateCanvases();var scroll=content.GetComponentInParent<ScrollRect>();
                scroll.StopMovement();scroll.verticalNormalizedPosition=0;
            },true);
        }
        void ComparisonValue(string title,string a,string b)
        {
            var row=Row(content,60);row.name="Comparison metric";var label=Label(row,title,19,muted);Place(label.rectTransform,8,6,256,48);
            var left=Label(row,title=="비교 항목"?a:"A  "+Loc.T(a),20,pale,TextAnchor.MiddleRight);Place(left.rectTransform,264,6,176,48);
            var right=Label(row,title=="비교 항목"?b??"—":"B  "+Loc.T(b??"—"),20,gold,TextAnchor.MiddleRight);Place(right.rectTransform,448,6,194,48);
        }
        void ReflowComparisonPages()
        {
            bool reading=Page.StartsWith("comparison-",StringComparison.Ordinal)&&Page!="comparison-equipment";
            if((!reading&&!(Page=="build"&&ComparisonEdictEditing))||content==null)return;
            var size=root.rect.size;if(comparisonLaidOutContent==content&&size==comparisonPageSize)return;
            Canvas.ForceUpdateCanvases();comparisonLaidOutContent=content;comparisonPageSize=size;
            var scroll=content.GetComponentInParent<ScrollRect>();var anchor=DialogReadingAnchor.Capture(scroll);
            headerTitle.fontSize=size.x<600?26:30;headerSubtitle.fontSize=17;
            Place(headerTitle.rectTransform,12,6,size.x-164,40);Place(headerSubtitle.rectTransform,12,50,size.x-164,40);
            Canvas.ForceUpdateCanvases();float titleHeight=Mathf.Max(36,headerTitle.preferredHeight),subtitleHeight=Mathf.Max(26,headerSubtitle.preferredHeight);
            float head=Mathf.Max(100,titleHeight+subtitleHeight+22),foot=80;
            Place(header,0,0,size.x,head);Place(headerTitle.rectTransform,12,6,size.x-164,titleHeight);Place(headerSubtitle.rectTransform,12,titleHeight+12,size.x-164,subtitleHeight);
            Place((RectTransform)header.GetComponentsInChildren<Button>().Single(b=>b.name=="설정·안내").transform,size.x-146,8,134,62);
            Place(footer,0,size.y-foot,size.x,foot);Canvas.ForceUpdateCanvases();
            foreach(var label in footer.GetComponentsInChildren<Text>())foot=Mathf.Max(foot,label.preferredHeight+28);
            Place(footer,0,size.y-foot,size.x,foot);Place(scroll.viewport,16,head+8,size.x-32,Mathf.Max(1,size.y-head-foot-16));
            FitInventoryContent(scroll);Canvas.ForceUpdateCanvases();
            foreach(RectTransform row in content)
            {
                if(row.name!="Comparison metric")continue;var texts=row.GetComponentsInChildren<Text>();float width=row.rect.width;
                bool narrow=width<620;float height=narrow?126:60;var layout=row.GetComponent<LayoutElement>();layout.minHeight=layout.preferredHeight=height;
                if(narrow)
                {
                    Place(texts[0].rectTransform,8,4,width-16,34);
                    Place(texts[1].rectTransform,8,43,width-16,34);texts[1].alignment=TextAnchor.MiddleLeft;
                    Place(texts[2].rectTransform,8,82,width-16,34);texts[2].alignment=TextAnchor.MiddleRight;
                }
                else
                {
                    Place(texts[0].rectTransform,8,6,width*.42f-16,48);
                    Place(texts[1].rectTransform,width*.42f,6,width*.27f-8,48);texts[1].alignment=TextAnchor.MiddleRight;
                    Place(texts[2].rectTransform,width*.69f,6,width*.31f-8,48);texts[2].alignment=TextAnchor.MiddleRight;
                }
            }
            FitInventoryContent(scroll);Canvas.ForceUpdateCanvases();anchor?.Restore();toastFrame.anchoredPosition=new Vector2(0,foot+4);
        }
        string FinishName(CombatFinish end)=>end==CombatFinish.Duration?"60초 완료":end==CombatFinish.HeroDeath?"사망":end==CombatFinish.TargetsDefeated?"표적 전멸":"중도 종료";
        void RenderComparisonMetrics(TrainingAttempt a,TrainingAttempt b,int training)
        {
            var x=a.statistics;var y=b?.statistics;
            ComparisonValue("비교 항목","A",b==null?"B 대기":"B");ComparisonValue("종료",FinishName(x.finish),y==null?null:FinishName(y.finish));
            ComparisonValue("전투 시간",Loc.F("{0:0.0}초", a.seconds),b==null?null:Loc.F("{0:0.0}초", b.seconds));ComparisonValue("실제 경과",Loc.F("{0:0.0}초", a.realSeconds),b==null?null:Loc.F("{0:0.0}초", b.realSeconds));
            ComparisonValue("남은 HP",$"{a.health:0}/{a.maxHealth:0}",b==null?null:$"{b.health:0}/{b.maxHealth:0}");
            ComparisonValue("총 피해",x.damage.ToString("N0"),y?.damage.ToString("N0"));ComparisonValue("실제 HP 피해",x.hpDamage.ToString("N0"),y?.hpDamage.ToString("N0"));
            ComparisonValue("전투 초당 피해",(x.damage/Math.Max(.05,a.seconds)).ToString("N1"),b==null?null:(y.damage/Math.Max(.05,b.seconds)).ToString("N1"));
            ComparisonValue("피격 판정",x.incomingHits.ToString(),y?.incomingHits.ToString());ComparisonValue("HP 손실",x.hpLost.ToString("N1"),y?.hpLost.ToString("N1"));
            ComparisonValue("보호막 흡수",x.absorbed.ToString("N1"),y?.absorbed.ToString("N1"));ComparisonValue("걸은 거리",$"{x.walkingDistance:0.0}m",y==null?null:$"{y.walkingDistance:0.0}m");
            ComparisonValue("이동기 시작/완료",$"{x.movementStarts}/{x.movementCompletions}",y==null?null:$"{y.movementStarts}/{y.movementCompletions}");
            ComparisonValue("자원 부족 판정",x.resourceChecks.ToString(),y?.resourceChecks.ToString());
            if(training==2)
            {
                ComparisonValue("예고 영역 이탈",$"{x.evasionSuccesses}/{x.evasionSuccesses+x.evasionHits}",y==null?null:$"{y.evasionSuccesses}/{y.evasionSuccesses+y.evasionHits}");
                ComparisonValue("회피 평가 제외",x.evasionExcluded.ToString(),y?.evasionExcluded.ToString());
                ComparisonNote("예고 안에서 이동기를 시작한 공격을 평가합니다. 잿불 사술사의 첫 피해 판정 때 영역을 벗어났으면 이탈로 셉니다. 취소된 공격은 제외하며, 보호막 흡수는 회피로 세지 않습니다.",muted,18);
            }
            else ComparisonNote("이 훈련의 표적은 공격하지 않습니다. 피격과 회피를 비교하려면 위험 패턴 훈련을 선택하세요.",muted,18);
            ComparisonNote("총 피해는 방어 계산 후의 피해이며, 실제 HP 피해는 과잉 피해를 제외한 값입니다. 자원 부족은 규칙 검사 횟수이며 여러 규칙이 같은 순간에 막힐 수도 있습니다.",muted,18);
            Note(content,"스킬·효과별 전체 기록",24,72,gold);
            var ids=x.skills.Select(s=>s.definitionId).Concat(y?.skills.Select(s=>s.definitionId)??Enumerable.Empty<string>()).Distinct().OrderBy(id=>id=="BASIC"?"":id);
            foreach(var id in ids)
            {
                var left=x.skills.Find(s=>s.definitionId==id)??new SkillStatistics();var right=y?.skills.Find(s=>s.definitionId==id)??new SkillStatistics();
                string name=id=="BASIC"?"기본 공격":game.catalog.skills.FirstOrDefault(s=>s.id==id)?.name??ItemCatalog.Unique(id)?.name??ItemCatalog.Sets.FirstOrDefault(s=>s.id+"4"==id)?.name??id;
                Note(content,name,22,58,gold);ComparisonValue("사용/발동",$"{left.starts}/{left.releases}",y==null?null:$"{right.starts}/{right.releases}");
                ComparisonValue("중단",left.interruptions.ToString(),y==null?null:right.interruptions.ToString());ComparisonValue("피해",left.damage.ToString("N0"),y==null?null:right.damage.ToString("N0"));
                ComparisonValue("자원 부족 판정",left.resourceChecks.ToString(),y==null?null:right.resourceChecks.ToString());
            }
        }
        void ShowComparisonChanges(TrainingComparisonRecord record)
        {
            pageRepaint=()=>ShowComparisonChanges(record);Base("comparison-changes","A와 B의 변경 조건","캐릭터·레벨·시드·시작 배치는 같습니다");
            var changes=record.DescribeChanges(game.catalog);
            if(changes.Count==0)ComparisonNote("A와 B는 같은 전투 설정입니다. 같은 조건에서 결과가 재현되는지 확인한 비교입니다.",gold);
            foreach(string change in changes)ComparisonNote(NamedConditions(change));
            ComparisonNote(record.version==1?"이전 기록에는 계정 룬 보드가 포함되지 않았습니다.":"비교 시작 때의 룬 보드를 사용합니다. 무기를 바꾸면 해당 무기의 보드 효과를 계산합니다.",muted);
            for(int side=0;side<2;side++){bool second=side==1;BigButton(content,second?"B 장비 원본 보기":"A 장비 원본 보기",()=>ShowComparisonEquipmentRecord(record,second));}
            FooterButton(0,1,"비교 결과로",()=>ShowComparisonRecord(record));
        }
        void ShowComparisonChoice(TrainingComparisonRecord record,bool useB)
        {
            var chosen=(useB?record.b:record.a).build.Copy();chosen.equipmentIds=record.Equipment(useB).Select(i=>i.id).ToList();
            pageRepaint=()=>ShowComparisonChoice(record,useB);Base("comparison-choice",Loc.F("{0} 설정 활용", (useB?"B":"A")),"현재 장비는 유지됩니다");
            var changes=record.useEdict?TrainingComparisonChanges.Describe(game.Store.Data.Hero,record.AttemptHero(useB),game.catalog):BuildEditing.DescribeCombatChanges(game.Store.Data.Hero.build,chosen,game.catalog,game.Store.Data.Hero.heroClass);
            ComparisonNote(record.useEdict?"슬롯에는 선택한 장비 참조·패시브·기본 행동을 저장합니다. 사냥 칙령 원본은 아래에서 별도로 불러온 뒤 직접 적용하세요.":"현재 행동과 선택한 설정의 차이를 확인하세요. 슬롯에 저장하거나, 성소의 행동 설계에서 편집안으로 불러와 직접 적용할 수 있습니다.");
            if(changes.Count==0)ComparisonNote("현재 행동과 같은 설정입니다.",gold);foreach(string change in changes)ComparisonNote(NamedConditions(change));
            for(int i=0;i<HuntEdict.PresetSlots;i++){int slot=i;BigButton(content,Loc.F("슬롯 {0}에 선택한 설정 저장", i+1),()=>
            {
                ShowPresetNameDialog(slot,chosen,name=>game.SaveComparisonPreset(record,useB,slot,name),()=>ShowComparisonChoice(record,useB));
            });}
            if(record.useEdict)BigButton(content,"실제 칙령 편집안으로 불러오기",()=>{game.ReturnTown();ShowEdictEditor();edictDraft=(useB?record.b:record.a).edict.Copy();RenderEdictEditor();},true);
            else BigButton(content,"실제 행동 편집안으로 불러오기",()=>{game.ReturnTown();ShowBuild();LoadEditing(chosen);},true);
            FooterButton(0,1,"비교 결과로",()=>ShowComparisonRecord(record));
        }
    }
}
