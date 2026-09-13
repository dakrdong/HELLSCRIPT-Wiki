using System;
using System.Collections.Generic;
using System.Linq;
using Hellscript.Runes;

namespace Hellscript
{
    public sealed class RuneDropBand
    {
        public readonly int Grade, MinimumStage, MaximumStage;
        public readonly IReadOnlyList<int> SizePercent;
        public RuneDropBand(int grade,int minimumStage,int maximumStage,params int[] sizePercent)
        {
            if(grade<0||grade>6||minimumStage<1||maximumStage<minimumStage||sizePercent.Length!=5||sizePercent.Any(p=>p<0)||sizePercent.Sum()!=100)
                throw new ArgumentException("Invalid rune drop band.");
            Grade=grade;MinimumStage=minimumStage;MaximumStage=maximumStage;SizePercent=Array.AsReadOnly((int[])sizePercent.Clone());
        }
        public int RollSize(int percentile)
        {
            if(percentile<0||percentile>=100)throw new ArgumentOutOfRangeException(nameof(percentile));
            for(int i=0;i<5;i++){percentile-=SizePercent[i];if(percentile<0)return i+1;}
            throw new InvalidOperationException("Rune size weights must sum to 100.");
        }
    }

    // One balance definition drives runtime rewards, the probability screen and the wiki export.
    public static class RuneEconomy
    {
        public const int FusionMaterials=2,MaximumFusionPairs=100;
        public static readonly IReadOnlyList<RuneDropBand> Bands=Array.AsReadOnly(new[]{
            new RuneDropBand(0,1,4,100,0,0,0,0),
            new RuneDropBand(1,5,9,80,20,0,0,0),
            new RuneDropBand(2,10,14,45,40,15,0,0),
            new RuneDropBand(3,15,19,15,35,35,15,0),
            new RuneDropBand(4,20,24,5,15,40,30,10),
            new RuneDropBand(5,25,29,0,5,30,40,25),
            new RuneDropBand(6,30,int.MaxValue,0,0,20,40,40)});
        public static RuneDropBand Band(int stage)=>Bands[Math.Min(6,Math.Max(1,stage)/5)];
        public static int DropPercent(RiftRewardSource source)
        {
            switch(source){case RiftRewardSource.Normal:return 2;case RiftRewardSource.Elite:return 20;case RiftRewardSource.Boss:return 100;default:throw new ArgumentOutOfRangeException(nameof(source));}
        }
        public static int DropCount(RiftRewardSource source)=>source==RiftRewardSource.Boss?4:1;
        public static bool Drops(RiftRewardSource source,int percentile)
        {if(percentile<0||percentile>=100)throw new ArgumentOutOfRangeException(nameof(percentile));return percentile<DropPercent(source);}
        public static uint Seed(string receipt)
        {uint rng=2166136261;foreach(char c in receipt)rng=unchecked((rng^c)*16777619);return rng;}
        public static OwnedRune RollRune(int stage,string id,ref uint rng)
        {
            var band=Band(stage);int size=band.RollSize(RandomStream.Range(ref rng,0,100));
            var shapes=RuneMasteryCatalog.Shapes.Where(s=>s.Size==size).ToArray();
            return new OwnedRune{id=id,grade=band.Grade,shapeId=shapes[RandomStream.Range(ref rng,0,shapes.Length)].Id};
        }
        public static bool FusionOutput(int grade,int size,out int nextGrade,out int nextSize)
        {
            nextGrade=grade;nextSize=size;
            if(grade<0||grade>6||size<1||size>5||grade==6&&size==5)return false;
            nextGrade=size==5?grade+1:grade;nextSize=size==5?1:size+1;return true;
        }
    }
}
