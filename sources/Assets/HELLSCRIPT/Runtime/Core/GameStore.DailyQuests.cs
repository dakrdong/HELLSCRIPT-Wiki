using System;

namespace Hellscript
{
    public sealed partial class GameStore
    {
        readonly Func<long> dailyQuestTime;
        public bool RefreshDailyQuests()
        {
            long day=DailyQuests.Day(AttendanceClock());
            if(day<Data.dailyQuests.day){Error=Loc.T("날짜가 이전 일일 퀘스트보다 이릅니다. 기기의 시간을 확인해 주세요.");return false;}
            if(day==Data.dailyQuests.day&&!DailyQuests.NeedsTargetUpdate(Data.dailyQuests))return true;
            string request="daily-quests-prepare:"+day+":catalog:"+DailyQuests.Configuration.version;
            return Transact(request,request,a=>DailyQuests.Prepare(a,AttendanceClock()));
        }
        public bool ClaimDailyQuest(string id,long expectedDay)
        {
            if(!RefreshDailyQuests())return false;
            long today=DailyQuests.Day(AttendanceClock());
            if(today!=expectedDay||today!=Data.dailyQuests.day)
            {Error=Loc.T("일일 퀘스트 날짜가 바뀌었습니다. 목록을 다시 확인해 주세요.");return false;}
            string request="daily-quest-claim:"+expectedDay+":"+id;
            return Transact(request,request,a=>
            {
                var q=a.dailyQuests.quests.Find(q=>q.id==id);
                if(q==null||q.claimed||q.progress<q.target||DailyQuests.ClaimedCoins(a.dailyQuests)+q.reward>DailyQuests.DailyReward)return false;
                a.premium=checked(a.premium+q.reward);q.claimed=true;return true;
            });
        }
    }
}
