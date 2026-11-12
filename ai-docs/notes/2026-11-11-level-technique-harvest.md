# Level technique harvest, 2026-11-11

Goal: new level-design and SceneTools techniques for MADNESS: Project Nexus custom scenes, beyond what [madnesswright-scenes](../../skills/madnesswright-scenes/SKILL.md) and its references already say. Each item was checked against the skill (SKILL.md, references/, RESEARCH.md, LEARNINGS.md) by keyword; only uncovered ones are listed.

Most usable material came from the official tutorials linked from the pinned Master Doc ("MPN Modding Resources"), not from community posts. The skill was originally built from the Scene Tools Overview, so most of that doc is already covered. These items are the details it missed.

Sources:
- SceneTools Overview (official): https://docs.google.com/document/d/1-fchWz0-kYVMUwZcM1RIST8VmWlQ-SmjcuSe2qz9EJI
- Event System Overview (official): https://docs.google.com/document/d/1aLz0hngXuPbw2UbXwU6yontyCwL7mzKk789GHfMTzJM
- Master Doc: https://docs.google.com/document/d/1fgcDNarKoSe3I8CwaciqVxovZZJQlNQwTRUWRmvslN4

## Techniques (ranked by usefulness)

1. **Weapon stockpiles are dealt back to front.** The last characters spawned in a wave draw first, starting from the end of the weapon list. A character takes the first weapon matching its WeaponPref and skips the others. Anyone left when the stockpile runs out spawns unarmed. To control which units get armed, order the list on purpose, set RandomSeed, and pad it with "Unarmed" (empty) loadouts. You can only see the result of a given seed by testing in game. Why: script-built waves otherwise arm the wrong units. Source: SceneTools Overview, Squad_Spawner / Weapon Stockpiles. Confidence: stated by the author.
2. **Doors have four states: Open, Closed, Opening and Closing.** Nobody can enter or leave through a Closed door. Characters cannot spawn into an Open door. While a door is Opening or Closing it accepts neither. Set NoClosingOrOpening true to skip the transition states. A custom animated door is driven by the `ToggleMe` boolean on its controller (the template is `Door_Standard`). Why: a spawner aimed at a door the player has just opened, or one still animating, stalls or delays the wave. Keep spawn doors closed, or use doors with NoClosingOrOpening. Source: SceneTools Overview, Custom Doors. Confidence: stated.
3. **Waypoint coverage rule.** Use only as many waypoints as you need for every part of the room to be visible from at least one waypoint in the chain. If the AI is heading somewhere that no waypoint can see, pathing goes strange. Why: this gives a script a checkable rule (a line-of-sight test from waypoints over the floor area) instead of an arbitrary node count. Source: SceneTools Overview, Waypoints. Confidence: stated. The "add a node where a corner hides an area" fix is inferred from the doc's example.
4. **Leave Leeway Mouse at its default in most rooms.** Extra camera scroll from mouse movement makes aiming harder. Why: this is a default for script-written Room_Layout values. Source: SceneTools Overview, Layout values. Confidence: stated (a recommendation).
5. **Events are scoped to one scene.** An EventCard registered in several scenes starts fresh in each, so state does not carry over. Use WorldChanges for anything that must cross scenes. Why: multi-scene stages that expect an event to remain "done" break. Source: Event System Overview, Part 1 notes. Confidence: stated.
6. **Platform Addon on moving CO_ Togglables.** Without it, a moving surface flings characters standing on it "like a catapult" or lets them fall through. Why: elevators, lifts and moving floors need it. Source: SceneTools Overview, Custom Togglable addons. Confidence: stated.
7. **Cutscene ViewCam animators.** When a Cutscene starts, it sets every Animator on its ViewCams to the Off state. Each Step then fires the `ToggleMe` trigger on that Step's ViewCam. Custom camera-move controllers therefore need an Off state and a transition on ToggleMe. The On state can have any name. Source: SceneTools Overview, Cutscenes / ViewCams. Confidence: stated.
8. **NPC_ExitModules on Seats** control when an NPC leaves its seat. Seats can also take a DriverCard and a DriverCard_Player for an attached Vessel. Why: these enable ambush-from-seated and vehicle setpieces. Source: SceneTools Overview, Custom Seats (section 4c). Confidence: stated. The exact module fields were not captured.
9. **AnimEvents_Interact.** An animated Interactive can fire its trigger from an Animation Event rather than straight away, so the effect lands on the right animation frame (a lever pull, a button press). Source: SceneTools Overview, Custom Interactives. Confidence: stated.
10. **Vendor Addon currency depends on the game mode.** A CO_ Interactive with the Vendor addon becomes a shop, but what the player pays with follows the mode. Do not assume a vendor works the same in Playground, Story and Arena. Source: SceneTools Overview, Custom Interactives. Confidence: stated. The per-mode behaviour itself is untested.
11. **Tower of MADNESS (Workshop map) as a pacing reference.** It is a 37-room demo where each room escalates in difficulty, made as the author's first SceneTools level. It shows that a linear escalating room chain is a viable first stage shape. Source: https://catalogue.smods.ru/?p=379858 (a Workshop mirror; the page itself returned 403, so this comes from the search snippet). Confidence: inferred and weak.

## Leads not yet read

The Master Doc lists these, but they were not fetched (video or not reached):
- The community "Scuffed Scene Tools" video tutorial and the official Campaign and Cutscene video tutorial. The Master Doc's YouTube links are https://youtu.be/u7VV0N33Sx0, https://www.youtube.com/watch?v=_xbtGGh6Rso, https://youtu.be/GAeAC3uuCoc, https://www.youtube.com/watch?v=N_SD_QYAMpw, https://www.youtube.com/watch?v=8rIUvsR_ekk, https://www.youtube.com/watch?v=AcD60wxGI2M, https://www.youtube.com/watch?v=YfUqQw-ajYA and https://youtu.be/a0NtVJeUhHE. Which link is which video was not mapped.
- A Steam guide linked from the Master Doc: https://steamcommunity.com/sharedfiles/filedetails/?id=2664043208

## Sources that failed or gave nothing

- https://steamcommunity.com/sharedfiles/filedetails/?id=3305980802: HTTP 429 (rate limited).
- https://steamcommunity.com/app/488860/discussions/0/4328600953645693620/: loaded, but it only points to the Master Doc. The comments are noise.
- https://steamcommunity.com/app/488860/discussions/ and /guides/: not fetched separately, because Steam was rate limiting.
- https://gamebanana.com/games/15564: the page shell only (an ad-block notice and navigation), with no submissions listed.
- A web search for "madness project nexus scenetools custom level tutorial" found no dedicated tutorial, only the Tower of MADNESS mirror. Results for other games were ignored.
- The Master Doc's plain `export?format=txt` drops its links; `export?format=html` keeps them. The linked Google Docs export fine as text without signing in.

## Round 2 (2026-11-11 evening)

The main new source was the commit history of github.com/MPN-Modders/MPN_SceneTools (https://github.com/MPN-Modders/MPN_SceneTools/commits/main). Its commit messages give only names and one line each. None of the terms below appear in the scenes skill. They are not in the skill yet: each one needs the Overview doc re-exported, or an in-game test, before it goes in.

12. **Spawner update (2024-12-09).** Character spawners can add or remove Traits and Behavior. Squad Spawners can be turned on and off. Zone_Spawner accepts Squads. Map use: vary the units in each room without new MadCards, and keep a room's squad off until its trigger fires. Confidence: stated in the commit.
13. **Traits NoIncap, NoDeadZoneDamage and BadBlock (2025-09-17).** NoRanged moved to the end of the trait list (2025-11-06), so any script that writes trait indices instead of names breaks. Map use: tune each room's difficulty through spawner traits. Confidence: stated.
14. **"Big Update" (2025-05-14).** Adds Boss Marker, Get-Out Zone, Core and Splosive Hazards, Debris Generator, Effect_DebrisGen, Effect_MaterialScroller, AnimEvents_Sound, Obj_Toy and Obj_Disposable. Map use: a Boss Marker for room 10 and explosive hazards as set pieces. Confidence: the names are stated; what each one does is inferred.
15. **Hazard_WaterZone, Effect_GlobalIntDisplay and Object_Randomizer (2026-04-07).** Effect_GlobalIntDisplay might show a wave or rooms-cleared counter on screen. Object_Randomizer could vary cover between runs. Confidence: inferred.
16. **Explosion Generator (2025-10-17).** A scriptable blast. Confidence: only the name.
17. **AssetBundle compression.** ChunkBasedCompression has been the recommended export since 2025-08-19 and replaces the slow old default, None. One commit, 08-12, recommended Uncompressed, but the later commit wins. Confidence: stated, and the commits contradict each other.
18. **Uniforms on Marker_Character (2026-09-02).** Check whether the skill says this. Confidence: stated.
19. **Workshop tags.** Custom stages are hard to find when the author leaves out the tags that appear in Steam's search menu. "Stage" and "Level" are not in that menu. Reference maps worth studying: Assault on Precinct, Madness Cellification: Interactive and Greenaders Rampage (custom Arenas). Source: https://steamcommunity.com/sharedfiles/filedetails/?id=3546293053. Confidence: stated.

YouTube links mapped by title (oEmbed):
- u7VV0N33Sx0: the official "SceneTools Tutorial: Custom Stages, Campaigns, and Cutscenes". This is the only video about scenes; get a transcript next.
- _xbtGGh6Rso: "Quick Room Tutorial for Firing Range". Not watched.
- The rest cover armor (GAeAC3uuCoc), weapon audio (N_SD_QYAMpw), projectiles (8rIUvsR_ekk), ranged weapons (AcD60wxGI2M), melee weapons (YfUqQw-ajYA) and importing animations (a0NtVJeUhHE, the "scuffed" one). None of them is about scenes.

Read but empty for scenes: guide 2664043208 (weapons, armor and the asset pipeline), guide 3305980802 (MadCard Maker only; it says stages are "not yet" covered), the Steam guides list (5 guides) and the SceneTools READMEs (installation only; no wiki).

Next leads: a transcript of u7VV0N33Sx0. Re-export the SceneTools Overview doc and diff it against RESEARCH.md for the 2025 and 2026 features (items 12 to 18). Check the Nexus Prominence Workshop tags against item 19.
