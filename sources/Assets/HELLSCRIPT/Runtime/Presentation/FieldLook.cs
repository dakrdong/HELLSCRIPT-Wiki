using UnityEngine;

namespace Hellscript
{
    // Mood preset per field: trilight ambient, linear fog, the key directional light, the hero's warm light pool
    // and the per-field colour tweaks applied to the runtime copy of the post profile. Colours are sRGB.
    // A fogged rift keeps a black camera background (unexplored floor is drawn black), so background shows
    // only in town and the legacy ring. particles names the ambient set the VFX layer builds for the field.
    public sealed class FieldLook
    {
        public string id,particles;
        public Color sky,equator,ground,fog,background,moon,hero=new Color(1,.73f,.48f);
        public float fogStart,fogEnd,moonIntensity,moonPitch,moonYaw,heroIntensity=9,heroRange=9;
        // saturation/contrast are URP percents (-100..100). exposure is EV and scales the scene before ACES. The RP assets
        // grade in LDR mode, so the profile's gamma offset runs after ACES, in the grading LUT: it lifts what ACES leaves
        // near black and brightens mid-tones too, so each exposure sits about 0.6 EV below the value that matched the
        // un-posted mid-tones through ACES alone. bloom scales the profile's own intensity. Point lights fall off with
        // 1/d², so the hero light (3 m up, 1.3 m behind) needs an intensity of several units to leave a visible pool.
        public float saturation,contrast,exposure,vignette=.3f,bloom=1;
        public Color filter=Color.white,shadows=new Color(.45f,.48f,.53f),highlights=new Color(.58f,.53f,.46f),vignetteColor=Color.black,bloomTint=Color.white;

        // Index = field id (PLAN §2): 0 graveyard, 1 fortress, 2 desert, 3 cavern, 4 grassland, 5 highland.
        public static readonly FieldLook[] Fields=
        {
            new FieldLook{id="graveyard",particles="sunlit-motes",sky=new Color(.75f,.88f,.97f),equator=new Color(.57f,.68f,.59f),ground=new Color(.36f,.42f,.34f),
                fog=new Color(.8f,.9f,.82f),fogStart=48,fogEnd=115,background=new Color(.75f,.88f,.97f),moon=new Color(1,.95f,.82f),moonIntensity=1.5f,moonPitch=50,moonYaw=-35,
                heroIntensity=2.5f,saturation=3,contrast=2,exposure=-.3f,shadows=new Color(.5f,.5f,.5f),highlights=new Color(.5f,.5f,.5f),vignette=.08f,bloom=.35f},
            new FieldLook{id="fortress",particles="courtyard-leaves",sky=new Color(.78f,.9f,.98f),equator=new Color(.72f,.68f,.56f),ground=new Color(.5f,.44f,.33f),
                fog=new Color(.87f,.88f,.79f),fogStart=48,fogEnd=115,background=new Color(.78f,.9f,.98f),moon=new Color(1,.94f,.8f),moonIntensity=1.45f,moonPitch=50,moonYaw=-35,
                heroIntensity=2.5f,saturation=3,contrast=2,exposure=-.4f,shadows=new Color(.5f,.5f,.5f),highlights=new Color(.5f,.5f,.5f),vignette=.08f,bloom=.35f},
            new FieldLook{id="desert",particles="sand-glints",sky=new Color(.75f,.9f,.97f),equator=new Color(.76f,.68f,.48f),ground=new Color(.52f,.42f,.25f),
                fog=new Color(.91f,.86f,.69f),fogStart=48,fogEnd=120,background=new Color(.75f,.9f,.97f),moon=new Color(1,.94f,.8f),moonIntensity=1.4f,moonPitch=50,moonYaw=-35,
                heroIntensity=2.5f,saturation=2,contrast=2,exposure=-.6f,shadows=new Color(.5f,.5f,.5f),highlights=new Color(.5f,.5f,.5f),vignette=.08f,bloom=.3f},
            new FieldLook{id="cavern",particles="crystal-motes",sky=new Color(.69f,.69f,.88f),equator=new Color(.64f,.57f,.78f),ground=new Color(.4f,.36f,.56f),
                fog=new Color(.78f,.75f,.92f),fogStart=44,fogEnd=110,background=new Color(.69f,.69f,.88f),moon=new Color(.93f,.91f,1),moonIntensity=1.3f,moonPitch=50,moonYaw=-35,
                heroIntensity=2.8f,saturation=3,contrast=2,exposure=-.2f,shadows=new Color(.5f,.5f,.5f),highlights=new Color(.5f,.5f,.5f),vignette=.1f,bloom=.35f},
            new FieldLook{id="grassland",particles="meadow-pollen",sky=new Color(.75f,.9f,.98f),equator=new Color(.6f,.72f,.55f),ground=new Color(.4f,.5f,.32f),
                fog=new Color(.76f,.9f,.85f),fogStart=48,fogEnd=115,background=new Color(.75f,.9f,.98f),moon=new Color(1,.95f,.79f),moonIntensity=1.6f,moonPitch=50,moonYaw=-35,
                heroIntensity=2.5f,saturation=4,contrast=2,exposure=-.35f,shadows=new Color(.5f,.5f,.5f),highlights=new Color(.5f,.5f,.5f),
                vignette=.08f,vignetteColor=new Color(.22f,.3f,.23f),bloom=.35f},
            new FieldLook{id="highland",particles="gentle-snow",sky=new Color(.8f,.92f,.99f),equator=new Color(.65f,.77f,.87f),ground=new Color(.44f,.5f,.6f),
                fog=new Color(.85f,.92f,.98f),fogStart=48,fogEnd=115,background=new Color(.8f,.92f,.99f),moon=new Color(1,.97f,.9f),moonIntensity=1.25f,moonPitch=50,moonYaw=-35,
                heroIntensity=2.5f,saturation=1,contrast=2,exposure=-.75f,shadows=new Color(.5f,.5f,.5f),highlights=new Color(.5f,.5f,.5f),vignette=.06f,bloom=.3f},
        };
        public static readonly FieldLook Town=new FieldLook{id="town",particles="village-motes",sky=new Color(.44f,.48f,.52f),equator=new Color(.4f,.43f,.39f),ground=new Color(.2f,.19f,.16f),
            fog=new Color(.09f,.13f,.12f),fogStart=38,fogEnd=105,background=new Color(.022f,.033f,.05f),moon=new Color(.68f,.78f,1),moonIntensity=1.3f,moonPitch=45,moonYaw=-25,
            heroIntensity=7,saturation=-8,contrast=8,exposure=.4f,shadows=new Color(.46f,.48f,.52f),highlights=new Color(.58f,.53f,.46f),vignette=.24f};
        // The legacy ring (training, developer training, boss arena) by theme: the pre-overhaul night look, graveyard or fortress tinted.
        public static readonly FieldLook[] Legacy=
        {
            new FieldLook{id="legacy-graveyard",particles="grave-mist",sky=new Color(.34f,.39f,.48f),equator=new Color(.29f,.34f,.43f),ground=new Color(.12f,.13f,.16f),
                fog=new Color(.035f,.05f,.075f),fogStart=35,fogEnd=95,background=new Color(.022f,.033f,.05f),moon=new Color(.68f,.78f,1),moonIntensity=1.3f,moonPitch=45,moonYaw=-25,
                saturation=-12,contrast=10,exposure=.5f},
            new FieldLook{id="legacy-fortress",particles="ash-embers",sky=new Color(.33f,.32f,.36f),equator=new Color(.3f,.27f,.26f),ground=new Color(.12f,.1f,.09f),
                fog=new Color(.07f,.05f,.045f),fogStart=35,fogEnd=95,background=new Color(.03f,.025f,.025f),moon=new Color(.66f,.72f,.92f),moonIntensity=1.15f,moonPitch=48,moonYaw=-28,
                hero=new Color(1,.68f,.4f),saturation=-12,contrast=12,exposure=.5f,highlights=new Color(.64f,.52f,.4f),bloomTint=new Color(1,.8f,.62f)},
        };
        public static FieldLook Field(int field)=>field>=0&&field<Fields.Length?Fields[field]:Fields[0];
        public static FieldLook LegacyTheme(int theme)=>theme>=0&&theme<Legacy.Length?Legacy[theme]:Legacy[0];
    }
}
