#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    [Serializable] public sealed class ClassSkillNativeReceipt
    {
        public string skill,activeSkill,choice,rewardState;
        public float after,health,resource,ultimateRemaining,chargingSeconds,spacingMeters,resourceDebt;
        public double damage;
        public uint rng,rewardRng,gemRng,sheetRng;
        public int damageEvents,effectEvents,pendingExperience,dotCount,kills,meter,runesAwarded,returnPhase,afterimages,rememberedElement;
        public long gold,accountGold;
        public static ClassSkillNativeReceipt Capture(CombatSimulation sim,string skill,float after)=>new ClassSkillNativeReceipt{
            skill=skill,activeSkill=sim.State.classSkills.action?.id??"",choice=ClassSkillOptions.Selected(sim.State.build.classSkills,skill)?.id??"",after=after,health=sim.State.health,resource=sim.State.resource,ultimateRemaining=sim.State.classSkills.ultimateRemaining,
            chargingSeconds=sim.ReadClassSkillPolicies().Sum(p=>p.chargingSeconds),spacingMeters=sim.ReadClassSkillPolicies().Sum(p=>p.movementMeters),
            resourceDebt=sim.State.classSkills.effects.Where(e=>e.kind=="loan").Sum(e=>e.value),returnPhase=sim.State.classSkills.effects.FirstOrDefault(e=>e.kind=="returnGlobe")?.count??-1,
            afterimages=sim.State.classSkills.effects.Count(e=>e.kind=="afterimage"),rememberedElement=sim.State.classSkills.lastElement,
            rewardState=string.Join("|",sim.State.resources.Select(r=>r.id+":"+r.kind+":"+r.amount+":"+r.gemId+":"+r.tier+":"+r.claimed))+";"+string.Join("|",sim.State.drops.Select(d=>d.id+":"+d.item.id+":"+d.claimed)),
            kills=sim.State.kills,meter=sim.State.meter,runesAwarded=sim.State.runesAwarded,rewardRng=sim.State.rewardRng,gemRng=sim.State.gemRng,sheetRng=sim.State.sheetRng,
            damage=sim.State.statistics.damage,rng=sim.State.rng,damageEvents=sim.State.damageEvents.Count,effectEvents=sim.State.effectEvents.Count,
            pendingExperience=sim.State.pendingExperience,dotCount=sim.State.classSkills.dots.Count,gold=sim.State.earnedGold,accountGold=sim.ClassSkillValidationGold};
        public void Verify(ClassSkillNativeReceipt actual)
        {
            if(Math.Abs(health-actual.health)>.002||Math.Abs(resource-actual.resource)>.002||Math.Abs(ultimateRemaining-actual.ultimateRemaining)>.002||Math.Abs(damage-actual.damage)>.01||
                rng!=actual.rng||rewardRng!=actual.rewardRng||gemRng!=actual.gemRng||sheetRng!=actual.sheetRng||kills!=actual.kills||meter!=actual.meter||runesAwarded!=actual.runesAwarded||rewardState!=actual.rewardState||damageEvents!=actual.damageEvents||effectEvents!=actual.effectEvents||pendingExperience!=actual.pendingExperience||dotCount!=actual.dotCount||gold!=actual.gold||accountGold!=actual.accountGold||activeSkill!=actual.activeSkill||choice!=actual.choice||Math.Abs(chargingSeconds-actual.chargingSeconds)>.002||Math.Abs(spacingMeters-actual.spacingMeters)>.002||Math.Abs(resourceDebt-actual.resourceDebt)>.002||returnPhase!=actual.returnPhase||afterimages!=actual.afterimages||rememberedElement!=actual.rememberedElement)
                throw new InvalidOperationException("Restart changed the combat result for "+skill+". Expected "+JsonUtility.ToJson(this)+"; actual "+JsonUtility.ToJson(actual));
        }
    }
    public static class ClassSkillNativeSmoke
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Run()
        {
            var args=Environment.GetCommandLineArgs();if(!args.Contains("-hellscriptClassSkillSmoke"))return;
            try
            {
                if(!Debug.isDebugBuild)throw new InvalidOperationException("Development player required.");
                string Argument(string key){int index=Array.IndexOf(args,key);if(index<0||index+1>=args.Length)throw new ArgumentException(key+" is required.");return Path.GetFullPath(args[index+1]);}
                string output=Argument("-hellscriptEvidence");Argument("-hellscriptSavePath");Directory.CreateDirectory(output);
                var catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();
                VerifyUltimateSupport(catalog,output);
                bool resume=args.Contains("-hellscriptClassSkillResume");
                var cases=new (int build,string skill,string key,float checkpoint,float after)[]{
                    (0,"W18","W18",.3f,2f),(8,"A07","A07",.25f,2f),(22,"M11","M11",1.2f,7f),(16,"M13","M13",.7f,3f),(25,"W02","W02",.3f,3f),
                    (4,"W09","W09-before-kill",0,5f),(4,"W09","W09-after-kill",0,5f),
                    (2,"W15","W15-debt",.6f,3f),(18,"M10","M10-return",.9f,2f),(22,"M12","M12-conduit",.6f,6f),(8,"A18","A18-image",.5f,2f),(22,"M05","M05-memory",.4f,2f)};
                foreach(var item in cases)
                {
                    string directory=Path.Combine(output,"checkpoint-"+item.key);Directory.CreateDirectory(directory);string expectedPath=Path.Combine(output,"expected-"+item.key+".json");
                    if(!resume)
                    {
                        var sim=CombatSimulation.CreateClassSkillValidation(catalog,item.build,0);sim.PrepareClassSkillNativeProbe();
                        if(item.skill=="W15")sim.State.resource=20;
                        if(item.skill=="M05")sim.State.classSkills.lastElement=3;
                        var target=sim.State.enemies[0];
                        if(item.skill=="W09")sim.PrepareClassSkillRewardProbe(item.key.EndsWith("after-kill",StringComparison.Ordinal));
                        else if(item.skill=="W02"||item.skill=="M13")sim.PrepareClassSkillPolicyProbe(item.skill);
                        else if(!sim.TryCastClassSkill(item.skill,target.id,target.position))throw new InvalidOperationException("Could not cast "+item.skill);
                        Advance(sim,item.checkpoint);
                        if(item.skill=="A18"){sim.State.position+=Vector2.right*1.6f;Advance(sim,.1f);}
                        if(item.skill=="W15"&&!sim.State.classSkills.effects.Any(e=>e.kind=="loan"&&e.value>0))throw new InvalidOperationException("Loan checkpoint has no debt.");
                        if(item.skill=="M10"&&!sim.State.classSkills.effects.Any(e=>e.kind=="returnGlobe"&&e.count==1&&e.extra>0))throw new InvalidOperationException("Frost checkpoint must occur during the return flight.");
                        if(item.skill=="A18"&&!sim.State.classSkills.effects.Any(e=>e.kind=="afterimage"))throw new InvalidOperationException("Afterimage checkpoint must contain an actual movement trail.");
                        File.WriteAllText(Path.Combine(directory,"hellscript-local-v1.json"),sim.ExportClassSkillValidationCheckpoint());
                        var store=new GameStore(directory,catalog);if(!store.Save())throw new IOException(store.Error);
                        Advance(sim,item.after);if(item.skill=="W09"){sim.ClaimClassSkillRewardProbe();sim.VerifyClassSkillRewardProbe();}File.WriteAllText(expectedPath,JsonUtility.ToJson(ClassSkillNativeReceipt.Capture(sim,item.skill,item.after),true));
                    }
                    else
                    {
                        var expected=JsonUtility.FromJson<ClassSkillNativeReceipt>(File.ReadAllText(expectedPath));var store=new GameStore(directory,catalog);
                        var sim=new CombatSimulation(store.Data,catalog,1,restore:store.Data.suspendedRun);Advance(sim,expected.after);
                        if(item.skill=="W09"){sim.ClaimClassSkillRewardProbe();sim.VerifyClassSkillRewardProbe();}var actual=ClassSkillNativeReceipt.Capture(sim,item.skill,item.after);expected.Verify(actual);
                        File.WriteAllText(Path.Combine(output,"actual-"+item.key+".json"),JsonUtility.ToJson(actual,true));
                    }
                }
                string status=resume?"CLASS_SKILL_NATIVE_RESTART_OK":"CLASS_SKILL_NATIVE_CHECKPOINT_OK";
                File.WriteAllText(Path.Combine(output,resume?"native-restart.txt":"native-checkpoint.txt"),status+"\n"+Application.unityVersion+"\n"+SystemInfo.operatingSystem+"\nTwelve cases: previous seven combat/reward cases plus resource debt, returning frost, conduit expiry, an afterimage and remembered shield element. Damage, resources, shared cooldown, RNG, events, rewards, policy progress and identity state match uninterrupted combat across a real player process restart. UI/art/device input not assessed.\n");
                Debug.Log(status);Application.Quit(0);
            }
            catch(Exception error){Debug.LogError("CLASS_SKILL_NATIVE_FAILED "+error);Application.Quit(1);}
        }
        static void VerifyUltimateSupport(GameCatalog catalog,string output)
        {
            var proof=new System.Collections.Generic.List<string>();
            foreach(var item in new[]{(6,"W17","WP18","WP19"),(7,"W18","WP19","WP18"),(11,"A17","AP18","AP19"),(15,"A18","AP19","AP18"),(28,"M17","MP18","MP19"),(23,"M18","MP19","MP18")})
            {
                float Outcome(string passive)
                {
                    var sim=CombatSimulation.CreateClassSkillValidation(catalog,item.Item1,0);
                    sim.PrepareUltimateSupportProbe(passive);
                    var target=sim.State.enemies[0];
                    if(!sim.TryCastClassSkill(item.Item2,target.id,target.position))throw new InvalidOperationException("Ultimate support probe cast failed: "+item.Item2);
                    Advance(sim,2.1f);return sim.UltimateSupportProbeOutcome(item.Item2);
                }
                float baseline=Outcome(""),matching=Outcome(item.Item3),unrelated=Outcome(item.Item4);
                if(matching<=baseline+.001f||Math.Abs(unrelated-baseline)>.001f)throw new InvalidOperationException("Ultimate support isolation failed: "+item.Item2+" "+baseline+" / "+matching+" / "+unrelated);
                proof.Add(item.Item2+" -> "+item.Item3+": baseline="+baseline+", matching="+matching+", unrelated="+unrelated);
            }
            File.WriteAllLines(Path.Combine(output,"ultimate-support.txt"),new[]{"ULTIMATE_SUPPORT_NATIVE_OK"}.Concat(proof));
        }
        static void Advance(CombatSimulation sim,float seconds){for(int n=0;n<Mathf.RoundToInt(seconds/CombatSimulation.Step);n++)sim.Tick(CombatSimulation.Step);}
    }
    public sealed partial class CombatSimulation
    {
        internal void PrepareUltimateSupportProbe(string passive)
        {
            PrepareClassSkillNativeProbe();Loadout.passives=new[]{passive,"",""};State.resource=0;Stats.regen=0;
            foreach(var enemy in State.enemies){enemy.health=enemy.maxHealth=1000000;enemy.speed=enemy.attack=0;}
            ApplyStatus(State.enemies[0],StatusKind.Mark,"PROBE",10,State.nextId++,.1f);
        }
        internal float UltimateSupportProbeOutcome(string ultimate)
        {
            if(ultimate=="A17")return (float)State.damageEvents.Where(d=>!d.incoming).Sum(d=>d.finalDamage);
            if(ultimate=="A18"){var f=Fx("A18");return f.until+f.limit;}
            return ultimate.EndsWith("17",StringComparison.Ordinal)?State.resource:State.shield;
        }
        internal void PrepareClassSkillNativeProbe()
        {
            if(!ClassSkills.ValidationBuild||!ClassSkillsActive)throw new InvalidOperationException("Development validation only.");
            State.decisionTime=100000;Loadout.automatic=Array.Empty<string>();State.enemies[0].position=State.position+Vector2.up*2;
            foreach(var e in State.enemies){e.speed=e.attack=0;e.cooldown=10000;}
        }
        internal void PrepareClassSkillRewardProbe(bool afterKill)
        {
            // Keep the controlled arena and isolated account, but exercise the real reward path.
            PrepareClassSkillNativeProbe();State.training=-1;var target=State.enemies[0];target.elite=0;
            if(!TryCastClassSkill("W09",target.id,target.position))throw new InvalidOperationException("Could not prepare a bleeding reward case.");
            for(int n=0;n<10;n++)Tick(Step);
            if(!CS.dots.Any(d=>d.id=="W09"&&d.target==target.id))throw new InvalidOperationException("A live W09 bleed is required before the reward checkpoint.");
            target.health=1;
            if(afterKill){for(int n=0;n<20;n++)Tick(Step);ClaimClassSkillRewardProbe();VerifyClassSkillRewardProbe();}
        }
        internal long ClassSkillValidationGold=>account.gold;
        internal void ClaimClassSkillRewardProbe()
        {
            var gold=State.resources.Single(r=>r.kind==RiftResourceKind.Gold);long before=account.gold;bool already=gold.claimed;
            bool collected=RiftResources.Claim(account,State,gold);
            if(collected==already||account.gold-before!=(already?0:gold.amount))throw new InvalidOperationException("A kill reward must credit the account exactly once.");
        }
        internal void VerifyClassSkillRewardProbe()
        {
            if(State.kills!=1||State.enemies[0].pendingDeath||!State.resources.Any(r=>r.kind==RiftResourceKind.Gold&&r.amount>0)||State.drops.Count!=1)
                throw new InvalidOperationException("Expected exactly one settled kill, actual gold and one elite item drop.");
        }
        internal void PrepareClassSkillPolicyProbe(string skill)
        {
            ClassSkillOptions.WithOption(Loadout,skill,skill=="W02"?"space":"hold").ProjectLegacy(State.build,catalog);
            if(skill=="M13"){ClassSkillOptions.WithOption(Loadout,"M13:goal","full").ProjectLegacy(State.build,catalog);State.resource=0;}
            Loadout.automatic=new[]{skill};State.decisionTime=0;PrepareEdict();
        }
    }
}

#endif
