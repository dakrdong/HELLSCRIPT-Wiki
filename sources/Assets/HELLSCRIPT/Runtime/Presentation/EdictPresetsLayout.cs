using UnityEngine;

namespace Hellscript
{
    // Screen-local dimensions from the 2026-10-17 Hunt Edict specification, section c.
    // Each profile is expressed in its reference board pixels, before Canvas scaling.
    internal readonly struct EdictPresetsLayout
    {
        public readonly bool Pc,Landscape;
        public EdictPresetsLayout(bool pc,bool landscape){Pc=pc;Landscape=landscape;}
        float Pick(float pc,float wide,float tall)=>Pc?pc:Landscape?wide:tall;
        public float Padding=>Pick(20,10,12);
        public float Close=>Pick(46,36,42);
        public float Gap=>Pick(14,8,10);
        public float CardPadding=>Pick(18,10,14);
        public float BodyText=>Pick(15.5f,12.5f,14);
        public float SmallText=>Pick(13.5f,11.5f,12.5f);
        public float CardTitle=>Pick(22,15.5f,19);
        public float ChoiceTitle=>Pick(19,14,16.5f);
        public float Button=>Pick(56,36,50);
        public float Chip=>Pick(48,32,44);
        public float Dialog=>Pick(560,440,396);
        public float ActionMin=>Pick(150,104,104);
    }
}
