# Nexus Prominence: overlap scan and artwright look pass (2026-11-11)

## Overlaps (owner's room 2 shot: "struts going over the doors")

Cause: `Shell` lays a Pilaster every 7.5 units along the back wall, plus pipe clamps, conduits, band stripes and
diagonal stripes, with no check against where each room puts its doors, posters, cases and windows.

Fix: `ClearWallOverlaps` runs after each room is dressed. Filler that crosses a feature or a real door opening is
removed (`NX_CLEAR` lines in the build log), feature-vs-feature overlaps are logged (`NX_OVERLAP`), and each room's
boxes are dumped to `MPN_SceneTools/NexusBoxes/<room>.json` for polywright `pw.py overlaps`.

- build-z: 62 filler pieces removed over 10 rooms. Logged and then fixed by hand in build-za: room 3's painted door
  inside the back window (now behind `FakeDoors`), crates in a room 4 door opening (`Crate` now skips door spots),
  room 7's slate over a banner (that banner skipped).
- Left as is: room 1's sign rods pass the window header (the sign hangs from it), room 3's file cabinets touch the
  window sill.
- Side effect to check in play: room 1's window lost its vertical mullions (pilasters crossing the window).

## Look (artwright-judge, sheet triage plus `aw.py light`, renders shots-zb)

A triage of the sheet, not a ranked verdict. Proposals, not applied (owner rejects large unplayed look changes):

1. The lower half of every room is empty floor (also the owner's earlier note). Cheapest: cover or debris in the
   front third, kept out of the walking line to the doors.
2. Every room repeats the same red band, skirting and chevron banners, so the rooms run together. Vary the band
   per room (colour value, height) and drop the banners where a room has its own feature.
3. `aw.py light` flags rooms 2, 6 and 8 `low_value_gap` (flat values). Add one bright accent per room
   (lit sign, window, lamp) as rooms 1, 7 and 10 already have.

Applied 2026-11-11 (build-ze): proposal 3 only. Pale `lit` accents: a lightbox over each of room 2's back doors, a lamp bar under room 6's shutter box (reads thin), a strip light over room 8's lockers. Render Ai/madnesswright-lab/nexus/compare-ze.jpg. Proposals 1 and 2 wait for the owner's play of build-ze.

Applied 2026-11-11 (build-zh): proposals 1 and 2 together, on the owner's ask for another round before the test. Render Ai/madnesswright-lab/nexus/compare-zh.jpg. All three proposals are now in; the next look ideas come from the owner's play.
