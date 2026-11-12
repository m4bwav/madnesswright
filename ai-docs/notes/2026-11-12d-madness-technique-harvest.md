# Madness technique harvest, 2026-11-12 (round d)

Goal: the same as [round c](2026-11-12c-madness-technique-harvest.md). I worked round c's leads and grepped the skills before counting anything (MadObj_Addon, AllowedAddons, Point_Addon, Reskin, mobj.log, Unlisted, CommitMods, ReturnAllMods, Asset Viewer, event path, NLA: none found except where noted). Item numbers continue from 54. Nothing here has been built or tested in game.

Every fetch was one try: curl with a browser user agent for the Google Doc HTML export, its linked Docs (`export?format=txt`) and Drive view pages; `drive.usercontent.google.com/download` for four documents (one RTF, three PDFs, read with pdftotext; no archives and no mod code); the GitHub REST API and raw files.

Sources:
- D1 link targets, from `export?format=html` of https://docs.google.com/document/d/1fgcDNarKoSe3I8CwaciqVxovZZJQlNQwTRUWRmvslN4
- D2: Blucop's Firearm Attachment Guide: https://docs.google.com/document/d/1ff3uKlEkKpZSQFWF4-O9VmIqyrU0XS4f0Yf-B6hjmCQ
- D3: MCM Manual (Overview for MadCard Maker v1.6c, DarkSignal): https://docs.google.com/document/d/1FrfLtZlpJ1_1yBQv6O9hQXwFCtXSi-wQamLU_523dt4
- D4: Slash's Asset Viewer guide: https://docs.google.com/document/d/18dokq2NWmuvXn16x3hTBCzn9PXg9eVxaJWW5EPERcwU
- D5: Diggy's Custom Material Guide (PDF): https://drive.google.com/file/d/1JwAUREA3FxggS3M-HtW4Cpz3T3Zx9c2q/view
- D6: LoreRaider's FMOD guide (PDF, 2024-07-02): https://drive.google.com/file/d/13yWsZ6w8x5XWPREwkYQjGxxQWYSmD7ux/view
- NEMLegacy source: https://github.com/NotEnoughMadness/NEMLegacy

Confidence labels as in round b.

## Techniques (ranked by usefulness)

55. **Firearm attachments: MadObj_Addon prefabs.** In Blender, add single-vertex mesh objects where each attachment meets the gun (and where bullets leave a barrel, extension, compensator or silencer) and snap each attachment's origin to its point with the 3D cursor; Shade Smooth; export FBX. In Unity set the tool handle from Center to Pivot so origins match Blender; untick Import Animations; make an empty GameObject with the `MadObj_Addon` script, rotate it so the green arrow points to the rear (scopes: down), scale the model to 12% if it was sized to game assets in Blender, parent and centre the model, make a prefab. In the script, elements 1 and 2 replace Grip_Main and Grip_Off (do not change the number of grip elements); `Point_Addon` is where bullets leave and overrides the gun's `Point_Shoot` (barrel core) or the previous `Point_Addon` (extension or cap); the blue arrow is the bullet direction. Bundle as for a gun; the MadCard is as for a gun, and the attachment must be listed in each gun's `AllowedAddons`. Skill: madnesswright-modtools (references/madobjects.md, new "Attachments" section). Source: D2. Confidence: single source (community). In-game test: yes. **Landed (0.1.27, PR harvest-d):** madnesswright-modtools references/madobjects.md, "Attachments: MadObj_Addon".

56. **Material reskins without a model: MCM "Reskins" plus a hand-written asset log.** Put the texture and its `_MAP` effect map in the ModTools Textures folder, make a material with shader Madness > Character - Standard and the map in Effect Map, add both to an AssetBundle, Madness > Build, drop the bundle loose in the mod folder. In MCM's Reskins: for Mirror skins write the new texture name; for armor, "Old" is the texture the armor uses and "New" is yours. To make MCM list your materials, put a log in the mod folder: `{ "Material": ["Material_Name", "Material_Name2"] }`. Skill: madnesswright-modtools (madobjects.md materials) and madnesswright (madcards.md). Source: D5. Confidence: single source; the card field names are MCM's, confirm there. In-game test: yes. **Landed (0.1.27, PR harvest-d):** modtools references/madobjects.md "Material reskins without a model" + madnesswright references/madcards.md "Reskins and asset logs in MCM".

57. **MCM bundle logs and load-order colours.** MCM reads asset names from JSON "bundle logs" saved with a `.LOG` extension; it writes `mobj.log` into the mod folder after the game has run in the MPN workspace, so asset dropdowns still fill when the mod is disabled. Categories: MadObj_Melee, MadObj_Ranged, MadObj_Addon, MadObj_Armor, MadObj_BodyPart, ParticleSystem, GameObject, AnimationClip, Texture, Material, Sprite, Fonts. Load-order colours: green clean, lime overrides a base statcard, yellow overrides a MadCard of another priority, red overrides one of the same priority (a real conflict). A card rooted on another MadCard must load after its root or it does not load in game; MCM sets that by itself on save. "Unlisted" cards accept non-statcard references and are flagged unsafe unless a plugin handles them. Workspace setting Local vs MPN; switching does not copy mods. Skill: madnesswright (references/troubleshooting.md for the colours; madcards.md Priority). Source: D3. Confidence: the tool author's manual for v1.6c (current MCM version not checked). **Landed (0.1.27, PR harvest-d):** madnesswright references/madcards.md "Reskins and asset logs in MCM" + references/troubleshooting.md "MCM load-order colours".

58. **Harmony hooks for a mod that loads its own files from every enabled mod.** NEMLegacy prefixes `Mod_Manager.CommitAllModsIfPending` (runs when mods are re-applied; unload your bundles there) and `Mod_Manager.CommitMods_MadObjs` (runs at start and after every re-apply), loops `Mod_Manager.ReturnAllMods(false)` and each `ModItem.Directory.GetFiles(...)`, and loads its own extension (`.nem`) with `AssetBundle.LoadFromFile`; scene bundles are told apart with `isStreamedSceneAssetBundle` and `GetAllScenePaths()`, and a non-scene bundle whose prefab holds `UI_MissionMap_Arena` was treated as a mission map. The repo's own comment says this broke with a later game update, so check the method names in a fresh decompile. Skill: madnesswright-modtools (references/harmony.md, a "Loading extra files" note). Source: NEMLegacy src/Patches/ApplyModsPatch.cs and src/Classes/AssetBundleLoader.cs. Confidence: source code, known stale. **Landed (0.1.27, PR harvest-d):** modtools references/harmony.md "Loading extra files from every enabled mod".

59. **Slash's Asset Viewer (SAV): preview a .mobj without the game.** Loads bundles into a list, sorts prefabs by component (MadObj_Ranged/Melee, Armor, Bodypart, Addon, particles, materials), previews armor alt meshes, alt materials and swatch colours, snaps addons to a spawned gun's attachment points (needs exactly one MadObj_Ranged in the scene), and plays exported animations on a preview grunt (left click single, right click the victim half of a double). Animations in the asset list itself do not work; use the Preview Animations tab. It is not an MCM replacement. Skill: madnesswright-modtools (a check step before playtesting) and sources.md. Source: D4. Confidence: single source. **Landed (0.1.27, PR harvest-d):** modtools references/madobjects.md "Checking a bundle before playtesting" + madnesswright references/sources.md.

60. **FMOD details from LoreRaider's guide.** Built banks land in the sample project's `Build/Desktop`; the Master bank must always be copied too. Stage music in MCM takes the FMOD event path (from File > Export GUIDs), and a stage can list more than one song. A voice set needs one 3D event with a multi-instrument per VoiceSet slot (one folder per voice set). The guide suggests one bank per mod, which fits the skill's "fresh project per extra bank" trap only if each bank comes from its own project. Skill: madnesswright-modtools (references/sounds-and-fonts.md). Source: D6. Confidence: community, agrees with G2. **Landed (0.1.27, PR harvest-d):** modtools references/sounds-and-fonts.md "More FMOD details".

Checked and already covered: the official Modding, SceneTools, Event System and Custom Animations docs (every event in the animation doc is in animation-events.md); Krinkels' Modern Blender notes (NLA-only export with All Actions off is already in animation-events.md; the rest is screenshots); Pykar's AssetStudio filtered export (porting.md covers AssetStudio).

## Leads not yet read

- Swain's Event System doc, "Mission card functions" (D1 marks it new, 2026-03-08): the doc (1aLz0hng...) is already a skill source, so diff its current text against stages-and-campaigns.md.
- Zep's videos (projectiles, ranged, melee, custom audio) and Bagmetti's animation importing video: YouTube, transcripts not tried.
- Cl4ssic's armor tutorial and the Drive folders in D1 (13N2tXfe..., 1FztFgPo..., 1ln1t2Wm..., 1zEPjlzi...), not opened.
- Debug Mode's flag: grep the decompiled game for a debug bool (no public source found).
- MPN-Modders/MPN_SceneTools_Testing and MPN_ModTools_Testing commit logs for beta-only features.

## Sources that failed or gave nothing

- Steam guide 2370383274 (G4): the page now comes back as Valve's new SSR shell with no guide text; one try, nothing.
- Steam discussions for 488860: the first page is player questions; only the pinned Master Doc is about modding.
- GitHub search "madness project nexus": no Debug Mode or NEM-port source. xvvd/MARC is a mod installer for cracked copies and nighthunter20/Set-game-speed is a spam trainer page; neither followed.
- NEMLegacy has no "Custom Stuff" JSON docs; SceneModLoader.LoadSceneData is an empty stub.
- The NEM "fanmade port" is still unidentified. ModDB returned 403. The Workshop top-items query needs an API key, so it was not tried. GameBanana and the Lua link were settled in round c.
