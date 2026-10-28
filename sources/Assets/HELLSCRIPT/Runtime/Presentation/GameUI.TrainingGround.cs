using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Hellscript
{
    // Training ground adapters on GameUI's existing canvas: the lobby and result windows, the edict shortcuts, and the battle
    // HUD's live DPS panel (also used by rifts) with its training pause dialog. Runs stay in GameController and CombatSimulation.
    public sealed partial class GameUI
    {
        TrainingGroundWindow trainingLobby;TrainingGroundResultWindow trainingResult;
        DpsHudDrag trainingDrag;
        RectTransform trainingHud;Text trainingDps,trainingDelta,trainingStats;TrainingChartView trainingChart;Button trainingPause,trainingFold;
        int trainingHudSecond=-1;bool trainingHudPaused;
        float[] trainingPreviousDamage,trainingPreviousLine;
        // The live graph can be folded away to watch the fight; the choice lasts for this session only.
        bool trainingFolded;
        public TrainingGroundWindow TrainingLobby=>trainingLobby;
        public TrainingGroundResultWindow TrainingResult=>trainingResult;
        public void ShowTraining()
        {
            if(!RequireContent(ContentUnlocks.Train))return;
            if(game.Active){ShowToast(Loc.T("진행 중인 전투를 먼저 마쳐 주세요."));return;}
            if(trainingLobby!=null)return;
            trainingLobby=TrainingGroundWindow.Open(transform,game,()=>InterfaceFactor,()=>{trainingLobby=null;RefreshHud();RefreshPlaza();});
        }
        public void CloseTrainingGround(){trainingLobby?.Close();CloseTrainingResult();}
        void CloseTrainingResult(){var current=trainingResult;trainingResult=null;current?.Close();}
        void ShowTrainingGroundResult()
        {
            if(trainingResult!=null)return;
            pageRepaint=ShowResult;Base("result","훈련 결과","",responsive:true);
            game.RecordGuide(()=>FirstPlayGuide.ReadResult(game.Store.Data,game.Combat.State));
            trainingResult=TrainingGroundResultWindow.Open(transform,game,()=>InterfaceFactor);
        }
        // A skill's edict opens on that skill's policy page; without a skill, on the skills tab.
        public void OpenTrainingEdict(string skill)
        {
            ShowEdictSkillTree();
            if(edictSkillTree!=null&&!string.IsNullOrEmpty(skill))
            {int index=Array.FindIndex(SkillTreeD4.For(game.Store.Data.Hero.heroClass).actives,a=>a.id==skill);if(index>=0)edictSkillTree.Select(SkillTreeD4Selection.Active(index));}
        }
        // Mid-run edits would leave the run unrecordable, so the edict and behaviour editors wait for the result screen.
        bool BlockTrainingEdit()
        {
            if(!game.Active||!game.TrainingGroundRun)return false;
            ShowToast(Loc.T("훈련 중에는 사냥 칙령을 바꿀 수 없습니다. 일시정지에서 훈련을 중단하거나 결과 화면에서 수정해 주세요."));return true;
        }

        // ---------------------------------------------------------------- battle HUD
        void BuildTrainingHud(RunState run)
        {
            // Capture the current run's presentation without changing the next non-training HUD's scope.
            trainingSkin=game.TrainingGroundRun?TrainingGroundSkin.Shared:CombatSkin;
            trainingHud=CombatHudPanel("Training DPS panel",root,trainingSkin);
            trainingDps=Label(trainingHud,"",20,gold);trainingDelta=Label(trainingHud,"",12,pale);trainingStats=Label(trainingHud,"",11,muted);
            CombatHudText(trainingDps,trainingSkin);CombatHudText(trainingDelta,trainingSkin);CombatHudText(trainingStats,trainingSkin);
            trainingDps.name="dps-drag-handle";trainingDps.raycastTarget=true;
            trainingDrag=trainingDps.gameObject.AddComponent<DpsHudDrag>();
            trainingDrag.Bind(trainingHud,game.DpsPosition,GraphPositionFailed,()=>BattleHudTopOnPage-8);
            trainingChart=TrainingChartView.Create(trainingHud,"Training DPS chart",InterfaceFactor,false,skin:trainingSkin);
            trainingFold=UiIconButton.CreateGlyph(trainingHud,"training-dps-fold","chevron",0,0,30,30,ToggleTrainingFold,flip:!trainingFolded);CombatHudButton(trainingFold,trainingSkin);
            trainingPause=null;
            if(game.TrainingGroundRun){trainingPause=Button(header,"||",ToggleTrainingPause,Color.clear);trainingPause.name="training-pause";}
            var previous=game.TrainingGroundBefore?.previous;
            trainingPreviousDamage=game.TrainingGroundRun?(previous?.Recorded==true?previous.damagePerSecond.Select(v=>(float)v).ToArray():null):run.tutorial?null:RiftResult.PreviousDps(game.Store.Data,run)?.damage.ToArray();
            trainingPreviousLine=trainingPreviousDamage==null?null:Enumerable.Range(1,trainingPreviousDamage.Length).Select(s=>TrainingGround.WindowDps(trainingPreviousDamage,s)).ToArray();
            trainingHudSecond=-1;BuildCasualTrainingChrome();
        }
        // Default placement stays beside the map and below the fixed boss band; a dragged position takes precedence.
        void PlaceTrainingHud(float w,bool narrow,Rect medals,Rect map)
        {
            if(trainingHud==null)return;if(CasualCombatHud){PlaceCasualTrainingHud();return;}float k=InterfaceFactor;bool wide=w>=720,folded=trainingFolded;
            if(trainingPause!=null)Place((RectTransform)trainingPause.transform,medals.width>0?medals.xMin-104:w-160,8,44,44);
            float b=Mathf.Round(30*k),bx=8+b+8;
            trainingChart.gameObject.SetActive(!folded);trainingDelta.gameObject.SetActive(!folded);trainingStats.gameObject.SetActive(wide&&!folded);
            // Row one is the fold button and the live number, row two the comparison across the panel's width.
            if(wide)
            {
                float pw=Mathf.Min(320,w*.3f),ph=folded?b+12:172*k;if(folded)pw=Mathf.Min(pw,bx+150*k);
                // Keep the shared panel beside the rift map; never cover the map's input or counters.
                float right=map.width>0?map.xMin-8:w-12;
                Place(trainingHud,right-pw,124,pw,ph);Place((RectTransform)trainingFold.transform,8,6,b,b);
                Place(trainingDps.rectTransform,bx,6,pw-bx-8,b);
                if(!folded)
                {
                    float row=6+b+2;Place(trainingDelta.rectTransform,10,row,pw-20,18*k);
                    float chart=row+18*k+4;Place((RectTransform)trainingChart.transform,10,chart,pw-20,ph-chart-(28*k));Place(trainingStats.rectTransform,10,ph-24*k,pw-20,18*k);
                }
            }
            else
            {
                float pw=w-24,ph=folded?b+12:(b+8+18*k+8),top=Mathf.Max((narrow?154:116)+6,medals.height>0?medals.yMax+6:0,map.height>0?map.yMax+6:0);Place(trainingHud,12,top,pw,ph);Place((RectTransform)trainingFold.transform,8,6,b,b);
                Place(trainingDps.rectTransform,bx,6,(folded?pw:pw*.5f)-bx-8,b);
                if(!folded)
                {
                    Place(trainingDelta.rectTransform,10,6+b+2,pw*.5f-16,18*k);
                    Place((RectTransform)trainingChart.transform,pw*.5f+6,6,pw*.5f-16,ph-12);
                }
            }
            if(bossHud!=null&&bossHud.gameObject.activeSelf)
            {
                float x=trainingHud.anchoredPosition.x,bx0=bossHud.anchoredPosition.x,bottom=-bossHud.anchoredPosition.y+bossHud.rect.height+8;
                if(x<bx0+bossHud.rect.width&&x+trainingHud.rect.width>bx0&&-trainingHud.anchoredPosition.y<bottom)
                    trainingHud.anchoredPosition=new Vector2(x,-bottom);
            }
            trainingDrag.Restore();
            trainingDps.fontSize=Mathf.RoundToInt(18*k);trainingDelta.fontSize=Mathf.RoundToInt(11*k);trainingStats.fontSize=Mathf.RoundToInt(10*k);
        }
        void ToggleTrainingFold()
        {
            trainingFolded=!trainingFolded;
            var mark=trainingFold.transform.Find("Icon");if(mark!=null)mark.localRotation=Quaternion.Euler(0,0,trainingFolded?0:180);
            battleSize=Vector2.zero;ReflowBattleHud();
        }
        void RefreshTrainingHud(RunState run)
        {
            if(trainingHud==null)return;
            if(game.TrainingGroundRun)
            {
                var setup=run.trainingGround.setup;int all=setup.enemies.Sum(e=>e.count),left=all-run.enemies.Count(e=>e.dead&&!e.add);
                meterText.text=Loc.F("남은 적 {0}/{1}",left,all);timerText.text=TrainingGroundResultWindow.Clock(run.time);
                trainingPause.GetComponentInChildren<Text>().text=run.paused?"▶":"||";
            }
            int second=Mathf.FloorToInt(run.time);if(second==trainingHudSecond&&run.paused==trainingHudPaused)return;trainingHudSecond=second;trainingHudPaused=run.paused;
            RefreshDamageGraphs(run);
            var timeline=game.TrainingGroundRun?run.trainingGround:run.dps;
            if(timeline==null)
            {
                trainingDps.text=Loc.F("실시간 DPS {0:N0}","—");trainingDelta.text=Loc.T("기록 없음");trainingStats.text="";
                trainingChart.Set(null,null,30,null);
                if(CasualCombatHud){trainingDps.text="—";foreach(var value in casualDpsValues)value.text="—";casualChartTop.text="—";foreach(var time in casualChartTimes)time.text="";}
                return;
            }
            var series=timeline.damage;bool compare=trainingPreviousDamage!=null;
            float live=TrainingGround.WindowDps(series,second);
            trainingDps.text=Loc.F("실시간 DPS {0:N0}",live);
            if(!compare)trainingDelta.text=run.tutorial?"":Loc.T("비교할 직전 판 없음");
            else
            {
                float before=TrainingGround.WindowDps(trainingPreviousDamage,second);
                trainingDelta.text=before>0?Loc.F("직전 판 같은 시점 대비 {0}",TrainingGroundResultWindow.Percent(live/before-1)):Loc.T("최근 3초 평균");
            }
            double total=0;foreach(float v in series)total+=v;
            var current=Enumerable.Range(1,Mathf.Max(0,second)).Select(s=>TrainingGround.WindowDps(series,s)).ToList();
            float peak=0;for(int s=1;s<=Mathf.Min(second,series.Count);s++)peak=Mathf.Max(peak,current[s-1]);
            trainingStats.text=Loc.F("평균 {0:N0} · 최고 {1:N0} · 총 피해 {2:N0}",run.time>0?total/run.time:0,peak,total);
            trainingChart.Set(current,trainingPreviousLine,Mathf.Max(30,Mathf.Max(current.Count,trainingPreviousLine?.Length??0)),timeline.casts);
            RefreshCasualTrainingValues(live,total,peak,run.time);
        }
        void ToggleTrainingPause()
        {
            if(!game.Active||!game.TrainingGroundRun)return;
            if(game.Combat.State.paused){game.TogglePause();return;}
            game.TogglePause();ShowTrainingPause();
        }
        void ShowTrainingPause()
        {
            var skin=game.TrainingGroundRun?trainingSkin:null;
            ShowHudPanel("일시정지",body=>
            {
                var note=Note(body,"전투 시간이 멈춰 있습니다. 멈춘 시간은 클리어 시간에 넣지 않습니다.",16,64,pale);
                CombatHudText(note,skin);
                var resume=BigButton(body,"계속하기",()=>{CloseHudPanel();if(game.Active&&game.Combat.State.paused)game.TogglePause();},true);resume.name="training-resume";
                var stop=BigButton(body,"훈련 중단",()=>{CloseHudPanel();ShowToast(Loc.T("훈련을 중단했습니다. 중단한 판은 기록하지 않습니다."));game.LeaveTrainingGround();});stop.name="training-stop";
            });
        }
        // Escape pauses a training and opens the pause dialog; with the dialog closed, it resumes. Windows handle it first.
        void UpdateTrainingKeys()
        {
            if(!game.Active||!game.TrainingGroundRun||Page!="battle"||Keyboard.current?.escapeKey.wasPressedThisFrame!=true||ContentWindowHost.EscapeHandledThisFrame||hudPanel!=null)return;
            ToggleTrainingPause();
        }
    }
}
