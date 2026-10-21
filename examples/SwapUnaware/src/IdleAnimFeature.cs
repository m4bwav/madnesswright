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
        string filter = "";
        Vector2 scroll;
        bool open;

        // Characters posing, with their wandering flag from before (wandering would cancel the pose).
        static readonly Dictionary<Combatant_Base, bool> posing = new Dictionary<Combatant_Base, bool>();

        protected override string Name { get { return "idle animations (F4)"; } }

        protected override void Tick()
        {
            if (Input.GetKeyDown(Key))
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
            filter = "";
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
            if (!posing.ContainsKey(c)) posing[c] = c.wanderingDisabled;
            Game.ReleaseClaims(c.myController);
            Controller_Base.SetAlertStatus(c.myController, AI_Wakeup.AlertStatus.Unaware);
            c.wanderingDisabled = true;
            Pathfind_Main.ClearWaypointsAndTarget(c);
            bool ok = c.PlayAsEmote(clip, true, false);
            Game.Say(ok ? c.name + " plays " + clip.name : c.name + " cannot play that now");
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
            GUILayout.BeginHorizontal();
            GUILayout.Label("Filter:", GUILayout.Width(50f));
            filter = GUILayout.TextField(filter);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Stop the animation"))
            {
                target.AttemptEndEmote();
                EndPose(target);
            }
            scroll = GUILayout.BeginScrollView(scroll);
            string f = filter.ToLowerInvariant();
            foreach (var a in clips)
            {
                if (f.Length > 0 && !a.name.ToLowerInvariant().Contains(f)) continue;
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
