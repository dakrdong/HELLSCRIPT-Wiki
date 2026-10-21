using System;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // Presentation tokens belong here. Gameplay rarity, costs and progression remain in Core.
    public static class UiTheme
    {
        // Obsidian & Gilt (2026-09-29): warm near-black surfaces, one hairline bronze edge and
        // gold reserved for choice/primary. Every screen surface picks a step of this ramp.
        public const string VoidHex="090807",BackgroundHex="100f0d",InsetHex="0c0b0a",PanelHex="191713",RaisedHex="211e19";
        public const string HoverHex="2b2720",SelectedHex="33291b",BorderHex="3a3228",EdgeHex="5e4f3a",GoldDeepHex="8a6b3c";
        public const string GoldHex="d3af72",GoldBrightHex="f2d9a4",TextHex="ece3cf",MutedHex="a0978a",FaintHex="6e675c";
        public const string SuccessHex="8fc07a",DangerHex="df927b";
        public const string PrimaryHex="93632d",DefeatHex="ee817b",DefeatPanelHex="2c1113",DefeatBorderHex="9b4645";
        public const string DangerFillHex="6e231d",DangerEdgeHex="b0604c";
        // Provider brand colours for the Google sign-in button; not part of the game palette.
        public const string GoogleTextHex="1f1f1f",GoogleStrokeHex="747775";
        public const string ResourceHex="8fb8d6";
        public const string RetreatHex="e98a5a",RejoinHex="6fd0b4";
        public static Color Retreat=>StorageSurface.Hex(RetreatHex);
        public static Color Rejoin=>StorageSurface.Hex(RejoinHex);
        public static Color Resource=>StorageSurface.Hex(ResourceHex);
        // Shared settings palette, used only through the opt-in casual skin.
        public static Color SettingsCardEdge {get;}=StorageSurface.Hex("c9d0e2");
        public static Color SettingsChoiceTint {get;}=StorageSurface.Hex("fff1b8");
        public static Color SettingsInfoFill {get;}=StorageSurface.Hex("eef7ff");
        public static Color SettingsInfoEdge {get;}=StorageSurface.Hex("bfe0ff");
        public static Color SettingsInfoInk {get;}=StorageSurface.Hex("1f4f7d");
        public static Color SettingsWarningFill {get;}=StorageSurface.Hex("fff0f0");
        public static Color SettingsWarningInk {get;}=StorageSurface.Hex("8c2327");
        public static Color SettingsGuideFill {get;}=StorageSurface.Hex("fff9dc");
        public static Color SettingsGuideInk {get;}=StorageSurface.Hex("6b4a00");
        public static Color SettingsOrangeInk {get;}=StorageSurface.Hex("c46d00");
        public static Color SettingsSkyInk {get;}=StorageSurface.Hex("1f6fb5");
        public static Color SettingsGreenInk {get;}=StorageSurface.Hex("26722f");
        public static Color SettingsMint {get;}=StorageSurface.Hex("5ee0d4");
        // Attendance state surfaces; opt-in consumers leave the global theme unchanged.
        public static Color AttendanceReadyFill {get;}=StorageSurface.Hex("e9f9e4");
        public static Color AttendanceMilestoneEdge {get;}=StorageSurface.Hex("e0a400");
        public static Color PlazaSecondaryInk {get;}=StorageSurface.Hex("697190");
        public static Color PlazaDetailInk {get;}=StorageSurface.Hex("4a5272");
        public static Color HuntEdictSelectedTop {get;}=StorageSurface.Hex("ffe26b");
        public static Color HuntEdictDangerTop {get;}=StorageSurface.Hex("ff8b8e");
        public static Color NoticeGemShade=>D4Token("GoldF08A00A1000");
        public static Color EdictChangedInk {get;}=StorageSurface.Hex("b86200");
        public static Color EdictFactInk {get;}=StorageSurface.Hex("8a5600");
        public static Color EdictSuccessFill {get;}=StorageSurface.Hex("e9f9e3");
        public static Color EdictScrollTrack {get;}=StorageSurface.Hex("efe6cf");
        public static Color EdictOffFill {get;}=StorageSurface.Hex("eef0f6");
        // Surfaces of the casual storage canvas that the shared skin does not name.
        public static Color MockWellFill {get;}=StorageSurface.Hex("eef2f9");
        public static Color MockWellEdge {get;}=StorageSurface.Hex("dfe4ef");
        public static Color MockVacantRing {get;}=StorageSurface.Hex("d5dbe8");
        public static Color MockSlotNumber {get;}=StorageSurface.Hex("b9c0d0");
        public static Color MockHoverTop {get;}=StorageSurface.Hex("fff6c2");
        public static Color MockSelectedTop {get;}=StorageSurface.Hex("ffe26b");
        public static Color MockCategoryOn {get;}=StorageSurface.Hex("fff1b8");
        public static Color MockWarningFill {get;}=StorageSurface.Hex("fff0f0");
        public static Color MockNextTab {get;}=StorageSurface.Hex("f6f8fc");
        public static Color MockLockedTab {get;}=StorageSurface.Hex("eef0f5");
        public static Color MockNewTop {get;}=StorageSurface.Hex("f1fbec");
        public static Color MockNewBottom {get;}=StorageSurface.Hex("c9f2a8");
        public static Color MockCellEnd {get;}=StorageSurface.Hex("f1f4fa");
        public static Color MockInfoInk {get;}=StorageSurface.Hex("1f6fb5");
        // Casual events mockup (daily quests and attendance): quest cards, reward pills, claim buttons and the info panel.
        public static Color MockIconTop {get;}=StorageSurface.Hex("fff6c2");
        public static Color MockIconBottom {get;}=StorageSurface.Hex("ffd23f");
        public static Color MockRewardEdge {get;}=StorageSurface.Hex("ffc21a");
        public static Color MockBarClaimed {get;}=StorageSurface.Hex("8ee07a");
        public static Color MockBarReady {get;}=StorageSurface.Hex("4cbf55");
        public static Color MockReadyWash {get;}=StorageSurface.Hex("e9f9e4");
        public static Color MockReadyGlow {get;}=new Color(.298f,.749f,.333f,.3f);
        public static Color MockDisabledEdge {get;}=StorageSurface.Hex("c3c9d6");
        public static Color MockDisabledInk {get;}=StorageSurface.Hex("a3abc2");
        public static Color MockDisabledFill {get;}=StorageSurface.Hex("eceff5");
        public static Color MockInfoFill {get;}=StorageSurface.Hex("eef7ff");
        public static Color MockInfoEdge {get;}=StorageSurface.Hex("9cc8ec");
        public static Color MockOfferWash {get;}=StorageSurface.Hex("eafbe3");
        public static Color MockGildLine {get;}=StorageSurface.Hex("d6b67a");
        public static Color MockChipWash {get;}=StorageSurface.Hex("fff6d6");
        public static Color MockChipInk {get;}=StorageSurface.Hex("b57400");
        public static Color MockRowRule {get;}=StorageSurface.Hex("e9d3a8");
        public static Color MockPurpleFill {get;}=StorageSurface.Hex("f4ebff");
        public static Color MockStrip {get;}=StorageSurface.Hex("fffaf0");
        public static Color MockTrack {get;}=StorageSurface.Hex("e6e9f0");
        public static Color MockRule {get;}=new Color(.165f,.192f,.314f,.12f);
        public static Color MockBody {get;}=StorageSurface.Hex("4a5272");
        public static Color MockMutedInk {get;}=StorageSurface.Hex("6b7393");
        public static Color MockTabGold {get;}=StorageSurface.Hex("ffe26b");
        public static Color MockTabGoldEnd {get;}=StorageSurface.Hex("ffc21a");
        public static Color MockCloseTop {get;}=StorageSurface.Hex("ff9a9d");
        public static Color MockCloseBottom {get;}=StorageSurface.Hex("ff5a5f");
        // Equipment shop (casual): scrollbars, coin ink, vacant slot ring, notice wash, grade inks.
        public static Color MockScrollTrack {get;}=StorageSurface.Hex("f1f1f1");
        public static Color MockScrollThumb {get;}=StorageSurface.Hex("c1c1c1");
        public static Color MockCoin {get;}=StorageSurface.Hex("f08a00");
        public static Color MockOrange {get;}=StorageSurface.Hex("e08a00");
        public static Color MockVacantEdge {get;}=StorageSurface.Hex("dde2ee");
        public static Color MockNoticeFill {get;}=StorageSurface.Hex("e8f4ff");
        public static Color MockRareInk {get;}=StorageSurface.Hex("c48d00");
        public static Color MockLegendInk {get;}=StorageSurface.Hex("ef6c00");
        public static Color MockCloseCoral {get;}=StorageSurface.Hex("ff8a80");
        // Soft cell fills of the character plaza equipment popup: grey, blue and orange grade.
        public static Color MockGradeSoft0 {get;}=StorageSurface.Hex("f3f5fa");
        public static Color MockGradeSoft1 {get;}=StorageSurface.Hex("e3f2ff");
        public static Color MockGradeSoft2 {get;}=StorageSurface.Hex("fff1d6");
        public static Color MockLegendRing {get;}=StorageSurface.Hex("ff9f1c");
        public static Color MockPlateTop {get;}=StorageSurface.Hex("fffdf8");
        public static Color MockDimInk {get;}=StorageSurface.Hex("9aa1b0");
        public static Color MockSkyMid {get;}=StorageSurface.Hex("bfe7ff");
        public static Color MockSkyBottom {get;}=StorageSurface.Hex("fff3d6");
        public static Color MockCheckOn {get;}=StorageSurface.Hex("e3f9dc");
        public static Color MockCheckEdge {get;}=StorageSurface.Hex("767676");
        public static Color EdictQuietFill {get;}=StorageSurface.Hex("f4f6fa");
        public static Color EdictEmptyFill {get;}=StorageSurface.Hex("f7f8fb");
        public static Color EdictSocketFill {get;}=StorageSurface.Hex("e9edf6");
        public static Color NoticePrimaryHover {get;}=StorageSurface.Hex("a2e891");
        public static Color NoticePrimaryHoverEnd {get;}=StorageSurface.Hex("5cc865");
        // This opt-in palette reads the approved skill skin. Other windows retain their ramp.
        static Color D4Color(string id)=>D4Token(SkillTreeD4Looks.Get(id).color);
        static Color D4StrokeColor(string state)=>D4Token(SkillTreeD4Looks.Get(state).stroke);
        static Color D4Alpha(Color ink,float alpha){ink.a*=alpha;return ink;}
        public static Color D4Token(string id)=>SkillTreeD4Looks.ColorToken(id);
        public static Color D4Ink=>D4Color("frame.root.pc");
        public static Color D4Text=>D4Color("frame.root.pc");
        public static Color D4Muted=>D4Color("header.stats.pc");
        public static Color D4GoldLine=>D4StrokeColor("line.g");
        public static Color D4BrownLine=>D4StrokeColor("line.o");
        public static Color D4LockedLine=>D4StrokeColor("line.k");
        public static Color D4SteelLine=>D4StrokeColor("line.s1");
        public static Color D4SteelDim=>D4StrokeColor("line.s2");
        public static Color D4SteelLocked=>D4StrokeColor("line.s3");
        public static Color D4SteelText=>D4Color("panel.unlock.ok");
        public static Color D4SteelTextDim=>D4Color("passive.text.name.o");
        public static Color D4SteelTextLocked=>D4Color("passive.text.name.k");
        public static Color D4Ember=>SkillTreeD4Looks.Paint(SkillTreeD4Looks.Get("chrome.tab.point").backgrounds[0],.5f,.5f);
        public static Color D4TitleTop=>D4Color("chrome.title.gold");
        public static Color D4TitleBottom=>D4TitleTop;
        public static Color D4RankGoldOn=>D4Token("FxGold");
        public static Color D4RankGoldOff=>D4Token(SkillTreeD4Looks.Get("panel.medal.active.landscape.l.lit").arcOff);
        public static Color D4RankSteelOn=>D4Token("FxSteel");
        public static Color D4RankSteelOff=>D4Token(SkillTreeD4Looks.Get("passive.medal.land.coin.l.lit").arcOff);
        public static Color D4GoldGlow=>D4Alpha(D4RankGoldOn,.30f);
        public static Color D4SteelGlow=>D4Alpha(D4RankSteelOn,.28f);
        public static Color D4Groove=>D4Token(SkillTreeD4Looks.Get("line.o.glow").stroke);
        public static Color D4GiltRim(float y)=>D4Ink;
        public static Color D4SteelRim(float y)=>D4Ink;
        public static Color D4ConicGilt(float angle)=>D4Ink;
        public static Color D4ConicSteel(float angle)=>D4Ink;
        public const float D4GlowGain=.50f;
        public static Color D4EdgeFade=>SkillTreeD4Looks.Paint(SkillTreeD4Looks.Get("plate.legend").backgrounds[0],.5f,1);
        public static Color D4GoldDeep=>D4Color("panel.level.land.active");
        public static Color D4Body=>D4Color("panel.desc.pc.active");
        public static Color D4Value=>D4Color("panel.mod.effTo.chosen");
        public static Color D4UnlockTrack=>D4RankGoldOff;
        public static Color D4FxGold=>D4Token("FxGold");
        public static Color D4FxSteel=>D4Token("FxSteel");
        public static Color D4FxAsh=>D4Token("FxAsh");
        public static Color D4FxWhite=>D4Color("header.close");
        public static Color Defeat=>StorageSurface.Hex(DefeatHex);
        public const string ItemTopHex="1f1c17",ItemBottomHex="0d0c0a",ItemInsetHex="12110e";
        public const string ItemRuleHex="5e4f3a",DropHex="efcd86";
        public const int ItemName=20,ItemValue=25;
        public static Color Drop=>StorageSurface.Hex(DropHex);
        public static Color Primary=>StorageSurface.Hex(PrimaryHex);
        public static Color PaidFatigue=>new Color(.69f,.53f,.85f);
        public static Color PaidFatigueText=>new Color(.77f,.68f,.88f);
        public static Color Background=>StorageSurface.Hex(BackgroundHex);
        public static Color Panel=>StorageSurface.Hex(PanelHex);
        public static Color Gold=>StorageSurface.Hex(GoldHex);
        public static Color Text=>StorageSurface.Hex(TextHex);
        public static Color Muted=>StorageSurface.Hex(MutedHex);
        public static Color Border=>StorageSurface.Hex(BorderHex);
        public static Color Edge=>StorageSurface.Hex(EdgeHex);
        public static Color Inset=>StorageSurface.Hex(InsetHex);
        public static Color Raised=>StorageSurface.Hex(RaisedHex);
        public static Color Faint=>StorageSurface.Hex(FaintHex);
        public static Color GoldBright=>StorageSurface.Hex(GoldBrightHex);
        public static Color Tint(string hex,float alpha=1){var c=StorageSurface.Hex(hex);c.a*=alpha;return c;}
        public static Color Scrim=>new Color(.02f,.018f,.015f,.82f);
        public static Color Success=>StorageSurface.Hex(SuccessHex);
        public const string ClaimableHex="55e68b";
        public static Color Claimable=>StorageSurface.Hex(ClaimableHex);
        public static Color Danger=>StorageSurface.Hex(DangerHex);
        public const int Title=20,Heading=15,Body=12,Caption=10;
        public const float Gap=6,Padding=12,Header=40,ActionHeight=40;
        public const float CompactEquipmentSlot=40;
        public static float SlotSize(bool landscape)=>landscape?54:52;
        public static float Scale(Rect safe)=>Mathf.Max(.1f,Mathf.Min(safe.width/(safe.width>safe.height?800:405),safe.height/(safe.width>safe.height?450:720)));
        public static int Columns(float available,bool landscape)=>Mathf.Max(1,Mathf.FloorToInt((available+Gap)/(SlotSize(landscape)+Gap)));
        public static void Button(Selectable control,bool primary=false,bool destructive=false)
        {
            if(control is UiButton button){button.Configure(destructive?UiButtonRole.Destructive:primary?UiButtonRole.Primary:UiButtonRole.Action);return;}
            var c=control.colors;c.normalColor=Color.white;c.highlightedColor=new Color(1.12f,1.12f,1.08f);c.selectedColor=c.highlightedColor;
            c.pressedColor=new Color(.8f,.8f,.8f);c.disabledColor=new Color(.55f,.55f,.55f,.65f);c.fadeDuration=.1f;control.colors=c;
            if(control.targetGraphic is StorageSurface s)
                s.Paint(destructive?DangerFillHex:primary?PrimaryHex:RaisedHex,destructive?"3a110f":primary?"4e3218":PanelHex,destructive?DangerEdgeHex:primary?GoldHex:BorderHex);
        }
        public const float ButtonFade=.085f;
        public static void Backdrop(Selectable control)
        {
            if(control is UiButton b&&b.Face!=null)b.Face.enabled=false;
            control.transition=Selectable.Transition.None;
            control.navigation=new Navigation{mode=Navigation.Mode.None};
        }
        public static void Choice(Selectable control,bool selected,bool tab=true,bool locked=false)
        {if(control is UiButton b)b.Configure(tab?UiButtonRole.Tab:UiButtonRole.Option,selected,locked);}
        public static void Skin(Selectable control,string skin)
        {
            if(!(control is UiButton b)){Button(control,skin=="button-primary"||skin=="primary");return;}
            bool selected=skin=="tab-current"||skin=="category-current"||skin=="active"||skin=="chip-on"||skin=="preset-current"||skin=="disclosure-open";
            bool tab=skin=="tab-current"||skin=="tab-idle"||skin=="category-current"||skin=="category-idle";
            b.Configure(tab?UiButtonRole.Tab:selected?UiButtonRole.Option:skin=="button-primary"||skin=="primary"?UiButtonRole.Primary:skin=="quiet"?UiButtonRole.Quiet:UiButtonRole.Action,selected);
        }
        public static UiButtonPalette ButtonPalette(UiButtonRole role,bool chosen,bool locked,bool available,bool hover,bool pressed)
        {
            if(role==UiButtonRole.GoogleSignIn)
            {
                var fill=StorageSurface.Hex(!available?"f2f2f2":pressed?"e8edf5":hover?"f5f8ff":"ffffff");
                var stroke=StorageSurface.Hex(GoogleStrokeHex);
                return new UiButtonPalette{top=fill,bottom=fill,edge=stroke,light=fill,accent=StorageSurface.Hex(GoogleTextHex)};
            }
            // top/bottom: face gradient, edge: hairline ring, light: inner top highlight, accent: ornaments,
            // glow: rising light from the lower edge (alpha carries its strength).
            string top=RaisedHex,bottom="15130f",edge="4a4033",light="6d604b",accent=GoldHex;float glow=0;Color glowColor=Gold;
            if(role==UiButtonRole.Quiet){top="17151200";bottom="17151200";edge="3a322880";light="00000000";}
            if(role==UiButtonRole.Tab){top="00000000";bottom="00000000";edge=BorderHex;light="00000000";}
            if(role==UiButtonRole.Option){top="1d1a16";bottom="13110e";edge=BorderHex;light="4c4336";}
            if(role==UiButtonRole.Item){top="00000000";bottom="00000000";edge=GoldHex;light="00000000";}
            if(role==UiButtonRole.Primary){top="a8763a";bottom="4f3217";edge=GoldHex;light=GoldBrightHex;accent=GoldBrightHex;glow=.18f;glowColor=GoldBright;}
            if(role==UiButtonRole.Destructive){top="84322a";bottom="3a110f";edge=DangerEdgeHex;light="e8a58e";accent="f0b9a2";glow=.14f;glowColor=Danger;}
            if(chosen&&role==UiButtonRole.Tab){top="d3af7224";bottom="d3af7200";edge=GoldHex;light=GoldBrightHex;accent=GoldBrightHex;glow=.2f;}
            else if(chosen){top="3d311f";bottom="221b11";edge=GoldHex;light="b8955c";accent=GoldBrightHex;glow=.14f;}
            if(locked){top="18161300";bottom="13110e";edge="332c24";light="00000000";accent=FaintHex;glow=0;}
            if(!available){top=role==UiButtonRole.Tab?"00000000":"17151200";bottom=role==UiButtonRole.Tab?"00000000":"121110";edge="2c2620";light="00000000";accent="5a5448";glow=0;}
            var p=new UiButtonPalette{top=StorageSurface.Hex(top),bottom=StorageSurface.Hex(bottom),edge=StorageSurface.Hex(edge),light=StorageSurface.Hex(light),accent=StorageSurface.Hex(accent)};
            p.glow=new Color(glowColor.r,glowColor.g,glowColor.b,glow);
            if(hover&&available)
            {
                bool ghost=role==UiButtonRole.Quiet||role==UiButtonRole.Tab&&!chosen;
                if(ghost){var lift=StorageSurface.Hex(HoverHex);lift.a=.6f;p.top=lift;p.bottom=new Color(lift.r,lift.g,lift.b,.25f);}
                else p.top=Color.Lerp(p.top,new Color(Gold.r,Gold.g,Gold.b,Mathf.Max(p.top.a,.2f)),.14f);
                p.edge=chosen||role==UiButtonRole.Primary?GoldBright:Gold;p.light=Color.Lerp(p.light,GoldBright,.35f);
                p.glow.a=Mathf.Max(p.glow.a,.1f)+.1f;
            }
            if(pressed&&available){p.top=Color.Lerp(p.bottom,Color.black,.25f);p.bottom=Color.Lerp(p.bottom,Gold,.08f);p.light.a*=.35f;p.glow.a*=.5f;}
            return p;
        }
        // Same semantic roles and input states, with a matte face and a single restrained edge.
        public static UiButtonPalette MinimalButtonPalette(UiButtonRole role,bool chosen,bool locked,bool available,bool hover,bool pressed)
        {
            if(role==UiButtonRole.Icon||role==UiButtonRole.Item||role==UiButtonRole.GoogleSignIn)
                return ButtonPalette(role,chosen,locked,available,hover,pressed);
            Color top=Raised,bottom=Panel,edge=Border,accent=Gold;
            if(role==UiButtonRole.Quiet){top=bottom=edge=Color.clear;}
            if(role==UiButtonRole.Tab){top=bottom=Color.clear;edge=Border;}
            if(role==UiButtonRole.Primary){top=Tint(SelectedHex);bottom=Panel;edge=Tint(GoldDeepHex);}
            if(role==UiButtonRole.Destructive){top=Tint(DangerFillHex,.6f);bottom=Panel;edge=Danger;accent=Danger;}
            if(chosen){top=bottom=Tint(SelectedHex);edge=Gold;}
            if(hover&&available&&!locked){top=Tint(HoverHex);bottom=Raised;edge=chosen?Gold:Tint(GoldDeepHex);}
            if(pressed&&available&&!locked){top=Panel;bottom=Background;}
            if(locked||!available){top=Tint(PanelHex,.5f);bottom=Tint(BackgroundHex,.5f);edge=Tint(BorderHex,.5f);accent=Faint;}
            return new UiButtonPalette{top=top,bottom=bottom,edge=edge,accent=accent,light=Color.clear,glow=Color.clear};
        }
    }
    public static class UiFonts
    {
        static Font body,display,google,casualDisplay,casualBody,casualBold,casualHeavy;
        public static Font CasualDisplay=>casualDisplay!=null?casualDisplay:casualDisplay=CasualFont("Jua-SkillSubset");
        public static Font CasualBody(bool bold=false)=>CasualWeight(bold?700:400);
        public static Font CasualWeight(int weight)=>weight>=900?(casualHeavy!=null?casualHeavy:casualHeavy=CasualFont("NotoSansKR-Skill900")):weight>=700?(casualBold!=null?casualBold:casualBold=CasualFont("NotoSansKR-Skill700")):(casualBody!=null?casualBody:casualBody=CasualFont("NotoSansKR-Skill400"));
        static Font CasualFont(string name)=>Resources.Load<Font>("Fonts/CasualSkill/"+name)??throw new InvalidOperationException("Missing bundled casual skill font: "+name);
        public static Font GoogleSignIn=>Loc.Language=="ko"?Body:(google!=null?google:google=Resources.Load<Font>("Authentication/Roboto-Medium"))??Body;
        public static Font Body=>body!=null?body:body=Application.platform==RuntimePlatform.WebGLPlayer?WebBody():
            Font.CreateDynamicFontFromOSFont(new[]{"Apple SD Gothic Neo","Malgun Gothic","Noto Sans CJK KR","Noto Sans CJK","Droid Sans Fallback","Arial"},32)??Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        public static Font Display=>display!=null?display:display=Application.platform==RuntimePlatform.WebGLPlayer?Body:
            Font.CreateDynamicFontFromOSFont(new[]{"Baskerville","Georgia","Times New Roman","Noto Serif"},100)??Body;
        static Font WebBody()
        {
            // Web builds preload this bundled font; browser players cannot read OS fonts.
            // Keeping it outside Resources avoids adding it to existing native builds.
            foreach(var font in Resources.FindObjectsOfTypeAll<Font>())
                if(font.name=="NanumGothic-Regular")return font;
            throw new InvalidOperationException("The Web player is missing its preloaded Korean font.");
        }
    }
    public static class UiLayout
    {
        public static RectTransform Rect(string name,Transform parent)
        {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);return r;}
        public static void Place(RectTransform r,float x,float y,float w,float h)
        {r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(Mathf.Max(1,w),Mathf.Max(1,h));}
        public static void Stretch(RectTransform r)
        {r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        public static Text Text(Transform parent,string name,string value,float x,float y,float w,float h,int size,Color color,Font font=null)
        {
            var r=Rect(name,parent);Place(r,x,y,w,h);var t=r.gameObject.AddComponent<Text>();t.font=font??UiFonts.Body;t.fontSize=size;t.text=value;t.color=color;
            t.supportRichText=true;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;
        }
        public static void Clear(Transform parent)
        {for(int i=parent.childCount-1;i>=0;i--){var child=parent.GetChild(i);child.gameObject.SetActive(false);if(Application.isPlaying)UnityEngine.Object.Destroy(child.gameObject);else UnityEngine.Object.DestroyImmediate(child.gameObject);}}
    }
    // Colours of the casual rune board mockup (HELLSCRIPT 룬 보드 · 캐주얼): ink, soft panels, yellow/green/coral buttons, rune colour families and the board beds.
    public static class RuneCasual
    {
        static Color H(string value)=>StorageSurface.Hex(value);
        public static Color Ink {get;}=H("2a3150");
        public static Color Text2 {get;}=H("4a5272");
        public static Color Faint {get;}=H("6b7393");
        public static Color Edge {get;}=H("8a93b5");
        public static Color Blue {get;}=H("1f6fb5");
        public static Color BlueBright {get;}=H("35a7ff");
        public static Color BlueDeep {get;}=H("1f7fd1");
        public static Color YellowTop {get;}=H("fff6c2");
        public static Color Yellow {get;}=H("ffd23f");
        public static Color Amber {get;}=H("ffc21a");
        public static Color Orange {get;}=H("ff9f1c");
        public static Color OrangeInk {get;}=H("e08a00");
        public static Color Gold {get;}=H("c48d00");
        public static Color EliteInk {get;}=H("f08a00");
        public static Color GreenTop {get;}=H("8ee07a");
        public static Color Green {get;}=H("4cbf55");
        public static Color GreenInk {get;}=H("2a8a36");
        public static Color GreenSoft {get;}=H("eefbe9");
        public static Color CoralTop {get;}=H("ff9a9d");
        public static Color Coral {get;}=H("ff5a5f");
        public static Color CoralInk {get;}=H("d93a40");
        public static Color CoralSoft {get;}=H("fff0f0");
        // Text inks of the battle HUD damage rows: a risky share (20%+) and a dangerous one (40%+).
        public static Color AmountWarn {get;}=H("b45f00");
        public static Color AmountDanger {get;}=H("c62f35");
        public static Color Cream {get;}=H("fff6e3");
        public static Color Paper {get;}=H("fffaf0");
        public static Color Mist {get;}=H("f1f4fa");
        public static Color SkySoft {get;}=H("e3f2ff");
        public static Color SkyTop {get;}=H("e8f4ff");
        public static Color SkyBottom {get;}=H("d4ebff");
        public static Color Track {get;}=H("e6e9f0");
        public static Color DisabledFill {get;}=H("eceff5");
        public static Color DisabledEdge {get;}=H("c3c9d6");
        public static Color DisabledInk {get;}=H("a3abc2");
        public static Color Gray {get;}=H("9aa1b0");
        public static Color BedOpen {get;}=H("fffaf0");
        public static Color BedOpenEdge {get;}=H("c9cfdc");
        public static Color BedClosed {get;}=H("e3e8f1");
        public static Color BedClosedEdge {get;}=H("c3c9d6");
        public static Color BedSealed {get;}=H("cdd3df");
        public static Color BedSealedEdge {get;}=H("b6bdcc");
        public static Color Alpha(Color c,float a){c.a=a;return c;}
        // fill, ink (rings, outlines), text, dark (block wall) per rune colour; index 0..4 = attack, magic, support, critical, skill; 5 = elite, 6 = neutral (root).
        public static readonly Color[] Fill={H("ff5a5f"),H("35a7ff"),H("f5d523"),H("b06cff"),H("ffb21e"),H("ffd23f"),H("fff6e3")};
        public static readonly Color[] RingInk={H("e5484d"),H("1f7fd1"),H("c2a200"),H("8a4fd8"),H("e08a00"),H("f08a00"),H("8a93b5")};
        public static readonly Color[] TextInk={H("d93a40"),H("1f6fb5"),H("8f7800"),H("7a3fc8"),H("b86a00"),H("c48d00"),H("6b7393")};
        public static readonly Color[] Dark={H("a8323a"),H("1f5c99"),H("8f7800"),H("5f34a0"),H("a86400"),H("b07a00"),H("b9ad8c")};
        public static int Slot(int type,bool elite=false)=>elite?5:type<0||type>4?6:type;
    }
    public static class UiTime
    {
        // Wall-clock work and simulation time pass their own duration; this formatter never reads a clock.
        public static string Duration(long seconds)
        {
            var span=TimeSpan.FromSeconds(Math.Max(0,seconds));
            return span.TotalDays>=1?Loc.F("{0}일 {1}시간 {2}분",(int)span.TotalDays,span.Hours,span.Minutes):span.TotalHours>=1?Loc.F("{0}시간 {1}분",(int)span.TotalHours,span.Minutes):Loc.F("{0}분 {1}초",span.Minutes,span.Seconds);
        }
    }
}
