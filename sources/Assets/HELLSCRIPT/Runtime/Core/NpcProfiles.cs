using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    [Serializable] public sealed class NpcProfile
    {
        public string id,name,station,residentId,country,gender,food,appearance,palette,greeting,portrait,role;
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
        // Speakers who never stand in the town: the prologue's voice and the three heroes. Kept out of All so
        // town placement and reach checks only see residents.
        public const string Voice="prologue-voice";
        static readonly NpcProfile[] Speakers={
            new NpcProfile{id=Voice,name="하늘의 목소리",role="칙령의 목소리",portrait="Art/NpcPortraits/prologue-divine-voice"},
            new NpcProfile{id="hero-warrior",name="전사",role="계시를 받은 자",portrait="Art/NpcPortraits/hero-warrior"},
            new NpcProfile{id="hero-ranger",name="궁수",role="계시를 받은 자",portrait="Art/NpcPortraits/hero-ranger"},
            new NpcProfile{id="hero-mage",name="마법사",role="계시를 받은 자",portrait="Art/NpcPortraits/hero-mage"}};
        public static NpcProfile ForStation(TownStation station)=>All.FirstOrDefault(p=>p.Station==(station==TownStation.Reroller?TownStation.Blacksmith:station));
        public static NpcProfile ForResident(string id)=>All.FirstOrDefault(p=>p.residentId==id);
        public static bool WithinReach(NpcProfile profile,Vector2 position)=>profile!=null&&Vector2.Distance(profile.Position,position)<=TownLayout.InteractionRadius;
        public static NpcProfile Nearby(Vector2 position)=>All.Where(p=>WithinReach(p,position)).OrderBy(p=>(p.Position-position).sqrMagnitude).FirstOrDefault();
    }
}
