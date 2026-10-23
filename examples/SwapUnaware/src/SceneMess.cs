using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SwapUnaware
{
    // Blood, bullet holes, casings and debris (0.7.4). The game keeps each in a capped list in
    // Decay_Manager, oldest first (1 debris, 2 casings, 3 holes, 4 splats; the oldest decays when
    // the list is full). A save remembers the newest piece of each while the level stays loaded;
    // a story or arena load removes everything after it. With no mark, or a mark that has decayed
    // or gone with a restart, every piece left is newer than the save, so all of it goes. Nothing
    // is written to the scene file: blood from before the save cannot be painted back anyway.
    // The Playground load clears all four lists (the game's Clear All).
    static class SceneMess
    {
        static readonly FieldInfo listsField = typeof(Decay_Manager).GetField("decayList", BindingFlags.NonPublic | BindingFlags.Static);
        static readonly int[] Kinds = { 1, 2, 3, 4 };
        static readonly Dictionary<string, GameObject[]> marks = new Dictionary<string, GameObject[]>();

        static string Key(SceneData d) { return d.stageKey + "|" + d.savedAt; }

        static List<GameObject>[] Lists() { return listsField != null ? listsField.GetValue(null) as List<GameObject>[] : null; }

        internal static void Mark(SceneData d)
        {
            var lists = Lists();
            if (lists == null) return;
            var m = new GameObject[lists.Length];
            foreach (int k in Kinds)
            {
                var l = lists[k];
                for (int i = l == null ? -1 : l.Count - 1; i >= 0; i--)
                    if (l[i] != null) { m[k] = l[i]; break; }
            }
            marks[Key(d)] = m;
        }

        // Returns how many pieces it removed, or -1 when the game's lists could not be read.
        internal static int ClearNewer(SceneData d)
        {
            var lists = Lists();
            if (lists == null) { Game.Log("scene: Decay_Manager.decayList not found, blood not reset"); return -1; }
            GameObject[] m;
            marks.TryGetValue(Key(d), out m);
            int n = 0;
            foreach (int k in Kinds)
            {
                var l = lists[k];
                if (l == null) continue;
                var mark = m != null ? m[k] : null;
                int from = 0;
                // ReferenceEquals: a destroyed mark would Equals every destroyed entry.
                if (mark != null)
                    for (int i = l.Count - 1; i >= 0; i--)
                        if (ReferenceEquals(l[i], mark)) { from = i + 1; break; }
                for (int i = from; i < l.Count; i++)
                {
                    if (l[i] == null) continue;
                    try { UnityEngine.Object.Destroy(l[i]); n++; }
                    catch (Exception e) { Game.Error("scene: clear mess", e); }
                }
                l.RemoveRange(from, l.Count - from);
            }
            return n;
        }
    }
}
