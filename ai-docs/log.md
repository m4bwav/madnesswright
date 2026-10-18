# Session log

Append-only, newest first.

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
