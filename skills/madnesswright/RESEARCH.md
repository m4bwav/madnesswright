# Research: madnesswright

Findings that back [SKILL.md](SKILL.md). Changes they caused are logged in [CHANGELOG.md](CHANGELOG.md); procedural lessons live in [LEARNINGS.md](LEARNINGS.md); test runs and their evidence in [TESTS.md](TESTS.md); schedule and state in `evergreen.json`. Protocol: MAINTENANCE.md.

Topic: MADNESS: Project Nexus official modding: releasebeta branch, mod folders, info.txt, MadCards, enabling, mods.txt, Steam Workshop sharing, legacy routes. Tier `moderate`. Last refresh 2026-10-17; next due 2026-11-16.

## Current understanding

- Settled: MADNESS: Project Nexus (Steam 488860, GIBBING TREE, LLC, 2021, Windows only, Steam Deck Playable) has official modding: mod folders in `%USERPROFILE%/AppData/LocalLow/Gibbing Tree/Madness Project Nexus/Mods/<Mod>/` with `info.txt`, MadCards (`.json` standard, `.txt` shorthand), `.mobj` and `.sobj` bundles, enabled from the gear icon, debugged from `Mods/mods.txt` (R-20261017-2, R-20261017-3). High confidence: from the developer's own docs.
- Moving: modding runs only on the `releasebeta` branch; release v1.10.a (2026-02-16) still excludes it, beta v2.19.a (2026-04-15) is the newest numbered beta (R-20261017-2). Medium confidence the split still holds; a volatile claim, re-checked at use.
- Settled: the Steam install folder is `Madness Project Nexus 2`; the Flash "Classic" is a different game (R-20261017-5).
- Unverified: `Player.log` location (Unity convention, not seen); the Proton prefix data path on Linux and Deck; the colour value syntax in MadCards (the doc's example is not valid JSON).
- Tooling: nothing agent-facing exists for this game (R-20261017-6). The community has MCM (MadCard Maker), two Workshop uploaders and the official asset dumps; no in-game uploader.
- The bundled `mpn.py` is ours; tested on synthetic fixtures and on the developer's sample scenes, not yet on a machine with the game installed.

## Open questions

- Is modding still releasebeta-only after v1.10.a? (Steam thread "Are mods allowed outside of beta yet?", 2026-10-11, unread)
- Exact Player.log path and whether the game ships `0Harmony.dll` in Managed (needs an install).
- Data path inside the Proton prefix on Steam Deck.
- Does `mods.txt` name each mod and its errors in a parseable form? (`mpn.py log` scans keywords until a real file is seen)

## Search plan

Four tracks; every refresh runs at least one query on each, scoped to the time since the last refresh (add the year). Protocol §4 explains the tracks.

Subject:

- Steam News API, the real changelog (the store's news page renders empty to fetchers): `https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=488860&count=25&maxlength=0&feeds=steam_community_announcements`; page back with `enddate=<unix>`. Look for "beta", "modding", "SceneTools", "ModTools", "Workshop", version numbers.
- The official Google Docs as text: `https://docs.google.com/document/d/<id>/export?format=txt` (follow the 307 redirect) for the Modding Overview `1f_RbQGE45CY8no6mFuVGTaOTF7XnaK7yq5tJXty_JSA`, SceneTools Overview `1-fchWz0-kYVMUwZcM1RIST8VmWlQ-SmjcuSe2qz9EJI`, Event System `1aLz0hngXuPbw2UbXwU6yontyCwL7mzKk789GHfMTzJM`, Master Doc `1fgcDNarKoSe3I8CwaciqVxovZZJQlNQwTRUWRmvslN4`; diff against the last copy.
- GitHub API: `https://api.github.com/repos/MPN-Modders/MPN_SceneTools/commits` and `.../MPN_ModTools/commits` since the last check; `https://api.github.com/orgs/MPN-Modders/repos` for new repos.
- `"Madness Project Nexus" modding OR mod OR SceneTools <month> <year>`
- Official forums: Steam discussions `https://steamcommunity.com/app/488860/discussions/0/` and Workshop discussions `https://steamcommunity.com/workshop/discussions/?appid=488860`; announcements `https://steamcommunity.com/app/488860/eventcomments/`. The Discord (`discord.com/invite/projectnexus`) is not readable by fetchers; ask the user for pinned modding posts when a claim depends on it. Steam returns 429 to fast fetches: space them out or use fetchwright.

Tooling:

- `"Madness Project Nexus" mod OR modding OR tool site:github.com <year>`; `path:SKILL.md "Project Nexus"` and `path:SKILL.md "MadCard"` on GitHub code search
- `https://registry.modelcontextprotocol.io/v0/servers?search=unity` and `?search=ilspy` (decompile and runtime-inspection servers)
- `"BepInEx" OR "Harmony" skill OR plugin OR "mcp server" <year> site:github.com`; check rehan-remade/universal-modder for an MPN field note
- Supersession sweep: archive flags on MPN-Workshop-Uploader, mcilspy, universal-modder

Practice:

- `"Project Nexus" mod "Claude" OR "ChatGPT" OR "AI" <year>`; `"Madness Project Nexus" modding tutorial <year>` (YouTube titles count as leads)
- `site:steamcommunity.com "488860" "mods.txt" OR "not loading" OR "releasebeta"`
- `"Harmony" "Could not find method" game update <year>` for general Unity-mod agent failure reports

Testing:

- `"Madness Project Nexus" mods.txt OR "Player.log" error <year>`
- `unity mod smoke test BepInEx OR Harmony CI <year>`; `"LogOutput.log" "Chainloader" test`
- Level design playtesting: `book.leveldesignbook.com` changes; `"level playtest checklist" side-scroller <year>`

Best sources (primary first): Steam News API; the official Google Docs; github.com/MPN-Modders; Steam Workshop and discussions; the Modding Discord through the user. Noisy: generic "Nexus" searches (Nexus Mods app news), SteamDB (403), nexusmods.com (403 to fetchers), Newgrounds (403), SEO guide farms.

## Findings log

Newest first. One entry per material finding; a quiet refresh gets one entry saying so. `Track` is subject, tooling, practice, or testing.

### R-20261017-8 · 2026-10-17 · No checker exists
- Summary: No checker exists for MPN mods; the evidence available is mods.txt, the in-game Mods list and the user's report. mpn.py check-mod and log were written to fill the gap.
- Track: testing
- Sources: Modding Overview (Troubleshooting)
- Magnitude: minor
- Applied: C-20261017-1 (scripts/mpn.py)

### R-20261017-7 · 2026-10-17 · Practice lessons
- Summary: The lessons that shaped the body come from the general modding route (read-only recon, reversible backup, verify in the running game, three strikes then change approach) and from Harmony failure reports (methods renamed by updates). No dated case study of an agent modding this game exists.
- Track: practice
- Sources: universal-modder README; https://ostranauts.wiki.gg/wiki/Modding/BepInEx_Modding
- Magnitude: minor
- Applied: C-20261017-1 (Step 2 backup, Step 5 evidence)

### R-20261017-6 · 2026-10-17 · No agent tooling for this game
- Summary: No skill, plugin or MCP server for this game. rehan-remade/universal-modder (MIT, 8.2k stars, 12 agent-neutral skills, v0.2.0) gives a general game-modding workflow with no MPN notes: pointed at, not built on. JosiahSiegel's unity-modding skill is about making a game moddable: not useful. MCP registry Unity servers drive the Editor, not a built game.
- Track: tooling
- Sources: https://github.com/rehan-remade/universal-modder, https://registry.modelcontextprotocol.io/v0/servers?search=unity
- Magnitude: major (initial)
- Applied: C-20261017-1 (modtools references/harmony.md)

### R-20261017-5 · 2026-10-17 · Legacy routes and naming
- Summary: Assembly-CSharp.dll replacement (MDU, Wingidon's), Not Enough Madness (1.08c; a BepInEx build for releasebeta; defunct on the Workshop beta), UABE texture swaps in sharedassets (GamePretty 2021). Install folder `Madness Project Nexus 2`. The Flash Classic is a different game. Mono backend (DLL replacement works; no GameAssembly.dll reported).
- Track: subject
- Sources: https://www.nexusmods.com/madnessprojectnexus/mods/1, https://gamepretty.com/madness-project-nexus-how-to-replace-models-textures/
- Magnitude: minor
- Applied: C-20261017-1 (Step 1)

### R-20261017-4 · 2026-10-17 · Community and sharing
- Summary: About 925 Workshop items (arena 151, origins 77, characters 68, weapons 54 ...); top authors include DarkSignal (MCM, Supermall campaign). No in-game uploader; MPN-Workshop-Uploader (MIT, last push 2025-02-28) and Soup-o-Stat's GUI uploader. Nexus Mods has a few legacy files; no GameBanana or Thunderstore presence found.
- Track: subject
- Sources: https://steamcommunity.com/app/488860/workshop/, Master Doc
- Magnitude: minor
- Applied: C-20261017-1 (Step 6, references/sources.md)

### R-20261017-3 · 2026-10-17 · Mod format
- Summary: From the Modding Overview: info.txt keys (Name, Author, Description, Tags, Dependencies by Workshop id; Harmony keys), MadCard rules (Filename and Type required, Root inheritance, override by Filename, '*' and null in arrays, 'empty', Priority, ObjRef with *Filename), .json strict vs .txt shorthand, gear-icon enabling, mods.txt for troubleshooting. Its examples use curly quotes and a non-JSON colour syntax.
- Track: subject
- Sources: https://docs.google.com/document/d/1f_RbQGE45CY8no6mFuVGTaOTF7XnaK7yq5tJXty_JSA
- Magnitude: major (initial)
- Applied: C-20261017-1 (Steps 3 to 5, references/madcards.md)

### R-20261017-2 · 2026-10-17 · Modding is releasebeta-only
- Summary: 'The release branch is up to date with all beta updates EXCEPT FOR MODDING!' (2024-12-20); the 2024-11-02 post says switch to beta and back up saves. Release v1.10.a 2026-02-16; beta v2.19.a 2026-04-15 (Harmony 2.4.2, Vessels, Character_Ref). A modded-inventory wipe on mod removal was fixed in beta 2.13.l (2024-12-19).
- Track: subject
- Sources: Steam News API items 2024-06-28, 2024-11-02, 2024-12-19, 2024-12-20, 2026-02-16, 2026-04-15
- Magnitude: major (initial)
- Applied: C-20261017-1 (Step 2)

### R-20261017-1 · 2026-10-17 · Creation pass
- Summary: Three parallel researchers (subject, community modding plus testing, tooling plus practice). The game has a full official modding pipeline (Workshop, ModTools, SceneTools, Harmony) documented in Google Docs and two GitHub repos; nothing for AI agents exists for it. Cost: searches 48 · fetches 79 · tool calls 130, plus 4 doc exports and 3 repo clones by the main session.
- Track: subject
- Sources: Steam News API; https://github.com/MPN-Modders
- Magnitude: n/a (initial)
- Applied: C-20261017-1
