# Casual hero model integration — 2026-10-14

[한국어](Casual_Hero_Models_Integration_20261014.md)

Integrate the user-confirmed completed `claude/casual-hero-models` commit `616c88b24c0f7cac49cd7e12c7d9408b6f2886ce` into local main. Preserve the three READY class models (HW02 warrior, HW17 ranger, HW82 mage), nine LOD FBXs and 36 512×512 PNGs. No generation, rerigging or payment is performed. The six-slot save and gender-specific 2D art contracts remain intact; both genders share each class's 3D model as before.

READY rigs lack the procedural weapon sockets. Preserve the authored left hand for the bow and shield, and the right hand for the sword and staff. Keep the quiver parented to Chest while placing it at the recipe's backBone rest position behind the body, without inheriting Cape cloth motion. Existing WorldArtTests gains a regression exercising the real WorldView body construction and all five prop attachments.

The 144 asset files have no missing metadata, GUID collision or unresolved prefab reference. All nine FBXs match recipe hashes. The preserved provenance recipe_sha256 identifies the original recipe with the HWxx art key; the installed Hero_* recipe has a distinct hash after renaming.

Final validation and executed/unexecuted scopes are recorded under task-23 validation/cleanup-20261014 and its cleanup handoff receipt. Earlier UI proof and the preserved native app retain their original source identity and do not establish player validation of these new models.

## Integration result

After model merge `831f3424` and history-only merges for already integrated training/shop work, validation used main `16c75fa7938700fbd4ec419e5f7755876220287e`. Runtime, Editor and Tests compile with zero errors and the existing 88/28/35 warnings. UI contract 15 and refresh 16 checks pass. Muted EditMode on final main passes **13/13**: five new prop attachment cases, one existing model/socket case and seven altar-art cases, with no failure or skip (tests 5.625 s; whole process 75.013 s). Back up and restore the 465 automatic whitespace-only metas and IDE setting after the run. No full suite, native player or device run was performed for these new models. The originating owner's separate 86/86 proof retains source identity `616c88b2`.

Following the user's merge-first cleanup request, move 15 sparse working copies and one already integrated older HUD worktree into normal macOS Trash. All Git/stash/dirty/ignored evidence remains recoverable: logical 6,078,038,016 bytes. Preserve the linked HUD's 2,329,721-byte Git admin/index/log metadata before pruning its registration. Remove 12 unused local branches whose tips are main ancestors and the published-main-merged remote branches `claude/casual-bgm` and `claude/monsters-casual-prompts`. No permanent deletion, stash drop, object/reflog purge, main push or CI invocation.

Keep the hero/tutorial folders with active cwd processes, dirty original/living-title, field/monster work with uncertain owner completion, unfinished hero-rig and open PR130 material. Remote `codex/**` deletion remains pending because its telemetry Actions push filter can match under the no-CI constraint. Wiki check fails because the sparse environment lacks the pre-existing generated `Wiki/site/data.json`; no wiki build/publication is added. Trash movement is not reclaimed space. task-23 `validation/cleanup-20261014/final-handoff.json` records exact paths, SHAs, restoration and final disk measurement.

## Follow-up documentation and duplicate workspace cleanup

Merge the completed two documents and four captures from `8172639e` as `7272f126`. Preserve capture bytes and align the notes with the left-hand bow, six gender slots and shared class 3D models in main. Game source, assets and settings remain identical, retaining the 13/13 test inputs without another Unity, build or full-suite invocation.

After confirming PR128/main/wiki completion and idle ownership, move local `casual-field-kit` to normal Trash. Also move the unused local `wt-casual-art`, whose 192 branch-changed paths all match main. Preserve 9,373,985,022 logical bytes and 4,471,862 bytes of Git admin/index/log recovery metadata; prune only their registrations and two local branches. Cumulative cleanup is 18 folders, 14 local branches and two remote branches. PR130 and remote art-concept remain unchanged.

Keep the model/monster owner's workspaces while actual Unity verification and the owner session remain busy. Retired tutorial work is idle but its Claude/terminal cwd remains open, so use has not ended. living-title retains an unmerged TitleArtImporter/prompt and user changes; its owner's durable thread is unavailable, preventing completion/freeze confirmation. Do not resume canceled hero-rig generation. Preserve original dirty material, remote main and no-CI constraints. task-23 `validation/cleanup-20261014/followup/final-handoff.json` records exact recovery paths and remaining blockers.
