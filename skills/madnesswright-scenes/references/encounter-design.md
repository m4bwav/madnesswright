# Encounter design and playtesting for Project Nexus levels

Adapted from The Level Design Book (book.leveldesignbook.com, process/combat and process/blockout/playtesting, read 2026-10-17), which is written for 3D games; the side-view adaptation is ours. Madness rooms are small, side-on, with the camera fixed relative to the player and enemies entering through doors.

## Plan per room before placing enemies

Write one line per room in `LEVEL-PLAN.md`: purpose (introduce, combine, test, rest), enemy types and counts, entry doors, cover, items, the beat it plays. Copy the table from [level-plan-template.md](level-plan-template.md).

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

## Enemy roles

The sections below follow Rogers, *Level Up!* (3rd ed., Levels 10 to 13), adapted to side-on rooms.

Give every enemy card the level uses one role in `LEVEL-PLAN.md` before choosing groups. The game does not label its enemies by role, so decide from what the card does: spawn it alone in a Playground arena and watch whether it closes in, keeps its distance, holds its spot or soaks hits up front. Write the role you saw, not one you assumed from the name.

| Role | What it does | Pairs well with |
|---|---|---|
| rusher | closes to melee range fast | a gunner behind it, so the player cannot just back off and shoot |
| gunner | fires from mid range | a rusher or a shield in front |
| shield | takes hits up front, slow to kill | a gunner behind it: the player has to flank or break the shield first |
| sniper | long range from a Sniperpoint | a guard on the door below it |
| guard | holds a doorway or a spot | a sniper above, or gunners entering from another door |
| heavy | big, slow, hits hard | a few weak fast ones, so standing still to fight the heavy is a risk |
| late unit | arrives near the end of a wave (GenerationScale pushes some types late) | whatever is left of the wave, as its last push |

- Mix roles so the player has to choose what to kill first. Two of the same role is just more of the same.
- Even a big group should have only one or two enemies actually attacking at once. Cap that with squad size and Max, not with the total count.
- A fast enemy must not attack without pause, and a sudden lunge needs a wind-up first. A heavy that is also fast feels unfair.
- Enemies are there to be fought. A design the player can only run from (a threat chasing in from off-camera) teaches them to stop playing.

## Orchestration order

For a stage or a campaign, bring in new things in this order and give each step a room:

1. Movement and a pickup, no enemies or one easy one.
2. One enemy role alone.
3. A second role alone.
4. The two together.
5. A hazard or an environmental threat over familiar fighting.
6. The hazard and enemies together.
7. A twist on something known: enemies from both doors, lights out, a Blocker_Backtrack lock-in.

At most two or three new things per stage, the first stage aside. Each new enemy comes with the tool or space that beats it. By about three quarters of the way through, the player has met everything, and the last quarter is for using it.

## Wave and squad pacing

For Squad Spawner waves, Arena Siege rounds and Zone_Spawner fights:

- Cap the number attacking at once (one or two), not only the total.
- Enemies enter through doors off camera, or from a spawner the player cannot stand on. Never spawn one inside melee range or in view next to the player.
- A new role's first wave is that role alone, readable, in a room the player can see across.
- Waves get harder by what is in them and where it stands (roles, doors, Sniperpoints), not only by how many.
- The last wave is the peak. Follow it with a release: a quiet room, a locker or a weapon cache.
- Keep an arena round to roughly 3 to 5 minutes (a heuristic; check against playtest times). Long rounds get harder from fatigue alone.
- Write the economy per wave: what Weapon Stockpiles, lockers and Item_Spawns hand out, checked against the wave count.

## Boss or finale room

- One readable pattern: a main attack to learn, a big move to survive, then an opening with a clear tell. The openings get shorter each round.
- Build the room around the fight: a wide flat floor, cover that can break, no corner that traps the player.
- Base the fight on what the player already knows, or on one ability just gained, never both.
- The player lands the final hit, not a script.
- If the stage allows, a death does not reset the whole fight.
- The last boss is a little easier than the hardest fight before it.

## Fairness

- No death the player could not see coming: no unseen ledge drop, no off-screen ambush without a tell (a sound, a door opening, a shadow).
- Hazards look dangerous, and timed ones keep a fixed rhythm the player can learn, never a random one.
- A respawn or checkpoint faces into the room, on flat ground, away from enemy spawns and their sight lines.
- Plan for challenge, never for a death count: no room's plan says "the player dies here N times".
- A designer who has played a room a hundred times judges it too easy. Trust fresh players' times and deaths over your own feel.

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
9. Before handing it over, play it badly yourself: walk the level backwards through every door, run past enemies to see whether they follow, hug walls and corners, jump at the camera borders, and set each knob to its extremes (Squad Max, GenerationScale) before settling on a value.
10. For a player new to the level: set the questions before the session (can they find the exit, clear each fight, tell what to do next?), record the screen and their voice if they agree, and watch without helping. Act on a problem two players hit; treat one player's suggestion as a note.

After the session: run `mpn.py log`, compare results with `LEVEL-PLAN.md`, change one thing at a time.
