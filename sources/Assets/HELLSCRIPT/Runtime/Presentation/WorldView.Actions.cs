using System.Collections.Generic;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class WorldView
    {
        readonly Dictionary<int,GameObject> projectileViews=new Dictionary<int,GameObject>();
        readonly Dictionary<int,GameObject> trapViews=new Dictionary<int,GameObject>();
        readonly HashSet<int> activeProjectiles=new HashSet<int>(),activeTraps=new HashSet<int>();
        readonly List<int> staleActions=new List<int>();
        void PresentActions(RunState run)
        {
            activeProjectiles.Clear();activeTraps.Clear();
            foreach(var p in run.projectiles)
            {
                activeProjectiles.Add(p.id);
                if(!projectileViews.TryGetValue(p.id,out var go))
                {
                    Material mat=p.hostile?red:p.shadow?purple:p.explosionRadius>0?ember:p.element==3?blue:trim;
                    bool arrow=p.explosionRadius<=0&&p.element==0;
                    go=new GameObject("Projectile "+p.id);go.transform.SetParent(world.transform,false);projectileViews[p.id]=go;
                    Shape(arrow?"Arrow":"Magic orb",arrow?PrimitiveType.Cube:PrimitiveType.Sphere,go.transform,Vector3.zero,arrow?new Vector3(.12f,.12f,p.skill==6?1.5f:.95f):Vector3.one*Mathf.Max(.32f,p.radius*2),mat);
                    SkillLine("Projectile trail",go.transform,new[]{new Vector3(0,0,-.2f),new Vector3(0,0,arrow?-1.3f:-.85f)},mat,arrow?.06f:.14f);
                }
                go.SetActive(p.delay<=0&&(p.hostile?CanDisplayEnemyMarker(run,p.position,p.radius):Vector2.Distance(p.position,run.position)<25));go.transform.position=Position(p.position)+Vector3.up;
                if(p.direction.sqrMagnitude>.001f)go.transform.rotation=Quaternion.LookRotation(Position(p.direction));
            }
            staleActions.Clear();foreach(int id in projectileViews.Keys)if(!activeProjectiles.Contains(id))staleActions.Add(id);
            foreach(int id in staleActions){Destroy(projectileViews[id]);projectileViews.Remove(id);}
            foreach(var trap in run.traps)
            {
                activeTraps.Add(trap.id);
                if(!trapViews.TryGetValue(trap.id,out var go)){go=Ring(world.transform,Position(trap.position)+Vector3.up*.11f,trap.radius,green,.09f);trapViews[trap.id]=go;Shape("Trap core",PrimitiveType.Cube,go.transform,Vector3.up*.15f,new Vector3(.55f,.22f,.55f),green);}
                go.SetActive(Vector2.Distance(trap.position,run.position)<25);go.transform.localScale=Vector3.one*(trap.arm>0?.55f:1);
                go.GetComponent<LineRenderer>().widthMultiplier=trap.triggered?.16f:.055f;
            }
            staleActions.Clear();foreach(int id in trapViews.Keys)if(!activeTraps.Contains(id))staleActions.Add(id);
            foreach(int id in staleActions){Destroy(trapViews[id]);trapViews.Remove(id);}
            AnimateHeroSkill(run);TickHeroRig(run);PresentClassSkillFx(run);
        }
    }
}
