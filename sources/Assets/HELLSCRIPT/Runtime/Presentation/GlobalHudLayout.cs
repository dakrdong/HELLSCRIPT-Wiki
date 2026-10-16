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
        public Color gold=new Color(1.0f,0.8235294f,0.2470588f,1f);
        public Color pale=new Color(1.0f,1.0f,1.0f,1f);
        public Color hpColor=new Color(1.0f,0.3529412f,0.372549f,1f);
        public Color resourceColor=new Color(0.2078431f,0.654902f,1.0f,1f);
        public Color shieldColor=new Color(0.3686275f,0.8784314f,0.8313725f,1f);
        public Color vitalTrack=new Color(0.9019608f,0.9137255f,0.9411765f,1f);
        public Color xpTrack=new Color(0.9019608f,0.9137255f,0.9411765f,1f);
        public Color xpColor=new Color(1.0f,0.6235294f,0.1098039f,1f);
        public Color slotPlate=new Color(1.0f,0.9647059f,0.8901961f,1f);
        public Color cooldownTint=new Color(0.1647059f,0.1921569f,0.3137255f,0.6f);
        public Color disabledTint=new Color(0.5411765f,0.5764706f,0.7098039f,1f);
        public Color activeTint=new Color(1.0f,0.8235294f,0.2470588f,1f);
        public Color resourceWarning=new Color(0.2078431f,0.654902f,1.0f,1f);
        public Color textShadow=new Color(0.1647059f,0.1921569f,0.3137255f,1f);
        public Color deadTint=new Color(0.5411765f,0.5764706f,0.7098039f,1f);
        public Color expiredTint=new Color(0.5411765f,0.5764706f,0.7098039f,0.35f);
        public float valueFont=12f;
        public float levelFont=20f;
        public float captionFont=14f;
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
        public readonly bool landscape,wrapped,compact;
        public readonly float presentationScale,collapsedStatusWidth,textScale;
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
            compact=landscape&&pixelsHigh<600&&pixelsWide/pixelsHigh>=2;
            wrapped=!landscape;
            // Keep the 1600x900 area scale and its floor. Each orientation has its own approved
            // composition; convert its design points back to logical units exactly once.
            float requested=Mathf.Max(style.minimumScale,Mathf.Sqrt(Mathf.Max(1,pixelsWide*pixelsHigh)/(style.referenceLong*style.referenceShort)))*interfaceFactor;
            float referenceW=!landscape?440:compact?956:1600,referenceH=!landscape?956:compact?440:900;
            float baseline=Mathf.Sqrt(referenceW*referenceH/(style.referenceLong*style.referenceShort));
            presentationScale=Mathf.Min(requested/baseline,Mathf.Min(pixelsWide/referenceW,pixelsHigh/referenceH));
            scale=Mathf.Max(.01f,presentationScale*baseline);
            width=pixelsWide/scale;height=pixelsHigh/scale;
            float k=presentationScale,unit=k/scale,w=pixelsWide/k,h=pixelsHigh/k;
            textScale=k*(landscape&&!compact?1:.72f);
            Rect R(float x,float y,float rw,float rh)=>new Rect(x*unit,y*unit,rw*unit,rh*unit);
            float margin=!landscape?16:compact?66:32,slot=!landscape?50:compact?48:76;
            float gap=!landscape?4:compact?8:12,group=!landscape?16:compact?14:24;
            // Keep each portrait group compact; use spare width for bounded icons, never wider gaps.
            if(!landscape)slot=Mathf.Min(slot,(w-2*margin-6*gap-group)/8);
            float row3=3*slot+2*gap,row8=8*slot+6*gap+group;
            float first=!landscape?(w-row8)*.5f:w-margin-row8,slotY=!landscape?147:compact?37:54;
            for(int i=0;i<3;i++)potions[i]=R(first+i*(slot+gap),slotY,slot,slot);
            potionBounds=R(first,slotY-18,row3,slot+18);
            float start=first+row3+group;
            ultimate=R(start,slotY,slot,slot);
            for(int i=0;i<4;i++)actives[i]=R(start+(i+1)*(slot+gap),slotY,slot,slot);
            float sealX=!landscape?16:compact?70:24,sealY=!landscape?58:compact?34:60,sealSize=!landscape?60:compact?64:112;
            seal=R(sealX,sealY,sealSize,sealSize);
            float lw=landscape&&!compact?72:48,lh=landscape&&!compact?26:20;
            level=R(sealX+(sealSize-lw)/2,sealY-(landscape&&!compact?16:12),lw,lh);
            float barX=!landscape?88:compact?146:152,barY=!landscape?62:compact?38:86;
            float barW=Mathf.Min(!landscape?260:compact?200:300,w-barX-(!landscape?76:compact?60:90));
            float barH=!landscape?14:compact?15:20,barGap=!landscape?5:compact?6:8;
            resource=R(barX,barY,barW,barH);hp=R(barX,barY+barH+barGap,barW,barH);
            potionSettings=R(barX+barW+12,barY-2,landscape&&!compact?48:44,landscape&&!compact?48:44);
            shieldLine=R(barX,barY+2*barH+barGap,barW,4);
            shield=new Rect(shieldLine.x,shieldLine.yMax,shieldLine.width,(landscape&&!compact?18:14)*unit);
            float statusSize=!landscape?32:compact?30:44,sg=landscape&&!compact?8:6;
            statusIcon=statusSize*unit;statusGap=sg*unit;buttonHit=44*unit;
            float sy=!landscape?200:compact?88:134,sx=!landscape?16:first;
            float caption=Mathf.Max(style.statusTextHeight*scale,FontSize(style.statusFont,(int)style.minimumFont)+2)/k;
            collapsedStatusWidth=(4.5f*statusSize+4*sg)*unit;
            maximumStatusWidth=Mathf.Min(8*statusSize+7*sg,w-sx-44-12)*unit;
            status=R(sx,sy,collapsedStatusWidth/unit,statusSize+caption);
            float xpY=!landscape?40:12,xpX=!landscape?16:compact?70:32;
            xp=R(xpX,xpY,w-xpX-margin,landscape&&!compact?8:6);
            xpText=R(!landscape?88:compact?146:32,xpY+9,200,landscape&&!compact?22:16);
            float button=landscape&&!compact?60:44,right=!landscape?12:compact?66:24,top=!landscape?110:compact?58:74;
            edict=R(w-right-button,h-top-button,button,button);
            occupiedHeight=Mathf.Max(status.yMax,Mathf.Max(seal.yMax,potionBounds.yMax))+8*unit;
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
        public int FontSize(float basis,int minimum)=>Mathf.Max(1,Mathf.RoundToInt(Mathf.Max(basis,minimum)*textScale));
        public static float ContentWidth(int count,float icon=44,float gap=8)=>Mathf.Max(0,count)*icon+Mathf.Max(0,count-1)*gap;
        public static float EdgeAlpha(float distance,float width)
        {if(width<=0)return 1;float t=Mathf.Clamp01(distance/width);return t*t*(3-2*t);}
        public static Vector2 FadeWidths(float scroll,float maximum,float cap=22)
            =>new Vector2(Mathf.Min(cap,Mathf.Max(0,scroll)),Mathf.Min(cap,Mathf.Max(0,maximum-scroll)));
    }
}
