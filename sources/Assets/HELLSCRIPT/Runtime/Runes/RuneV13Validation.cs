using System;
using System.Collections.Generic;
using System.Linq;

namespace Hellscript.Runes
{
    public static class RuneV13Validation
    {
        static readonly HexCell[] neighbors={new HexCell(1,0),new HexCell(0,1),new HexCell(-1,1),new HexCell(-1,0),new HexCell(0,-1),new HexCell(1,-1)};
        static readonly HashSet<HexCell> gaps=new HashSet<HexCell>{new HexCell(1,1),new HexCell(2,-1),new HexCell(1,-2),new HexCell(-1,-1),new HexCell(-2,1),new HexCell(-1,2)};
        public static RuneValidation Validate(RuneBoardDefinition board,int grade,IReadOnlyList<RunePlacement> placements,Func<RunePiece,bool> owned,Func<string,bool> elsewhere)
        {
            var result=new RuneValidation();var occupied=new Dictionary<HexCell,RunePlacement>();var seen=new HashSet<string>();
            var links=new Dictionary<string,HashSet<string>>();
            foreach(var p in placements)
            {
                if(!seen.Add(p.InstanceId))result.Add(RuneValidationCode.DuplicateRune,p.InstanceId);
                if(!links.ContainsKey(p.InstanceId))links[p.InstanceId]=new HashSet<string>();
                if(owned!=null&&!owned(p.Piece))result.Add(RuneValidationCode.NotOwned,p.InstanceId);
                if(elsewhere!=null&&elsewhere(p.InstanceId))result.Add(RuneValidationCode.AlreadyUsedOnAnotherBoard,p.InstanceId);
                foreach(var c in p.OccupiedCells)
                {
                    if(!board.TryGetCell(c,out var node))result.Add(RuneValidationCode.OutsideBoard,p.InstanceId,coordinate:c);
                    else if(!board.IsOpen(node,grade))result.Add(RuneValidationCode.LockedRegion,p.InstanceId,coordinate:c);
                    if(c.Equals(new HexCell(0,0)))result.Add(RuneValidationCode.ForeignStart,p.InstanceId,coordinate:c);
                    if(occupied.TryGetValue(c,out var other)){result.Add(RuneValidationCode.Overlap,p.InstanceId,other.InstanceId,c);result.Add(RuneValidationCode.Overlap,other.InstanceId,p.InstanceId,c);}
                    else occupied[c]=p;
                }
            }
            for(int i=0;i<placements.Count;i++)for(int j=i+1;j<placements.Count;j++)
            {
                var a=placements[i];var b=placements[j];if(a.Piece.Type!=b.Piece.Type||a.InstanceId==b.InstanceId)continue;
                bool edge=false,linked=false;
                foreach(var ac in a.OccupiedCells)foreach(var bc in b.OccupiedCells)
                {
                    if(ac.DistanceTo(bc)==1)edge=true;
                    if(!gaps.Contains(bc-ac))continue;
                    var common=neighbors.Select(n=>ac+n).Where(c=>c.DistanceTo(bc)==1).ToArray();
                    if(common.Length!=2||common.Any(c=>!board.TryGetCell(c,out var cell)||!board.IsOpen(cell,grade)))continue;
                    // One third piece occupying both intermediate cells blocks the gap. Two separate pieces do not.
                    if(occupied.TryGetValue(common[0],out var x)&&occupied.TryGetValue(common[1],out var y)&&x.InstanceId==y.InstanceId&&x.InstanceId!=a.InstanceId&&x.InstanceId!=b.InstanceId)continue;
                    linked=true;
                }
                if(edge){result.Add(RuneValidationCode.SameColorEdgeContact,a.InstanceId,b.InstanceId);result.Add(RuneValidationCode.SameColorEdgeContact,b.InstanceId,a.InstanceId);}
                else if(linked){links[a.InstanceId].Add(b.InstanceId);links[b.InstanceId].Add(a.InstanceId);}
            }
            for(int type=0;type<5;type++)
            {
                var reached=new HashSet<string>(placements.Where(p=>p.Piece.Type==type&&result.IsRuneValid(p.InstanceId)&&p.OccupiedCells.Any(c=>c.DistanceTo(new HexCell(0,0))==1)).Select(p=>p.InstanceId));
                var queue=new Queue<string>(reached);
                while(queue.Count>0)foreach(var next in links[queue.Dequeue()])if(result.IsRuneValid(next)&&reached.Add(next))queue.Enqueue(next);
                foreach(var p in placements)if(p.Piece.Type==type&&!reached.Contains(p.InstanceId))result.Add(RuneValidationCode.DisconnectedFromStart,p.InstanceId);
            }
            return result;
        }
    }
}
