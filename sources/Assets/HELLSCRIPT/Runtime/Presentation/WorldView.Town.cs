using System.Collections.Generic;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class WorldView
    {
        readonly Dictionary<TownStation,GameObject> stationMarks=new Dictionary<TownStation,GameObject>();

        // The plaza reuses the rift's placeholder body, materials and camera so replacing the hero
        // model later replaces it in both places at once.
        public void BuildTown(TownWalk walk)
        {
            ClearDungeon();stationMarks.Clear();world=new GameObject("Sanctuary Plaza");
            // The floor extends well past the walls: the isometric camera near the spawn otherwise shows the void.
            Shape("Plaza floor",PrimitiveType.Cube,world.transform,new Vector3(0,-.45f,0),new Vector3(TownLayout.HalfWidth*2+30,.8f,TownLayout.HalfHeight*2+30),stone);
            Shape("Plaza court",PrimitiveType.Cube,world.transform,new Vector3(0,-.4f,0),new Vector3(TownLayout.HalfWidth*2+2,.8f,TownLayout.HalfHeight*2+2),darkStone);
            for(int edge=0;edge<4;edge++)
            {
                bool horizontal=edge<2;float sign=edge%2==0?-1:1;
                Vector3 pos=horizontal?new Vector3(0,.7f,sign*(TownLayout.HalfHeight+1.6f)):new Vector3(sign*(TownLayout.HalfWidth+1.6f),.7f,0);
                Vector3 size=horizontal?new Vector3(TownLayout.HalfWidth*2+4,1.4f,.8f):new Vector3(.8f,1.4f,TownLayout.HalfHeight*2+4);
                Shape("Plaza wall",PrimitiveType.Cube,world.transform,pos,size,darkStone);
            }
            foreach(var station in TownLayout.Stations)
            {
                var root=new GameObject(station.name);root.transform.SetParent(world.transform,false);root.transform.position=Position(station.position);
                Shape("Post",PrimitiveType.Cylinder,root.transform,Vector3.up*1.6f,new Vector3(.9f,1.6f,.9f),darkStone);
                Shape("Brazier",PrimitiveType.Cylinder,root.transform,Vector3.up*3.3f,new Vector3(1.3f,.14f,1.3f),trim);
                Shape("Ember",PrimitiveType.Sphere,root.transform,Vector3.up*3.6f,new Vector3(.5f,.65f,.5f),station.id==TownStation.Warehouse?blue:ember);
                // The attendant is the same one-piece placeholder as an enemy body, tinted as a hero, so it is
                // recognisably a person until a real NPC model exists.
                var attendant=CreateBody(Loc.F("{0} 담당", station.name),(int)station.id%3,false,false);attendant.transform.SetParent(root.transform,false);
                attendant.transform.localPosition=new Vector3(0,0,1.6f);attendant.transform.localRotation=Quaternion.Euler(0,180,0);
                var mark=Ring(root.transform,Vector3.up*.1f,TownLayout.ArriveRadius,blue,.12f);mark.SetActive(false);stationMarks[station.id]=mark;
            }
            hero=CreateBody("Hero",(int)game.Store.Data.Hero.heroClass,false,false);
            hero.transform.position=Position(walk.Position);hero.transform.rotation=Quaternion.LookRotation(Position(walk.Facing));
            viewCamera.transform.position=CameraPosition(walk.Position);
        }

        // Stations carry no colliders, so a tap is resolved by projecting each station to the screen and
        // taking the nearest within a thumb-sized radius. The radius scales with the window height.
        public bool TryPickStation(Vector2 screen,out TownStation station)
        {
            station=default;if(world==null||viewCamera==null)return false;
            var projected=new List<(TownStation,Vector2)>();
            foreach(var s in TownLayout.Stations)
            {
                var point=viewCamera.WorldToScreenPoint(Position(s.position)+Vector3.up*1.6f);
                if(point.z>0)projected.Add((s.id,new Vector2(point.x,point.y)));
            }
            return TownPick.Nearest(projected,screen,Mathf.Max(44f,Screen.height*.07f),out station);
        }

        public void PresentTown(TownWalk walk,float dt)
        {
            if(world==null||hero==null)return;elapsed+=dt;
            Vector3 target=Position(walk.Position);Vector3 movement=target-hero.transform.position;movement.y=0;
            hero.transform.position=Vector3.Lerp(hero.transform.position,target,Mathf.Min(1,dt*22));
            Vector3 facing=Position(walk.Facing);if(facing.sqrMagnitude>.01f)hero.transform.rotation=Quaternion.Slerp(hero.transform.rotation,Quaternion.LookRotation(facing),dt*14);
            var body=hero.transform.GetChild(0);body.localPosition=new Vector3(0,1+(walk.Walking?Mathf.Sin(elapsed*13)*.08f:0),0);
            viewCamera.transform.position=Vector3.Lerp(viewCamera.transform.position,CameraPosition(walk.Position),dt*5);
            foreach(var pair in stationMarks)pair.Value.SetActive(walk.Destination==pair.Key);
            ApplyBattleViewport();
        }
    }
}
