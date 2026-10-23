# Books track 9: game-design rules for levels and weapons

Date: 2026-10-23. Status: decided (small rulings, flagged to the owner in the PR).

Source: the owner's reading notes on Rogers, *Level Up!* (3rd ed.), section "madnesswright". Everything is written fresh; the book is credited where a rule comes from it.

- **M1. Roles per card, no mapping table.** The notes asked to check the MPN enemy-to-archetype mapping against the game's cards before naming one. The vanilla cards are compiled into the game's bundles (StreamingAssets holds only FMOD banks and two test bundles on build 25615196), so no mapping could be checked. The skill says to watch each card alone in a Playground arena and write the role seen. Eval runs did that and marked their guesses as unconfirmed.
- **M2. No `mpn.py plan-lint`.** A lint of LEVEL-PLAN.md would only check numbers the planner typed in, no real plan files exist to calibrate a flag on, and `mpn.py` is already over its green token line. The limits went into the template's column headers instead (scenes L-002).
- **M3. Icon to 512 px.** The tracked repository was at 1,500 of 1,536 KB. The 1024 px icon (745 KB) became 512 px (283 KB), as brandwright and fetchwright ship. Now 1,055 KB.
- **M4. Heuristics marked.** "3 to 5 minutes per arena round" is marked as a heuristic to check against playtest times. "Two or three new things per stage" stays as the book's rule of thumb.
- **M5. Five roles in five waves.** When a brief has more new roles than waves, the plan says so and offers another wave or a move to the next stage, instead of debuting two in the finale (T-20261023-1).
- **M6. Attack matrix stays in the reference.** It is a step before modelling, not a file the mod ships. No version bump: changes sit under Unreleased; the owner tags.
