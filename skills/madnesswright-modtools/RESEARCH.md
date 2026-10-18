# Research: madnesswright-modtools

Findings that back [SKILL.md](SKILL.md). Changes they caused are logged in [CHANGELOG.md](CHANGELOG.md); procedural lessons live in [LEARNINGS.md](LEARNINGS.md); test runs and their evidence in [TESTS.md](TESTS.md); schedule and state in `evergreen.json`. Protocol: MAINTENANCE.md.

Topic: MADNESS: Project Nexus ModTools MadObjects (.mobj) and Harmony 2.4.2 assemblies loaded from info.txt. Tier `moderate`. Last refresh 2026-10-17; next due 2026-11-16.

## Current understanding

- Settled: new art goes through the developer's ModTools project (Unity 2019.2.6f1) as MadObjects in `.mobj` bundles, referenced by MadCards through ObjRef (prefab name, not bundle name); output folder `LocalLow/Gibbing Tree/Madness Project Nexus ModTools/MadObjs/` (R-20261017-1).
- Settled: since beta v2.19.a (2026-04-15) mods can ship Harmony 2.4.2 assemblies through info.txt (AssemblyPath, HarmonyID, EntryType implementing IModEntrypoint, PatchOrder, ManualPatch); BepInEx is no longer needed (R-20261017-2).
- Settled practice: decompile before patching; pick the framework from the Managed folder (BepInEx's rule); re-check targets after updates (R-20261017-3).
- Settled for build 25615280: `IModEntrypoint` is in the global namespace; `Managed/0Harmony.dll` is Harmony 2.4.2.0; no `netstandard.dll`, so net472; Mono (MonoBleedingEdge, managed DLLs on disk) (R-20261017-6).

## Open questions

- Resolved 2026-10-17 (R-20261017-6): global namespace; 0Harmony.dll 2.4.2 ships.
- Are Automatons moddable yet?
- Resolved 2026-10-17 (R-20261017-6): both. `Mods/mods.log` gets `! ERROR: Harmony Patch <id> Failed!` and Player.log the exception.

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

### R-20261017-6 · 2026-10-17 · The game's Harmony loader, read from its code
- Summary: `IModEntrypoint` (global namespace: `Initialize()`, `Unload()`); mods patched in ascending PatchOrder; the DLL is loaded from bytes at `<mod>/<AssemblyPath>`; `PatchAll` unless ManualPatch; EntryType resolved with `Assembly.GetType` and created with a parameterless constructor; an Initialize exception unloads and unpatches; HarmonyID blank becomes `workshop.<id>` or `local.<guid>` (new every launch), spaces become `_`; extra DLLs resolved from the AssemblyPath folder. Log strings: `APPLIED HARMONY PATCH:`, `! ERROR: Harmony Patch <id> Failed!`, `HARMONY FAIL: Path <p> not found.`, `FAIL: Entrypoint "<t>" Not Found`, `does not implement IModEntrypoint`. `Managed/` holds `0Harmony.dll` 2.4.2.0 and `mscorlib.dll`, no `netstandard.dll`; Unity 2019.2.6f1 per Player.log.
- Track: subject
- Sources: the user's own install (Steam build 25615280, release v1.11.a, Unity 2019.2.6f1), read 2026-10-17: `mpn.py where`, Player.log, a listing of `Madness Project Nexus_Data/Managed/`, and a decompile of `Assembly-CSharp.dll` with ilspycmd 11.1 (classes Mod_Manager, ModItem, WorkshopItem, Steam_WorkshopIntegration, Converter_JSON, AssetBundle_Handler, Game_Manager, Saves_Manager; output kept out of the repository); the official Modding Overview as text, https://docs.google.com/document/d/1f_RbQGE45CY8no6mFuVGTaOTF7XnaK7yq5tJXty_JSA/export?format=txt
- Magnitude: moderate
- Applied: C-20261017-3

### R-20261017-5 · 2026-10-17 · Load evidence
- Summary: Evidence that a mod loaded is a log line naming it (Debug.Log from Initialize with the HarmonyID) plus mods.txt; Surity (a BepInEx test runner) does not apply to the built-in loader.
- Track: testing
- Sources: https://docs.unity3d.com/Manual/log-files.html, https://packages.nuget.org/packages/Surity.CLI/1.0.4
- Magnitude: minor
- Applied: C-20261017-1 (Step 5.5)

### R-20261017-4 · 2026-10-17 · Decompile tooling
- Summary: mcilspy (PyPI, MIT, 2026.10.12) is an ILSpy MCP server (decompile_method, search_methods, ...); ilspycmd is the default route. UnityInfoMCP (runtime bridge via BepInEx or MelonLoader) is rated stale and unverified: not recommended. universal-modder gives the general workflow.
- Track: tooling
- Sources: https://pypi.org/project/mcilspy/, https://glama.ai/mcp/servers/snowyegret23/UnityInfoMCP
- Magnitude: minor
- Applied: C-20261017-1 (references/harmony.md Tools)

### R-20261017-3 · 2026-10-17 · Decompile first, framework from Managed
- Summary: Harmony failures ('Could not find method', 'No target method specified', breakage after hotfixes) are fixed by decompiling the current Assembly-CSharp and checking signatures; BepInEx's target framework rule (netstandard.dll present: netstandard2.0; else net46 by mscorlib). No dated report of an AI agent inventing MPN APIs; the guard is inferred from general reports.
- Track: practice
- Sources: https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/2_plugin_start.html, https://ostranauts.wiki.gg/wiki/Modding/BepInEx_Modding
- Magnitude: major (initial)
- Applied: C-20261017-1 (Step 5 guards)

### R-20261017-2 · 2026-10-17 · Built-in Harmony
- Summary: Modding Overview Part 8: Harmony 2.4.2 support, info.txt keys and the IModEntrypoint entry class. Steam news 2026-04-15: 'Assemblies can now be run directly from your workshop item'.
- Track: subject
- Sources: Modding Overview Part 8; Steam News API 2026-04-15
- Magnitude: major (initial)
- Applied: C-20261017-1 (Step 5, references/harmony.md)

### R-20261017-1 · 2026-10-17 · Creation pass: ModTools project
- Summary: Shared creation pass (cost line in madnesswright R-20261017-1). github.com/MPN-Modders/MPN_ModTools (15 commits, newest 2026-04-07: Character_Ref). Modding Overview Parts 2, 5, 7: MadObject types, axis rules, Create Points, prefab name as ObjRef, AssetBundle names, Build menu, Weapon Animator states, parameters and events, Character_Ref layers, tags, BodyParts and SubParts, root at 0,0,0.
- Track: subject
- Sources: https://github.com/MPN-Modders/MPN_ModTools, https://docs.google.com/document/d/1f_RbQGE45CY8no6mFuVGTaOTF7XnaK7yq5tJXty_JSA
- Magnitude: n/a (initial)
- Applied: C-20261017-1
