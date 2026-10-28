# Infinite ammo and spawning any weapon: how the game does it (build 25615196)

Read from a fresh decompile of beta build 25615196 (Beta v2.20.a) on 2026-10-28, for Director 0.11.0. The build had not changed since 2026-10-18 (`appmanifest_488860.acf`, branch `releasebeta`). Member names only; no game code is copied here. Nothing below has been seen in play yet.

## Ammo

- A gun's rounds are `Item_Ranged.myAmmo` (loaded), `myAmmoLimbo` (mid-reload) and `myExtraMags` (spare magazines on the weapon itself). Firing (`FireRound`) calls `SpendAmmo`, which takes `myAmmoSpend()` rounds: `ReturnAmmoSpend_Owner` returns the owner's `buff_InfAmmo`, `ReturnAmmoSpend_Ownerless` returns 1.
- Reloading (`TakeAmmoFromStockpile`) takes one of the weapon's own `myExtraMags` first, then a free one when the card has `InfiniteReloads` (`myData.myInfiniteReload`), then one from the owner's pool through `Combatant_Base.SpareMag_Spend`. Whether a reload may start: `Combatant_Base` checks `CanReload()`, `ReturnCanBeReloaded()` and `inRanged.HaveAmmo(this)`, which falls back to `SpareMag_Have`.
- **The game has two per-character infinite-ammo flags.**
  - `Combatant_Base.infAmmo` (bool), the `InfiniteAmmo` trait. With it, `SpareMag_Have` and `SpareMag_Spend` always return true: magazines empty and reloads play as usual, but the spare pool never runs out. Thrown weapons restock through the same `TakeAmmoFromStockpile` path, so grenades never run out either. A single-use weapon (`destroyOnAmmoEmpty`) resets its ammo instead of despawning. Melee durability is not touched.
  - `Target_Object.buff_InfAmmo` (int, default 1), the arena's Infinite Ammo powerup (`Buffs_Manager.Buff_InfiniteAmmo_Start` sets 0, `_End` sets 1; `Item_Powerup_InfiniteAmmo`; arena bribed allies get 0). It is a multiplier: 0 means a shot spends no round (no reloads at all), and melee breakage (`CheckBreakOnSurface(..., n * buff_InfAmmo)` in `Combat_Manager`, `Combat_CharacterHit`, `Combatant_Base`) never wears the weapon.
- `infAmmo` is recomputed from the character's traits by `Combatant_Character.SetTraitBools`, run by `ApplyAllStats` (spawn `Init`, `DeathEvents.ApplyToCharacter`, the arena shop's `UI_PlayerMenu.PressBuy_PostEvents`) and by `Controller_Boss` on each new pattern. A mod that sets it must set it again each frame, and on turning it off should restore `HaveTrait(TraitList.InfiniteAmmo)`.
- No developer-console command, difficulty setting or `MadCard` stat for infinite ammo was found besides the trait and the per-card `InfiniteReloads`.
- Both flags live on the body, so a controller swap (F5, `AssignController`) leaves them with the body.

## The weapon catalog

- `StatCard_Base.RegisterAllUnmoddedCardCatalogs` loads every weapon card from `Resources/Stats/Weapon/<category>/` for each `WeaponCategories` value but `None` into `RegisteredUnmoddedCards[typeof(StatCard_Weapon)]`.
- `StatCard_Base.ReturnAllWeapons()` returns those plus the mods' new cards of type `StatCard_Weapon`, `StatCard_Ranged` and `StatCard_Thrown` (`Converter_SC.ReturnAllNewModCardsByType`). Override cards replace a card by name: call `OrOverride()` on each (as `Catalog_Weapons.CatalogAllWeapons` and `Market_Main.DefineItemListsByTier` do).
- Card tree: `StatCard_Item` > `StatCard_Weapon` (melee; `Category`, `Traits`) > `StatCard_WeaponStats_Base` > `StatCard_WeaponStats_Additional` > `StatCard_Ranged` / `StatCard_Thrown`; `StatCard_WeaponModded : StatCard_Weapon` (a ranged weapon with preset addons, through `LinkedWeapon`).
- Category data: `StatCard_Weapon.Category`, enum `WeaponCategories` Pistol 0, Revolver 1, SMG 2, Shotgun 3, Rifle 4, Heavy 5, Melee 6, Unarmed 7, Thrown 8, Shield 9, Wand 10, None 100. Also `Tier`, `ForSale` (`StatCard_Item.Store`: Quartermaster, Blackmarket, Hairstyles, Arcane, ArenaWave, Fence, NONE), `ItemEnabled`, `FullName`, `AmNewCard` (a mod's card).

## The Playground's spawn menu and what it hides

- `InteractiveMode.Event_SpawnWeapon` opens `CreateMenu_WeaponCategory` (a `UI_MenuButtons` list, pauses the game), then `CreateMenu_SpawnWeapon` lists the category's weapons with two buttons: "Mine!" (`WeaponSpawn_SpawnMe`) and "Stage" (`WeaponSpawn_SpawnStage`, at a random `Spawn_Point` or `Path_Node`, thrown with some velocity).
- Its list (`InteractiveMode.Start`): `ReturnAllCardsByType<StatCard_Weapon>()` filtered to `ItemEnabled && TierValidForStore() && ForSale != Arcane`, sorted by `SortCards` (mod cards first, then by asset name). Categories `Unarmed` and `None` are removed from the category menu.
- So it hides: disabled cards (`ItemEnabled` false), tiers outside 0 to 100, Arcane (wizard shop) weapons, the Unarmed category, cards whose category is None, and **mods' new ranged and thrown weapons** (`ReturnAllCardsByType<T>` looks the mod cards up by exact type, so only mod cards of type `StatCard_Weapon`, melee, appear).
- Arena shops (`Market_Main`) list cards with `ForSale` other than NONE and ArenaWave; arena waves use `Catalog_Weapons.allWaveWeapons_*` (Quartermaster, Blackmarket, ArenaWave in tier 0 to 100).

## Putting a weapon into the world or a hand

- Data first: `Catalog_Weapons.GenerateWeapon(card)` (`card.GenerateMe()`), or `GenerateHeld(StatCard_Item)` for any item.
- On the floor: `Item_Held.CreateHeld(data, pos, rot, room)` (`ItemDrop(room)` when a room is given).
- In a hand: `Char_DataAssigner.GiveItemToCharacter(data, body, slot, hand)` (the Playground's "Mine!" with slot 0, hand 0) and `GiveThrownToCharacter` for a grenade-slot thrown weapon (`AmType_Thrown() && amGrenadeSlot`). `GiveItemToCharacter` creates the item with no room, then `CollectHeld`. `Combatant_Character.CollectHeld` drops what the hand held, but returns without collecting when that item cannot be dropped (`CanBeDropped` false), which leaves the new item unowned and roomless; check the owner after the call.

## What would break when spawned (from the code)

- A card with no prefab (`StatCard_Entity.myRef` null): `GenerateRefObj_GameObject` instantiates null and throws.
- A `StatCard_WeaponModded` with no `LinkedWeapon`: `GenerateWeapon_Modded` returns null and its `GenerateMe` then dereferences it.
- Unarmed-category items are handled as ordinary held items (`Item_Held` lengthens their melee reach); nothing in the code marks them unsafe. Disabled (`ItemEnabled` false) cards are not guarded anywhere either; which ones are unfinished cannot be told from code.

## Keys

- `decomp.py keys` (2026-10-28): Backslash, Pause, the number row, the keypad and B, H, N, U, V, Z are never read by `KeyCode`, not bound by default (`rebindable_defaults`), and no `GetKey("...")` string reads exist. F12 is read only by `Dev_Control` (and is Steam's screenshot key). Director takes F1 to F11, INSERT and CAPS LOCK.
