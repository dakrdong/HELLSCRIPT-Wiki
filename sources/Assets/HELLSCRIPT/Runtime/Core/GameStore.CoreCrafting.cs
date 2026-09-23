using System;
using System.Linq;

namespace Hellscript
{
    public sealed partial class GameStore
    {
        public bool CraftCoreEquipment(string request,CoreCraftQuote quote,uint seed)
        {
            if(quote==null)return false;string reason="";
            bool result=Transact(request,"core-craft:"+quote.heroId+":"+quote.fingerprint+":"+quote.investment,a=>
            {reason=CoreCrafting.Check(a,quote);return reason==""&&CoreCrafting.Apply(a,quote,request,ref seed);});
            if(!result&&reason!="")Error=Loc.T(reason);return result;
        }
        public bool AcknowledgeCoreCraft(string recordId)
        {
            if(Data.coreCraft.pendingId!=recordId)return Data.coreCraft.history.Any(r=>r.id==recordId);
            return Transact("core-confirm:"+recordId,"core-confirm:"+recordId,a=>
            {if(a.coreCraft.pendingId!=recordId)return false;a.coreCraft.pendingId="";return true;});
        }
    }
}
