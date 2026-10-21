using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hellscript
{
    // A conversation is a one-line story scene: the resident's figure, the greeting written out and the services offered
    // at once. The shared shell owns the bottom dock, safe area and input; the story dialogue draws inside it.
    public static class NpcDialogueWindow
    {
        public static ContentWindowView Open(Transform parent,NpcProfile profile,Func<float> readingScale,Action<int> service,Action closed,string listener="")
        {
            ContentWindowView window=null;
            bool merchant=profile.Station==TownStation.Merchant||profile.Station==TownStation.Gambler;
            var choices=new List<StoryChoice>();
            if(profile.Station.HasValue)
            {
                choices.Add(new StoryChoice("npc-service",merchant?"구매":TownLayout.Station(profile.Station.Value).action,()=>{window.Close();service?.Invoke(0);},true));
                if(merchant)choices.Add(new StoryChoice("npc-sell","판매",()=>{window.Close();service?.Invoke(1);}));
            }
            choices.Add(new StoryChoice("npc-goodbye","대화를 마친다",()=>window.Close()));
            var greeting=new StoryDialogueState(new[]{new StoryLine(profile.id,profile.greeting)});
            window=ContentWindowView.Open(parent,profile.name,EquipmentViewSource.Catalog,readingScale,
                StoryDialogueWindow.Scene(greeting,readingScale,()=>choices,listener:listener,choicesAtOnce:true,closable:true,lineName:"NPC greeting"),
                closed,maximumSize:StoryDialogueWindow.Size,bottomDock:true);
            return window;
        }
    }
}
