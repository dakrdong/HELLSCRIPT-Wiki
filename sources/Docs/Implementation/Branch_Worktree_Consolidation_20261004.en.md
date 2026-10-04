# 2026-10-04 branch and worktree consolidation

[Korean](Branch_Worktree_Consolidation_20261004.md)

At the user's request, the uncommitted Hunt Edict progression and starter comparison tutorial were preserved in `93d59959`, fast-forwarded into the primary `main` and pushed. The remaining skill and remote branch commits were already in `main`; ancestry was verified before deletion.

| Removed branch | Last commit | Disposition |
| --- | --- | --- |
| codex/edict-progression | 93d59959 | Commit/preserve source, docs and evidence, integrate, delete locally |
| codex/distinct-class-skills | 7c6f2724 | Verify existing integration, delete locally |
| codex/attendance-collect | ab7985f2 | Verify existing integration, delete remotely |
| codex/rune-drop-stage15 | 527c29ce | Verify existing integration, delete remotely |
| codex/town-npc-buttons | cc1fddea | Verify existing integration, delete remotely |

Remote deletion used an atomic push and leases on the observed tips. Final local and GitHub queries show only `main`, with the primary project as the sole registered checkout. Eight existing stashes were retained without bulk application or deletion.

## Source and evidence retention

Test XML, native logs/screenshots, synthetic save fixtures, hashes, recipes and small editor settings were relocated under the primary project's `Artifacts/Validation/EdictProgression/`. Public evidence also lives in the [tutorial validation record](EdictProgressionEvidence20261004/validation.json). Original project assets/meta, settings, Git history, conversations, credentials and delivery outputs were protected.

The managed `edict-progression` worktree was archived through Codex's recoverable archive/restore lifecycle. Immediately beforehand, related Unity/player processes and working-directory references were absent. No other process was terminated. Only verified Unity-generated settings/material side effects were restored.

The worktree copies of four auxiliary regression-output folders (`CompactRift`, `EdictV2Codes`, `LiveOps`, `OrganicRift`) were omitted from separate retention. Complete result XML, test source and generation recipes remain; retention of those auxiliary outputs is not claimed.

## Retained output and measured space

One development player (584,283,049 bytes) reproducing attendance preceding town arrival is retained. It predates the popup sequencing repair and is not a new release. The tutorial task owns it, with review on 2026-10-11. Other task-local caches and build intermediates were disposed of with the worktree archive.

The same Data volume had 220,813,520,896 free bytes immediately before archival and 226,992,541,696 afterward, an observed increase of 6,179,020,800 bytes. APFS shared blocks and concurrent writes are included; this is neither summed directory size nor exclusive-block usage. The lifecycle manifest records exact paths, owners, approvals, retention reasons and measurements.

## Validation and publication

Runtime C# compilation of 711 source files and shared UI ownership/refresh checks passed. Existing EditMode results were reused for unchanged source. The town-popup conflict was repaired at the shared cinematic owner, but ended QA/game tests were not restarted at the user's direction. Repaired town arrival and remaining class/town/entry/manual-restock input checks remain explicitly incomplete in the [tutorial record](Hunt_Edict_Progression.en.md).

The public wiki publishes the complete read-only mirror generated and checked from consolidated `main`. Final build, Pages and logged-out in-app-browser results are retained separately in this consolidation's deployment evidence.
