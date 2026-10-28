using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SwapUnaware
{
    // F2: freeze everyone but you and the other players; F2 again lets go of the ones it froze.
    public class FreezeFeature : Feature
    {
        public KeyCode Key = KeyCode.F2;

        // controller: the one controller Director has locked for this body (at most one).
        // held: the body is pinned now; false while a player drives it.
        class Saved { public bool wasFrozen; public float animSpeed; public Controller_Base controller; public bool held; }
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
            // Dead bodies must ragdoll; live bodies keep their freeze across controller changes. Every
            // stale lock is let go before any new one is taken: when the game swaps two frozen bodies'
            // controllers, releasing one body's old controller would otherwise drop the other's new lock.
            var bodies = frozen.Keys.ToList();
            foreach (var c in bodies)
                if (c == null || c.AmDead()) Release(c);
                else Unbind(c);
            foreach (var c in bodies)
                if (c != null && frozen.ContainsKey(c)) Apply(c);
        }

        static int FreezeAll()
        {
            int n = 0;
            foreach (var c in FindObjectsOfType<Combatant_Base>())
            {
                if (!Game.Alive(c) || Game.IsPlayer(c) || c.myController == null || frozen.ContainsKey(c)) continue;
                Freeze(c);
                n++;
            }
            return n;
        }

        // The cutscene lock stops the AI and movement input; waypoints would still move the body, so
        // they are cleared; SetFrozen pins the rigidbody; animator speed 0 holds the pose.
        // A body any player controls is never held: it stays on the list and is held again when the
        // player leaves it.
        internal static void Freeze(Combatant_Base c)
        {
            if (instance == null || frozen.ContainsKey(c)) return;
            var anim = c.myAnimator != null ? c.myAnimator.myAnim : null;
            frozen[c] = new Saved { wasFrozen = c.AmFrozen(), animSpeed = anim != null ? anim.speed : 1f };
            Rebind(c);
        }

        internal static void Detach(Combatant_Base c)
        {
            Saved s;
            if (!frozen.TryGetValue(c, out s) || s.controller == null) return;
            s.controller.ReleaseController(instance);
            s.controller = null;
        }

        internal static void Rebind(Combatant_Base c)
        {
            Unbind(c);
            Apply(c);
        }

        // Lets go of a lock on a controller the body no longer has, or on a player's controller.
        static void Unbind(Combatant_Base c)
        {
            Saved s;
            if (!frozen.TryGetValue(c, out s) || s.controller == null) return;
            if (s.controller != c.myController || Game.IsPlayer(c)) Detach(c);
        }

        static void Apply(Combatant_Base c)
        {
            Saved s;
            if (!frozen.TryGetValue(c, out s)) return;
            var anim = c.myAnimator != null ? c.myAnimator.myAnim : null;
            if (Game.IsPlayer(c))
            {
                if (!s.held) return;
                s.held = false;
                c.SetFrozen(false);
                if (anim != null) anim.speed = s.animSpeed;
                return;
            }
            bool changed = !s.held;
            if (s.controller == null && c.myController != null)
            {
                s.controller = c.myController;
                s.controller.LockController(instance);
                changed = true;
            }
            if (!changed) return;
            if (!s.held) Pathfind_Main.ClearWaypointsAndTarget(c);
            s.held = true;
            c.SetFrozen(true);
            if (anim != null) anim.speed = 0f;
        }

        internal static void KeepFrozen(Combatant_Base c)
        {
            if (!IsFrozen(c) || Game.IsPlayer(c)) return;
            c.SetFrozen(true);
            var anim = c.myAnimator != null ? c.myAnimator.myAnim : null;
            if (anim != null) anim.speed = 0f;
        }

        internal static void Release(Combatant_Base c)
        {
            Saved s;
            if (c == null || !frozen.TryGetValue(c, out s)) { frozen.Remove(c); return; }
            frozen.Remove(c);
            var anim = c.myAnimator != null ? c.myAnimator.myAnim : null;
            if (anim != null) anim.speed = s.animSpeed;
            if (!c.AmDead() && !Game.IsPlayer(c)) c.SetFrozen(s.wasFrozen || GuardFeature.IsGuard(c));
            if (s.controller != null) s.controller.ReleaseController(instance);
        }

        internal static void ReleaseAll()
        {
            foreach (var c in frozen.Keys.ToList()) Release(c);
            frozen.Clear();
            // Safety net: no player controller keeps Director's lock, even one the bookkeeping lost.
            if (instance == null) return;
            foreach (var pc in Controller_Player.PlayerControllers)
                if (pc != null) pc.ReleaseController(instance);
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
            FreezeFeature.KeepFrozen(c);
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
            foreach (var c in guards.Keys.ToList())
                if (c == null || c.AmDead() || Game.IsPlayer(c)) Unguard(c);
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
