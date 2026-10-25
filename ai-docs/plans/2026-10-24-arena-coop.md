# Plan: arena co-op in Director (0.9.0)

Request (a Workshop commenter, 2026-10-24): "a fully functional arena multiplayer ... where both people can start out and choose origins and all". Today a pad player in an arena needs a free squadmate, and a young arena career has none (the hireling squad cap is 0 until the pilot is rescued). Game side: [notes/2026-10-24-arena-coop-research.md](../notes/2026-10-24-arena-coop-research.md).

## What "pick an origin" can honestly mean

In the game an origin is chosen once, when the arena career's one character is made (or at a new imprint), and the career, its cash, XP, checkpoints and unlocks belong to that character. There is no second career slot and no half-built co-op path. So player two cannot get a career of their own without rewriting the arena's save and reward flow. What Director can do: build a **guest character** for player two from an origin card when they join an arena, at player one's level, beside player one, and drop it again when the arena ends. Player one keeps the career exactly as now.

## Routes (agent time and size; risk)

| Route | What player two gets | Size | Risk |
| --- | --- | --- | --- |
| **(a) Copy player one's origin** | Press Start in an arena: a guest with player one's origin and level appears beside player one. No choice. | about 150 lines, 1 agent hour | low |
| **(b) Pad origin picker** (recommended) | Press Start in an arena: a small Director panel on screen, driven by that pad (left and right to browse, A to pick, B to cancel), lists the origins player one's career has unlocked with their attributes and traits. The pick spawns the guest beside player one at player one's level with the origin's armour and a starter weapon. Director remembers each pad's pick for the next arena. | about 350 lines, 2 to 3 agent hours | medium (pad buttons per pad number; the panel is drawn while the wave runs) |
| (c) Run the game's own origin screen a second time | The real character creator. It is its own scene that always creates a new career and travels to the HQ, so it would need a scene round trip and patches that stop it making a career. | 600+ lines, a day of agent time with play tests between | high: one mistake breaks a career save |

## Rulings for (a) and (b) (small calls, recorded in the mod README)

1. The guest is not saved in the career: not in `chars_Core` or `chars_Hired`, no career link. Cash, XP, tokens, checkpoints and stats stay player one's, as now. A stale squad id is dropped by the game at the next load.
2. Player two's death never ends the wave (the game ignores a character with no career). Player two is back beside player one when the next wave starts. **Player one's death still ends the wave**, as now: the arena has no revive, so keeping player one dead while player two plays on would end the run in a state the game never makes.
3. Only in arena stages, never in the HQ, the Playground or story levels (they have their own co-op). A pad that leaves (Start with both shoulders) takes the guest away with a poof, as the story does with bonus characters.
4. Every existing Director feature keeps treating the guest as a player (it is player-controlled, so 0.8.0's checks cover it). Scenes save it like any other player's character.
5. Not done: online play (the game has none), a separate career or progression for player two, origins the career has not unlocked (they stay locked, as in the creator).

## Play test (needs two people and a pad; a new arena career if none exists)

Back up saves first (`mpn.py backup`) and Player.log. This PC has no arena career, so the test starts with one (the intro bus ride comes first).

1. Start an arena from the HQ. Press Start on the pad (No to "Control Player One?"). (b): the panel shows, browse, pick. Player.log: `arena guest: player 2 picked <origin>`, `player 2 now plays <name>`.
2. Wave 1 with both. Player two dies mid-wave: the wave goes on; at the next wave player two is back.
3. Player one dies: the wave fails as before; the stage-end screen and the career save look normal (cash and XP as solo, no guest in the HQ squad).
4. A Director scene save and load mid-wave with both: both on their spots.
5. Start another arena: the pad's last pick is offered first.
