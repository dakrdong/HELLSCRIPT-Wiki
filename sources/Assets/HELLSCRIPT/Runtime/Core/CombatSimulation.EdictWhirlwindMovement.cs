using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        Vector2 WhirlwindCenterGoal(HuntEdictV2Document d,Dictionary<string,string> values,EnemyState target,EnemyState[] seen)
        {
            var group=seen.Where(e=>Vector2.Distance(e.position,target.position)<=5).ToArray();
            Vector2 center=group.Aggregate(Vector2.zero,(p,e)=>p+e.position)/group.Length;
            var points=new List<Vector2>{center,State.position};
            // The centroid often coincides with an enemy body. Seek a nearby legal gap instead of
            // rejecting the occupied centroid and standing still, or disabling collision checks.
            for(int ring=1;ring<=3;ring++)for(int i=0;i<24;i++)points.Add(center+EnemyCombat.Rotate(Vector2.up,i*15)*(.9f*ring));
            foreach(var p in points.Where(p=>(p-center).sqrMagnitude<=(State.position-center).sqrMagnitude+.0001f)
                .OrderBy(p=>(p-center).sqrMagnitude).ThenBy(p=>(p-State.position).sqrMagnitude).ThenBy(p=>p.x).ThenBy(p=>p.y))
                if(Map.LineClear(State.position,p)&&group.Any(e=>Vector2.Distance(p,e.position)<=RuneSkillRadius(0,2.5f))&&EdictAttackPointAllowed(d,values,p,seen))return p;
            return State.position;
        }
        Vector2 WhirlwindEdgeGoal(HuntEdictV2Document d,Dictionary<string,string> values,EnemyState target,EnemyState[] seen)
        {
            var group=seen.Where(e=>Vector2.Distance(e.position,target.position)<=5).ToArray();
            Vector2 center=group.Aggregate(Vector2.zero,(p,e)=>p+e.position)/group.Length;
            Vector2 radial=State.position-center;if(radial.sqrMagnitude<.0001f)radial=Vector2.down;
            float turn=values["position.clockwise"]=="ON"?1:-1;
            Vector2 tangent=new Vector2(-radial.y,radial.x).normalized*turn;
            Vector2 ideal=center+EnemyCombat.Rotate(radial,15*turn);
            var points=new List<Vector2>{ideal};
            foreach(var e in group)for(int i=0;i<24;i++)points.Add(e.position+EnemyCombat.Rotate(Vector2.up,i*15)*2.35f);
            // Keep moving around the outside in the chosen direction. No forced reversal or
            // artificial travel is introduced to manufacture the set's walking charge.
            foreach(var p in points.Distinct().Where(p=>Vector2.Distance(p,State.position)<=3&&Vector2.Dot(p-State.position,tangent)>.01f&&group.Any(e=>Vector2.Distance(p,e.position)<=RuneSkillRadius(0,2.5f)&&Map.LineClear(p,e.position)))
                .OrderByDescending(p=>group.Min(e=>Vector2.Distance(p,e.position))).ThenBy(p=>(p-ideal).sqrMagnitude).ThenBy(p=>p.x).ThenBy(p=>p.y))
                if(Map.LineClear(State.position,p)&&EdictAttackPointAllowed(d,values,p,seen))return p;
            return State.position;
        }
        bool MoveEdictWhirlwind(float dt)
        {
            var action=State.heroAction;var d=action.policy?.whirlwindEdict;
            if(action.skill!=0||action.phase!=HeroActionPhase.Channeling||d==null)return false;
            var seen=ForecastSeen().Where(EdictTargetEligible).ToArray();var target=seen.FirstOrDefault(e=>e.id==State.targetId);
            if(target==null)return true;
            var values=d.global.ToDictionary(o=>o.id,o=>o.value,StringComparer.Ordinal);
            string movement=HuntEdictV2.Value(d,"W01",3);Vector2 delta=State.position-target.position,goal=State.position;
            if(delta.sqrMagnitude<.0001f)delta=Vector2.down;
            float distance=Vector2.Distance(State.position,target.position);
            if(movement=="STAND")return true;
            if(movement=="EDGE"||movement=="GLOBAL"&&values["position.mode"]=="EDGE")goal=WhirlwindEdgeGoal(d,values,target,seen);
            else if(movement=="CENTER")
            {
                // This skill's movement policy is independent of global positioning. The shared
                // hazard, navigation and pursuit gates below still govern every movement step.
                goal=WhirlwindCenterGoal(d,values,target,seen);
            }
            else if(movement=="FOLLOW")
            {if(distance<=2.4f)return true;goal=target.position+delta.normalized*2.3f;}
            else
            {
                float desired=EdictDistance(values);string global=values["position.mode"];
                if(global=="STAND"&&distance<=RuneSkillRadius(0,2.5f))return true;
                if(global=="DISTANCE")
                {if(Mathf.Abs(distance-desired)<=.75f)return true;goal=target.position+delta.normalized*desired;}
                else goal=target.position+delta.normalized*Mathf.Min(desired,1.6f);
            }
            if((goal-State.position).sqrMagnitude<.000001f||!EdictAttackPointAllowed(d,values,goal,seen))return true;
            if(!AdvanceEdictPursuit(dt,false,target,action.policy.rule))return true;
            float speed=MovementSpeed(false)*(State.enemySlowTime>0?.65f:1);
            Vector2 next=Map.Move(State.position,goal,speed*dt,0,State.time);
            if(!EdictAttackPointAllowed(d,values,next,seen)||!EdictPursuitStepAllowed(next,false,target,action.policy.rule))return true;
            State.moveDistance+=Vector2.Distance(State.position,next);State.position=next;State.destination=goal;
            return true;
        }
    }
}
