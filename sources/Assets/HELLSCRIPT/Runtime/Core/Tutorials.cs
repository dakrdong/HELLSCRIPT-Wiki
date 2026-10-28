using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    [Serializable] public sealed class TutorialRecord
    {
        public string id,hero="",variant="";
        public bool available,read,practiced,deferred,hidden,announced;
    }
    public sealed class TutorialDefinition
    {
        public readonly string id,ko,bodyKo,target;
        public readonly bool perHero,supported;
        public string Title=>Loc.T(ko);
        public string Body=>Loc.T(id=="H08"&&SkillTreeD4.Enabled?bodyKo.Replace("40","41"):bodyKo);
        public TutorialDefinition(string id,string ko,string bodyKo,string target,bool perHero=false,bool supported=false)
        {this.id=id;this.ko=ko;this.bodyKo=bodyKo;this.target=target;this.perHero=perHero;this.supported=supported;}
    }
    // The guide is a projection of committed gameplay facts, never a replacement for domain validation.
    public static class Tutorials
    {
        public const int Version=1;
        public static readonly TutorialDefinition[] All=new[]{
            new TutorialDefinition("P00","칙령의 길","칙령의 여신 벨리나를 따라 길을 걸으며 스킬 습득, 전투 위치, 공격 우선순위, 전리품 회수를 실제 사냥 칙령에서 바꿔 봅니다. 길 끝의 보스를 쓰러뜨리면 캐릭터를 선택하고 마을에서 모험을 이어갑니다. 룬 보드는 균열 10단계 클리어 후 열립니다.","tutorial"),
            new TutorialDefinition("F01","마을 이동과 주민","화면의 목적지를 누르거나 이동 패드·WASD로 움직이세요. 균열지기 가까이에서 상호작용하면 입장 준비가 열립니다.","rift",true),
            new TutorialDefinition("F02","첫 균열 준비","현재 장비·스킬·물약과 단계를 확인하세요. 일반 입장은 1배속이며, 전투 중 일시정지에도 피로도는 흐릅니다. 준비를 마치고 출발하세요.","rift",true),
            new TutorialDefinition("F03","균열 진행","이동·공격·획득은 자동입니다. 처치 게이지와 입장할 때 확정된 제한시간, 미니맵을 확인하세요. 목표를 달성하면 보스가 등장합니다.","battle",true),
            new TutorialDefinition("F04","결과와 첫 클리어 보상","성공·실패 결과와 마지막 행동을 확인하세요. 이미 얻은 보상은 유지됩니다. 첫 클리어 상자는 균열 입장 화면에서 별도로 수령합니다.","rift",true),
            new TutorialDefinition("F05","첫 위험 상황 검토","마지막 위험 상황을 확인하고 자동 HP 물약 또는 회피의 간편 선택을 바꿔 저장하세요. 다음 사냥에서 달라진 행동을 관찰하세요.","edict",true),
            new TutorialDefinition("F06","새 스킬 장착","첫 스킬 외에 아직 장착한 적 없는 액티브 스킬을 실제 슬롯에 장착하고 저장하세요. 배우지 않은 스킬은 2레벨부터 받는 스킬 포인트로 먼저 배웁니다. 패시브는 장착하지 않아도 적용됩니다.","skills",true),
            new TutorialDefinition("F07","수정한 설정으로 재도전","변경한 칙령을 훈련장에서 시험하고, 같은 설정으로 새 균열에 도전하세요. 훈련은 보상과 피로도를 소비하지 않습니다.","training",true),
            new TutorialDefinition(ContentUnlocks.Train,"훈련장","균열 단계와 적을 고르고, 스킬별 사냥 칙령을 바꿔 가며 같은 세팅으로 다시 싸워 보세요. 결과에서 직전 판과 클리어 시간·DPS를 비교합니다.","training"),
            new TutorialDefinition(ContentUnlocks.Enhance,"장비 강화","소유한 +0 장비를 고르고 비용을 확인한 뒤 +1로 강화하세요. 최초 실습은 이 한 단계의 골드를 지원합니다.","blacksmith",supported:true),
            new TutorialDefinition(ContentUnlocks.Offline,"미접속 보급","실제 균열 기록을 바탕으로 정산된 보급을 확인하세요. 튜토리얼 전투는 보급 기준을 만들지 않습니다.","offline"),
            new TutorialDefinition(ContentUnlocks.SlotEnhance,"장비 부위 강화","1레벨 부위를 2레벨로 강화하세요. 최초 실습은 강화석을 지원하며, 정상 제작 시간 5분은 그대로 적용됩니다.","blacksmith",supported:true),
            new TutorialDefinition(ContentUnlocks.Gem,"보석 합성","같은 종류의 1단계 보석 5개를 2단계 1개로 합성하세요. 최초 실습은 선택한 1단계 보석 5개를 지원합니다.","jeweler",supported:true),
            new TutorialDefinition(ContentUnlocks.Rune,"룬 구입과 배치","G0 한 칸 룬을 구입하고 룬 보드에서 배치를 확인하세요. 최초 실습은 룬 1개의 구입 골드를 지원합니다.","runes",supported:true),
            new TutorialDefinition(ContentUnlocks.Shop,"미확인 장비 구입","원하는 부위의 미확인 장비 1개를 구입하세요. 최초 실습은 골드를 지원하며, 등급 확률은 일반 구입과 같습니다.","gambler",supported:true),
            new TutorialDefinition(ContentUnlocks.Reroll,"속성 재설정","소유 장비에서 바꿀 속성 한 줄을 선택하고 재설정하세요. 최초 실습은 한 번의 골드를 지원하며 결과는 무작위입니다.","blacksmith",supported:true),
            new TutorialDefinition(ContentUnlocks.Aspect,"위상 각인","실제로 보유한 위상과 각인 가능한 소유 장비를 선택하세요. 무료 각인을 실행하며, 연습용 전설이나 위상은 지급하지 않습니다.","aspect"),
            new TutorialDefinition(ContentUnlocks.Sweep,"균열 소탕","실제로 클리어한 단계와 남은 일일 횟수를 확인한 뒤 소탕하세요. 안내가 추가 횟수를 지급하지는 않습니다.","sweep"),
            new TutorialDefinition(ContentUnlocks.Elixir,"보석 영약 제조","선택한 1단계 보석으로 영약 1개를 제조하세요. 최초 실습은 보석 1개를 지원합니다. 제조 후 물약 슬롯에서 사용 설정을 확인하세요.","jeweler",supported:true),
            new TutorialDefinition(ContentUnlocks.Masterwork,"명품화","실제 +5 이상 장비의 명품화 0단계를 1단계로 올리세요. 최초 실습은 골드와 재료를 지원하며, 선행 강화는 직접 진행합니다.","blacksmith",supported:true),
            new TutorialDefinition(ContentUnlocks.CoreCraft,"코어 제작","제작법을 선택해 코어 10개로 장비 1개를 제작하세요. 최초 실습은 해당 코어를 지원하며 유료 투자는 0으로 고정합니다.","blacksmith",supported:true),
            new TutorialDefinition("H01","장비 상세와 보호","착용품은 왼쪽, 후보는 오른쪽에서 비교합니다. 옵션 범위 표시와 보호 기능을 확인하세요. 비교만으로 장착되지는 않습니다.","inventory"),
            new TutorialDefinition("H02","상점과 재구입","판매 전에 보호 상태와 가격을 확인하세요. 판매한 장비는 재구입 목록에 남아 있는 동안 되살 수 있습니다.","merchant"),
            new TutorialDefinition("H03","분해와 자동 선택","분해 미리보기에서 보상과 선택 목록을 확인하세요. 장착·보호·보석·프리셋에 연결된 장비는 보호 규칙을 따릅니다.","inventory"),
            new TutorialDefinition("H04","창고와 장비 프리셋","계정 창고와 영웅 가방을 구분하세요. 프리셋은 소유 장비를 참조하며 복제하지 않습니다.","storage"),
            new TutorialDefinition("H05","물약 슬롯과 순서","소유한 물약을 슬롯에 지정하고 공통 사용 순서를 확인하세요. 생존 조건은 사냥칙령에서 설정합니다.","inventory"),
            new TutorialDefinition("H06","물약 보충과 부족","출발 전 보충 비용과 보유 수량을 확인하세요. 재고나 골드가 부족하면 조건을 바꾸거나 보충을 취소할 수 있습니다.","inventory"),
            new TutorialDefinition("H07","스킬 포인트와 패시브","획득한 포인트로 스킬을 성장시키세요. 해금된 패시브는 슬롯에 장착하지 않아도 적용됩니다.","skills",true),
            new TutorialDefinition("H08","궁극기","실제 레벨 40에 도달하면 궁극기를 확인하고 전용 슬롯에 장착하세요. 사용 조건과 자원 소모도 확인하세요.","skills",true),
            new TutorialDefinition("H09","칙령과 판단 이유","현재 행동의 판단 기록에서 조건·거리·자원 부족을 확인하세요. 현재 설정에서 필요한 항목만 직접 수정할 수 있습니다.","edict"),
            new TutorialDefinition("H10","로컬 프리셋","설정을 슬롯에 저장하고 불러올 때 변경 내용을 확인하세요. 공개되지 않은 항목을 바꾸는 설정은 적용할 수 없습니다.","edict"),
            new TutorialDefinition("H11","반복 사냥과 절전","성공·실패 후의 행동과 가방이 찼을 때의 정책을 정하세요. 절전 화면은 게임 진행을 별도로 보장하는 서버 기능이 아닙니다.","edict"),
            new TutorialDefinition("H12","실패와 이어하기","실패 결과에서 원인을 확인하세요. 중단된 균열은 저장된 상태로 이어서 진행하며 새 입장과 구분됩니다.","rift"),
            new TutorialDefinition("H13","가방과 미수령 보상","가방이 찼다면 보호 상태를 확인하며 정리하세요. 미수령 보상과 이미 획득한 보상을 구분하고 직접 수령 결과를 확인하세요.","inventory"),
            new TutorialDefinition("H14","전설·세트·각성 장비","새로 얻은 장비의 고유 효과·세트 조건·각성 속성을 공통 상세 화면에서 확인하세요. 각 종류의 첫 안내는 별도로 기록됩니다.","inventory"),
            new TutorialDefinition("H15","균열 목표와 특수 조우","봉인·운반자·공물의 목표와 상자·성소·고블린·정예·보스의 행동을 확인하세요. 전투 중 안내는 진행을 막지 않으며 자세한 설명은 마을에서 볼 수 있습니다.","battle"),
            new TutorialDefinition("H16","출석 이벤트","출석 일차와 받을 수 있는 보상을 확인하고 직접 수령하세요. 창을 닫거나 자동 표시를 숨겨도 수령 기록은 유지됩니다.","attendance"),
            new TutorialDefinition("H17","직업별 한 가지 실험","현재 설정으로 A를 실행하고, 조건 하나만 바꾼 B를 같은 장비·레벨·난수로 시험하세요. 실제 발동이나 행동 차이를 확인한 뒤 적용 여부를 선택합니다.","class-practice",true)
        }.Concat(new[]{
            new TutorialDefinition("E02","전투 성향","공격적·균형·신중한 성향 중 하나를 고르고 저장하세요. 물약 HP 기준과 구매 설정은 유지합니다.","edict"),
            new TutorialDefinition("E08C","저주 상자 정책","실제로 발견한 저주 상자를 열지, 건너뛸지 정하세요. 체력과 남은 시간의 세부 조건은 균열 20단계에서 공개됩니다.","edict")
        }).Concat(HuntEdictProgression.Rules.rules.Where(r=>!string.IsNullOrEmpty(r.guide)).Select(r=>new TutorialDefinition(r.guide,r.title,r.body,"edict"))).ToArray();
        public static string VariantTitle(string variant)=>Loc.T(variant switch {
            "legendary"=>"전설 장비",
            "set"=>"세트 장비",
            "awakened"=>"각성 장비",
            "Seals"=>"봉인 해제",
            "EssenceCarriers"=>"정수 운반자",
            "Offerings"=>"공물과 제단",
            "chest"=>"보물 상자",
            "shrine"=>"성소 효과",
            "goblin"=>"보물 고블린",
            "elite"=>"정예 몬스터",
            "boss"=>"보스 전투",
            _=>variant});
        public static string VariantBody(string variant)=>Loc.T(variant switch {
            "legendary"=>"장비 상세의 고유 효과를 확인하세요. 조건에 맞는 스킬과 칙령을 함께 사용하면 효과를 활용할 수 있습니다.",
            "set"=>"같은 세트의 실제 장착 개수와 2세트·4세트 효과를 확인하세요. 가방에 보관한 장비는 세트 개수에 포함되지 않습니다.",
            "awakened"=>"장비 상세에서 각성 속성과 수치 범위를 확인하세요. 기존 착용품과 비교한 뒤 교체 여부를 결정하세요.",
            "Seals"=>"봉인 주변의 적을 정리하면 봉인 해제를 진행할 수 있습니다. 미니맵의 목표와 진행 수를 확인하세요. 상호작용 우선순위는 사냥칙령을 따릅니다.",
            "EssenceCarriers"=>"표시된 운반자를 처치해 목표를 진행하세요. 일반 적 처치 게이지와 운반자 목표 진행을 구분해서 확인하세요.",
            "Offerings"=>"공물을 지키는 적을 처치하고 공물을 모으세요. 필요한 공물을 모으면 제단에 전달하는 단계가 남습니다.",
            "chest"=>"발견한 상자에 접근해 열면 보상을 획득합니다. 열지 않은 상자의 보상은 지급되지 않으며 접근 우선순위는 사냥칙령에서 바꿀 수 있습니다.",
            "shrine"=>"성소에 접근해 상호작용하면 일시적인 효과를 얻습니다. 화면의 효과 표시와 남은 시간을 확인하세요.",
            "goblin"=>"고블린은 도망갑니다. 사냥칙령의 추격·대상 우선순위가 현재 목표와 생존에 맞는지 확인하세요.",
            "elite"=>"정예의 추가 속성과 위험 지역을 확인하세요. 반복해서 피해를 받으면 결과의 행동 기록을 보고 회피·생존 조건을 조정하세요.",
            "boss"=>"보스가 공격을 준비하는 방향과 위험 지역을 살펴보세요. 사냥칙령의 회피·거리 유지 조건이 실제 전투에서 어떻게 작동하는지 확인하세요.",
            _=>""});
        public static TutorialDefinition Definition(string id)=>All.FirstOrDefault(d=>d.id==id);
        // The prologue's first skill per class: the preset it starts on and the preset the voice asks for.
        public static (string skill,string start,string pick) PrologueSkill(HeroClass hero)=>SkillTreeD4.Enabled?D4Starter(hero):hero switch
        {HeroClass.Ranger=>("A01","elite","steady"),HeroClass.Mage=>("M01","elite","steady"),_=>("W01","stand","edge")};
        static (string skill,string start,string pick) D4Starter(HeroClass hero)=>hero switch
        {HeroClass.Ranger=>("A07","opportunity","setup"),HeroClass.Mage=>("M07","opportunity","setup"),_=>("W09","opportunity","native")};
        public static (string skill,string start,string pick) StarterComparison(HeroClass hero)=>SkillTreeD4.Enabled?D4Starter(hero):hero switch
        {HeroClass.Ranger=>("A01","steady","pack"),HeroClass.Mage=>("M01","steady","pack"),_=>("W01","stand","edge")};
        public const int PrologueGold=1000;
        public static bool Mandatory(AccountSave a)=>!a.guide.legacyExempt&&!a.guide.mapComplete;
        public static bool RequiredArmor(AccountSave a,string id)=>Mandatory(a)&&id==a.guide.armorId;
        public static TutorialRecord Record(AccountSave a,string id,string variant="")
        {
            string hero=Definition(id)?.perHero==true?a.Hero.id:"";
            var r=a.guide.tutorials.Find(x=>x.id==id&&x.hero==hero&&x.variant==variant);
            if(r!=null)return r;r=new TutorialRecord{id=id,hero=hero,variant=variant};a.guide.tutorials.Add(r);return r;
        }
        public const int SlotVersion=1;
        // Parked shares need checking only when a save is read, not at every write: nothing changes them in between.
        public static void Normalize(AccountSave a,bool checkSlots=true)
        {
            a.guide??=new AccountGuide();var g=a.guide;
            if(g.tutorialVersion>Version||g.tutorialVersion<0||a.schema>=17&&g.tutorialVersion!=Version)throw new NotSupportedException("Unsupported tutorial save version.");
            g.tutorials??=new List<TutorialRecord>();g.supportUsed??=new List<string>();g.claimedChapters??=new List<string>();g.tutorialSlots??=new List<TutorialSlot>();
            RuneBoardTutorial.Normalize(a);
            foreach(var h in HeroSlots.Created(a))
            {
                // The retired puzzle tutorial (2026-10-14) left its authored skill kit on a hero who stopped partway: give the points back.
                if(h.build.classSkills?.version==5&&h.build.classSkills.kitKind=="puzzle")GameStore.ReleaseAuthoredSkillKit(h);
                h.guide??=new HeroGuide();h.guide.equippedSkillHistory??=new List<string>();
                foreach(string id in h.build.classSkills?.actives??Array.Empty<string>())if(!string.IsNullOrEmpty(id)&&!h.guide.equippedSkillHistory.Contains(id))h.guide.equippedSkillHistory.Add(id);
            }
            NormalizeShare(a);
            if(g.tutorialVersion==0)
            {
                // Old acknowledgement combined read and skip, so it cannot prove a performed action.
                g.legacyExempt=true;g.tutorialVersion=Version;
                int selected=a.selectedHero;
                for(int i=0;i<a.heroes.Count;i++)
                {
                    if(!HeroSlots.IsCreated(a,i))continue;
                    a.selectedHero=i;var hero=a.Hero;
                    if(FirstPlayGuide.Done(a,hero,GuideStep.Rift))Performed(a,"F02");
                    if(FirstPlayGuide.Done(a,hero,GuideStep.Result))Performed(a,"F04");
                    if(FirstPlayGuide.Done(a,hero,GuideStep.Retry))Performed(a,"F07");
                    if(FirstPlayGuide.Done(a,hero,GuideStep.Training))Performed(a,ContentUnlocks.Train);
                    if(FirstPlayGuide.Done(a,hero,GuideStep.Equipment)&&!hero.guide.equipmentExplanation&&!string.IsNullOrEmpty(hero.guide.comparedItem))Performed(a,"H01");
                }
                a.selectedHero=selected;
                foreach(string id in a.contentUnlocks?.guidesCompleted??new List<string>())if(Definition(id)!=null)Record(a,id).read=true;
            }
            MigrateSlots(a);MigrateArmorReceipts(a);if(checkSlots)NormalizeSlots(a);
        }
        // The checks on the selected hero's share of the tutorial; a parked share passes through here too (NormalizeSlots).
        static void NormalizeShare(AccountSave a)
        {
            var g=a.guide;
            if(g.tutorialRun!=null&&string.IsNullOrEmpty(g.tutorialRun.id))g.tutorialRun=null;
            if(g.tutorialRun!=null)
            {
                var r=g.tutorialRun;
                if(!r.tutorial||r.training!=3||r.heroId!=g.mapHero||!HeroSlots.Created(a).Any(h=>h.id==r.heroId)||r.tutorialPhase<0||r.tutorialPhase>(GuideTutorial.Running(r)?GuideTutorial.LastFor(r):ProloguePhase.Last)||g.mapComplete)
                    throw new NotSupportedException("Invalid tutorial checkpoint; original save preserved.");
                MigrateEdictLesson(r);
                // Valid flow-7 checkpoints resume without refilling; older flows keep their entry behavior (GameStore.StartGuideTutorial).
                // A checkpoint from another flow (an earlier prologue, the retired puzzle levels) means different phases and is dropped.
                if(GuideTutorial.Running(r)){if(r.layout?.fingerprint!=GuideTutorial.FingerprintFor(r))g.tutorialRun=null;else GameStore.NormalizeRun(r);}
                else if(r.layout?.fingerprint!=ProloguePhase.PitFingerprint)g.tutorialRun=null;
                else
                {
                    if(r.tutorialFlowVersion!=ProloguePhase.PitFlow)throw new NotSupportedException("Unsupported tutorial flow checkpoint; original save preserved.");
                    GameStore.NormalizeRun(r);
                }
            }
        }
        internal static void MigrateEdictLesson(RunState run)
        {
            // The retired comparison beat used flow 3 on the pit map; resume its actual skill lesson without refilling the run.
            if(run.tutorialFlowVersion==3&&run.tutorialPhase==ProloguePhase.EdictLesson&&run.layout?.fingerprint==ProloguePhase.PitFingerprint)
            {run.tutorialFlowVersion=ProloguePhase.PitFlow;run.tutorialLessonStep=0;}
        }
        // The tutorial runs per hero. The account's tutorial fields (mapComplete, mapHero, armorId, tutorialRun, townArmorGiven) belong to the selected hero:
        // choosing another hero parks them under the old one and takes that hero's share. Guide-stage graduation exempts future heroes too.
        public static void Switch(AccountSave a,int from,int to)
        {
            var g=a.guide;if(from==to||!HeroSlots.IsCreated(a,to))return;
            MigrateArmorReceipts(a);
            if(HeroSlots.IsCreated(a,from))Park(g,a.heroes[from].id);
            a.selectedHero=to;Take(g,a.heroes[to].id);
        }
        static void Park(AccountGuide g,string heroId)
        {
            g.tutorialSlots.RemoveAll(s=>s.heroId==heroId);
            g.tutorialSlots.Add(new TutorialSlot{heroId=heroId,mapComplete=g.mapComplete,mapHero=g.mapHero,armorId=g.armorId,tutorialRun=g.tutorialRun,townArmorGiven=g.townArmorGiven});
        }
        static void Take(AccountGuide g,string heroId)
        {
            var slot=g.tutorialSlots.Find(s=>s.heroId==heroId);g.tutorialSlots.Remove(slot);
            g.mapComplete=g.everythingOpen||slot?.mapComplete==true;g.mapHero=slot?.mapHero??"";g.armorId=slot?.armorId??"";g.tutorialRun=g.everythingOpen?null:slot?.tutorialRun;g.townArmorGiven=slot?.townArmorGiven??false;
        }
        static void MigrateArmorReceipts(AccountSave a)
        {
            var g=a.guide;if(g.townArmorReceiptVersion>=1)return;
            var owners=HeroSlots.Created(a).Where(h=>TownQuest.GiftArmor(h)!=null||
                h.equipmentShop?.offers?.Any(o=>o.gift&&o.purchased)==true||
                a.transactions?.Any(r=>r.operation=="shop-buy:"+h.id+":"+TownQuest.GiftArmorId(h))==true||
                g.tutorialSlots.Any(s=>s.heroId==h.id&&s.townArmorGiven)).Select(h=>h.id).ToArray();
            // The legacy account flag may be on a different selected hero. Recover the purchaser from durable ownership/transaction evidence.
            // If no owner can be established, retain the legacy selected receipt rather than silently discard saved progress.
            if(owners.Length>0)g.townArmorGiven=owners.Contains(a.Hero.id);
            foreach(string id in owners.Where(id=>id!=a.Hero.id))
            {
                var slot=g.tutorialSlots.Find(s=>s.heroId==id);
                if(slot==null){slot=new TutorialSlot{heroId=id};g.tutorialSlots.Add(slot);}slot.townArmorGiven=true;
            }
            g.townArmorReceiptVersion=1;
        }
        // Progress from before per-hero tutorials: a finished tutorial counts for every hero the account already has; one under way stays with its hero.
        static void MigrateSlots(AccountSave a)
        {
            var g=a.guide;if(g.tutorialSlotVersion>=SlotVersion)return;
            g.tutorialSlotVersion=SlotVersion;
            if(g.legacyExempt)return;
            if(g.mapComplete){foreach(var h in HeroSlots.Created(a))if(h.id!=a.Hero.id)g.tutorialSlots.Add(new TutorialSlot{heroId=h.id,mapComplete=true});return;}
            int owner=a.heroes.FindIndex(h=>h.id==g.mapHero);
            if(owner>=0&&owner!=a.selectedHero){Park(g,g.mapHero);Take(g,a.Hero.id);}
        }
        // A parked share passes the checks the selected one does: swap it in, check it, swap back. A share without a hero is dropped.
        static void NormalizeSlots(AccountSave a)
        {
            var g=a.guide;int selected=a.selectedHero;string current=a.Hero.id;
            g.tutorialSlots=g.tutorialSlots.GroupBy(s=>s.heroId).Select(group=>group.Last()).Where(s=>s.heroId!=current&&HeroSlots.Created(a).Any(h=>h.id==s.heroId)).ToList();
            foreach(string id in g.tutorialSlots.Select(s=>s.heroId).ToArray())
            {int index=a.heroes.FindIndex(h=>h.id==id);Switch(a,selected,index);NormalizeShare(a);Switch(a,index,selected);}
        }
        public static bool Available(AccountSave a,TutorialDefinition d)
        {
            if(d.id=="P00")return true;
            if(Mandatory(a)||!HuntEdictProgression.GuideVisible(a,d.id))return false;
            if(d.id.StartsWith("E"))return true;
            if(d.id.StartsWith("UL"))return ContentUnlocks.Has(a,d.id);
            if(d.id=="H08")return a.Hero.level>=(SkillTreeD4.Enabled?41:40);
            if(d.id=="F06")return TutorialProgress.NewSkills(a.Hero).Any();
            if(d.id=="H07")return a.Hero.level>=2;
            if(d.id=="H17")return ClassPracticeLesson.Error(a)=="";
            return Record(a,d.id).available||d.id=="F01"||d.id=="F02"||d.id=="H01"||d.id=="H05";
        }
        // Guides the player performs (a supported practice or a real edict change) stay open after being read.
        public static bool HasAction(TutorialDefinition d)=>d.supported||d.id=="F05"||d.id=="F06"||d.id=="F07"||d.id=="H07"||d.id=="H17";
        public static void Discover(AccountSave a,string id,string variant="")=>Record(a,id,variant).available=true;
        public static void Performed(AccountSave a,string id,string variant="")
        {var r=Record(a,id,variant);r.available=true;r.practiced=true;r.deferred=false;}
        public static TutorialDefinition Next(AccountSave a)
        {
            if(Mandatory(a))return Definition("P00");
            if(a.guide.hintsHidden)return null;
            string priority=FirstPlayRecommendation.Choose(a)?.guideId;var chapter=TutorialChapters.Current(a);
            return All.Where(d=>d.id!="P00"&&Available(a,d)&&(!a.guide.legacyExempt||Record(a,d.id).available))
                .OrderBy(d=>d.id==priority?0:chapter!=null&&chapter.guides.Contains(d.id)?1:d.id=="H17"?3:2).FirstOrDefault(d=>
                {var r=Record(a,d.id);return (!r.read||HasAction(d))&&!r.deferred&&!r.hidden&&!r.practiced;});
        }
        public static void ObserveTransaction(AccountSave a,string operation)
        {
            string prefix=operation.Split(':')[0];string id=prefix switch {
                "jeweler-convert"=>ContentUnlocks.Gem,"jeweler-craft"=>ContentUnlocks.Elixir,"town-rune"=>ContentUnlocks.Rune,"aspect-imprint"=>ContentUnlocks.Aspect,
                "gamble"=>ContentUnlocks.Shop,"forge-masterwork"=>ContentUnlocks.Masterwork,"masterwork"=>ContentUnlocks.Masterwork,"offline-seen"=>ContentUnlocks.Offline,"attendance-claim"=>"H16","potion-slot"=>"H05","potion-fallback"=>"H05","potions"=>"H06",
                "storage-move"=>"H04","storage-place"=>"H04","storage-bulk"=>"H04","storage-preset"=>"H04","storage-register"=>"H04",
                "first-clear-boxes"=>"H13","reward-box"=>"H13",
                "forge-gear"=>ContentUnlocks.Enhance,"forge-start"=>ContentUnlocks.SlotEnhance,
                "forge-reroll"=>ContentUnlocks.Reroll,"gem-fuse"=>ContentUnlocks.Gem,
                "core-craft"=>ContentUnlocks.CoreCraft,"sweep"=>ContentUnlocks.Sweep,
                "inventory-review"=>"H01","shop-buy"=>"H02","shop-sell"=>"H02","shop-buyback"=>"H02",
                "inventory-salvage"=>"H03","inventory-equip"=>"H01",_=>null};
            if(id!=null)Performed(a,id);
        }
    }
}
