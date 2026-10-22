# Scenes in every level: match and restore, not clear and respawn

Date: 2026-10-21. Status: decided (built as Playground Director 0.5.0, playtest pending; flagged to the owner).

## The request

Scene saving and loading (F11 save, CAPS LOCK list, while paused) works only in the Playground, and the requester wants it in the levels across the game. The owner approved a deeper rework.

## Choice

Four levels of work were on the table (A same room, B whole level, C load from another level, D level state). Built: **A and B everywhere, C in the Playground only, no D.** Research: [notes/2026-10-21-scenes-in-levels-research.md](../notes/2026-10-21-scenes-in-levels-research.md).

- **Playground:** unchanged behaviour. Clear All, then spawn every saved character, in its saved room when that room is still there. You keep your place. Old (version 1) scene files still load.
- **Story, arena, custom stages and hubs:** loading first moves you back to your saved spot and room (the game then spawns that room's own characters), waits 0.75 s, then:
  - characters already in the level are matched to the saved ones (same character while the level is loaded, then serial number, then the nearest with the same card in the same room) and put back: position, facing, damage, alert, frozen, guard, patrol. Their weapons stay as they are.
  - saved characters with no match are respawned in their room, with their serial number re-registered so the level's scripts find them again; wave units join the running wave (`WaveContainer.AddUnit`).
  - characters the scene does not hold are removed, except the player's squad, essential and playable characters, characters with a serial number (the level's script refers to them), bosses and cutscene actors.
  - respawning happens before removing, so no faction empties and no "faction dead" trigger fires.
- **Scenes from another level:** a Playground scene loads its Playground stage first, then restores. A story, arena or custom scene from another stage only says where it was saved: starting a stage by code writes the career save (squad reset, last played), and the owner's ruling for D was "only if it can be done without touching the career save".
- **Rooms the game has closed:** story levels lock each room you leave. If your saved room is closed, loading says so and asks you to restart the stage (pause, Restart) and load again; from a fresh start the mod moves you straight to that room.
- **Not saved or touched outside the Playground:** bosses and cutscene actors (their boss bars, patterns and cutscene links cannot be rebuilt).

## Small calls (in the mod README)

- The list shows this stage's scenes first; others show their stage in brackets.
- Outside the Playground you are moved back to where you saved; in the Playground you keep your place, as in 0.4.0.
- Restored "your squad" characters get their own Player-faction squad outside the Playground, so the story career never stores them.

## Known limits

- Removing a character counts as a kill for the level's "kill N" counters and arena kill counts. Doors, triggers, objectives and wave numbers are not rewound (D). A trigger already fired stays fired.
- Arenas cannot be paused between waves (the game blocks it), so scenes there save and load only mid-wave or before the first wave.

## Rejected

- Clear All everywhere (0.4.0's way): it removes squadmates, bosses and scripted characters in the room and fires every death trigger.
- Loading story stages by code (C everywhere): writes the career save.
- Rewinding level state (D): event conditions latch and mission data is spread over many classes; nothing showed that A and B feel broken without it.
