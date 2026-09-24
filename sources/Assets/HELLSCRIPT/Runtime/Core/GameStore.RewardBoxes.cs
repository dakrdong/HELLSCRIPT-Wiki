using System.Collections.Generic;

namespace Hellscript
{
    public sealed partial class GameStore
    {
        public IReadOnlyList<Item> LastBoxEquipment {get;private set;}=System.Array.Empty<Item>();
        public bool ClaimRiftFirstRewards(string request,int stage)
            =>Transact(request,"first-clear-boxes:"+stage,a=>RewardBoxes.Claim(a,stage));
        public bool OpenRewardBox(string request,string boxId,int count=1,string choice="")
        {
            LastBoxEquipment=System.Array.Empty<Item>();
            var items=new List<Item>();
            bool success=Transact(request,"reward-box:"+Data.Hero.id+":"+boxId+":"+count+":"+(choice??""),
                a=>RewardBoxes.Open(a,boxId,count,choice??"",items));
            if(success)LastBoxEquipment=items.AsReadOnly();return success;
        }
    }
}
