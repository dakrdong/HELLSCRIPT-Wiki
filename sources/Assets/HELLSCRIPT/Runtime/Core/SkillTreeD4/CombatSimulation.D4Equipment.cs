using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        void CaptureD4Equipment(ClassSkillCast cast)
        {
            if(cast.id=="W04"&&cast.mod==2&&Gear("DES_LW44")&&Fx("DES_LW44") is ClassSkillEffect wave&&!wave.consumed)
            {wave.consumed=true;cast.legendary+=.25f;}
        }
        void AfterD4EquipmentCast(ClassSkillCast cast)
        {
            if(cast.id=="W02"&&cast.mod==1&&Gear("DES_LW43"))Buff("DES_LW43",5,root:cast.root,kind:"charge");
            if(cast.id=="W05"&&cast.mod==1&&Gear("DES_LW43")&&Fx("DES_LW43") is ClassSkillEffect wall&&!wall.consumed)
            {wall.consumed=true;Barrier("DES_LW43",.08f,4,cast.root);}
            if(cast.id=="W05"&&cast.mod==2&&Gear("DES_LW45")&&Fx("W06:regen-pause")!=null&&State.resource>0&&Proc("DES_LW45",8,cast.root))
            {float paid=Mathf.Min(8,State.resource);State.resource-=paid;Barrier("DES_LW45",paid*.01f,4,cast.root);}
            if(cast.id=="A03"&&cast.mod==3&&Gear("DES_LA45")&&Fx("DES_LA45") is ClassSkillEffect prepared&&!prepared.consumed)
            {var turret=CS.effects.FirstOrDefault(e=>e.kind=="turret"&&e.root==cast.root);if(turret!=null){prepared.consumed=true;turret.until+=2;turret.limit+=2;}}
        }
        void AfterD4EquipmentHit(EnemyState enemy,DamageEvent damage,ClassSkillCast cast)
        {
            string id=damage.definitionId;bool direct=damage.kind==DamageKind.Direct;
            if(direct&&id=="W08"&&cast.mod==3&&Gear("DES_LW44")&&Once("DES_LW44",cast.root))Buff("DES_LW44",5,root:cast.root,kind:"charge");
            if(direct&&Normal(id)&&Gear("DES_LW46")&&Fx("W18:form") is ClassSkillEffect form&&Once("DES_LW46",form.root))Resource(20,"DES_LW46",form.root);
            if(id=="A03"&&cast.mod==2&&damage.kind==DamageKind.Periodic&&Gear("DES_LA42")&&HasDot("A06:poison",enemy)&&Proc("DES_LA42:"+enemy.id,4,cast.root,enemy.id))
                Dot("DES_LA42",enemy,.9f,4,3,cast);
            if(id=="A17"&&Gear("DES_LA46")&&CombatEffects.Has(enemy,StatusKind.Mark)&&Once("DES_LA46",cast.root))ApplyStatus(enemy,StatusKind.Slow,"DES_LA46",2,cast.root,.3f);
            if(direct&&id=="M07")
            {
                if(Gear("DES_LM44")&&InWard()&&Proc("DES_LM44",6,cast.root))Barrier("DES_LM44",.1f,4,cast.root);
                if(cast.mod==3&&Gear("DES_LM43"))ChargeD4EquipmentOrb(enemy.position,cast.root);
            }
            if(direct&&id=="M02"&&cast.mod==3&&Gear("DES_LM42")&&Fx("DES_LM42:freeze:"+enemy.id)!=null&&Proc("DES_LM42",3,cast.root))
                Splash(cast,enemy.position,2,.6f,2,secondary:true,source:"DES_LM42");
            if(direct&&id=="M17"&&Gear("DES_LM46")&&CS.effects.Any(e=>e.kind=="d4collapse"&&e.root==cast.root&&e.count==3)&&Once("DES_LM46",cast.root))Resource(15,"DES_LM46",cast.root);
            if(direct&&id=="M01")
            {
                ConsumeFrostMark(enemy,new CombatProjectile{actionId=cast.root,snapshot=cast.snapshot});
                if(Gear("LM02")&&Once("LM02",cast.root))ScheduleExtra(cast,"LM02",enemy,AspectGrowth.SnapshotValue(cast.snapshot,"LM02",.7f),1,.4f,3.5f);
            }
        }
        void ChargeD4EquipmentOrb(Vector2 point,int root)
        {
            var orb=CS.effects.Where(e=>e.kind=="d4orb"&&e.until>State.time&&Vector2.Distance(e.position,point)<=e.radius).OrderBy(e=>e.created).ThenBy(e=>e.id).FirstOrDefault();
            if(orb!=null&&orb.extra<3&&Once("DES_LM43",root))orb.extra++;
        }
        void D4EquipmentUltimateHit(DamageEvent hit,int root)
        {if(hit?.hpLoss>0&&Gear("DES_LA46")&&Once("DES_LA46",root))Barrier("DES_LA46",.12f,4,root);}
        bool D4LeapRecharging=>D4Combat&&Loadout.Modifier("W02")==3&&CS.charges?.Any(c=>c.id=="W02"&&c.count<2)==true;
        void ReduceD4LeapCooldown(float seconds)
        {
            var charge=CS.charges?.FirstOrDefault(c=>c.id=="W02");
            if(charge!=null&&charge.count<2){charge.next=Mathf.Max(State.time,charge.next-seconds);SyncD4LeapCharges();}
        }
    }
}
