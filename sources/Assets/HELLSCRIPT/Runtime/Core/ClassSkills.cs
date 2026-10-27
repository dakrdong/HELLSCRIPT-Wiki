using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    [Serializable] public sealed class SkillNumber { public string key; public float value; }
    [Serializable] public sealed class ClassSkillDefinition
    {
        public string id,category,name,nameEn,description,descriptionEn,automatic,automaticEn,element;
        public int heroClass,order,unlock,maxRank;
        public string resultMetric,resultSecondaryMetric;
        public SkillResultMetric ResultMetric=>string.IsNullOrEmpty(resultMetric)?SkillResultMetric.Damage:(SkillResultMetric)Enum.Parse(typeof(SkillResultMetric),resultMetric);
        public SkillResultMetric? SecondaryResultMetric=>string.IsNullOrEmpty(resultSecondaryMetric)?null:(SkillResultMetric)Enum.Parse(typeof(SkillResultMetric),resultSecondaryMetric);
        public bool legacy;
        public SkillNumber[] parameters;
        public float Number(string key,float fallback=0)=>parameters?.FirstOrDefault(p=>p.key==key)?.value??fallback;
        public string Name=>Loc.Language=="en"?nameEn:name;
        public string Description=>Loc.Language=="en"?descriptionEn:description;
        public bool Ultimate=>category=="ultimate";
        public bool Passive=>category=="passive";
    }
    [Serializable] public sealed class ClassGearDefinition
    {
        public string id,setId,name,nameEn,description,descriptionEn;
        public int heroClass,slot;
        public UniqueItemDefinition Item=>new UniqueItemDefinition(id,name,heroClass,slot,0,description,setId:setId,nameEn:nameEn,descriptionEn:descriptionEn);
    }
    [Serializable] public sealed class ClassSetBonus
    {
        public string id,setId,description,descriptionEn;
        public int count;
        public SkillNumber[] parameters;
        public float Number(string key)=>parameters.FirstOrDefault(p=>p.key==key)?.value??0;
    }
    [Serializable] public sealed class ClassSkillReason {public string id,ko,en;}
    [Serializable] public sealed class ClassSkillData
    {public int version;public bool playerEnabled;public ClassSkillReason[] reasons;public ClassSkillDefinition[] skills;public ClassGearDefinition[] gear;public ClassSetBonus[] bonuses;public ClassSkillValidationBuild[] validationBuilds;public ClassSkillOptionDefinition[] options;}
    public static class ClassSkills
    {
        public static float BasicReach(HeroClass hero)=>hero==HeroClass.Warrior?2f:10f;
        static ClassSkillData data;
        public static ClassSkillData Data
        {
            get
            {
                if(data!=null)return data;
                var asset=Resources.Load<TextAsset>("Data/ClassSkills");
                if(asset==null)throw new InvalidOperationException("Missing class-skill catalog.");
                var value=JsonUtility.FromJson<ClassSkillData>(asset.text);
                if(value.version!=2||value.skills.Length!=144||value.skills.Select(s=>s.id).Distinct().Count()!=144||value.gear.Length!=123||value.bonuses.Length!=60)
                    throw new NotSupportedException("Unsupported class-skill catalog.");
                foreach(var s in value.skills)
                    if(s.heroClass<0||s.heroClass>2||s.unlock<1||s.unlock>60||string.IsNullOrEmpty(s.nameEn)||string.IsNullOrEmpty(s.descriptionEn)||s.parameters.Any(p=>float.IsNaN(p.value)||float.IsInfinity(p.value)))
                        throw new InvalidOperationException("Invalid skill: "+s.id);
                return data=value;
            }
        }
        public static bool Enabled(HeroSave hero)=>ClassSkillTree.Uses(hero?.build?.classSkills);
        public static bool ValidationBuild=>Application.isEditor||Debug.isDebugBuild;
        public static int LevelCap(HeroSave hero)=>SkillTreeD4.Enabled&&hero?.classSkillValidation!=true?60:Enabled(hero)?40:30;
        public static IEnumerable<ClassSkillDefinition> For(HeroClass hero)=>Data.skills.Where(s=>s.heroClass==(int)hero).OrderBy(s=>s.order);
        public static ClassSkillDefinition Find(string id)=>Data.skills.FirstOrDefault(s=>s.id==id);
        public static IEnumerable<ClassSkillDefinition> ForLoadout(HeroClass hero,ClassSkillLoadout load)=>load?.version==5?SkillTreeD4Definitions.For(hero):For(hero);
        public static ClassSkillDefinition FindForLoadout(string id,ClassSkillLoadout load)=>load?.version==5?SkillTreeD4Definitions.Find(id):Find(id);
        public static float RankScale(string id,int rank)=>rank<=0?0:Find(id)?.Ultimate==true?1:1+.1f*(rank-1);
        public static string LegacyId(int index)=>index<0?"BASIC":index==16?"M04-3":index<18?"WAM"[index/6]+(index%6+1).ToString("00"):throw new ArgumentOutOfRangeException(nameof(index));
        public static int LegacyIndex(string id)
        {for(int n=0;n<18;n++)if(LegacyId(n)==id)return n;return -1;}
        public static int ElementOf(string id)
        {switch(Find(id)?.element){case "Fire":return 1;case "Cold":return 2;case "Lightning":return 3;case "Poison":return 4;case "Shadow":return 5;default:return 0;}}
    }
    [Serializable] public sealed class SkillInvestment {public string id;public int rank=1,modifier=-1,modifierRank;}
    [Serializable] public sealed class ClassSkillLoadout
    {
        public string format;
        public int version=2,heroClass;
        public string[] actives={"","","",""},passives={"","",""},order=Array.Empty<string>(),automatic=Array.Empty<string>();
        public string ultimate="";
        public string kitKind="";
        public int kitLevel;
        public string[] kitSkills=Array.Empty<string>();
        public List<SkillInvestment> ranks=new List<SkillInvestment>();
        public List<ClassSkillOptionSelection> options=new List<ClassSkillOptionSelection>();
        public List<SkillPresetSelection> presetSelections=new List<SkillPresetSelection>();
        public int tacticsVersion;
        public List<SkillTacticValue> tactics=new List<SkillTacticValue>();
        public HuntEdictV2Document legacyEdict;
        public ClassSkillLoadout Copy()=>JsonUtility.FromJson<ClassSkillLoadout>(JsonUtility.ToJson(this));
        public int Rank(string id)=>ranks.FirstOrDefault(r=>r.id==id)?.rank??(version==5?0:1);
        public int Modifier(string id)=>version==5?(ranks.FirstOrDefault(r=>r.id==id)?.modifier??-1)+1:0;
        public int ModifierRank(string id)=>version==5?ranks.FirstOrDefault(r=>r.id==id)?.modifierRank??0:0;
        public bool Equipped(string id)=>id=="BASIC"||actives.Contains(id)||passives.Contains(id)||ultimate==id;
        public static bool IsAbsent(ClassSkillLoadout value)=>value==null||value.version<=2&&string.IsNullOrEmpty(value.format)&&(value.ranks==null||value.ranks.Count==0)&&(value.actives==null||value.actives.All(string.IsNullOrEmpty))&&(value.passives==null||value.passives.All(string.IsNullOrEmpty))&&string.IsNullOrEmpty(value.ultimate)&&(value.options==null||value.options.Count==0)&&!SkillTacticsStorage.HasData(value);
        public static ClassSkillLoadout FromHero(HeroSave hero)
        {
            if(!IsAbsent(hero.build.classSkills))return Canonical(hero.build.classSkills);
            var d=new ClassSkillLoadout{format="CLASS_SKILLS_V1",heroClass=(int)hero.heroClass,legacyEdict=hero.edict?.Copy()};
            var old=SkillProgression.FromBuild(hero.build);
            d.ranks=LegacySkillWire.For(hero.heroClass).Select(s=>new SkillInvestment{id=s.id,rank=s.legacy?old[SkillProgression.Index(hero.heroClass,s.id)]:1}).ToList();
            for(int i=0;i<Math.Min(4,hero.build.activeSkills.Count);i++)d.actives[i]=LegacySkillWire.SlotId((HeroClass)(hero.build.activeSkills[i]/6),hero.build.activeSkills[i]%6+1);
            for(int i=0;i<Math.Min(3,hero.build.passives.Length);i++)d.passives[i]=SkillProgression.Id(hero.heroClass,hero.build.passives[i]);
            d.order=d.actives.Where(s=>s!="").Concat(new[]{"BASIC"}).ToArray();d.automatic=d.order.Where(id=>hero.edict==null||HuntEdictV2Storage.IsAbsent(hero.edict)||LegacySkillWire.Automatic(hero.edict,id)).ToArray();return Canonical(d);
        }
        public static ClassSkillLoadout Canonical(ClassSkillLoadout source)
        {
            if(source?.version==5)return SkillTreeD4Loadout.Canonical(source);
            if(SkillTacticsStorage.HasData(source))throw new ArgumentException("Tactical settings require their original D4 schema.");
            if(source==null||source.format!="CLASS_SKILLS_V1"||(source.version!=1&&source.version!=2&&source.version!=3&&source.version!=4)||source.heroClass<0||source.heroClass>2)throw new ArgumentException("Unsupported class-skill loadout.");
            var d=source.Copy();var definitions=LegacySkillWire.For((HeroClass)d.heroClass).ToArray();
            if(!string.IsNullOrEmpty(d.kitKind)||d.kitLevel!=0||d.kitSkills?.Length>0)throw new ArgumentException("Legacy loadouts cannot contain D4 kit allowances.");
            if(d.ranks!=null&&d.ranks.Any(r=>r!=null&&(r.modifier!=-1||r.modifierRank!=0)))throw new ArgumentException("Legacy ranks cannot contain D4 modifiers.");
            if(d.version==1)
            {
                string prefix="WAM"[d.heroClass].ToString(),oldId=prefix+"P18",newId=prefix+"P19";
                var legacy=definitions.Where(s=>s.id!=newId).ToArray();
                if(d.ranks==null||d.ranks.Count!=legacy.Length||d.ranks.Select(r=>r?.id).Distinct().Count()!=legacy.Length||d.ranks.Any(r=>r==null||!legacy.Any(s=>s.id==r.id&&r.rank>=1&&r.rank<=s.maxRank)))throw new ArgumentException("Invalid legacy ID-based skill ranks.");
                var oldRank=d.ranks.Single(r=>r.id==oldId);int newRank=1;
                // Transfer the old investment to the selected ultimate's dedicated support.
                // Preserve total spending and never equip an extra passive implicitly.
                if(d.ultimate==prefix+"18")
                {
                    newRank=oldRank.rank;oldRank.rank=1;
                    if(d.passives!=null)d.passives=d.passives.Select(id=>id==oldId?newId:id).ToArray();
                }
                d.ranks.Add(new SkillInvestment{id=newId,rank=newRank});d.version=2;
            }
            bool Valid(string id,string kind)=>id==""||definitions.Any(s=>s.id==id&&s.category==kind);
            void Slots(string[] ids,int count,string kind)
            {if(ids==null||ids.Length!=count||ids.Any(id=>id==null||!Valid(id,kind))||ids.Where(id=>id!="").Distinct().Count()!=ids.Count(id=>id!=""))throw new ArgumentException("Invalid "+kind+" slots.");}
            Slots(d.actives,4,"active");Slots(d.passives,3,"passive");
            if(d.version>=3&&d.passives.Any(id=>id!=""))throw new ArgumentException("Skill-tree passives do not use equipment slots.");
            if(d.ultimate==null||!Valid(d.ultimate,"ultimate"))throw new ArgumentException("Invalid ultimate selection.");
            if(d.ranks==null||d.ranks.Count!=definitions.Length||d.ranks.Select(r=>r?.id).Distinct().Count()!=definitions.Length||d.ranks.Any(r=>r==null||!definitions.Any(s=>s.id==r.id&&r.rank>=ClassSkillTree.MinimumRank(d)&&r.rank<=s.maxRank)))throw new ArgumentException("Invalid ID-based skill ranks.");
            if(d.version==4&&d.actives.Concat(new[]{d.ultimate}).Any(id=>id!=""&&d.Rank(id)==0))throw new ArgumentException("Unlearned skills cannot be equipped.");
            var actions=d.actives.Where(s=>s!="").Concat(d.ultimate==""?Array.Empty<string>():new[]{d.ultimate}).Concat(new[]{"BASIC"}).ToArray();
            if(d.order==null||d.order.Length!=actions.Length||d.order.Distinct().Count()!=actions.Length||d.order.Any(id=>!actions.Contains(id)))throw new ArgumentException("Attack order must contain every equipped action once.");
            if(d.automatic==null||d.automatic.Distinct().Count()!=d.automatic.Length||d.automatic.Any(id=>!actions.Contains(id)))throw new ArgumentException("Invalid automatic skill IDs.");
            if(HuntEdictV2Storage.IsAbsent(d.legacyEdict))d.legacyEdict=null;
            else d.legacyEdict=HuntEdictV2.Canonical(d.legacyEdict);
            if(d.legacyEdict!=null&&d.legacyEdict.heroClass!=HuntEdict.ClassId((HeroClass)d.heroClass))throw new ArgumentException("Different class in preserved edict.");
            LegacySkillWire.ValidateOptions(d);
            LegacySkillWire.ValidatePresets(d);
            d.ranks=d.ranks.OrderBy(r=>LegacySkillWire.Find(r.id).order).ToList();return d;
        }
        public static void Validate(ClassSkillLoadout source,HeroSave hero,bool departing=false)
        {
            if(source?.version==5){SkillTreeD4Loadout.Validate(source,hero);return;}
            var d=Canonical(source);
            if(d.heroClass!=(int)hero.heroClass||hero.level<1||hero.level>40)throw new ArgumentException("Different class or invalid level.");
            if(d.version==3||d.version==4)
            {
                var available=ClassSkillTree.Unlocked(d,hero.level);
                if(d.actives.Concat(new[]{d.ultimate}).Any(id=>id!=""&&!available.Contains(id))||d.ranks.Any(r=>r.rank>ClassSkillTree.MinimumRank(d)&&!available.Contains(r.id)))throw new ArgumentException(Loc.T("아직 해금하지 않은 스킬입니다."));
                if(ClassSkillTree.Remaining(d,hero.level)<0)throw new ArgumentException(Loc.T("스킬 포인트가 부족합니다. 레벨 배분을 조정한 뒤 적용하세요."));
                return;
            }
            foreach(var id in d.actives.Concat(d.passives).Concat(new[]{d.ultimate}).Where(s=>s!=""))if(LegacySkillWire.Find(id).unlock>hero.level)throw new ArgumentException("Skill is not unlocked: "+id);
            if(d.ranks.Sum(r=>r.rank-1)>Math.Max(0,hero.level-1)||d.ranks.Any(r=>r.rank>1&&(LegacySkillWire.Find(r.id).unlock>hero.level||LegacySkillWire.Find(r.id).Passive&&LegacySkillWire.Find(r.id).legacy&&ContentUnlocks.PassiveSlots(hero)==0)))throw new ArgumentException("Invalid skill-point allocation.");
            if(departing&&hero.level>=40&&d.ultimate=="")throw new ArgumentException("Choose one ultimate before departing.");
            for(int i=ContentUnlocks.PassiveSlots(hero);i<3;i++)if(d.passives[i]!="")throw new ArgumentException("Passive slot is locked.");
        }
        int TransportIndex(string id)
        {
            if(version==5)return ClassSkills.LegacyIndex(id);
            var wire=LegacySkillWire.Find(id);return wire?.legacy==true&&!wire.Passive&&int.TryParse(id.Substring(1),out int slot)&&slot<=6?heroClass*6+slot-1:-1;
        }
        public HuntEdictV2Document ProjectEdict()
        {
            var d=Canonical(this);var result=d.legacyEdict??HuntEdictV2.Create((HeroClass)d.heroClass);
            string Current(string id){int index=d.TransportIndex(id);return index>=0?ClassSkills.LegacyId(index):id=="BASIC"?id:"";}
            result.slots=d.actives.Select(Current).ToArray();
            var equipped=d.order.Select(Current).Where(id=>id!="").ToArray();
            result.skillOrder=equipped.Concat(result.skillOrder.Where(id=>!equipped.Contains(id))).ToArray();result=HuntEdictV2.Canonical(result);
            var automatic=d.automatic.Select(Current).ToHashSet();foreach(var skill in result.skills)HuntEdictV2.SetOption(result,skill.id,1,automatic.Contains(skill.id)?"ON":"OFF");
            return d.version==5?ClassSkillOptions.ProjectLegacy(d,result):result;
        }
        public void ProjectLegacy(BuildConfig build,GameCatalog catalog)
        {
            var d=Canonical(this);build.classSkills=d;
            build.activeSkills=d.actives.Select(d.TransportIndex).Where(i=>i>=0).ToList();
            build.passives=d.passives.Where(id=>ClassSkills.Find(id)?.legacy==true).Select(id=>SkillProgression.Index((HeroClass)d.heroClass,id)).ToArray();
            // The legacy magnitude array has no unlearned state; the tree owns eligibility.
            build.skillRanks=Enumerable.Range(0,12).Select(i=>Math.Max(1,d.Rank(SkillProgression.Id((HeroClass)d.heroClass,i)))).ToArray();
            build.rules=build.rules.Where(rule=>rule.action!=RuleAction.Skill||build.activeSkills.Contains(rule.skill)).ToList();
            foreach(int index in build.activeSkills)if(!build.rules.Any(rule=>rule.action==RuleAction.Skill&&rule.skill==index))build.rules.Add(new Rule(index));
            BehaviorRules.Normalize(build);
        }
    }
}
