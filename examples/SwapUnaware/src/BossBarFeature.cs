using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SwapUnaware
{
    // ] (right bracket) on the character under the mouse, enemy, squadmate or you: the game's own boss
    // health bar (UI_Game.AddBoss, the call Controller_Boss makes for story and arena bosses); ] again
    // takes it away. The game shows one boss bar at a time and switches to another when a player hits
    // it; a new bar is shown at once. A dead character's bar goes two seconds after it dies.
    public class BossBarFeature : Feature
    {
        public KeyCode Key = KeyCode.RightBracket;
        const float DeadLinger = 2f;

        static readonly HashSet<Combatant_Base> bodies = new HashSet<Combatant_Base>();
        static readonly Dictionary<Combatant_Base, float> deadSince = new Dictionary<Combatant_Base, float>();

        protected override string Name { get { return "boss bar (])"; } }
        protected override void Register() { Menu("Boss health bar on / off", Key, true); }

        internal static bool Has(Combatant_Base c) { return c != null && bodies.Contains(c); }

        internal static void Set(Combatant_Base c, bool on)
        {
            if (c == null) return;
            if (!on) { Off(c); return; }
            if (bodies.Contains(c)) return;
            // Linking the bar ends the character's other health UI (CharUI.LinkToCharacter), which for a
            // player or squadmate is its HUD icon. Hold it aside and put it back: the bar still reads the
            // character's damage every frame, and ending the bar later leaves the HUD alone.
            var hud = c.myHealthUI;
            c.myHealthUI = null;
            try { UI_Game.AddBoss(c); }
            finally { if (hud != null) c.myHealthUI = hud; }
            bodies.Add(c);
            ShowNow(c);
        }

        internal static void ClearAll()
        {
            foreach (var c in bodies.ToList()) Off(c);
            deadSince.Clear();
        }

        protected override void Tick()
        {
            if (Pressed(Key))
            {
                var c = Game.CharacterUnderMouse();
                if (c == null) Game.Say("no character under the mouse");
                else if (Has(c)) { Off(c); Game.Say(c.name + ": boss bar off"); }
                else { Set(c, true); c.FlashColor(Color.red, 8f, 1f); Game.Say(c.name + ": BOSS BAR (] again to remove)"); }
            }
            foreach (var c in bodies.ToList())
            {
                if (Game.Alive(c)) { deadSince.Remove(c); continue; }
                float t;
                if (!deadSince.TryGetValue(c, out t)) deadSince[c] = Time.unscaledTime;
                else if (Time.unscaledTime - t >= DeadLinger) Off(c);
            }
        }

        // The game shows the first bar and hides later ones until a player hits them; show the new one now.
        static void ShowNow(Combatant_Base c)
        {
            var ui = UI_Game.currentUI_Game;
            if (ui == null) return;
            foreach (var bar in ui.BossfightIcons)
                if (bar != null && bar.linkedTarget == c) { bar.ShowMe(true); return; }
        }

        static void Off(Combatant_Base c)
        {
            bodies.Remove(c);
            deadSince.Remove(c);
            if (c != null) UI_Game.RemoveBoss(c);
        }

        void OnDestroy() { ClearAll(); }
    }
}
