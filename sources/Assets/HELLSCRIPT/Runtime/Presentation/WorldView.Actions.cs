using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class WorldView
    {
        readonly Dictionary<int,GameObject> projectileViews=new Dictionary<int,GameObject>();
        readonly Dictionary<int,GameObject> trapViews=new Dictionary<int,GameObject>();
        void PresentActions(RunState run)
        {
            foreach(var p in run.projectiles)
            {
                if(!projectileViews.TryGetValue(p.id,out var go))
                {
                    Material mat=p.hostile?red:p.explosionRadius>0?ember:p.element==3?blue:trim;
                    go=Shape("Projectile "+p.id,PrimitiveType.Sphere,world.transform,Position(p.position)+Vector3.up,Vector3.one*Mathf.Max(.18f,p.radius*2),mat);projectileViews[p.id]=go;
                }
                go.SetActive(p.delay<=0&&(p.hostile?CanDisplayEnemyMarker(run,p.position,p.radius):Vector2.Distance(p.position,run.position)<25));go.transform.position=Position(p.position)+Vector3.up;
            }
            foreach(int id in projectileViews.Keys.ToArray())if(!run.projectiles.Any(p=>p.id==id)){Destroy(projectileViews[id]);projectileViews.Remove(id);}
            foreach(var trap in run.traps)
            {
                if(!trapViews.TryGetValue(trap.id,out var go)){go=Ring(world.transform,Position(trap.position)+Vector3.up*.11f,trap.radius,green,.09f);trapViews[trap.id]=go;}
                go.SetActive(Vector2.Distance(trap.position,run.position)<25);go.transform.localScale=Vector3.one*(trap.arm>0?.55f:1);
                go.GetComponent<LineRenderer>().widthMultiplier=trap.triggered?.16f:.055f;
            }
            foreach(int id in trapViews.Keys.ToArray())if(!run.traps.Any(t=>t.id==id)){Destroy(trapViews[id]);trapViews.Remove(id);}
            var body=hero.transform.GetChild(0);body.localRotation=Quaternion.Euler(run.heroAction.phase==HeroActionPhase.Preparing?12:0,0,0);
        }
    }
}
