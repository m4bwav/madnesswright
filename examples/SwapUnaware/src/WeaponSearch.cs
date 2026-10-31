using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SwapUnaware
{
    // The filter box at the top of a long list (ListFilter.Shows: more than 20 items). Lists that
    // had a filter before 0.15.0 keep it whatever their length (always: true).
    public class FilterBox
    {
        public string Text = "";
        readonly string control;
        bool active, focus;

        public FilterBox(string control) { this.control = control; }

        static int drawnFrame = -10;

        // A window with a text box calls this each time it draws one.
        public static void Touch() { drawnFrame = Time.frameCount; }

        // True while a Director text box was drawn last frame and something has the keyboard, so
        // typed letters are not taken as keys. Update runs before OnGUI, hence "last frame".
        public static bool Typing { get { return GUIUtility.keyboardControl != 0 && Time.frameCount - drawnFrame <= 1; } }

        // Keys that type a character or edit text (Unity's KeyCode: space to Delete), and CAPS LOCK.
        public static bool Eats(KeyCode k)
        {
            return ((k >= KeyCode.Space && k <= KeyCode.Delete) || k == KeyCode.CapsLock) && Typing;
        }

        public void Reset(bool focusIt = false)
        {
            Text = "";
            focus = focusIt;
        }

        public void Draw(int count, bool always = false)
        {
            active = always || ListFilter.Shows(count);
            if (!active) return;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Filter (* and ? work):", GUILayout.ExpandWidth(false));
            Touch();
            GUI.SetNextControlName(control);
            Text = GUILayout.TextField(Text ?? "", 40);
            if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false))) Text = "";
            GUILayout.EndHorizontal();
            if (focus) { GUI.FocusControl(control); focus = false; }
        }

        // Whether an item stays in the list; everything does while no box is shown.
        public bool Pass(params string[] texts) { return !active || ListFilter.Matches(Text, texts); }
    }

    // One search window for every place a weapon is picked (Spawn a weapon, a wave enemy's weapon,
    // off hand and throwable). It opens over the window that asked, which stops drawing until it
    // closes and keeps its own pause. The box has the keyboard at once; ENTER picks the top match,
    // ESC goes back. Picks are kept in RecentPicks for the Recent row (this game session only).
    public class WeaponSearch : MonoBehaviour
    {
        static string title;
        static List<WeaponEntry> list;
        static Action<WeaponEntry> pick;
        static readonly FilterBox box = new FilterBox("pd_weapon_search");
        Vector2 scroll;

        public static bool IsOpen { get { return list != null; } }

        public static void Open(string what, IEnumerable<WeaponEntry> weapons, Action<WeaponEntry> onPick)
        {
            title = what;
            list = weapons.Where(w => w != null).ToList();
            pick = onPick;
            box.Reset(true);
            Game.Log("search: opened for " + what + " (" + list.Count + " weapons)");
        }

        public static void Cancel()
        {
            list = null;
            pick = null;
            GUIUtility.keyboardControl = 0;
        }

        void Awake() { SceneManager.sceneLoaded += OnSceneLoaded; }

        void OnSceneLoaded(Scene s, LoadSceneMode m) { Cancel(); }

        static void Choose(WeaponEntry w)
        {
            var then = pick;
            Cancel();
            RecentPicks.Add(w.Card);
            Game.Log("search: picked " + w.Card + " for " + title);
            if (then != null) then(w);
        }

        // "ranged, damage 30, ammo 8": the kind always, a stat when the card gives one.
        internal static string Describe(WeaponEntry w)
        {
            return (w.Kind ?? "weapon") + (string.IsNullOrEmpty(w.Stat) ? "" : ", " + w.Stat)
                + (w.Hidden ? "   *" : "") + (w.Mod ? "   (mod)" : "");
        }

        void OnGUI()
        {
            if (list == null) return;
            GUI.depth = -10;
            Cursor.visible = true;
            var e = Event.current;
            var matches = ListFilter.Apply(list, box.Text, w => new[] { w.Label, w.Name, w.Card });
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape) { e.Use(); Cancel(); return; }
            if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
            {
                e.Use();
                if (matches.Count > 0) Choose(matches[0]);
                return;
            }
            float w = Mathf.Min(760f, Screen.width * 0.65f), h = Screen.height * 0.75f;
            GUILayout.BeginArea(new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h), GUI.skin.box);
            GUILayout.Label("Search: " + title + ".   Type part of a name; * and ? work.   ENTER: the top match.   ESC: back.");
            box.Draw(list.Count, true);
            // A click is acted on after the layout closes, so the window's groups stay balanced.
            WeaponEntry chosen = null;
            bool back = false;
            var recent = RecentPicks.Cards.Select(c => list.FirstOrDefault(x => x.Card == c)).Where(x => x != null).ToList();
            if (recent.Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Recent:", GUILayout.ExpandWidth(false));
                foreach (var r in recent) if (GUILayout.Button(r.Label, GUILayout.ExpandWidth(false))) chosen = r;
                GUILayout.EndHorizontal();
            }
            GUILayout.Label(matches.Count + " of " + list.Count + (matches.Count == 1 ? " matches" : " match") + (matches.Count > 0 ? ";   ENTER picks " + matches[0].Label : ""));
            scroll = GUILayout.BeginScrollView(scroll);
            foreach (var x in matches)
                if (GUILayout.Button(x.Label + "     " + Describe(x) + (x.Category != null ? "   [" + x.Category + "]" : ""))) chosen = x;
            GUILayout.EndScrollView();
            if (GUILayout.Button("Back")) back = true;
            GUILayout.EndArea();
            if (chosen != null) Choose(chosen);
            else if (back) Cancel();
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Cancel();
        }
    }
}
