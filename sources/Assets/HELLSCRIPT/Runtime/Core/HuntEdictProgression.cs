using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    [Serializable] public sealed class EdictDisclosureRule { public int stage; public string condition,guide,title,body,focus; public string[] ids; }
    [Serializable] public sealed class EdictDisclosureData { public int version; public EdictDisclosureRule[] rules; }
    // Permissions are account facts; current skill/item availability remains a hero/domain check.
    public static class HuntEdictProgression
    {
        public const int Version=1;
        public const string Details="details",Styles="styles",Presets="presets",Sharing="sharing",Dodge="quick-dodge",Curse="seen-curse";
        static EdictDisclosureData data;
        public static EdictDisclosureData Rules
        {
            get
            {
                if(data!=null)return data;
                var value=JsonUtility.FromJson<EdictDisclosureData>(Resources.Load<TextAsset>("HuntEdictProgression").text);
                var ids=value.rules.SelectMany(r=>r.ids).ToArray();
                if(value.version!=Version||ids.Length!=EdictOptions.Global.Length||ids.Distinct().Count()!=ids.Length||!ids.All(id=>EdictOptions.Global.Any(o=>o.id==id)))
                    throw new InvalidOperationException("Disclosure rules must cover every global option exactly once.");
                return data=value;
            }
        }
        public static void Normalize(AccountSave a,bool legacy=false)
        {
            a.guide??=new AccountGuide();var g=a.guide;
            if(g.edictProgressionVersion<0||g.edictProgressionVersion>Version)throw new NotSupportedException("Unsupported edict disclosure version.");
            g.edictUnlocks??=new List<string>();
            if(g.edictProgressionVersion==0){g.edictLegacyAccess=legacy;g.edictProgressionVersion=Version;}
            Reconcile(a);
        }
        static void Grant(AccountGuide g,string id){if(!g.edictUnlocks.Contains(id))g.edictUnlocks.Add(id);}
        public static void Reconcile(AccountSave a)
        {
            var g=a.guide;if(g==null)return;
            g.edictUnlocks??=new List<string>();int best=ContentUnlocks.AccountClear(a);
            bool Condition(string c)=>c switch
            {
                "retired"=>false,"first-result"=>a.contentUnlocks?.firstRunEnded==true,
                "rune-complete"=>g.runeBoard?.step==RuneBoardLesson.Complete,
                "special-equipment"=>Has(a,"special-equipment-acquired"),_=>true
            };
            foreach(var rule in Rules.rules)if(rule.stage>=0&&(g.edictLegacyAccess||best>=rule.stage&&Condition(rule.condition)))
                foreach(string id in rule.ids)Grant(g,id);
            if(g.edictLegacyAccess||best>=2)Grant(g,Styles);
            if(a.cores.Any(c=>c>0))Grant(g,"seen-core");
            if(a.heroes.Any(h=>h.potions?.stacks?.Any(p=>p.count>0&&PotionCatalog.Get(p.id).effect=="resource")==true))Grant(g,"seen-resource");
            if(a.heroes.Any(h=>h.potions?.stacks?.Any(p=>p.count>0&&PotionCatalog.IsUtility(p.id))==true))Grant(g,"seen-utility");
            if(g.edictLegacyAccess||best>=6){Grant(g,Details);Grant(g,Presets);}
            if(g.edictLegacyAccess||best>=20)Grant(g,Sharing);
            if(g.edictLegacyAccess||a.contentUnlocks?.firstRunEnded==true)Grant(g,Dodge);
            if(a.suspendedRun?.layout?.chests?.Any(c=>c.discovered&&c.definitionId=="CH03")==true)Grant(g,Curse);
        }
        public static bool Has(AccountSave a,string id)=>id!="autoEquip.preserveEffects"&&a?.guide?.edictUnlocks?.Contains(id)==true;
        public static bool Legacy(AccountSave a)=>a?.guide?.edictLegacyAccess==true;
        public static bool Visible(AccountSave a,HeroSave hero,string id)
        {
            if(!Has(a,id))return false;
            if(Legacy(a))return true;
            if(id.StartsWith("explore.cursed",StringComparison.Ordinal)&&!Has(a,Curse))return false;
            if(id=="loot.gem"||id=="bag.protectSocketed")return a.contentUnlocks?.gemAcquired==true;
            if(id=="loot.core")return Has(a,"seen-core");
            if(id.StartsWith("potion.utility",StringComparison.Ordinal)||id=="potion.condition")
                return a.contentUnlocks?.unlocked?.Contains(ContentUnlocks.Elixir)==true&&Has(a,"seen-utility");
            if(id.StartsWith("potion.resource",StringComparison.Ordinal))return Has(a,"seen-resource");
            return true;
        }
        public static bool Direct(AccountSave a,HeroSave hero,string id)=>Visible(a,hero,id)&&(Legacy(a)||Has(a,Details)||id=="survival.potionHpPercent"||id=="position.mode"||id=="survival.potion"||id=="survival.defenseSkill"||id=="survival.escapeSkill");
        public static string Condition(string id)
        {
            var r=Rules.rules.FirstOrDefault(x=>x.ids.Contains(id));
            return r==null?Loc.T("진행 후 공개됩니다."):r.condition=="first-result"?Loc.T("첫 일반 균열을 마치면 공개됩니다."):
                r.condition=="rune-complete"?Loc.T("균열 15단계와 룬 안내를 마치면 공개됩니다."):
                r.condition=="special-equipment"?Loc.T("균열 12단계 이후 전설 또는 세트 장비를 얻으면 공개됩니다."):Loc.F("균열 {0}단계 클리어 후 공개됩니다.",r.stage);
        }
        public static bool Tab(AccountSave a,HeroSave hero,string tab)=>tab=="overview"||tab=="skills"||tab=="survival"||
            (tab=="presets"?Has(a,Presets):HuntEdictUiCatalog.Data.groups.Any(g=>g.tab==tab&&g.ids.Any(id=>Visible(a,hero,id))));
        public static bool Quick(AccountSave a,string scope,string preset,HeroSave hero=null)
        {
            if(Legacy(a))return true;
            if(scope=="global/dodge.ground.policy")return Has(a,Dodge)&&(preset=="balanced"||preset=="all"||Has(a,Details)&&Has(a,"dodge.ground.policy"));
            if(scope.StartsWith("global/",StringComparison.Ordinal))return Has(a,scope.Substring(7))&&scope!="global/survival.potion";
            if(scope=="order"||scope.StartsWith("basic/",StringComparison.Ordinal))return Has(a,Details);
            if(scope.StartsWith("skill/",StringComparison.Ordinal))
            {
                var owner=hero??a.Hero;var (skill,start,pick)=Tutorials.StarterComparison(owner.heroClass);
                return Has(a,Details)||owner.level>=2||scope=="skill/"+skill&&(preset==start||preset==pick);
            }
            return false;
        }
        public static EdictDisclosureRule Guide(string id)=>Rules.rules.FirstOrDefault(r=>r.guide==id);
        public static bool GuideVisible(AccountSave a,string id)
        {
            var guide=Guide(id);if(guide!=null)return Has(a,guide.ids[0]);
            return id switch
            {
                "E02"=>Has(a,Styles),"E08C"=>Has(a,"explore.cursedChest")&&Has(a,Curse),
                "F05"=>Has(a,Dodge),"F07" or "H09" or "H17"=>Has(a,Details),"H10"=>Has(a,Presets),"H11"=>Has(a,"repeat.enabled"),_=>true
            };
        }
        public static bool CombatStyleScope(string scope)=>scope!="global/survival.potion";
        public static void RecordEquipmentAcquisition(AccountSave a,Item item)
        {if(item?.rarity==3&&a.guide!=null&&ContentUnlocks.AccountClear(a)>=12)Grant(a.guide,"special-equipment-acquired");}
        public static void InitializeHero(HeroSave h)
        {
            h.useEdict=true;h.useRecommendedEdict=false;
            foreach(var o in h.edict.global)
                if(o.id=="potion.autoBuy"||o.id=="autoEquip.enabled"||o.id=="repeat.enabled")o.value="OFF";
                else if(o.id=="bag.cleanupAt")o.value="OFF";
            h.edict=HuntEdictV2.Canonical(h.edict);
            if(!ClassSkillLoadout.IsAbsent(h.build.classSkills))h.build.classSkills.legacyEdict=h.edict.Copy();
        }
        public static void ValidateChange(AccountSave a,HeroSave hero,HuntEdictLoadout candidate,IEnumerable<SkillPresetSelection> recipes=null)
        {
            if(Legacy(a))return;
            if(Tutorials.Mandatory(a)&&a.guide.mapHero=="")
            {
                var initial=RuneGrowth.Copy(hero);GameStore.ApplyHuntEdict(initial,GameStore.ProloguePowers(hero,true));
                if(JsonUtility.ToJson(candidate)==JsonUtility.ToJson(HuntEdictLoadout.FromHero(initial)))return;
                var bare=ClassSkillTree.ResetAllocation(ClassSkillTree.FromHero(hero));
                GameStore.ApplyHuntEdict(initial,new HuntEdictLoadout{version=2,classSkills=bare,edict=bare.ProjectEdict()});
                if(JsonUtility.ToJson(candidate)==JsonUtility.ToJson(HuntEdictLoadout.FromHero(initial)))return;
            }
            var original=new HuntEdictEditSession(hero);if(candidate.UsesTree)original.UseSkillTree(hero);
            // No permission delta in an identical document. GameStore.Write still validates owned skills and commits the save.
            if(JsonUtility.ToJson(candidate)==JsonUtility.ToJson(original.Draft))return;
            var expected=original.Draft;
            foreach(var recipe in recipes??Array.Empty<SkillPresetSelection>())
            {
                if(recipe.scope=="style")
                {
                    if(!Has(a,Styles))throw new ArgumentException(Condition("position.engage"));
                    expected=HuntEdictQuickPresets.ApplyStyle(expected,recipe.preset,CombatStyleScope);
                }
                else
                {
                    if(!Quick(a,recipe.scope,recipe.preset,hero))throw new ArgumentException(Loc.T("아직 공개되지 않은 간편 설정입니다."));
                    // Skill allocation/equipment has its own validation; recipes resolve against the candidate's owned slots.
                    var basis=expected.Copy();if(candidate.UsesTree){basis.classSkills=candidate.classSkills.Copy();basis.classSkills.legacyEdict=expected.edict.Copy();basis=HuntEdictLoadout.Canonical(basis);}
                    expected=HuntEdictQuickPresets.Apply(basis,recipe.scope,recipe.preset);
                }
            }
            foreach(var o in candidate.edict.global)
                if(HuntEdictSummary.Value(original.Draft.edict,o.id)!=o.value&&!Direct(a,hero,o.id)&&HuntEdictSummary.Value(expected.edict,o.id)!=o.value)
                    throw new ArgumentException(Condition(o.id));
            if(!candidate.UsesTree)return;
            var first=Tutorials.StarterComparison(hero.heroClass).skill;
            if(hero.level<2&&(candidate.classSkills.actives.Any(id=>id!=""&&id!=first&&!original.Draft.classSkills.Equipped(id))||
                candidate.classSkills.ranks.Any(r=>r.id!=first&&r.rank>original.Draft.classSkills.Rank(r.id))))
                throw new ArgumentException(Loc.T("배우고 장착한 스킬의 공개된 사용 방식만 저장할 수 있습니다."));
            if(Has(a,Details))return;
            // Allocation and equipped slots are domain changes. Compare policies with those same slots,
            // retaining their prior options and the normal automatic-use default for newly equipped skills.
            var policyBaseline=original.Draft.Copy();var allocation=candidate.classSkills.Copy();
            allocation.options=original.Draft.classSkills.options.Select(o=>new ClassSkillOptionSelection{id=o.id,choice=o.choice}).ToList();
            allocation.presetSelections=original.Draft.classSkills.presetSelections.Select(p=>new SkillPresetSelection{scope=p.scope,preset=p.preset}).ToList();
            allocation.legacyEdict=original.Draft.edict.Copy();
            allocation.automatic=allocation.order.Where(id=>original.Draft.classSkills.automatic.Contains(id)||id!="BASIC"&&!original.Draft.classSkills.Equipped(id)).ToArray();
            policyBaseline.classSkills=allocation;policyBaseline=HuntEdictLoadout.Canonical(policyBaseline);
            foreach(var skill in ClassSkills.For(hero.heroClass).Where(s=>!s.Passive))
            {
                string scope="skill/"+skill.id;
                if(HuntEdictSkillPresets.SamePolicy(candidate,policyBaseline,scope))continue;
                var preset=HuntEdictQuickPresets.Match(candidate,scope);
                if(!candidate.classSkills.Equipped(skill.id)||candidate.classSkills.Rank(skill.id)<=0||!Quick(a,scope,preset,hero)||!HuntEdictQuickPresets.For(scope).Any(p=>p.id==preset))
                    throw new ArgumentException(Loc.T("배우고 장착한 스킬의 공개된 사용 방식만 저장할 수 있습니다."));
            }
            string basic="basic/"+candidate.edict.heroClass;
            if(!HuntEdictSkillPresets.SamePolicy(candidate,policyBaseline,basic))throw new ArgumentException(Loc.T("세부 스킬 설정은 균열 6단계부터 공개됩니다."));
            var prior=original.Draft.classSkills.order.Where(candidate.classSkills.order.Contains);
            if(!prior.SequenceEqual(candidate.classSkills.order.Where(original.Draft.classSkills.order.Contains)))throw new ArgumentException(Loc.T("세부 스킬 설정은 균열 6단계부터 공개됩니다."));
        }
        static string PresetKey(IEnumerable<HuntEdictPreset> values)=>string.Join(";",values.Select(v=>JsonUtility.ToJson(v)));
        static string BuildPresetKey(IEnumerable<BuildConfig> values)=>string.Join(";",values.Select(v=>JsonUtility.ToJson(v)));
        internal static void ValidateTransaction(AccountSave before,AccountSave after,HuntEdictEditSession session)
        {
            if(before==null||ReferenceEquals(before,after)||Legacy(before))return;
            foreach(var hero in after.heroes)
            {
                var old=before.heroes.FirstOrDefault(h=>h.id==hero.id)??before.heroes.FirstOrDefault(h=>h.heroClass==hero.heroClass);if(old==null)continue;
                if(!Has(before,Sharing)&&(old.useRecommendedEdict!=hero.useRecommendedEdict||old.useEdict&&!hero.useEdict))throw new ArgumentException(Loc.T("추천 모드 전환은 균열 20단계부터 공개됩니다."));
                // Compare the committed snapshot before projecting unchanged policies. Mode, presets and domain checks remain independent.
                if(JsonUtility.ToJson(old.build)!=JsonUtility.ToJson(hero.build)||JsonUtility.ToJson(old.edict)!=JsonUtility.ToJson(hero.edict)||
                    !(old.edictPassiveSlots??Array.Empty<string>()).SequenceEqual(hero.edictPassiveSlots??Array.Empty<string>()))
                {
                    var candidate=HuntEdictLoadout.FromHero(hero);
                    ValidateChange(before,old,candidate,session?.HeroId==hero.id?session.Recipes:null);
                }
                if(!Has(before,Presets)&&(PresetKey(old.edictPresets)!=PresetKey(hero.edictPresets)||BuildPresetKey(old.presets)!=BuildPresetKey(hero.presets)))throw new ArgumentException(Loc.T("프리셋 관리는 균열 6단계부터 공개됩니다."));
                for(int slot=0;slot<hero.edictPresets.Count;slot++)
                {
                    var preset=hero.edictPresets[slot];
                    if(!HuntEdictPreset.HasValue(preset)||JsonUtility.ToJson(preset)==JsonUtility.ToJson(old.edictPresets[slot]))continue;
                    if(!Has(before,Sharing)&&!string.IsNullOrEmpty(preset.originalCode))throw new ArgumentException(Loc.T("공유와 가져오기는 균열 20단계부터 공개됩니다."));
                    ValidateChange(before,old,preset.loadout);if(preset.loadout.UsesTree)ClassSkillLoadout.Validate(preset.loadout.classSkills,hero);
                }
            }
        }
    }
}
