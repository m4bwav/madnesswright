using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SwapUnaware
{
    // INSERT menu, "Spawn a weapon": every weapon card the game and the enabled mods hold, by the
    // game's own categories (Other last), including the ones the Playground's spawn menu leaves out
    // (marked *). A click puts the weapon in your hands with the Playground's own call, or on the
    // floor in front of you when the box is ticked or your hands hold something that cannot be
    // dropped. The window holds its own pause, as the scene list does, so nobody moves or shoots
    // while it is open. ESC goes back from the weapons to the categories, then closes.
    public class WeaponSpawnFeature : Feature
    {
        enum Step { Closed, Categories, Weapons }

        Step step = Step.Closed;
        List<WeaponGroup> groups = new List<WeaponGroup>();
        WeaponGroup group;
        Vector2 catScroll, weaponScroll;
        bool toFloor;
        string last = "";
        readonly FilterBox filter = new FilterBox("pd_spawn_filter");
        readonly HashSet<string> failed = new HashSet<string>();

        protected override string Name { get { return "spawn a weapon (INSERT menu)"; } }
        protected override bool RunWhilePaused { get { return true; } }

        protected override void Register()
        {
            DirectorMenu.Add(new MenuEntry { Label = "Spawn a weapon (every weapon in the game, by category)", Run = Open });
        }

        void Awake() { SceneManager.sceneLoaded += OnSceneLoaded; }

        void OnSceneLoaded(Scene s, LoadSceneMode m) { Close(); }

        protected override void Tick()
        {
            if (step != Step.Closed && Game.Me() == null) Close();
        }

        void Open()
        {
            if (Game.Me() == null) { Game.Say("spawning a weapon needs a character to play; start a stage first"); return; }
            groups = Build();
            group = null;
            last = "";
            if (step == Step.Closed)
            {
                Game_Manager.PauseGame(this);
                UI_Menu.ClosePauseMenu();
            }
            step = Step.Categories;
        }

        void Close()
        {
            if (step == Step.Closed) return;
            step = Step.Closed;
            WeaponSearch.Cancel();
            Game_Manager.UnpauseGame(this);
        }

        void Back()
        {
            if (step == Step.Weapons) { step = Step.Categories; group = null; }
            else Close();
        }

        // ---- the list ----

        internal static List<WeaponGroup> Build()
        {
            var entries = new List<WeaponEntry>();
            var broken = new List<string>();
            var props = new HashSet<StatCard_Weapon>(Props());
            foreach (var raw in StatCard_Base.ReturnAllWeapons().Concat(props))
            {
                if (raw == null) continue;
                var card = raw.OrOverride() ?? raw;
                if (Broken(card)) { broken.Add(card.name); continue; }
                var cat = card.Category;
                bool known = Enum.IsDefined(typeof(WeaponCategories), cat) && cat != WeaponCategories.None;
                bool prop = props.Contains(raw);
                entries.Add(new WeaponEntry
                {
                    Card = card.name,
                    Name = card.FullName,
                    Category = prop ? PropsCategory : known ? cat.ToString() : null,
                    Order = prop ? (int)WeaponCategories.None : (int)cat,
                    Hidden = prop || HiddenInPlayground(raw, card),
                    Mod = card.AmNewCard,
                    Source = card,
                    Kind = Kind(card),
                    Stat = Stat(card),
                });
            }
            var groups = WeaponCatalog.Group(entries);
            int n = groups.Sum(g => g.Weapons.Count), hidden = groups.Sum(g => g.Weapons.Count(w => w.Hidden));
            Game.Log("weapons: " + n + " in " + groups.Count + " categories (" + string.Join(", ", groups.Select(g => g.Name + " " + g.Weapons.Count).ToArray())
                + "), " + hidden + " not in the Playground's list, " + broken.Count + " left out as broken" + (broken.Count > 0 ? ": " + string.Join(", ", broken.ToArray()) : ""));
            return groups;
        }

        // The game files its held props (clipboard, crate, laptop, umbrella, rock...) as weapon cards
        // under Stats/Weapon/None/ and skips that folder when it registers weapons, so they are in no
        // weapon list and not found by name. Loaded here for the Director's own lists only: adding them
        // to the game's register would put them in random loot, the market and arena drops.
        internal const string PropsCategory = "Props";
        const string PropsFolder = "Stats/Weapon/None/";

        static IEnumerable<StatCard_Weapon> Props()
        {
            try { return Resources.LoadAll<StatCard_Weapon>(PropsFolder).Where(c => c != null && !c.AmNewCard).ToList(); }
            catch (Exception e) { Game.Error("weapons: props", e); return new StatCard_Weapon[0]; }
        }

        // A weapon card by name, props included: for wave files and scene files.
        internal static StatCard_Weapon Card(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            var w = StatCard_Base.ReturnCardByName<StatCard_Weapon>(name);
            if (w != null) return w;
            var prop = Resources.Load<StatCard_Weapon>(PropsFolder + name);
            return prop != null ? (prop.OrOverride() ?? prop) : null;
        }

        // A preset (StatCard_WeaponModded) is described by the gun it is built on.
        static StatCard_Weapon Basis(StatCard_Weapon card)
        {
            var preset = card as StatCard_WeaponModded;
            return preset != null && preset.LinkedWeapon != null ? preset.LinkedWeapon : card;
        }

        static string Kind(StatCard_Weapon card)
        {
            var b = Basis(card);
            return b is StatCard_Thrown ? "throwable" : b is StatCard_Ranged ? "ranged" : "melee";
        }

        // The card's own numbers, before quality, skills and addons: what the cards give cheaply.
        static string Stat(StatCard_Weapon card)
        {
            var b = Basis(card);
            var thrown = b as StatCard_Thrown;
            if (thrown != null) return thrown.Damage > 0f ? "damage " + thrown.Damage.ToString("0.#") : null;
            var ranged = b as StatCard_Ranged;
            if (ranged != null)
                return (ranged.Damage > 0f ? "damage " + ranged.Damage.ToString("0.#") + ", " : "") + "ammo " + ranged.Ammo;
            return b.MeleeDamage > 0f ? "damage " + b.MeleeDamage.ToString("0.#") : null;
        }

        // The game cannot make these: no prefab (Instantiate throws), or a preset with no base weapon.
        static bool Broken(StatCard_Weapon card)
        {
            if (card.myRef == null) return true;
            var preset = card as StatCard_WeaponModded;
            return preset != null && preset.LinkedWeapon == null;
        }

        // InteractiveMode's own filter, its category menu (no Unarmed, no None), and its card lookup,
        // which finds a mod's new cards by exact type, so only mods' melee weapons reach it.
        static bool HiddenInPlayground(StatCard_Weapon raw, StatCard_Weapon card)
        {
            if (!card.ItemEnabled || !card.TierValidForStore() || card.ForSale == StatCard_Item.Store.Arcane) return true;
            if (card.Category == WeaponCategories.Unarmed || card.Category == WeaponCategories.None) return true;
            return raw.AmNewCard && raw.GetType() != typeof(StatCard_Weapon);
        }

        // ---- spawning ----

        void Spawn(WeaponEntry w)
        {
            var me = Game.Me();
            var card = w.Source as StatCard_Weapon;
            if (me == null || card == null) return;
            try
            {
                bool floor = toFloor || !HandsFree(me);
                Item_Held held = null;
                if (!floor)
                {
                    held = GiveToHands(card, me);
                    // CollectHeld can still refuse (the item stays ownerless and roomless): drop it instead.
                    if (held != null && held.myOwner != me) { held.DeSpawn(); held = null; }
                    floor = held == null;
                }
                if (floor) held = Drop(card, me);
                if (held == null) throw new Exception("the game made no weapon from the card");
                last = w.Label + (floor ? ": on the floor in front of you" : ": in your hands");
                Game.Log("weapons: spawned " + w.Card + " (" + (w.Category ?? WeaponCatalog.Other) + ")" + (floor ? " on the floor" : " in hand"));
            }
            catch (Exception e)
            {
                failed.Add(w.Card);
                last = w.Label + ": failed (see Player.log)";
                Game.Error("weapons: spawn " + w.Card, e);
            }
        }

        // GiveItemToCharacter forgets the held item's data before CollectHeld, which keeps an item
        // that cannot be dropped: never call it then.
        static bool HandsFree(Combatant_Base me)
        {
            foreach (var it in me.myItemsHeld) if (it != null && !it.CanBeDropped()) return false;
            return true;
        }

        // The Playground's "Mine!": a grenade-slot throwable goes to the grenade slot, the rest to hand 0.
        internal static Item_Held GiveToHands(StatCard_Weapon card, Combatant_Base me)
        {
            var data = Catalog_Weapons.GenerateWeapon(card);
            if (data == null) return null;
            var ranged = data as Item_Data_RangedWeapon;
            if (ranged != null && data.AmType_Thrown() && ranged.amGrenadeSlot) return Char_DataAssigner.GiveThrownToCharacter(ranged, me);
            return Char_DataAssigner.GiveItemToCharacter(data, me, 0, 0);
        }

        static Item_Held Drop(StatCard_Weapon card, Combatant_Base me)
        {
            var data = Catalog_Weapons.GenerateWeapon(card);
            if (data == null) return null;
            var ahead = me.transform.forward;
            ahead.y = 0f;
            var pos = me.Position + (ahead.sqrMagnitude > 0.01f ? ahead.normalized : Vector3.zero) + Vector3.up;
            return Item_Held.CreateHeld(data, pos, Quaternion.identity, me.myRoom);
        }

        // ---- the window ----

        void OnGUI()
        {
            if (step == Step.Closed) return;
            Cursor.visible = true;
            if (WeaponSearch.IsOpen) return;
            var e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape) { Back(); e.Use(); return; }
            float w = Mathf.Min(720f, Screen.width * 0.6f), h = Screen.height * 0.75f;
            GUILayout.BeginArea(new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h), GUI.skin.box);
            if (step == Step.Categories)
            {
                GUILayout.Label("Spawn a weapon: pick a category.   ESC: close.");
                GUILayout.Label("*: not in the Playground's own spawn list.   (mod): from a mod.");
                toFloor = GUILayout.Toggle(toFloor, " Drop it on the floor in front of me (instead of into my hands)");
                GUILayout.Label(last);
                if (GUILayout.Button("SEARCH every weapon by name"))
                    WeaponSearch.Open("spawn a weapon", groups.SelectMany(g => g.Weapons).OrderBy(x => x.Label, StringComparer.OrdinalIgnoreCase), Spawn);
                catScroll = GUILayout.BeginScrollView(catScroll);
                if (groups.Count == 0) GUILayout.Label("(the game holds no weapon cards here)");
                foreach (var g in groups)
                {
                    int hidden = g.Weapons.Count(x => x.Hidden);
                    if (GUILayout.Button(g.Name + "   (" + g.Weapons.Count + (hidden > 0 ? ", " + hidden + " *" : "") + ")"))
                    {
                        group = g;
                        weaponScroll = Vector2.zero;
                        filter.Reset();
                        step = Step.Weapons;
                    }
                }
                GUILayout.EndScrollView();
                if (GUILayout.Button("Close")) Close();
            }
            else if (group != null)
            {
                GUILayout.Label(group.Name + ": click a weapon to spawn it.   ESC: back to the categories.");
                toFloor = GUILayout.Toggle(toFloor, " Drop it on the floor in front of me (instead of into my hands)");
                GUILayout.Label(last);
                filter.Draw(group.Weapons.Count);
                weaponScroll = GUILayout.BeginScrollView(weaponScroll);
                foreach (var x in group.Weapons)
                {
                    if (!filter.Pass(x.Label, x.Card)) continue;
                    string label = x.Label + (x.Hidden ? "   *" : "") + (x.Mod ? "   (mod)" : "") + (failed.Contains(x.Card) ? "   (failed)" : "");
                    if (GUILayout.Button(label)) Spawn(x);
                }
                GUILayout.EndScrollView();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Back")) Back();
                if (GUILayout.Button("Close")) Close();
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Close();
        }
    }
}
