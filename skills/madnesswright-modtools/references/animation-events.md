# Custom animation clips: pipeline and Animation Events

Source: the official Custom Animations Google Doc (parts 1 to 4), read 2026-11-12 for [the technique harvest](../../../ai-docs/notes/2026-11-12-madness-technique-harvest.md) (items 20 to 22). The doc is a draft: names below are as it states them, but **none of this is verified in game yet**. Before telling the user a custom attack works, have them play one swing that carries the four core melee events.

## Pipeline (item 22; stated, but terse)

- Rig: the M:PN Blender Sample Project armature, made in Blender 2.79. Do not change the armature; the doc warns "problems will ensue".
- Export `.fbx`, never `.blend`. Mind the Sync and Root nodes.
- Scale 0.45 in Unity. Two community guides (2026-11-12b harvest, item 33) say this is the importer's Scale Factor for Blender 2.8+ exports with Convert Units OFF (Blender 2.79: Scale Factor 45, Convert Units ON); untick Import Visibility, Cameras, Lights and Preserve Hierarchy for clips. See [porting.md](porting.md). Unverified in game.
- Events are added to the clip in the ModTools project (Unity 2019.2.6f), then built as in SKILL.md Step 3.
- Clip types: Sync Anim (two characters, e.g. grapples and executions), Attack (Warmup, Swing, Hit, End, Sweep) and Entrance.
- Part 3 of the doc (the Animations MadCard format for new attacks, grapples and executions) is a placeholder only: there is no documented way yet to register a new attack by card. Say so rather than guessing a format.

## Animation Events (item 20; stated, draft doc)

Clips take event nodes of two kinds, "Run Events" and "Method Events"; they behave the same, and the doc lists only Method Events. Parameters are the event's INT, FLOAT or STRING field.

A custom attack without `H2H_Warmup`, `H2H_Swing_*`, `H2H_Hit_*` and `H2H_End` at the right frames will not deal damage or chain into the next swing.

| Group | Event | What it does |
|---|---|---|
| Melee | `H2H_Warmup` | pull-back; INT 0 right hand, 1 left hand |
| | `H2H_Swing_M` | armed swing starts; the counter check happens here |
| | `H2H_Swing_U` | unarmed swing; INT 0-4 = right hand, left hand, right foot, left foot, headbutt |
| | `..._Heavy` variants | heavy versions of the swings |
| | `H2H_SwingEnd` | swing ends |
| | `H2H_Hit_M` / `H2H_Hit_U` | the strike frame |
| | `H2H_Stab_M` / `H2H_Stab_U` | strike with Piercing damage |
| | `H2H_Bite`, `H2H_Kick` | bite; kick with special knockback |
| | `H2H_Sweep`, `H2H_SweepUnarmed`, `H2H_SweepEnd` | hit everyone in the arc (heavy attacks sweep automatically) |
| | `H2H_DashAttack` | INT 1 weak, 2 strong |
| | `H2H_Advance` | FLOAT speed push toward the target or cursor |
| | `H2H_End` | where the next combo swing starts (ignored if the player does not continue) |
| Ranged, sync | `ShootEvent`, `ShootEvent_Head`, `ShootEvent_Body` | hit the sync partner; fails outside a sync anim (the doc says `_Body` "targets their head", a copy slip) |
| | `ShootEvent_Sourced_ByName` | fire as a named MadCard_Ranged or Thrown |
| | `WeaponEvent_Degrade` | INT damage to the weapon's condition |
| | `H2H_GrappleSynch_ByName` | start a grapple with a named Animations MadCard (v2.13.i) |
| | `ExecuteEvent` | STRING execution category |
| Spawning | `SpawningComplete` | release the entrance early |
| | `EnterComplete`, `TriggerEntranceAddonEffects`, `SpawnEndUpdateRoot` | entrance bookkeeping |
| | `EndAnimUpdateRoot` | stop the Hips sliding back when a clip ends away from its start |
| Sound, voice | `SoundEvent` (FMOD event name), `SoundEvent_Hands` | sounds |
| | `VoiceEvent` | INT 0-9: Angry, Attack, BigAttack, Cheer, Laugh, Grunt, Death, Pain, Affirmative, Dialogue; `VoiceEvent_20` plays at 20 % |
| | `WeaponSoundEvent` | INT 0-5: Equip, Stow, Drop, Block, Hit, Swing |
| | `JumpEvent`, `Footstep`, `LandEvent` | movement sounds |
| Misc | `AnimEvent` | play another clip |
| | `GibMe` | gib the character |

## Effects fired from animation frames (item 21; stated)

Boss entrances, suicide units and ground slams without Harmony code:

- `CreationEvent_ByName` spawns a ParticleSystem or ModTools GameObject at the feet; variants `_Center`, `_Head`, `_Chest`, `_HandR`, `_HandL` and `_Center_ParentCapsule` (parented to the character's collider) (v2.13.i).
- `SplodeEvent_ByName`, `SplodeEvent_Friendly_ByName`, `SplodeEvent_Friendly_Hand_ByName` create a named MadCard_Explosion (see madnesswright's [madcards.md](../../madnesswright/references/madcards.md)); the Friendly ones spare allies.
- `SlamEvent`, `SlamEvent_Sourced`; `SplodeDeath` removes the character and explodes in its place; `GibSplode`, `GibEvent`.
- `CamShakeEvent` (FLOAT 0-25), `SlowMoEvent_PlayerSquad`, `ChatterEvent` (INT 0-9 chatter categories), `SpeakEvent` (STRING custom line).

All unverified in game: test each effect once in a Playground before building a mod on it.

## Exporting a clip from Blender (item 34 of the 2026-11-12b harvest; community, G2, unverified in game)

- NLA editor: push the action to a named track on Armature_Main. Do not save the .blend in that state, or you will need a fresh rig file.
- Export FBX with no object limit, Apply Scalings "FBX All", -Y forward, Z up, Add Leaf Bones off, Animation on with All Actions off.
- G2 agrees with the doc on event kinds: Method Events use the Function field, Run Events the String field; Float and Int are the parameters.
