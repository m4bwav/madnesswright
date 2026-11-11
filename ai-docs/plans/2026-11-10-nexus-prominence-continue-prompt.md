# Continue: Nexus Prominence map template

1. Read ai-docs/HANDOFF.md (top entry and Owed 24), then examples/NexusProminence/README.md and the madnesswright-scenes skill.
2. Waits on the owner: enabling the NexusProminence mod in the gear menu and playing it (built and deployed 2026-11-10).
3. Order: after the owner plays, read `mpn.py log` and their notes; fix placement faults in the generator, rebuild headless (`NexusProminenceBuilder.Build`, then `NexusBatch.SyncAndBuild`), copy the .sobj, `check-mod`, `deploy`; repeat the README checklist.
4. Stop when the player has walked all ten rooms and `mpn.py log` is clean, or when a fault needs the owner's eyes in the editor.
5. Hand over: update HANDOFF.md and the log, then print the same start command: `Execute ai-docs/plans/2026-11-10-nexus-prominence-continue-prompt.md` in the madnesswright folder.
