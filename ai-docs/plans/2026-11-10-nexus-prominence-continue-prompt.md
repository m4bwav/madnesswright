# Continue: Nexus Prominence map template

1. Read ai-docs/HANDOFF.md (top entry and Owed 24), then examples/NexusProminence/README.md and the madnesswright-scenes skill.
2. Waits on the owner: a play of the 2026-11-11 rebuild (renders in examples/NexusProminence/renders/) and the OK to update the Workshop item; also the requester's test of Workshop item 3837705778 and their notes. Fixes update that same item (wsup.exe with the item id and a change-notes file).
3. Order: after the owner plays, read `mpn.py log` and their notes; fix placement faults in the generator, rebuild headless (`NexusProminenceBuilder.Build`, check looks with `NexusShots.Render -shotDir <dir>` against the frames in Ai/madnesswright-lab/nexus/frames, then `NexusBatch.SyncAndBuild`; the .sobj lands in LocalLow/Gibbing Tree/Madness Project Nexus SceneTools/SceneObjs), copy the .sobj, `check-mod`, `deploy`; repeat the README checklist.
4. Stop when the player has walked all ten rooms and `mpn.py log` is clean, or when a fault needs the owner's eyes in the editor.
5. Hand over: update HANDOFF.md and the log, then print the same start command: `Execute ai-docs/plans/2026-11-10-nexus-prominence-continue-prompt.md` in the madnesswright folder.
