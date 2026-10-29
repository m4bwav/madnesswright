using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwapUnaware
{
    // One enemy in a wave built with the wave builder.
    public class WaveUnit
    {
        public string Card;        // the character card's asset name
        public string Name;        // its FullName, for the list
        public string Weapon;      // a weapon card's asset name; null: the card's own weapons
        public string WeaponName;
        public bool Infinite;      // infinite ammo, as \ gives it
        public bool Boss;          // the game's boss bar, as ] gives it

        public WaveUnit Clone() { return (WaveUnit)MemberwiseClone(); }
    }

    // The wave builder's list. Plain C# apart from Apply, so the harness (tests/DirectorFreezeChecks)
    // checks it without the game. Saved waves are text files, one unit per line, tab-separated:
    // card, weapon, infinite (0/1), boss (0/1), name, weapon name.
    public class WaveRoster
    {
        public readonly List<WaveUnit> Units = new List<WaveUnit>();

        public WaveUnit Add(string card, string name)
        {
            var u = new WaveUnit { Card = card, Name = name };
            Units.Add(u);
            return u;
        }

        // A copy of row i added at the end; null when there is no row i.
        public WaveUnit Copy(int i)
        {
            if (i < 0 || i >= Units.Count) return null;
            var u = Units[i].Clone();
            Units.Add(u);
            return u;
        }

        public void Remove(int i)
        {
            if (i >= 0 && i < Units.Count) Units.RemoveAt(i);
        }

        public static string Describe(WaveUnit u)
        {
            var s = new StringBuilder(Label(u.Name, u.Card)).Append(": ").Append(u.Weapon == null ? "own weapons" : Label(u.WeaponName, u.Weapon));
            if (u.Infinite) s.Append(", infinite ammo");
            if (u.Boss) s.Append(", boss bar");
            return s.ToString();
        }

        static string Label(string name, string card) { return string.IsNullOrWhiteSpace(name) ? card : name.Trim(); }

        public static string Save(IEnumerable<WaveUnit> units)
        {
            var s = new StringBuilder("# Director wave: card, weapon, infinite ammo, boss bar, name, weapon name\n");
            foreach (var u in units)
                s.Append(string.Join("\t", new[] { Field(u.Card), Field(u.Weapon), u.Infinite ? "1" : "0", u.Boss ? "1" : "0", Field(u.Name), Field(u.WeaponName) })).Append('\n');
            return s.ToString();
        }

        static string Field(string v) { return (v ?? "").Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' '); }

        // Lines that are blank, comments or too short are skipped, so a damaged file still loads the rest.
        public static List<WaveUnit> Load(string text)
        {
            var list = new List<WaveUnit>();
            foreach (var raw in (text ?? "").Split('\n'))
            {
                var line = raw.TrimEnd('\r');
                if (line.Trim().Length == 0 || line.StartsWith("#")) continue;
                var f = line.Split('\t');
                if (f.Length < 4 || f[0].Trim().Length == 0) continue;
                list.Add(new WaveUnit
                {
                    Card = f[0].Trim(),
                    Weapon = f[1].Trim().Length == 0 ? null : f[1].Trim(),
                    Infinite = f[2].Trim() == "1",
                    Boss = f[3].Trim() == "1",
                    Name = f.Length > 4 ? f[4] : null,
                    WeaponName = f.Length > 5 ? f[5] : null,
                });
            }
            return list;
        }

        // A file name that is the same on every system: path and reserved characters become _.
        public static string FileName(string name)
        {
            var bad = "\\/:*?\"<>|";
            var s = new string((name ?? "").Trim().Select(ch => ch < ' ' || bad.IndexOf(ch) >= 0 ? '_' : ch).ToArray()).Trim();
            return s.Length == 0 ? "wave" : s;
        }

        // What a spawned unit gets beyond its card and weapon, through the features so that \ and ]
        // can take it away again.
        internal static void Apply(Combatant_Base c, WaveUnit u)
        {
            if (u.Infinite) AmmoFeature.Set(c, true);
            if (u.Boss) BossBarFeature.Set(c, true);
        }
    }
}
