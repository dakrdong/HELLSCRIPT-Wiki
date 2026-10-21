using System;
using System.Linq;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        bool D4Combat=>SkillTreeD4.Enabled&&Loadout?.version==5;
        static bool AtHealthFraction(float health,float maximum,float fraction)
        {float ratio=maximum>0?health/maximum:0;return ratio<=fraction||UnityEngine.Mathf.Approximately(ratio,fraction);}
        int D4BaseRank(string id)
        {
            int rank=Loadout.Rank(id);var data=SkillTreeD4.For(Hero.heroClass).actives.FirstOrDefault(a=>a.id==id);
            if(data==null||data.ultimate||rank<=0)return rank;
            return rank+Stats.ClassSkillBonus(id,Loadout);
        }
        float D4Scale(string id,int rank)
        {
            if(rank<=0)return 0;var data=SkillTreeD4.For(Hero.heroClass).actives.FirstOrDefault(a=>a.id==id);
            if(data==null)return 1;
            // Damage/shield growth is data-driven. Other rows have independent accessors.
            if(data.ultimate||data.stats[0].value.unit=="D%")return (float)(SkillTreeD4Values.Active(id,0,rank)/data.stats[0].value.from);
            return 1;
        }
        float D4BaseValue(string id,int stat,ClassSkillCast cast=null)
            =>(float)SkillTreeD4Values.Active(id,stat,cast?.baseRank??D4BaseRank(id));
        float D4LegacyRankPower(int index,ClassSkillCast cast=null)
        {
            if(index/6!=(int)Hero.heroClass)return 0;string id=ClassSkills.LegacyId(index);
            var active=SkillTreeD4.For(Hero.heroClass).actives.FirstOrDefault(a=>a.id==id);if(active==null||Loadout.Rank(id)==0)return 0;
            int stat=Array.FindIndex(active.stats,s=>s.value.unit=="D%");if(stat<0)return 0;
            return (float)(SkillTreeD4Values.Active(id,stat,cast?.id==id?cast.baseRank:D4BaseRank(id))/active.stats[stat].value.from-1)*100;
        }
        float D4ModValue(string id,ClassSkillCast cast=null)
        {
            int mod=cast?.mod??Loadout.Modifier(id),rank=cast?.modRank??Loadout.ModifierRank(id);
            return mod==0||rank==0?0:(float)SkillTreeD4Values.Modifier(id+"-"+mod,rank);
        }
        float D4Number(string id,string key,float original,ClassSkillCast cast)
        {
            int mod=cast?.mod??Loadout.Modifier(id);
            if(Definition(id)?.Ultimate==true&&key=="cooldownSeconds")
            {
                var cooldown=SkillTreeD4.For(Hero.heroClass).actives.Where(a=>a.ultimate).SelectMany(a=>a.stats).First(s=>s.en=="Cooldown").value;
                return (float)SkillTreeD4Values.At(cooldown,cast?.baseRank??Loadout.Rank(id));
            }
            // First shared-frame acceptance: shape and short bleed of Spinning Rend.
            if(id=="W09"&&mod==3)
            {if(key=="width")return 360;if(key=="duration")return D4ModValue(id,cast);}
            return Hero.heroClass==HeroClass.Warrior?D4WarriorNumber(id,key,original,cast):Hero.heroClass==HeroClass.Ranger?D4RangerNumber(id,key,original,cast):D4MageNumber(id,key,original,cast);
        }
        bool D4SourceAvailable(string source,int mod=0)
        {
            string id=source.Split(':')[0];var data=SkillTreeD4.For(Hero.heroClass);
            if(id.Contains('-'))
            {
                int dash=id.IndexOf('-');if(!int.TryParse(id.Substring(dash+1),out mod))return false;id=id.Substring(0,dash);
            }
            if(data.passives.Any(p=>p.id==id))return Passive(id);
            if(data.actives.Any(a=>a.id==id))return Loadout.Equipped(id)&&(mod==0||Loadout.Modifier(id)==mod);
            // Named generic counters remain valid; unknown legacy skill ids do not.
            return LegacySkillWire.Find(id)==null&&ClassSkills.Find(id)==null;
        }
        float D4FormArea=>D4Combat&&Hero.heroClass==HeroClass.Warrior&&Fx("W18:form")!=null?1.5f:1;
        float D4ProjectedArea(string id)=>D4FormArea*((id=="W03"||id=="W04")&&State.resource/Stats.maxResource>=.8f&&Set("REF_SW08",4)?Set("REF_SW08",6)?1.35f:1.2f:1);
        void PresentD4LegacyRelease(HeroActionState action)
        {
            if(action.skill==6||action.skill==7||action.skill==12||action.skill==13)return;
            var skill=catalog.skills[action.skill];var cast=Cast(action.id);float radius=skill.radius;
            if(cast?.id=="W03"&&cast.mod==1)radius=D4ModValue("W03",cast);
            if(cast?.id=="W04"&&cast.mod==1)radius=D4ModValue("W04",cast);
            Visual?.Invoke(action.origin,action.skill==1||action.skill==9||action.skill==15?action.destination:action.aim,action.skill,radius*(cast?.area??1));
        }
    }
}
