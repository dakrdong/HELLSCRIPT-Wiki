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
        public float minimumScale=.4f,sealX=24,sealY=60,landscapeSeal=112,portraitSeal=96,levelY=42,levelWidth=88,levelHeight=24,edictGap=10;
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
        public readonly Rect seal,edict,potionSettings,level,hp,resource,shield,shieldLine,status,xp,xpText,potionBounds,ultimate;
        public readonly Rect[] actives=new Rect[4],potions=new Rect[3];
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
            float s=style.landscapeSkill,g=style.skillGap;
            float row5=5*s+4*g,row3=3*s+2*g,row8=row5+row3+style.groupGap;
            float requested=Mathf.Sqrt(Mathf.Max(1,pixelsWide*pixelsHigh)/(style.referenceLong*style.referenceShort))*interfaceFactor;
            // One landscape composition, fitted as a group at every aspect ratio, never split or rewrapped.
            // A portrait screen has no use for the empty middle of the landscape composition: the potion group closes up to the
            // vitals (and the potion settings button beside them), so the whole HUD is as large as the screen's width allows.
            float settingsSide=2*Mathf.Max(style.landscapeVitalHeight,style.valueFont+2)+style.vitalGap;
            float compactLong=style.landscapeVitalsX+style.landscapeVitalWidth+style.edictGap+settingsSide+style.edictGap+row8+style.margin;
            float composition=landscape?style.referenceLong:Mathf.Min(style.referenceLong,compactLong);
            float fit=Mathf.Min(pixelsWide/composition,pixelsHigh/style.referenceShort);
            scale=Mathf.Max(.01f,Mathf.Min(requested,fit));
            width=pixelsWide/scale;height=pixelsHigh/scale;
            float origin=(width-composition)*.5f,m=style.margin+origin;
            float left=style.landscapeVitalsX+origin,barWidth=style.landscapeVitalWidth;
            wrapped=false;
            float rightStart=width-m-row8;
            barWidth=Mathf.Min(barWidth,Mathf.Max(style.minimumVitalWidth,width-left-m));
            float vitalHeight=Mathf.Max(style.landscapeVitalHeight,style.valueFont+2),baseY=style.landscapeVitalY;
            float sealSize=style.landscapeSeal;
            // Text and its row shrink together. Compensate only for whole-pixel font rounding,
            // never for an unscaled pixel floor that would enlarge text after the next HUD refresh.
            float Row(float height,float font)=>Mathf.Max(height,font);
            seal=new Rect(origin+style.sealX,style.sealY,sealSize,sealSize);
            // The Hunt Edict button rides on the seal, a skill icon in size, so the combat settings are one tap from the hero.
            edict=new Rect(seal.center.x-s*.5f,seal.yMax+style.edictGap,s,s);
            level=new Rect(seal.center.x-style.levelWidth*.5f,style.levelY,style.levelWidth,Row(style.levelHeight,style.levelFont));
            resource=new Rect(left,baseY,barWidth,vitalHeight);
            hp=new Rect(left,baseY+vitalHeight+style.vitalGap,barWidth,vitalHeight);
            // Right of the two bars, as tall as both: the potion thresholds are one tap from the vitals they guard.
            potionSettings=new Rect(left+barWidth+style.edictGap,resource.y,hp.yMax-resource.y,hp.yMax-resource.y);
            shieldLine=new Rect(left,hp.yMax+1,barWidth,2);
            shield=new Rect(left,hp.yMax+3,barWidth,Row(style.shieldHeight,style.shieldFont));
            float activeY=Mathf.Max(style.skillBottom,style.xpBottom+style.xpThickness+style.captionHeight+8);
            for(int i=0;i<3;i++)potions[i]=new Rect(rightStart+i*(s+g),activeY,s,s);
            potionBounds=new Rect(rightStart,activeY-style.captionHeight,row3,s+style.captionHeight);
            rightStart+=row3+style.groupGap;
            ultimate=new Rect(rightStart,activeY,s,s);
            for(int i=0;i<4;i++)actives[i]=new Rect(rightStart+(i+1)*(s+g),activeY,s,s);
            statusIcon=style.statusIcon;statusGap=style.statusGap;buttonHit=44;
            float sy=Mathf.Max(style.landscapeStatusY,shield.yMax+1);
            status=new Rect(left,sy,style.collapsed,statusIcon+style.statusTextHeight);
            maximumStatusWidth=Mathf.Max(style.collapsed,Mathf.Min(style.expanded,potions[0].x-status.x-buttonHit-20));
            xp=new Rect(m,style.xpBottom,composition-2*style.margin,style.xpThickness);
            xpText=new Rect(m,style.landscapeXpTextY,200,Row(style.xpTextHeight,style.captionFont));
            occupiedHeight=Mathf.Max(status.yMax,potionBounds.yMax)+16;
        }
        // Page units of a content shortcut (and the settings gear): a skill icon's size on screen (skill logical units at the HUD scale, in page units
        // at the canvas scale) plus 15% for the clear margin the emblem art keeps inside its square, not under minimum page units, and small enough
        // that six of them fit between the top edge and hudTop (the page height where the HUD starts). The town asks for 44 screen pixels as its
        // minimum, the battle for the 44 page units its shortcuts always had.
        public static float ShortcutIcon(float skill,float hudScale,float canvasScale,float hudTop,float minimum)
        {
            float card=hudScale/Mathf.Max(.001f,canvasScale);
            return Mathf.Min(Mathf.Max(minimum,skill*1.15f*card),(hudTop-3*12*card-5*UiTheme.Gap*card)/6);
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
