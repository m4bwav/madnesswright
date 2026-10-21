# Plan: Playground Director (Swap And Unaware 0.4.0)

Date: 2026-10-21. Decision: grow the existing mod ([decisions/2026-10-21-update-swap-mod-not-new.md](../decisions/2026-10-21-update-swap-mod-not-new.md)). Game hooks: [notes/2026-10-21-director-mod-research.md](../notes/2026-10-21-director-mod-research.md).

## Rulings (decided, flagged once to the owner)

- "Do not make a button that is not F5, F6, F7, F8 and F12" is read as "do not take F5 to F8 or F12": every key asked for avoids them. F5 to F8 keep their jobs.
- No key moved: none of the requested keys is read by the game in normal play (decompile check). CapsLock is also autowalk; the mod reads it only while paused.
- "Playable" (spared by F1): controlled by a player, in the career's core roster, or a character whose card belongs to the Player faction (Sanford, Deimos, Hank and the other player cards).
- F1 removes the way the Playground's Clear All does (no corpses), keeping weapons and decals.
- F2 freeze also stops the animation; a frozen character still dies when shot, and the freeze lets go of it then.
- F3 patrols belong to the body you control when you place them; that body walks them whenever it is not yours and has not noticed an enemy (alert Unaware). The path loops; a red square is a stop. U places a new square, left click on a square selects it (yellow outline), Delete removes it, P makes it red (or black again). While patrol squares are shown, left click does not attack.
- F4 lists the character's own idle emotes first, then every looping clip in the game's library; a filter box narrows the list. The picked clip loops until the character notices someone; it does not wander off while posing.
- F9 toggles: on the character under the mouse it stands still (frozen in place, no wandering), goes unaware, and fights whoever it sees that is not on its team.
- F10 cycles by the character's current state: unaware goes to search, search goes to attack (you are the target), attack goes to unaware.
- F11 works only while paused, in the Playground. ENTER saves; CAPS LOCK lists; click loads; ESC closes. A scene holds every living character other than you (card, position, facing, squad and team, damage, weapons, frozen, guard, patrol) and your own position. Loading clears the room the way Clear All does, then spawns the saved characters. Scenes are tied to the stage they were saved on.

## Build order

1. Split the behaviour into one component per feature (`Feature` base with its own error guard), shared helpers in `Game`, an on-screen message line (`Hud`). Done.
2. F2 freeze, F1 clear, F10 alert cycle, F9 guard.
3. F4 idle animations (IMGUI list, attack blocked while open).
4. F3 patrols (IMGUI squares at the waypoints' screen points, a Harmony prefix on `Controller_Player.RunControls_Attacks` while the mod owns the mouse).
5. F11 scenes (JSON via `JsonUtility`, own pause source while the window is open).

Each step: `pwsh ./pack.ps1 -Version 0.4.0`, `mpn.py deploy --force`, then `mpn.py log` after the owner plays.

## Risks

- Freezing or guarding a body and then taking it over with F5: the mod lets go of a body when the player takes it.
- F1 in an arena kills wave units, which advances the waves; it is meant for the Playground.
- Patrols across rooms: `AssignWalkToPosition` only reaches the door; the mod re-issues the walk every second.
- Scene load spawns into `Cam_Main.currentRoom`; points from another room are skipped.
- Nothing here has run yet: every step needs the owner's playtest.

## Playtest checklist (for the player)

Start the Playground, spawn a few enemies and a friend or two, then:

1. F2: everyone but you stops dead. Shoot one: it dies. F2 again: the rest move again.
2. F1: every enemy and grunt disappears; you, Sanford, Hank and other player characters stay.
3. Point at an enemy, F10 three times: it searches, then attacks you, then forgets you.
4. Point at an enemy, F9: it stands still. Walk into its view: it shoots without moving. F9 on it again: it moves normally.
5. Point at an enemy that has not seen you, F4: a list opens. Click "typing" or another one: the enemy plays it. ESC closes the list.
6. Take over a grunt with F5. F3: a black square follows the mouse; left click drops it (no shot). U and click a few more. Click one square and press P: it turns red. Click another and press Delete: it goes. F3 hides them, F3 shows them. Press F8 to go home: the grunt walks the squares and stops at the red one. Take over someone else: the squares are not shown.
7. Pause (ESC), F11, type a name, ENTER. Unpause, F1 to clear. Pause, F11, CAPS LOCK, click the name: everyone comes back where they were.
8. F5 to F8 still work as before.

Tell us what broke, which key, and what you were doing; then we read the log.
