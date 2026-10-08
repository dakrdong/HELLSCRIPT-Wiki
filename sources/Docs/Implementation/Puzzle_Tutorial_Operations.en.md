# Operating and maintenance puzzles — stage 7

Written 2026-10-07. Connects L9–L13 and noncombat P1/P2 to actual loot, equipment, chest, cleanup, enhancement and repeat-hunt owners. This is not final tutorial acceptance. L4 ownership approval and stages 8–9 remain. No new images, packages or project copies are introduced.

## Transactions and outcomes

- L9 uses twelve real drops and a three-slot trial scope without changing the owned bag capacity or original items. Collect three rare items and remain within 8m of the start at 20 seconds. The distant legendary item is at 14.2m; its real poison pool activates after seven seconds. Collect-all misses required rares; broad recovery misses the return zone. Losses are not forced deaths.
- L10 opens the actual CH03 chest from 50% HP and a real 20-second potion cooldown. Well Prepared checks 85% HP and 120 seconds remaining. The 12-second trial has four N01 enemies, HP 95/95/90 by class, a real initial direct strike and a 300-second repeat cooldown. Regular rift CH03 composition is unchanged. The safe answer heals then suppresses the echoes; early opening fails suppression and skipping misses the chest goal.
- L11 uses three actual normal/magic/enhanced trial items and a legendary drop. Shared `Economy.AddItem` and `EdictCleanupPolicy` receive only trial IDs as their replacement/cleanup scope. Ordinary value replacement now respects the same investment protection owner. The answer sells one magic item for 80 gold, keeps the legendary and investment and frees one slot. Return/ignore/no-cleanup/no-investment-protection miss actual objectives. The portal tick guard now settles operating trial results while retaining normal rift portal behavior.
- L12 drops a weaker copy of the current main weapon, an off-class weapon and a same-family +6 enhancement upgrade. Actual automatic equipment and an actual kill are required. N07/E06 arrives at three seconds with HP 2400/2450/1900. Equipment-off and weapon-off leave the enemy alive at the 45-second limit. Failed loan equipment is removed and only originals displaced by those loans are restored through existing equipment/storage owners. The successful upgrade remains as a first-clear equipment reward. Hero XP and the full kit are never reset.
- L13 reuses `RepeatHunt.Start/Complete`, stop conditions and `RiftEarnings.GrantGold` only for its explicitly scoped real child battles. Each battle has a fresh ID/seed; completed failure health and outcomes are preserved. Actual completion awards 10/100/0 for stages 1/2/3. The third stage exceeds its 15-second combat limit; LOWER returns to stage two. Within the 90-second goal, the answer earns 210 through `1,2,3 failed,2`. Once/same/failure-hold miss the target. Active battle/session JSON is captured at saving, rather than copying whole state every tick. Restoration verifies actual counters, rewards, times and individual battle results.

The result transaction removes L9/L11 trial loot and L11/L13 trial proceeds, preserving original ownership and gold and awarding first-clear XP 220 once. Replays use the existing independent sandbox. Reviews distinguish operating losses, deaths and duration failures. Normal rift capacity, chests, portal handling and training repeat rejection remain; only explicitly scoped child trial battles qualify.

## P1/P2 and shared UI

P1 precedes L10; P2 precedes L12. Start from the hub opens the generated `PuzzleMaintenance` shared shell with `EquipmentComparisonView`, `ItemComparison.Preview`, `UiFonts`, `UiTheme` and keyed `StoreViewBinding`. Close the hub before maintenance and return to the same progress hub on completion/back. Existing hub lifecycle hides the world. The account is owned live state; comparison candidates are read-only copies.

P1 grants catalog B42/AF03 plate armor and B39/AF04 resistance robes with valid maximum affix rolls. Shared `HeroStats.Resistance` and `StatCatalog.Mitigation` require at least 10% reductions to both fire and poison at the hero's level. Real resistance equipment passes; physical armor fails. P2 grants 110 trial gold and executes the existing +1 weapon/armor quote transaction. Actual `HeroStats.damage` must increase. General enhancement unlocks remain closed; only the prepared trial's two item IDs, one upgrade and budget are admitted (the forge's gear enhancement opens after level 12 is cleared, see [Tutorial forge](Tutorial_Forge_20261008.en.md)). A retry reverses only that upgrade and spending while its saved item fingerprint still matches. Support/refund cannot be duplicated and refunded attempts do not farm daily enhancement progress. Four hints follow actual wrong attempts.

## Checks and remaining acceptance

338 focused checks passed. Reuse 269 successful shared-owner/localization/save-binding cases; after separating fresh construction from saved-state validation, rerun only the failing 68 operating/maintenance cases and pass all. One additional repeat-trial replay check preserves the original account. Retain earlier failed reports. Coverage includes 15 bag choices, nine auto-equipment choices and actual equipped-loan rollback, twelve repeat choices plus active-battle restore, and twelve maintenance transactions. The L8 fixtures come from actual first-eight clears on synthetic new accounts, never user saves. [Matrix and fixture hashes](Evidence/Puzzle_Operations_Matrix.json) record reproducible inputs.

Shared UI ownership, eleven UI contract checks and the 14-binding/165-source refresh inventory passed. Native macOS input, KO/EN, five resolutions, saving failures and performance acceptance remain for final stage 9. Full Edit Mode/native smoke has not run; physical mobile is untested. This PR remains Draft pending native acceptance. No public wiki deployment occurs; publishing belongs to the main merge owner. XML/logs/tuning failures/scratch are retained in task-owned Artifacts and its lifecycle manifest for review on 2026-10-13; permanent deletion is not approved.

| Level | Class | Choice | Result | Actual record |
|---|---|---|---|---|
| L12 | Warrior | answer | Cleared | hp=505.0 time=38.05 remaining=0.0 stored=0 |
| L12 | Warrior | off | Failed | hp=505.0 time=45.05 remaining=392.7 stored=0 |
| L12 | Warrior | weapon-off | Failed | hp=505.0 time=45.05 remaining=392.7 stored=0 |
| L12 | Ranger | answer | Cleared | hp=410.0 time=37.20 remaining=0.0 stored=0 |
| L12 | Ranger | off | Failed | hp=410.0 time=45.05 remaining=282.3 stored=0 |
| L12 | Ranger | weapon-off | Failed | hp=410.0 time=45.05 remaining=282.3 stored=0 |
| L12 | Mage | answer | Cleared | hp=365.0 time=41.70 remaining=0.0 stored=0 |
| L12 | Mage | off | Failed | hp=365.0 time=45.05 remaining=399.8 stored=0 |
| L12 | Mage | weapon-off | Failed | hp=365.0 time=45.05 remaining=399.8 stored=0 |
| L13 | Warrior | climb | Cleared | elapsed=47.85 gold=210 rounds=4 stages=1,2,3,2 |
| L13 | Warrior | same | Failed | elapsed=90.00 gold=80 rounds=8 stages=1,1,1,1,1,1,1,1 |
| L13 | Warrior | once | Failed | elapsed=10.90 gold=10 rounds=1 stages=1 |
| L13 | Warrior | hold | Failed | elapsed=67.00 gold=110 rounds=5 stages=1,2,3,3,3 |
| L13 | Ranger | climb | Cleared | elapsed=55.90 gold=210 rounds=4 stages=1,2,3,2 |
| L13 | Ranger | same | Failed | elapsed=90.00 gold=60 rounds=6 stages=1,1,1,1,1,1 |
| L13 | Ranger | once | Failed | elapsed=13.95 gold=10 rounds=1 stages=1 |
| L13 | Ranger | hold | Failed | elapsed=72.55 gold=110 rounds=5 stages=1,2,3,3,3 |
| L13 | Mage | climb | Cleared | elapsed=54.45 gold=210 rounds=4 stages=1,2,3,2 |
| L13 | Mage | same | Failed | elapsed=90.00 gold=60 rounds=6 stages=1,1,1,1,1,1 |
| L13 | Mage | once | Failed | elapsed=13.10 gold=10 rounds=1 stages=1 |
| L13 | Mage | hold | Failed | elapsed=71.40 gold=110 rounds=5 stages=1,2,3,3,3 |
| L11 | Warrior | answer | Cleared | time=8.00 sale=80 slots=1 invested=True |
| L11 | Warrior | return | Failed | time=4.05 sale=0 slots=0 invested=True |
| L11 | Warrior | leave | Failed | time=8.00 sale=120 slots=2 invested=True |
| L11 | Warrior | keep | Failed | time=8.00 sale=0 slots=0 invested=True |
| L11 | Warrior | unprotected | Failed | time=8.00 sale=120 slots=2 invested=False |
| L11 | Ranger | answer | Cleared | time=8.00 sale=80 slots=1 invested=True |
| L11 | Ranger | return | Failed | time=4.05 sale=0 slots=0 invested=True |
| L11 | Ranger | leave | Failed | time=8.00 sale=120 slots=2 invested=True |
| L11 | Ranger | keep | Failed | time=8.00 sale=0 slots=0 invested=True |
| L11 | Ranger | unprotected | Failed | time=8.00 sale=120 slots=2 invested=False |
| L11 | Mage | answer | Cleared | time=8.00 sale=80 slots=1 invested=True |
| L11 | Mage | return | Failed | time=4.05 sale=0 slots=0 invested=True |
| L11 | Mage | leave | Failed | time=8.00 sale=120 slots=2 invested=True |
| L11 | Mage | keep | Failed | time=8.00 sale=0 slots=0 invested=True |
| L11 | Mage | unprotected | Failed | time=8.00 sale=120 slots=2 invested=False |
| L10 | Warrior | safe | Cleared | hp=275.9/505.0 time=25.35 event=Succeeded started=20.30 |
| L10 | Warrior | skip | Failed | hp=432.0/505.0 time=160.01 event=Cancelled started=0.00 |
| L10 | Warrior | after | Failed | hp=174.4/505.0 time=12.30 event=Failed started=0.30 |
| L10 | Ranger | safe | Cleared | hp=212.8/410.0 time=29.20 event=Succeeded started=20.25 |
| L10 | Ranger | skip | Failed | hp=350.7/410.0 time=160.01 event=Cancelled started=0.00 |
| L10 | Ranger | after | Failed | hp=136.0/410.0 time=12.25 event=Failed started=0.25 |
| L10 | Mage | safe | Cleared | hp=194.4/365.0 time=24.90 event=Succeeded started=20.30 |
| L10 | Mage | skip | Failed | hp=312.2/365.0 time=160.01 event=Cancelled started=0.00 |
| L10 | Mage | after | Failed | hp=64.6/365.0 time=12.30 event=Failed started=0.30 |
| L9 | Warrior | answer | Cleared | hp=470.0 time=20.05 loot=3 position=(4.50, 0.88) |
| L9 | Warrior | all | Failed | hp=470.0 time=20.05 loot=3 position=(4.49, 0.85) |
| L9 | Warrior | far | Failed | hp=470.0 time=20.05 loot=3 position=(11.13, 0.28) |
| L9 | Ranger | answer | Cleared | hp=382.0 time=20.05 loot=3 position=(4.52, 0.89) |
| L9 | Ranger | all | Failed | hp=382.0 time=20.05 loot=3 position=(4.50, 0.85) |
| L9 | Ranger | far | Failed | hp=382.0 time=20.05 loot=3 position=(11.13, 0.28) |
| L9 | Mage | answer | Cleared | hp=340.0 time=20.05 loot=3 position=(4.50, 0.88) |
| L9 | Mage | all | Failed | hp=340.0 time=20.05 loot=3 position=(4.49, 0.85) |
| L9 | Mage | far | Failed | hp=340.0 time=20.05 loot=3 position=(11.13, 0.28) |
