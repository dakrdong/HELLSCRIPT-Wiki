using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class WorldView
    {
        readonly Dictionary<string,GameObject> enemyThreatViews=new Dictionary<string,GameObject>();
        void EnemyOutline(string key,Vector2[] points,Material material,float width,HashSet<string> active)
        {
            active.Add(key);
            if(!enemyThreatViews.TryGetValue(key,out var go))
            {go=new GameObject(key);go.transform.SetParent(world.transform,false);var line=go.AddComponent<LineRenderer>();line.useWorldSpace=true;enemyThreatViews[key]=go;}
            var renderer=go.GetComponent<LineRenderer>();renderer.sharedMaterial=material;renderer.widthMultiplier=width;renderer.positionCount=points.Length;
            for(int i=0;i<points.Length;i++)renderer.SetPosition(i,Position(points[i])+Vector3.up*.19f);
        }
        void PresentEnemyCombat(RunState run)
        {
            var active=new HashSet<string>();
            foreach(var threat in EnemyCombat.Threats(run,game.Combat.Map))
            {
                if(!CanDisplayEnemyMarker(run,threat.origin,threat.radius))continue;
                int index=0;float width=threat.delay>0?.06f+Mathf.Clamp01(1-threat.delay/1.5f)*.08f:.18f;
                foreach(var line in EnemyCombat.Outlines(threat))EnemyOutline(threat.key+"-"+index++,line,red,width,active);
            }
            foreach(var e in run.enemies.Where(e=>!e.dead&&Vector2.Distance(run.position,e.position)<=12&&game.Combat.Map.LineClear(run.position,e.position)))
            {
                if(e.boss)foreach(var refuge in e.brain.boss.refuges)
                {int index=e.brain.boss.refuges.IndexOf(refuge);foreach(var line in EnemyCombat.Outlines(new EnemyThreat("","REFUGE",AttackShape.Circle,refuge.position,refuge.position,Vector2.up,refuge.radius)))EnemyOutline("refuge-"+e.id+"-"+index,line,blue,.14f,active);}
                if(!e.boss&&e.kind==10)
                    foreach(var line in EnemyCombat.Outlines(new EnemyThreat("","N11",AttackShape.Circle,e.position,e.position,e.brain.facing,4)))EnemyOutline("aura-"+e.id,line,purple,.04f,active);
                if(EnemyCombat.Trait(e,2))
                {var other=run.enemies.Find(x=>x.id==e.elitePartner&&!x.dead&&x.elite>=0&&Vector2.Distance(x.position,e.position)<=5&&Vector2.Distance(run.position,x.position)<=12&&game.Combat.Map.LineClear(run.position,x.position));if(other!=null)EnemyOutline("link-"+e.id,new[]{e.position,other.position},purple,.1f,active);}
                if(EnemyCombat.Trait(e,3)||e.brain.rearWindow>0)
                {
                    bool rear=e.brain.rearWindow>0;var direction=rear?-e.brain.facing:e.brain.facing;var points=new Vector2[17];
                    for(int i=0;i<17;i++)points[i]=e.position+EnemyCombat.Rotate(direction,-60+i*120f/16)*1.15f;
                    EnemyOutline("guard-"+e.id,points,rear?ember:blue,.12f,active);
                }
            }
            foreach(var key in enemyThreatViews.Keys.ToArray())if(!active.Contains(key)){Destroy(enemyThreatViews[key]);enemyThreatViews.Remove(key);}
        }
    }
}
