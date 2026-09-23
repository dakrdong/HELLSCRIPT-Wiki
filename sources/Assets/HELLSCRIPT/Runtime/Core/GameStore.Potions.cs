using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class GameStore
    {
        public bool SetPotionSlot(string request,string heroId,int index,string potionId)
            =>Transact(request,"potion-slot:"+heroId+":"+index+":"+potionId,a=>
            {
                var hero=a.heroes.Single(h=>h.id==heroId);
                if(a.Hero.id!=heroId||index<0||index>=PotionLoadout.SlotCount)throw new ArgumentException("잘못된 물약 슬롯입니다.");
                if(a.suspendedRun?.heroId==heroId&&!RepeatHunt.Terminal(a.suspendedRun))throw new ArgumentException("물약 배치는 마을에서 변경할 수 있습니다.");
                potionId??="";
                if(potionId!=""){PotionCatalog.Get(potionId);if(hero.potions.Count(potionId)<=0)throw new ArgumentException("보유한 물약만 지정할 수 있습니다.");}
                var slots=PotionPolicy.Resolve(hero).Slots;slots[index].id=potionId;PotionLoadout.Validate(slots);
                hero.potions.slots=slots;hero.potions.revision++;return true;
            });
        public bool SetPotionFallback(string request,string heroId,PotionFallback fallback)
            =>Transact(request,"potion-fallback:"+heroId+":"+fallback,a=>
            {
                var hero=a.heroes.Single(h=>h.id==heroId);
                if(a.Hero.id!=heroId)throw new ArgumentException("잘못된 물약 슬롯입니다.");
                if(!Enum.IsDefined(typeof(PotionFallback),fallback))throw new ArgumentException("Invalid potion fallback.");
                hero.potions.fallback=fallback;hero.potions.fallbackVersion=1;hero.potions.revision++;return true;
            });
        public bool PreparePotions(string heroId,string visit)
        {
            try
            {
                var hero=Data.heroes.Single(h=>h.id==heroId);
                if(Data.suspendedRun!=null&&!RepeatHunt.Terminal(Data.suspendedRun))throw new InvalidOperationException("물약 구매는 성소에서만 가능합니다.");
                var staged=JsonUtility.FromJson<AccountSave>(JsonUtility.ToJson(Data));var candidate=staged.heroes.Single(h=>h.id==heroId);
                var policy=PotionPolicy.Resolve(hero);string before=JsonUtility.ToJson(hero.potions);
                PotionRestock.Apply(staged,candidate,visit,policy);
                if(staged.gold==Data.gold&&before==JsonUtility.ToJson(candidate.potions)){Error="";return true;}
                return Transact("potions:"+Guid.NewGuid().ToString("N"),"potions:"+heroId+":"+visit,a=>
                {PotionRestock.Apply(a,a.heroes.Single(h=>h.id==heroId),visit,policy);return true;});
            }
            catch(Exception e){Error=Loc.F("물약을 준비하지 못했습니다: {0}",e.Message);return false;}
        }
        internal void ValidateUtilityChange(HeroSave hero,HuntEdictV2Document candidate)
        {
            if(Data.suspendedRun?.heroId==hero.id&&Data.suspendedRun.training<0&&!RepeatHunt.Terminal(Data.suspendedRun)&&
                HuntEdictV2.Canonical(hero.edict).global.Single(o=>o.id=="potion.utility").value!=candidate.global.Single(o=>o.id=="potion.utility").value)
                throw new ArgumentException("보조 물약 종류는 성소 또는 훈련 편집에서 변경할 수 있습니다.");
        }
    }
}
