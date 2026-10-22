using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SwapUnaware
{
    // F11 while paused: name the scene, ENTER saves it. CAPS LOCK while paused: the saved scenes;
    // click one to load it. ESC closes. Works in the Playground, story levels, arenas and custom
    // stages. A scene holds every living character but you (card, room, position, facing, squad and
    // team, damage, weapons, alert, frozen, guard and patrol), since 0.5.0 your own place, and since
    // 0.7.0 ammo, holsters, loose weapons and (Playground only) your own weapons.
    // Files: <persistentDataPath>/PlaygroundDirector/Scenes/<name>.json, outside Mods/ (the loader
    // reads every .json in a mod folder as a card). What loading does where: SceneData.
    public class SceneFeature : Feature
    {
        public KeyCode SaveKey = KeyCode.F11;
        public KeyCode ListKey = KeyCode.CapsLock;

        // Seconds to wait after moving you into another room, so its own characters spawn first.
        const float RoomSettle = 0.75f;
        // Seconds to wait after a Playground stage finishes loading.
        const float StageSettle = 1f;

        enum Mode { Closed, Name, List }
        Mode mode;
        string sceneName = "";
        Vector2 scroll;
        Entry[] saved = new Entry[0];
        bool focusName;

        class Entry { public string name, where; public bool here; }

        // A load waiting for a stage to load or a room to fill.
        class Pending { public SceneData data; public string name, scene; public float notBefore, deadline; }
        Pending pending;

        protected override string Name { get { return "scenes (F11)"; } }
        protected override bool RunWhilePaused { get { return true; } }

        static string Folder { get { return Path.Combine(Path.Combine(Application.persistentDataPath, "PlaygroundDirector"), "Scenes"); } }

        void Awake() { SceneManager.sceneLoaded += OnSceneLoaded; }

        // Opened straight from the paused menu: a queued key would arrive unpaused.
        protected override void Register()
        {
            DirectorMenu.Add(new MenuEntry { Label = "Save a scene", Key = SaveKey, Run = () => Open(Mode.Name) });
            DirectorMenu.Add(new MenuEntry { Label = "Saved scenes: load one", Key = ListKey, Run = () => Open(Mode.List) });
        }

        // A stage load clears every pause source; the window goes with it.
        void OnSceneLoaded(Scene s, LoadSceneMode m)
        {
            if (mode != Mode.Closed) Close();
        }

        protected override void Tick()
        {
            if (pending != null) RunPending();
            if (mode == Mode.Closed)
            {
                bool f11 = Input.GetKeyDown(SaveKey), caps = Input.GetKeyDown(ListKey);
                if (!f11 && !caps) return;
                if (!Game_Manager.ReturnPaused()) { if (f11) Game.Say("pause the game first (ESC), then F11"); return; }
                if (Game.Me() == null) { if (f11) Game.Say("scenes need a character to play; start a stage first"); return; }
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
            float w = Mathf.Min(620f, Screen.width * 0.55f), h = Screen.height * 0.6f;
            GUILayout.BeginArea(new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h), GUI.skin.box);
            if (mode == Mode.Name)
            {
                if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
                {
                    e.Use();
                    Save(sceneName);
                }
                GUILayout.Label("Name your scene, then ENTER.   CAPS LOCK: saved scenes.   ESC: close.");
                GUILayout.Label("Here: " + SceneData.StageName());
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
                GUILayout.Label("Scenes from this stage come first; others show where they were saved.");
                scroll = GUILayout.BeginScrollView(scroll);
                if (saved.Length == 0) GUILayout.Label("(none yet: F11 while paused saves one)");
                foreach (var s in saved)
                    if (GUILayout.Button(s.here ? s.name : s.name + "     [" + s.where + "]")) Load(s.name);
                GUILayout.EndScrollView();
                if (GUILayout.Button("Close")) Close();
            }
            GUILayout.EndArea();
        }

        static Entry[] ListScenes()
        {
            if (!Directory.Exists(Folder)) return new Entry[0];
            var list = new List<Entry>();
            foreach (var f in Directory.GetFiles(Folder, "*.json").OrderByDescending(File.GetLastWriteTimeUtc))
            {
                var en = new Entry { name = Path.GetFileNameWithoutExtension(f), where = "?" };
                var d = Read(f);
                if (d != null) { en.where = d.Where; en.here = d.SavedHere(); }
                list.Add(en);
            }
            return list.Where(x => x.here).Concat(list.Where(x => !x.here)).ToArray();
        }

        static SceneData Read(string path)
        {
            try { return SceneData.FromJson(File.ReadAllText(path)); }
            catch (Exception ex) { Game.Error("read " + path, ex); return null; }
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
            File.WriteAllText(Path.Combine(Folder, file + ".json"), data.ToJson());
            Game.Say("saved scene \"" + file + "\" on " + data.stageName + ", room " + data.playerRoom.name + ": " + data.chars.Count + " characters, "
                + data.floor.Count + " weapons on the floor");
            Close();
        }

        void Load(string name)
        {
            var path = Path.Combine(Folder, name + ".json");
            var data = Read(path);
            Close();
            if (data == null) { Game.Say("could not read scene \"" + name + "\""); return; }
            if (data.chars.Count == 0 && string.IsNullOrEmpty(data.playerRoom.name))
            {
                Game.Say("\"" + name + "\" holds no room and no characters (0.4.0 and 0.5.0 lost them when saving); save it again");
                return;
            }
            if (data.SavedHere())
            {
                if (data.IsPlayground) { Finish(data, name); return; }
                StartLevelLoad(data, name);
                return;
            }
            // Another Playground stage: load it, then the scene. Playground careers are never saved.
            // Story, arena and custom stages are left to the player: starting one writes the career.
            var target = string.IsNullOrEmpty(data.scenePath) ? data.stage : data.scenePath;
            if (SceneData.InPlayground && data.IsPlayground && Game_Manager.GameTransitions.ReturnSceneExists(target))
            {
                pending = new Pending { data = data, name = name, scene = target, deadline = Time.realtimeSinceStartup + 90f };
                Game.Say("loading " + data.Where + " for scene \"" + name + "\"");
                Game_Manager.GameTransitions.LoadScene(target, Transitions_Main.TransitionType.WipeSide);
                return;
            }
            Game.Say("\"" + name + "\" was saved on " + data.Where + "; play that stage, then load it (the mod does not start stages)");
        }

        // Puts you back in your saved room first, so the game spawns that room's own characters,
        // then restores the scene a moment later.
        void StartLevelLoad(SceneData data, string name)
        {
            float wait = 0f;
            if (data.hasPlayer)
            {
                var room = SceneData.FindRoom(data.playerRoom);
                var me = Game.Me();
                if (!SceneData.Usable(room))
                {
                    Game.Say("\"" + name + "\" was saved in room " + data.playerRoom.name + ", which the game has closed (it locks rooms you leave). Restart the stage, then load it");
                    return;
                }
                if (me == null || me.AmDead()) { Game.Say("you need a living character to load a scene"); return; }
                if (me.myRoom != room) wait = RoomSettle;
                me.TeleportTo(data.playerPos, SceneData.Facing(data.playerFacing) * Vector3.forward, room);
            }
            pending = new Pending { data = data, name = name, notBefore = Time.realtimeSinceStartup + wait, deadline = Time.realtimeSinceStartup + 30f };
            RunPending();
        }

        void RunPending()
        {
            var p = pending;
            float now = Time.realtimeSinceStartup;
            if (now > p.deadline) { pending = null; Game.Say("scene \"" + p.name + "\" not loaded: the stage did not finish loading"); return; }
            if (p.scene != null)
            {
                bool there = SceneData.ScenePath() == p.scene || SceneManager.GetActiveScene().name == p.scene;
                if (!there || Game.Me() == null || Cam_Main.currentRoom == null || Transitions_Main.ReturnTransitionRunning()) return;
                p.scene = null;
                p.notBefore = now + StageSettle;
            }
            if (now < p.notBefore || Game_Manager.ReturnPaused()) return;
            pending = null;
            Finish(p.data, p.name);
        }

        void Finish(SceneData data, string name)
        {
            var me = Game.Me();
            string where = data.Where + ", room " + (me != null && me.myRoom != null ? me.myRoom.name : "?");
            if (data.IsPlayground)
            {
                var p = data.RestorePlayground();
                Game.Say("loaded scene \"" + name + "\" on " + where + ": " + p.chars + " of " + data.chars.Count + " characters"
                    + Items(data, p.floor) + (p.yours ? ", your weapons reset" : ""));
                return;
            }
            var r = data.RestoreLevel();
            Game.Say("loaded scene \"" + name + "\" on " + where + ": " + r.moved + " moved back, " + r.respawned + " respawned, "
                + r.removed + " removed, " + r.skipped + " skipped (of " + data.chars.Count + " saved)" + Items(data, r.floor)
                + (data.HasItems ? ", ammo reset in " + r.yourAmmo + " of your " + data.playerItems.Count + " weapons" : ""));
        }

        static string Items(SceneData data, SceneData.FloorResult f)
        {
            if (!data.HasItems) return "; weapons not reset (saved before 0.7.0, save it again)";
            return "; weapons on the floor: " + f.removed + " cleared, " + f.placed + " of " + data.floor.Count + " put back";
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Close();
        }
    }
}
