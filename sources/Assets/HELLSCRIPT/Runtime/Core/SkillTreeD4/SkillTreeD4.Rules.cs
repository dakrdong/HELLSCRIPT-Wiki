// SkillTreeD4 part 2: the point rules (plan chapter 6).
// C# port of golden/reference/rules.js. Verified against rules_golden.json: every possible invest/refund on 288
// allocations at 16 hero levels plus 48 random replays, and the recorded interaction transcripts.
//
// Everything the player can do with points goes through Apply(). The skill window, the battle quick edit and the
// edict code must all call this one engine.
//
//   points available  = heroLevel - spent            (1 point per hero level, the first one at level 1)
//   spent             = sum over actives of (rank + modifier rank) + sum of passive ranks
//   Before(i)         = spent - (points in tiers i..4)     "points spent outside tiers i and above"
//   a tier is OPEN when Before(i) >= gate[i] or it already holds a point (display only; investing re-checks Before(i))
//
// A blocked invest reports the FIRST failing rule, in this order:
//   active:    Tier (Need = missing points) -> Max -> Pts
//   modifier:  Tier (Need) -> Base (active rank < 1) -> Swap (another modifier is chosen) -> Max/Cap -> Pts
//   passive:   Lv (Need = required level) -> Max -> Key (the other key passive holds points) -> Pts
// "Max" vs "Cap" for a modifier: the modifier is at min(max rank, active rank); it is "Max" when that is the max rank
// and "Cap" when only the active's current rank holds it back.
using System;
using System.Linq;

namespace Hellscript
{
    public enum SkillTreeD4Action { Invest, Refund }

    /// <summary>Why a point could not be invested or refunded. Nothing/Gate only occur for refunds.</summary>
    public enum SkillTreeD4Block { None, Tier, Lv, Max, Cap, Pts, Base, Swap, Key, Nothing, Gate }

    public sealed class SkillTreeD4Result
    {
        public bool Ok;
        public SkillTreeD4Alloc Alloc;       // the new allocation when Ok, otherwise the unchanged input
        public SkillTreeD4Block Reason;      // None when Ok
        public int Need;                     // Tier: points still missing; Lv: the required level
    }

    public sealed class SkillTreeD4Rules
    {
        public const int PassiveMax = 5;
        readonly SkillTreeD4Model model;
        public SkillTreeD4Rules(SkillTreeD4Model model) { this.model = model; }

        /// <summary>Points held by each tier's actives (rank + modifier rank). Passives are not in a tier.</summary>
        public int[] TierPoints(string hero, SkillTreeD4Alloc al)
        {
            var t = new int[5];
            var actives = model.Classes[hero].Actives;
            for (int n = 0; n < actives.Length; n++) t[actives[n].Tier] += al.Actives[n].Rank + al.Actives[n].ModifierRank;
            return t;
        }
        public int Spent(string hero, SkillTreeD4Alloc al) { return TierPoints(hero, al).Sum() + al.Passives.Sum(); }
        public int Before(string hero, SkillTreeD4Alloc al, int tier)
        {
            var tp = TierPoints(hero, al);
            int s = Spent(hero, al);
            for (int j = tier; j < 5; j++) s -= tp[j];
            return s;
        }
        public bool[] TierOpen(string hero, SkillTreeD4Alloc al)
        {
            var tp = TierPoints(hero, al); var open = new bool[5];
            for (int i = 0; i < 5; i++) open[i] = Before(hero, al, i) >= model.Gates[i] || tp[i] > 0;
            return open;
        }
        /// <summary>Every tier that holds points must still meet its gate.</summary>
        public bool Valid(string hero, SkillTreeD4Alloc al)
        {
            var tp = TierPoints(hero, al);
            for (int i = 0; i < 5; i++) if (tp[i] > 0 && Before(hero, al, i) < model.Gates[i]) return false;
            return true;
        }

        /// <summary>One invest or refund on the selected node. The input allocation is never changed.</summary>
        public SkillTreeD4Result Apply(SkillTreeD4Action action, string hero, int level, SkillTreeD4Alloc alloc, SkillTreeD4Selection sel)
        {
            var c = model.Classes[hero]; var gates = model.Gates; var al = alloc.Clone();
            Func<SkillTreeD4Block, int, SkillTreeD4Result> no = (r, n) => new SkillTreeD4Result { Ok = false, Alloc = alloc, Reason = r, Need = n };
            Func<SkillTreeD4Result> yes = () => new SkillTreeD4Result { Ok = true, Alloc = al, Reason = SkillTreeD4Block.None };

            if (action == SkillTreeD4Action.Invest)
            {
                bool pts = level - Spent(hero, al) > 0;
                if (!sel.Passive)
                {
                    var a = c.Actives[sel.Index]; var x = al.Actives[sel.Index]; int b = Before(hero, al, a.Tier);
                    if (b < gates[a.Tier]) return no(SkillTreeD4Block.Tier, gates[a.Tier] - b);
                    if (sel.Modifier < 0)
                    {
                        if (x.Rank >= a.MaxRank) return no(SkillTreeD4Block.Max, 0);
                        if (!pts) return no(SkillTreeD4Block.Pts, 0);
                        x.Rank++; al.Actives[sel.Index] = x; return yes();
                    }
                    if (x.Rank < 1) return no(SkillTreeD4Block.Base, 0);
                    if (x.Modifier != -1 && x.Modifier != sel.Modifier) return no(SkillTreeD4Block.Swap, 0);
                    if (x.Modifier == sel.Modifier && x.ModifierRank >= Math.Min(a.MaxRank, x.Rank)) return no(x.ModifierRank >= a.MaxRank ? SkillTreeD4Block.Max : SkillTreeD4Block.Cap, 0);
                    if (!pts) return no(SkillTreeD4Block.Pts, 0);
                    if (x.Modifier == -1) { x.Modifier = sel.Modifier; x.ModifierRank = 1; }
                    else x.ModifierRank++;
                    al.Actives[sel.Index] = x; return yes();
                }
                var p = c.Passives[sel.Index];
                if (level < p.UnlockLevel) return no(SkillTreeD4Block.Lv, p.UnlockLevel);
                if (al.Passives[sel.Index] >= PassiveMax) return no(SkillTreeD4Block.Max, 0);
                if (p.Key && c.Passives.Where((q, k) => q.Key && k != sel.Index && al.Passives[k] > 0).Any()) return no(SkillTreeD4Block.Key, 0);
                if (!pts) return no(SkillTreeD4Block.Pts, 0);
                al.Passives[sel.Index]++; return yes();
            }

            /* refund: the target must hold points, an active can not drop below its chosen modifier, and no tier may be
               left holding points without meeting its gate */
            if (!sel.Passive)
            {
                var x = al.Actives[sel.Index];
                if (sel.Modifier < 0)
                {
                    if (x.Rank <= 0 || x.Rank <= x.ModifierRank) return no(SkillTreeD4Block.Nothing, 0);
                    x.Rank--;
                }
                else
                {
                    if (x.Modifier != sel.Modifier || x.ModifierRank <= 0) return no(SkillTreeD4Block.Nothing, 0);
                    x.ModifierRank--; if (x.ModifierRank == 0) x.Modifier = -1;
                }
                al.Actives[sel.Index] = x;
            }
            else
            {
                if (al.Passives[sel.Index] <= 0) return no(SkillTreeD4Block.Nothing, 0);
                al.Passives[sel.Index]--;
            }
            return Valid(hero, al) ? yes() : no(SkillTreeD4Block.Gate, 0);
        }
    }
}
