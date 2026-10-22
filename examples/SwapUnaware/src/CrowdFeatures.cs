using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SwapUnaware
{
    // F2: freeze everyone but you; F2 again lets go of the ones it froze.
    public class FreezeFeature : Feature
    {
        public KeyCode Key = KeyCode.F2;

        class Saved { public bool wasFrozen; public float animSpeed; }
        static readonly Dictionary<Combatant_Base, Saved> frozen = new Dictionary<Combatant_Base, Saved>();
        static FreezeFeature instance;

        protected override string Name { get { return "freeze (F2)"; } }
        protected override void Register() { Menu("Freeze everyone but you / unfreeze", Key); }

        void Awake() { instance = this; }

        internal static bool IsFrozen(Combatant_Base c) { return c != null && frozen.ContainsKey(c); }

        protected override void Tick()
        {
            if (Pressed(Key))
            {
                if (frozen.Count > 0) { int n = frozen.Count; ReleaseAll(); Game.Say(n + " characters unfrozen"); }
                else Game.Say(FreezeAll() + " characters frozen; F2 to unfreeze");
            }
            // Let go of the dead (so they ragdoll) and of a body the player has just taken over.
            var me = Game.Me();
            foreach (var c in frozen.Keys.ToList())
                if (c == null || c.AmDead() || c == me) Release(c);
        }

        static int FreezeAll()
        {
            var me = Game.Me();
            int n = 0;
            foreach (var c in FindObjectsOfType<Combatant_Base>())
            {
                if (!Game.Alive(c) || c == me || c.myController == null || frozen.ContainsKey(c)) continue;
                Freeze(c);
                n++;
            }
            return n;
        }

        // The cutscene lock stops the AI and movement input; waypoints would still move the body, so
        // they are cleared; SetFrozen pins the rigidbody; animator speed 0 holds the pose.
        internal static void Freeze(Combatant_Base c)
        {
            if (instance == null || frozen.ContainsKey(c)) return;
            var anim = c.myAnimator != null ? c.myAnimator.myAnim : null;
            frozen[c] = new Saved { wasFrozen = c.AmFrozen(), animSpeed = anim != null ? anim.speed : 1f };
            c.myController.LockController(instance);
            Pathfind_Main.ClearWaypointsAndTarget(c);
            c.SetFrozen(true);
            if (anim != null) anim.speed = 0f;
        }

        internal static void Release(Combatant_Base c)
        {
            Saved s;
            if (c == null || !frozen.TryGetValue(c, out s)) { frozen.Remove(c); return; }
            frozen.Remove(c);
            var anim = c.myAnimator != null ? c.myAnimator.myAnim : null;
            if (anim != null) anim.speed = s.animSpeed;
            if (!c.AmDead()) c.SetFrozen(s.wasFrozen || GuardFeature.IsGuard(c));
            if (c.myController != null) c.myController.ReleaseController(instance);
        }

        internal static void ReleaseAll()
        {
            foreach (var c in frozen.Keys.ToList()) Release(c);
            frozen.Clear();
        }

        void OnDestroy() { ReleaseAll(); }
    }

    // F1: removes every living character except you and the playable characters, the way the
    // Playground's Clear All removes them (no corpses).
    public class ClearFeature : Feature
    {
        public KeyCode Key = KeyCode.F1;

        protected override string Name { get { return "clear (F1)"; } }
        protected override void Register() { Menu("Remove every living character but you and the playable ones", Key); }

        protected override void Tick()
        {
            if (!Pressed(Key)) return;
            var me = Game.Me();
            var doomed = FindObjectsOfType<Combatant_Base>()
                .Where(c => Game.Alive(c) && c != me && c != SwapUnawareBehaviour.Home && !Game.Playable(c)).ToList();
            Game.PruneClaimants();
            int n = 0;
            foreach (var c in doomed)
            {
                try
                {
                    Game.ReleaseClaims(c.myController);
                    c.RemoveFromGame();
                    n++;
                }
                catch (System.Exception e) { Game.Error("clear " + c.name, e); }
            }
            Game.Say(n + " characters removed");
        }
    }

    // F10 on the character under the mouse: unaware -> search -> attack you -> unaware.
    public class AlertFeature : Feature
    {
        public KeyCode Key = KeyCode.F10;

        protected override string Name { get { return "alert cycle (F10)"; } }
        protected override void Register() { Menu("Alert cycle: unaware, searching, attacking you", Key, true); }

        protected override void Tick()
        {
            if (!Pressed(Key)) return;
            var c = Game.NpcUnderMouse();
            if (c == null) { Game.Say("no character under the mouse"); return; }
            if (FreezeFeature.IsFrozen(c)) { Game.Say(c.name + " is frozen; F2 to unfreeze"); return; }
            var ctl = c.myController;
            switch (ctl.myAlertStatus)
            {
                case AI_Wakeup.AlertStatus.Unaware:
                    Game.ReleaseClaims(ctl);
                    Controller_Base.SetAlertStatus(ctl, AI_Wakeup.AlertStatus.Hunting);
                    c.FlashColor(Color.yellow, 8f, 1f);
                    Game.Say(c.name + ": SEARCHING");
                    break;
                case AI_Wakeup.AlertStatus.Hunting:
                    Controller_Base.SetAlertStatus(ctl, AI_Wakeup.AlertStatus.Combat);
                    var me = Game.Me();
                    if (me != null && !me.AmDead()) AI_Manager.SetKillTarget(ctl, me);
                    c.FlashColor(Color.red, 8f, 1f);
                    Game.Say(c.name + ": ATTACKING you");
                    break;
                default:
                    Game.ReleaseClaims(ctl);
                    Controller_Base.SetAlertStatus(ctl, AI_Wakeup.AlertStatus.Unaware);
                    c.FlashColor(Color.white, 8f, 1f);
                    Game.Say(c.name + ": UNAWARE (lost track of you)");
                    break;
            }
        }
    }

    // F9 on the character under the mouse: stands guard (does not move, fights anyone not on its
    // team once it sees them), as the game's own spawner option FixedInPlace does. F9 again: normal.
    public class GuardFeature : Feature
    {
        public KeyCode Key = KeyCode.F9;

        static readonly Dictionary<Combatant_Base, bool> guards = new Dictionary<Combatant_Base, bool>();

        protected override string Name { get { return "guard (F9)"; } }
        protected override void Register() { Menu("Stand guard / stop guarding", Key, true); }

        internal static bool IsGuard(Combatant_Base c) { return c != null && guards.ContainsKey(c); }

        protected override void Tick()
        {
            if (Pressed(Key))
            {
                var c = Game.NpcUnderMouse();
                if (c == null) Game.Say("no character under the mouse");
                else if (guards.ContainsKey(c)) { Unguard(c); Game.Say(c.name + " stops guarding"); }
                else { Guard(c); Game.Say(c.name + " is guarding: stays put, attacks anyone not on its team it sees"); }
            }
            var me = Game.Me();
            foreach (var c in guards.Keys.ToList())
                if (c == null || c.AmDead() || c == me) Unguard(c);
        }

        internal static void Guard(Combatant_Base c)
        {
            if (guards.ContainsKey(c)) return;
            guards[c] = c.wanderingDisabled;
            c.wanderingDisabled = true;
            Pathfind_Main.ClearWaypointsAndTarget(c);
            c.SetFrozen(true);
            Game.ReleaseClaims(c.myController);
            Controller_Base.SetAlertStatus(c.myController, AI_Wakeup.AlertStatus.Unaware);
            c.FlashColor(Color.cyan, 8f, 1.5f);
        }

        internal static void Unguard(Combatant_Base c)
        {
            bool wander;
            if (c == null || !guards.TryGetValue(c, out wander)) { guards.Remove(c); return; }
            guards.Remove(c);
            c.wanderingDisabled = wander || PatrolFeature.HasRoute(c);
            if (!c.AmDead() && !FreezeFeature.IsFrozen(c)) c.SetFrozen(false);
        }

        void OnDestroy() { foreach (var c in guards.Keys.ToList()) Unguard(c); }
    }
}
