using UnityEngine;

namespace Hellscript
{
    // Reward definitions and account state stay with Attendance/GameStore.
    public static class AttendanceArt
    {
        public static Sprite For(AttendanceRewardKind kind)
        {
            string path=kind switch
            {
                AttendanceRewardKind.Coins=>"Art/GlobalHUD/currency-abyssal-coin",
                AttendanceRewardKind.Gold=>"Art/Attendance/reward-gold",
                AttendanceRewardKind.Cores=>"Art/Attendance/reward-random-core",
                AttendanceRewardKind.LegendaryChest=>"Art/Attendance/reward-legendary-chest",
                _=>null
            };
            return path==null?null:Resources.Load<Sprite>(path);
        }
    }
}
