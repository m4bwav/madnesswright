using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SwapUnaware
{
    // Where a menu entry makes sense. Every feature works everywhere today; the flag is there so a
    // later one (a Playground-only spawner, say) greys itself out elsewhere.
    [Flags]
    public enum Modes { Story = 1, Arena = 2, Custom = 4, Playground = 8, All = 15 }

    // One line in the Director menu. A keyed entry with no Run presses its key for the feature
    // (Feature.Pressed); Pick first asks for a click on a character, so the key lands with the
    // mouse on it. Run is for actions that have no key, or that must run while still paused.
    public class MenuEntry
    {
        public string Label;
        public KeyCode Key = KeyCode.None;
        public Action Run;
        public Modes Where = Modes.All;
        public bool Pick;
    }

    // INSERT: the Director menu. Pauses the game and lists every feature as a button with its key.
    // Features add their own entries in Register(), so a new feature is one Add call, not a new
    // button here. ESC or INSERT closes it.
    public class DirectorMenu : Feature
    {
        public KeyCode Key = KeyCode.Insert;

        static readonly List<MenuEntry> entries = new List<MenuEntry>();
        static KeyCode queued = KeyCode.None;
        static int queuedFrame;
        static MenuEntry picking;

        bool open;
        Vector2 scroll;

        protected override string Name { get { return "menu (INSERT)"; } }
        protected override bool RunWhilePaused { get { return true; } }

        // True while the menu waits for a click on a character; AttackBlockPatch skips the shot.
        internal static bool Picking { get { return picking != null || queued != KeyCode.None; } }

        internal static void Add(MenuEntry e) { entries.Add(e); }

        // A key pressed from the menu counts for one frame after the menu closes or the pick lands
        // (the features skip paused frames and run in no fixed order).
        internal static bool TakeQueued(KeyCode k)
        {
            if (queued != k || Time.frameCount <= queuedFrame) return false;
            queued = KeyCode.None;
            return true;
        }

        void Awake() { SceneManager.sceneLoaded += OnSceneLoaded; }

        void OnSceneLoaded(Scene s, LoadSceneMode m)
        {
            Close();
            picking = null;
            queued = KeyCode.None;
        }

        protected override void Register()
        {
            Add(new MenuEntry { Label = "Clear corpses (keeps your squad's, bosses' and the level's scripted bodies)", Run = Cleanup.Corpses });
            Add(new MenuEntry { Label = "Clear blood, bullet holes, casings and debris", Run = Cleanup.BloodAndDebris });
        }

        protected override void Tick()
        {
            if (queued != KeyCode.None && Time.frameCount > queuedFrame + 3) queued = KeyCode.None;
            if (picking != null && !Game_Manager.ReturnPaused())
            {
                if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) { picking = null; Game.Say("cancelled"); }
                else if (Input.GetMouseButtonDown(0)) { Queue(picking.Key); picking = null; }
            }
            if (Input.GetKeyDown(Key))
            {
                if (open) Close();
                else if (Game.Me() == null) Game.Say("the menu needs a character to play; start a stage first");
                else Open();
            }
        }

        static void Queue(KeyCode k)
        {
            queued = k;
            queuedFrame = Time.frameCount;
        }

        // Holds its own pause and closes the game's pause menu, as the scene window does.
        void Open()
        {
            picking = null;
            open = true;
            Game_Manager.PauseGame(this);
            UI_Menu.ClosePauseMenu();
        }

        void Close()
        {
            if (!open) return;
            open = false;
            Game_Manager.UnpauseGame(this);
        }

        static Modes Here()
        {
            switch (Game_Manager.currentGameMode)
            {
                case GameModes.Arena: return Modes.Arena;
                case GameModes.Custom: return Modes.Custom;
                case GameModes.Playground: return Modes.Playground;
                default: return Modes.Story;
            }
        }

        static string KeyName(KeyCode k)
        {
            return k == KeyCode.None ? "" : k == KeyCode.CapsLock ? "CAPS LOCK" : k.ToString().ToUpperInvariant();
        }

        void Choose(MenuEntry e)
        {
            Close();
            Game.Log("menu: " + e.Label);
            if (e.Run != null) { e.Run(); return; }
            if (e.Pick) { picking = e; Game.Say("click a character for: " + e.Label + "   (right click cancels)"); return; }
            Queue(e.Key);
        }

        void OnGUI()
        {
            if (!open) return;
            Cursor.visible = true;
            var ev = Event.current;
            if (ev.type == EventType.KeyDown && ev.keyCode == KeyCode.Escape) { Close(); ev.Use(); return; }
            float w = Mathf.Min(720f, Screen.width * 0.6f), h = Screen.height * 0.75f;
            GUILayout.BeginArea(new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h), GUI.skin.box);
            GUILayout.Label("Playground Director " + ModEntry.Version + ": click an action.   INSERT or ESC: close.");
            scroll = GUILayout.BeginScrollView(scroll);
            var here = Here();
            foreach (var e in entries.OrderBy(x => x.Key == KeyCode.None).ThenBy(x => (int)x.Key))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(KeyName(e.Key), GUILayout.Width(90f));
                GUI.enabled = (e.Where & here) != 0;
                if (GUILayout.Button(e.Label + (e.Pick ? "  (then click a character)" : ""))) Choose(e);
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            if (GUILayout.Button("Close")) Close();
            GUILayout.EndArea();
        }

        // The mod was disabled: the features add their entries again when it comes back.
        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Close();
            entries.Clear();
            picking = null;
        }
    }

    // The menu's keyless actions.
    static class Cleanup
    {
        // Every corpse in every room of the level, the way the developer console's clear does it
        // (HaveRunKillMe, then Decay_Manager.DestroyMe_Character, which arenas also use). Kept:
        // your body, your home body, your squad and the playable characters (a story squadmate can
        // be revived), bosses and cutscene actors, the level's scripted characters (serial numbers:
        // its triggers may still read them), essential ones the game never lets decay, and bodies
        // dead under a second (the game is still running their death).
        internal static void Corpses()
        {
            var me = Game.Me();
            int here = 0, elsewhere = 0, kept = 0;
            var room = Cam_Main.currentRoom;
            foreach (var c in UnityEngine.Object.FindObjectsOfType<Combatant_Base>())
            {
                if (!Game.Corpse(c) || c == me) continue;
                if (SceneData.Essential(c) || SceneData.Untouchable(c) || c.HasSerialNumber()
                    || !Decay_Manager.ReturnCanCondemnCharacter(c) || !c.ReturnAppearDeadAndInert()) { kept++; continue; }
                try
                {
                    bool inRoom = c.myRoom == room;
                    Decay_Manager.DestroyMe_Character(c);
                    if (inRoom) here++; else elsewhere++;
                }
                catch (Exception e) { Game.Error("clear corpse " + c.name, e); }
            }
            Game.Say("cleared " + here + " corpses in room " + (room != null ? room.name : "?") + " and " + elsewhere
                + " in other rooms" + (kept > 0 ? "; kept " + kept + " (squad, scripted or boss)" : ""));
        }

        // The five lists the Playground's Clear All empties, minus the characters.
        internal static void BloodAndDebris()
        {
            int ok = 0;
            Action[] steps = { Decay_Manager.ClearAll_Splats, Decay_Manager.ClearAll_Holes, Decay_Manager.ClearAll_Casing, Decay_Manager.ClearAll_Debris };
            foreach (var step in steps)
            {
                try { step(); ok++; }
                catch (Exception e) { Game.Error("clear debris", e); }
            }
            Game.Say("cleared blood, bullet holes, casings and debris in " + SceneData.StageName() + (ok < steps.Length ? " (" + (steps.Length - ok) + " lists failed, see log)" : ""));
        }
    }
}
