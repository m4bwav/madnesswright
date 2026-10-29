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
        enum Step { Closed, Roster, Characters, WeaponCategories, Weapons }

        Step step = Step.Closed;
        readonly WaveRoster roster = new WaveRoster();
        int editing = -1;                 // the row whose weapon is being picked
        List<StatCard_Character> characters = new List<StatCard_Character>();
        List<WeaponGroup> groups = new List<WeaponGroup>();
        WeaponGroup group;
        string filter = "", waveName = "", last = "";
        Vector2 rosterScroll, listScroll;

        static string Folder { get { return Path.Combine(Path.Combine(Application.persistentDataPath, "PlaygroundDirector"), "Waves"); } }

        protected override string Name { get { return "wave builder (INSERT menu)"; } }
        protected override bool RunWhilePaused { get { return true; } }

        protected override void Register()
        {
            DirectorMenu.Add(new MenuEntry { Label = "Build a wave (enemies with set weapons, ammo and boss bars)", Run = Open });
        }

        void Awake() { SceneManager.sceneLoaded += OnSceneLoaded; }

        void OnSceneLoaded(Scene s, LoadSceneMode m) { Close(); }

        protected override void Tick()
        {
            if (step != Step.Closed && Game.Me() == null) Close();
        }

        void Open()
        {
            if (Game.Me() == null) { Game.Say("building a wave needs a character to play; start a stage first"); return; }
            characters = StatCard_Base.ReturnAllCardsByType<StatCard_Character>()
                .Where(c => c != null && c.CharacterEnabled)
                .OrderBy(c => CardName(c), StringComparer.OrdinalIgnoreCase).ToList();
            groups = WeaponSpawnFeature.Build();
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
            else if (step == Step.Characters || step == Step.WeaponCategories) step = Step.Roster;
            else Close();
        }

        static string CardName(StatCard_Character c) { return string.IsNullOrWhiteSpace(c.FullName) ? c.name : c.FullName.Trim(); }

        // ---- spawning ----

        void SpawnWave()
        {
            var me = Game.Me();
            if (me == null || roster.Units.Count == 0) return;
            var room = Cam_Main.currentRoom != null ? Cam_Main.currentRoom : me.myRoom;
            var entrances = room != null && room.myEntrances != null ? room.myEntrances.Where(e => e != null && !e.disableSpawn).ToList() : new List<Entrance_Base>();
            var squad = Squad_Main.CreateSquad(Factions.None);
            int made = 0;
            foreach (var u in roster.Units)
            {
                try
                {
                    var c = SpawnUnit(u, me, room, entrances);
                    if (c == null) { Game.Log("wave: " + u.Card + " was not spawned"); continue; }
                    squad.AddMember(c.myData);
                    Controller_Base.SetAlertStatus(c.myController, AI_Wakeup.AlertStatus.Combat);
                    WaveRoster.Apply(c, u);
                    made++;
                }
                catch (Exception e) { Game.Error("wave: spawn " + u.Card, e); }
            }
            Game.Log("wave: spawned " + made + " of " + roster.Units.Count + " (" + string.Join("; ", roster.Units.Select(WaveRoster.Describe).ToArray()) + ")");
            last = "spawned " + made + " of " + roster.Units.Count;
            Close();
            Game.Say("wave: " + made + " of " + roster.Units.Count + " spawned");
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
            return c;
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
                File.WriteAllText(file, WaveRoster.Save(roster.Units));
                last = "saved " + Path.GetFileNameWithoutExtension(file);
                Game.Log("wave: saved " + roster.Units.Count + " units to " + file);
            }
            catch (Exception e) { last = "save failed (see Player.log)"; Game.Error("wave: save", e); }
        }

        void Load(string file)
        {
            try
            {
                var units = WaveRoster.Load(File.ReadAllText(file));
                roster.Units.Clear();
                roster.Units.AddRange(units);
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
                if (GUILayout.Button(u.Weapon == null ? "Own weapons" : (u.WeaponName ?? u.Weapon), GUILayout.Width(Col(0.26f)))) { editing = i; step = Step.WeaponCategories; }
                u.Infinite = GUILayout.Toggle(u.Infinite, u.Infinite ? " Infinite ammo" : " Normal ammo", GUILayout.Width(Col(0.15f)));
                u.Boss = GUILayout.Toggle(u.Boss, " Boss bar", GUILayout.Width(Col(0.11f)));
                if (GUILayout.Button("Copy")) roster.Copy(i);
                if (GUILayout.Button("Remove")) { roster.Remove(i); GUILayout.EndHorizontal(); break; }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
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
            GUILayout.Label("Weapon for " + Row() + ": pick a category.   ESC: back.");
            listScroll = GUILayout.BeginScrollView(listScroll);
            if (GUILayout.Button("Own weapons (what the character comes with)")) SetWeapon(null);
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

        string Row()
        {
            return editing >= 0 && editing < roster.Units.Count ? (editing + 1) + ". " + (roster.Units[editing].Name ?? roster.Units[editing].Card) : "?";
        }

        void SetWeapon(WeaponEntry x)
        {
            if (editing >= 0 && editing < roster.Units.Count)
            {
                var u = roster.Units[editing];
                u.Weapon = x != null ? x.Card : null;
                u.WeaponName = x != null ? x.Label : null;
            }
            editing = -1;
            step = Step.Roster;
        }

        static float Col(float share) { return Mathf.Min(820f, Screen.width * 0.7f) * share; }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Close();
        }
    }
}
