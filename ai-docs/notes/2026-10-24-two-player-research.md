# Two players: how the game's local co-op works (build 25615196)

Read from a fresh decompile of beta build 25615196 (Beta v2.20.a) on 2026-10-24, for Director 0.8.0. Member names only; no game code is copied here. Nothing below has been seen in play yet.

## Joining and leaving

- `Joystick_Manager.Update` polls four XInput pads every frame. **Start** on a pad that has not joined adds it (`Joystick_Joins`); **Start with both shoulder buttons held** takes it out (`Joystick_Quits`). Up to four players (`MaxPlayersAllowed = 4`).
- The first pad to press Start while player one is on the keyboard gets a popup, "Control Player One?". Yes hands player one to the pad (`Player1UseJoystick`); No makes the pad player two.
- Players are slots in `Controller_Player.PlayerControllers[4]`, made once by `InitPlayerControllers`. Slot 0 is the keyboard and mouse (`SetKeys(-1)`, `AmKeyboard`), slots 1 to 3 are pads. `Controller_Player.PlayerController` is slot 0 and `Controller_Base.PlayerOne` its body. `Joystick_Manager.ReturnPlayerActive(i)` says whether slot i has a pad.
- A joining player takes the first member of the player squad that no player controls (`AddPlayerToGame`: `AssignController(PlayerControllers[n], override)`). With no free squadmate the player waits in `PlayersWaitingForAdd` until the next door (`Controller_Player.PlayerPassesThroughDoor` calls `PlayerSquad_AddWaitingPlayers`).
- At a door, story and custom stages (mode feature `MultiplayerSquadExpand`: Story, Custom) add a bonus squadmate from the stage card's `BonusPlayerCharacters` for each waiting player (`Career_Data.CreateOrReturnSquadmateByCard`, `Char_Data.ReserveController`, a new combatant at the door). The Playground and arenas do not, so there a second player needs a free squadmate in your squad.
- A new stage hands out controllers in order: `Career_Data.InitPlayerSquad` gives each squad member `ReturnPlayerOrBotByJoystick(index)`, a player controller while there are active controls left, a bot after.
- Leaving (`PlayerSquad_AttemptRemovePlayer`): the body gets a new bot controller. In story and custom stages that are not hubs, and only while the squad is larger than the stage's required characters, the bonus character is removed (`TeleportPoof`, `DeSpawn`); a required character swaps controllers with a non-required squadmate first.

## Player two in the rest of the game

- `Controller_Player.SetConnection` sets `amPlayer` on the body and calls `Game_Manager.MakeGameFocus`: the camera follows every focused body. Only player one sets `PlayerOne`.
- Player bodies are `Controller_Player`, not `Controller_NPC`: `AmType_Player()` is true and `AmType_NPC()` false. Bots (`Controller_Bot`) are `Controller_NPC`.
- `SwapControllersWithSquadmate` runs for every player (pad button `pressSwapSquadmate`, and with `overrideChecks` when the body dies, from `Combatant_Base.KillMe`).
- **The Playground's Clear All** (`InteractiveMode.ClearAll_DoIt`) removes every occupant of the current room except `PlayerOne`, and every held item whose owner is not `PlayerOne`: a second player's character and weapons go too. Director's Playground scene load used it, so 0.8.0 has its own copy that spares every player.

## Director's one-player assumptions (audit, 2026-10-24)

| Feature | Before 0.8.0 | 0.8.0 |
| --- | --- | --- |
| F2 freeze | froze every living character but player one's body, player two included | skips every player's body; lets go of one a player takes over |
| F9 guard | only NPCs can be picked; a guarded squadmate a joining player takes stayed pinned | unguards any body a player takes over |
| Left-click block (patrol squares, menu picks) | blocked every player's attacks | blocks only the keyboard player's |
| F8 home | a joining player can take your home body; said "dead" | says which player plays it |
| Clear corpses, scene body clearing | kept the player squad (covers player two) | also skips any player body explicitly |
| Scenes | saved only NPCs, so player two's character was left out; the Playground load's Clear All removed player two; a Playground load respawned a saved squadmate player two had taken since, doubling it | version 6 saves the other players' characters (`player` 2 to 4); a load puts a character a player now plays back on its spot with its weapons (ammo only in careers) and changes nothing else; never respawns or removes it; the Playground clear spares every player |
| F1 clear, F5, F6, F7, F10, F4, F3 | already NPC-only or `Playable` (covers `amPlayer`) | unchanged; F10's "attack you" means the keyboard player |
| `LossSwapPatch` | home-on-death only for player one; others fall through to the game's swap | unchanged |

New: `PlayersFeature` logs `player N joined on pad P`, `player N left` and `player N now plays <name> (<squad>)`.
