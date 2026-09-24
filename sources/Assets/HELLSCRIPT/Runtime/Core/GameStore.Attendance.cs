using System;
using System.Linq;

namespace Hellscript
{
    public sealed partial class GameStore
    {
        public Func<long> AttendanceClock=()=>DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public bool RecordAttendance()
        {
            long day=Attendance.Day(AttendanceClock());
            if(day==Data.attendance.lastDay)return true;
            if(day<Data.attendance.lastDay){Error=Loc.T("날짜가 이전 출석보다 이릅니다. 기기의 시간을 확인해 주세요.");return false;}
            return Transact("attendance-visit:"+day,"attendance-visit:"+day,a=>Attendance.Visit(a,day));
        }
        public bool ClaimAttendance(AttendanceKind kind,long period,int day)
        {
            if(!Attendance.Valid(kind)||day<1||day>Attendance.Length(kind))return false;
            long today=Attendance.Day(AttendanceClock());
            if(today<Data.attendance.lastDay||period!=Attendance.Period(kind,today))
            {Error=Loc.T("출석 기간이 바뀌었습니다. 보상 목록을 다시 확인해 주세요.");return false;}
            string request="attendance-claim:"+kind+":"+period+":"+day;
            string reason="";
            bool result=Transact(request,request,a=>
            {
                if(!Attendance.Visit(a,today))return false;
                var track=Attendance.Track(a.attendance,kind);
                if(track.period!=period||day>track.earned||track.Claimed(day))return false;
                if(!Attendance.Grant(a,kind,day,request)){reason="보석 보관함의 공간을 확보한 뒤 다시 받아 주세요.";return false;}
                track.claimed|=1<<(day-1);return true;
            });
            if(!result&&reason!="")Error=Loc.T(reason);return result;
        }
        public bool OpenAttendanceChest(string id)
        {
            string request="attendance-open:"+id;
            return Transact(request,request,a=>
            {
                var chest=a.attendance.chests.FirstOrDefault(c=>c.id==id);if(chest==null||Economy.FreeSlots(a.Hero)<1)return false;
                uint seed=chest.seed;int slot=(int)(RandomStream.Next(ref seed)%8);
                var item=ItemGenerator.Create(chest.heroClass,slot,3,chest.level,ref seed,id:request);
                if(!Economy.AddItem(a.Hero,item,BagPolicy.Ignore,a))return false;
                a.attendance.chests.Remove(chest);return true;
            });
        }
    }
}
