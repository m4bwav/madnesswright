# Encounter design and playtesting for Project Nexus levels

Adapted from The Level Design Book (book.leveldesignbook.com, process/combat and process/blockout/playtesting, read 2026-10-17), which is written for 3D games; the side-view adaptation is ours. Madness rooms are small, side-on, with the camera fixed relative to the player and enemies entering through doors.

## Plan per room before placing enemies

Write one line per room in `LEVEL-PLAN.md`: purpose (introduce, combine, test, rest), enemy types and counts, entry doors, cover, items, the beat it plays.

- One or two enemy types per encounter; three is complex; five or more is chaos.
- Introduce a new type alone in a readable room, then combine it with known ones.
- Raise difficulty by order, position and conditions (snipers on Sniperpoints, enemies from both sides, a Blocker_Backtrack lock-in), not only by count.
- Let the player read the room on entry: some enemies visible at the door, reinforcements from doors off to the side or later in the wave.
- Ambushes (Seats with V_Exit_EnemyNear, enemies behind the player) need a way out or a forgiving first hit.
- Each encounter has a start, a middle and an end: open with a squad, escalate with the Squad Spawner (GenerationScale pushes late units like Engineers to the end), finish with a boss or a final squad.
- Alternate pressure and rest: a quiet room with a locker after a hard one.
- Weapons and health are the difficulty lever in Madness: count what the Weapon Stockpiles hand out and what lockers and Item_Spawns give the player.
- Cover follows the weapons: ranged enemies need cover the player can use against them; melee rooms need space to dodge.
- Use the Difficulty Remover to add enemies on harder difficulties rather than building separate rooms.
- Random spawn timing gives flat pacing; script the waves (Squad and Max, Zone_Spawner, Blockers) on purpose.

## Playtest checklist

Fill this in for the specific level and hand it to the player. Ask them to play without help first, then answer.

1. The scene appears in the Playground list (or the campaign starts) and loads without a crash.
2. The player spawns at Player_Start facing into the room; the camera frames the action and stops at sensible borders in every room.
3. Every door works both ways where intended; no one gets stuck in a doorway.
4. Enemies reach every area (watch them chase the player into each corner); none walk off ledges unless meant to.
5. Every encounter can be cleared; Blockers lift when their spawners finish; events and missions complete.
6. Per room: time taken, deaths, weapons used. Is there one dominant strategy that trivialises the level?
7. What was confusing? What was the hardest moment, and was it fair?
8. Anything visible that should not be (other rooms, backgrounds, missing walls, see-through outlines)?

After the session: run `mpn.py log`, compare results with `LEVEL-PLAN.md`, change one thing at a time.
