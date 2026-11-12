# Continue: madness technique harvest

Work in D:/m4bwa/Claude/Projects/Ai/madnesswright. Goal: new MADNESS: Project Nexus modding techniques (MadCards, SceneTools levels, ModTools, Harmony) that the madnesswright skills lack.

1. Load state: top of ai-docs/HANDOFF.md, then the newest ai-docs/notes/*-madness-technique-harvest.md and 2026-11-11-level-technique-harvest.md (items so far, leads left, failed sources). Read the memory note on Reddit access routes before any Reddit fetch.
2. Harvest: one subagent works through the leads left in the newest note: Steam guides 2664043208 and 3305980802 (429 so far, one try each), Steam discussions and the most-subscribed Workshop items for app 488860, Reddit, GameBanana, the BepInEx Lua/MoonSharp link, NEM devlogs, ModDB. It writes a new dated note continuing the item numbers. No posting, voting or downloaded code.
3. Build: one subagent puts the items into the skills on a branch, labelling claims that are untested in game as "unverified in game". It runs the tests, waits for the PR's CI to register and pass (`gh pr checks --watch`; round c merged before checks registered), merges its own PR, pushes the tag (the release workflow cuts the release) and updates the installed plugin.
4. Waits on the owner: in-game tests of items labelled unverified (see HANDOFF.md). Never block on them; list them.
5. Stop when no lead is left or every source fails. Hand over: run evergreen-wrapup if installed, update HANDOFF.md and log.md, then print the start command `Execute D:/m4bwa/Claude/Projects/Ai/madnesswright/ai-docs/plans/madness-technique-harvest-continue-prompt.md`.
