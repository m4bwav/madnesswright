# Session log

Append-only, newest first.

## 2026-10-21 · session 6: scenes in every level (Playground Director 0.5.0)

- Request: the requester wants F11 scenes to save and load in the story levels and arenas, not only the Playground; the owner approved a deeper rework.
- Same build (25615196). Save backup before work. Player.log from the last play had no F9 or F11 use and no scene folder: both still unconfirmed.
- Fresh decompile; two research agents in parallel (level and room identity, stage loading, career save, respawn; pause, Clear All, scripted characters, arena waves, skip flags). Findings: notes/2026-10-21-scenes-in-levels-research.md.
- Decision (decisions/2026-10-21-scenes-in-every-level.md): A and B everywhere, C only in the Playground (starting a story stage writes the career save), no D. Loading outside the Playground matches existing characters to saved ones and puts them back instead of Clear All plus respawn; respawns come before removals so no faction-dead trigger fires.
- Built 0.5.0: `SceneData.cs` split out (version 2 files with stage key, rooms, serials, your place), `SceneFeature.cs` gains a pending load (wait for a Playground stage to load, or 0.75 s for a room's own characters). Clean build, deployed; playtest pending.
- Gotcha: `Game_Manager.ClearPauseGame()` on every scene load wipes the mod's pause source; the window now closes on `SceneManager.sceneLoaded`.

## 2026-10-21 · session 5: Playground Director (swap mod 0.4.0)

- Release: the owner reported "everything worked"; Player.log confirmed F1 to F4, F10 and F5 to F8, no exception, but no F9 or F11 use (released at the owner's request; noted). New preview (Z-Image Turbo, seed 4102, four candidates across both ComfyUI hosts). Workshop update of 3822329165 through session 4's uploader build (same Program.cs): `result OK`, visibility 0, title changed, 28 subscribers. Tagged v0.1.5 on the owner's go-ahead.

- Request: the swap mod's player sent eight new keys. Decided to grow the existing mod (one Workshop item he already has, F5 to F8 kept) under the name Playground Director, split into one `Feature` component per key group so one failing cannot stop the rest.
- Fresh decompile (same build 25615196). Three research agents (freeze/kill/AI, idle animations and movement, Playground spawn and saving) ran in parallel on the scratch decompile; findings in notes/2026-10-21-director-mod-research.md.
- Keys: a grep found seven of the requested keys in game code; reading the callers showed all were debug-only, `-devtools`-only or profile-window-only (L-003). Only CapsLock (autowalk) is live in play.
- Built F1 (RemoveFromGame on non-playable characters), F2 (controller lock, waypoints cleared, SetFrozen, animator speed 0), F3 (IMGUI squares, AssignWalkToPosition, Harmony prefix skipping RunControls_Attacks while placing), F4 (PlayAsEmote list), F9 (FixedInPlace copy), F10 (SetAlertStatus cycle, SetKillTarget), F11 (JsonUtility scenes outside Mods/, own pause source). Builds clean, deployed; playtest pending.
- New `decomp.py` saved reading whole decompiled files: `find` returned each signature in one line for about 60 members.
- Gotchas met:
  - The research agent suggested saving under `Mods/<mod>/`: the loader would read those JSON files as cards (L-004).
  - A Bash heredoc with a Windows path in doubled backslashes is refused by the hook; write paths with forward slashes.
  - `find` capped at `--max 3` hid `Char_Data.CreateCharacter` behind three `Char_Background` matches; raise `--max` before concluding a member is missing.

## 2026-10-18 · session 4: swap mod 0.3.0, example in the repo, Workshop prep

- 0.2.0's F5 failed on characters mid-fight. Cause (fresh decompile, beta v2.20.a): F6 nulled `myKillTarget` by hand, leaving a stale entry in the target's `AI_TargetClaimer.myClaimants`; the AI re-claimed and `ClaimTarget` adds duplicates; `Controller_Base.LostConnection` nulls `myOwner`, removes one copy, re-scores the rest and throws in `SetPriority`. Not team-based. 0.3.0 releases both controllers' claims before a swap, prunes dead and duplicate claimants, F6 unclaims through the game, and a swap that still throws reconnects orphaned controllers. Deployed; the owner's playtest passed on two PCs (T-20261018-2).
- The mod is now `examples/SwapUnaware` (PR #2, merged, plugin 0.1.4, not tagged). The repo copy is the build folder; `mpn-mods/SwapUnaware` keeps old zips and a MOVED.md.
- Workshop: no in-game uploader (decompile only queries). Community MPN-Workshop-Uploader moved to github.com/placenamehereig/MPN-Workshop-Uploader (MIT, 1.1.0, Facepunch.Steamworks, interactive console; its content path must contain a dot, so name the folder `X.content`). The owner wants the item Public ("team" in the session-3 prompt was a misreading: he meant swapping characters). Uploaded as public item 3822329165 with a no-prompt Facepunch program, now examples/WorkshopUpload.

## 2026-10-18 · session 3: first beta launch, swap-and-unaware mod

- Beta launch: build 25615196, Beta v2.20.a, `mods_allowed: true`. GreenBloodHank enabled from the gear icon, `mods.log` showed `OVERRIDE: Hank (Character)`, and the owner saw green blood in the Playground. Real log shapes: [notes/2026-10-18-beta-launch.md](notes/2026-10-18-beta-launch.md).
- The owner could not find the gear icon before: the release build destroys the mod button (`Object_Modding_Remover`). No gear icon means not on the beta.
- Swap-and-unaware mod built (modtools' first live test): F5 swap to the character under the mouse (allies or enemies, as the owner's son asked), F6 everyone else unaware, F7 keep them unaware. No Harmony patch needed; a MonoBehaviour from `Initialize()` polls the keys. Source outside this repo; a pack script makes a zip with an `install.cmd` for another computer. Waiting on the in-game test.

## 2026-10-17 · session 2: real game, evals, install, launch

- Request: the owner's session-2 prompt (real-game checks, harder evals, local install, public launch).
- Real game: release branch, v1.11.a, build 25615280, Unity 2019.2.6f1, Harmony 2.4.2 shipped, no netstandard.dll. Decompiled Assembly-CSharp with ilspycmd into the session scratch folder (never into the repo) and read the loader: [notes/2026-10-17-real-game-loader.md](notes/2026-10-17-real-game-loader.md). The official doc's `mods.txt` is really `Mods/mods.log`.
- Branch not switched: backup taken, test mod GreenBloodHank deployed; waits for the owner.
- mpn.py: where/log read the build from Player.log, enabled.txt, mods.log line shapes; check-mod follows the loader's rules and flags near-miss Roots. 26 unit tests.
- Evals: new fixtures subtle-mod and Arena_PG; value runs 2 and 3 and baselines; triggers 30/36, then 36/36 after routing the level prompts to madnesswright-scenes (scenes L-001). A tester found net472 does not build against the game's net48 Harmony.
- Launch: public repo, v0.1.1 and v0.1.2 releases, wiki (10 pages, live check clean), banner, icon, Dependabot. Listings skipped: the owner halted awesome-copilot for good and put the Claude directory on hold.
- Gotchas met:
  - `cat > file` with no input inside a Bash call hangs until the 120 s timeout; write scripts with the Write tool.
  - The installed plugin is a cached copy: bump the version and `claude plugin update`, or headless trigger runs test the old descriptions.
  - A trigger runner that stops at the first Skill call scores "hub first, then specialist" as a miss; record up to three.
  - The vault clone committed with the Gmail address and the push was declined; set the no-reply email per clone.

## 2026-10-17 · creation

- Request: an evergreen plugin for modding MADNESS: Project Nexus and level editing.
- The game was not installed on the dev machine at the start; a Steam manifest for it (0 bytes downloaded) appeared during the session. Everything was built from research plus the developer's public tool repositories.
- Key discovery: no in-game level editor. Levels come from the official SceneTools Unity 2019.2.6f1 project (`.sobj`), assets from ModTools (`.mobj`), data from MadCards, code from built-in Harmony 2.4.2. Modding is releasebeta-only.
- Decisions: [decisions/2026-10-17-name-and-shape.md](decisions/2026-10-17-name-and-shape.md).
- Gotchas met:
  - The official Google Docs examples use curly quotes and a non-JSON colour syntax; `check-mod` flags curly quotes.
  - Google Docs fetch: `/export?format=txt` works where the normal view does not.
  - The Steam News API is the real changelog; the store news page renders empty to fetchers.
  - The SceneTools build menu says Compressed is *RECOMMENDED* while the 2024 doc says compressed loads slower; the skill offers both.
  - `rm -rf "$VAR/..."` is refused by a safety check; write `"${VAR:?}/..."`.
  - Heredocs holding a doubled backslash are refused by a hook; write generator scripts with the Write tool.
- Tests: 18 unit tests pass; six value cases passed run 1 with the skill; baselines for the hub and scenes action cases also found the faults (fixtures too easy), the modtools baseline used BepInEx and no info.txt (the skill made the difference).
