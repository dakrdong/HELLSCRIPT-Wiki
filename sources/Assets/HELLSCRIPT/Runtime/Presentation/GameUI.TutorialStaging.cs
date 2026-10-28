using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // Staging for the prologue: who speaks, what the camera and the world show, and the two forced Hunt Edict lessons.
    // The run, its phases and the reward stay with CombatSimulation, GameController and GameStore; every beat ends in
    // game.ContinueTutorial or game.FinishTutorial, and each lesson step is derived from the hero's real saved state.
    public sealed partial class GameUI
    {
        public const string TutorialGuide="anton-jindark";
        const string V=NpcProfiles.Voice;
        TutorialCinematic cinematic;PrologueGate gate;TutorialAnchorRing prologueRing;
        string stagedRun="";bool bossIntroShown,killBark,bossBark,interventionTold,lessonClosing;
        public TutorialCinematic Staging=>cinematic;
        public PrologueGate Gate=>gate;
        NpcProfile GuideProfile=>NpcProfiles.Find(TutorialGuide);
        NpcProfile VoiceProfile=>NpcProfiles.Find(V);
        HeroSave PrologueHero=>game.Combat?.Hero??game.Store.Data.Hero;
        string HeroSpeaker=>"hero-"+PrologueHero.heroClass.ToString().ToLowerInvariant();
        // The hero listens, dimmed, on the right of every conversation; the art shares the NPC portrait import settings.
        string HeroPortrait=>"Art/NpcPortraits/"+HeroSpeaker;
        // The Hunt Edict opens only while a lesson asks for it.
        public bool PrologueEdictOpen=>game.TutorialActive&&!cinematic.Busy&&(GuideActive?GuideSkillLesson:(game.Combat.State.tutorialPhase==ProloguePhase.EdictLesson||
            game.Combat.State.tutorialPhase==ProloguePhase.MovementLesson||game.Combat.State.tutorialPhase==ProloguePhase.SurvivalLesson&&interventionTold));
        void ResetStaging(RunState run)
        {if(stagedRun==run.id)return;stagedRun=run.id;bossIntroShown=killBark=bossBark=interventionTold=lessonClosing=false;pitBarks.Clear();}
        void EnsureGate()
        {
            if(gate!=null)return;gate=PrologueGate.Create(PrologueOverlay(),()=>InterfaceFactor);
            ContentWindowHost.BackLocked=TutorialBackLocked;
        }
        bool TutorialBackLocked()=>gate!=null&&gate.Active||RuneIntroductionActive;
        void OnDestroy()
        {
            UnbindGuideModifierTree();
            if(ContentWindowHost.BackLocked==TutorialBackLocked)ContentWindowHost.BackLocked=null;
            if(gate!=null){if(Application.isPlaying)Destroy(gate.gameObject);else DestroyImmediate(gate.gameObject);}gate=null;
        }
        string TutorialObjective(RunState r)=>GuideTutorial.Running(r)?GuideObjective(r):PitRun(r)?PitObjective(r):r.tutorialPhase switch
        {
            ProloguePhase.Surrounded=>Loc.T("목표 · 하늘의 목소리에 귀 기울이기"),
            ProloguePhase.EdictLesson=>Loc.T("목표 · 사냥 칙령 받아들이기"),
            ProloguePhase.Encircled=>Loc.F("목표 · 포위한 마물 {0}/{1}마리 처치",r.enemies.Count(e=>e.dead&&!e.boss),r.enemies.Count(e=>!e.boss)),
            ProloguePhase.Gatekeeper or ProloguePhase.Showdown=>Loc.T("목표 · 길 끝의 수문장 처치"),
            ProloguePhase.SurvivalLesson=>Loc.T("목표 · 살아남는 법을 칙령에 새기기"),
            _=>Loc.T("수문장을 쓰러뜨렸다")
        };
        // Plays the staging for the beat the run has reached, then opens its dialogue.
        public void PresentTutorialStep()
        {
            if(!game.TutorialActive)return;var r=game.Combat.State;ResetStaging(r);
            if(game.Combat.IsGuide){if(GuideTutorial.Waiting(r.tutorialPhase)&&tutorialJournal==null)ShowGuideBeat();return;}
            int key=r.tutorialPhase+(r.health<=0?10:0);if(key==tutorialPromptPhase&&(tutorialJournal!=null||cinematic.Busy))return;
            tutorialPromptPhase=key;
            var beat=PitRun(r)?PitBeat(r):r.health<=0?DefeatBeat():r.tutorialPhase switch
            {
                ProloguePhase.Surrounded=>IntroBeat(),ProloguePhase.Gatekeeper=>ClearedPackBeat(),
                ProloguePhase.SurvivalLesson=>InterventionBeat(),ProloguePhase.Cleared=>VictoryBeat(),_=>null
            };
            if(beat==null){ShowTutorialPrompt();return;}
            cinematic.StopAllCoroutines();cinematic.StartCoroutine(beat);
        }
        // The HUD button and a return request re-open the current beat's dialogue. Lessons speak through the gate instead.
        public void ShowTutorialPrompt()
        {
            if(GuideActive){if(GuideTutorial.Waiting(game.Combat.State.tutorialPhase))ShowGuideBeat();return;}
            if(!game.TutorialActive)return;var r=game.Combat.State;ResetStaging(r);
            if(cinematic.Playing){cinematic.StopAllCoroutines();cinematic.HideCard();cinematic.FadeTo(0,.3f);cinematic.End();}
            var lines=new List<StoryLine>();var choices=PrologueScript(r,lines);
            if(choices==null){ShowToast(Loc.T("빛나는 곳을 따라 사냥 칙령을 설정하세요."));return;}
            OpenPrologueDialogue(lines,choices);
        }
        void OpenPrologueDialogue(List<StoryLine> lines,StoryChoice[] choices)
        {
            CloseTutorialJournal();var r=game.Combat.State;
            tutorialJournal=StoryDialogueWindow.Open(transform,PitRun(r)?"최종 시험":"칙령의 목소리",EquipmentViewSource.Owned,()=>InterfaceFactor,
                new StoryDialogueState(lines,PrologueHero.gender),()=>choices,()=>tutorialJournal=null,"",blocksGameplay:false,listener:HeroPortrait);
        }
        StoryChoice[] Go(string label,Action act)=>new[]{new StoryChoice("tutorial-continue",label,()=>{CloseTutorialJournal();act();},true)};
        StoryChoice[] PrologueScript(RunState r,List<StoryLine> lines)
        {
            if(PitRun(r))return PitScript(r,lines);
            string H=HeroSpeaker;
            if(r.health<=0)
            {
                lines.Add(new StoryLine(V,"일어나라. 네게 내린 명은 아직 다하지 않았다."));
                lines.Add(new StoryLine(V,"붉은 예고가 차오르면 그 자리를 벗어나라. 칙령이 너를 이끌리라."));
                return Go("다시 일어선다",()=>{cinematic.Vignette(Color.black,0);game.ContinueTutorial();});
            }
            switch(r.tutorialPhase)
            {
                case ProloguePhase.Surrounded:
                    if(r.tutorialFlowVersion==3)
                    {
                        lines.Add(new StoryLine(V,"사냥은 자동으로 진행됩니다. 당신은 싸우는 규칙을 정합니다."));
                        lines.Add(new StoryLine(V,"첫 스킬을 배우고 두 가지 사용 방식을 비교해 보세요."));
                        return Go("첫 스킬 설정하기",()=>game.ContinueTutorial());
                    }
                    lines.Add(new StoryLine(H,"…사방이 막혔다. 무기를 쥔 손에 힘이 들어가지 않는다.",()=>game.World.Frame(EnemyCenter(r),.78f)));
                    lines.Add(new StoryLine(H,"여기까지인가. 이름도 남기지 못하고, 이 어둠 속에서 끝나는 건가…",()=>game.World.Frame(null,.66f)));
                    return Go("고개를 든다",()=>cinematic.StartCoroutine(DescentBeat()));
                case ProloguePhase.Gatekeeper:
                    lines.Add(new StoryLine(H,"해냈다… 정말로 살아남았어."));
                    lines.Add(new StoryLine(V,"아직 기뻐하지 말라. 길 끝에 수문장이 버티고 있다."));
                    lines.Add(new StoryLine(V,"칙령은 네 곁에 있다. 두려워 말고 나아가라."));
                    return Go("수문장에게로",()=>{if(game.ContinueTutorial())cinematic.Notice("새 목표","길 끝의 수문장을 처치하세요",false);});
                case ProloguePhase.SurvivalLesson:
                    lines.Add(new StoryLine(H,"크윽… 이대로는…!"));
                    lines.Add(new StoryLine(V,"멈추라. 내가 이 순간을 붙들었노라."));
                    lines.Add(new StoryLine(V,"힘만으로는 수문장을 넘지 못한다. 살아남는 법을 칙령에 새기라."));
                    lines.Add(new StoryLine(V,r.tutorialFlowVersion==3?"물약 HP 기준을 40%에서 60%로 바꾸어, 더 일찍 회복하게 하세요.":"물약을 언제 마실지, 어떤 공격을 피할지. 칙령이 정하면 네 몸이 따르리라."));
                    return Go("사냥 칙령을 펼친다",()=>{interventionTold=true;cinematic.Notice("새 목표",r.tutorialFlowVersion==3?"물약 HP 기준을 60%로 바꾸세요":"자동 물약과 회피 설정을 바꾸세요",false);});
                case ProloguePhase.Cleared:
                    lines.Add(new StoryLine(V,"보았느냐. 칙령을 따른 자는 쓰러지지 않는다."));
                    lines.Add(new StoryLine(V,"수고의 값으로 금화 1,000을 내리노라. 성소 마을로 가라. 그곳에서 너의 사냥이 시작되리라."));
                    lines.Add(new StoryLine(H,"…당신의 명을 따르겠습니다."));
                    return Go("보상을 받고 마을로",()=>cinematic.StartCoroutine(ToTown()));
                default:return null;
            }
        }
        static Vector2? EnemyCenter(RunState r)
        {var alive=r.enemies.Where(e=>!e.dead&&!e.boss).ToList();return alive.Count==0?(Vector2?)null:alive.Aggregate(Vector2.zero,(s,e)=>s+e.position)/alive.Count;}
        void ReleaseStaging(){game.World.Frame(null);cinematic.Letterbox(false);}
        IEnumerator IntroBeat()
        {
            cinematic.Begin();cinematic.Letterbox(true,true);cinematic.FadeTo(1,0);game.World.StagedHeroAction=ActorAction.Despair;game.World.StagedHeroProgress=1;
            game.World.Frame(null,.56f,snap:true);game.Audio?.Play("flow.tutorial");
            yield return null;cinematic.FadeTo(0,1.8f);game.World.Frame(null,.66f);
            yield return cinematic.Wait(.6f);
            cinematic.Card(CinematicCard.Chapter,"프롤로그","칙령의 목소리","포위된 자에게 명이 내려진다",2.2f);
            yield return cinematic.Wait(3f);cinematic.HideCard();cinematic.FadeTo(0,.2f);
            yield return cinematic.Wait(.4f);cinematic.End();ShowTutorialPrompt();
        }
        // Light pours from above and the voice speaks for the first time.
        IEnumerator DescentBeat()
        {
            var r=game.Combat.State;cinematic.Begin();game.World.DivineLight(true);game.Audio?.Play("field.shrine_use");
            cinematic.Vignette(UiTheme.GoldBright,.28f);game.World.Lighting?.Shake(.35f,.4f);game.World.Frame(null,.72f);
            yield return cinematic.Wait(1.4f);cinematic.End();
            var lines=new List<StoryLine>{
                new StoryLine(V,"고개를 들라, 필멸의 자여. 네 절망이 하늘에 닿았노라."),
                new StoryLine(V,"내가 너에게 명하는 대로 따르라. 그리하면 이곳에서 반드시 살아 나가리라."),
                new StoryLine(HeroSpeaker,"…누구십니까? 이 눈부신 빛은…"),
                new StoryLine(V,"묻지 말고 받으라. 이는 사냥 칙령. 네 손과 걸음을 이끌 나의 뜻이니라.")};
            if(!game.TutorialActive||game.Combat.State!=r)yield break;
            OpenPrologueDialogue(lines,Go("두루마리를 받든다",()=>cinematic.StartCoroutine(ScrollBeat())));
        }
        // The scroll descends, the hero raises it to the sky, and the edict's emblem flies to its place in the menu.
        IEnumerator ScrollBeat()
        {
            var r=game.Combat.State;cinematic.Begin();var world=game.World;
            for(float t=0;t<2.4f;t+=Time.unscaledDeltaTime)
            {
                float p=Mathf.Clamp01(t/2.4f),ease=1-(1-p)*(1-p);world.PlaceScroll(Mathf.Lerp(10,3.1f,ease));
                if(p>.45f){world.StagedHeroAction=ActorAction.Offer;world.StagedHeroProgress=Mathf.Clamp01((p-.45f)/.45f);}
                if(cinematic.Skipping)break;yield return null;
            }
            world.StagedHeroAction=ActorAction.Offer;world.StagedHeroProgress=1;world.PlaceScroll(3.1f);game.Audio?.Play("item.pick.paper");
            yield return cinematic.Wait(.5f);
            world.FlareScroll();game.Audio?.Play("flow.skill_unlock");cinematic.Vignette(UiTheme.GoldBright,.4f);
            yield return cinematic.Wait(.9f);cinematic.Vignette(UiTheme.GoldBright,.12f);
            EnsureGate();var dock=globalHud?.EdictButton;
            if(dock!=null&&game.TutorialActive&&game.Combat.State==r)
            {
                var corners=new Vector3[4];dock.GetWorldCorners(corners);var to=(Vector2)(corners[0]+corners[2])/2;
                bool landed=false;gate.Fly(world.ScrollScreenPoint,to,1.3f,()=>landed=true);
                while(!landed)yield return null;
            }
            world.RemoveScroll();world.StagedHeroAction=null;cinematic.Vignette(Color.black,0);cinematic.End();ReleaseStaging();
            game.Audio?.Play("ui.confirm");
            if(game.TutorialActive&&game.Combat.State==r)game.ContinueTutorial();
        }
        IEnumerator ClearedPackBeat()
        {
            cinematic.Begin();cinematic.Notice("목표 완료","포위를 뚫었다",true);game.Audio?.Play("flow.objective");game.World.DivineLight(false);
            yield return cinematic.Wait(1.2f);cinematic.End();ShowTutorialPrompt();
        }
        // The hero's life falls below half: time stops, the light returns, and the voice steps in.
        IEnumerator InterventionBeat()
        {
            var r=game.Combat.State;cinematic.Begin();cinematic.Letterbox(true);cinematic.Vignette(StorageSurface.Hex("5a0a0c"),.4f);game.Audio?.Play("flow.defeat");
            game.World.Frame(null,.72f);
            yield return cinematic.Wait(1f);
            game.World.DivineLight(true);game.Audio?.Play("field.shrine_use");cinematic.Vignette(UiTheme.GoldBright,.24f);
            yield return cinematic.Wait(1.1f);cinematic.Vignette(Color.black,0);cinematic.End();ReleaseStaging();
            if(game.TutorialActive&&game.Combat.State==r)ShowTutorialPrompt();
        }
        IEnumerator DefeatBeat()
        {
            cinematic.Begin();cinematic.Vignette(StorageSurface.Hex("5a0a0c"),.45f);game.Audio?.Play("flow.defeat");
            cinematic.Card(CinematicCard.Defeat,"","쓰러졌다","",1.4f);
            yield return cinematic.Wait(2.1f);cinematic.HideCard();cinematic.End();ShowTutorialPrompt();
        }
        IEnumerator VictoryBeat()
        {
            var r=game.Combat.State;cinematic.Begin();cinematic.Letterbox(true);cinematic.Vignette(UiTheme.Gold,.16f);game.World.Frame(null,.8f);game.Audio?.Play("flow.victory");
            game.World.DivineLight(true);
            if(PitRun(r))cinematic.Card(CinematicCard.Victory,"시험 통과","최종 시험","수습의 이름을 벗고 정식 전사가 되었다",2.2f);
            else cinematic.Card(CinematicCard.Victory,"승리","수문장 처치","칙령을 따른 자가 살아남았다",2.2f);
            yield return cinematic.Wait(3f);cinematic.HideCard();
            cinematic.Notice("보상","금화 1,000",true);game.Audio?.Play("gold.gain");
            yield return cinematic.Wait(.9f);cinematic.Vignette(UiTheme.Gold,0);cinematic.End();ShowTutorialPrompt();
        }
        // The gatekeeper steps into view: frame it, let it roar, name it, then hand the fight back.
        IEnumerator BossBeat(EnemyState boss)
        {
            var r=game.Combat.State;r.paused=true;cinematic.Begin();cinematic.Letterbox(true);
            game.World.Frame(Vector2.Lerp(r.position,boss.position,.55f),.85f);
            yield return cinematic.Wait(.7f);
            game.Audio?.Play("boss.summoned");game.World.Lighting?.Shake(1.2f,.7f);cinematic.Vignette(StorageSurface.Hex("7a1010"),.2f);
            cinematic.Card(CinematicCard.Boss,"보스",GameCatalog.BossName(boss.pattern),"성소로 가는 길의 수문장",2f);
            yield return cinematic.Wait(.5f);game.Audio?.Play("boss.enrage");
            yield return cinematic.Wait(2.2f);cinematic.HideCard();cinematic.Vignette(Color.black,0);ReleaseStaging();
            yield return cinematic.Wait(.5f);cinematic.End();
            if(game.TutorialActive&&game.Combat.State==r)ContentWindowHost.AcceptPauseState(this,r,false);
            cinematic.Bark(VoiceProfile,"수문장이다. 칙령이 정한 대로 싸우라. 쓰러지게 두지는 않으리라.");
        }
        // Voice lines and the gatekeeper's entrance while the fight runs; nothing here changes the run except the entrance pause.
        void StageTutorialCombat(RunState r)
        {
            if(PitRun(r)){StagePitCombat(r);return;}
            if(r.paused||cinematic.Busy||tutorialJournal!=null||CommonPanelOpen)return;
            if(r.tutorialPhase==ProloguePhase.Encircled&&!killBark&&r.enemies.Any(e=>e.dead&&!e.boss)){killBark=true;cinematic.Bark(VoiceProfile,"보았느냐. 칙령이 네 몸을 이끄는 한, 너는 쓰러지지 않는다.");}
            var boss=r.tutorialPhase==ProloguePhase.Gatekeeper||r.tutorialPhase==ProloguePhase.Showdown?r.enemies.Find(e=>e.boss&&!e.dead):null;if(boss==null)return;
            // WorldView draws an enemy model within 12 m in line of sight; enter only once the gatekeeper is on screen.
            if(!bossIntroShown&&Vector2.Distance(r.position,boss.position)<=11&&game.World.CanDisplayEnemyMarker(r,boss.position)){bossIntroShown=true;cinematic.StopAllCoroutines();cinematic.StartCoroutine(BossBeat(boss));return;}
            if(r.tutorialPhase==ProloguePhase.Showdown&&!bossBark&&boss.health<=boss.maxHealth*.3f){bossBark=true;cinematic.Bark(VoiceProfile,"수문장이 흔들린다! 끝을 맺으라!");}
        }
        // ---------------------------------------------------------------- forced Hunt Edict lessons
        // Each frame the lesson reads where the player is and names the one control that moves it forward.
        void TickPrologueLesson()
        {
            if(GuideActive){TickGuideLesson();return;}
            var run=game.TutorialActive?game.Combat.State:null;
            bool lesson=run!=null&&PrologueEdictOpen&&tutorialJournal==null&&!lessonClosing&&Page=="battle";
            if(!lesson){if(!RuneIntroductionActive&&!RiftGraphLessonActive&&!RiftEntryTourActive&&!ForgeIntroductionActive&&!townGateOn&&gate!=null&&gate.Active)gate.Hide();ClearPrologueRing();return;}
            EnsureGate();
            var (name,line)=PitRun(run)?run.tutorialPhase==ProloguePhase.MovementLesson?PitMovementStep():PitSkillLessonStep():
                run.tutorialPhase==ProloguePhase.EdictLesson?SkillLessonStep():SurvivalLessonStep();
            // An empty name means a dialogue has just taken the step over.
            if(name==""){gate.Hide();ClearPrologueRing();return;}
            if(name==null)
            {
                gate.Hide();ClearPrologueRing();
                FinishLesson(run);
                return;
            }
            var target=LessonTarget(name);gate.Show(target,PitRun(run)?C:V,line);
            if(target==null){ClearPrologueRing();return;}
            if(prologueRing!=null&&prologueRing.transform.parent==target)return;
            ClearPrologueRing();var rect=UiLayout.Rect("Prologue focus",target);UiLayout.Stretch(rect);prologueRing=rect.gameObject.AddComponent<TutorialAnchorRing>();
            prologueRing.FillInterior=name!="starter-observation-area"&&name!="starter-selection-area"&&name!="prologue-position-choices";
        }
        void ClearPrologueRing(){if(prologueRing!=null)Destroy(prologueRing.gameObject);prologueRing=null;}
        RectTransform LessonTarget(string name)
        {
            if(name=="menu-hunt-edict")return globalHud?.EdictButton;
            if(name=="prologue-potion")return globalHud?.PotionButton;
            if(name=="prologue-skill"||name=="prologue-skill-rank")
            {
                if(edictSkillTree==null)return null;
                string skill=Tutorials.PrologueSkill(PrologueHero.heroClass).skill;
                int index=Array.FindIndex(SkillTreeD4.For(PrologueHero.heroClass).actives,a=>a.id==skill);
                var selection=SkillTreeD4Selection.Active(index);
                return edictSkillTree.GuideTarget(selection,name=="prologue-skill-rank"?(bool?)true:null);
            }
            if(name=="prologue-tree-close")return edictSkillTree?.GuideCloseTarget();
            if(name=="prologue-position-choices")return Named(sheetCard,"edict-position-choices");
            var target=Named(quickRoot,name);
            if(target!=null&&UiVisibility.Fraction(target,UiSafeArea.Current)<.99f)UiVisibility.EnsureVisible(target);
            return target;
        }
        (string,string) SkillLessonStep()
        {
            var hero=PrologueHero;string skill=Tutorials.PrologueSkill(hero.heroClass).skill;
            bool equipped=hero.build.classSkills?.actives[0]==skill;
            if(edictSkillTree==null)return equipped?(null,null):("menu-hunt-edict",Loc.T("사냥 칙령 버튼을 눌러 주세요."));
            int index=Array.FindIndex(SkillTreeD4.For(hero.heroClass).actives,a=>a.id==skill);
            var selection=SkillTreeD4Selection.Active(index);
            if(hero.build.classSkills.Rank(skill)<=0)
                return edictSkillTree.Session.Selection.Equals(selection)?("prologue-skill-rank",Loc.F("{0}을(를) 선택하고 1포인트를 투자해 주세요.",ClassSkillTree.Name(skill))):("prologue-skill",Loc.T("첫 스킬을 눌러 효과를 확인하세요."));
            if(edictSkillTree.Slots?.Slots()[0]!=skill)
            {
                if(edictSkillTree.Slots?.PressedSlot!=0)edictSkillTree.PressSlot(0);
                return("prologue-skill",Loc.T("첫 스킬을 1번 슬롯에 장착하세요."));
            }
            return("prologue-tree-close",Loc.T("선택한 방식이 저장되었습니다. 창을 닫고 사냥을 이어가세요."));
        }
        (string,string) SurvivalLessonStep()
        {
            bool saved=HuntEdictSummary.Value(PrologueHero.edict,"survival.potionHpPercent")=="60";
            if(quickRoot==null||quickKind!="potion")return saved?(null,null):("prologue-potion",Loc.T("왼쪽 아래 물약 설정 버튼을 눌러 주세요."));
            var slider=Named(quickRoot,"edict-quick-survival.potionHpPercent")?.GetComponent<Slider>();
            return slider!=null&&Mathf.RoundToInt(slider.value)==60?("edict-quick-potion-apply",Loc.T("60%를 적용하세요.")):
                ("edict-quick-survival.potionHpPercent",Loc.T("물약 HP 기준에 60을 입력하세요."));
        }
        // The lesson's real change is saved and the window is closed: a closing line, then the fight resumes.
        void FinishLesson(RunState run)
        {
            if(PitRun(run)){FinishPitLesson(run);return;}
            lessonClosing=true;var lines=new List<StoryLine>();StoryChoice[] choices;
            if(run.tutorialPhase==ProloguePhase.EdictLesson)
            {
                lines.Add(new StoryLine(V,"칙령이 네 몸에 새겨졌다. 이제 네 손은 망설이지 않으리라."));
                lines.Add(new StoryLine(V,"가라. 포위를 뚫고 살아남으라."));
                choices=Go("포위를 뚫는다",()=>
                {
                    lessonClosing=false;game.World.DivineLight(false);
                    if(game.ContinueTutorial())cinematic.Notice("새 목표","포위한 마물을 쓰러뜨리세요",false);
                });
            }
            else
            {
                lines.Add(new StoryLine(V,"되었다. 시간이 다시 흐른다."));
                lines.Add(new StoryLine(V,"이번에는 쓰러지지 않으리라. 수문장을 쓰러뜨려라."));
                choices=Go("다시 싸운다",()=>{lessonClosing=false;cinematic.Vignette(Color.black,0);game.World.DivineLight(false);game.ContinueTutorial();});
            }
            OpenPrologueDialogue(lines,choices);
        }
        IEnumerator ToTown(bool chief=false)
        {
            cinematic.Begin();cinematic.FadeTo(1,.7f);
            // Not skippable: the fade hides the dungeon being replaced by the town.
            for(float end=Time.unscaledTime+.75f;Time.unscaledTime<end;)yield return null;
            game.FinishTutorial();
            if(game.TutorialActive){cinematic.FadeTo(0,.3f);cinematic.End();yield break;}
            if(Page=="characters"){ReleaseStaging();cinematic.Clear();yield break;}
            yield return ArriveInTown(chief);
        }
        public void ShowGuideTownArrival()=>cinematic.StartCoroutine(ArriveInTown(true));
        IEnumerator ArriveInTown(bool chief)
        {
            cinematic.Begin();cinematic.FadeTo(1,0);
            ReleaseStaging();cinematic.FadeTo(0,1.4f);
            cinematic.Card(CinematicCard.Location,"잿빛 숲","정착민 마을","",2.4f);
            for(float end=Time.unscaledTime+3.1f;Time.unscaledTime<end;)yield return null;
            cinematic.End();
            if(Page!="plaza"||tutorialJournal!=null||CommonPanelOpen)yield break;
            // The guide stage opens everything and hides the hints on graduation, so the chief's welcome does not wait for them.
            if(chief)ShowChiefArrival();else if(!game.Store.Data.guide.hintsHidden)ShowArrivalDialogue();
        }
        // The town's first event: the rift keeper saw the light fall and meets the hero it chose.
        void ShowArrivalDialogue()
        {
            const string G=TutorialGuide;CloseTutorialJournal();
            var lines=new[]{
                new StoryLine(G,"어서 오게, 정식 전사여. 정착민 마을에 온 걸 환영하네."),
                new StoryLine(G,"나는 성소의 균열지기, 안톤 진다크라 하네. 신의 계시를 받고 자네를 맞으러 나왔지. 최종 시험을 통과했다는 소식은 벌써 마을까지 퍼졌다네."),
                new StoryLine(G,"이곳도 한때는 조용한 마을이었네. 하지만 균열이 열리고 그 틈에서 악마들이 뛰쳐나오면서, 세상은 순식간에 위기에 빠졌지."),
                new StoryLine(G,"그때부터 전해지는 예언이 있네. 신의 계시를 받은 자가 나타나 균열의 악마들을 처단하리라."),
                new StoryLine(G,"그리고 계시를 받은 건 자네만이 아니네. 신의 뜻을 받은 여러 전사들이 이곳에 모여 함께 싸우고 있지."),
                new StoryLine(G,"그들이 모여 살다 보니 어느새 집성촌이 되었고, 전사들을 위한 대장장이와 상인, 보석상 같은 이들도 하나둘 자리를 잡았다네."),
                new StoryLine(G,"균열은 아직 가장 얕은 곳만 열려 있네. 그 깊은 곳을 본 자는 아직 많지 않지."),
                new StoryLine(G,"서두를 것 없네. 마을을 한 바퀴 돌아보게나. 채비가 끝나면 균열 앞으로 오게. 첫 문은 내가 열어 주지.")};
            var choices=new[]{new StoryChoice("tutorial-arrival-go","균열지기에게 가기",()=>{CloseTutorialJournal();game.RequestStation(TownStation.RiftKeeper);},true),
                new StoryChoice("tutorial-arrival-close","둘러보기",CloseTutorialJournal)};
            tutorialJournal=StoryDialogueWindow.Open(transform,"정착민 마을",EquipmentViewSource.Catalog,()=>InterfaceFactor,new StoryDialogueState(lines,PrologueHero.gender),()=>choices,()=>tutorialJournal=null,"균열지기",back:CloseTutorialJournal,listener:HeroPortrait);
        }
        // First use of a content: the resident who runs it explains it in the dialogue box. Read, later and practice keep
        // the journal's meanings, and the journal stays the place to browse every guide.
        public void ShowTutorialBriefing(string id)
        {
            if(id==ContentUnlocks.Rune&&!ContentUnlocks.Has(game.Store.Data,id)){ShowToast(ContentUnlocks.LockedToast(id),4,true);return;}
            var d=Tutorials.Definition(id);var npc=d!=null?NpcProfiles.ForStation(TutorialPractice.Station(id)):null;
            if(npc==null){ShowTutorialJournal(id);return;}
            var a=game.Store.Data;CloseTutorialJournal();
            var lines=new List<StoryLine>{new StoryLine(npc.id,d.bodyKo)};
            if(d.supported&&!a.guide.supportUsed.Contains(id))lines.Add(new StoryLine(npc.id,"최초 실습 지원 1회가 남아 있습니다. 실행과 저장이 성공할 때만 사용됩니다."));
            var choices=new[]{
                new StoryChoice("tutorial-practice","실습",()=>StartTutorialAction(d),true,Tutorials.Available(a,d)),
                new StoryChoice("tutorial-read","읽음",()=>{game.Store.SetTutorialPreference(id,"read");CloseTutorialJournal();}),
                new StoryChoice("tutorial-defer","나중에",()=>{if(game.Store.SetTutorialPreference(id,"defer")){tutorialFocus="";pendingTutorial="";}CloseTutorialJournal();})};
            tutorialJournal=StoryDialogueWindow.Open(transform,d.ko,EquipmentViewSource.Catalog,()=>InterfaceFactor,new StoryDialogueState(lines,PrologueHero.gender),()=>choices,()=>tutorialJournal=null,d.ko,back:CloseTutorialJournal,listener:HeroPortrait);
        }
    }
}
