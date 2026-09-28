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
            new FieldLook{id="graveyard",particles="grave-mist",sky=new Color(.3f,.38f,.44f),equator=new Color(.2f,.27f,.28f),ground=new Color(.09f,.11f,.11f),
                fog=new Color(.07f,.12f,.12f),fogStart=30,fogEnd=82,background=new Color(.02f,.035f,.04f),moon=new Color(.66f,.78f,1),moonIntensity=1.25f,moonPitch=48,moonYaw=-25,
                saturation=-20,contrast=12,exposure=.5f,filter=new Color(.94f,1,1),shadows=new Color(.43f,.5f,.51f),highlights=new Color(.6f,.56f,.5f),vignetteColor=new Color(0,.02f,.02f)},
            new FieldLook{id="fortress",particles="ash-embers",sky=new Color(.29f,.26f,.24f),equator=new Color(.26f,.19f,.14f),ground=new Color(.1f,.075f,.06f),
                fog=new Color(.13f,.085f,.055f),fogStart=28,fogEnd=78,background=new Color(.035f,.025f,.02f),moon=new Color(.6f,.64f,.74f),moonIntensity=.9f,moonPitch=50,moonYaw=-30,
                hero=new Color(1,.68f,.4f),heroIntensity=10,saturation=-12,contrast=14,exposure=.55f,filter=new Color(1,.95f,.88f),shadows=new Color(.48f,.47f,.47f),highlights=new Color(.64f,.52f,.4f),
                vignetteColor=new Color(.04f,.015f,0),bloom=1.15f,bloomTint=new Color(1,.8f,.62f)},
            new FieldLook{id="desert",particles="sand-drift",sky=new Color(.55f,.48f,.38f),equator=new Color(.45f,.36f,.25f),ground=new Color(.22f,.16f,.1f),
                fog=new Color(.42f,.32f,.2f),fogStart=32,fogEnd=96,background=new Color(.1f,.07f,.045f),moon=new Color(1,.8f,.55f),moonIntensity=1.6f,moonPitch=32,moonYaw=-60,
                hero=new Color(1,.78f,.55f),heroIntensity=6.5f,saturation=-6,contrast=10,exposure=.2f,filter=new Color(1,.96f,.88f),shadows=new Color(.47f,.47f,.5f),highlights=new Color(.62f,.54f,.43f),
                vignette=.28f,vignetteColor=new Color(.12f,.07f,.03f)},
            new FieldLook{id="cavern",particles="cave-spores",sky=new Color(.12f,.21f,.25f),equator=new Color(.09f,.16f,.17f),ground=new Color(.04f,.06f,.06f),
                fog=new Color(.03f,.08f,.09f),fogStart=24,fogEnd=66,background=new Color(.008f,.02f,.025f),moon=new Color(.45f,.65f,.75f),moonIntensity=.35f,moonPitch=60,moonYaw=-20,
                heroIntensity=11.5f,heroRange=10,saturation=-10,contrast=16,exposure=.8f,filter=new Color(.9f,1,1),shadows=new Color(.43f,.5f,.52f),highlights=new Color(.54f,.58f,.56f),
                vignette=.38f,vignetteColor=new Color(0,.03f,.04f),bloom=1.2f,bloomTint=new Color(.7f,1,.95f)},
            new FieldLook{id="grassland",particles="dusk-pollen",sky=new Color(.42f,.36f,.48f),equator=new Color(.36f,.3f,.3f),ground=new Color(.14f,.14f,.1f),
                fog=new Color(.22f,.16f,.26f),fogStart=30,fogEnd=90,background=new Color(.05f,.035f,.06f),moon=new Color(1,.62f,.5f),moonIntensity=1.3f,moonPitch=26,moonYaw=-70,
                heroIntensity=7,saturation=-8,contrast=10,exposure=.4f,filter=new Color(1,.95f,.97f),shadows=new Color(.48f,.45f,.54f),highlights=new Color(.63f,.51f,.45f),
                vignetteColor=new Color(.06f,.02f,.08f)},
            new FieldLook{id="highland",particles="snowfall",sky=new Color(.56f,.64f,.74f),equator=new Color(.44f,.5f,.58f),ground=new Color(.3f,.33f,.38f),
                fog=new Color(.46f,.54f,.64f),fogStart=30,fogEnd=92,background=new Color(.12f,.14f,.18f),moon=new Color(.86f,.92f,1),moonIntensity=1.5f,moonPitch=40,moonYaw=-35,
                hero=new Color(1,.76f,.55f),heroIntensity=6.5f,saturation=-18,contrast=12,exposure=.1f,filter=new Color(.94f,.98f,1),shadows=new Color(.45f,.48f,.54f),highlights=new Color(.55f,.54f,.53f),
                vignette=.26f,vignetteColor=new Color(.04f,.06f,.1f)},
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
