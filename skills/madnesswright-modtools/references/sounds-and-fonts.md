# Custom sounds, music and fonts

Source: two community Steam guides read 2026-11-12 for [the technique harvest](../../../ai-docs/notes/2026-11-12b-madness-technique-harvest.md) (items 27, 28, 31): G1 "Modder's Guide to Madness [UPDATED]" (2664043208) and G2 "The Definitive M:PN Modding Guide (INCOMPLETE)" (3305980802). Community steps, **unverified in game**. Sounds already in the game need none of this: name their `fmodPath` or `SoundEvent` as usual.

## Sounds through FMOD banks (item 27; community, G2, G1 agrees FMOD is needed)

1. FMOD Studio **1.10.11** with the community FMOD sample project (linked from G1/G2; the user downloads it).
2. Import each sound as an event: 3D for effects and voice lines, 2D for music. G2: a 3D sound made as 2D still works, not the other way round. A multi-instrument picks one of several takes per play.
3. Assign the events to a new bank, then File > Save, File > Export GUIDs, File > Build.
4. Copy the Master bank, the Master strings bank and your bank into the mod folder, loose (no subfolder).
5. In MadCard Maker make a SoundPack card (melee weapon sounds) or a VoiceSet card (character voice) and paste the GUIDs from the exported GUIDs text file into its slots.

Traps (G2): building more than one bank from one FMOD project makes the sounds stop playing; use a fresh project per extra bank. If nothing plays, delete the project's Metadata folder and rebuild.

## Music (item 28; community, G2)

As above, but the event is **2D**, sits in a folder named after the artist, and has a loop region (right-click between the time bar and the waveform). It then shows in the Playground Radio and can be set as stage music on a Stage_Arena card (madnesswright-scenes, stages-and-campaigns.md). Mastering and loop seams: musicwright-game if installed.

## Fonts (item 31; community, G1 and G2 agree)

- TrueType `.ttf` only, not `.otf`; a font with OpenType variations shows only its first one.
- Put it in the ModTools project's Fonts folder, add it to an AssetBundle, build.
- MadCard Maker cannot set a font: add `"Font": "<font file name without extension>",` by hand in the Chatter card's raw .json (mind the comma). `"Style"`: 0 regular, 1 bold, 2 italic, 3 bold italic.
- Rich-text tags and name tokens for the text itself: madnesswright references/madcards.md, "Text".
