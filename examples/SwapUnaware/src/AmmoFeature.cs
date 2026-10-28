using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SwapUnaware
{
    // \ (backslash) on the character under the mouse, NPC or player: infinite ammo; \ again takes it
    // away. It is the game's own InfiniteAmmo trait flag (Combatant_Base.infAmmo): magazines empty and
    // reloads play, but spare magazines and grenades never run out and single-use weapons come back.
    // Melee weapons still wear out (that is the arena powerup's buff_InfAmmo, left alone). The flag
    // stays with the body through F5 and ends when the body dies.
    public class AmmoFeature : Feature
    {
        public KeyCode Key = KeyCode.Backslash;

        static readonly HashSet<Combatant_Base> bodies = new HashSet<Combatant_Base>();

        protected override string Name { get { return "infinite ammo (\\)"; } }
        protected override void Register() { Menu("Infinite ammo on / off (reloads still play; melee still wears out)", Key, true); }

        internal static bool Has(Combatant_Base c) { return c != null && bodies.Contains(c); }

        internal static void Set(Combatant_Base c, bool on)
        {
            if (c == null) return;
            if (on) { bodies.Add(c); c.infAmmo = true; }
            else Off(c);
        }

        // A scene load starts from nobody; the keep box (players keep what they have) spares the players.
        internal static void ClearAll(bool sparePlayers)
        {
            foreach (var c in bodies.ToList())
                if (!sparePlayers || !Game.IsPlayer(c)) Off(c);
        }

        protected override void Tick()
        {
            if (Pressed(Key))
            {
                var c = Game.CharacterUnderMouse();
                if (c == null) Game.Say("no character under the mouse");
                else if (Has(c)) { Off(c); c.FlashColor(Color.white, 8f, 1f); Game.Say(c.name + ": normal ammo"); }
                else { Set(c, true); c.FlashColor(Color.green, 8f, 1f); Game.Say(c.name + ": INFINITE AMMO (reloads still play; \\ again to stop)"); }
            }
            // The game sets the flag from the traits again on a level-up, a shop purchase or a boss's
            // next pattern (ApplyAllStats, SetTraitBools), so it is put back every frame.
            foreach (var c in bodies.ToList())
                if (!Game.Alive(c)) Off(c);
                else c.infAmmo = true;
        }

        // Back to what the character's own traits give it.
        static void Off(Combatant_Base c)
        {
            bodies.Remove(c);
            if (c != null) c.infAmmo = c.HaveTrait(TraitList.InfiniteAmmo);
        }

        void OnDestroy() { ClearAll(false); }
    }
}
