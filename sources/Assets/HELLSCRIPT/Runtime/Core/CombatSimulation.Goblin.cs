using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        EnemyState Goblin=>State.goblin==null||State.goblin.enemyId<0?null:State.enemies.Find(e=>e.id==State.goblin.enemyId);
        // One golden goblin per rift from stage five, drawn from its own stream seeded by the layout.
        // Nothing here touches the combat or reward streams, and the roll is stored with the run, so
        // a restart neither rerolls the appearance nor shifts the fight that follows.
        void SpawnGoldenGoblin()
        {
            var g=State.goblin??=new GoldenGoblinState();
            if(g.rolled||State.training>=0||State.layout.legacy||State.stage<GoldenGoblin.MinimumStage)return;
            g.rolled=true;g.rng=State.layout.rewardSeed^0x6F1D2B9Du^(uint)(State.stage*7919);if(g.rng==0)g.rng=1;
            if(RandomStream.Unit(ref g.rng)>=GoldenGoblin.Chance)return;
            int startRoom=Map.RoomAt(State.layout.start);
            var rooms=State.layout.rooms.Where(r=>!r.boss&&r.index!=State.layout.bossRoom&&r.index!=startRoom&&Map.CanLand(r.position)&&Map.Reachable(r.position)&&
                Vector2.Distance(r.position,State.position)>GoldenGoblin.DetectRange).OrderBy(r=>r.index).ToArray();
            if(rooms.Length==0){Log("GOBLIN_SKIPPED",Loc.F("{0} · 시작·보스 구역 밖에 도달 가능한 위치가 없습니다.",GoldenGoblin.Name));return;}
            var room=rooms[RandomStream.Range(ref g.rng,0,rooms.Length)];
            float health=70*Mathf.Pow(1.08f,State.stage-1)*GoldenGoblin.HealthMultiplier;
            var e=new EnemyState{id=State.nextId++,room=room.index,kind=0,position=room.position,goblin=true,attack=0,cooldown=1000,speed=GoldenGoblin.Speed,health=health,maxHealth=health};
            InitializeEnemyBrain(e,State);State.enemies.Add(e);
            g.spawned=true;g.enemyId=e.id;g.spawnedAt=State.time;
            Log("GOBLIN_SPAWN",Loc.F("{0} 출현",GoldenGoblin.Name));
        }
        // The goblin never attacks. Seeing the hero within eight metres or taking damage starts a
        // flight; after twenty seconds it spends two seconds escaping and is gone without rewards.
        // Stun and freeze are skipped before this call and pause the escape; root only holds the body.
        void TickGoldenGoblin(EnemyState e,float dt)
        {
            var g=State.goblin;if(g==null)return;var b=e.brain;
            bool sees=Vector2.Distance(e.position,State.position)<=GoldenGoblin.DetectRange&&Map.LineClear(e.position,State.position),hurt=e.health<e.maxHealth-.0001f;
            if(!g.fleeing&&(sees||hurt))
            {
                g.fleeing=true;g.fleeingSince=State.time;g.fleeGoalTime=State.time;EnemyMode(e,"도주");
                Log("GOBLIN_FLEE",Loc.F("{0} · {1}",GoldenGoblin.Name,hurt?"피격을 감지해 도주합니다.":"영웅을 발견해 도주합니다."));
            }
            if(!g.fleeing){EnemyMode(e,"배회");return;}
            if(State.time-g.fleeingSince>=GoldenGoblin.FleeSeconds-.0001f)
            {
                EnemyMode(e,"탈출 시도");g.escapeProgress+=dt;
                if(g.escapeProgress>=GoldenGoblin.EscapeSeconds-.0001f)
                {g.escaped=true;e.dead=true;e.pendingDeath=false;e.health=0;EnemyEvent(e,"ESCAPED");Log("GOBLIN_ESCAPED",Loc.F("{0} 탈출 · 보상 없음",GoldenGoblin.Name));}
                return;
            }
            if(Immobilized(e))return;
            if(State.time>=g.fleeGoalTime-.0001f||Vector2.Distance(e.position,g.fleeGoal)<.5f){g.fleeGoal=GoblinFleeGoal(e);g.fleeGoalTime=State.time+1;}
            var before=e.position;e.position=Map.Move(e.position,g.fleeGoal,e.speed*(1-SlowRatio(e))*dt,e.id+1,State.time,.4f);
            if(e.position!=before&&b.rearWindow<=0)b.facing=(e.position-before).normalized;
        }
        Vector2 GoblinFleeGoal(EnemyState e)
        {
            var away=e.position-State.position;if(away.sqrMagnitude<.01f)away=Vector2.up;
            float current=Vector2.Distance(e.position,State.position);
            var room=State.layout.rooms.Where(r=>Map.CanLand(r.position)&&Vector2.Distance(r.position,State.position)>current+2)
                .OrderByDescending(r=>Vector2.Distance(r.position,State.position)).ThenBy(r=>r.index).FirstOrDefault();
            return room!=null?room.position:Map.MoveDirect(e.position,e.position+away.normalized*8,8,.4f);
        }
        // Eight times the elite base gold and one rare-or-better piece with the elite legendary odds,
        // paid once; no experience, meter, rune or gem reward comes with it.
        void RewardGoldenGoblin(EnemyState e)
        {
            var g=State.goblin;if(g==null||g.rewarded)return;g.rewarded=true;
            if(State.training>=0)return;
            RiftResources.Add(State,RiftResourceKind.Gold,e.position,KillGold(GoldenGoblin.GoldMultiplier*(25+5*State.stage)));
            int rarity=Mathf.Max(2,RiftRarity.Roll(RiftRewardSource.Elite,State.stage,ref g.rng,Stats.magicFind));
            DropFrom(e.position,rarity,ref g.rng);
            Log("GOBLIN_KILLED",Loc.F("{0} 처치 · 정예 금화 {1}배와 희귀 이상 장비 1개",GoldenGoblin.Name,GoldenGoblin.GoldMultiplier));
        }
    }
}
