using UnityEngine;

namespace Hellscript
{
    public sealed class InventoryLayout
    {
        public readonly bool split,three;
        public readonly Rect header,footer,list,detail,comparison,tool;
        public InventoryLayout(float width,float height,bool detailOpen,float footerHeight=72)
        {
            float w=Mathf.Max(1,width),h=Mathf.Max(1,height),head=h<600?76:100;
            float foot=Mathf.Clamp(footerHeight,72,Mathf.Max(72,h-head-64));
            split=w>=820;three=w>=1440;
            header=new Rect(0,0,w,head);footer=new Rect(0,h-foot,w,foot);
            tool=new Rect(10,head+8,w-20,Mathf.Max(1,h-head-foot-16));
            if(split)
            {
                float left=Mathf.Clamp(w*.27f,280,400),remaining=tool.width-left-10;
                list=new Rect(tool.x,tool.y,left,tool.height);
                detail=new Rect(list.xMax+10,tool.y,three?(remaining-10)/2:remaining,tool.height);
                comparison=new Rect(detail.xMax+10,tool.y,three?detail.width:0,tool.height);
            }
            else {list=detail=tool;comparison=new Rect();}
        }
    }
}
