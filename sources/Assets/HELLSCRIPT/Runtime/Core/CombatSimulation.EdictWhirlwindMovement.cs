using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        bool MoveEdictWhirlwind(float dt)
        {
            var action=State.heroAction;var d=action.policy?.whirlwindEdict;
            if(action.skill!=0||action.phase!=HeroActionPhase.Channeling||d==null)return false;
            var seen=ForecastSeen().Where(EdictTargetEligible).ToArray();var target=seen.FirstOrDefault(e=>e.id==State.targetId);
            if(target==null)return true;
            var values=EdictGlobals(d);
            string movement=HuntEdictV2.Value(d,"W01",3);Vector2 delta=State.position-target.position,goal=State.position;
            if(delta.sqrMagnitude<.0001f)delta=Vector2.down;
            float distance=Vector2.Distance(State.position,target.position);
            if(movement=="STAND")return true;
            if(movement=="EDGE"||movement=="GLOBAL"&&values["position.mode"]=="EDGE")goal=EdictPackEdgeGoal(d,values,target,seen);
            else if(movement=="CENTER"||movement=="GLOBAL"&&values["position.mode"]=="CENTER")
            {
                // This skill's movement policy is independent of global positioning. The shared
                // hazard, navigation and pursuit gates below still govern every movement step.
                goal=EdictPackCenterGoal(d,values,target,seen);
            }
            else if(movement=="FOLLOW")
            {float radius=D4Combat?RuneSkillRadius(0,2.5f)*ClassArea(action.id):2.5f;if(distance<=radius-.1f)return true;goal=target.position+delta.normalized*(radius-.2f);}
            else
            {
                float desired=EdictDistance(values);string global=values["position.mode"];
                if(global=="STAND")return true;
                if(global=="DISTANCE")
                {if(distance>=desired-.75f&&distance<=EdictDistanceUpper(values,desired))return true;goal=target.position+delta.normalized*desired;}
                else goal=target.position+delta.normalized*Mathf.Min(desired,1.6f);
            }
            if((goal-State.position).sqrMagnitude<.000001f||!EdictAttackPointAllowed(d,values,goal,seen))return true;
            if(!AdvanceEdictPursuit(dt,false,target,action.policy.rule))return true;
            float speed=MovementSpeed(false)*HeroPace;
            Vector2 next=Map.Move(State.position,goal,speed*dt,0,State.time);
            if(!EdictAttackPointAllowed(d,values,next,seen)||!EdictPursuitStepAllowed(next,false,target,action.policy.rule))return true;
            State.moveDistance+=Vector2.Distance(State.position,next);State.position=next;State.destination=goal;
            return true;
        }
    }
}
