# SceneTools component fields (tooltip text)

Field names and their in-editor tooltip help from the modder-kept MPN_SceneTools_Testing repository (`Assets/Code/{Addons,SceneObjs,Setup,Widgets}/*.cs`), harvested 2026-11-12 (round g, items 83-99). That repo may lag or lead the live SceneTools: check the Inspector before relying on a name. Everything here is unverified in game. Object overview and hard rules: [scenetools-rules.md](scenetools-rules.md); Event actions: [event-actions.md](event-actions.md).

## Destructibles (CustomObj_Destructible)

- Toughness: `Health` (hit count when `DamageIrrelevant`), `ArmorValue`, `RangedDamageMult`, `WeaponSerialNumber` (only these weapons damage it), `AllowSources` / `AllowTypes` / `AllowFactions` (true = whitelist, false = blacklist), `failedShotsPassThrough`.
- Damage looks: `DestructionStates` (objects swapped one at a time as it takes damage; with 2+ the last is full damage), `FlashColorOnHit`, `HitEffect`, `CompletionEffect`, `Explosion` (an Explosion MadCard) at `ExplodePoint`, `FailSayLine` (spoken when it cannot be broken).
- Bar: `ShowDamageBar`, `BarAlwaysOnScreen`, `BarFillIsBad` (red).
- Regeneration: `Heal`, `Heal_Wait`, `Heal_Speed`. `ResetOnNewTask`.
- Navigation and targeting: `ResetPathNodes` (recalculate room nav when it opens a path), `SiegeTargetCollider`, `AimTarget`, `IconPoint`, `HitLocations`.
- Task visibility: `EnabledWhileTask`, `DisabledWhileTask`, `EnabledWhileLocked`, `EnabledWhileUnlocked`, `LockedTaskIgnoresEnabled`. `TaskEvent` runs its EventCard with Conditions skipped.

## Rooms and blockers

- RoomObj `LeaveLockRules`: Default (stages lock doors while enemies are present, hubs do not), Ignored, Forced. Also `ForceLockout_PrevRoom`, `DisableLockout_PrevRoom`, `SoftDeactivate` (disable instead of destroy on leaving; the tooltip says avoid it).
- `Blocker_Progress`: `LinkedCharacters`, `LinkedSquads` (all must die).
- `Blocker_Backtrack`: `TriggerArea` (put a Zone_Spawner on the same collider to spawn enemies on trigger), `CameraStopper`, `LinkedOffLimits` (a Zone_OffLimits failsafe against clipping past).

## Zones

- `Hazard_WaterZone`: `Footprints` (colour of wet footprints after leaving), `Buoyancy` (lower sinks faster), `LinkedDeadZone` (after about 3 s the character is sent there as if they fell).
- `Zone_Camera`: `AmActive` (off = waits for `CameraAreaOn`), `Priority`, `CameraPoint`, `TransitionFrames`, `StationaryModifier` (0 fixed, 1 follows exactly), `Influence`, `InfluenceByDistance` + `InfluenceDistanceStart` (soft blend in), `AxisMult_X/Y/Z`, `DistanceCutoff`, `CutoffTracksAspectRatio`.
- `Zone_Event`: `PlayerOnly`, `TrackPlayer` / `TrackSerialNumber` / `TrackFaction` (that priority), `Amount` (entrants needed, min 1), `TrackOutsideArea` (fires on leaving), `DisplayTimer`, `TimerName`, `TimerLength` (frames), `TaskEvent` (skips Conditions).
- `Zone_GetOut` (NPC flee volume): SafeZone or DangerRange, `ForgetMyTarget`, `AffectAllStates`, `NPCsMustBeAware`, `ClearOrdersInMe`, `IgnoreHasOrdersToLeave`.

## Vessels and seats

- Vessel: `DriverCard`, `LinkedDestructible` (its health), `PathPoints` cycled by the `WaypointTick()` / `WaypointSet()` animation events (VesselAnimationEvents), a `FindTarget()` event that uses `Faction`, `LinkedAnimators` (take DriverCard parameters), `FloorReference` / `FloorCollider` (lock or snap Y), `EnforceUpY`, `CollidersIgnoreUser`, `PlayerUserFocalPoint` (camera focus while P1 drives).
- Seat: `EnterRange` must be above 0 to be enterable; `EnterAnim` (custom clips need an `EnterComplete` event), `IdleLoop`, `EnterPoint` / `SitPoint`, `EnterText` / `ExitText`, `LeaveOnAnyAction`, `AmExitLocked`, `WalkAwayOnExit`, `AllowedSerials` (only these characters), `EnabledOnOccupied`, `DriverCard` (NPC) and `DriverCard_Player` override the Vessel's card.

## Arenas, sieges and waves

- SceneRules: `EventCards` (auto-loaded by filename), `ArenaStartButton` (waves wait for this Interactive), `LockedDuringWaves`, `SiegeTargets`, `SiegeTargetsTilFail`, `SiegeTargetWarnings`, `WaveSound_Volume`, `EndCutscenes` (the first one found plays on arena victory; remove the others with Removers).
- `Zone_StagingGround` (siege conscripts): `RequiredOccupants`, `FullAttacksOnly`, `JoinExistingSquad`, `CoordinatedZones` (attack together), `StandPoints`, `CoverPoints`, `DefendArea`, `StealOccupants`, `myFaction`, `SiegeWall` (attack only after this Destructible or Interactive goes inactive), `SiegeWallAttackerCap`, `ShortcutWall` with `Walk_/Sprint_ShortcutUp/Down` (WalkOrder; the Hard Sell garage doors are the example), `Walk_Defense`, `MarkAttackers`, `SiegeSquadList` + `SiegeSquadGear` (extra enemies from `SiegeSquadDoor`), `DelaySpawnAfterAttack_Seconds`.
- `Marker_Squad`: `WaveSpawnType` (NewTandem, AddToMatchingFaction, MissionWaves with `StartText`), `NeverendingWaves`, `SpawnFrequencySlowdownMult`, `IgnoreFactionAllyCount`, `MaxSquadSize`, `MaxUnitCount` (on-screen cap; an empty screen always spawns), `WeaponStock` + `WeaponStock_RandomSeed` (same random weapons every play), `WeaponDrafts` by spawn number (IgnorePref, Melee, Ranged, ForceUnarmed).

## Characters, bosses and dummies

- `Marker_Character`: `MadCard` (overrides the picker), `Worn`, `Keycard`, `AmEssential` (less execution damage, no fall-break, no decay; a Condition target), `ToPlayerSquad` (tooltip: unsupported; needs a single Player squad), `PermanentSquadmate`, `SpawnEnabled` (off = wait for Zone_Spawn or an Event), `Alertness`, `SpeakLine` (said by the first into the room), `PathPoints` (blank entries = wait here), `GenerateReturnPath`.
- `Marker_Boss`: `StaggerDeath`; per pattern `ConstantModules`, `RemoveModules`, `PhaseModules`, `OneShots`, `PhaseWaypoints` (only Gil's module uses them so far), `CompletionEvent`. AI modules themselves are code (madnesswright-modtools).
- `Marker_Dummy`: `AdditionalArmor`, `HeldInHands`, `ReplaceAnims` / `ReplaceAll` (placeholder clips swapped for game clips of the same name), Mecanim int `PlayState` from `PlayStates`, `StartStates`, `WinceStates`.

## Removers and randomizers

Removers make a hub change after a stage is beaten without Events. The author called Removers untested (round f, item 78).

- `Remover_WorldChange`: `StagesRequired`, `StagesForbidden`, `StagesPlayed`, `RequiredChanges`, `ForbiddenChanges`, `DisableOnly`.
- `Remover_WaveCheckpoint`: `Imprint_Min/Max`, `Checkpoint_Min/Max` (-1 = none), `RemoveIfTrue`.
- `Remover_Difficulty`: `RemoveObjectOnTheseDifficulties`, `AffectTheseInstead`.
- `Remover_Origin`: `Required`, `Forbidden`, `InvertOnSecondPlay` (first-play-only content; Wizard Origin uses it).
- `Remover_Random`: `RandomChance` (default 0.5). `Remover_ArenaUpgrade`: `myUpgrade`, `MinLevel`, `MaxLevel`.
- `Object_Randomizer`: `RandomizedObjects`, `EnableCount`, `RandomizeOnce` (false = re-roll on every enable).

## Hazards and effects

- `Hazard_Core`: `damageEffect` (NoKill, Standard, IgnoreThreshold, NoKillPlayer), `armorPierce`, `bounceAway`, `UseBounceDir`, `DoT` (a DoT MadCard), `HitAvoid` (AttackQuality; interacts with dodge i-frames), `myHitFrequency` (Always, OncePerFrame, OnceTilReset), `IgnoreFactions`.
- `Hazard_Splosive`: `Explosion` MadCard, `Unbreakable`, `myRemains`, `Debris` (Obj_Debris list), `VelocityExplosion` (impact speed), `FramesBeforeSplode`, `BoutToExplodeObjects`, `flashOnTrigger`, `ignoreInvincible` (the crate on the Sheriff's head).
- `Effect_DebrisGenerator`: `CreateList`, `CloneDebris`, `DecayDebrisOnRest`, `RandomizeRotation`, `ExplosiveForceFromMe`, `ScaleDebrisToLocalScale`, `ConvertToDebrisLayer`.
- `Effect_RendererVanisher` (hide roofs and upper floors): `VanisherCeiling`, `VanishWhenBehindPoint`, `VanishWhenBelowCeiling`, `VanishForever`, `InvertChecks`.
- `Effect_MaterialScroller` (conveyors, water): `Shift` (seconds per loop per axis), `Properties` such as `_MainTex`, `_CubeMap`, `_BumpMap`, `ValidMaterials`.

## Lockers and vendors

- Locker: `Contents` (MadCards; blanks space the Stick Points), `AssignedSerialNumbers`, `StartOpen`, `StartSpawned`, `ExtraMags` (Story), `CloseOnEmpty`, `RespawnOnClose`, `DespawnPreviousOnRespawn`, `WeaponsAreQuest`, `ItemStickPoints`, `DropPoint` + `ExpelForce` (Proxy Lockers ignore both), `PowerupValue` (cash amount).
- Vendor addon: `Name`, `Type` (MenuTypes), `Stock`, `AmShoppingList` (infinite stock), `EnforceTier`, `TabPriority`, `StartFilter`.
