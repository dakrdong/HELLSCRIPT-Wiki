using System.Collections.Generic;
using System.Linq;

namespace Hellscript
{
    public sealed partial class GameStore
    {
        public System.Func<int,LiveOpsRunSnapshot> LiveOpsPreview;
        public System.Func<int> LiveOpsPreviewVersion;
        public RewardBoxGrant[] FirstClearRewards(int stage)=>RewardBoxes.Preview(Data,stage,LiveOpsPreview?.Invoke(stage));
        public IReadOnlyList<Item> LastBoxEquipment {get;private set;}=System.Array.Empty<Item>();
        RewardBoxReceipt lastBoxReceipt;
        public RewardBoxReceipt LastBoxReceipt=>lastBoxReceipt==null?null:RuneGrowth.Copy(lastBoxReceipt);
        public bool ClaimRiftFirstRewards(string request,int stage)
            =>Transact(request,"first-clear-boxes:"+stage,a=>RewardBoxes.Claim(a,stage));
        public bool OpenRewardBox(string request,string boxId,int count=1,string choice="")
        {
            LastBoxEquipment=System.Array.Empty<Item>();
            lastBoxReceipt=null;
            var items=new List<Item>();
            bool success=Transact(request,"reward-box:"+Data.Hero.id+":"+boxId+":"+count+":"+(choice??""),
                a=>
                {
                    var receipt=new RewardBoxReceipt{request=request};
                    if(!RewardBoxes.Open(a,boxId,count,choice??"",items,receipt))return false;
                    a.rewardBoxes.openingReceipts.Add(receipt);
                    if(a.rewardBoxes.openingReceipts.Count>20)a.rewardBoxes.openingReceipts.RemoveAt(0);
                    return true;
                });
            if(success)
            {
                var saved=Data.rewardBoxes.openingReceipts.FirstOrDefault(r=>r.request==request);
                lastBoxReceipt=saved==null?null:RuneGrowth.Copy(saved);
                LastBoxEquipment=lastBoxReceipt?.equipment.AsReadOnly()??items.AsReadOnly();
            }
            return success;
        }
    }
}
