# Skill window skin verification evidence

2026-10-14 · [Korean](README.md) · [Pilot report](../Casual_Skill_Window_Skin_20261014.en.md)

Retains actual Mac before/after captures, actual Chrome design captures and related regressions. Contact sheets are review thumbnails; PNG originals in `before/` and `after/` are unmodified.

- `PASSED.txt`, `RESUME_PASSED.txt`, `native-receipt.json`: final native player, separate-process resume, binary and source provenance.
- `screenshots.json`: 84 captures, 3 classes × KO/EN × 7 sizes × active/passive, with dimensions/SHA256. All raw captures remain under task `Builds/Validation/CasualSkillSkin/native`; 10 representative originals are in `after/`.
- `before-after.png`, `viewport-contact-sheet.png`, `before-provenance.json`: actual comparison and reused verified before captures with identical baseline Assets/Packages/ProjectSettings trees.
- `claude-*.jpg`, `design-decisions.json`: actual tab/passive/scroll/refund/reinvestment observations, user-approved body 400 and invalid transparent background CSS difference.
- `editmode-provenance.json`, two XML reports: 64 latest affected tests plus 38 unaffected results reused from the preceding run, 102 distinct related tests passed. The intermediate disabled-shield XML is retained too.
- `final-*.log`, `font-validation.json`: 41 generated goldens verified, 55,803 checks each in JS/C#, looks/fonts/UI static checks. This is not a full project EditMode run.
- `raw-layout-differences.json`, `final-golden-diff.json`, `geometry-diff.json`: 3,600 raw layout/scenario differences caused by new color/stroke/radius appearance. **Literal raw diff=0 / styles-only is not met.** Semantic viewmodels/interactions/rule files are byte identical. Raw appearance differences are accepted under the approved color/stroke/corner criterion; child text/decorative dimensions are separately reported in `acceptance-review.json`.
- `native-attempts.json`, `failures/`: six corrected attempts. Earlier binaries were not copied; only the final player is retained.
- `native-binary-manifest.json`: SHA256/bytes for 323 app files. The app itself is not tracked.
- `final-scope-isolation.json`, `restored-editor-side-effects-final.json`: zero protected-path differences, preserved original dirty art-document hashes, restored Editor side effects in this checkout only.
- `artifact-lifecycle.json`: measured task/cache/player/raw-data sizes, retention and review date. This is not exclusive APFS usage or reclamation. Unmerged source/review dependencies are preserved; no permanent deletion was executed.

Use the existing CLI/builder and Unity 6000.6.0f1. Exact build/test/runtime recipes are in the lifecycle manifest. Native runs require `-force-sound-off` and a disposable `-hellscriptSavePath` **directory**. Resume uses that same disposable save/output with `-hellscriptSkillTreeResume`. No web build, push, other UI expansion or real-user save test is included.

Remaining differences: larger live-canvas text, Chrome-invalid panel background CSS, some uGUI corner/fade/glyph/antialias boundaries. A fresh set of 24 locked casual browser references was not generated. CPU/GC is unmeasured; devices/accessibility/WebGL and full post-integration verification remain outside this local pilot.
