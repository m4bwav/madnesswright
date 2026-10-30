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
        public string Weapon2;     // a second weapon card (0.13.0); null: none
        public string Weapon2Name;
        public string Throwable;   // a grenade-slot throwable card (0.13.0); null: none
        public string ThrowableName;

        public WaveUnit Clone() { return (WaveUnit)MemberwiseClone(); }
    }

    // Where a unit's second weapon goes.
    public enum SecondPlace { None, OffHand, Backup, Throwable }

    // The wave builder's list. Plain C# apart from Apply, so the harness (tests/DirectorFreezeChecks)
    // checks it without the game. Saved waves are text files, one unit per line, tab-separated:
    // card, weapon, infinite (0/1), boss (0/1), name, weapon name, then (version 2, 0.13.0) second
    // weapon, its name, throwable, its name.
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

        // The game's rules: a grenade-slot card always goes to the grenade slot (Wave_Generator.GiveItem),
        // and a picked throwable wins over a grenade picked as the second weapon; any other second weapon
        // goes in the off hand when Char_Data.ReturnCanDual allows the pair, else stowed as a backup the
        // AI swaps to (Combatant_Character would drop a refused off-hand item on the floor).
        public static SecondPlace PlaceSecond(bool grenade, bool throwableSet, bool canDual)
        {
            if (grenade) return throwableSet ? SecondPlace.None : SecondPlace.Throwable;
            return canDual ? SecondPlace.OffHand : SecondPlace.Backup;
        }

        public static string Describe(WaveUnit u)
        {
            var s = new StringBuilder(Label(u.Name, u.Card)).Append(": ").Append(u.Weapon == null ? "own weapons" : Label(u.WeaponName, u.Weapon));
            if (u.Weapon2 != null) s.Append(" and ").Append(Label(u.Weapon2Name, u.Weapon2));
            if (u.Throwable != null) s.Append(", throws ").Append(Label(u.ThrowableName, u.Throwable));
            if (u.Infinite) s.Append(", infinite ammo");
            if (u.Boss) s.Append(", boss bar");
            return s.ToString();
        }

        static string Label(string name, string card) { return string.IsNullOrWhiteSpace(name) ? card : name.Trim(); }

        public static string Save(IEnumerable<WaveUnit> units)
        {
            var s = new StringBuilder("# Director wave v2: card, weapon, infinite ammo, boss bar, name, weapon name, second weapon, its name, throwable, its name\n");
            foreach (var u in units)
                s.Append(string.Join("\t", new[] { Field(u.Card), Field(u.Weapon), u.Infinite ? "1" : "0", u.Boss ? "1" : "0", Field(u.Name), Field(u.WeaponName),
                    Field(u.Weapon2), Field(u.Weapon2Name), Field(u.Throwable), Field(u.ThrowableName) })).Append('\n');
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
                    // Version 2 fields; a 0.12.0 file has none, so both slots read as none.
                    Weapon2 = Card(f, 6),
                    Weapon2Name = f.Length > 7 ? f[7] : null,
                    Throwable = Card(f, 8),
                    ThrowableName = f.Length > 9 ? f[9] : null,
                });
            }
            return list;
        }

        static string Card(string[] f, int i) { return f.Length > i && f[i].Trim().Length > 0 ? f[i].Trim() : null; }

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
