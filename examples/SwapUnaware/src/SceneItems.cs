using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SwapUnaware
{
    // Weapons and items in a scene (version 3, Director 0.7.0): each character's hands, holsters
    // and grenade with their ammo, your own loadout (Playground only), and every weapon or held
    // item lying loose in a room. Loading takes the loose ones out and puts the saved ones back.
    // Career weapons (registered to a story or arena career) and quest items are never touched.
    public partial class SceneData
    {
        public List<ItemRecord> playerItems = new List<ItemRecord>();
        public List<FloorRecord> floor = new List<FloorRecord>();

        [Serializable]
        public class FloorRecord
        {
            public string card;
            public Vector3 pos;
            public Quaternion rot;
            public bool hasAmmo;
            public int ammo;
            public float mags;
            public RoomRef room = new RoomRef();
        }

        internal bool HasItems { get { return version >= 3; } }

        // ---- saving ----

        // Hands (slot 0), holsters (slot 1) and the grenade slot. Items that cannot be unequipped
        // come with the character's card, so they are left out.
        internal static List<ItemRecord> Loadout(Combatant_Base c)
        {
            var list = new List<ItemRecord>();
            for (int hand = 0; hand < c.myItemsHeld.Length; hand++) AddItem(list, c.myItemsHeld[hand], 0, hand, false);
            var cc = c as Combatant_Character;
            if (cc != null)
            {
                for (int hand = 0; hand < cc.myItemsStowed.Length; hand++) AddItem(list, cc.myItemsStowed[hand], 1, hand, false);
                AddItem(list, cc.myItemThrown, 0, 0, true);
            }
            return list;
        }

        static void AddItem(List<ItemRecord> list, Item_Held it, int slot, int hand, bool thrown)
        {
            if (it == null || it.myData == null || it.myData.myCard == null || !it.CanBeDropped()) return;
            var r = new ItemRecord { card = it.myData.myCard.name, slot = slot, hand = hand, thrown = thrown };
            ReadAmmo(it, out r.hasAmmo, out r.ammo, out r.mags);
            list.Add(r);
        }

        internal static List<FloorRecord> CaptureFloor()
        {
            var list = new List<FloorRecord>();
            foreach (var it in UnityEngine.Object.FindObjectsOfType<Item_Held>())
            {
                if (!Loose(it) || it.myRoom == null) continue;
                var f = new FloorRecord { card = it.myData.myCard.name, pos = it.transform.position, rot = it.transform.rotation, room = Ref(it.myRoom) };
                ReadAmmo(it, out f.hasAmmo, out f.ammo, out f.mags);
                list.Add(f);
            }
            return list;
        }

        // On the floor, in flight, or still holstered on a body.
        static bool Loose(Item_Held it)
        {
            if (it == null || !it.gameObject.activeInHierarchy || it.AmDespawning || it.AmMenuObject || it.AmSecondaryObject) return false;
            if (it.myData == null || it.myData.myCard == null || it.myData.amRegistered || it.AmQuest()) return false;
            return it.myOwner == null || !Game.Alive(it.myOwner);
        }

        // Ranged weapons and grenades: rounds (loaded and mid-reload) and spare magazines.
        // Melee weapons: the durability the game calls melee ammo.
        static void ReadAmmo(Item_Held it, out bool has, out int ammo, out float mags)
        {
            var r = it as Item_Ranged;
            if (r != null) { has = true; ammo = r.myAmmo + r.myAmmoLimbo; mags = r.myExtraMags; return; }
            has = it.myMeleeAmmoMax > 0;
            ammo = it.myMeleeAmmo;
            mags = 0f;
        }

        static void ApplyAmmo(Item_Held it, bool has, int ammo, float mags)
        {
            if (it == null || !has) return;
            var r = it as Item_Ranged;
            if (r != null)
            {
                // ResetAmmo ends a reload and empties the rounds in limbo, which LoadAmmo would
                // otherwise load over the saved count once the reload finishes.
                if (it.myOwner != null) it.myOwner.CancelReload();
                r.ResetAmmo();
                r.SetAmmo(ammo);
                r.SetMags(mags);
            }
            else it.SetMeleeAmmo(ammo);
        }

        // ---- loading ----

        // A career's weapons belong to the career save; a character holding one keeps its loadout.
        static bool HasCareerItems(Combatant_Base c)
        {
            foreach (var it in c.myItemsHeld) if (it != null && it.myData != null && it.myData.amRegistered) return true;
            var cc = c as Combatant_Character;
            if (cc == null) return false;
            foreach (var it in cc.myItemsStowed) if (it != null && it.myData != null && it.myData.amRegistered) return true;
            return cc.myItemThrown != null && cc.myItemThrown.myData != null && cc.myItemThrown.myData.amRegistered;
        }

        // Empties hands, holsters and the grenade slot, then gives the saved loadout. Someone holding
        // a career weapon keeps the weapons themselves and only gets the saved ammo back.
        internal static bool ResetLoadout(Combatant_Base c, List<ItemRecord> items)
        {
            string given;
            return ResetLoadout(c, items, out given);
        }

        // given: the saved cards for the log, each failed one marked "(failed)".
        internal static bool ResetLoadout(Combatant_Base c, List<ItemRecord> items, out string given)
        {
            given = "";
            if (c == null) return false;
            if (HasCareerItems(c))
            {
                given = "career weapons kept, ammo reset";
                return RefillAmmo(c, items) > 0;
            }
            Strip(c);
            var names = new List<string>();
            foreach (var it in items) names.Add(it.card + (Give(c, it) ? "" : " (failed)"));
            given = string.Join(", ", names.ToArray());
            return true;
        }

        // Puts the saved ammo into each weapon still in the same slot with the same card; the weapon
        // objects stay, so a career's weapons are never swapped. A slot emptied since the save (the
        // weapon thrown or dropped) first picks that same weapon back up from wherever it lies, or
        // gets a fresh one if it is gone. Call it before RestoreFloor, which would clear the weapon.
        // Returns how many weapons it reset.
        internal static int RefillAmmo(Combatant_Base c, List<ItemRecord> items)
        {
            if (c == null) return 0;
            int n = 0;
            foreach (var r in items)
            {
                try
                {
                    var it = InSlot(c, r);
                    if (it == null)
                    {
                        var lying = FindLying(c, r.card);
                        if (lying != null)
                        {
                            if (lying.myOwner != null) lying.myOwner.ClearItemFromSlot(lying);
                            c.PickupItem(lying, r.hand, r.slot);
                            it = InSlot(c, r);
                        }
                        if (it == null) { Give(c, r); n++; continue; }
                    }
                    if (it.myData == null || it.myData.myCard == null || it.myData.myCard.name != r.card) continue;
                    ApplyAmmo(it, r.hasAmmo, r.ammo, r.mags);
                    n++;
                }
                catch (Exception e) { Game.Error("scene: ammo " + r.card, e); }
            }
            return n;
        }

        static Item_Held InSlot(Combatant_Base c, ItemRecord r)
        {
            var cc = c as Combatant_Character;
            if (r.thrown) return cc != null ? cc.myItemThrown : null;
            if (r.slot == 1) return cc != null && r.hand < cc.myItemsStowed.Length ? cc.myItemsStowed[r.hand] : null;
            return r.hand < c.myItemsHeld.Length ? c.myItemsHeld[r.hand] : null;
        }

        // The nearest weapon with this card that no living character holds, career weapons included.
        static Item_Held FindLying(Combatant_Base c, string card)
        {
            return UnityEngine.Object.FindObjectsOfType<Item_Held>()
                .Where(x => x != null && x.gameObject.activeInHierarchy && !x.AmDespawning && !x.AmMenuObject && !x.AmSecondaryObject
                    && x.myData != null && x.myData.myCard != null && x.myData.myCard.name == card && !x.AmQuest()
                    && (x.myOwner == null || !Game.Alive(x.myOwner)))
                .OrderBy(x => (x.transform.position - c.Position).sqrMagnitude)
                .FirstOrDefault();
        }

        // Item_Held.DeSpawn lets go of a held or thrown item itself; a holstered one is cleared first.
        static void Strip(Combatant_Base c)
        {
            var cc = c as Combatant_Character;
            if (cc != null)
                for (int hand = 0; hand < cc.myItemsStowed.Length; hand++)
                {
                    var s = cc.myItemsStowed[hand];
                    if (s == null || !s.CanBeDropped()) continue;
                    cc.RemoveItemFromSlot(1, hand, false);
                    s.DeSpawn();
                }
            foreach (var it in c.myItemsHeld.ToList())
                if (it != null && it.CanBeDropped()) it.DeSpawn();
            if (cc != null && cc.myItemThrown != null && cc.myItemThrown.CanBeDropped()) cc.myItemThrown.DeSpawn();
        }

        static Item_Data_Held Generate(string card)
        {
            StatCard_Item sc = WeaponSpawnFeature.Card(card);
            if (sc == null) sc = StatCard_Base.ReturnCardByName<StatCard_Item>(card);
            if (sc == null) sc = Loaded(card);
            if (sc == null) { Game.Log("scene: no item card " + card); return null; }
            return Catalog_Weapons.GenerateHeld(sc);
        }

        // The game registers only the weapon cards under Stats/Weapon/<category>/; a level's own props
        // (the laborers' crate, N_Crate) live elsewhere and are not found by name, but their card is
        // loaded while the level is. Cached per level.
        static Dictionary<string, StatCard_Item> loaded;
        static string loadedFor;

        static StatCard_Item Loaded(string card)
        {
            var key = ScenePath();
            if (loaded == null || loadedFor != key)
            {
                loaded = new Dictionary<string, StatCard_Item>();
                loadedFor = key;
            }
            StatCard_Item sc;
            if (loaded.TryGetValue(card, out sc) && sc != null) return sc;
            sc = Resources.FindObjectsOfTypeAll<StatCard_Item>().FirstOrDefault(x => x != null && x.name == card);
            if (sc != null) loaded[card] = sc;
            return sc;
        }

        static bool Give(Combatant_Base c, ItemRecord it)
        {
            try
            {
                var data = Generate(it.card);
                if (data == null) return false;
                var ranged = data as Item_Data_RangedWeapon;
                Item_Held held;
                if (ranged != null && (it.thrown || (data.AmType_Thrown() && ranged.amGrenadeSlot)))
                    held = Char_DataAssigner.GiveThrownToCharacter(ranged, c);
                else
                    held = Char_DataAssigner.GiveItemToCharacter(data, c, it.slot, it.hand);
                ApplyAmmo(held, it.hasAmmo, it.ammo, it.mags);
                return held != null;
            }
            catch (Exception e) { Game.Error("scene: give " + it.card, e); return false; }
        }

        internal class FloorResult { public int removed, placed; }

        // Takes every loose weapon out of the open rooms, then lays the saved ones back where they
        // were (rooms the game has closed are left alone). In the Playground a saved room that is
        // gone becomes the room you are in.
        internal FloorResult RestoreFloor()
        {
            var res = new FloorResult();
            foreach (var it in UnityEngine.Object.FindObjectsOfType<Item_Held>())
            {
                if (!Loose(it) || !Usable(it.myRoom)) continue;
                try
                {
                    if (it.myOwner != null) it.myOwner.ClearItemFromSlot(it);
                    it.DeSpawn();
                    res.removed++;
                }
                catch (Exception e) { Game.Error("scene: remove item " + it.name, e); }
            }
            foreach (var f in floor)
            {
                var room = FindRoom(f.room);
                if (!Usable(room)) room = IsPlayground ? Cam_Main.currentRoom : null;
                if (room == null) continue;
                try
                {
                    var data = Generate(f.card);
                    if (data == null) continue;
                    var held = Item_Held.CreateHeld(data, f.pos, f.rot, room);
                    ApplyAmmo(held, f.hasAmmo, f.ammo, f.mags);
                    res.placed++;
                }
                catch (Exception e) { Game.Error("scene: place " + f.card, e); }
            }
            return res;
        }
    }
}
