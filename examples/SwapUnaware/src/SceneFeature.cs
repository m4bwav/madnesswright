using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SwapUnaware
{
    // F11 while paused in the Playground: name the scene, ENTER saves it. CAPS LOCK while paused:
    // the saved scenes; click one to load it. ESC closes. A scene holds every living character but
    // you: card, position, facing, squad and team, damage, weapons, alert, frozen, guard and patrol.
    // Files: <persistentDataPath>/PlaygroundDirector/Scenes/<name>.json, outside Mods/ (the loader
    // reads every .json in a mod folder as a card).
    public class SceneFeature : Feature
    {
        public KeyCode SaveKey = KeyCode.F11;
        public KeyCode ListKey = KeyCode.CapsLock;

        enum Mode { Closed, Name, List }
        Mode mode;
        string sceneName = "";
        Vector2 scroll;
        string[] saved = new string[0];
        bool focusName;

        protected override string Name { get { return "scenes (F11)"; } }
        protected override bool RunWhilePaused { get { return true; } }

        static string Folder { get { return Path.Combine(Path.Combine(Application.persistentDataPath, "PlaygroundDirector"), "Scenes"); } }

        protected override void Tick()
        {
            if (mode == Mode.Closed)
            {
                bool f11 = Input.GetKeyDown(SaveKey), caps = Input.GetKeyDown(ListKey);
                if (!f11 && !caps) return;
                if (!Game_Manager.ReturnPaused()) { if (f11) Game.Say("pause the game first (ESC), then F11"); return; }
                if (Game_Manager.currentGameMode != GameModes.Playground) { Game.Say("scenes work in the Playground only"); return; }
                Open(f11 ? Mode.Name : Mode.List);
            }
            else if (Input.GetKeyDown(ListKey)) Open(mode == Mode.List ? Mode.Name : Mode.List);
        }

        // Holds its own pause and closes the game's pause menu, so ENTER and clicks reach this window.
        void Open(Mode m)
        {
            if (mode == Mode.Closed)
            {
                Game_Manager.PauseGame(this);
                UI_Menu.ClosePauseMenu();
            }
            mode = m;
            focusName = m == Mode.Name;
            if (m == Mode.List) saved = ListScenes();
        }

        void Close()
        {
            if (mode == Mode.Closed) return;
            mode = Mode.Closed;
            Game_Manager.UnpauseGame(this);
        }

        void OnGUI()
        {
            if (mode == Mode.Closed) return;
            Cursor.visible = true;
            var e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape) { Close(); e.Use(); return; }
            float w = Mathf.Min(560f, Screen.width * 0.5f), h = Screen.height * 0.6f;
            GUILayout.BeginArea(new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h), GUI.skin.box);
            if (mode == Mode.Name)
            {
                if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
                {
                    e.Use();
                    Save(sceneName);
                }
                GUILayout.Label("Name your scene, then ENTER.   CAPS LOCK: saved scenes.   ESC: close.");
                GUI.SetNextControlName("pd_scene_name");
                sceneName = GUILayout.TextField(sceneName, 40);
                if (focusName) { GUI.FocusControl("pd_scene_name"); focusName = false; }
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Save")) Save(sceneName);
                if (GUILayout.Button("Saved scenes")) Open(Mode.List);
                if (GUILayout.Button("Close")) Close();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("Saved scenes: click one to load it.   CAPS LOCK: back.   ESC: close.");
                scroll = GUILayout.BeginScrollView(scroll);
                if (saved.Length == 0) GUILayout.Label("(none yet: F11 while paused saves one)");
                foreach (var s in saved)
                    if (GUILayout.Button(s)) Load(s);
                GUILayout.EndScrollView();
                if (GUILayout.Button("Close")) Close();
            }
            GUILayout.EndArea();
        }

        static string[] ListScenes()
        {
            if (!Directory.Exists(Folder)) return new string[0];
            return Directory.GetFiles(Folder, "*.json")
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .Select(Path.GetFileNameWithoutExtension).ToArray();
        }

        static string Safe(string name)
        {
            var bad = Path.GetInvalidFileNameChars();
            var s = new string(name.Trim().Select(ch => bad.Contains(ch) ? '_' : ch).ToArray());
            return s.Length > 0 ? s : null;
        }

        void Save(string name)
        {
            var file = Safe(name);
            if (file == null) { Game.Say("type a name first"); return; }
            var data = SceneData.Capture();
            Directory.CreateDirectory(Folder);
            File.WriteAllText(Path.Combine(Folder, file + ".json"), JsonUtility.ToJson(data, true));
            Game.Say("saved scene \"" + file + "\" (" + data.chars.Count + " characters)");
            Close();
        }

        void Load(string name)
        {
            var path = Path.Combine(Folder, name + ".json");
            SceneData data;
            try { data = JsonUtility.FromJson<SceneData>(File.ReadAllText(path)); }
            catch (Exception ex) { Game.Error("read " + path, ex); Game.Say("could not read scene \"" + name + "\""); return; }
            Close();
            if (data.stage != SceneManager.GetActiveScene().name)
            {
                Game.Say("\"" + name + "\" was saved on " + data.stage + "; load that stage first");
                return;
            }
            int n = data.Restore();
            Game.Say("loaded scene \"" + name + "\" (" + n + " of " + data.chars.Count + " characters)");
        }

        void OnDestroy() { Close(); }
    }

    [Serializable]
    public class SceneData
    {
        public int version = 1;
        public string stage;
        public string savedAt;
        public List<CharRecord> chars = new List<CharRecord>();

        [Serializable]
        public class CharRecord
        {
            public string card;
            public Vector3 pos;
            public float facing;
            public int squad;       // -1: your squad
            public int team;        // Factions
            public float damage;    // Target_Object.myTrauma
            public int alert;       // AI_Wakeup.AlertStatus
            public bool frozen, guard;
            public List<ItemRecord> items = new List<ItemRecord>();
            public List<PointRecord> patrol = new List<PointRecord>();
        }

        [Serializable]
        public class ItemRecord { public string card; public int hand; public bool thrown; }

        [Serializable]
        public class PointRecord { public Vector3 pos; public bool stop; }

        internal static SceneData Capture()
        {
            var d = new SceneData { stage = SceneManager.GetActiveScene().name, savedAt = DateTime.Now.ToString("s") };
            var me = Game.Me();
            var mySquad = me != null && me.myData != null ? me.myData.mySquad : null;
            var squads = new List<Squad_Main>();
            foreach (var c in UnityEngine.Object.FindObjectsOfType<Combatant_Base>())
            {
                if (!Game.Alive(c) || c == me || c.myData == null || c.myData.myCard == null || !(c.myController is Controller_NPC)) continue;
                var sq = c.myData.mySquad;
                int si = -2;
                if (sq != null && sq == mySquad) si = -1;
                else if (sq != null) { si = squads.IndexOf(sq); if (si < 0) { squads.Add(sq); si = squads.Count - 1; } }
                var r = new CharRecord
                {
                    card = c.myData.myCard.name,
                    pos = c.Position,
                    facing = c.transform.eulerAngles.y,
                    squad = si,
                    team = (int)c.myFaction,
                    damage = c.myTrauma,
                    alert = (int)c.myController.myAlertStatus,
                    frozen = FreezeFeature.IsFrozen(c),
                    guard = GuardFeature.IsGuard(c),
                };
                for (int hand = 0; hand < c.myItemsHeld.Length; hand++)
                {
                    var it = c.myItemsHeld[hand];
                    if (it != null && it.myData != null && it.myData.myCard != null) r.items.Add(new ItemRecord { card = it.myData.myCard.name, hand = hand });
                }
                var cc = c as Combatant_Character;
                if (cc != null && cc.myItemThrown != null && cc.myItemThrown.myData != null && cc.myItemThrown.myData.myCard != null)
                    r.items.Add(new ItemRecord { card = cc.myItemThrown.myData.myCard.name, thrown = true });
                PatrolFeature.Route route;
                if (PatrolFeature.routes.TryGetValue(c, out route))
                    foreach (var p in route.points) r.patrol.Add(new PointRecord { pos = p.pos, stop = p.stop });
                d.chars.Add(r);
            }
            return d;
        }

        // Clears the room the way the Playground's Clear All does, then spawns the saved characters.
        internal int Restore()
        {
            FreezeFeature.ReleaseAll();
            InteractiveMode.ClearAll_DoIt();
            var me = Game.Me();
            var room = Cam_Main.currentRoom;
            var squads = new Dictionary<int, Squad_Main>();
            int n = 0;
            foreach (var r in chars)
            {
                try
                {
                    var card = StatCard_Base.ReturnCardByName<StatCard_Character>(r.card);
                    if (card == null) { Debug.Log("[" + ModEntry.Id + "] scene: no card " + r.card); continue; }
                    var data = Char_Data.CreateCharacter(card);
                    var c = Combatant_Base.CreateCombatant(data, room, Controller_Base.ControllerType.NPC, r.pos, Quaternion.Euler(0f, r.facing, 0f), false);
                    if (c == null) continue;
                    Squad_Main sq;
                    if (r.squad == -1 && me != null && me.myData != null && me.myData.mySquad != null) sq = me.myData.mySquad;
                    else if (!squads.TryGetValue(r.squad, out sq)) { sq = Squad_Main.CreateSquad((Factions)r.team); squads[r.squad] = sq; }
                    sq.AddMember(c.myData);
                    foreach (var it in r.items) Give(c, it);
                    c.myTrauma = r.damage;
                    Controller_Base.SetAlertStatus(c.myController, (AI_Wakeup.AlertStatus)r.alert);
                    if (r.patrol.Count > 0) PatrolFeature.SetRoute(c, r.patrol.Select(p => new PatrolFeature.Point { pos = p.pos, stop = p.stop }).ToList());
                    if (r.guard) GuardFeature.Guard(c);
                    if (r.frozen) FreezeFeature.Freeze(c);
                    n++;
                }
                catch (Exception e) { Game.Error("scene: spawn " + r.card, e); }
            }
            return n;
        }

        static void Give(Combatant_Base c, ItemRecord it)
        {
            var card = StatCard_Base.ReturnCardByName<StatCard_Weapon>(it.card);
            if (card == null) return;
            var item = Catalog_Weapons.GenerateWeapon(card);
            if (item == null) return;
            if (item.AmType_Thrown() && item is Item_Data_RangedWeapon && ((Item_Data_RangedWeapon)item).amGrenadeSlot)
                Char_DataAssigner.GiveThrownToCharacter((Item_Data_RangedWeapon)item, c);
            else
                Char_DataAssigner.GiveItemToCharacter(item, c, 0, it.hand);
        }
    }
}
