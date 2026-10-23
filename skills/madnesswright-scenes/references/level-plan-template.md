# LEVEL-PLAN.md template

Copy this into the mod folder as `LEVEL-PLAN.md` and fill it in before placing enemies. The playtest checks against it. Rules for filling it: [encounter-design.md](encounter-design.md). The beat chart follows Rogers, *Level Up!* (3rd ed., Level 4), adapted.

```markdown
# <stage name>

- Goal type: survive | explore | learn
- Objective (one line): <what this stage teaches or asks, e.g. "teaches fighting a shield and a gunner together">
- Target length: <minutes>
- New this stage (two or three at most): <role, hazard or mechanic>

## Rooms

| Room | Purpose | New element (one at most) | Enemy roles x count | Max attacking at once (1 or 2) | Entry doors and spawns | Cover | Items handed out | Big or small moment | Expected time | Expected deaths |
|---|---|---|---|---|---|---|---|---|---|---|
| R1 | introduce | | | | | | | small | | 0 |
| R2 | | | | | | | | | | |

Enemy cards and the role seen for each (from watching it alone in a Playground arena):

| Card Filename | Role | Seen doing |
|---|---|---|

## Waves (Squad Spawner, Arena Siege, Zone_Spawner)

| Wave | New role (one at most, none in the last wave) | Roles x count | Squad size and Max (1 or 2 attacking at once) | Doors used | Change from the wave before | Economy (stockpiles, lockers, Item_Spawns) |
|---|---|---|---|---|---|---|

After the last wave: <the release: a quiet room, a locker, a cache>

## Playtest results

| Room | Time | Deaths | Weapons used | Notes |
|---|---|---|---|---|
```

Expected deaths is what the room may cost a new player, a check on fairness, never a target to reach.

## Beat chart (campaigns)

One column per stage, one row per element. Read across each row before building:

- two new things landing in the same stage (clumping),
- the same combination of roles and space twice (sameness),
- the same lighting, colour or music cue in neighbouring stages,
- a stretch with no weapon or health handed out (economy gap),
- big moments next to each other with no rest between.

```markdown
| | Stage 1 | Stage 2 | Stage 3 |
|---|---|---|---|
| Stage card Filename | | | |
| Story beat | | | |
| Gameplay focus | | | |
| New enemy role | | | |
| New hazard or mechanic | | | |
| Space (tight rooms, open floor, both) | | | |
| Lighting and colour | | | |
| Music cue | | | |
| Weapons and health handed out | | | |
| Estimated time | | | |
```

A music row filled here is what musicwright-game builds the cue list from.
