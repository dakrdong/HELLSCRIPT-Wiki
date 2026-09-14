using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public static class RiftBossPlacement
    {
        public const float MinimumDistance=6,MaximumDistance=18,MaximumPath=26;
        // Placement has its own deterministic order and never consumes combat or reward RNG.
        public static Vector2? NearPlayer(RiftLayout layout,RiftNavigation navigation,Vector2 player,Func<Vector2,bool> safe)
        {
            uint seed=RiftGenerator.Derive(layout.mapSeed,"roaming-boss");
            float phase=RandomStream.Unit(ref seed)*Mathf.PI*2;
            foreach(float radius in new[]{10f,8f,12f,6f,14f,16f,18f})
                for(int n=0;n<32;n++)
                {
                    float angle=phase+n*Mathf.PI/16;var point=player+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;
                    if(!navigation.CanLand(point,1.2f)||!navigation.Reachable(point)||!safe(point))continue;
                    // Reject a nearby point across a wall if reaching it requires a long detour.
                    if(navigation.Length(player,point)>MaximumPath)continue;
                    return point;
                }
            return null;
        }
    }
}
