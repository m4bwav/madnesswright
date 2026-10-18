# Plan: swap characters and make everyone else unaware (a Harmony mod)

Owner's request, 2026-10-17: in game, swap control to another character, and with the same key (or a second one) put every other character into the unaware state. This is a madnesswright-modtools job (a Harmony assembly), and a good real test of the skill once the game is on `releasebeta`.

## What the game already has (read from the decompile of build 25615280, 2026-10-17; re-check on the beta build before writing code)

- **Swapping exists, gated.** `Controller_Player.SwapControllersWithSquadmate(Combatant_Base inSquadmate)` hands the player's controller to the squadmate and back through `Combatant_Base.AssignController(Controller_Base, bool overrideCurrentController)`. The caller only allows it when the target is alive, is NPC-controlled (`myController.AmType_NPC()`), and both characters are in the career's core roster (`Game_Manager.currentCareer.chars_Core`). Around `Controller_Player.cs` lines 1725 to 1750 in the decompile.
- **"Unaware" is an AI alert state.** `AI_Wakeup.AlertStatus` is `Unaware = 0, Hunting = 1, Combat = 2, Match = 100`. `Controller_Base.SetAlertStatus(Controller_Base C, AI_Wakeup.AlertStatus inStatus)` (static) sets it; `AI_Modules` uses it already (wandering only when Unaware; executions allowed on an Unaware target).

## Route

1. Decompile the beta build (madnesswright-modtools Step 5), confirm both members and find: how the game lists every living combatant in the scene, and where input is read each frame (or add a small MonoBehaviour from `Initialize()` that polls a key).
2. **Swap:** on key press, pick the character under the cursor (or cycle through every living NPC) and call the swap. Either a Harmony patch that relaxes the `chars_Core` check, or calling `AssignController` the way `SwapControllersWithSquadmate` does. Decide after reading what the gate protects (saves, career roster); test in Playground first, not the story career.
3. **Unaware:** on key press (or right after a swap), loop over every combatant other than the one now controlled and call `Controller_Base.SetAlertStatus(c.myController, AI_Wakeup.AlertStatus.Unaware)`. Check whether the AI re-alerts at once (`AI_Wakeup` may wake them again on sight or sound); a timer or a patch on the wake-up check may be needed.
4. Skeleton from madnesswright-modtools: net48, references from `Managed/` with `Private=false`, `info.txt` with `AssemblyPath`, `HarmonyID` (for example `com.m4bwav.mpn.swapunaware`), `EntryType`, the project outside the mod folder. `MPN check-mod`, `MPN deploy`, enable, play, `MPN log` (`applied_harmony` must list the HarmonyID).

## Open questions

- Swap to enemies too, or only allies? (Enemies have another faction; controlling one may confuse the AI or the win condition.)
- Which keys? Keep clear of the game's own bindings.
- Does it break saves if used in the story career? Back up first (`MPN backup`) and keep it to Playground until known.

Needs the `releasebeta` branch: the release build does not load mods (see [../notes/2026-10-17-real-game-loader.md](../notes/2026-10-17-real-game-loader.md)).
