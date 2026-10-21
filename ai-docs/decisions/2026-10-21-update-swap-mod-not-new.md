# Grow Swap And Unaware instead of starting a new mod

Date: 2026-10-21. Status: decided (small ruling, flagged to the owner).

## The request

Eight new keys for the same player who uses Swap And Unaware: F1 clear the room, F2 freeze, F3 patrol paths, F4 idle animations, F9 guard, F10 alert cycle, F11 save and load scenes. The requester asked that F5 to F8 stay as they are.

## Choice: one mod, split into one component per feature

The new keys go into the existing mod (Workshop item 3822329165), version 0.4.0, under a new display name, **Playground Director**, with "(was Swap And Unaware)" in the Workshop title so the subscriber still recognises it.

- The requester already subscribes to that item and wants F5 to F8 kept: one item means one thing to subscribe, enable and update, and no key fights between two mods polling the same F keys.
- The folder name, DLL name and HarmonyID stay `SwapUnaware` and `com.m4bwav.mpn.swapunaware`, so `Mods/enabled.txt` and the Workshop item keep working without re-enabling.
- The risk that argued for a new mod (a crash in a new feature breaking the swap he likes) is handled in the code instead: each feature is its own `MonoBehaviour` on the mod's host object with its own try/catch, so one throwing every frame cannot stop the others. Shared helpers (picking the character under the mouse, the claim fixes from L-002) move to one static class.
- The 233-line behaviour would have grown past 1,000 lines; the split keeps each file to one feature.

## Rejected

- A second mod ("Swap And Unaware 2"): two Workshop items to keep in step, duplicated picking and claim code, and both would need to be enabled.
- Renaming the folder or HarmonyID: the game keys enabled state on the folder, and a local copy and the Workshop copy would both load.
