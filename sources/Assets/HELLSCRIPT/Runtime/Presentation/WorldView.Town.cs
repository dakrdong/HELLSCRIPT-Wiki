using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hellscript
{
    public sealed partial class WorldView
    {
        readonly Dictionary<TownStation,GameObject> stationMarks=new Dictionary<TownStation,GameObject>();
        readonly List<Mesh> townMeshes=new List<Mesh>();
        readonly List<TownBuildingFade> townBuildings=new List<TownBuildingFade>();
        Transform townPortal;
        readonly List<Transform> portalSparks=new List<Transform>();
        public int FadedTownBuildings=>townBuildings.Count(b=>b.Alpha<.95f);
        public static Vector3 TownPoint(Vector2 point,float height=0){var p=TownLayout.ToWorld(point);return new Vector3(p.x,height,p.y);}
        static readonly Quaternion TownRotation=Quaternion.LookRotation(TownPoint(Vector2.up));
        public void BuildTown(TownWalk walk)
        {
            ClearDungeon();world=new GameObject("Ashwood Settlement");
            RenderSettings.ambientLight=new Color(.4f,.43f,.39f);RenderSettings.fogColor=new Color(.09f,.13f,.12f);
            var layout=new GameObject("Village ground").transform;layout.SetParent(world.transform,false);layout.rotation=TownRotation;
            BuildForestGround(layout);
            foreach(var s in TownLayout.Stations)
            {
                if(s.id==TownStation.AspectStone){BuildAspectStone(layout,s);continue;}
                if(s.building.width>0)BuildTownHouse(layout,s);
                if(s.id==TownStation.Training)BuildTrainingYard(layout,s);
                if(s.id==TownStation.RiftKeeper)BuildTownPortal(layout,s.position);
                var attendant=CreateTownAttendant(s);attendant.transform.SetParent(world.transform,false);attendant.transform.position=TownPoint(s.NpcPosition);attendant.transform.rotation=TownRotation;
            }
            for(int i=0;i<TownLayout.Residents.Length;i++)
            {
                var resident=TownLayout.Residents[i];
                var cloth=i==0?TownMat("Resident blue",.2f,.29f,.4f):i==1?TownMat("Resident burgundy",.4f,.19f,.23f):TownMat("Resident green",.24f,.36f,.29f);
                var figure=CreateTownFigure("Resident "+resident.id,cloth);
                figure.transform.SetParent(world.transform,false);figure.transform.position=TownPoint(resident.position);figure.transform.rotation=TownRotation*Quaternion.Euler(0,i==0?-25:i==1?20:-15,0);
            }
            foreach(var s in TownLayout.Stations)
            {
                var mark=Ring(world.transform,TownPoint(s.position,.09f),TownLayout.InteractionRadius,trim,.065f);mark.name="Service reach "+s.id;mark.SetActive(false);stationMarks[s.id]=mark;
            }
            hero=CreateBody("Hero",(int)game.Store.Data.Hero.heroClass,false,false);hero.transform.position=TownPoint(walk.Position);
            hero.transform.rotation=Quaternion.LookRotation(TownPoint(walk.Facing));
            viewCamera.transform.position=CameraPosition(TownLayout.ToWorld(walk.Position));
        }
        void ClearTownPresentation()
        {
            foreach(var b in townBuildings)b.Dispose();townBuildings.Clear();stationMarks.Clear();portalSparks.Clear();townPortal=null;
            foreach(var m in townMeshes)if(m!=null)Destroy(m);townMeshes.Clear();
            RenderSettings.ambientLight=new Color(.29f,.34f,.43f);RenderSettings.fogColor=new Color(.035f,.05f,.075f);
        }
        public bool TryPickTownGround(Vector2 screen,out Vector2 point)
        {
            point=default;var ray=viewCamera.ScreenPointToRay(screen);if(!new Plane(Vector3.up,Vector3.zero).Raycast(ray,out float distance))return false;
            var p=ray.GetPoint(distance);point=TownLayout.FromWorld(new Vector2(p.x,p.z));return TownLayout.Walkable(point);
        }
        public bool TryPickStation(Vector2 screen,out TownStation station)
        {
            station=default;if(world==null||viewCamera==null)return false;var projected=new List<(TownStation,Vector2)>();
            foreach(var s in TownLayout.Stations)
            {
                void AddTarget(Vector2 point)
                {
                    var p=viewCamera.WorldToViewportPoint(TownPoint(point,s.id==TownStation.AspectStone?3.5f:1.6f));if(p.z<=0||p.x<0||p.x>1||p.y<0||p.y>1)return;
                    var pixel=viewCamera.WorldToScreenPoint(TownPoint(point,s.id==TownStation.AspectStone?3.5f:1.6f));projected.Add((s.id,new Vector2(pixel.x,pixel.y)));
                }
                AddTarget(s.NpcPosition);if(s.id==TownStation.RiftKeeper)AddTarget(s.position);
            }
            return TownPick.Nearest(projected,screen,Mathf.Max(32,Screen.height*.045f),out station);
        }
        public Vector3 TownNpcScreen(Vector2 position)=>viewCamera.WorldToScreenPoint(TownPoint(position,2.9f));
        public Vector3 TownStationScreen(TownStation station)=>station==TownStation.AspectStone?viewCamera.WorldToScreenPoint(TownPoint(TownLayout.Station(station).building.center,7.5f)):TownNpcScreen(TownLayout.Station(station).NpcPosition);
        public void PresentTown(TownWalk walk,float dt)
        {
            if(world==null||hero==null)return;elapsed+=dt;hero.transform.position=TownPoint(walk.Position);
            Vector3 facing=TownPoint(walk.Facing);if(facing.sqrMagnitude>.01f)hero.transform.rotation=Quaternion.Slerp(hero.transform.rotation,Quaternion.LookRotation(facing),dt*14);
            var body=hero.transform.GetChild(0);body.localPosition=new Vector3(0,1+(walk.Walking?Mathf.Sin(elapsed*13)*.08f:0),0);
            viewCamera.transform.position=Vector3.Lerp(viewCamera.transform.position,CameraPosition(TownLayout.ToWorld(walk.Position)),1-Mathf.Exp(-dt*8));
            foreach(var pair in stationMarks)pair.Value.SetActive(walk.Nearby==pair.Key||walk.Destination==pair.Key);
            ApplyBattleViewport();
            foreach(var b in townBuildings)b.Update(viewCamera,hero.transform.position,dt);
            if(townPortal!=null)
            {
                townPortal.localRotation=Quaternion.Euler(0,0,elapsed*16);
                for(int i=0;i<portalSparks.Count;i++)
                {
                    float a=elapsed*(.6f+i%3*.12f)+i*2.39996f;float r=2.6f+(i%4)*.13f;
                    portalSparks[i].localPosition=new Vector3(Mathf.Cos(a)*r,4.2f+Mathf.Sin(a)*r*1.6f,-.2f-Mathf.Sin(a*2)*.25f);
                }
            }
        }
        Material TownMat(string name,float r,float g,float b,bool emission=false)=>Mat("Village "+name,new Color(r,g,b),emission);
        Material PortalMat(string name,Color color)
        {
            if(!materials.TryGetValue(name,out var material))
            {
                material=new Material(Resources.Load<Shader>("TownGlow")){name=name};material.SetColor("_BaseColor",color);materials[name]=material;
            }
            return material;
        }
        GameObject Block(Transform p,string n,Vector3 at,Vector3 size,Material mat)=>Shape(n,PrimitiveType.Cube,p,at,size,mat);
        void TownLantern(Transform p,Vector3 at)
        {
            var iron=TownMat("Iron",.15f,.17f,.16f);var glow=TownMat("Lantern light",1,.56f,.17f,true);
            Shape("Lantern post",PrimitiveType.Cylinder,p,at+Vector3.up*1.65f,new Vector3(.17f,1.65f,.17f),iron);
            Block(p,"Lantern",at+Vector3.up*3.2f,new Vector3(.45f,.65f,.45f),glow);
            Block(p,"Lantern cap",at+Vector3.up*3.58f,new Vector3(.67f,.12f,.67f),iron);
        }
        Mesh ConeMesh(float radius,float height,int sides=9)
        {
            var vertices=new List<Vector3>();var triangles=new List<int>();
            for(int n=0;n<sides;n++)
            {
                float a=n*Mathf.PI*2/sides,b=(n+1)*Mathf.PI*2/sides;int i=vertices.Count;
                vertices.Add(new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius));vertices.Add(Vector3.up*height);vertices.Add(new Vector3(Mathf.Cos(b)*radius,0,Mathf.Sin(b)*radius));
                triangles.Add(i);triangles.Add(i+1);triangles.Add(i+2);
            }
            var mesh=new Mesh{name="Village cone"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();townMeshes.Add(mesh);return mesh;
        }
        GameObject MeshPart(Transform p,string name,Mesh mesh,Vector3 pos,Material mat)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(p,false);go.transform.localPosition=pos;
            go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=mat;return go;
        }
        void BatchTownGeometry(Transform parent)
        {
            var filters=parent.GetComponentsInChildren<MeshFilter>();
            foreach(var group in filters.GroupBy(f=>f.GetComponent<Renderer>().sharedMaterial))
            {
                var combines=group.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=parent.worldToLocalMatrix*f.transform.localToWorldMatrix}).ToArray();
                var mesh=new Mesh{name="Village batch",indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(combines);townMeshes.Add(mesh);
                MeshPart(parent,"Static "+group.Key.name,mesh,Vector3.zero,group.Key);
            }
            foreach(var f in filters){f.gameObject.SetActive(false);Destroy(f.gameObject);}
        }
        void BuildForestGround(Transform layout)
        {
            var soil=TownMat("Forest earth",.19f,.21f,.16f);var moss=TownMat("Moss",.16f,.21f,.12f);var mud=TownMat("Worn trail",.33f,.29f,.22f);
            var wood=TownMat("Timber",.23f,.16f,.1f);var bark=TownMat("Bark",.15f,.12f,.085f);var needles=TownMat("Pine",.105f,.19f,.14f);var needlesLight=TownMat("Pine tips",.16f,.25f,.18f);
            BuildTownGroundMesh(layout);
            var random=new System.Random(74219);float R(float a,float b)=>Mathf.Lerp(a,b,(float)random.NextDouble());
            var pine=ConeMesh(1,1,13);
            for(int n=0;n<115;n++)
            {
                bool horizontal=n%2==0;float x=horizontal?R(-73,73):(n%4==1?-1:1)*R(59,76);
                float z=horizontal?(n%4==0?-1:1)*R(40,57):R(-51,51);float h=R(8,14),r=R(2,3.5f);
                Shape("Pine trunk",PrimitiveType.Cylinder,layout,new Vector3(x,h*.32f,z),new Vector3(.55f,h*.32f,.55f),bark);
                for(int layer=0;layer<3;layer++)
                {
                    var branch=MeshPart(layout,"Pine canopy",pine,new Vector3(x,h*(.24f+layer*.18f),z),layer==2?needlesLight:needles);
                    branch.transform.localScale=new Vector3(r*(1-layer*.22f),h*.55f,r*(1-layer*.22f));branch.transform.localRotation=Quaternion.Euler(R(-4,4),R(0,360),R(-4,4));
                }
            }
            for(int n=0;n<110;n++)
            {
                float x=R(-55,55),z=R(-35,35);if(Mathf.Abs(z)<5||Mathf.Abs(x)<4)continue;
                if(TownLayout.Stations.Any(s=>Vector2.Distance(s.position,new Vector2(x,z))<5||s.building.Contains(new Vector2(x,z)))||TownLayout.Residents.Any(r=>Vector2.Distance(r.position,new Vector2(x,z))<2))continue;
                Shape("Mossy stone",PrimitiveType.Sphere,layout,new Vector3(x,-.05f,z),new Vector3(R(.4f,1.3f),R(.12f,.6f),R(.4f,1.2f)),n%3==0?stone:moss);
            }
            for(int n=0;n<180;n++)
            {
                float x=R(-54,54),z=R(-34,34);var point=new Vector2(x,z);
                if(Mathf.Abs(z)<5||Mathf.Abs(x)<5||TownLayout.Stations.Any(s=>Vector2.Distance(s.position,point)<5||s.building.Contains(point))||TownLayout.Residents.Any(r=>Vector2.Distance(r.position,point)<2))continue;
                var bush=MeshPart(layout,"Low forest scrub",pine,new Vector3(x,0,z),n%2==0?moss:needlesLight);
                bush.transform.localScale=new Vector3(R(.6f,1.5f),R(.4f,1.2f),R(.6f,1.5f));bush.transform.localRotation=Quaternion.Euler(0,R(0,360),0);
            }
            // A small communal fire and split logs make the center a lived-in gathering place.
            for(int n=0;n<9;n++)
            {
                float a=n*Mathf.PI*2/9;Shape("Campfire stone",PrimitiveType.Sphere,layout,new Vector3(-9+Mathf.Cos(a)*1.4f,.2f,-7+Mathf.Sin(a)*1.4f),new Vector3(.6f,.4f,.6f),stone);
            }
            for(int n=0;n<4;n++)
            {
                var log=Shape("Split firewood",PrimitiveType.Cylinder,layout,new Vector3(-9,.3f,-7),new Vector3(.3f,1.1f,.3f),wood);log.transform.localRotation=Quaternion.Euler(90,n*45,0);
                var flame=MeshPart(layout,"Hearth flame",pine,new Vector3(-9+R(-.4f,.4f),.35f,-7+R(-.4f,.4f)),ember);flame.transform.localScale=new Vector3(.3f,R(.5f,1.1f),.3f);
            }
            for(int n=0;n<44;n++)
            {
                float x=-54+n*2.5f;
                for(int side=-1;side<=1;side+=2)
                {
                    Block(layout,"Settlement palisade",new Vector3(x,1.25f,side*37),new Vector3(.6f,2.5f,.55f),wood);
                    Block(layout,"Fence rail",new Vector3(x,1.2f,side*37),new Vector3(2.5f,.22f,.2f),bark);
                }
            }
            for(int n=0;n<8;n++)TownLantern(layout,new Vector3(-49+n*14,0,n%2==0?5:-5));
            // A low stone well and supply cart leave the 110 m main road unobstructed.
            Shape("Well rim",PrimitiveType.Cylinder,layout,new Vector3(-7,.6f,7),new Vector3(3.3f,.6f,3.3f),stone);
            Shape("Well water",PrimitiveType.Cylinder,layout,new Vector3(-7,1.21f,7),new Vector3(2.5f,.01f,2.5f),TownMat("Well water",.08f,.18f,.2f));
            for(int side=-1;side<=1;side+=2)Block(layout,"Well upright",new Vector3(-7+side*1.5f,2.2f,7),new Vector3(.25f,3.2f,.25f),wood);
            Block(layout,"Well crossbeam",new Vector3(-7,3.65f,7),new Vector3(3.6f,.3f,.3f),wood);
            BatchTownGeometry(layout);
        }
        void BuildTownHouse(Transform layout,TownStationDefinition s)
        {
            var house=new GameObject("Building "+s.id).transform;house.SetParent(layout,false);house.localPosition=new Vector3(s.building.center.x,0,s.building.center.y);
            float w=s.building.width,d=s.building.height;var timber=TownMat("Timber",.23f,.16f,.1f);var plaster=TownMat("Old plaster",.43f,.4f,.31f);
            var roof=TownMat("Slate",.2f,.25f,.25f);var frame=TownMat("Dark beams",.12f,.105f,.075f);var lit=TownMat("Window amber",.9f,.53f,.2f,true);
            Block(house,"Stone footing",new Vector3(0,.38f,0),new Vector3(w,.76f,d),stone);
            Block(house,"Plaster walls",new Vector3(0,2.65f,0),new Vector3(w-.4f,4.6f,d-.4f),plaster);
            for(int row=0;row<5;row++)
            {
                Block(house,"Front weatherboard",new Vector3(0,1+row*.82f,-d/2-.02f),new Vector3(w,.12f,.2f),timber);
                Block(house,"Back weatherboard",new Vector3(0,1+row*.82f,d/2+.02f),new Vector3(w,.12f,.2f),timber);
            }
            foreach(float x in new[]{-w/2+.14f,0,w/2-.14f})foreach(float z in new[]{-d/2,d/2})Block(house,"Wall beam",new Vector3(x,2.75f,z),new Vector3(.3f,5.1f,.35f),frame);
            // Real pitched roof with individual overlapping slates and visible fascia.
            float rise=2.6f,half=d/2+.7f,angle=Mathf.Atan2(rise,half)*Mathf.Rad2Deg,slope=Mathf.Sqrt(half*half+rise*rise);
            for(int side=-1;side<=1;side+=2)
            {
                var roofSide=Block(house,"Pitched roof",new Vector3(0,5.7f+rise/2,side*half/2),new Vector3(w+1.5f,.27f,slope),roof);roofSide.transform.localRotation=Quaternion.Euler(side*angle,0,0);
                for(int row=0;row<5;row++)
                {
                    float t=(row+.3f)/5;
                    var slate=Block(house,"Slate seam",new Vector3(0,5.8f+rise*(1-t),side*half*t),new Vector3(w+1.65f,.09f,.08f),stone);slate.transform.localRotation=Quaternion.Euler(side*angle,0,0);
                }
            }
            Block(house,"Ridge beam",new Vector3(0,8.38f,0),new Vector3(w+1.9f,.24f,.3f),frame);
            foreach(float x in new[]{-w/2+.1f,w/2-.1f})
            {
                var gable=MeshPart(house,"Timber gable",ConeMesh(1,1,4),new Vector3(x,4.9f,0),timber);gable.transform.localScale=new Vector3(.2f,3.1f,d*.7f);
            }
            Block(house,"Door recess",new Vector3(0,1.75f,-d/2-.18f),new Vector3(1.8f,3,.16f),frame);
            Block(house,"Plank door",new Vector3(.08f,1.72f,-d/2-.28f),new Vector3(1.45f,2.7f,.15f),timber);
            Shape("Door latch",PrimitiveType.Sphere,house,new Vector3(.58f,1.65f,-d/2-.4f),Vector3.one*.16f,trim);
            foreach(float x in new[]{-w*.3f,w*.3f})
            {
                Block(house,"Window frame",new Vector3(x,3,-d/2-.19f),new Vector3(1.8f,1.8f,.16f),frame);
                Block(house,"Window glow",new Vector3(x,3,-d/2-.3f),new Vector3(1.45f,1.4f,.1f),lit);
                Block(house,"Window mullion",new Vector3(x,3,-d/2-.37f),new Vector3(.12f,1.5f,.08f),frame);
                Block(house,"Window crossbar",new Vector3(x,3,-d/2-.37f),new Vector3(1.5f,.12f,.08f),frame);
            }
            Block(house,"Porch",new Vector3(0,.1f,-d/2-1),new Vector3(w-1,.2f,2),timber);
            TownLantern(house,new Vector3(w/2-1,0,-d/2-1.2f));
            BuildServiceProps(house,s,w,d);
            BatchTownGeometry(house);
            townBuildings.Add(new TownBuildingFade(house.GetComponentsInChildren<Renderer>(),Resources.Load<Shader>("TownFade")));
        }
        void BuildServiceProps(Transform p,TownStationDefinition s,float w,float d)
        {
            var wood=TownMat("Timber",.23f,.16f,.1f);var iron=TownMat("Iron",.15f,.17f,.16f);float z=-d/2-1;
            if(s.id==TownStation.Warehouse)
            {
                for(int n=0;n<5;n++)
                {
                    var pos=new Vector3(-w*.34f+(n%2)*1.5f,.6f+(n/2)*1.15f,z);Block(p,"Supply crate",pos,new Vector3(1.3f,1.1f,1.1f),wood);
                    Block(p,"Crate band",pos+Vector3.back*.57f,new Vector3(1.34f,.14f,.08f),trim);
                }
                for(int n=0;n<3;n++)Shape("Storage barrel",PrimitiveType.Cylinder,p,new Vector3(w*.27f+n*.55f,.65f,z+.3f),new Vector3(.85f,.65f,.85f),wood);
            }
            else if(s.id==TownStation.Blacksmith)
            {
                Block(p,"Forge chimney",new Vector3(w/2-2,5,1),new Vector3(2,10,2),darkStone);
                Block(p,"Forge hearth",new Vector3(w*.28f,1.25f,z),new Vector3(2.5f,2.5f,1.8f),stone);
                Block(p,"Coal opening",new Vector3(w*.28f,1.4f,z-1),new Vector3(1.65f,.95f,.1f),ember);
                Block(p,"Anvil base",new Vector3(-w*.28f,.6f,z-.6f),new Vector3(1,.9f,1),wood);
                Block(p,"Anvil face",new Vector3(-w*.28f,1.3f,z-.6f),new Vector3(2.1f,.45f,.8f),iron);
                var horn=MeshPart(p,"Anvil horn",ConeMesh(.4f,1.2f,6),new Vector3(-w*.28f-1,1.35f,z-.6f),iron);horn.transform.localRotation=Quaternion.Euler(0,0,90);
            }
            else
            {
                Material cloth=s.id==TownStation.Merchant?TownMat("Merchant canvas",.42f,.23f,.15f):s.id==TownStation.Gambler?TownMat("Gambler canvas",.27f,.14f,.32f):s.id==TownStation.GemMerchant?TownMat("Jewel canvas",.2f,.35f,.33f):TownMat("Rune canvas",.3f,.23f,.4f);
                var canopy=Block(p,"Shop awning",new Vector3(-w*.23f,3.5f,z-.5f),new Vector3(w*.43f,.13f,2.5f),cloth);canopy.transform.localRotation=Quaternion.Euler(-9,0,0);
                foreach(float x in new[]{-w*.45f,-w*.02f})Block(p,"Awning pole",new Vector3(x,1.7f,z-1.6f),new Vector3(.12f,3.4f,.12f),wood);
                Block(p,"Display counter",new Vector3(-w*.23f,.9f,z-.5f),new Vector3(w*.4f,1.5f,1.4f),wood);
                for(int n=0;n<5;n++)
                {
                    var pos=new Vector3(-w*.4f+n*.8f,1.9f,z-.6f);
                    if(s.id==TownStation.GemMerchant){var gem=MeshPart(p,"Gem display",ConeMesh(.25f,.55f,5),pos,n%2==0?blue:purple);gem.transform.localRotation=Quaternion.Euler(12,n*47,15);}
                    else if((s.id==TownStation.RuneMerchant||s.id==TownStation.RuneMaster)){var tile=Shape("Rune block",PrimitiveType.Cylinder,p,pos,new Vector3(.5f,.13f,.5f),purple);Block(p,"Rune engraving",pos+Vector3.up*.15f,new Vector3(.22f,.02f,.055f),trim);}
                    else if(s.id==TownStation.Gambler){Block(p,"Sealed parcel",pos,new Vector3(.55f,.55f,.7f),cloth);Block(p,"Parcel seal",pos+Vector3.up*.29f,new Vector3(.2f,.02f,.3f),trim);}
                    else {Block(p,"Weapon grip",pos,new Vector3(.13f,.2f,.6f),iron);Block(p,"Sword blade",pos+Vector3.forward*.6f,new Vector3(.23f,.1f,.95f),stone);}
                }
            }
        }
        GameObject CreateTownAttendant(TownStationDefinition s)
        {
            var cloth=s.id==TownStation.Blacksmith?TownMat("Smith apron",.32f,.19f,.1f):s.id==TownStation.Gambler?TownMat("Gambler coat",.29f,.15f,.33f):s.id==TownStation.GemMerchant?TownMat("Jewel coat",.17f,.37f,.34f):(s.id==TownStation.RuneMerchant||s.id==TownStation.RuneMaster)?TownMat("Rune robe",.35f,.22f,.42f):TownMat("Settler coat",.34f,.36f,.24f);
            return CreateTownFigure("NPC "+s.id,cloth,s.id);
        }
        GameObject CreateTownFigure(string name,Material cloth,TownStation? station=null)
        {
            var npc=new GameObject(name);var p=npc.transform;
            var skin=TownMat("Skin",.59f,.43f,.32f);var boots=TownMat("Boot leather",.1f,.09f,.07f);
            foreach(float x in new[]{-.23f,.23f})
            {
                Shape("Boot",PrimitiveType.Capsule,p,new Vector3(x,.48f,0),new Vector3(.3f,.5f,.35f),boots);
                Shape("Sleeve",PrimitiveType.Capsule,p,new Vector3(x*2.2f,1.6f,0),new Vector3(.32f,.42f,.34f),cloth);
                Shape("Hand",PrimitiveType.Sphere,p,new Vector3(x*2.3f,1.15f,-.12f),Vector3.one*.25f,skin);
            }
            Shape("Coat",PrimitiveType.Capsule,p,new Vector3(0,1.4f,0),new Vector3(.9f,.62f,.65f),cloth);
            Block(p,"Belt",new Vector3(0,1.2f,-.04f),new Vector3(.86f,.14f,.65f),boots);
            Shape("Head",PrimitiveType.Sphere,p,new Vector3(0,2.23f,0),new Vector3(.54f,.65f,.54f),skin);
            Shape("Hair",PrimitiveType.Sphere,p,new Vector3(0,2.47f,.08f),new Vector3(.57f,.25f,.51f),boots);
            foreach(float x in new[]{-.12f,.12f})Shape("Eye",PrimitiveType.Sphere,p,new Vector3(x,2.28f,-.25f),Vector3.one*.055f,boots);
            if(station==TownStation.Blacksmith){Block(p,"Hammer shaft",new Vector3(.57f,1.12f,-.35f),new Vector3(.1f,.65f,.1f),boots);Block(p,"Hammer head",new Vector3(.57f,1.46f,-.35f),new Vector3(.5f,.2f,.22f),stone);}
            else if(station==TownStation.RuneMerchant||station==TownStation.RuneMaster||station==TownStation.RiftKeeper){Block(p,"Rune staff",new Vector3(.65f,1.45f,-.1f),new Vector3(.1f,2.9f,.1f),trim);Shape("Staff crystal",PrimitiveType.Sphere,p,new Vector3(.65f,3,-.1f),Vector3.one*.3f,purple);}
            else if(station==TownStation.Training){Block(p,"Training blade",new Vector3(.68f,1.5f,-.1f),new Vector3(.17f,2,.16f),stone);Block(p,"Crossguard",new Vector3(.68f,.85f,-.1f),new Vector3(.7f,.12f,.2f),trim);}
            else if(station.HasValue){Block(p,"Ledger",new Vector3(.55f,1.18f,-.28f),new Vector3(.38f,.45f,.12f),trim);}
            BatchTownGeometry(p);return npc;
        }
        void BuildTrainingYard(Transform p,TownStationDefinition s)
        {
            var wood=TownMat("Timber",.23f,.16f,.1f);var straw=TownMat("Straw",.58f,.47f,.26f);
            Block(p,"Training sand",new Vector3(39,-.025f,22),new Vector3(20,.1f,20),TownMat("Sand",.4f,.36f,.26f));
            for(int n=0;n<4;n++)
            {
                float x=32+n*4.4f;Shape("Dummy post",PrimitiveType.Cylinder,p,new Vector3(x,1.2f,23),new Vector3(.25f,1.2f,.25f),wood);
                Shape("Training dummy",PrimitiveType.Capsule,p,new Vector3(x,2,23),new Vector3(1.1f,.7f,.7f),straw);
                Shape("Dummy head",PrimitiveType.Sphere,p,new Vector3(x,3,23),Vector3.one*.7f,straw);
                Block(p,"Dummy arms",new Vector3(x,2.3f,23),new Vector3(2.8f,.2f,.2f),wood);
                for(int ring=0;ring<3;ring++){var r=Ring(p,new Vector3(x,2,22.58f),.18f+ring*.15f,ring%2==0?red:trim,.05f);r.transform.localRotation=Quaternion.Euler(90,0,0);}
            }
            for(int n=0;n<10;n++){float x=29+n*2.3f;Block(p,"Training fence",new Vector3(x,1,33),new Vector3(.2f,2,.2f),wood);Block(p,"Training rail",new Vector3(x,1.3f,33),new Vector3(2.3f,.15f,.15f),wood);}
            TownLantern(p,new Vector3(29,0,12));TownLantern(p,new Vector3(49,0,12));
        }
        void BuildTownPortal(Transform p,Vector2 at)
        {
            var root=new GameObject("Golden Rift Portal").transform;root.SetParent(p,false);root.localPosition=new Vector3(at.x,0,at.y);
            Shape("Portal dais",PrimitiveType.Cylinder,root,new Vector3(0,.12f,0),new Vector3(7,.12f,5),darkStone);
            var goldLight=PortalMat("Portal gold",new Color(1,.73f,.19f));var orange=PortalMat("Portal orange",new Color(1,.3f,.045f));
            townPortal=new GameObject("Turning golden runes").transform;townPortal.SetParent(root,false);townPortal.localPosition=Vector3.up*4.2f;
            for(int n=0;n<3;n++)
            {
                var ring=Ring(root,new Vector3(0,4.2f,-n*.06f),2.35f+n*.17f,n==1?goldLight:orange,n==1?.13f:.08f);
                ring.transform.localRotation=Quaternion.Euler(90,0,0);ring.transform.localScale=new Vector3(1,1,1.65f);
            }
            for(int n=0;n<18;n++)
            {
                float a=n*Mathf.PI*2/18;var rune=Block(townPortal,"Orbiting sigil",new Vector3(Mathf.Cos(a)*2.77f,Mathf.Sin(a)*2.77f*1.6f,0),new Vector3(.09f,.28f,.09f),goldLight);rune.transform.localRotation=Quaternion.Euler(0,0,a*Mathf.Rad2Deg);
                var spark=Shape("Rift ember",PrimitiveType.Sphere,root,Vector3.zero,Vector3.one*(.06f+n%3*.025f),n%2==0?goldLight:orange);portalSparks.Add(spark.transform);
            }
            // Translucent elliptical membranes leave the destination visible through the opening.
            var fadeShader=Resources.Load<Shader>("TownFade");if(!materials.TryGetValue("Portal membrane",out var veil)){veil=new Material(fadeShader){name="HELLSCRIPT Portal membrane"};veil.SetColor("_BaseColor",new Color(1,.37f,.04f,.22f));materials["Portal membrane"]=veil;}
            var membrane=Shape("Portal membrane",PrimitiveType.Sphere,root,new Vector3(0,4.2f,.1f),new Vector3(4.5f,7.3f,.12f),veil);
            var light=new GameObject("Portal glow").AddComponent<Light>();light.transform.SetParent(root,false);light.transform.localPosition=new Vector3(0,2,-1);light.type=LightType.Point;light.color=new Color(1,.45f,.08f);light.intensity=3;light.range=12;light.shadows=LightShadows.None;
        }
    }
}
