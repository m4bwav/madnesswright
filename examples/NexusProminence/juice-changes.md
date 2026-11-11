# Juice changes

## 2026-11-11: shootable set pieces (breakables)

- Suggestion: shot feedback in the set (destructible props), from the game-juice plugin's juice-apply skill, kept to pieces that are in the film's rooms.
- File: Editor/NexusProminenceBuilder.cs: `Breakable()` wraps built boxes in a `CustomObj_Destructible` (the game converts it to `Interactive_Target`), with an `Intact` state shown until hit and a `Broken` state after.
- Where: room 2 display-case glass (shards on the floor), room 7 four ceiling lamps (glow and red cone go dark), room 10 eighteen monitor screens (black with a crack). 23 in all.
- Constants: `JuiceBreakables = true` (toggle), `GlassHits = 1`, `LampHits = 2`, `MonitorHits = 2`, sounds `GlassBreakSound` (Wine Bottle Break), `LampBreakSound` (Beer Bottle Break), `MonitorHitSound` (Richochet Sheild), `MonitorBreakSound` (Sparks/Electrocution), `MonitorExplosion = ""` (set `ElecSplode` for a damaging spark burst; that changes gameplay).
- Rules kept: `AllBulletsPassThrough`, no damage bar, no popup text, no explosion, `FlashColorOnHit` on. Hit boxes sit on the back wall, above heads or inside the case, out of the walk lane.
- Revert: set `JuiceBreakables = false` and rebuild (the broken pieces are then not made, and the intact ones stay as plain set boxes).
- Sources: SceneTools `CustomObj_Destructible.cs` and its sample prefab; the game's `Converter_Proxy.ConvertCustomObj` (decompile, build 25615196); explosion card names from resources (`stats/explosion/*`).
- Play check: shoot each kind once; confirm the sound, the flash, the swap, that no bar or icon shows, and that bullets still reach enemies behind. Tune `MonitorHits` first if the wall breaks too fast or too slow.
