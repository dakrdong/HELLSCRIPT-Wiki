using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class WorldView
    {
        // Outline LineRenderers by key ("<threat key>-<outline index>", "refuge-", "aura-", "link-", "guard-"); smokes read it.
        readonly Dictionary<string,GameObject> enemyThreatViews=new Dictionary<string,GameObject>();
        // WorldFx views under the outlines, by threat key (no outline index): windup fills, refuge circles, auras.
        readonly Dictionary<string,WorldFx.TelegraphView> threatFills=new Dictionary<string,WorldFx.TelegraphView>();
        // Windup gauges: how full each warning is, and which warnings completed this frame (they flash).
        readonly TelegraphGauge gauge=new TelegraphGauge();
        readonly List<TelegraphGauge.Footprint> completedWarnings=new List<TelegraphGauge.Footprint>();
        readonly HashSet<string> activeThreats=new HashSet<string>(),activeFills=new HashSet<string>();
        readonly List<string> staleThreats=new List<string>();
        void EnemyOutline(string key,Vector2[] points,Material material,float width,HashSet<string> active,bool tiled=false)
        {
            active.Add(key);
            if(!enemyThreatViews.TryGetValue(key,out var go))
            {
                go=new GameObject(key);go.transform.SetParent(world.transform,false);var line=go.AddComponent<LineRenderer>();line.useWorldSpace=true;
                line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.receiveShadows=false;enemyThreatViews[key]=go;
            }
            var renderer=go.GetComponent<LineRenderer>();renderer.sharedMaterial=RiftMaterial(material);renderer.widthMultiplier=width;renderer.positionCount=points.Length;
            renderer.textureMode=tiled?LineTextureMode.Tile:LineTextureMode.Stretch;
            for(int i=0;i<points.Length;i++)renderer.SetPosition(i,Position(points[i])+Vector3.up*.19f);
        }
        // A lingering zone that already landed (poison pool, fire patch, beam, whirl) changes from the danger gauge to its
        // element's colour at lower strength; warnings that have not landed keep the red gauge.
        Color LandedZoneTint(RunState run,in EnemyThreat threat)
        {
            if(threat.delay>0||!threat.key.StartsWith("hazard-",StringComparison.Ordinal)||!int.TryParse(threat.key.Substring(7),out int id))return default;
            foreach(var h in run.enemyHazards)
            {
                if(h.id!=id)continue;if(h.duration<=0)return default;
                var c=ElementColor(h.element!=0?h.element:AttackElement(h.definitionId));c.a=.5f;return c;
            }
            return default;
        }
        WorldFx.TelegraphView ThreatView(WorldFx library,string key,WorldFx.TelegraphKind kind)
        {
            activeFills.Add(key);
            if(threatFills.TryGetValue(key,out var view)&&view.go!=null&&view.Kind==kind)return view;
            if(view!=null)library.ReturnTelegraph(view);
            view=library.RentTelegraph(kind);threatFills[key]=view;return view;
        }
        // Runs once per Present after `elapsed` advanced, so it also steps the FX library (WorldView.StepFx).
        void PresentEnemyCombat(RunState run)
        {
            StepFx(Position(run.position));
            var library=Fx;activeThreats.Clear();activeFills.Clear();gauge.BeginFrame(completedWarnings);
            foreach(var threat in EnemyCombat.Threats(run,Combat.Map))
            {
                if(!CanDisplayEnemyMarker(run,threat.origin,threat.radius))continue;
                bool boss=threat.key.StartsWith("boss-",StringComparison.Ordinal);float progress=gauge.Observe(run,threat,boss);
                int index=0;float width=threat.delay>0?.06f+progress*.08f:.18f;if(boss)width*=1.35f;
                var edge=library==null?red:boss?library.BossTelegraphEdge:library.TelegraphEdge;
                foreach(var line in EnemyCombat.Outlines(threat))EnemyOutline(threat.key+"-"+index++,line,edge,width,activeThreats);
                // The footprint fills like a gauge and is full on the step the attack lands (CHK-P04: shape and fill, not colour alone).
                if(library!=null)library.ShowFill(ThreatView(library,threat.key,WorldFx.TelegraphKind.Fill),threat.shape,threat.origin,threat.end,threat.direction,
                    threat.radius,threat.innerRadius,threat.angle,progress,boss,0,LandedZoneTint(run,threat));
            }
            foreach(var e in run.enemies.Where(e=>!e.dead&&Vector2.Distance(run.position,e.position)<=12&&Combat.Map.LineClear(run.position,e.position)))
            {
                if(e.boss)foreach(var refuge in e.brain.boss.refuges)
                {
                    string key="refuge-"+e.id+"-"+e.brain.boss.refuges.IndexOf(refuge);
                    foreach(var line in EnemyCombat.Outlines(new EnemyThreat("","REFUGE",AttackShape.Circle,refuge.position,refuge.position,Vector2.up,refuge.radius)))
                        EnemyOutline(key,line,library==null?blue:library.RefugeEdge,.14f,activeThreats);
                    if(library!=null)library.ShowRefuge(ThreatView(library,key,WorldFx.TelegraphKind.Refuge),refuge.position,refuge.radius);
                }
                if(!e.boss&&e.kind==10)
                {
                    foreach(var line in EnemyCombat.Outlines(new EnemyThreat("","N11",AttackShape.Circle,e.position,e.position,e.brain.facing,4)))
                        EnemyOutline("aura-"+e.id,line,library==null?purple:library.AuraEdge,.04f,activeThreats);
                    if(library!=null)library.ShowAura(ThreatView(library,"aura-"+e.id,WorldFx.TelegraphKind.Aura),e.position,4);
                }
                if(EnemyCombat.Trait(e,2))
                {
                    var other=run.enemies.Find(x=>x.id==e.elitePartner&&!x.dead&&x.elite>=0&&Vector2.Distance(x.position,e.position)<=5&&Vector2.Distance(run.position,x.position)<=12&&Combat.Map.LineClear(run.position,x.position));
                    if(other!=null)EnemyOutline("link-"+e.id,new[]{e.position,other.position},library==null?purple:library.Tether,library==null?.1f:.22f,activeThreats,library!=null);
                }
                if(EnemyCombat.Trait(e,3)||e.brain.rearWindow>0)
                {
                    bool rear=e.brain.rearWindow>0;var direction=rear?-e.brain.facing:e.brain.facing;var points=new Vector2[17];
                    for(int i=0;i<17;i++)points[i]=e.position+EnemyCombat.Rotate(direction,-60+i*120f/16)*1.15f;
                    EnemyOutline("guard-"+e.id,points,library==null?rear?ember:blue:rear?library.RearEdge:library.GuardEdge,.12f,activeThreats);
                }
            }
            staleThreats.Clear();foreach(var key in enemyThreatViews.Keys)if(!activeThreats.Contains(key))staleThreats.Add(key);
            foreach(var key in staleThreats){Destroy(enemyThreatViews[key]);enemyThreatViews.Remove(key);}
            staleThreats.Clear();foreach(var key in threatFills.Keys)if(!activeFills.Contains(key))staleThreats.Add(key);
            foreach(var key in staleThreats){if(library!=null)library.ReturnTelegraph(threatFills[key]);threatFills.Remove(key);}
            gauge.EndFrame(run);
            // A full gauge that landed flashes its whole footprint once; shots show their flight instead.
            if(library!=null)foreach(var warning in completedWarnings)if(!TelegraphGauge.Shot(warning.definition)){library.ReleaseFlash(warning);LandAttack(library,run,warning);}
        }
    }
}
