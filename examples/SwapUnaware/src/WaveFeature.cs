using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SwapUnaware
{
    // INSERT menu, "Build a wave": pick enemies, give each a set weapon from the Spawn a weapon list (or
    // its own), infinite or normal ammo and a boss bar, then spawn them all as one squad. The spawn is
    // the Playground's own Spawn Units: a new Factions.None squad (it fights you and every other squad),
    // each unit at one of the current room's entrances, alert at once. Waves can be saved by name as
    // <persistentDataPath>/PlaygroundDirector/Waves/<name>.txt. The window holds its own pause.
    public class WaveFeature : Feature
    {
        enum Step { Closed, Roster, Characters, WeaponCategories, Weapons, Throwables }

        Step step = Step.Closed;
        readonly WaveRoster roster = new WaveRoster();
        int editing = -1;                 // the row whose weapon is being picked
        bool second;                      // picking the row's second weapon, not its first
        List<StatCard_Character> characters = new List<StatCard_Character>();
        List<WeaponGroup> groups = new List<WeaponGroup>();
        List<WeaponEntry> throwables = new List<WeaponEntry>();
        WeaponGroup group;
        string filter = "", waveName = "", last = "";
        Vector2 rosterScroll, listScroll;
        // Waves spawning under their limit: the next enemy comes in when one dies.
        class Running { public WaveQueue Queue; public Squad_Main Squad; public int Made; }
        readonly List<Running> running = new List<Running>();
        readonly Dictionary<string, bool?> dualCache = new Dictionary<string, bool?>();
        float nextFeed;

        static string Folder { get { return Path.Combine(Path.Combine(Application.persistentDataPath, "PlaygroundDirector"), "Waves"); } }

        protected override string Name { get { return "wave builder (INSERT menu)"; } }
        protected override bool RunWhilePaused { get { return true; } }

        protected override void Register()
        {
            DirectorMenu.Add(new MenuEntry { Label = "Build a wave (enemies with set weapons, ammo and boss bars)", Run = Open });
        }

        void Awake() { SceneManager.sceneLoaded += OnSceneLoaded; }

        void OnSceneLoaded(Scene s, LoadSceneMode m)
        {
            Close();
            foreach (var r in running) Game.Log("wave: scene changed; " + r.Queue.Left + " queued enemies dropped");
            running.Clear();
        }

        protected override void Tick()
        {
            if (step != Step.Closed && Game.Me() == null) Close();
            if (running.Count == 0 || Game_Manager.ReturnPaused() || Time.unscaledTime < nextFeed) return;
            nextFeed = Time.unscaledTime + 0.25f;
            Feed();
        }

        void Open()
        {
            if (Game.Me() == null) { Game.Say("building a wave needs a character to play; start a stage first"); return; }
            characters = StatCard_Base.ReturnAllCardsByType<StatCard_Character>()
                .Where(c => c != null && c.CharacterEnabled)
                .OrderBy(c => CardName(c), StringComparer.OrdinalIgnoreCase).ToList();
            groups = WeaponSpawnFeature.Build();
            throwables = groups.SelectMany(g => g.Weapons).Where(x => IsGrenade(x.Source as StatCard_Weapon))
                .OrderBy(x => x.Label, StringComparer.OrdinalIgnoreCase).ToList();
            last = "";
            if (step == Step.Closed)
            {
                Game_Manager.PauseGame(this);
                UI_Menu.ClosePauseMenu();
            }
            step = Step.Roster;
        }

        void Close()
        {
            if (step == Step.Closed) return;
            step = Step.Closed;
            Game_Manager.UnpauseGame(this);
        }

        void Back()
        {
            if (step == Step.Weapons) step = Step.WeaponCategories;
            else if (step == Step.Characters || step == Step.WeaponCategories || step == Step.Throwables) step = Step.Roster;
            else Close();
        }

        static string CardName(StatCard_Character c) { return string.IsNullOrWhiteSpace(c.FullName) ? c.name : c.FullName.Trim(); }

        // The grenade slot, as Wave_Generator.GiveItem decides it.
        static bool IsGrenade(StatCard_Weapon w) { return w != null && w.AmGrenadeSlot(); }

        // ---- spawning ----

        void SpawnWave()
        {
            var me = Game.Me();
            if (me == null || roster.Units.Count == 0) return;
            var r = new Running { Queue = new WaveQueue(roster.Units, roster.MaxAlive), Squad = Squad_Main.CreateSquad(Factions.None) };
            Game.Log("wave: " + roster.Units.Count + " enemies, " + (roster.MaxAlive > 0 ? "at most " + roster.MaxAlive + " alive at once" : "no limit") +
                " (" + string.Join("; ", roster.Units.Select(WaveRoster.Describe).ToArray()) + ")");
            running.Add(r);
            Close();
            Feed();
            Game.Say("wave: " + r.Made + " of " + r.Queue.Total + " spawned" + (r.Queue.Done ? "" : "; the next comes in when one dies"));
        }

        // Spawns what each running wave has room for, the way the Playground's Spawn Units does.
        void Feed()
        {
            var me = Game.Me();
            if (me == null) return;
            var room = Cam_Main.currentRoom != null ? Cam_Main.currentRoom : me.myRoom;
            var entrances = room != null && room.myEntrances != null ? room.myEntrances.Where(e => e != null && !e.disableSpawn).ToList() : new List<Entrance_Base>();
            foreach (var r in running.ToList())
            {
                var due = r.Queue.Due();
                foreach (var u in due)
                {
                    Combatant_Base c = null;
                    try
                    {
                        c = SpawnUnit(u, me, room, entrances);
                        if (c == null) { Game.Log("wave: " + u.Card + " was not spawned"); continue; }
                        r.Squad.AddMember(c.myData);
                        Controller_Base.SetAlertStatus(c.myController, AI_Wakeup.AlertStatus.Combat);
                        // The grenade-throwing module is chosen from what the unit holds; refresh it in
                        // case the alert was already Combat when the items arrived.
                        var npc = c.myController as Controller_NPC;
                        if (npc != null) Controller_NPC.SetWeaponBasedModules(npc);
                        WaveRoster.Apply(c, u);
                        r.Made++;
                    }
                    catch (Exception e) { Game.Error("wave: spawn " + u.Card, e); }
                    finally { r.Queue.Spawned(c); }
                }
                if (due.Count > 0 && r.Made > due.Count) Game.Log("wave: next in (" + r.Made + " of " + r.Queue.Total + " spawned, " + r.Queue.Left + " queued)");
                if (r.Queue.Done)
                {
                    Game.Log("wave: spawned " + r.Made + " of " + r.Queue.Total);
                    last = "spawned " + r.Made + " of " + r.Queue.Total;
                    running.Remove(r);
                }
            }
        }

        static Combatant_Base SpawnUnit(WaveUnit u, Combatant_Base me, Room_Main room, List<Entrance_Base> entrances)
        {
            var card = StatCard_Base.ReturnCardByName<StatCard_Character>(u.Card);
            if (card == null) return null;
            var data = Char_Data.CreateCharacter(card);
            if (data == null) return null;
            var weapon = u.Weapon != null ? StatCard_Base.ReturnCardByName<StatCard_Weapon>(u.Weapon) : null;
            if (u.Weapon != null && weapon == null) Game.Log("wave: no weapon card " + u.Weapon + "; " + u.Card + " gets its own");
            if (weapon == null) OwnWeapons(data);

            Combatant_Base c = null;
            var fit = entrances.Where(e => e.ReturnMeetVocationRequirements(data)).ToList();
            var pick = (fit.Count > 0 ? fit : entrances);
            if (pick.Count > 0) c = Spawn_Manager.SpawnCharacterToEntrance(data, pick[UnityEngine.Random.Range(0, pick.Count)], Controller_Base.ControllerType.NPC);
            // A room with no entrance to spawn from: near you instead.
            if (c == null) c = Spawn_Manager.SpawnCharacterToPoint(data, me.Position + me.transform.forward * 3f, room, Controller_Base.ControllerType.NPC, 1f);
            if (c == null) c = Combatant_Base.CreateCombatant(data, room, Controller_Base.ControllerType.NPC, me.Position + me.transform.forward * 3f, me.transform.rotation, false);
            if (c == null) return null;
            if (weapon != null && WeaponSpawnFeature.GiveToHands(weapon, c) == null) Game.Log("wave: the game made no " + u.Weapon + " for " + u.Card);
            GiveThrowable(u, c);
            GiveSecond(u, c);
            return c;
        }

        static StatCard_Weapon WeaponCard(string card, string unit)
        {
            if (card == null) return null;
            var w = StatCard_Base.ReturnCardByName<StatCard_Weapon>(card);
            if (w == null) Game.Log("wave: no weapon card " + card + " for " + unit);
            return w;
        }

        static void GiveThrowable(WaveUnit u, Combatant_Base c)
        {
            var card = WeaponCard(u.Throwable, u.Card);
            if (card == null) return;
            var data = Catalog_Weapons.GenerateWeapon(card) as Item_Data_RangedWeapon;
            if (data == null || !data.amGrenadeSlot) { Game.Log("wave: " + u.Throwable + " is not a grenade-slot throwable"); return; }
            if (Char_DataAssigner.GiveThrownToCharacter(data, c) == null) Game.Log("wave: the game made no " + u.Throwable + " for " + u.Card);
        }

        // Off hand when the game allows the pair, else stowed as a backup; a grenade becomes the throwable.
        static void GiveSecond(WaveUnit u, Combatant_Base c)
        {
            var card = WeaponCard(WaveRoster.SecondWeapon(u), u.Card);
            if (card == null) return;
            var data = Catalog_Weapons.GenerateWeapon(card);
            if (data == null) { Game.Log("wave: the game made no " + u.Weapon2 + " for " + u.Card); return; }
            var main = c.myData.ReturnItem(0, 0) as Item_Data_Held;
            var place = WaveRoster.PlaceSecond(IsGrenade(card), u.Throwable != null, main == null || c.myData.ReturnCanDual(main, data));
            if (place == SecondPlace.None) { Game.Log("wave: " + u.Card + " keeps its throwable; second weapon " + u.Weapon2 + " left out"); return; }
            if (place == SecondPlace.Throwable) { Char_DataAssigner.GiveThrownToCharacter(data as Item_Data_RangedWeapon, c); return; }
            Item_Held held = place == SecondPlace.OffHand
                ? Char_DataAssigner.GiveItemToCharacter(data, c, 0, main == null ? 0 : 1)
                : Char_DataAssigner.GiveItemToCharacter(data, c, 1, 0);
            if (held == null || held.myOwner != c) { if (held != null) held.DeSpawn(); Game.Log("wave: " + u.Card + " could not take " + u.Weapon2); return; }
            Game.Log("wave: " + u.Card + " " + (place == SecondPlace.OffHand ? "dual wields " : "carries as a backup ") + u.Weapon2);
        }

        // As the Playground's Spawn Units: the card's default weapons, else a wave weapon near its level.
        static void OwnWeapons(Char_Data data)
        {
            Wave_Generator.AssignAllDefaultWeapons(data, 0);
            if (data.ReturnHoldingAnyItem() || data.myWeaponPreference == WeaponCategories.None || data.myWeaponPreference == WeaponCategories.Unarmed) return;
            var list = Catalog_Weapons.ReturnWaveWeapons_ArenaOnly(data.myLevel - 3, data.myLevel + 3);
            if (list.Count > 0) data.HoldItem(0, 0, list[UnityEngine.Random.Range(0, list.Count)].GenerateMe() as Item_Data_Held);
        }

        // ---- saved waves ----

        void Save()
        {
            try
            {
                Directory.CreateDirectory(Folder);
                var file = Path.Combine(Folder, WaveRoster.FileName(waveName) + ".txt");
                File.WriteAllText(file, WaveRoster.Save(roster.Units, roster.MaxAlive));
                last = "saved " + Path.GetFileNameWithoutExtension(file);
                Game.Log("wave: saved " + roster.Units.Count + " units to " + file);
            }
            catch (Exception e) { last = "save failed (see Player.log)"; Game.Error("wave: save", e); }
        }

        void Load(string file)
        {
            try
            {
                var text = File.ReadAllText(file);
                var units = WaveRoster.Load(text);
                roster.Units.Clear();
                roster.Units.AddRange(units);
                roster.MaxAlive = WaveRoster.MaxAliveIn(text);
                waveName = Path.GetFileNameWithoutExtension(file);
                last = "loaded " + waveName + " (" + units.Count + ")";
            }
            catch (Exception e) { last = "load failed (see Player.log)"; Game.Error("wave: load", e); }
        }

        static string[] SavedWaves()
        {
            try { return Directory.Exists(Folder) ? Directory.GetFiles(Folder, "*.txt").OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToArray() : new string[0]; }
            catch { return new string[0]; }
        }

        // ---- the window ----

        void OnGUI()
        {
            if (step == Step.Closed) return;
            Cursor.visible = true;
            var e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape) { Back(); e.Use(); return; }
            float w = Mathf.Min(820f, Screen.width * 0.7f), h = Screen.height * 0.8f;
            GUILayout.BeginArea(new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h), GUI.skin.box);
            if (step == Step.Roster) DrawRoster();
            else if (step == Step.Characters) DrawCharacters();
            else if (step == Step.WeaponCategories) DrawCategories();
            else if (step == Step.Throwables) DrawThrowables();
            else DrawWeapons();
            GUILayout.EndArea();
        }

        void DrawRoster()
        {
            GUILayout.Label("Build a wave: add enemies, set each one's weapon, ammo and boss bar, then Spawn.   ESC: close.");
            GUILayout.Label(last);
            rosterScroll = GUILayout.BeginScrollView(rosterScroll, GUILayout.ExpandHeight(true));
            if (roster.Units.Count == 0) GUILayout.Label("(no enemies yet: Add an enemy)");
            for (int i = 0; i < roster.Units.Count; i++)
            {
                var u = roster.Units[i];
                GUILayout.BeginHorizontal();
                GUILayout.Label((i + 1) + ". " + (string.IsNullOrWhiteSpace(u.Name) ? u.Card : u.Name), GUILayout.Width(Col(0.26f)));
                if (GUILayout.Button(u.Weapon == null ? "Own weapons" : (u.WeaponName ?? u.Weapon), GUILayout.Width(Col(0.26f)))) { editing = i; second = false; step = Step.WeaponCategories; }
                u.Infinite = GUILayout.Toggle(u.Infinite, u.Infinite ? " Infinite ammo" : " Normal ammo", GUILayout.Width(Col(0.15f)));
                u.Boss = GUILayout.Toggle(u.Boss, " Boss bar", GUILayout.Width(Col(0.11f)));
                if (GUILayout.Button("Copy")) roster.Copy(i);
                if (GUILayout.Button("Remove")) { roster.Remove(i); GUILayout.EndHorizontal(); break; }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(Col(0.26f)));
                u.Dual = GUILayout.Toggle(u.Dual, u.Dual ? " Dual wielding: on" : " Dual wielding: off", GUILayout.Width(Col(0.2f)));
                if (u.Dual && GUILayout.Button(u.Weapon2 == null ? "Off hand: pick" : "Off hand: " + (u.Weapon2Name ?? u.Weapon2), GUILayout.Width(Col(0.24f)))) { editing = i; second = true; step = Step.WeaponCategories; }
                if (GUILayout.Button(u.Throwable == null ? "Throwable: none" : "Throws: " + (u.ThrowableName ?? u.Throwable), GUILayout.Width(Col(0.24f)))) { editing = i; listScroll = Vector2.zero; step = Step.Throwables; }
                GUILayout.EndHorizontal();
                var note = WaveRoster.DualNote(u, u.Dual ? CanDual(u) : null);
                if (note != null) { GUILayout.BeginHorizontal(); GUILayout.Label("", GUILayout.Width(Col(0.26f))); GUILayout.Label(note); GUILayout.EndHorizontal(); }
            }
            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            GUILayout.Label(roster.Units.Count + (roster.Units.Count == 1 ? " enemy" : " enemies") + " in this wave.   Max enemies alive at once:", GUILayout.ExpandWidth(false));
            if (GUILayout.Button("-", GUILayout.Width(28f)) && roster.MaxAlive > 0) roster.MaxAlive--;
            GUILayout.Label(roster.MaxAlive == 0 ? "no limit" : roster.MaxAlive.ToString(), GUILayout.Width(64f));
            if (GUILayout.Button("+", GUILayout.Width(28f))) roster.MaxAlive++;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add an enemy")) { filter = ""; listScroll = Vector2.zero; step = Step.Characters; }
            GUI.enabled = roster.Units.Count > 0;
            if (GUILayout.Button("SPAWN the wave")) SpawnWave();
            if (GUILayout.Button("Clear")) roster.Units.Clear();
            GUI.enabled = true;
            if (GUILayout.Button("Close")) Close();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Name:", GUILayout.ExpandWidth(false));
            waveName = GUILayout.TextField(waveName, 40, GUILayout.Width(Col(0.3f)));
            GUI.enabled = roster.Units.Count > 0;
            if (GUILayout.Button("Save the wave", GUILayout.ExpandWidth(false))) Save();
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            var saved = SavedWaves();
            if (saved.Length > 0)
            {
                GUILayout.Label("Saved waves (click to load):");
                GUILayout.BeginHorizontal();
                foreach (var f in saved.Take(8)) if (GUILayout.Button(Path.GetFileNameWithoutExtension(f))) Load(f);
                GUILayout.EndHorizontal();
            }
        }

        void DrawCharacters()
        {
            GUILayout.Label("Add an enemy: click a character.   ESC: back.");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Filter:", GUILayout.ExpandWidth(false));
            filter = GUILayout.TextField(filter, 40);
            GUILayout.EndHorizontal();
            listScroll = GUILayout.BeginScrollView(listScroll);
            foreach (var c in characters)
            {
                var n = CardName(c);
                if (filter.Length > 0 && n.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0 && c.name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0) continue;
                if (GUILayout.Button(n + "   [" + c.DefaultFaction + "]" + (c.AmNewCard ? "   (mod)" : ""))) { roster.Add(c.name, n); step = Step.Roster; }
            }
            GUILayout.EndScrollView();
            if (GUILayout.Button("Back")) Back();
        }

        void DrawCategories()
        {
            GUILayout.Label((second ? "Second weapon for " : "Weapon for ") + Row() + ": pick a category.   ESC: back.");
            if (second) GUILayout.Label("Held in the other hand when the game allows the pair, else carried as a backup.");
            listScroll = GUILayout.BeginScrollView(listScroll);
            if (GUILayout.Button(second ? "None" : "Own weapons (what the character comes with)")) SetWeapon(null);
            foreach (var g in groups)
                if (GUILayout.Button(g.Name + "   (" + g.Weapons.Count + ")")) { group = g; listScroll = Vector2.zero; step = Step.Weapons; }
            GUILayout.EndScrollView();
            if (GUILayout.Button("Back")) Back();
        }

        void DrawWeapons()
        {
            if (group == null) { step = Step.WeaponCategories; return; }
            GUILayout.Label("Weapon for " + Row() + ", " + group.Name + ": click one.   ESC: back.   *: not in the Playground's list.");
            listScroll = GUILayout.BeginScrollView(listScroll);
            foreach (var x in group.Weapons)
                if (GUILayout.Button(x.Label + (x.Hidden ? "   *" : "") + (x.Mod ? "   (mod)" : ""))) SetWeapon(x);
            GUILayout.EndScrollView();
            if (GUILayout.Button("Back")) Back();
        }

        void DrawThrowables()
        {
            GUILayout.Label("Throwable for " + Row() + ": click one. The enemy throws it on its own; infinite ammo keeps it throwing.   ESC: back.");
            listScroll = GUILayout.BeginScrollView(listScroll);
            if (GUILayout.Button("None")) SetThrowable(null);
            if (throwables.Count == 0) GUILayout.Label("(the game holds no grenade-slot throwables here)");
            foreach (var x in throwables)
                if (GUILayout.Button(x.Label + (x.Hidden ? "   *" : "") + (x.Mod ? "   (mod)" : ""))) SetThrowable(x);
            GUILayout.EndScrollView();
            if (GUILayout.Button("Back")) Back();
        }

        void SetThrowable(WeaponEntry x)
        {
            if (editing >= 0 && editing < roster.Units.Count)
            {
                var u = roster.Units[editing];
                u.Throwable = x != null ? x.Card : null;
                u.ThrowableName = x != null ? x.Label : null;
            }
            editing = -1;
            step = Step.Roster;
        }

        string Row()
        {
            return editing >= 0 && editing < roster.Units.Count ? (editing + 1) + ". " + (roster.Units[editing].Name ?? roster.Units[editing].Card) : "?";
        }

        void SetWeapon(WeaponEntry x)
        {
            if (editing >= 0 && editing < roster.Units.Count)
            {
                var u = roster.Units[editing];
                if (second) { u.Weapon2 = x != null ? x.Card : null; u.Weapon2Name = x != null ? x.Label : null; }
                else { u.Weapon = x != null ? x.Card : null; u.WeaponName = x != null ? x.Label : null; }
            }
            editing = -1;
            step = Step.Roster;
        }

        // Whether the game lets this character hold the pair (Char_Data.ReturnCanDual), from fresh data
        // made the way the spawn makes it; null when it cannot tell (own weapons, a grenade, a missing card).
        bool? CanDual(WaveUnit u)
        {
            if (u.Weapon == null || u.Weapon2 == null) return null;
            var key = u.Card + "|" + u.Weapon + "|" + u.Weapon2;
            bool? known = null;
            if (dualCache.TryGetValue(key, out known)) return known;
            try
            {
                var card = StatCard_Base.ReturnCardByName<StatCard_Character>(u.Card);
                var w1 = StatCard_Base.ReturnCardByName<StatCard_Weapon>(u.Weapon);
                var w2 = StatCard_Base.ReturnCardByName<StatCard_Weapon>(u.Weapon2);
                if (card != null && w1 != null && w2 != null && !IsGrenade(w2))
                {
                    var data = Char_Data.CreateCharacter(card);
                    var a = Catalog_Weapons.GenerateWeapon(w1) as Item_Data_Held;
                    var b = Catalog_Weapons.GenerateWeapon(w2) as Item_Data_Held;
                    if (data != null && a != null && b != null) known = data.ReturnCanDual(a, b);
                }
            }
            catch (Exception e) { Game.Error("wave: dual check " + key, e); }
            dualCache[key] = known;
            return known;
        }

        static float Col(float share) { return Mathf.Min(820f, Screen.width * 0.7f) * share; }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Close();
        }
    }
}
