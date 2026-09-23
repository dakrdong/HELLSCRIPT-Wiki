using System;
using UnityEngine;

namespace Hellscript
{
    [Serializable] public sealed class GlobalHudStyle
    {
        public float referenceLong=1600,referenceShort=900,margin=32;
        public float landscapeSkill=76.8f,portraitSkill=64,landscapePotion=45,portraitPotion=60,skillGap=12,groupGap=24;
        public float statusIcon=44,statusGap=8,fade=22,collapsed=230,expanded=408,expandSeconds=.18f;
        public float xpThickness=3,xpBottom=16;
        public float minimumScale=.4f,sealX=24,sealY=60,landscapeSeal=112,portraitSeal=96,levelY=42,levelWidth=88,levelHeight=24;
        public float landscapeVitalsX=152,portraitVitalsX=136,landscapeVitalWidth=300,portraitVitalWidth=260,minimumVitalWidth=120;
        public float landscapeVitalHeight=20,portraitVitalHeight=18,vitalGap=8,landscapeVitalY=86,portraitVitalY=96;
        public float centerClearance=160,portraitWrapWidth=760,skillBottom=32,rowGap=28;
        public float potionRowGap=36,landscapePotionPitch=96,portraitPotionPitch=84;
        public float portraitStatusIcon=40,landscapeStatusY=150,portraitStatusY=188,portraitStatusX=56,statusTextHeight=24;
        public float portraitXpY=72,landscapeXpTextY=24,portraitXpTextY=44;
        public Color gold=new Color(0.77f,0.66f,0.45f,1f);
        public Color pale=new Color(0.953f,0.922f,0.855f,1f);
        public Color hpColor=new Color(0.784f,0.251f,0.235f,1f);
        public Color resourceColor=new Color(0.2f,0.541f,0.796f,1f);
        public Color shieldColor=new Color(0.35f,0.87f,0.83f,1f);
        public Color vitalTrack=new Color(0.035f,0.045f,0.052f,0.7f);
        public Color xpTrack=new Color(0.2f,0.17f,0.12f,0.65f);
        public Color xpColor=new Color(0.847f,0.718f,0.455f,1f);
        public Color slotPlate=new Color(0.025f,0.03f,0.04f,0.66f);
        public Color cooldownTint=new Color(0f,0f,0f,0.7f);
        public Color disabledTint=new Color(0.38f,0.38f,0.4f,0.75f);
        public Color activeTint=new Color(1f,0.87f,0.52f,1f);
        public Color resourceWarning=new Color(0.45f,0.72f,1f,1f);
        public Color textShadow=new Color(0.02f,0.025f,0.03f,0.95f);
        public Color deadTint=new Color(0.55f,0.55f,0.55f,1f);
        public Color expiredTint=new Color(0.4f,0.4f,0.4f,0.35f);
        public float valueFont=16f;
        public float levelFont=20f;
        public float captionFont=16f;
        public float cooldownFont=25f;
        public float potionCooldownFont=22f;
        public float statusFont=16f;
        public float extraFont=18f;
        public float minimumFont=10f;
        public float minimumValueFont=11f;
        public float minimumLevelFont=12f;
        public float captionHeight=22f;
        public float shieldHeight=12f;
        public float shieldFont=12f;
        public float xpTextHeight=18f;
        public float potionCaptionPadding=14f;
        public float potionGlyph=18f;
        public float statusBadge=14f;
        public float statusBadgeInset=10f;
        public float controlGap=8f;
        public float controlSize=32f;
        public float controlAlpha=0.68f;
        public float scrollSensitivity=40f;
        public float xpTickHeight=9f;
        public float xpMajorTickHeight=13f;
        public float xpTickWidth=2f;
        public static GlobalHudStyle Load()
        {
            var file=Resources.Load<TextAsset>("Data/GlobalHudLayout");
            return file==null?new GlobalHudStyle():JsonUtility.FromJson<GlobalHudStyle>(file.text);
        }
    }
    // Bottom-left coordinates. All rectangles are logical; Scale is applied exactly once by the view.
    public sealed class GlobalHudLayout
    {
        public readonly float scale,width,height,statusIcon,statusGap,buttonHit,maximumStatusWidth;
        public readonly bool landscape,wrapped;
        public readonly Rect seal,level,hp,resource,shield,shieldLine,status,xp,xpText,potionTray;
        public readonly Rect[] passives=new Rect[3],actives=new Rect[4],potions=new Rect[3];
        public readonly float occupiedHeight;
        // Reading pages reserve at most 42% for the persistent HUD. Fit the entire HUD into that
        // region instead of clipping the reservation while its controls still extend above it.
        public static float ContentFactor(float w,float h,float requested,GlobalHudStyle style)
        {
            float limit=Mathf.Max(1,h*.42f-12);
            bool Fits(float value){var layout=new GlobalHudLayout(w,h,value,style);return layout.occupiedHeight*layout.scale<=limit;}
            if(Fits(requested))return requested;
            float low=.01f,high=requested;
            for(int i=0;i<16;i++){float mid=(low+high)*.5f;if(Fits(mid))low=mid;else high=mid;}
            return low;
        }
        public GlobalHudLayout(float pixelsWide,float pixelsHigh,float interfaceFactor=1,GlobalHudStyle style=null)
        {
            style??=new GlobalHudStyle();
            landscape=pixelsWide>=pixelsHigh;
            scale=Mathf.Max(style.minimumScale,Mathf.Sqrt(Mathf.Max(1,pixelsWide*pixelsHigh)/(style.referenceLong*style.referenceShort))*interfaceFactor);
            width=pixelsWide/scale;height=pixelsHigh/scale;
            float m=style.margin,s=landscape?style.landscapeSkill:style.portraitSkill,g=style.skillGap;
            float left=landscape?style.landscapeVitalsX:style.portraitVitalsX,barWidth=landscape?style.landscapeVitalWidth:style.portraitVitalWidth;
            float row4=4*s+3*g,row3=3*s+2*g,row7=row4+row3+style.groupGap;
            wrapped=landscape?width<left+barWidth+style.centerClearance+row7+m:width<style.portraitWrapWidth;
            float rightStart=width-m-(landscape&&!wrapped?row7:row4);
            // Keep the action row on the bottom baseline. At enlarged portrait sizes, stack
            // vitals above the class seal instead of pushing the whole action group upward.
            bool stackedVitals=!landscape&&left+barWidth+g>rightStart;
            if(stackedVitals){left=style.sealX;barWidth=rightStart-g-left;}
            barWidth=Mathf.Min(barWidth,Mathf.Max(style.minimumVitalWidth,width-left-m));
            float vitalHeight=Mathf.Max(landscape?style.landscapeVitalHeight:style.portraitVitalHeight,FontSize(style.valueFont,(int)style.minimumValueFont)/scale+2),baseY=landscape?style.landscapeVitalY:style.portraitVitalY;
            float sealSize=landscape?style.landscapeSeal:style.portraitSeal;
            if(stackedVitals)baseY=style.sealY+sealSize+style.rowGap+style.xpTextHeight+style.vitalGap;
            // Text and its row shrink together. Compensate only for whole-pixel font rounding,
            // never for an unscaled pixel floor that would enlarge text after the next HUD refresh.
            float Row(float height,float font)=>Mathf.Max(height,FontSize(font,1)/scale);
            seal=new Rect(style.sealX,style.sealY,sealSize,sealSize);
            level=new Rect(seal.center.x-style.levelWidth*.5f,style.levelY,style.levelWidth,Row(style.levelHeight,style.levelFont));
            resource=new Rect(left,baseY,barWidth,vitalHeight);
            hp=new Rect(left,baseY+vitalHeight+style.vitalGap,barWidth,vitalHeight);
            shieldLine=new Rect(left,hp.yMax+1,barWidth,2);
            shield=new Rect(left,hp.yMax+3,barWidth,Row(style.shieldHeight,style.shieldFont));
            float activeY=style.skillBottom;
            if(landscape)activeY=Mathf.Max(activeY,style.xpBottom+style.xpThickness+style.captionHeight+8);
            if(landscape&&!wrapped)
            {
                for(int i=0;i<3;i++)passives[i]=new Rect(rightStart+i*(s+g),activeY,s,s);
                rightStart+=row3+style.groupGap;
            }
            else for(int i=0;i<3;i++)passives[i]=new Rect(rightStart+(row4-row3)*.5f+i*(s+g),activeY+s+style.rowGap,s,s);
            for(int i=0;i<4;i++)actives[i]=new Rect(rightStart+i*(s+g),activeY,s,s);
            float bottle=landscape?style.landscapePotion:style.portraitPotion;
            float potionY=Mathf.Max(actives[0].yMax,passives[0].yMax)+style.potionRowGap;
            float pitch=landscape?style.landscapePotionPitch:style.portraitPotionPitch;
            float potionStart=width-m-3*pitch;
            for(int i=0;i<3;i++)potions[i]=new Rect(potionStart+i*pitch+(pitch-bottle)*.5f,potionY,bottle,bottle);
            potionTray=new Rect(potionStart,potionY-style.captionHeight-8,3*pitch,bottle+style.captionHeight+16);
            statusIcon=landscape?style.statusIcon:style.portraitStatusIcon;statusGap=style.statusGap;buttonHit=Mathf.Max(44,40/scale);
            float sy=Mathf.Max(landscape?style.landscapeStatusY:style.portraitStatusY,shield.yMax+1);
            status=new Rect(landscape?left:style.portraitStatusX,sy,landscape?style.collapsed:4*statusIcon+3*statusGap,statusIcon+style.statusTextHeight);
            if(stackedVitals)status=new Rect(left,Mathf.Max(sy,shield.yMax+style.vitalGap),Mathf.Min(status.width,barWidth),status.height);
            float other=landscape?passives[0].x:width-m;
            maximumStatusWidth=landscape?Mathf.Max(style.collapsed,Mathf.Min(style.expanded,other-status.x-buttonHit-20)):status.width;
            xp=new Rect(landscape?m:left,landscape?style.xpBottom:style.portraitXpY,landscape?width-2*m:barWidth,style.xpThickness);
            xpText=new Rect(landscape?m:left,landscape?style.landscapeXpTextY:style.portraitXpTextY,200,Row(style.xpTextHeight,style.captionFont));
            if(stackedVitals){xp.y=seal.yMax+8;xpText.y=xp.yMax+4;xpText.width=barWidth;}
            occupiedHeight=Mathf.Max(status.yMax,Mathf.Max(passives[0].yMax,potionTray.yMax))+16;
        }
        public Rect Pixels(Rect r)=>new Rect(r.x*scale,r.y*scale,r.width*scale,r.height*scale);
        public int FontSize(float basis,int minimum)=>Mathf.Max(1,Mathf.RoundToInt(Mathf.Max(basis,minimum)*scale));
        public static float ContentWidth(int count,float icon=44,float gap=8)=>Mathf.Max(0,count)*icon+Mathf.Max(0,count-1)*gap;
        public static float EdgeAlpha(float distance,float width)
        {if(width<=0)return 1;float t=Mathf.Clamp01(distance/width);return t*t*(3-2*t);}
        public static Vector2 FadeWidths(float scroll,float maximum,float cap=22)
            =>new Vector2(Mathf.Min(cap,Mathf.Max(0,scroll)),Mathf.Min(cap,Mathf.Max(0,maximum-scroll)));
    }
}
