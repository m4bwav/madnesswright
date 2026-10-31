using System;
using UnityEngine;

namespace SwapUnaware
{
    // One component per feature on the mod's host object, so a feature that throws cannot stop
    // the others. After MaxErrors exceptions in a row a feature switches itself off and says so,
    // instead of flooding Player.log with one exception per frame.
    public abstract class Feature : MonoBehaviour
    {
        const int MaxErrors = 5;
        int errors;

        protected abstract string Name { get; }
        protected abstract void Tick();

        // The game's characters stand still while paused (Combatant_Base.Update returns early);
        // only the scene window runs then.
        protected virtual bool RunWhilePaused { get { return false; } }

        // Each feature adds its lines to the Director menu here (DirectorMenu.Add).
        protected virtual void Register() { }

        // The key, pressed on the keyboard or from the Director menu.
        protected static bool Pressed(KeyCode k)
        {
            // A key typed into a Director text box is a letter, not a feature key.
            return (Input.GetKeyDown(k) && !FilterBox.Eats(k)) || DirectorMenu.TakeQueued(k);
        }

        protected static void Menu(string label, KeyCode key, bool pick = false)
        {
            DirectorMenu.Add(new MenuEntry { Label = label, Key = key, Pick = pick });
        }

        void Start()
        {
            try { Register(); }
            catch (Exception e) { Game.Error(Name + " menu", e); }
        }

        void Update()
        {
            try
            {
                if (Game_Manager.ReturnPaused() && !RunWhilePaused) return;
                Tick();
                errors = 0;
            }
            catch (Exception e)
            {
                Game.Error(Name, e);
                if (++errors >= MaxErrors)
                {
                    enabled = false;
                    Game.Say(Name + " stopped after repeated errors; the other keys still work");
                }
            }
        }
    }

    // A one-line message at the top of the screen for a few seconds.
    public class Hud : MonoBehaviour
    {
        static string text;
        static float until;
        GUIStyle style;

        internal static void Show(string msg)
        {
            text = msg;
            until = Time.unscaledTime + 3f;
        }

        void OnGUI()
        {
            if (text == null || Time.unscaledTime > until) return;
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.box) { fontSize = Mathf.Max(14, Screen.height / 50), wordWrap = true };
                style.normal.textColor = Color.white;
            }
            float w = Mathf.Min(Screen.width * 0.6f, 900f);
            GUI.Box(new Rect((Screen.width - w) / 2f, Screen.height * 0.06f, w, style.fontSize * 2.2f), text, style);
        }
    }
}
