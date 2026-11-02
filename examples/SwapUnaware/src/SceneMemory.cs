using System.Collections.Generic;
using System.Linq;

namespace SwapUnaware
{
    // What a scene save needs that the game does not keep (0.17.0, the requester's asks):
    // - the F4 animation a character was given, while it holds the pose. A one-shot clip is over
    //   before the save, so the animator no longer names it; looping ones are read live.
    // - who a zed turned. The game swaps the victim's card for the zed's and keeps only the
    //   clothes on screen (Combatant_Base.MorphCombatant, keepArmor), so the card alone respawns
    //   a bare zed. This remembers the card the character had and the armor it wore.
    static class SceneMemory
    {
        static readonly Dictionary<Combatant_Base, string> poses = new Dictionary<Combatant_Base, string>();
        static readonly Dictionary<Combatant_Base, Turned> zeds = new Dictionary<Combatant_Base, Turned>();

        internal class Turned
        {
            public string from;                                // the character card before the zed got it
            public List<string> armor = new List<string>();    // "slot|armor card", as ArmorEntry writes
        }

        internal static void Posed(Combatant_Base c, string clip) { if (c != null && !string.IsNullOrEmpty(clip)) poses[c] = clip; }
        internal static void Unposed(Combatant_Base c) { if (c != null) poses.Remove(c); }

        internal static string Pose(Combatant_Base c)
        {
            string clip;
            return c != null && poses.TryGetValue(c, out clip) ? clip : null;
        }

        // A character turned twice keeps the card it had first.
        internal static void Turn(Combatant_Base c, string from, IEnumerable<string> armor)
        {
            if (c == null || string.IsNullOrEmpty(from)) return;
            Turned old;
            if (zeds.TryGetValue(c, out old)) return;
            zeds[c] = new Turned { from = from, armor = armor != null ? armor.ToList() : new List<string>() };
        }

        internal static Turned TurnedFrom(Combatant_Base c)
        {
            Prune();
            Turned t;
            return c != null && zeds.TryGetValue(c, out t) ? t : null;
        }

        internal static string ArmorEntry(string slot, string card) { return slot + "|" + card; }

        internal static bool ParseArmor(string entry, out string slot, out string card)
        {
            slot = card = null;
            int i = entry != null ? entry.IndexOf('|') : -1;
            if (i <= 0 || i == entry.Length - 1) return false;
            slot = entry.Substring(0, i);
            card = entry.Substring(i + 1);
            return true;
        }

        static void Prune()
        {
            foreach (var c in poses.Keys.Where(k => k == null).ToList()) poses.Remove(c);
            foreach (var c in zeds.Keys.Where(k => k == null).ToList()) zeds.Remove(c);
        }

        internal static void Reset() { poses.Clear(); zeds.Clear(); }
    }
}
