# Plan: two players in Director (0.8.0)

Request (the owner, 2026-10-24): "two player support for the mod". Asked once which kind; the owner chose **the game's own local co-op** (a second player on a pad, same screen). Online play was not asked for. Game side: [notes/2026-10-24-two-player-research.md](../notes/2026-10-24-two-player-research.md).

## Rulings

1. **The keyboard player drives the Director.** Its keys are keyboard keys and its picks use the mouse, so every key acts for player one (slot 0). A pad player gets no Director buttons. Small call, recorded in the mod README.
2. **Every feature treats every player as a player:** never frozen by F2, never guarded, never cleared, never made a target by F6, F7 or F10, never removed or respawned by a scene load. F10's "attacks you" still means the keyboard player.
3. **Scenes keep the other players' characters** (file version 6, `player` 2 to 4). A load puts a character a player plays now back on its saved spot with its saved weapons (in a story or arena career only the ammo, as for you) and changes nothing else. A player who joined after the save stays where they are; a character a player played at save time but nobody plays now loads as a squadmate. Older files load as before.
4. **The Playground load stops using the game's Clear All**, which removes player two, and clears the room the same way while sparing every player.
5. Player.log gets one line per join, leave and change of character (`PlayersFeature`).

## Not done

- Pad buttons for Director actions (a pad player cannot reach F-keys). Ask if the players want it.
- F5 and F8 per player (only the keyboard player takes over and goes home).
- Adding a second player where the game has no free squadmate (Playground, arenas): the game's join waits for one. Spawning a character into your squad in the Playground gives one.

## Play test (needs two people and a pad)

1. Story stage: press Start on the pad (answer No to "Control Player One?"), go through a door if player two has no character yet. Player.log: `player 2 joined on pad 0`, `player 2 now plays ...`.
2. F2: everyone freezes except both players; F2 again. F9 on a squadmate, then let player two take it over (pad Start-and-shoulders out and Start in again): it moves.
3. Patrol squares (F3) placing and the INSERT menu's click: player two can still shoot during them.
4. Save a scene (F11) with both players, both walk away and fire, load: both back on their spots with their ammo; the log names `played by player 2: put back`.
5. Playground: spawn a squadmate into your squad, join player two on it, save, load: player two is still there (not removed), nobody doubled.
6. Player two dies, then leaves (Start with both shoulders). Load a scene after: no exception, `no player plays it now` line.
7. Clear corpses with player two's body down: it stays.
