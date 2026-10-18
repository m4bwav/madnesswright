# MadObjects in ModTools

Source: official Modding Overview Google Doc, Parts 2, 5 and 7, and the MPN_ModTools repository, read 2026-10-17.

## Types

Characters, melee weapons, ranged weapons, ranged weapon addons, armor, body parts (`MadObj_BodyPart`) and full characters (`Character_Ref`, 2026-04). Particle systems, fonts, textures, icons and animations need no MadObject wrapper.

## Orientation

- Melee: blue Z forward, green Y toward the tip.
- Ranged: blue Z roughly the firing direction, Y up.
- Wrap the model in an empty parent with the `MadObj_` component; rotate the child to fix exported axes.

## Points and colliders

- Melee: two grip points (both hands) and the business-end extent. "Create Points" in the component's context menu makes the mandatory ones; optional points are added by hand. Hover a field for its tooltip.
- Resize the auto-added Box Collider to enclose the weapon; extra colliders refine the physics.
- Orientation points: copies of the grips to animate instead of the grips themselves.
- Stow Length Override: length used when stowed on the back.
- Bullet Override: a particle system with colliders fires instead of bullets (flamethrower); RoF then only drives ammo use.

## Weapon Animator

Add an Animator to the weapon, assign it on the MadObj, and build a new controller with only the states you need (the sample in `Library/Animation/WeaponAnim` is a demo). With an animator the slide and breach no longer animate by themselves.

- States the game drives: Base, Dropped, Stowed, Thrown, Attack, Empty, Breach, Reloaded, and Ammo (held on the frame matching remaining ammo or durability; keep it on its own layer).
- Parameters the game sets: AutoFire (bool), Warmup, Ammo, RoF (floats), CoreSkill (0 to 4), Hand (0 main, 1 off).
- Animation events: `Event_Casing(float)` with the NoCasings trait; `SoundEvent(string fmodPath)` and `SoundEvent25`, `50`, `75` for quieter playback.
- Addons on animated weapons are re-parented to the attach point's parent and renamed `Addon_<slot>` (Barrel0, Barrel1, Barrel2, Butt, Rail, Mag).

## Character_Ref

- Fill every Bone slot; the game hard-codes facing and hand behaviour around the Grunt's shape.
- Layers and tags: Character_Ref on Layer Character, Tag Character; `SubPart_CharacterBone` on CharacterHits, Untagged; `MadObj_BodyPart` on Character, Untagged.
- No controller assigned: the default character controller is used (recommended with vanilla animations, overridable on the card). Custom animations: copy `Library/Animation/Character/Character/Char_DefaultAnimator` and fill every state the character uses; never remove parameters.
- `MadObj_BodyPart` on each SkinnedMeshRenderer for head, hands, body, feet; head and body are mandatory. Two scripts on one mesh: drag the components themselves into the BodyParts array (Unity 2019 picks only the first otherwise).
- `SubPart_CharBone` on armature bones with their Rigidbody and Collider; several per type allowed (a second Chest as a weak point).
- DeathParts: Rigidbodies that drop as debris on death; for gore bursts use DeathEffects on the card.
- The armature root must start at 0,0,0 or the character spawns in the wrong place.

## Bundles

AssetBundle name at the bottom of the Inspector; `Madness > AssetBundle > >> Build (Compressed) *RECOMMENDED*` or Uncompressed; output in `%USERPROFILE%\AppData\LocalLow\Gibbing Tree\Madness Project Nexus ModTools\MadObjs\`. Cards reference the asset name inside the bundle, never the bundle name.
