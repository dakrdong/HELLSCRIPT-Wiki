using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public enum BossAttack { Basic=100, Slam, Hook, Summon, Beam, Charge, Blasts, Legacy }
    [Serializable] public sealed class BossRefuge
    {
        public Vector2 position;
        public float radius=.65f;
        public List<Vector2> path=new List<Vector2>();
    }
    [Serializable] public sealed class BossPatternState
    {
        public bool initialized,enraged,enragePending;
        public float[] cooldowns=new float[3];
        public float planRetry;
        public List<BossRefuge> refuges=new List<BossRefuge>();
    }
    public static class BossCombat
    {
        public static string Definition(int kind)
        {
            switch((BossAttack)kind)
            {
                case BossAttack.Basic:return "BOSS_BASIC";case BossAttack.Slam:return "BOSS01_SLAM";case BossAttack.Hook:return "BOSS01_HOOK";
                case BossAttack.Summon:return "BOSS02_SUMMON";case BossAttack.Beam:return "BOSS02_BEAM";case BossAttack.Charge:return "BOSS03_CHARGE";
                case BossAttack.Blasts:return "BOSS03_BLAST";default:return "LEGACY_BOSS";
            }
        }
        public static string Name(int kind)
        {
            switch((BossAttack)kind)
            {case BossAttack.Basic:return "근접 공격";case BossAttack.Slam:return "내려찍기";case BossAttack.Hook:return "갈고리";case BossAttack.Summon:return "부하 소환";case BossAttack.Beam:return "집중 광선";case BossAttack.Charge:return "돌진";case BossAttack.Blasts:return "연속 폭발";default:return "이전 공격 마무리";}
        }
        public static EnemyAttackDefinition Attack(BossAttack attack)
        {
            switch(attack)
            {
                case BossAttack.Slam:return new EnemyAttackDefinition(4,1.2f,6);case BossAttack.Hook:return new EnemyAttackDefinition(10,1,9);
                case BossAttack.Summon:return new EnemyAttackDefinition(12,.8f,14);case BossAttack.Beam:return new EnemyAttackDefinition(12,1.5f,8);
                case BossAttack.Charge:return new EnemyAttackDefinition(8,1,7);case BossAttack.Blasts:return new EnemyAttackDefinition(12,1.3f,12);
                default:return new EnemyAttackDefinition(3,.6f,2);
            }
        }
        public static bool OwnAdd(EnemyState add,EnemyState boss)=>add.add&&!add.boss&&string.IsNullOrEmpty(add.eventId)&&(add.summonerId==boss.id||add.summonerId<0);
        public static IEnumerable<EnemyThreat> Threats(RunState run,RiftNavigation navigation)
        {
            foreach(var enemy in run.enemies.Where(e=>e.boss&&!e.dead))
            {
                var a=enemy.brain.action;if(a.phase==EnemyActionPhase.Idle||a.phase==EnemyActionPhase.Recovering)continue;
                string key="boss-"+a.id,definition=Definition(a.kind);var kind=(BossAttack)a.kind;
                if(kind==BossAttack.Summon)continue;
                if(kind==BossAttack.Blasts)
                {for(int i=0;i<a.points.Count;i++)yield return new EnemyThreat(key+"-"+i,definition,AttackShape.Circle,a.points[i],a.points[i],a.direction,2.5f,delay:a.remaining+i*.35f);}
                else if(kind==BossAttack.Hook||kind==BossAttack.Beam||kind==BossAttack.Charge)
                {
                    var from=a.phase==EnemyActionPhase.Charging?enemy.position:a.origin;float radius=kind==BossAttack.Beam?.75f:.95f;
                    yield return new EnemyThreat(key,definition,AttackShape.Line,from,a.aim,a.direction,radius,delay:a.phase==EnemyActionPhase.Charging?0:a.remaining);
                }
                else if(kind==BossAttack.Legacy)yield return new EnemyThreat(key,definition,AttackShape.Circle,a.aim,a.aim,a.direction,3,delay:a.remaining);
                else yield return new EnemyThreat(key,definition,AttackShape.Sector,a.origin,a.aim,a.direction,kind==BossAttack.Slam?4:3,delay:a.remaining);
            }
        }
        public static bool TryBlastPlan(RunState run,RiftNavigation map,EnemyState boss,float speed,out List<Vector2> centers,out List<BossRefuge> refuges)
        {
            centers=new List<Vector2>();refuges=new List<BossRefuge>();float budget=Mathf.Max(0,speed*1.1f);
            var threats=EnemyCombat.Threats(run,map).Where(t=>t.delay<=2&&Vector2.Distance(run.position,t.origin)<=14+t.radius&&map.LineClear(run.position,t.origin)).ToList();
            foreach(var f in run.effects.Where(f=>f.hostile&&f.delay<=2))threats.Add(new EnemyThreat("",f.definitionId,f.kind==6?AttackShape.Ring:AttackShape.Circle,f.position,f.position,Vector2.up,f.radius,f.kind==6?2:0));
            foreach(var p in run.projectiles.Where(p=>p.hostile))threats.Add(new EnemyThreat("",p.definitionId,AttackShape.Line,p.position,map.ProjectileEnd(p.position,p.position+p.direction*p.remaining,p.radius,out _),p.direction,p.radius+.45f));
            var candidates=new List<BossRefuge>();
            foreach(float radius in new[]{3.3f,4f})for(int n=0;n<16;n++)
            {
                if(radius>budget)continue;Vector2 point=run.position+EnemyCombat.Rotate(Vector2.up,n*22.5f)*radius;
                if(!map.CanLand(point,.65f)||!map.Reachable(point)||run.enemies.Any(e=>!e.dead&&Vector2.Distance(e.position,point)<(e.boss?1.2f:.4f)+.65f))continue;
                var path=map.FindPath(run.position,point);if(RiftNavigation.Length(path)>budget||path==null||Vector2.Distance(path[path.Count-1],point)>.01f)continue;
                bool safe=true;
                for(int i=1;i<path.Count&&safe;i++)for(float t=0;t<=1.0001f;t+=1f/Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(path[i-1],path[i])/.25f)))
                {var p=Vector2.Lerp(path[i-1],path[i],t);if(threats.Any(th=>EnemyCombat.Contains(th,p)&&map.LineClear(th.origin,p))||run.enemies.Any(e=>!e.dead&&Vector2.Distance(e.position,p)<(e.boss?1.2f:.4f)+.45f)){safe=false;break;}}
                if(safe)candidates.Add(new BossRefuge{position=point,path=path});
            }
            for(int n=0;n<8;n++)
            {
                var axis=EnemyCombat.Rotate(Vector2.up,n*22.5f);var points=new List<Vector2>{run.position,run.position+axis*3.5f,run.position-axis*3.5f};
                if(points.Any(p=>!map.CanLand(p,.1f)))continue;
                var valid=candidates.Where(r=>points.All(p=>Vector2.Distance(p,r.position)>2.5f+r.radius+.01f)).ToList();
                foreach(var first in valid)
                {
                    var second=valid.FirstOrDefault(r=>Vector2.Distance(first.position,r.position)>=1.5f);if(second==null)continue;
                    centers=points;refuges=new List<BossRefuge>{first,second};return true;
                }
            }
            return false;
        }
    }
}
