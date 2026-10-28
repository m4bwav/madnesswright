using System;
using System.Collections.Generic;
using System.Linq;

namespace SwapUnaware
{
    // One weapon in the Spawn a weapon list. Plain C#, so the grouping is checked without the game
    // (tests/DirectorFreezeChecks); WeaponSpawnFeature fills it from the game's weapon cards.
    public class WeaponEntry
    {
        public string Card;       // the card's asset name, unique
        public string Name;       // the card's FullName; empty falls back to Card
        public string Category;   // the game's category; null or empty goes under Other
        public int Order;         // the category's WeaponCategories value
        public bool Hidden;       // the Playground's own spawn list leaves it out
        public bool Mod;          // a mod's new card
        public object Source;     // the card itself
        public string Label;      // set by Group: the name, plus the card when two share one name
    }

    public class WeaponGroup
    {
        public string Name;
        public List<WeaponEntry> Weapons = new List<WeaponEntry>();
    }

    public static class WeaponCatalog
    {
        public const string Other = "Other";

        // Groups in the game's category order with Other last and empty ones left out; each sorted
        // by name ignoring case, then by card. A card listed twice keeps its first entry.
        public static List<WeaponGroup> Group(IEnumerable<WeaponEntry> all)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var list = all.Where(e => e != null && !string.IsNullOrEmpty(e.Card) && seen.Add(e.Card)).ToList();
            var groups = new List<WeaponGroup>();
            foreach (var g in list
                .GroupBy(e => string.IsNullOrEmpty(e.Category) ? null : e.Category)
                .OrderBy(g => g.Key == null ? 1 : 0)
                .ThenBy(g => g.Min(e => e.Order))
                .ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase))
            {
                var weapons = g.OrderBy(Name, StringComparer.OrdinalIgnoreCase).ThenBy(e => e.Card, StringComparer.Ordinal).ToList();
                var counts = weapons.GroupBy(Name, StringComparer.OrdinalIgnoreCase).ToDictionary(x => x.Key, x => x.Count(), StringComparer.OrdinalIgnoreCase);
                foreach (var w in weapons) w.Label = counts[Name(w)] > 1 ? Name(w) + " [" + w.Card + "]" : Name(w);
                groups.Add(new WeaponGroup { Name = g.Key ?? Other, Weapons = weapons });
            }
            return groups;
        }

        static string Name(WeaponEntry e) { return string.IsNullOrWhiteSpace(e.Name) ? e.Card : e.Name.Trim(); }
    }
}