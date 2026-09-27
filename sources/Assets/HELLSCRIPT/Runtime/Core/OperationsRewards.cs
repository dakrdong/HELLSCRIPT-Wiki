using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Hellscript
{
    [Serializable] public sealed class OperationsPackage
    {
        public string id,nameKo,nameEn,status,reasonKo,reasonEn;
        public int minimumStage;
        public RewardBoxGrant[] grants;
        public string Name=>Loc.Language=="en"?nameEn:nameKo;
    }
    [Serializable] public sealed class OperationsRewardDatabase
    {public int version;public bool automaticDistribution;public OperationsPackage[] packages;}

    // Package IDs are revisioned, append-only promises. Events choose when/whom to grant.
    public static class OperationsRewards
    {
        static OperationsRewardDatabase database;
        public static OperationsRewardDatabase Data
        {
            get
            {
                if(database!=null)return database;
                var source=Resources.Load<TextAsset>("Data/OperationsRewards");
                if(source==null)throw new InvalidOperationException("Missing operations rewards.");
                var next=JsonUtility.FromJson<OperationsRewardDatabase>(source.text);Validate(next);database=next;return database;
            }
        }
        public static IReadOnlyList<OperationsPackage> All=>Data.packages;
        public static OperationsPackage Find(string id)=>All.FirstOrDefault(p=>p.id==id);
        public static bool ValidId(string id)=>id!=null&&Regex.IsMatch(id,@"\A[a-z0-9][a-z0-9._-]{0,119}\z");
        public static void Validate(OperationsRewardDatabase data)
        {
            if(data==null||data.version!=1||data.automaticDistribution||data.packages==null||data.packages.Length==0)
                throw new NotSupportedException("Unsupported operations rewards.");
            var ids=new HashSet<string>();
            foreach(var p in data.packages)
            {
                if(p==null||!ValidId(p.id)||!ids.Add(p.id)||string.IsNullOrWhiteSpace(p.nameKo)||string.IsNullOrWhiteSpace(p.nameEn)||p.minimumStage<1||p.minimumStage>1000||p.grants==null)
                    throw new InvalidOperationException("Invalid operations package.");
                if(p.status=="reserved")
                {
                    if(p.grants.Length!=0||string.IsNullOrWhiteSpace(p.reasonKo)||string.IsNullOrWhiteSpace(p.reasonEn))throw new InvalidOperationException("Reserved rewards must not be payable.");
                    continue;
                }
                if(p.status!="ready"||p.grants.Length<1||p.grants.Length>16||!Regex.IsMatch(p.id,@"-v[1-9][0-9]*\z"))throw new InvalidOperationException("Unversioned operations package.");
                LiveOpsConfig.ValidateGrants(p.grants,p.minimumStage);
                if(p.grants.Select(g=>g.boxId).Distinct().Count()!=p.grants.Length)throw new InvalidOperationException("Duplicate package grant.");
                foreach(var g in p.grants)
                    if(RewardBoxCatalog.Find(g.boxId).kind!="equipment"&&g.minimumQuality!=0)throw new InvalidOperationException("Quality applies only to equipment.");
            }
        }
    }

    public sealed partial class GameStore
    {
        // Trusted local/domain entrypoint, not an authenticated remote operator endpoint.
        // deliveryId must identify the account entitlement, independently of UI retries.
        public bool GrantOperationsPackage(string deliveryId,string packageId,int sourceStage)
        {
            if(!OperationsRewards.ValidId(deliveryId))
            {Error=Loc.T("운영 지급 ID가 올바르지 않습니다.");return false;}
            var package=OperationsRewards.Find(packageId);
            if(package==null||package.status!="ready"||sourceStage<package.minimumStage||sourceStage>1000)
            {Error=Loc.T("운영 보상 지급 조건을 확인해 주세요.");return false;}
            var grants=package.grants.Select(CombatJournal.Copy).ToArray();
            string fingerprint=CombatJournalArchive.Hash(JsonUtility.ToJson(package));
            return Transact("operations:"+deliveryId,"operations:"+packageId+":"+sourceStage+":"+fingerprint,a=>
            {
                if(a.suspendedRun!=null)return false;
                int cleared=Math.Max(1,a.heroes.SelectMany(h=>h.riftProgress.best).Where(b=>b.milliseconds>0).Select(b=>b.stage).DefaultIfEmpty(1).Max());
                if(sourceStage>cleared)return false;
                LiveOpsConfig.ValidateGrants(grants,sourceStage);
                int index=0;
                foreach(var grant in grants)
                {
                    string id="operations-"+deliveryId+"-"+index++;
                    if(a.rewardBoxes.owned.Any(b=>b.id==id))return false;
                    a.rewardBoxes.owned.Add(new OwnedRewardBox{id=id,boxId=grant.boxId,sourcePackage=packageId,count=grant.count,
                        sourceStage=sourceStage,itemLevel=RewardBoxCatalog.ItemLevel(sourceStage),minimumQuality=grant.minimumQuality,
                        seed=RuneEconomy.Seed(Guid.NewGuid().ToString("N"))|1u});
                }
                return true;
            });
        }
    }
}
