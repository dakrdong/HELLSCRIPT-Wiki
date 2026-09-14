# Global HUD 08: effects, scrolling and alpha edges

Date: 2026-09-14

Show only effects currently applied to the player: semantic artwork, buff/debuff sign, remaining duration and stacks. Duration is not skill cooldown. Permanent effects show a permanent label; a single stack may omit its count. Use small +/− badges as well as color.

Build a read-only adapter from actual shields, item effects and player states already inspected by `GameUI.Effects.cs`. Enemy poison/marks/freeze and player-created ground fields are not automatically player debuffs. Each entry provides instance/definition/target IDs, polarity, icon ID, stacks, remaining/total duration, localized name/description and source. Where fields lack stable IDs, derive persistent adapter keys from definition and source.

## Width and interaction

Collapsed width is `4.5s + 4g`; expanded maximum is `8s + 7g`; content width is `Ns + max(0,N−1)g`. For 44-unit icons and 8-unit gaps, these are 230, 408 and 616 for twelve effects. At most four effects need no expand control. Five or more show 4.5 slots and a translucent + outside the mask. Expand rightward while holding the left edge, then place − at the new right edge. Never delete effects beyond eight. Insufficient safe width caps expansion and leaves remaining content scrollable.

Both states accept horizontal drag, trackpad and wheel while pointed at the strip. Disable vertical movement and elastic overscroll. Proposed expand artwork/hit sizes are 32/44 without covering neighboring effects. Expansion may animate over 0.18 seconds. Preserve the first visible effect ID and its offset, clamping only when content bounds require it.

## Real alpha fade

Fade artwork, frame, timer and stacks together from opaque to transparent over at most half a slot (22). Start: right only; middle: both; end: left only; no overflow: neither. For scroll `p` and maximum `m`, left width is `min(22,p)` and right is `min(22,m−p)`. For inward edge distance `d`, multiply original alpha by `t²(3−2t)` where `t=clamp01(d/width)`. Zero width means factor 1; multiply factors when both edges are active. Shrinking widths near the bounds avoids popping.

Keep `RectMask2D` for hard outside containment and share viewport coordinates across UI material variants. Existing `UnityEngine.UI.Text` needs a compatible font-material fade as well; fading sprites alone while digits clip sharply is incomplete. Controls and descriptions stay outside. `mask-edge-smooth` is an optional alpha lookup, never a dark overlay drawn over the scene.

## Descriptions and updates

In both collapsed and expanded states, briefly select an icon to inspect its name, full description, time, stacks and source. Descriptions must remain reachable with four or fewer effects when no expand button exists. This input only inspects information and never activates an effect or combat action. Distinguish taps from drags; drag release must not select an effect. Desktop hover can show the same description. The existing detailed combat-effects screen retains its pause policy, but expand/collapse/scroll alone never pauses combat.

Proposed initial ordering is debuffs first, then arrival order; do not continually sort by shrinking duration. During a drag, expired effects become inactive temporary placeholders rather than being falsely shown as active. On release, remove expired slots and insert pending effects, preserving the current anchor or nearest surviving neighbor.

Acceptance: 0/4/5/8/12/40 effects; start/middle/end; expand/collapse/rotate/expire; all effects reachable; real scene visibility and smooth text fading over bright/dark ground; no combat actions from inspection gestures. The supplied status set provides shared semantic glyphs, not bespoke artwork for every possible game effect.

한국어: [상태 효과](HELLSCRIPT_GlobalHUD_08_Status.md).
