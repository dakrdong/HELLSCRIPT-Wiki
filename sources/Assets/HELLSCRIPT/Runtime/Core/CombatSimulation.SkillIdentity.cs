using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        bool IdentityReady(string id)=>id!="W15"||!CS.effects.Any(e=>e.id=="W15"&&e.kind=="loan"&&e.value>0);

        void BreakFront(ClassSkillCast a,EnemyState[] victims)
        {
            Vector2 forward=(a.destination-a.origin).normalized,side=new Vector2(-forward.y,forward.x);
            foreach(var enemy in victims)
            {
                SkillHit(a,enemy,N(a.id,"coefficient"),0);
                float sign=Vector2.Dot(enemy.position-a.origin,side);
                if(Mathf.Abs(sign)<.01f)sign=enemy.id%2==0?1:-1;
                Pull(enemy,enemy.position+side*Mathf.Sign(sign)*N(a.id,"pushMeters"),N(a.id,"pushMeters"),a.id,a.root);
            }
        }
        void BorrowResource(ClassSkillCast a)
        {
            if(!IdentityReady(a.id))return;
            float credited=Resource(N(a.id,"resourceGain")*a.rankScale,a.id,a.root);
            if(credited<=0)return;
            var loan=Buff(a.id,0,credited,a.root,"loan");loan.persistent=true;
            CSEvent(a.id,"BORROW",a.root,value:credited);
        }
        float RepayResource(float amount)
        {
            var loan=CS?.effects.FirstOrDefault(e=>e.id=="W15"&&e.kind=="loan");
            if(loan==null||amount<=0)return 0;
            float paid=Mathf.Min(amount,loan.value);loan.value-=paid;
            CSEvent("W15","REPAY",loan.root,value:paid);
            if(loan.value<=.00001f)CS.effects.Remove(loan);
            return paid;
        }
        bool Crossfire(ClassSkillEffect ballista,EnemyState enemy)
        {
            var shot=Fx("A15:shot:"+enemy.id);
            return shot!=null&&Vector2.Angle(shot.position-enemy.position,ballista.position-enemy.position)>=N("A15","crossfireAngle")&&Map.ProjectileClear(ballista.position,enemy.position,.25f);
        }
        void FireBallista(ClassSkillEffect f,ClassSkillCast a)
        {
            var enemy=Nearby(f.position,f.radius,100).Where(Perceived).OrderByDescending(e=>Crossfire(f,e))
                .ThenByDescending(e=>CombatEffects.Has(e,StatusKind.Mark)).ThenBy(e=>(e.position-f.position).sqrMagnitude).ThenBy(e=>e.id).FirstOrDefault();
            if(enemy==null)return;
            bool cross=Crossfire(f,enemy);float power=N(a.id,"coefficient")+(cross?N(a.id,"crossfireCoefficient"):0);
            Arrow(a,f.position,f.position+(enemy.position-f.position).normalized*f.radius,power,0,true,pierce:cross&&Passive("AP17")?2:1);
            if(cross)CSEvent(a.id,"CROSSFIRE",a.root,enemy.id);
        }
        void LeaveAfterimage()
        {
            var pursuit=Fx("A18");if(pursuit==null||pursuit.count>=pursuit.limit||Vector2.Distance(State.position,pursuit.position)<N("A18","trailMeters"))return;
            var echoes=CS.effects.Where(e=>e.kind=="afterimage"&&e.source=="A18").OrderBy(e=>e.created).ToArray();
            if(echoes.Length>=2)CS.effects.Remove(echoes[0]);
            CS.effects.Add(new ClassSkillEffect{id="A18:echo:"+State.nextId++,source="A18",kind="afterimage",root=pursuit.root,position=pursuit.position,created=State.time,until=pursuit.until});
            pursuit.position=State.position;pursuit.count++;CSEvent("A18","AFTERIMAGE",pursuit.root,value:pursuit.count);
        }
        void IdentityDirectHit(string id,int root,EnemyState enemy,Vector2 origin)
        {
            if(Hero.heroClass==HeroClass.Warrior&&!IdentityReady("W15")&&Vector2.Distance(origin,enemy.position)<=3&&Once("W15:hit",root))RepayResource(N("W15","hitRepayment"));
            if(Hero.heroClass!=HeroClass.Ranger||!new[]{"BASIC","A01","A02","A07","A08","A13","A14"}.Contains(id))return;
            if(Fx("A15")!=null){var shot=Buff("A15:shot:"+enemy.id,N("A15","shotWindow"),root:root,position:origin);shot.source="A15";}
            var echo=CS.effects.Where(e=>e.kind=="afterimage"&&e.until>State.time&&Vector2.Distance(e.position,enemy.position)<=10&&Map.ProjectileClear(e.position,enemy.position,.25f)).OrderBy(e=>e.created).FirstOrDefault();
            if(echo==null||!Proc("A18",1,root))return;
            var ultimate=Cast(echo.root);CS.effects.Remove(echo);
            if(ultimate==null)return;
            Arrow(ultimate,echo.position,enemy.position,N("A18","coefficient"),5,true);
        }
        void PrepareReactiveShield(ClassSkillCast a)
        {
            if(a.element>=1&&a.element<=3)CS.lastElement=a.element;
            if(a.id=="M05"){var memory=Buff("M05:reaction",4,root:a.root,kind:"reactionShield");memory.element=CS.lastElement;}
        }
        void ReactShield(ShieldEffect shield,float absorbed)
        {
            if(!ClassSkillsActive||shield.definitionId!="M05"||absorbed<=0)return;
            var reaction=Fx("M05:reaction");if(reaction==null||reaction.root!=shield.rootCastId||reaction.consumed)return;
            reaction.consumed=true;reaction.position=State.position;var a=Cast(reaction.root);if(a==null||reaction.element==0)return;
            reaction.count=1;
            if(reaction.element==3)Resource(N("M05","reactionResource")*a.rankScale,"M05",a.root);
            else foreach(var enemy in Nearby(State.position,N("M05","reactionRadius")))
            {
                if(reaction.element==1)Dot("M05",enemy,N("M05","reactionBurn"),1,3,a);
                else ApplyStatus(enemy,StatusKind.Root,"M05",N("M05","reactionRoot"),a.root);
            }
            CSEvent("M05","ELEMENT_REACTION",a.root,value:reaction.element);
        }
        void TickReturningGlobe(ClassSkillEffect f,ClassSkillCast a)
        {
            if(a==null)return;
            Vector2 from=f.position;float distance=Mathf.Min(f.extra,N("M10","speed")*classDelta);
            Vector2 wanted=from+f.direction*distance,to=Map.MoveDirect(from,wanted,distance,.05f);
            bool wall=Vector2.Distance(to,wanted)>.02f;f.position=to;f.extra-=Vector2.Distance(from,to);
            foreach(var hit in State.enemies.Where(e=>!e.dead&&!f.targets.Contains(e.id)&&Map.LineClear(from,e.position))
                .Select(e=>new{enemy=e,entry=Entry(from,to,e.position,N("M10","width")*.5f+(e.boss?1.2f:.4f))})
                .Where(h=>!float.IsInfinity(h.entry)).OrderBy(h=>h.entry).ThenBy(h=>h.enemy.id).Take(Mathf.Max(0,f.limit-f.targets.Count)))
            {
                var enemy=hit.enemy;f.targets.Add(enemy.id);bool slowed=CombatEffects.Has(enemy,StatusKind.Slow);
                bool frozen=enemy.statuses.Any(s=>s.definitionId=="M09"&&s.kind==StatusKind.Freeze&&s.remaining>0);
                SkillHit(a,enemy,N("M10","coefficient"),2,attackOrigin:f.origin);
                if(f.count==1&&slowed)ApplyStatus(enemy,StatusKind.Freeze,"M10",N("M10","freezeSeconds"),a.root);
                else ApplyStatus(enemy,StatusKind.Slow,"M10",N("M10","slowSeconds"),a.root,N("M10","slowFraction"));
                if(frozen&&Gear("DES_LM42")&&Once("DES_LM42",a.root)&&Interval("DES_LM42",3))Splash(a,enemy.position,2,.6f,2,secondary:true,source:"DES_LM42");
            }
            if(!wall&&f.extra>.01f)return;
            if(f.count==0)
            {
                f.count=1;f.targets.Clear();f.origin=to;f.direction=(a.origin-to).normalized;f.extra=Vector2.Distance(to,a.origin);
                CSEvent("M10","RETURN",a.root);return;
            }
            if(!wall&&Vector2.Distance(State.position,a.origin)<=N("M10","pickupRadius"))Resource(N("M10","resourceGain")*a.rankScale,"M10",a.root);
            else CSEvent("M10","MISSED_PICKUP",a.root);
            CS.effects.Remove(f);
        }
        bool MoveReturningGlobe(float dt)
        {
            if(!ClassSkillsActive||!Hero.useEdict||HeroActionBusy||InDanger||EdictDodgeHolding)return false;
            var globe=Fx("M10");if(globe==null||globe.kind!="returnGlobe"||globe.count!=1||UseChoice("M10")?.rule=="CONTROLLED")return false;
            var a=Cast(globe.root);if(a==null||Vector2.Distance(State.position,a.origin)<=N("M10","pickupRadius")*.8f||Vector2.Distance(State.position,a.origin)>3||DangerAt(a.origin,1.5f)||!Map.LineClear(State.position,a.origin))return false;
            Vector2 point=Map.Move(State.position,a.origin,MovementSpeed(false)*dt,0,State.time);
            if(DangerAt(point,1.5f))return false;
            float moved=Vector2.Distance(point,State.position);State.position=point;State.destination=a.origin;State.moveDistance+=moved;PolicyRecord("M10").movementMeters+=moved;
            return moved>.0001f;
        }
        EnemyState ChainRouteTarget(Vector2 previous,int previousId,IDictionary<int,int> visits,IEnumerable<EnemyState> observed,ref bool usedConduit,out bool relayed)
        {
            relayed=false;var seen=observed.ToArray();var next=NextChainTarget(previous,previousId,visits,seen);if(next!=null||usedConduit)return next;
            var rod=Fx("M12");if(rod==null||rod.kind!="conduit"||Vector2.Distance(previous,rod.position)>4||!Map.LineClear(previous,rod.position))return null;
            next=NextChainTarget(rod.position,previousId,visits,seen);
            if(next==null)return null;usedConduit=relayed=true;return next;
        }
        void ConduitRelay(int root)
        {
            var rod=Fx("M12");if(rod==null)return;CSEvent("M12","RELAY",root,value:1);
            if(Once("M12:resource",root))Resource(N("M12","resourceGain")*RankScale("M12"),"M12",root);
            if(Gear("DES_LM43")&&Fx("M11") is ClassSkillEffect orb&&Vector2.Distance(orb.position,rod.position)<=3&&Once("DES_LM43",root))orb.extra=Mathf.Min(3,orb.extra+1);
        }
    }
}
