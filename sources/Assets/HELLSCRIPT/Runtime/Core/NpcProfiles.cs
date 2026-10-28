using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    [Serializable] public sealed class NpcProfile
    {
        public string id,name,station,residentId,country,gender,food,appearance,palette,greeting,portrait,portraitFacing,portraitMirror,role;
        // The base portrait looks toward portraitFacing ("right" when empty); portraitMirror retains its authored mirror reference.
        // Two figures in a scene face each other: the one on the left looks right and the one on the right looks left.
        public bool IsHero=>id=="hero-warrior"||id=="hero-ranger"||id=="hero-mage";
        // Hero masters are full-body; their upper 70% matches the NPCs' thigh-up story composition.
        public Rect PortraitUvFor(bool onLeft)
        {
            var frame=IsHero?new Rect(0,.3f,1,.7f):new Rect(0,0,1,1);
            string facing=IsHero?portraitFacing:string.IsNullOrEmpty(portraitFacing)?"right":portraitFacing;
            bool mirrored=(IsHero||!string.IsNullOrEmpty(portraitMirror))&&facing!=(onLeft?"right":"left");
            return mirrored?new Rect(frame.xMax,frame.y,-frame.width,frame.height):frame;
        }
        // The guide's mouth and chin sit below the older narration crop. Keep the sampled pixels square.
        public Rect FaceUvFor(float textureAspect)=>id==NpcProfiles.Guide?new Rect(.16f,.5f,.68f,.68f*textureAspect):new Rect(.25f,.62f,.5f,.5f*textureAspect);
        public string PortraitFor(bool onLeft,HeroGender gender=HeroGender.Male)
        {
            // Every figure reuses its base texture; the shared story renderer applies PortraitUvFor to either slot.
            return IsHero?portrait+(gender==HeroGender.Female?"-f":""):portrait;
        }
        public TownStation? Station=>Enum.TryParse(station,out TownStation value)?value:(TownStation?)null;
        public Vector2 Position=>Station.HasValue?TownLayout.Station(Station.Value).NpcPosition:TownLayout.Residents.Single(r=>r.id==residentId).position;
        public Vector2 ApproachPosition=>Position+Vector2.down*TownLayout.ArriveRadius;
        public string Role=>!string.IsNullOrEmpty(role)?role:Station.HasValue?TownLayout.Station(Station.Value).name:"마을 주민";
    }
    // Authored presentation definitions. No account/save state belongs to this catalog.
    public static class NpcProfiles
    {
        [Serializable] sealed class Data {public NpcProfile[] profiles;}
        static NpcProfile[] profiles;
        public static IReadOnlyList<NpcProfile> All=>profiles??=JsonUtility.FromJson<Data>(Resources.Load<TextAsset>("NpcProfiles").text).profiles;
        public static NpcProfile Find(string id)=>All.FirstOrDefault(p=>p.id==id)??Speakers.FirstOrDefault(p=>p.id==id);
        // A scene names its listener by portrait path; the figure drawn is the one that faces the speaker.
        public static NpcProfile FindByPortrait(string path)=>string.IsNullOrEmpty(path)?null:All.Concat(Speakers).FirstOrDefault(p=>p.portrait==path);
        public static IEnumerable<NpcProfile> WithSpeakers=>All.Concat(Speakers);
        // Speakers who never stand in the town: the prologue's voice and the three heroes. Kept out of All so
        // town placement and reach checks only see residents. The village chief speaks only in dialogue until he is given a place in the town.
        public const string Voice="prologue-voice",Commander="pit-commander",Guide="guide-goddess",Chief="dun-kas";
        static readonly NpcProfile[] Speakers={
            new NpcProfile{id=Voice,name="하늘의 목소리",role="칙령의 목소리",portrait="Art/NpcPortraits/prologue-divine-voice",portraitFacing="right",portraitMirror="Assets/HELLSCRIPT/Art/NpcPortraitMirrors/prologue-divine-voice-mirror.png"},
            new NpcProfile{id=Guide,name="벨리나",role="칙령의 여신",portrait="Art/NpcPortraits/guide-goddess",portraitFacing="right",portraitMirror="Assets/HELLSCRIPT/Art/NpcPortraitMirrors/guide-goddess-mirror.png"},
            new NpcProfile{id=Commander,name="사령관",role="최종 시험 감독관",portrait="Art/NpcPortraits/pit-commander",portraitFacing="right",portraitMirror="Assets/HELLSCRIPT/Art/NpcPortraitMirrors/pit-commander-mirror.png"},
            new NpcProfile{id=Chief,name="던 카스",role="촌장",portrait="Art/NpcPortraits/dun-kas",portraitFacing="right",portraitMirror="Assets/HELLSCRIPT/Art/NpcPortraitMirrors/dun-kas-mirror.png"},
            new NpcProfile{id="hero-warrior",name="전사",role="수습전사",portrait="Art/NpcPortraits/hero-warrior",portraitFacing="left"},
            new NpcProfile{id="hero-ranger",name="궁수",role="수습전사",portrait="Art/NpcPortraits/hero-ranger",portraitFacing="left"},
            new NpcProfile{id="hero-mage",name="마법사",role="수습전사",portrait="Art/NpcPortraits/hero-mage",portraitFacing="left"}};
        public static NpcProfile ForStation(TownStation station)=>All.FirstOrDefault(p=>p.Station==(station==TownStation.Reroller?TownStation.Blacksmith:station));
        public static NpcProfile ForResident(string id)=>All.FirstOrDefault(p=>p.residentId==id);
        public static bool WithinReach(NpcProfile profile,Vector2 position)=>profile!=null&&Vector2.Distance(profile.Position,position)<=TownLayout.InteractionRadius;
        public static NpcProfile Nearby(Vector2 position)=>All.Where(p=>WithinReach(p,position)).OrderBy(p=>(p.Position-position).sqrMagnitude).FirstOrDefault();
    }
}
