using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        void ReactShield(ShieldEffect shield,float absorbed)
        {
            if(!D4Combat)return;
            if(Hero.heroClass==HeroClass.Warrior)ReflectD4IronWall(shield,absorbed);
            if(Hero.heroClass==HeroClass.Mage)ReactD4MageShield(shield,absorbed);
        }
        EnemyState ChainRouteTarget(Vector2 previous,int previousId,IDictionary<int,int> visits,IEnumerable<EnemyState> observed,ref bool usedConduit,out bool relayed)
        {
            relayed=false;var seen=observed.ToArray();var next=NextChainTarget(previous,previousId,visits,seen);if(next!=null||usedConduit)return next;
            var rod=CS?.effects.FirstOrDefault(e=>e.kind=="d4conduit"&&e.until>State.time);if(rod==null||rod.kind!="d4conduit"||Vector2.Distance(previous,rod.position)>4||!Map.LineClear(previous,rod.position))return null;
            next=NextChainTarget(rod.position,previousId,visits,seen);
            if(next==null)return null;usedConduit=relayed=true;return next;
        }
    }
}
