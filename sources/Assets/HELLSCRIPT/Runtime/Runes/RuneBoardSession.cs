using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Hellscript.Runes
{
    // Account-wide editing session. View changes, rotations and camera changes never mutate the owned account.
    public sealed class RuneBoardSession
    {
        public RuneGrowthState Draft {get;private set;}
        public RuneGrowthState Saved {get;private set;}
        public RuneInventory Inventory {get;private set;}
        public RuneBoardEditor Editor {get;private set;}
        public string Weapon {get;private set;}
        public string SelectedId {get;private set;}
        public HexCell? SelectedCell;
        public int Rotation {get;private set;}
        public bool Viewing;
        public int Version {get;private set;}
        public int UndoCount=>history.Count;
        public RunePiece Selected=>SelectedId==null?null:Inventory.FindOwned(SelectedId);
        public RuneWeaponProgress Progress=>RuneMasteryProgress.Get(Draft,Weapon);
        public bool Dirty=>Signature(Draft)!=Signature(Saved);
        public bool SelectedPlaced=>SelectedId!=null&&Editor.IsInDraft(SelectedId);
        readonly List<RuneGrowthState> history=new List<RuneGrowthState>();
        readonly Func<RunePiece,int> initialRotation;
        readonly Dictionary<string,int> turns=new Dictionary<string,int>();
        string anchorSignature;IReadOnlyList<RuneAnchorPreview> anchors=Array.Empty<RuneAnchorPreview>();
        public RuneBoardSession(RuneGrowthState owned,string weapon="sword",Func<RunePiece,int> initialRotation=null) {this.initialRotation=initialRotation;Weapon=weapon;Reload(owned);}
        static string Signature(RuneGrowthState s)=>JsonUtility.ToJson(new RuneGrowthState{placements=s.placements.OrderBy(p=>p.runeId,StringComparer.Ordinal).ToList(),mastery=s.mastery,presets=s.presets});
        public bool WeaponDirty(string weapon)=>JsonUtility.ToJson(RuneMasteryProgress.Get(Draft,weapon))!=JsonUtility.ToJson(RuneMasteryProgress.Get(Saved,weapon))||JsonUtility.ToJson(new RunePreset{placements=Draft.placements.Where(p=>p.weapon==weapon).ToList()})!=JsonUtility.ToJson(new RunePreset{placements=Saved.placements.Where(p=>p.weapon==weapon).ToList()});
        public void Reload(RuneGrowthState owned){Saved=RuneGrowth.Copy(owned);Draft=RuneGrowth.Copy(owned);history.Clear();ClearSelection();Rebuild();}
        void Rebuild(){Inventory=RuneGrowth.Restore(Draft,6);Editor=Inventory.BeginEdit(Weapon);Version++;anchorSignature=null;}
        public void ClearSelection(){SelectedId=null;SelectedCell=null;Rotation=0;anchorSignature=null;}
        public void SwitchWeapon(string weapon){if(!RuneMasteryCatalog.IsWeapon(weapon))return;Weapon=weapon;ClearSelection();Editor=Inventory.BeginEdit(weapon);Version++;}
        public void SetMode(bool view){Viewing=view;ClearSelection();}
        public static string StackKey(RunePiece p)=>p.Type+"|"+p.Shape.Id+"|"+p.Grade;
        public int StoredRotation(RunePiece p)=>turns.TryGetValue(StackKey(p),out var n)?n:initialRotation?.Invoke(p)??0;
        public IEnumerable<RunePiece> Stored=>Inventory.OwnedPieces.Where(p=>Inventory.GetCommittedOwner(p.InstanceId)==null);
        public IReadOnlyList<RuneStack> Stacks()=>Stored.GroupBy(StackKey).Select(g=>new RuneStack(g.OrderBy(p=>p.InstanceId,StringComparer.Ordinal).ToArray())).OrderBy(g=>g.Piece.Shape.Size).ThenBy(g=>g.Piece.Shape.ShapeNumber).ThenBy(g=>g.Piece.Type).ThenBy(g=>g.Piece.Grade).ToArray();
        public void Select(string id)
        {
            var p=Inventory.FindOwned(id);if(p==null)return;
            var owner=Inventory.GetCommittedOwner(id);if(owner!=null&&owner!=Weapon)return;
            Viewing=false;SelectedId=id;SelectedCell=null;
            Rotation=Editor.DraftPlacements.FirstOrDefault(v=>v.InstanceId==id)?.Rotation??StoredRotation(p);anchorSignature=null;
        }
        public RuneBoardEffects Effects()=>RuneEffectEvaluator.Evaluate(Editor.Board,6,Editor.DraftPlacements,Editor.ValidateDraft(),Weapon,true);
        public RuneCandidate Preview(string id,HexCell at,int rotation)
        {
            var piece=Inventory.FindOwned(id);if(piece==null)return new RuneCandidate{Reason=Loc.T("보유하지 않은 룬입니다.")};
            var candidate=new RunePlacement(piece,at,rotation);var validation=Editor.ValidatePlacement(id,at,rotation);
            var list=Editor.DraftPlacements.Where(p=>p.InstanceId!=id).Append(candidate).ToArray();
            var result=new RuneCandidate{Placement=candidate,Ok=validation.IsValid,Reason=RuneRefusal.Describe(validation,id,list),Size=piece.Shape.Size};
            result.ActiveCount=candidate.OccupiedCells.Count(c=>Editor.Board.TryGetCell(c,out var cell)&&validation.IsRuneValid(id)&&(cell.IsElite||cell.AbilityType==piece.Type)&&cell.Ability.Meaning!="root");return result;
        }
        public IReadOnlyList<RuneAnchorPreview> LegalAnchors()
        {
            if(Viewing||Selected==null||SelectedPlaced)return Array.Empty<RuneAnchorPreview>();
            string key=Version+"/"+SelectedId+"/"+Rotation;if(key==anchorSignature)return anchors;
            anchors=Editor.Board.Cells.Where(c=>Editor.Board.IsOpen(c,6)&&c.Ability.Meaning!="root").Select(c=>(c,p:Preview(SelectedId,c.Coordinate,Rotation))).Where(v=>v.p.Ok).Select(v=>new RuneAnchorPreview{Cell=v.c.Coordinate,Perfect=v.p.ActiveCount==v.p.Size}).ToArray();anchorSignature=key;return anchors;
        }
        void Remember(){history.Add(RuneGrowth.Copy(Draft));if(history.Count>50)history.RemoveAt(0);}
        public bool Place(string id,HexCell at,int rotation,out string reason)
        {
            var candidate=Preview(id,at,rotation);reason=candidate.Reason;if(!candidate.Ok||Viewing)return false;
            Remember();Editor.TryPlaceDraft(id,at,rotation,out reason);Editor.TryCommit(out _);Draft.placements=RuneGrowth.Capture(Inventory);Select(id);Version++;return true;
        }
        public bool Rotate(out string reason)
        {
            reason="";if(Viewing||Selected==null)return false;int next=(Rotation+1)%6;
            var placed=Editor.DraftPlacements.FirstOrDefault(p=>p.InstanceId==SelectedId);
            if(placed!=null&&!Place(SelectedId,placed.Anchor,next,out reason))return false;
            Rotation=next;turns[StackKey(Selected)]=next;anchorSignature=null;return true;
        }
        public void SetDragRotation(int value){Rotation=(value%6+6)%6;if(Selected!=null)turns[StackKey(Selected)]=Rotation;anchorSignature=null;}
        public bool Remove(out string reason)
        {
            reason="";if(Viewing||!SelectedPlaced)return false;
            var remaining=Editor.DraftPlacements.Where(p=>p.InstanceId!=SelectedId).ToArray();var valid=RunePlacementValidator.Validate(Editor.Board,6,remaining);
            if(!valid.IsValid){reason=Loc.T("이 블록을 빼면 연결이 끊어집니다. 바깥 블록부터 회수하세요.");return false;}
            Remember();turns[StackKey(Selected)]=Rotation;Editor.TryRemoveDraft(SelectedId,out reason);Editor.TryCommit(out _);Draft.placements=RuneGrowth.Capture(Inventory);Version++;return true;
        }
        public string UnlockReason(HexCell c)
        {
            if(!Editor.Board.TryGetCell(c,out var cell))return Loc.T("보드 밖입니다.");
            if(Editor.Board.IsOpen(cell,6))return Loc.T("이미 개방된 슬롯입니다.");
            if(cell.RegionGrade!=RuneMasteryProgress.Expanding(Progress))return Loc.T("먼저 이 영역을 선택하세요.");
            if(RuneMasteryProgress.Points(Progress)<=0)return Loc.T("슬롯 포인트가 부족합니다.");
            if(!RuneMasteryProgress.CanUnlock(Progress,c))return Loc.T("기존에 열린 슬롯과 변으로 연결된 칸부터 열어야 합니다.");return "";
        }
        public bool Unlock(HexCell c,out string reason){reason=UnlockReason(c);if(Viewing||reason!="")return false;Remember();RuneMasteryProgress.Unlock(Progress,c);Rebuild();return true;}
        public bool Choose(int region,out string reason){reason="";if(Viewing||!RuneMasteryProgress.CanChoose(Progress,region)){reason=Loc.T("현재 영역 37칸을 모두 개방한 뒤 선택할 수 있습니다.");return false;}Remember();RuneMasteryProgress.Choose(Progress,region);Rebuild();return true;}
        public void Undo(){if(history.Count==0)return;Draft=history[history.Count-1];history.RemoveAt(history.Count-1);ClearSelection();Rebuild();}
        public void Revert(){Draft=RuneGrowth.Copy(Saved);history.Clear();ClearSelection();Rebuild();}
        public void SavePreset(int slot,string name){if(slot<0||slot>=5)return;Remember();Draft.presets[slot]=new RunePreset{occupied=true,name=(name??"").Trim().Substring(0,Math.Min(40,(name??"").Trim().Length)),placements=Draft.placements.Select(RuneGrowth.Copy).ToList()};Version++;}
        public void ClearPreset(int slot){Remember();Draft.presets[slot]=new RunePreset();Version++;}
        public bool LoadPreset(int slot,out string reason)
        {
            reason="";var p=Draft.presets[slot];if(!p.occupied)return false;
            try{RuneGrowth.Validate(Draft,p.placements,6);}catch(Exception e){reason=e.Message;return false;}
            Remember();Draft.placements=p.placements.Select(RuneGrowth.Copy).ToList();ClearSelection();Rebuild();return true;
        }
        public bool Save(GameStore store,CombatSimulation simulation,out string reason)
        {if(!store.CommitRuneBoardState(Draft.placements,Draft.mastery,Saved.revision,simulation,Draft.presets)){reason=store.Error;return false;}Reload(store.Data.runes);reason="";return true;}
    }
    public sealed class RuneStack {public readonly RunePiece[] Pieces;public RunePiece Piece=>Pieces[0];public string Key=>RuneBoardSession.StackKey(Piece);public RuneStack(RunePiece[] pieces){Pieces=pieces;}}
    public sealed class RuneCandidate {public RunePlacement Placement;public bool Ok;public int ActiveCount,Size;public string Reason;}
    public sealed class RuneAnchorPreview {public HexCell Cell;public bool Perfect;}
    public static class RuneRefusal
    {
        static int Priority(RuneValidationCode c)=>c==RuneValidationCode.OutsideBoard?1:c==RuneValidationCode.ForeignStart?2:c==RuneValidationCode.LockedRegion?3:c==RuneValidationCode.Overlap?4:c==RuneValidationCode.SameColorEdgeContact?5:c==RuneValidationCode.DisconnectedFromStart?6:0;
        public static string Describe(RuneValidation v,string id,IEnumerable<RunePlacement> placements)
        {
            if(v.IsValid)return "";var issue=v.Issues.OrderBy(x=>Priority(x.Code)).ThenBy(x=>x.RuneId==id?0:1).First();
            switch(issue.Code)
            {
                case RuneValidationCode.OutsideBoard:return Loc.T("블럭이 보드 밖으로 나갑니다.");
                case RuneValidationCode.ForeignStart:return Loc.T("중앙 시작점은 비워 두세요.");
                case RuneValidationCode.LockedRegion:return Loc.T("아직 개방하지 않은 슬롯입니다.");
                case RuneValidationCode.Overlap:return Loc.T("다른 룬 블럭과 겹칩니다.");
                case RuneValidationCode.SameColorEdgeContact:return Loc.T("같은 색의 다른 블럭끼리는 변을 맞댈 수 없습니다.");
                case RuneValidationCode.DisconnectedFromStart:
                    var bad=placements.FirstOrDefault(p=>p.InstanceId==issue.RuneId);string color=RuneV13Catalog.TypeName(bad?.Piece.Type??0);
                    return placements.Any(p=>p.Piece.Type==bad?.Piece.Type&&p.OccupiedCells.Any(c=>c.DistanceTo(new HexCell(0,0))==1))?Loc.F("{0} 룬이 중앙과 연결되지 않습니다. 중앙 옆 또는 연결된 같은 색 룬의 한 변 간격에 놓으세요.",Loc.T(color)):Loc.F("{0} 룬의 첫 블럭은 중앙 시작점 옆에 놓으세요.",Loc.T(color));
                case RuneValidationCode.AlreadyUsedOnAnotherBoard:return Loc.T("다른 장비에 저장된 룬입니다. 해당 보드에서 먼저 회수해 저장하세요.");
                default:return Loc.T("유효하지 않거나 중복된 룬 데이터입니다.");
            }
        }
    }
}
