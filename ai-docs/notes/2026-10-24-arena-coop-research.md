# Arena co-op: how arena runs, origins and player slots work (build 25615196)

Read from a fresh decompile of beta build 25615196 (Beta v2.20.a, unchanged since 2026-10-18) on 2026-10-24, session 16, for the request "both players start the arena and pick origins". Member names only; no game code is copied here. Nothing below has been seen in play. Background on pads and slots: [2026-10-24-two-player-research.md](2026-10-24-two-player-research.md).

## What an arena run is

- Arena is its own career type (`GameModes.Arena`), not a menu over the story save. A new arena career starts in `UI_CharCreator`: name, then an **origin**, then `Game_Manager.NewCareer(CareerInputData(GameModes.Arena, chars))` with exactly one character. The first run (fewer than 2 imprints, or an origin with `ForceBusRideOnFirstPlay` never played) starts in `Arena_0_Intro`; later ones in `Arena_HQ` (`Career_Data.ReturnDefaultStartScene`, `ResetStartScene_Arena`).
- **An origin is a card, `StatCard_Origin`**, applied once when the career's character is made: `Char_Data.ApplyOriginAndResetFromStats(origin, imprints)` sets the body card (`CharacterType`, built on the `"Arena"` character card), special traits (`SpecialSkills`), skill focus and weakness, attribute bonuses plus an imprint bonus (`UI_CharCreator.AttribBonusByOrigin`), buffs, `DefaultArmor` and `DefaultGear`. Origins also seed the career's warchest (`WarchestStarterGear`, `WarchestStarterWeapons`). The picker lists `StatCard_Base.ReturnAllCardsByType<StatCard_Origin>()` with `amEnabled`, greyed out while `ImprintsTilUnlock` is above the career's imprints + 1.
- **Imprints** are the arena's prestige: starting over with a new origin carries the career forward (`Career_Data.Imprints`, `ImprintCareer`, `OriginHistory`). So "pick an origin" happens at career creation or a new imprint, not before each arena.
- Before each arena the HQ's stage menu (`UI_StageMenu_Arena.Init`) shows the squad: `chars_Core` (player one) plus `chars_Favorites` (hired squadmates picked with the menu's add button, `UI_SquadSetup.Button_AddSquadmate`), capped by `MaxPlayerSquadSize` on the stage card and `Career_Data.ReturnMaxSquadmates()`. The difficulty and checkpoint are chosen there; there is no per-run loadout screen in the arena flow (loadouts are the characters' own gear from HQ).
- `Headquarters_Manager.LaunchLevel(stage, difficulty)` loads the scene and calls `Career_Data.ApplySquad(chars_Core + chars_Favorites)` (favorites skipped when `MaxPlayerSquadSize` is 0). The squad leader spawns from the stage's player spawner (`Char_DataAssigner.Spawncharacter`, controller type Player) and the rest of the roster spawns with it. Then `Career_Data.PostInitCareerSquad` marks each member `amEssential`, serial `PSQUAD<i>`, and death action `Stage_Manager.DeathEvent_ArenaPlayer`.

## Squadmates, and why arenas usually have no free body

- Hired squadmates (`chars_Hired`) come from the HQ. The squad cap is `HQ_Stats.Value_Hireling_SquadMax`: **0 until the world change `RescuePilot`**, 1 after it, 3 after `RescueChef`, 5 after `RescueFence`. A young arena career therefore brings player one alone, and a pad that presses Start waits in `Joystick_Manager.PlayersWaitingForAdd` forever: arenas have no `MultiplayerSquadExpand` door bonus.
- With a hired squadmate in the squad, the game's own co-op should already work in an arena: `AddPlayerToGame` gives a pressing pad the first squadmate that is not player-controlled. Not played.
- Hirelings are second-class in the arena code: `Controller_Player.CanBePlayerControlled` lets a player swap only between core characters (or clones with the same id), and a hireling who dies goes to the medbay for 900 s or is lost (`Career_Data.Hireling_KillMe`, after the run).

## Is there a half-built arena co-op path?

No. Searched `Multiplayer`, `PlayerControllers`, `PlayersWaitingForAdd`, `ReserveController`, `chars_Core` and origin types. The only multi-body player feature is the **Replicate** origin trait (clones of player one with the same unique id: `Arena_Manager.Clone_*`, `Stage_Manager.GiveXP_ArenaClone`). `UI_CharCreator` always builds a one-character career. The story's bonus-character recipe (`Joystick_Manager.PlayerSquad_AddWaitingPlayers`) is the closest working pattern: make a `Char_Data`, `ReserveController(Controller_Player.PlayerControllers[n])`, `Squad_Main.playerSquad.AddMember`, `Combatant_Base.CreateCombatant(data, room, DEFAULT, position, rotation, amDummy: false)`.

## Career data, rewards and deaths are player one's

- Cash, XP and tokens per wave (`Arena_Manager` wave end, `Stage_Manager.ApplyXPAndCashToPlayers`) go to every `chars_Core` character registered as an XP recipient (`RegisterCharacterAsXPRecipient`); hired squadmates get flat hireling XP. The cash bonus reads `chars_Core[0]`. Stage stats, checkpoints and tokens are per career (`StageSaveData`).
- `Arena_Manager` reads `Controller_Base.PlayerOne` for origin traits (Replicate, ImaginaryFriend), Summon Minions (paid from player one's cash), the coin holder and power-ups.
- No leaderboards in the code. Wave size does not scale with the number of players.
- **Death** (`Stage_Manager.DeathEvent_ArenaPlayer`): ignored for a character with no career or a hired one. For a core character the wave fails (`Arena_Manager.WaveFail`, no rewards, then the stage-end screen) unless another squad body is alive that has the same id, is also core, or is one the dying player could swap into. There is **no revive** in an arena: a dead squadmate stays dead until the run ends.

## What could stand in for a second origin picker

- Director already reads pads through the game (`Joystick_Manager.ReturnPlayerActive`, the controller's keys) and logs joins (`PlayersFeature`). A Director overlay can read the waiting pad's buttons through `Input.GetKey(KeyCode.Joystick<n>Button<b>)` and draw with IMGUI, as the Director menu does.
- Building player two: `Char_Data.CreateCharacter(Catalog_Characters.ReturnCardForType("Arena"), null)`, then `ApplyOriginAndResetFromStats(origin, imprints)`; `Career_Data.CheatCharacterLevel(data, level)` levels a character with origin-weighted skill purchases (a public static the game uses for test careers), so player two can match player one's level.
- Keeping player two out of `chars_Core` and `chars_Hired` (no `myCareer`) keeps the career save clean: `InitPlayerSquad` drops unknown ids from `IDs_Squad` at the next load, and the death rule ignores a character with no career, so player two's death never ends the wave.
- The Playground's spawn menu and Director's F5 take-over are not needed: the story's bonus recipe above spawns a player-controlled body directly.
