# Sources and community hubs

Checked 2026-10-17. Primary sources first.

## Official

- Steam store, app 488860: https://store.steampowered.com/app/488860/ (demo 1446020). Windows only; Steam Deck "Playable".
- Steam news as JSON (the real changelog; the store news page renders empty to fetchers): https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=488860&count=25&maxlength=0&feeds=steam_community_announcements
- Modding Overview (ModTools, MadCards, Harmony): https://docs.google.com/document/d/1f_RbQGE45CY8no6mFuVGTaOTF7XnaK7yq5tJXty_JSA (fetch as text with `/export?format=txt`)
- SceneTools Overview: https://docs.google.com/document/d/1-fchWz0-kYVMUwZcM1RIST8VmWlQ-SmjcuSe2qz9EJI
- Event System Overview: https://docs.google.com/document/d/1aLz0hngXuPbw2UbXwU6yontyCwL7mzKk789GHfMTzJM
- Modding Resources Master Doc (index of tools, tutorials, asset dumps; links live only in the doc): https://docs.google.com/document/d/1fgcDNarKoSe3I8CwaciqVxovZZJQlNQwTRUWRmvslN4
- Official tool projects (Unity 2019.2.6f1, no license file, no releases): https://github.com/MPN-Modders (MPN_SceneTools, MPN_ModTools, and their _Testing twins; MPN_ModTools last pushed 2026-04-07, MPN_SceneTools 2026-09-02 per the GitHub API on 2026-11-12, and community guide G2 moved its ModTools link here on 2025-09-23)

## Community

- Steam Workshop (about 925 items on 2026-10-17): https://steamcommunity.com/app/488860/workshop/
- Steam discussions: https://steamcommunity.com/app/488860/discussions/0/ and Workshop discussions https://steamcommunity.com/workshop/discussions/?appid=488860
- Announcements and comments: https://steamcommunity.com/app/488860/eventcomments/
- Discord: https://discord.com/invite/projectnexus (the Modding Discord is linked from the master doc)
- Uploaders: https://github.com/placenamehereig/MPN-Workshop-Uploader (moved from Twingamerdudes) and https://soup-o-stat.itch.io/mpn-gui-workshop-uploader. The community uploader wants the icon as PNG or GIF (JPG is rejected), small, 512x512 usual, given as a full path with extension; the content path is the mod folder under LocalLow/.../Mods (guide 3305980802, single source, unverified in game).
- NotEnoughMadness org (Harmony sources of the split NEM plugins): https://github.com/NotEnoughMadness
- MadCard Maker community "Master Document", a link index (2026-11-12c harvest, item 54): https://docs.google.com/document/d/1fgcDNarKoSe3I8CwaciqVxovZZJQlNQwTRUWRmvslN4 . It lists Slash's Asset Viewer, Pykar's custom trail shader and Game Asset Extraction tutorial, Cturix's sound library, Gibbing Tree "official misc assets" and dialogue scripts, Krinkels' "Modern Blender notes" (Blender 2.8+), Zep's tutorials (custom weapon audio, projectiles, ranged, melee), BluCop's ranged attachment guide, Bagmetti's animation importing tutorial, Cl4ssic's armor tutorial, Diggy's material and font tutorials, and the Event System tutorial ("mission card functions", 2026-03-08). It marks Lua for M:PN obsolete and notes ModTools Character Refs (2026-04-07) and SceneTools custom Arena stages (2026-03-08). Fetch `export?format=html` for the link targets.
- Slash's Asset Viewer (SAV, linked from the Master Document): previews `.mobj` bundles without the game; use in madnesswright-modtools references/madobjects.md (2026-11-12d harvest, item 59).
- Official dialogue transcripts and briefings for writing in the game's voice (2026-11-12f harvest, item 80; community-kept copies of official text): STORY MODE folder https://drive.google.com/drive/folders/1zEPjlzi-pgBnmCeLdrxMb1CeY0PR8j2X (per-area folders, "All Mission Briefings" https://docs.google.com/document/d/1kA-pS-YbIJ3UrI5mlMPTSGJ2MNI7c2PrRr0CLx6qJoE, layout doc "(FORMAT)" https://docs.google.com/document/d/1xwLvuWlGnc0sCJ43KGrqRh9cAcK383SDjcFEshePCeE: BRIEFING / OBJECTIVES / MISSION ENGAGED / MISSION COMPLETE) and ARENA MODE folder https://drive.google.com/drive/folders/1ln1t2WmTIT8BhQihTdDHPE5vvnlMbV1W (INDEX https://docs.google.com/document/d/12AYsoek_0l5ahKePKSfwxdzLKAbIPZ5zwFSZHzhGqzE). "MPN Official Misc assets" (.blend, .unitypackage, textures, Nov 2024): https://drive.google.com/drive/folders/1FztFgPoyyrXtZFt7VIo1WtptykoKKjQp
- Event Conditions and Actions sheet (official): https://docs.google.com/spreadsheets/d/1RYN4OIM1cvu2E422TRfWfcJJNmy1YE1pJWEYw8dGEs4 (`export?format=csv`); digested in madnesswright-scenes references/event-actions.md.
- Nexus Mods has a handful of legacy files: https://www.nexusmods.com/madnessprojectnexus

## Not sources

- forums.madnessprojectnexus.com no longer resolves.
- "Lua for M:PN" is marked obsolete in the master doc.
- Fan pages for the Flash classic, Roblox "MPN" games and itch.io "pn2" projects are other games.
