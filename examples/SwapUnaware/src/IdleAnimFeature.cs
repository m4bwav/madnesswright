using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SwapUnaware
{
    // F4 on the character under the mouse: a list of idle animations (its own idle emotes first,
    // then every looping clip in the game's library); clicking one plays it as an emote, the way
    // level markers make enemies type or smoke. F4 or ESC closes the list.
    public class IdleAnimFeature : Feature
    {
        public KeyCode Key = KeyCode.F4;

        Combatant_Base target;
        List<AnimationClip> clips;
        readonly FilterBox filter = new FilterBox("pd_idle_filter");
        Vector2 scroll;
        bool open;

        // Characters posing, with their wandering flag from before (wandering would cancel the pose).
        static readonly Dictionary<Combatant_Base, bool> posing = new Dictionary<Combatant_Base, bool>();

        protected override string Name { get { return "idle animations (F4)"; } }
        protected override void Register() { Menu("Idle animations", Key, true); }

        protected override void Tick()
        {
            if (Pressed(Key))
            {
                if (open) Close();
                else Open();
            }
            if (open && (target == null || target.AmDead() || Input.GetKeyDown(KeyCode.Escape))) Close();
            // A posing character that noticed someone goes back to wandering when the pose ends.
            foreach (var c in posing.Keys.ToList())
            {
                if (c == null) { posing.Remove(c); continue; }
                if (c.AmDead() || (!c.InLoopingEmote() && c.myController != null && c.myController.myAlertStatus != AI_Wakeup.AlertStatus.Unaware))
                    EndPose(c);
            }
        }

        void Open()
        {
            target = Game.NpcUnderMouse();
            if (target == null) { Game.Say("no character under the mouse"); return; }
            clips = CollectClips(target);
            if (clips.Count == 0) { Game.Say("no idle animations found"); return; }
            filter.Reset();
            scroll = Vector2.zero;
            open = true;
            // Locked, the player neither moves nor attacks while clicking and typing in the list.
            var pc = Controller_Player.PlayerController;
            if (pc != null) pc.LockController(this);
            Game.Say(clips.Count + " animations for " + target.name);
        }

        void Close()
        {
            open = false;
            var pc = Controller_Player.PlayerController;
            if (pc != null) pc.ReleaseController(this);
        }

        static List<AnimationClip> CollectClips(Combatant_Base c)
        {
            var list = new List<AnimationClip>();
            var seen = new HashSet<string>();
            var own = c.myAnimSet != null && c.myAnimSet.IdleEmotes != null ? c.myAnimSet.IdleEmotes.myClips : null;
            if (own != null)
                foreach (var a in own)
                    if (a != null && seen.Add(a.name)) list.Add(a);
            int ownCount = list.Count;
            var lib = LibraryStorageList.Library != null && LibraryStorageList.Library.AnimClips != null ? LibraryStorageList.Library.AnimClips.Storage : null;
            if (lib != null)
            {
                var more = lib.OfType<AnimationClip>()
                    .Where(a => a != null && (a.isLooping || a.name.ToLowerInvariant().Contains("idle")) && !seen.Contains(a.name))
                    .OrderBy(a => a.name).ToList();
                foreach (var a in more) if (seen.Add(a.name)) list.Add(a);
            }
            Debug.Log("[" + ModEntry.Id + "] idle list: " + ownCount + " own emotes, " + (list.Count - ownCount) + " from the library");
            return list;
        }

        void Play(AnimationClip clip)
        {
            var c = target;
            bool ok = Pose(c, clip, true);
            Game.Say(ok ? c.name + " plays " + clip.name : c.name + " cannot play that now");
        }

        // fade false starts the loop at once, at a random point (a scene load).
        internal static bool Pose(Combatant_Base c, AnimationClip clip, bool fade)
        {
            if (!posing.ContainsKey(c)) posing[c] = c.wanderingDisabled;
            Game.ReleaseClaims(c.myController);
            Controller_Base.SetAlertStatus(c.myController, AI_Wakeup.AlertStatus.Unaware);
            c.wanderingDisabled = true;
            Pathfind_Main.ClearWaypointsAndTarget(c);
            return c.PlayAsEmote(clip, fade, false);
        }

        // The looping emote a character is playing (F4's or a level's typing, smoking...), or null.
        // PlayAsEmote alternates two override slots and flips the private emoteState after each.
        static readonly System.Reflection.FieldInfo EmoteSlot = typeof(Combatant_Base).GetField("emoteState",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        internal static string CurrentEmote(Combatant_Base c)
        {
            if (c == null || !c.InLoopingEmote() || EmoteSlot == null) return null;
            var aoc = c.myAnimator_Character != null ? c.myAnimator_Character.myOverride : null;
            if (aoc == null || Catalog_Animations.DefaultAnimatorClips == null) return null;
            var slot = (int)EmoteSlot.GetValue(c) == 1 ? Catalog_Animations.OverrideStates.Emote_0 : Catalog_Animations.OverrideStates.Emote_1;
            AnimationClip key;
            if (!Catalog_Animations.DefaultAnimatorClips.TryGetValue(slot, out key) || key == null) return null;
            var clip = aoc[key];
            return clip != null && clip != key ? clip.name : null;
        }

        // A clip by name: the character's own emotes, the game's library, then any clip loaded.
        internal static AnimationClip FindClip(Combatant_Base c, string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            var own = c.myAnimSet != null && c.myAnimSet.IdleEmotes != null ? c.myAnimSet.IdleEmotes.myClips : null;
            var a = own != null ? own.FirstOrDefault(x => x != null && x.name == name) : null;
            if (a != null) return a;
            var lib = LibraryStorageList.Library != null && LibraryStorageList.Library.AnimClips != null ? LibraryStorageList.Library.AnimClips.Storage : null;
            a = lib != null ? lib.OfType<AnimationClip>().FirstOrDefault(x => x != null && x.name == name) : null;
            return a ?? Resources.FindObjectsOfTypeAll<AnimationClip>().FirstOrDefault(x => x.name == name);
        }

        static void EndPose(Combatant_Base c)
        {
            bool wander;
            if (posing.TryGetValue(c, out wander) && c != null)
                c.wanderingDisabled = wander || GuardFeature.IsGuard(c) || PatrolFeature.HasRoute(c);
            posing.Remove(c);
        }

        void OnGUI()
        {
            if (!open || clips == null) return;
            Cursor.visible = true;
            float h = Screen.height * 0.7f, w = Mathf.Min(520f, Screen.width * 0.45f);
            GUILayout.BeginArea(new Rect(Screen.width - w - 20f, (Screen.height - h) / 2f, w, h), GUI.skin.box);
            GUILayout.Label("Idle animations for " + target.name + "  (F4 or ESC closes)");
            filter.Draw(clips.Count, true);
            if (GUILayout.Button("Stop the animation"))
            {
                target.AttemptEndEmote();
                EndPose(target);
            }
            scroll = GUILayout.BeginScrollView(scroll);
            foreach (var a in clips)
            {
                if (!filter.Pass(a.name)) continue;
                if (GUILayout.Button(a.name)) Play(a);
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        void OnDestroy()
        {
            if (open) Close();
            foreach (var c in posing.Keys.ToList()) EndPose(c);
        }
    }
}
