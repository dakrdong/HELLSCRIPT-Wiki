using System;
using System.Collections.Generic;
using System.Linq;

namespace Hellscript
{
    public static class SkillTreeD4Loadout
    {
        public static ClassSkillLoadout Empty(HeroClass hero)
        {
            var data=SkillTreeD4.For(hero);
            return new ClassSkillLoadout{format="CLASS_SKILLS_V1",version=5,heroClass=(int)hero,order=new[]{"BASIC"},automatic=new[]{"BASIC"},
                ranks=data.actives.Select(a=>a.id).Concat(data.passives.Select(p=>p.id)).Select(id=>new SkillInvestment{id=id,rank=0,modifier=-1,modifierRank=0}).ToList()};
        }
        public static ClassSkillLoadout Canonical(ClassSkillLoadout source)
        {
            if(source==null||source.version!=5||source.format!="CLASS_SKILLS_V1"||source.heroClass<0||source.heroClass>2)throw new ArgumentException("Unsupported D4 loadout.");
            var d=source.Copy();var c=SkillTreeD4.For((HeroClass)d.heroClass);
            d.kitKind??="";d.kitSkills??=Array.Empty<string>();
            if(d.kitKind!=""&&d.kitKind!="prologue"||d.kitKind==""&&(d.kitLevel!=0||d.kitSkills.Length!=0)||d.kitSkills.Distinct().Count()!=d.kitSkills.Length||d.kitSkills.Any(id=>!c.actives.Any(a=>a.id==id&&!a.ultimate)&&!c.passives.Any(p=>p.id==id&&!p.key)))throw new ArgumentException("Invalid D4 kit allowance.");
            if(d.kitKind=="prologue"&&(d.kitLevel!=0||!d.kitSkills.SequenceEqual(new[]{Tutorials.PrologueSkill((HeroClass)d.heroClass).skill})))throw new ArgumentException("Unknown authored D4 kit.");
            string[] ids=c.actives.Select(a=>a.id).Concat(c.passives.Select(p=>p.id)).ToArray();
            if(d.ranks==null||d.ranks.Count!=20||d.ranks.Any(r=>r==null)||!d.ranks.Select(r=>r.id).SequenceEqual(ids))throw new ArgumentException("D4 ranks must contain the 20 class entries in catalog order.");
            for(int i=0;i<20;i++)
            {
                var r=d.ranks[i];int maximum=i<10?c.actives[i].maxRank:5;
                if(r.rank<0||r.rank>maximum||r.modifierRank<0||r.modifierRank>r.rank||r.modifierRank>maximum||r.modifier< -1||r.modifier>=(i<10?c.actives[i].modifiers.Length:0)||(r.modifier==-1)!=(r.modifierRank==0))
                    throw new ArgumentException("Invalid D4 rank or modifier: "+r.id);
            }
            if(d.actives==null||d.actives.Length!=4||d.actives.Any(id=>id==null||id!=""&&!c.actives.Any(a=>a.id==id&&!a.ultimate))||d.actives.Where(id=>id!="").Distinct().Count()!=d.actives.Count(id=>id!="")||d.passives==null||d.passives.Length!=3||d.passives.Any(id=>id!=""))throw new ArgumentException("Invalid D4 equipment slots.");
            if(d.ultimate==null||d.ultimate!=""&&!c.actives.Any(a=>a.id==d.ultimate&&a.ultimate))throw new ArgumentException("Invalid D4 ultimate.");
            var equipped=d.actives.Where(id=>id!="").Concat(d.ultimate==""?Array.Empty<string>():new[]{d.ultimate}).ToArray();
            if(equipped.Any(id=>d.Rank(id)==0))throw new ArgumentException("Unlearned D4 skills cannot be equipped.");
            var actions=equipped.Concat(new[]{"BASIC"}).ToArray();
            if(d.order==null||d.order.Length!=actions.Length||d.order.Distinct().Count()!=actions.Length||d.order.Any(id=>!actions.Contains(id))||d.automatic==null||d.automatic.Distinct().Count()!=d.automatic.Length||d.automatic.Any(id=>!actions.Contains(id)))throw new ArgumentException("Invalid D4 action order.");
            if(HuntEdictV2Storage.IsAbsent(d.legacyEdict))d.legacyEdict=null;
            else d.legacyEdict=HuntEdictV2.Canonical(d.legacyEdict);
            if(d.legacyEdict!=null&&d.legacyEdict.heroClass!=HuntEdict.ClassId((HeroClass)d.heroClass))throw new ArgumentException("Different D4 edict class.");
            if(d.options==null||d.presetSelections==null||d.options.Any(o=>o==null||!KeptScope(d.heroClass,o.id.Split(':')[0]))||d.presetSelections.Any(p=>p==null||p.scope.StartsWith("skill/",StringComparison.Ordinal)&&!KeptScope(d.heroClass,p.scope.Substring(6))))throw new ArgumentException("A removed skill remains in a D4 policy.");
            SkillTacticsStorage.Validate(d);ClassSkillOptions.Validate(d);HuntEdictSkillPresets.ValidateSelections(d);return d;
        }
        public static bool KeptScope(int heroClass,string id)=>id=="BASIC"||SkillTreeD4.For((HeroClass)heroClass).actives.Any(a=>a.id==id)||SkillTreeD4.For((HeroClass)heroClass).passives.Any(p=>p.id==id);
        public static SkillTreeD4Alloc Allocation(ClassSkillLoadout load)
            =>new SkillTreeD4Alloc{Actives=load.ranks.Take(10).Select(r=>new SkillTreeD4ActiveAlloc{Rank=r.rank,Modifier=r.modifier,ModifierRank=r.modifierRank}).ToArray(),Passives=load.ranks.Skip(10).Select(r=>r.rank).ToArray()};
        public static void SetAllocation(ClassSkillLoadout load,SkillTreeD4Alloc allocation)
        {
            for(int i=0;i<10;i++){var r=load.ranks[i];var a=allocation.Actives[i];r.rank=a.Rank;r.modifier=a.Modifier;r.modifierRank=a.ModifierRank;load.ranks[i+10].rank=allocation.Passives[i];}
        }
        public static void Validate(ClassSkillLoadout source,HeroSave hero)
        {
            var d=Canonical(source);var c=SkillTreeD4.For(hero.heroClass);var a=Allocation(d);
            if(d.kitKind!="")
            {if(d.heroClass!=(int)hero.heroClass||hero.level<1||hero.level>60||d.ranks.Any(r=>r.rank>1||r.modifierRank!=0||r.rank>0&&!d.kitSkills.Contains(r.id))||d.ultimate!="")throw new ArgumentException("Invalid authored D4 kit allocation.");return;}
            if(d.heroClass!=(int)hero.heroClass||hero.level<1||hero.level>60||SkillTreeD4.Rules.Spent(c.hero,a)>hero.level||!SkillTreeD4.Rules.Valid(c.hero,a)||c.passives.Where((p,i)=>a.Passives[i]>0&&p.unlockLevel>hero.level).Any()||c.passives.Where((p,i)=>p.key&&a.Passives[i]>0).Count()>1)throw new ArgumentException("Invalid D4 point allocation or hero level.");
        }
        public static HashSet<string> Unlocked(ClassSkillLoadout source,int level)
        {
            var c=SkillTreeD4.For((HeroClass)source.heroClass);var a=Allocation(source);var result=new HashSet<string>{"BASIC"};
            if(level<1)return result;
            if(!string.IsNullOrEmpty(source.kitKind)){result.UnionWith(source.kitSkills);return result;}
            foreach(var s in c.actives)if(SkillTreeD4.Rules.Before(c.hero,a,s.tier-1)>=c.tiers[s.tier-1].gate)result.Add(s.id);
            foreach(var p in c.passives)if(level>=p.unlockLevel)result.Add(p.id);
            return result;
        }
        public static ClassSkillLoadout ChangeRank(ClassSkillLoadout source,HeroSave hero,string id,int delta)
        {
            if(Math.Abs(delta)!=1)throw new ArgumentException("A D4 point change must be one point.");
            var d=Canonical(source);var c=SkillTreeD4.For(hero.heroClass);SkillTreeD4Selection selection;
            if(d.kitKind!="")
            {var entry=d.ranks.FirstOrDefault(r=>r.id==id);if(entry==null||!d.kitSkills.Contains(id)||entry.rank+delta<0||entry.rank+delta>1)throw new ArgumentException("This skill is outside the authored kit.");entry.rank+=delta;d.actives=d.actives.Select(s=>s==id&&entry.rank==0?"":s).ToArray();ClassSkillTree.NormalizeActions(d);Validate(d,hero);return d;}
            int active=Array.FindIndex(c.actives,a=>a.id==id),passive=Array.FindIndex(c.passives,p=>p.id==id);
            if(active>=0)selection=SkillTreeD4Selection.Active(active);
            else if(passive>=0)selection=SkillTreeD4Selection.PassiveAt(passive);
            else
            {
                active=Array.FindIndex(c.actives,a=>a.modifiers.Any(m=>m.id==id));
                if(active<0)throw new ArgumentException("Unknown D4 point target.");
                selection=SkillTreeD4Selection.ModifierOf(active,Array.FindIndex(c.actives[active].modifiers,m=>m.id==id));
            }
            var change=SkillTreeD4.Rules.Apply(delta>0?SkillTreeD4Action.Invest:SkillTreeD4Action.Refund,c.hero,hero.level,Allocation(d),selection);
            if(!change.Ok)throw new ArgumentException("D4 point change blocked: "+change.Reason);
            SetAllocation(d,change.Alloc);
            d.actives=d.actives.Select(s=>s!=""&&d.Rank(s)==0?"":s).ToArray();if(d.ultimate!=""&&d.Rank(d.ultimate)==0)d.ultimate="";
            ClassSkillTree.NormalizeActions(d);Validate(d,hero);return d;
        }
        public static ClassSkillLoadout PolicyOnly(ClassSkillLoadout source,HeroClass hero)
        {
            var old=ClassSkillLoadout.Canonical(source);var d=Empty(hero);
            if(old.heroClass!=(int)hero)throw new ArgumentException("Different class in old skill policy.");
            d.legacyEdict=old.legacyEdict?.Copy();
            d.options=old.options.Where(o=>KeptScope(d.heroClass,o.id.Split(':')[0])).Select(o=>new ClassSkillOptionSelection{id=o.id,choice=o.choice}).ToList();
            d.presetSelections=old.presetSelections.Where(p=>!p.scope.StartsWith("skill/",StringComparison.Ordinal)||KeptScope(d.heroClass,p.scope.Substring(6))).Select(p=>new SkillPresetSelection{scope=p.scope,preset=p.preset}).ToList();
            d.automatic=old.automatic.Contains("BASIC")?new[]{"BASIC"}:Array.Empty<string>();return Canonical(d);
        }
        public static int ModifierMask(ClassSkillLoadout load)
        {int mask=0;if(load?.version==5)for(int i=0;i<10;i++)mask|=(load.ranks[i].modifier+1)<<(i*2);return mask;}
        public static bool SourceMatches(ClassSkillLoadout load,string scope,string id,int capturedMask=-1)
        {
            if(scope=="*"||scope==id)return true;if(load?.version!=5)return false;var active=SkillTreeD4.For((HeroClass)load.heroClass).actives.FirstOrDefault(a=>a.id==id);var modifier=active?.modifiers.FirstOrDefault(m=>m.id==scope);
            if(modifier==null)return false;
            int index=Array.FindIndex(SkillTreeD4.For((HeroClass)load.heroClass).actives,a=>a.id==id);int chosen=capturedMask<0?load.Modifier(id):(capturedMask>>(index*2))&3;
            return index>=0&&chosen==modifier.id[modifier.id.Length-1]-'0';
        }
        public static ClassSkillLoadout ReleaseKit(ClassSkillLoadout source,HeroSave hero)
        {
            if(source?.version!=5||string.IsNullOrEmpty(source.kitKind))return source;
            // The kit goes first, so a kit no longer authored (the retired puzzle levels) can still be released.
            var load=source.Copy();load.kitKind="";load.kitLevel=0;load.kitSkills=Array.Empty<string>();load=ClassSkillTree.ResetAllocation(load);Validate(load,hero);return load;
        }
        public static void ValidateKitOwner(AccountSave account,HeroSave hero)
        {
            var load=hero.build.classSkills;if(load?.version!=5||string.IsNullOrEmpty(load.kitKind))return;
            var slot=account.guide.tutorialSlots.FirstOrDefault(s=>s.heroId==hero.id);bool selected=account.Hero.id==hero.id;
            bool complete=selected?account.guide.mapComplete:slot?.mapComplete??false;
            if(account.guide.legacyExempt||complete)
                throw new NotSupportedException("A skill kit has no active authored tutorial owner. The original save is preserved.");
        }
        public static ClassSkillLoadout PrologueKit(HeroSave hero)
        {
            var load=Empty(hero.heroClass);load.kitKind="prologue";load.kitSkills=new[]{Tutorials.PrologueSkill(hero.heroClass).skill};return Canonical(load);
        }
    }
}
