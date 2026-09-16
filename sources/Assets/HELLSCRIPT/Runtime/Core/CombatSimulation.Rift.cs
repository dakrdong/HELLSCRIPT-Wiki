using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        public RiftNavigation Map {get;private set;}
        public Func<RiftChest,bool> CommitChest;
        RiftChest ActiveChest=>State.layout.chests.Find(c=>c.id==State.exploration.chestId);
        bool ChestBusy=>ActiveChest!=null&&(ActiveChest.phase==ChestPhase.Opening||ActiveChest.phase==ChestPhase.CommitPending);
        Vector2 CombatApproach(EnemyState target,Vector2 preferred,bool requireShot=false)
        {
            Vector2 best=State.position;float score=float.MaxValue;
            float range=Mathf.Min(Policy.distance,Hero.heroClass==HeroClass.Warrior?1.6f:8);
            for(int n=0;n<16;n++)
            {
                float angle=n*Mathf.PI/8;Vector2 p=target.position+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*range;
                if(!Map.Walkable(p)||!Map.LineClear(p,target.position)||requireShot&&!Map.ProjectileClear(p,target.position,.3f))continue;
                float value=Map.Length(State.position,p)+Vector2.Distance(preferred,p)*.25f;if(value>=score)continue;best=p;score=value;
            }
            return best;
        }
        Vector2 ShotApproach(EnemyState target,Vector2 preferred)
        {
            if(State.shotApproachTarget!=target.id||State.time>=State.shotApproachReview||Vector2.Distance(State.shotApproachAim,target.position)>1.5f)
            {
                State.shotApproachTarget=target.id;State.shotApproachAim=target.position;State.shotApproachReview=State.time+.6f;
                State.shotApproachPosition=CombatApproach(target,preferred,true);
            }
            State.action="화살과 마법이 통과할 위치로 이동";
            return State.shotApproachPosition;
        }
        public bool RetryRiftFault()
        {
            if(string.IsNullOrEmpty(State.navigationError))return true;
            State.navigationError="";Map.Repath();CompleteReadyChest();TickShrine(0);TickFieldEvents(0);
            if(State.phase==RunPhase.Boss&&State.bossId<0)TrySpawnBoss(0);
            ResolveCombatDeaths();if(string.IsNullOrEmpty(State.navigationError))TickOfferings();SettleCombatOutcome();
            return string.IsNullOrEmpty(State.navigationError);
        }
        void InitializeRift(bool fresh,RiftObjectiveKind? forcedObjective=null)
        {
            State.exploration??=new RiftExplorationState();State.exploration.enemies??=new System.Collections.Generic.List<EnemyObservation>();
            if(fresh&&State.training<0)
            {
                State.layout=RiftGenerator.Generate(State.rng,State.id,State.stage,Hero.heroClass,Hero.lastRiftFingerprint,Hero.lastRiftBoss,forcedObjective:forcedObjective);
                State.position=State.layout.start;State.theme=State.layout.theme;State.rng=State.layout.combatSeed;State.rewardRng=State.layout.rewardSeed;
                Hero.lastRiftFingerprint=State.layout.fingerprint;Hero.lastRiftBoss=State.layout.bossKind;
            }
            if(State.layout!=null&&State.layout.version>RiftLayout.CurrentVersion)throw new InvalidOperationException("진행 중인 균열은 더 새로운 지도 버전이 필요합니다.");
            if(State.layout==null||State.layout.rooms==null||State.layout.rooms.Count==0)State.layout=RiftLayout.Legacy(State.theme);
            if(State.rewardRng==0)State.rewardRng=State.rng;
            GameStore.NormalizeRun(State);Map=new RiftNavigation(State.layout);
        }
        void TrySpawnBoss(float dt)
        {
            if(State.phase!=RunPhase.Boss||State.bossId>=0)return;
            bool Safe(Vector2 p)=>!State.enemies.Any(e=>!e.dead&&Vector2.Distance(p,e.position)<2.5f)&&
                !State.effects.Any(f=>f.hostile&&Vector2.Distance(p,f.position)<f.radius+1.2f)&&!EnemyThreatAt(p,1.5f);
            Vector2? position=null;
            if(State.layout.roamingBoss)
            {
                // Crowds and ground effects can temporarily occupy every nearby candidate. Keep
                // fighting and retry, rather than ending an otherwise valid run after two seconds.
                float before=State.bossWait;State.bossWait+=dt;
                if(dt>0&&before>0&&Mathf.FloorToInt(before*4)==Mathf.FloorToInt(State.bossWait*4))return;
                position=RiftBossPlacement.NearPlayer(State.layout,Map,State.position,Safe);
            }
            else foreach(var p in State.layout.bossPoints)
            {
                if(!Map.CanLand(p,1.2f)||Vector2.Distance(p,State.position)<6||!Safe(p))continue;
                position=p;break;
            }
            if(position.HasValue)
            {
                int room=State.layout.roamingBoss?Map.RoomAt(position.Value):State.layout.bossRoom;
                if(room<0)room=State.layout.rooms.OrderBy(r=>Vector2.Distance(r.position,position.Value)).First().index;
                SpawnEnemy(room,0,position.Value,-1,true);
                Log("BOSS_SPAWN",Loc.F("처치 게이지 충전 · {0}", GameCatalog.BossNames[State.layout.bossKind]));return;
            }
            State.action="보스 등장 위치 확인";
            if(State.layout.roamingBoss)return;
            State.bossWait+=dt;
            if(State.bossWait>=2){State.navigationError="보스 등장 공간을 확보하지 못했습니다. 같은 균열의 상태를 보존했습니다.";DisableAutoRepeat();Log("RIFT_FAULT",State.navigationError);}
        }
        bool SafeForChest(RiftChest chest)=>FieldSafe(chest.openingPosition);
        void UpdateChestAvailability()
        {
            foreach(var c in State.layout.chests)
            {
                if(c.phase!=ChestPhase.Locked||c.abandoned||State.meter<c.Gate)continue;
                if(c.definitionId=="CH03"&&State.layout.events.Any(e=>e.chestId==c.id&&e.phase!=RiftEventPhase.Dormant&&e.phase!=RiftEventPhase.Succeeded))continue;
                if(c.guardGroup>=0&&State.enemies.Any(e=>!e.dead&&e.group==c.guardGroup))continue;
                c.phase=ChestPhase.Available;if(c.discovered)Log("CHEST_UNLOCK",c.definitionId=="CH02"?"경비 무리 처치 · 봉인 해제":"보물 상자 개봉 가능");
            }
        }
        void ChooseChestGoal(ref Vector2 goal)
        {
            if(State.layout.legacy||!Policy.openChests||State.phase==RunPhase.Boss&&!Policy.chestsAfterBoss||LowTime){CancelChest("");return;}
            var active=ActiveChest;
            if(active!=null&&active.phase==ChestPhase.Approaching&&SafeForChest(active)&&CursedAllowed(active))
            {
                if(!OpeningPointFree(active.openingPosition))
                {var alternative=active.accessPoints.Where(OpeningPointFree).OrderBy(p=>Map.Length(State.position,p)).ToList();if(alternative.Count==0){CancelChest("접근 위치가 막혔습니다");return;}active.openingPosition=alternative[0];Map.Repath();}
                goal=active.openingPosition;State.action=active.definitionId=="CH03"?"저주 상자로 이동":"보물 상자로 이동";return;
            }
            if(ChestBusy)return;
            float best=float.MaxValue;RiftChest selected=null;Vector2 access=Vector2.zero;
            foreach(var c in State.layout.chests)
            {
                if(!c.discovered||c.abandoned||c.phase!=ChestPhase.Available||c.retryAfter>State.time||c.definitionId=="CH01"&&!Policy.commonChests||c.definitionId=="CH02"&&!Policy.sealedChests||!CursedAllowed(c)||!SafeForChest(c))continue;
                foreach(var p in c.accessPoints)
                {
                    if(!OpeningPointFree(p))continue;float length=Map.Length(State.position,p);if(length>Policy.chestDetour||length>=best)continue;
                    best=length;selected=c;access=p;
                }
            }
            if(selected==null)return;
            selected.openingPosition=access;selected.phase=ChestPhase.Approaching;State.exploration.chestId=selected.id;goal=access;State.action="보물 상자로 이동";
        }
        void TickChests(float dt)
        {
            var c=ActiveChest;if(c==null)return;
            if(!Policy.openChests||!SafeForChest(c)||!CursedAllowed(c)||State.phase==RunPhase.Boss&&!Policy.chestsAfterBoss)
            {CancelChest("전투를 우선합니다");return;}
            if(c.phase==ChestPhase.Approaching)
            {
                if(Vector2.Distance(State.position,c.openingPosition)>.25f)return;
                if(c.definitionId=="CH03"&&State.layout.events.Any(e=>e.chestId==c.id&&e.phase==RiftEventPhase.Dormant)){StartCursedEvent(c);return;}
                c.phase=ChestPhase.Opening;c.progress=0;State.activeSkill=-1;Log("CHEST_OPEN_START",c.definitionId=="CH01"?"보물 상자 개봉":"봉인 상자 개봉");
            }
            if(c.phase!=ChestPhase.Opening)return;
            if(Vector2.Distance(State.position,c.position)>1.5f){CancelChest("개봉 거리 이탈");return;}
            State.action=Loc.F("상자 개봉 {0:0.0} / {1:0.0}초", c.progress, c.Duration);CompleteReadyChest();
        }
        void AdvanceChestOpening(float dt)
        {var c=ActiveChest;if(c!=null&&c.phase==ChestPhase.Opening&&SafeForChest(c)&&Vector2.Distance(State.position,c.position)<=1.5f)c.progress=Mathf.Min(c.Duration,c.progress+dt);}
        void CompleteReadyChest()
        {
            var c=ActiveChest;if(c==null||c.phase!=ChestPhase.Opening||c.progress+.0001f<c.Duration)return;
            bool success=CommitChest!=null?CommitChest(c):ChestRewards.Apply(account,State,c.id);
            if(!success){State.navigationError="상자 보상을 저장하지 못했습니다. 같은 요청으로 다시 확인해야 합니다.";DisableAutoRepeat();Log("CHEST_COMMIT_FAILED",c.requestId);return;}
            Log("CHEST_OPENED",Loc.F("{0} · 골드 {1} / 재료 {2}{3}", c.definitionId, c.gold, c.materials, (c.reward!=null&& !string.IsNullOrEmpty(c.reward.id)?" / 장비 드롭":"")));
            State.exploration.chestId="";Visual?.Invoke(c.position,c.position,20,1.5f);
            State.action="보물 상자 개봉 완료";
        }
        void CancelChest(string reason)
        {
            var c=ActiveChest;if(c==null)return;
            if(c.phase==ChestPhase.Approaching||c.phase==ChestPhase.Opening)
            {c.phase=ChestPhase.Available;c.progress=0;c.retryAfter=State.time+2;if(!string.IsNullOrEmpty(reason))Log("CHEST_INTERRUPTED",reason);}
            State.exploration.chestId="";
        }
        void CloseUnopenedChests()
        {foreach(var c in State.layout.chests)if(c.phase!=ChestPhase.Opened){c.abandoned=true;c.phase=ChestPhase.Exhausted;c.progress=0;}State.exploration.chestId="";CloseFieldContent();}
    }
}
