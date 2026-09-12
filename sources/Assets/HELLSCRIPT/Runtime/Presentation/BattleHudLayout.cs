using UnityEngine;

namespace Hellscript
{
    // Top-left logical coordinates. This plan only describes presentation space.
    public sealed class BattleHudLayout
    {
        public readonly bool wide, compact, collapsedSkills;
        public readonly Rect header, status, actions, minimap, footer, world;
        public BattleHudLayout(float width, float height, bool boss)
        {
            float w = Mathf.Max(1, width), h = Mathf.Max(1, height);
            wide = w >= 960 && h >= 640; compact = !wide && h < 600;
            collapsedSkills = compact || !wide && w < 600 && h < 820;
            float head = wide ? 72 : compact ? 48 : 92, foot = wide ? 64 : compact ? 48 : 70;
            header = new Rect(0, 0, w, head); footer = new Rect(0, h - foot, w, foot);
            if (wide)
            {
                status = new Rect(12, head + 8, w - 336, 112 + (boss ? 82 : 0));
                actions = new Rect(w - 312, head + 8, 300, 286);
                minimap = new Rect(w - 312, head + 306, 300, Mathf.Min(280, h - head - foot - 318));
                world = new Rect(12, status.yMax + 8, status.width, Mathf.Max(1, footer.y - status.yMax - 20));
            }
            else
            {
                float statusHeight = compact ? 76 + (boss ? 48 : 0) : 160 + (boss ? 76 : 0);
                float actionHeight = collapsedSkills ? 44 : w >= 600 ? 114 : 170;
                status = new Rect(8, head + 4, Mathf.Max(1, w - 16), statusHeight);
                actions = new Rect(8, footer.y - actionHeight - 4, Mathf.Max(1, w - 16), actionHeight);
                minimap = new Rect();
                world = new Rect(8, status.yMax + 4, Mathf.Max(1, w - 16), Mathf.Max(1, actions.y - status.yMax - 8));
            }
        }
        public static float CameraHalfHeight(float aspect, float logicalHeight)
        { return Mathf.Max(Mathf.Clamp(logicalHeight / 60f, 5, 10), 6 / Mathf.Max(.1f, aspect)); }
    }
}
