# NPC fixed greeting playback preparation

Base: `e1c1434a843a552c2c0d72a1100234ac9045c998`. Branch: `codex/npc-greetings-20261024`. [한국어](Npc_Greetings_20261024.md)

This prepares optional fixed-clip playback. Zero NPC voice assets are supplied. Mother tongues, lines, voice selection and paid generation await a separate decision; nationality motifs do not establish mother tongues. No empty audio assets or existing SFX are substituted.

After the existing reach/page/common-window guards accept a talk and a ContentWindowView is opened, GameUI.ShowNpcDialogue attempts the greeting once. Regular dialogue, Anton gossip and the seller gift story share this post-open path. Movement, quests, rewards, services and TalkingNpcId semantics are preserved.

One AudioClip resource at `Audio/NpcGreetings/<npc-id>` is optional for each of the 12 existing NpcProfiles.All identities: mark-kus, jake-bokun, chador-samaf, anton-jindark, injel-mir, pyonya-nermwen, jean-jorin, darc-alvi, jacques-chei, turk-garbi, guzel-pan, mishu-karu. UI language does not select a different clip. Dialogue-only speakers, player heroes and arbitrary IDs are excluded.

GameAudio resolves a clip lazily on accepted talk, and caches both clips and null results for its lifetime. Missing resources remain silent without repeated loads or warnings. Build resources are immutable; adopting new clips requires a new build/session. No clip state is saved to an account.

At most one NPC greeting is active. A conversation cannot replay after completion, mute or interruption; reopening a new conversation reuses the fixed cached clip. Playback is 2D at fixed pitch and follows the existing EffectsLevel (master multiplied by effects, including mute). Volume changes apply immediately; zero/mute, power saving, application pause and audio disable/destruction stop the greeting.

The AudioSource is attached to the conversation GameObject itself: repaint replaces UI children while preserving the source, and closing/deactivating/destroying the window stops its playback. The existing volume loop checks page/account/combat validity only while a greeting source exists. The existing page-close path also clears the greeting. Greeting cleanup does not stop other effects or scheduled cues.

No account save keys, economy values, combat randomness, runtime network calls or generation are added. UI layout, localization, NPC profiles, original audio and serialized assets are unchanged.

Focused Unity handoff: Hellscript.Tests.NpcGreetingAudioTests, plus existing SoundBankTests and NpcDialogueTests. New tests cover optional silence, clip reuse, duplicate suppression, repaint/close/destruction, context invalidation, volume/mute/pause/power/disable, unrelated effects and audio-owner teardown. Synthetic in-memory silent clips are test fixtures only.

Unity tests, listening, UI interaction, builds and CLI review were not run by this agent. Root still needs isolated development validation of all three accepted-talk branches, close/service transitions, page/account changes, KO/EN/rotation and eventual real audio quality. Silent preparation is not completed voice content.

Static checks passed: 15 shared-UI Python regressions, UI Refresh (14 bindings/245 UI files), and whitespace. The 16 new Unity test cases remain unexecuted. Receipts are retained under Builds/Validation/NpcGreetings20261024/; these do not establish C# compilation, runtime UI or listening acceptance.
