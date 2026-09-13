# Local playable completion and gem services

Date: 2026-09-13 · [한국어](Playable_Completion.md)

The delivery target is a game the owner can play. The owner will tune details, farming and balance during play. This phase implements major features and checks progression and persistence without balance measurements or large repeated experiments. This current record supersedes older incomplete labels in design documents.

## Connected gameplay

The build retains three classes and recommended builds; hunt-edict editing, saving, sharing and five slots; generated rifts, objectives, chests, shrines and bosses; loot and per-character first-clear rewards; affixes, legendary/set gear, enhancement, rerolling and masterworking; rune growth; repeat hunts; combat records; and saved-rift recovery. Temporary audio and primitive presentation for all 18 skills are covered in the [preceding record](Audio_Skill_Presentation.en.md).

This phase connects gem acquisition, shared storage, socket creation, installation, replacement, removal and fusion. The existing seven gem types, six tiers and slot-dependent effects remain unchanged. Socketed equipment remains protected from sale, dismantling and automatic cleanup; effects already apply to equipment comparisons and combat stats.

## Gems and sweeps

- Ordinary and elite enemies use the existing gem drop chances. Bosses drop two gems. Uncollected gems remain on the floor; training awards no real resources.
- All three heroes share gem storage. **50 slots and stacks of 999 are provisional, tunable local-play defaults**, not an owner-confirmed storage product policy.
- Rare or better weapons, helmets, chest armor, amulets and rings support one socket. Creation costs 2,000 gold per item level and 30 common materials. Empty-socket installation is free; replacement/removal costs 500 gold per item level and returns the old gem to storage.
- Three identical gems and the existing fusion cost create one gem of the next tier. Maximum-tier gems cannot fuse. Space freed by consuming ingredients counts toward capacity; insufficient space cancels the whole change.
- Sweeps retain their unlock and three-per-day rules. Three equipment rewards, two gems, gold, materials and the daily attempt are saved together. Capacity or save failure leaves them unchanged.

Open Gem storage from the sanctuary or inventory list, or Manage sockets and gems from an item. The reroller NPC opens the same inventory screen. Socket changes wait until the current rift ends; portal storage allows fusion without changing equipped combat stats.

Full gem storage preserves the rift and remaining loot. Fuse to make room and resume, or explicitly confirm Finish without remaining loot to reach the result. Already collected rewards remain owned.

## Hunt-edict corrections

Previously exposed resource preferences now control actual floor pickup and pursuit. Gold, materials, gems and cores honor passing collection, after-combat pursuit, priority pursuit and supported ignore choices, along with distance, sight, dangerous paths and end-of-rift collection. The minimum gold amount limits detours; small piles encountered along the route can still be collected.

The global action order previously considered only equipment grades when placing priority loot. It now includes resource preferences. Gem rolls use a separate persisted random stream and do not consume combat or equipment-reward randomness. Explicitly opened chest currency and first-clear bonuses retain direct settlement.

## Persistence and presentation

Gem services and sweeps validate a staged account and commit it only after saving. Repeated request IDs cannot duplicate spending or rewards. Save schema 4 prevents old clients from silently omitting gem storage; older saves receive an empty bag. Unsupported stored gem data stops loading while preserving the original file.

Small primitives distinguish floor resources. Gem acquisition has a sound cue and a result-screen storage link. New text supports Korean and English. Scrollable content and fixed return controls follow the [screen layout design](../Design/HELLSCRIPT_Screen_Layout_Detail.md).

## Evidence and scope

The preceding hunt-edict review passed 1,007 functional cases plus native editor/share/save/restart flows. This change passed 99 focused cases covering transactions, pickup, failed saves, retries, compatibility, sweeps and localization. These counts are not combined into a new full-suite claim.

Final native results and screenshots are recorded in the [validation summary](../../Artifacts/Validation/PlayableCompletion/validation-summary.json). A legal level-30 loadout in a stage-1 rift checks natural game flow; gem-service and full-storage cases use separate prepared fixtures. No difficulty, farming-rate or progression balance is assessed.

See the [play guide](Local_Play_Guide.en.md) for launch instructions and release work outside this local-play milestone.

Three processes of the final macOS build passed same-rift recovery, natural exploration/chests/boss/rewards, gem services, portal cleanup/leave, another rift, sweeps and the final restart. Nine screenshots, including Korean portrait and English landscape layouts, were inspected.

![Gem storage](../../Artifacts/Validation/PlayableCompletion/Native1/04-gem-storage-ko.png)

![Socket service](../../Artifacts/Validation/PlayableCompletion/Native1/05-socket-installed-en.png)
