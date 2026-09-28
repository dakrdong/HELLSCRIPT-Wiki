#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed class RuntimeEnemySmoke : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if(!Environment.GetCommandLineArgs().Contains("-hellscriptEnemySmoke"))return;
            Application.logMessageReceived+=(message,stack,type)=>{if(type==LogType.Exception)Application.Quit(1);};new GameObject("Enemy combat smoke").AddComponent<RuntimeEnemySmoke>();
        }
        static void Require(bool pass,string why){if(!pass)throw new InvalidOperationException(why);}
        static IEnumerator Until(Func<bool> ready,float timeout=10)
        {float end=Time.realtimeSinceStartup+timeout;while(!ready()&&Time.realtimeSinceStartup<end)yield return null;Require(ready(),"Enemy runtime scenario did not reach its expected state.");}
        static IEnumerator Capture(GameController game,string directory,string name)
        {game.Combat.State.paused=true;game.UI.RefreshHud();yield return new WaitForSecondsRealtime(.35f);ScreenCapture.CaptureScreenshot(Path.Combine(directory,name));yield return new WaitForSecondsRealtime(.2f);}
        static RunState Arena(GameController game,int[] kinds,Vector2[] offsets)
        {
            // A fresh smoke account would otherwise be sent into the mandatory tutorial instead of the arena.
            if(game.Running)game.ReturnTown();var account=game.Store.Data;account.guide.legacyExempt=true;account.selectedHero=0;var hero=account.Hero;hero.level=30;hero.inventory.Clear();uint rng=41923;
            // The store rejects illegal loadouts (an off-hand base in the main hand), so reroll until each slot is legal.
            for(int slot=0;slot<8;slot++)for(int roll=0;roll<20;roll++)
            {var item=ItemGenerator.Create(HeroClass.Warrior,slot,2,30,ref rng);item.equipped=true;if(!EquipmentSlots.Valid(hero.inventory.Where(i=>i.equipped).Append(item),HeroClass.Warrior))continue;hero.inventory.Add(item);break;}
            hero.build=GameCatalog.Preset(HeroClass.Warrior,0);hero.build.rules.Clear();hero.build.activeSkills=new[]{2,4}.ToList();hero.build.passives=Array.Empty<int>();hero.build.movement=MovementMode.Stand;hero.build.autoRepeat=false;hero.build.potionThreshold=0;
            var fixture=new CombatSimulation(account,game.catalog,1,1,seed:73121);var run=fixture.State;run.training=-1;run.position=RiftMap.Rooms[0];run.enemies.Clear();
            for(int i=0;i<kinds.Length;i++)run.enemies.Add(new EnemyState{id=900+i,kind=kinds[i],position=run.position+offsets[i],health=100000,maxHealth=100000,attack=5,speed=0,cooldown=1000});
            account.suspendedRun=run;game.Begin(resume:true);Require(game.Running,"Enemy arena did not start: "+game.Notice+" "+game.Store.Error);game.SetSpeed(1);game.Combat.State.paused=true;return game.Combat.State;
        }
        static void Checkpoint(GameController game,string directory,string name)
        {game.Save();Require(string.IsNullOrEmpty(game.Store.Error),"Enemy checkpoint save failed.");File.WriteAllText(Path.Combine(directory,name),JsonUtility.ToJson(game.Combat.State,true));}
        static RunState Resume(GameController game,string directory,string name)
        {
            var saved=JsonUtility.FromJson<RunState>(File.ReadAllText(Path.Combine(directory,name)));GameStore.NormalizeRun(saved);game.Begin(resume:true);var run=game.Combat.State;run.paused=true;
            Require(run.id==saved.id&&run.time==saved.time&&run.rng==saved.rng&&run.health==saved.health&&run.resource==saved.resource
                &&run.enemies.Count==saved.enemies.Count&&!run.enemies.Where((e,i)=>JsonUtility.ToJson(e)!=JsonUtility.ToJson(saved.enemies[i])).Any()
                &&run.enemyHazards.Count==saved.enemyHazards.Count&&!run.enemyHazards.Where((h,i)=>JsonUtility.ToJson(h)!=JsonUtility.ToJson(saved.enemyHazards[i])).Any()
                &&run.enemyCorpses.Count==saved.enemyCorpses.Count&&!run.enemyCorpses.Where((c,i)=>JsonUtility.ToJson(c)!=JsonUtility.ToJson(saved.enemyCorpses[i])).Any(),"Enemy action, corpse or hazard changed across process restart.");return run;
        }
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();string directory="";for(int i=0;i<args.Length-1;i++)if(args[i]=="-hellscriptScreenshots")directory=args[i+1];
            Require(args.Contains("-hellscriptSavePath")&&!string.IsNullOrEmpty(directory),"Enemy smoke requires isolated save and output paths.");Directory.CreateDirectory(directory);Screen.SetResolution(540,960,FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(1);var game=FindAnyObjectByType<GameController>();RunState run;
            if(!args.Contains("-hellscriptEnemyResume")&&!args.Contains("-hellscriptDeathResume"))
            {
                run=Arena(game,new[]{7},new[]{Vector2.up*6});var e=run.enemies[0];e.cooldown=0;yield return new WaitForSecondsRealtime(.5f);run.paused=false;yield return Until(()=>e.brain.action.phase==EnemyActionPhase.Preparing);run.paused=true;
                yield return Capture(game,directory,"01-charge-warning.png");run.paused=false;yield return Until(()=>e.brain.action.phase==EnemyActionPhase.Charging&&e.brain.action.moved>=1.2f);run.paused=true;
                game.UI.ShowEnemyCombat();yield return Capture(game,directory,"02-charge-checkpoint.png");Checkpoint(game,directory,"charge-checkpoint.json");Debug.Log("ENEMY_CHARGE_CHECKPOINT_READY");Application.Quit();yield break;
            }
            if(args.Contains("-hellscriptEnemyResume"))
            {
                run=Resume(game,directory,"charge-checkpoint.json");var e=run.enemies[0];game.UI.ShowBattle();yield return Capture(game,directory,"03-restored-charge.png");game.SetSpeed(1);run.paused=false;
                yield return Until(()=>e.brain.action.phase==EnemyActionPhase.Preparing&&e.brain.action.remainingCharges==1);run.paused=true;yield return Capture(game,directory,"04-second-warning.png");
                run.paused=false;yield return Until(()=>e.brain.action.phase==EnemyActionPhase.Idle);run.paused=true;Require(run.damageEvents.Count(d=>d.incoming&&d.definitionId=="N08")==2,"Two charges did not hit once each.");
                File.WriteAllText(Path.Combine(directory,"charge-completed.json"),JsonUtility.ToJson(run,true));
                run=Arena(game,new[]{8,9,10,6,4,0},new[]{Vector2.up*6,Vector2.right*5,new Vector2(4,3),Vector2.left*2,new Vector2(-4,2),new Vector2(-5,3)});
                foreach(int i in new[]{0,1,3,4})run.enemies[i].cooldown=0;run.enemies[4].health=50000;yield return new WaitForSecondsRealtime(.5f);run.paused=false;
                yield return Until(()=>run.projectiles.Count(p=>p.definitionId=="N09")==3&&run.enemyHazards.Any(h=>h.definitionId=="N10"));run.paused=true;
                Require(run.enemies[1].brain.auraSource==run.enemies[2].id,"Bell aura did not reach the nearby caster.");yield return Capture(game,directory,"05-fortress-patterns.png");game.UI.ShowEnemyCombat();yield return Capture(game,directory,"06-enemy-status.png");
                File.WriteAllText(Path.Combine(directory,"fortress-patterns.json"),JsonUtility.ToJson(run,true));
                run=Arena(game,new[]{0,6,0},new[]{Vector2.up*4,Vector2.left*4,Vector2.left*6});run.enemies[0].elite=0;run.enemies[0].eliteTraits.AddRange(new[]{0,1});run.enemies[1].elite=2;run.enemies[1].eliteTraits.AddRange(new[]{2,3});run.enemies[2].elite=2;run.enemies[1].elitePartner=run.enemies[2].id;run.enemies[2].elitePartner=run.enemies[1].id;
                yield return new WaitForSecondsRealtime(.5f);run.paused=false;yield return Until(()=>run.time>=.05f);run.paused=true;run.enemies[0].brain.traitCooldowns[0]=run.enemies[0].brain.traitCooldowns[1]=.05f;
                run.paused=false;yield return Until(()=>run.enemyHazards.Count==2);run.paused=true;yield return Capture(game,directory,"07-elite-ring-link-barrier.png");File.WriteAllText(Path.Combine(directory,"elite-patterns.json"),JsonUtility.ToJson(run,true));
                run=Arena(game,new[]{5,11,0},new[]{Vector2.up*2,new Vector2(.7f,2),Vector2.up*5});run.enemies[0].health=run.enemies[1].health=1;run.enemies[2].elite=4;
                var build=run.build.Copy();build.rules.Add(BehaviorRules.Make(2));Require(game.Combat.ApplyBuild(build),"Crush setup rejected.");yield return new WaitForSecondsRealtime(.5f);run.paused=false;yield return Until(()=>run.time>=.05f);run.paused=true;run.enemies[2].brain.traitCooldowns[4]=1;
                run.paused=false;yield return Until(()=>run.enemies.Take(2).All(x=>x.dead));run.paused=true;run.movementRule=-1;run.decisionTime=1000;run.paused=false;
                yield return Until(()=>run.enemyHazards.Count==3);run.paused=true;Require(run.enemyCorpses.Count==2&&run.enemyCorpses.Count(c=>c.consumedBy>=0)==1,"Corpse ownership was not assigned exactly once.");
                game.UI.ShowBattle();yield return Capture(game,directory,"08-death-warnings.png");Checkpoint(game,directory,"death-checkpoint.json");Debug.Log("ENEMY_DEATH_CHECKPOINT_READY");Application.Quit();yield break;
            }
            run=Resume(game,directory,"death-checkpoint.json");game.UI.ShowBattle();yield return Capture(game,directory,"09-restored-death-warnings.png");game.SetSpeed(1);run.paused=false;
            yield return Until(()=>run.enemyHazards.Count==0&&run.projectiles.All(p=>!p.hostile));run.paused=true;
            foreach(string definition in new[]{"N06_DEATH","N12_DEATH","E05"})Require(run.damageEvents.Count(d=>d.incoming&&d.definitionId==definition)==1,"Missing or duplicated restored death payload: "+definition);
            Require(run.effectEvents.Count(e=>e.kind=="DEATH")==2&&run.enemyEvents.Count(e=>e.kind=="CORPSE_CONSUMED")==1,"Death or corpse settlement repeated.");game.UI.ShowCombatEffects();yield return Capture(game,directory,"10-death-effects-completed.png");Checkpoint(game,directory,"death-completed.json");
            File.WriteAllText(Path.Combine(directory,"runtime-enemies-smoke.txt"),$"PASS: real N08 preparation and swept charge; actual restart preserves travel and resumes, second direction is warned again and each charge hits once. Real N09 triple arrows, N10 fixed ground, N11 aura, N07 and N05 states; elite dual E01/E02 independent hazards and E03/E04 display. Actual W03 kills N06/N12; E05 claims one corpse; another process restart preserves all three delayed payloads, each hits once and two deaths/one corpse consumption do not repeat.\nFixtures: level 30 Warrior with eight legal rare items; stationary enemies with attack 5 and high HP, base cooldowns disabled except showcased attackers; elite timers shortened once; N06/N12 HP set to 1 before an actual W03. Isolated save. UI methods, not physical input. No Android, balance, performance or completed boss-pattern claim.\nRenderer={SystemInfo.graphicsDeviceType}\nScreen={Screen.width}x{Screen.height}\n");
            Debug.Log("HELLSCRIPT_ENEMY_RUNTIME_SMOKE_OK");Application.Quit();
        }
    }
}

#endif
