// Adapted from ProjectPackbound; see Docs/Implementation/Rune_Growth_Import.json.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hellscript.Runes
{
    /// <summary>Flat-top axial coordinates; positive diagram half-Y points down.</summary>
    public readonly struct HexCell : IEquatable<HexCell>
    {
        public int Q { get; }
        public int R { get; }
        public int DiagramX => Q;
        public int DiagramHalfY => 2 * R + Q;
        public HexCell(int q, int r) { Q = q; R = r; }
        public static HexCell FromDiagram(int x, int halfY)
        {
            if ((halfY - x) % 2 != 0) throw new ArgumentException("Diagram coordinates must share the board parity.");
            return new HexCell(x, (halfY - x) / 2);
        }
        public HexCell RotateClockwise(int turns)
        {
            int q = Q, r = R;
            for (int i = 0, count = ((turns % 6) + 6) % 6; i < count; i++)
            { int previousQ = q; q = -r; r = previousQ + r; }
            return new HexCell(q, r);
        }
        public int DistanceTo(HexCell other)
        {
            int q = Q - other.Q, r = R - other.R;
            return Math.Max(Math.Abs(q), Math.Max(Math.Abs(r), Math.Abs(q + r)));
        }
        public static HexCell operator +(HexCell a, HexCell b) => new HexCell(a.Q + b.Q, a.R + b.R);
        public static HexCell operator -(HexCell a, HexCell b) => new HexCell(a.Q - b.Q, a.R - b.R);
        public bool Equals(HexCell other) => Q == other.Q && R == other.R;
        public override bool Equals(object obj) => obj is HexCell other && Equals(other);
        public override int GetHashCode() => (Q * 397) ^ R;
        public override string ToString() => "(" + Q + ", " + R + ")";
    }

    public enum RuneColor { Slate, Ivory, Green, Sky, Violet, Gold, Rose }
    public enum RuneCellKind { Normal, StructuralRare, NumericRare }
    public enum RuneCombatStatus { Applied, NumericOnlyScatterIncomplete, Incomplete, Undefined }
    public enum RuneEffectTarget { None, Weapon, Wearer, Support, FixedEquipment, FirstSkillDamage, SecondSkillControl, ThirdSkillDuration }

    public static class RuneColors
    {
        private static readonly string[] names = { "먹빛", "상아", "초록", "하늘", "보라", "황금", "장미" };
        private static readonly string[] hex = { "#59616B", "#E8DEC4", "#62C96B", "#3DAEE9", "#9B6BE3", "#F3B33D", "#F05AA6" };
        public static string KoreanName(int grade) { CheckGrade(grade); return names[grade]; }
        public static string EnglishName(int grade) { CheckGrade(grade); return ((RuneColor)grade).ToString(); }
        public static string HexColor(int grade) { CheckGrade(grade); return hex[grade]; }
        internal static void CheckGrade(int grade) { if (grade < 0 || grade > 6) throw new ArgumentOutOfRangeException(nameof(grade)); }
    }

    public sealed class RuneShapeDefinition
    {
        public string Id { get; }
        public int ShapeNumber { get; }
        public IReadOnlyList<HexCell> Cells { get; }
        public int Size => Cells.Count;
        public RuneShapeDefinition(string id, int shapeNumber, IEnumerable<HexCell> cells)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Shape id is required.");
            var values = new List<HexCell>(cells ?? throw new ArgumentNullException(nameof(cells)));
            if (values.Count < 1 || values.Count > 5 || new HashSet<HexCell>(values).Count != values.Count)
                throw new ArgumentException("A rune shape must have 1–5 unique cells.");
            var reached = new HashSet<HexCell> { values[0] };
            bool changed;
            do
            {
                changed = false;
                foreach (var cell in values)
                    if (!reached.Contains(cell))
                        foreach (var neighbor in values)
                            if (reached.Contains(neighbor) && cell.DistanceTo(neighbor) == 1)
                            { reached.Add(cell); changed = true; break; }
            } while (changed);
            if (reached.Count != values.Count) throw new ArgumentException("A rune shape must be edge-connected.");
            Id = id; ShapeNumber = shapeNumber; Cells = values.AsReadOnly();
        }
        public IReadOnlyList<HexCell> RotatedCells(int clockwiseTurns)
        {
            var result = new List<HexCell>(Size);
            foreach (var cell in Cells) result.Add(cell.RotateClockwise(clockwiseTurns));
            return result.AsReadOnly();
        }
    }

    public sealed class RunePiece
    {
        public string InstanceId { get; }
        public int Grade { get; }
        public RuneColor Color => (RuneColor)Grade;
        public int Type { get; }
        public RuneShapeDefinition Shape { get; }
        public RunePiece(string instanceId, int grade, RuneShapeDefinition shape, int type = -1)
        {
            if (string.IsNullOrWhiteSpace(instanceId)) throw new ArgumentException("Rune instance id is required.");
            RuneColors.CheckGrade(grade);
            if(type < -1 || type > 4) throw new ArgumentOutOfRangeException(nameof(type));
            Type = type < 0 ? grade % 5 : type;
            InstanceId = instanceId; Grade = grade; Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }
    }

    public sealed class RunePlacement
    {
        public RunePiece Piece { get; }
        public string InstanceId => Piece.InstanceId;
        public HexCell Anchor { get; }
        public int Rotation { get; }
        public IReadOnlyList<HexCell> OccupiedCells { get; }
        public RunePlacement(RunePiece piece, HexCell anchor, int rotation)
        {
            Piece = piece ?? throw new ArgumentNullException(nameof(piece)); Anchor = anchor;
            Rotation = ((rotation % 6) + 6) % 6;
            var cells = new List<HexCell>(piece.Shape.Size);
            foreach (var cell in piece.Shape.Cells) cells.Add(anchor + cell.RotateClockwise(Rotation));
            OccupiedCells = cells.AsReadOnly();
        }
    }

    public sealed class RuneAbilityDefinition
    {
        public string DefinitionId { get; }
        public string ItemId { get; }
        public string Name { get; }
        public string Meaning { get; }
        public RuneEffectTarget Target { get; }
        public string TargetText { get; }
        public string TierValuesText { get; }
        public string ConditionText { get; }
        public string StatusText { get; }
        public RuneCombatStatus Status { get; }
        public RuneAbilityDefinition(string itemId, string name, string meaning, RuneEffectTarget target,
            string targetText, string tierValuesText, string conditionText, string statusText, RuneCombatStatus status, string definitionId = null)
        {
            DefinitionId = definitionId ?? itemId + "/" + name;
            ItemId = itemId; Name = name; Meaning = meaning; Target = target; TargetText = targetText;
            TierValuesText = tierValuesText; ConditionText = conditionText; StatusText = statusText; Status = status;
        }
    }

    public sealed class RuneBoardCell
    {
        public string SourceId {get; internal set;}
        public string Glyph {get; internal set;}
        public int AbilityType {get; internal set;}
        public bool IsElite {get; internal set;}
        public bool Initial {get; internal set;}
        public HexCell Coordinate { get; }
        public int RegionGrade { get; }
        public bool IsStart { get; }
        public int RequiredRuneGrade { get; }
        public int NumericGrade { get; }
        public RuneCellKind Kind { get; }
        public string AbilityName { get; }
        public string ValueText { get; }
        public float Value { get; }
        public bool HasNumericValue { get; }
        public string Unit { get; }
        public string StatusText { get; }
        public RuneCombatStatus Status { get; }
        public RuneAbilityDefinition Ability { get; }
        public RuneBoardCell(HexCell coordinate, int regionGrade, bool isStart, int requiredRuneGrade,
            int numericGrade, RuneCellKind kind, string abilityName, string valueText, float value,
            bool hasNumericValue, string unit, string statusText, RuneCombatStatus status, RuneAbilityDefinition ability)
        {
            RuneColors.CheckGrade(regionGrade); RuneColors.CheckGrade(requiredRuneGrade); RuneColors.CheckGrade(numericGrade);
            Coordinate = coordinate; RegionGrade = regionGrade; IsStart = isStart; RequiredRuneGrade = requiredRuneGrade;
            NumericGrade = numericGrade; Kind = kind; AbilityName = abilityName; ValueText = valueText; Value = value;
            HasNumericValue = hasNumericValue; Unit = unit; StatusText = statusText; Status = status; Ability = ability;
        }
    }

    public sealed class RuneBoardDefinition
    {
        private readonly Dictionary<HexCell, RuneBoardCell> byCoordinate = new Dictionary<HexCell, RuneBoardCell>();
        private readonly Dictionary<int, HexCell> starts = new Dictionary<int, HexCell>();
        public bool IsV13 => SourceReleaseId == "hellscript-rune-v13";
        public IReadOnlyCollection<HexCell> OpenCells {get; private set;}
        public bool IsOpen(RuneBoardCell cell,int grade) => IsV13 ? OpenCells == null || OpenCells.Contains(cell.Coordinate) : cell.RegionGrade <= grade;
        public RuneBoardDefinition WithOpenCells(IEnumerable<HexCell> cells)
        { var board = new RuneBoardDefinition(ItemId,ItemName,BlueprintNumber,Cells,SourceReleaseId,SourceSeed,SourceBoardHash); board.OpenCells = new HashSet<HexCell>(cells); return board; }
        public string ItemId { get; }
        public string ItemName { get; }
        public int BlueprintNumber { get; }
        public string SourceReleaseId { get; }
        public string SourceSeed { get; }
        public string SourceBoardHash { get; }
        public IReadOnlyList<RuneBoardCell> Cells { get; }
        public RuneBoardDefinition(string itemId, string itemName, int blueprintNumber, IEnumerable<RuneBoardCell> cells,
            string sourceReleaseId = "", string sourceSeed = "", string sourceBoardHash = "")
        {
            if (blueprintNumber < 1 || blueprintNumber > 10) throw new ArgumentOutOfRangeException(nameof(blueprintNumber));
            ItemId = itemId; ItemName = itemName; BlueprintNumber = blueprintNumber;
            SourceReleaseId = sourceReleaseId; SourceSeed = sourceSeed; SourceBoardHash = sourceBoardHash;
            var values = new List<RuneBoardCell>(cells);
            foreach (var cell in values)
            {
                byCoordinate.Add(cell.Coordinate, cell);
                if (cell.IsStart) starts.Add(cell.RegionGrade, cell.Coordinate);
            }
            Cells = values.AsReadOnly();
        }
        public bool TryGetCell(HexCell coordinate, out RuneBoardCell cell) => byCoordinate.TryGetValue(coordinate, out cell);
        public bool TryGetStart(int grade, out HexCell coordinate) => starts.TryGetValue(grade, out coordinate);
    }
}
