using System;
using System.Collections.Generic;
using System.Linq;

namespace SwapUnaware
{
    // The filter box on long lists and the weapon search. Plain C#, so the matching is checked
    // without the game (tests/DirectorFreezeChecks); the windows draw it with FilterBox.
    public static class ListFilter
    {
        // Lists longer than this get a filter box; shorter ones are left as they were.
        public const int Threshold = 20;

        public static bool Shows(int count) { return count > Threshold; }

        // Caseless. Plain text matches anywhere; * is any run of characters and ? any one, and a
        // pattern with them also matches anywhere. Empty or blank matches everything.
        public static bool Matches(string pattern, params string[] texts)
        {
            var p = (pattern ?? "").Trim().ToLowerInvariant();
            if (p.Length == 0) return true;
            if (texts == null) return false;
            foreach (var t in texts)
            {
                if (t == null) continue;
                var s = t.ToLowerInvariant();
                if (p.IndexOf('*') < 0 && p.IndexOf('?') < 0 ? s.Contains(p) : Glob("*" + p + "*", s)) return true;
            }
            return false;
        }

        // Greedy wildcard match with backtracking to the last *.
        static bool Glob(string p, string s)
        {
            int i = 0, j = 0, star = -1, mark = 0;
            while (j < s.Length)
            {
                if (i < p.Length && (p[i] == '?' || p[i] == s[j])) { i++; j++; }
                else if (i < p.Length && p[i] == '*') { star = i++; mark = j; }
                else if (star >= 0) { i = star + 1; j = ++mark; }
                else return false;
            }
            while (i < p.Length && p[i] == '*') i++;
            return i == p.Length;
        }

        public static List<T> Apply<T>(IEnumerable<T> items, string pattern, Func<T, string[]> texts)
        {
            return items.Where(x => Matches(pattern, texts(x))).ToList();
        }
    }

    // The last few weapons picked through the search, newest first, for its "Recent" row.
    public static class RecentPicks
    {
        public const int Max = 5;
        static readonly List<string> cards = new List<string>();

        public static IList<string> Cards { get { return cards; } }

        public static void Add(string card)
        {
            if (string.IsNullOrEmpty(card)) return;
            cards.Remove(card);
            cards.Insert(0, card);
            if (cards.Count > Max) cards.RemoveRange(Max, cards.Count - Max);
        }

        internal static void Clear() { cards.Clear(); }
    }
}
