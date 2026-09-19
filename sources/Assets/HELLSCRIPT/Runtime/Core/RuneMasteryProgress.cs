using System;
using System.Collections.Generic;
using System.Linq;
using Hellscript.Runes;

namespace Hellscript
{
    [Serializable] public sealed class RuneWeaponProgress
    {
        public string weapon;public int level=1,xp;
        public List<string> unlocked=new List<string>();
        public List<int> order=new List<int>();
    }
    public static class RuneMasteryProgress
    {
        public const int MaximumLevel=41,PointsPerLevel=6;
        public static string Key(HexCell c)=>c.Q+","+c.R;
        public static RuneWeaponProgress Fresh(string weapon)=>new RuneWeaponProgress{weapon=weapon,unlocked=RuneMasteryCatalog.Board(weapon).Cells.Where(c=>c.Initial).Select(c=>Key(c.Coordinate)).ToList()};
        public static RuneWeaponProgress Get(RuneGrowthState state,string weapon)=>state.mastery.Single(p=>p.weapon==weapon);
        public static HashSet<HexCell> OpenCells(RuneWeaponProgress p)=>new HashSet<HexCell>(p.unlocked.Select(k=>{var parts=k.Split(',');return new HexCell(int.Parse(parts[0]),int.Parse(parts[1]));}));
        public static int Points(RuneWeaponProgress p)=>PointsPerLevel*(p.level-1)-(p.unlocked.Count-19-p.order.Count);
        public static int RegionCount(RuneWeaponProgress p,int region)=>RuneMasteryCatalog.Board(p.weapon).Cells.Count(c=>c.RegionGrade==region&&p.unlocked.Contains(Key(c.Coordinate)));
        public static int Expanding(RuneWeaponProgress p)=>RegionCount(p,0)<37?0:p.order.Count>0&&RegionCount(p,p.order.Last())<37?p.order.Last():-1;
        public static HashSet<HexCell> Connected(RuneWeaponProgress p)
        {
            var open=OpenCells(p);var reached=new HashSet<HexCell>();var queue=new Queue<HexCell>();
            if(open.Contains(new HexCell(0,0))){reached.Add(new HexCell(0,0));queue.Enqueue(new HexCell(0,0));}
            while(queue.Count>0){var cell=queue.Dequeue();foreach(var next in open)if(!reached.Contains(next)&&cell.DistanceTo(next)==1){reached.Add(next);queue.Enqueue(next);}}
            return reached;
        }
        public static bool CanUnlock(RuneWeaponProgress p,HexCell coordinate)
        {
            var board=RuneMasteryCatalog.Board(p.weapon);
            return Points(p)>0&&board.TryGetCell(coordinate,out var cell)&&!p.unlocked.Contains(Key(coordinate))&&cell.RegionGrade==Expanding(p)&&Connected(p).Any(c=>c.DistanceTo(coordinate)==1);
        }
        public static void Unlock(RuneWeaponProgress p,HexCell c)
        {if(!CanUnlock(p,c))throw new InvalidOperationException("연결된 개방 칸 옆에서 현재 영역의 칸을 선택하세요. 슬롯 포인트가 필요합니다.");p.unlocked.Add(Key(c));}
        public static bool CanChoose(RuneWeaponProgress p,int region)=>region>0&&region<7&&!p.order.Contains(region)&&Expanding(p)<0;
        public static void Choose(RuneWeaponProgress p,int region)
        {
            if(!CanChoose(p,region))throw new InvalidOperationException("현재 영역의 37칸을 모두 개방한 뒤 다음 영역을 선택하세요.");
            RuneMasteryCatalog.Board(p.weapon).TryGetStart(region,out var center);p.order.Add(region);p.unlocked.Add(Key(center));
        }
        public static void AddExperience(RuneWeaponProgress p,int amount)
        {
            if(amount<0)throw new ArgumentOutOfRangeException(nameof(amount));
            if(p.level==MaximumLevel)return;
            long xp=(long)p.xp+amount;
            while(p.level<MaximumLevel&&xp>=100*p.level){xp-=100*p.level;p.level++;}
            p.xp=p.level==MaximumLevel?0:(int)xp;
        }
        public static int KillExperience(RiftRewardSource source,int stage)=>(source==RiftRewardSource.Boss?100:source==RiftRewardSource.Elite?25:5)*(1+Math.Max(0,stage)/5);
        public static void Validate(RuneWeaponProgress p)
        {
            if(p==null||!RuneMasteryCatalog.IsWeapon(p.weapon)||p.level<1||p.level>MaximumLevel||p.xp<0||p.xp>=(p.level==MaximumLevel?1:p.level*100)||p.unlocked==null||p.order==null)
                throw new InvalidOperationException("무기 숙련도 정보가 올바르지 않습니다.");
            var board=RuneMasteryCatalog.Board(p.weapon);var keys=new HashSet<string>(board.Cells.Select(c=>Key(c.Coordinate)));
            if(p.unlocked.Count!=p.unlocked.Distinct().Count()||p.unlocked.Any(k=>!keys.Contains(k))||board.Cells.Any(c=>c.Initial&&!p.unlocked.Contains(Key(c.Coordinate)))||p.order.Distinct().Count()!=p.order.Count||p.order.Any(r=>r<1||r>6)||Points(p)<0)
                throw new InvalidOperationException("개방 칸과 슬롯 포인트를 확인하세요.");
            if(p.order.Count>0&&RegionCount(p,0)!=37||p.order.Take(Math.Max(0,p.order.Count-1)).Any(r=>RegionCount(p,r)!=37))throw new InvalidOperationException("완료되지 않은 영역이 개방 순서에 있습니다.");
            var free=new HashSet<HexCell>();foreach(int region in p.order){board.TryGetStart(region,out var center);free.Add(center);if(!p.unlocked.Contains(Key(center)))throw new InvalidOperationException("선택한 영역의 중앙 칸이 없습니다.");}
            var connected=Connected(p);
            foreach(var c in OpenCells(p))
            {board.TryGetCell(c,out var cell);if(cell.RegionGrade>0&&!p.order.Contains(cell.RegionGrade)||!free.Contains(c)&&!connected.Contains(c))throw new InvalidOperationException("시작점과 연결되지 않은 개방 칸입니다.");}
        }
        public static void GrantKill(RuneGrowthState state,string weapon,RiftRewardSource source,int stage)
        {if(weapon!=null)AddExperience(Get(state,weapon),KillExperience(source,stage));}
    }
}
