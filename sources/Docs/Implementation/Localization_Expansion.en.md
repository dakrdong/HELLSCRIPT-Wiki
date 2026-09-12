# Language setting development record

Written 2026-09-11 · Korean and English switching, and the English text

English counterpart of [언어 설정 개발 기록](Localization_Expansion.md). Both are kept in step; the Korean document is written first.

## Scope

The settings window gains a language choice, and every Korean line already in the game gains an English counterpart. Two languages can be chosen today, Korean and English; a third is one more row in the option list plus one more table file. It is a device setting like the screen direction and the reading size, so it was added to the settings window built in [Screen settings and shared guide](Screen_Settings_Expansion.md) rather than to a new screen.

## Why the source string is the key

Every line in this project was written in Korean inside the source. Inventing a key such as `settings.title` for each line would mean coining more than two thousand names and would stop the code from showing the actual sentence. The Korean source string is therefore the key itself.

| Result | Detail |
|---|---|
| Readable code | The source keeps its Korean sentences. With no translation the original reaches the screen, so no screen is ever left blank or showing a key name. |
| Default language | Korean reads no table. It is a language without a table, so it composes its lines exactly as it did before. |
| A new language | Add a row to `LanguageOptions.All` and a `Resources/Localization/<code>.txt`. The game code never learns which languages exist. |

The [project guidance](../../CLAUDE.md) says not to write strings in code but to put them in the table as keys. This implementation does not follow that wording literally, because the same guidance also requires that a missing translation shows Korean rather than a key name or an empty string, and the source string as key guarantees that structurally. With abstract keys, a gap in the Korean table puts a key name on screen.

The choice has one weakness. Rewording a Korean line changes the key, so the translation written for the old wording disappears silently. The table is therefore split into two sections, and a test requires every key in the first one to still exist in the source. Rewording a line fails that test, which is what brings the translation along with it.

## Text reaches the screen in one place

Every label on every screen is created by `GameUI.Label`, so that is the single place where the chosen language is read. The hundreds of calls that pass a sentence in were left untouched. The GameObject keeps the untranslated text as its name: code that finds a button by name, and the runtime smoke screens, then behave the same in every language.

The 31 places that rewrite a label after its screen was drawn go through `Loc.T` as well. The preset name field is the exception: its contents are saved as typed, so the language on screen must not change what would be stored.

## Where lines are composed

A composed line such as `$"처치 {kills}"` cannot match a key, because the finished string is not one. Every composing site became a format call.

```
$"처치 {run.kills} · 게이지 {meter} / 100"   →   Loc.F("처치 {0} · 게이지 {1} / 100", run.kills, meter)
```

`Loc.F` translates the format string **and its string arguments.** A skill name or a rule reason arrives carrying Korean, and without that step one line reads `"Whirlwind · 준비"`. A translation that lost a placeholder composes with the source template instead, and if that fails too the source is returned, so drawing a screen never throws.

| Situation | Handling |
|---|---|
| A whole sentence passed to a call | `Label` translates it. The call sites did not change. |
| The 388 sites that interpolate or concatenate | Now call `Loc.F`. Korean arriving as an argument is translated with it. |
| A line composed once when its type loads | It would freeze whichever language was active then, so it is not composed there. Set piece names and their two set lines are joined when read; the loot entry labels keep the source text and are translated on display. |
| A name that is written to a save file | Composed in the source language with `Loc.Source`, so the stored bytes never depend on the language on screen, and the finished name is an entry so it is translated on display. |

## Choosing and storing

A choice applies at once and is stored on the device. The file is `hellscript-language-v1.json` alone; the account save, the screen direction file and the reading size file are never touched.

| Situation | Handling |
|---|---|
| Nothing stored | Starts in Korean and writes no file. |
| Damaged file, or an unknown language | Starts in Korean, keeps the original file and reports that it could not be read. A file over 1KB is treated the same way. |
| The table is missing or unreadable | The language still applies, and the message says that lines without a translation are shown in Korean. |
| The write fails | The chosen language stays on screen, and the message names both the failure and the language the next run will use. Pressing the same language again retries the save only. |

## Redrawing the page in front of the reader

Lines are written while a screen is drawn, so a new language has to redraw the screen that is already there. Each of the 32 drawing functions remembers how to run itself again. Choosing a language closes the settings window, calls that function, reopens the window on the same tab and scrolls the language row back into view. A name dialog that was open closes with the redraw, which the settings screen says.

## The translation table

`Assets/HELLSCRIPT/Resources/Localization/en.txt` holds one entry per line: the Korean source, a tab, the English sentence. A newline is written `\n` and a tab `\t`. A line starting with `#` is a note. There are 2,496 entries; 2,170 of them are the Korean strings in the source and the other 326 are keys the code composes at runtime. One comment line separates the two groups, and what decides the side is whether the source holds that string today. A pipe separated choice list is registered both whole and item by item, as are the loot entries built from a rarity name and the six recommended build names across thirty levels.

| Korean | English |
|---|---|
| 균열 / 잿빛 성소 / 성소 (in combat) | Rift / Ashen Sanctuary / Shrine |
| 사냥 칙령 / 행동 설계 / 규칙 | Hunt edict / Behavior design / Rule |
| 처치 게이지 / 제압 / 잔향 | Kill gauge / Stagger / Echo |
| 접두·접미 / 분해 / 강화 / 한 줄 재설정 | Prefix and suffix / Salvage / Enhance / Reroll one line |
| 전사·궁수·마법사 | Warrior, Ranger, Mage |

## Verification

EditMode: 1,183 passed, 0 failed, of which 32 are new. The macOS development player exits 0 for both the language smoke and the restart that follows it. The evidence is in `Artifacts/Validation/LanguageEvidence`: twelve screenshots and two PASS notes, where `00-restart-town-in-english.png` is the town after a restart, `01` and `04` are the same screen in Korean and in English, and `11` is the return to Korean. The full results table is in the Korean document; both records carry the same numbers.

## What is left

- The log lines inside a combat record are composed when they are written, so they keep the language of that moment. The class name and the outcome in the same record are stored in the source language and are translated on display.
- The font list is the existing CJK-first one. No English-only face or letter spacing was introduced.
- An English sentence is sometimes longer than its Korean original. The runtime smoke fails if any label is taller than its box each time it captures a screen, and the screens it visits include the rule editor and the hunt edict editor, the two that pack widgets into fixed bands.
- The translation is a developer translation and has not been reviewed by a native speaker.
- One Korean word sometimes serves two contexts and has to settle on one English word. `돌진` is both an enemy state and a target behavior type, so it reads `Charge` in both.
