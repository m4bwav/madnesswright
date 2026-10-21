using System;
using UnityEngine;

namespace SwapUnaware
{
    // F5 take control, F6 everyone else unaware, F7 keep them unaware, F8 back home.
    // Unchanged behaviour from 0.3.0; the shared helpers moved to Game.
    public class SwapUnawareBehaviour : Feature
    {
        public KeyCode SwapKey = KeyCode.F5;
        public KeyCode UnawareKey = KeyCode.F6;
        public KeyCode KeepUnawareKey = KeyCode.F7;
        public KeyCode HomeKey = KeyCode.F8;

        const float KeepInterval = 0.5f;

        // The body the player had before the first swap. Unity turns it null when the scene unloads.
        static Combatant_Base home;
        internal static Combatant_Base Home { get { return home; } }
        static bool keepUnaware;
        float nextKeep;

        protected override string Name { get { return "swap (F5-F8)"; } }

        protected override void Tick()
        {
            if (Input.GetKeyDown(SwapKey)) SwapToCharacterUnderMouse();
            if (Input.GetKeyDown(HomeKey)) GoHome();
            if (Input.GetKeyDown(UnawareKey)) MakeOthersUnaware(true);
            if (Input.GetKeyDown(KeepUnawareKey))
            {
                keepUnaware = !keepUnaware;
                Game.Say("keep unaware " + (keepUnaware ? "ON" : "OFF"));
                if (keepUnaware) MakeOthersUnaware(true);
            }
            if (keepUnaware && Time.unscaledTime >= nextKeep)
            {
                nextKeep = Time.unscaledTime + KeepInterval;
                MakeOthersUnaware(false);
            }
        }

        // Works while the player is dead too, and reaches back out of a locked arena.
        static void SwapToCharacterUnderMouse()
        {
            var pc = Controller_Player.PlayerController;
            if (pc == null || pc.myCombatant == null) return;
            var target = Game.NpcUnderMouse();
            if (target == null)
            {
                Game.Say("no character under the mouse");
                return;
            }
            if (home == null || home.AmDead()) home = pc.myCombatant;
            SwapTo(pc, target);
        }

        static void GoHome()
        {
            var pc = Controller_Player.PlayerController;
            if (pc == null) return;
            if (home == null) { Game.Say("no home body to return to"); return; }
            if (home == pc.myCombatant) { Game.Say("already home"); return; }
            if (!Game.IsNpc(home)) { Game.Say("your home body is dead; F5 another character"); return; }
            SwapTo(pc, home);
        }

        // Called by the patch when the controlled body dies or leaves the squad.
        internal static bool TryReturnHomeOnLoss(Controller_Player pc)
        {
            if (pc != Controller_Player.PlayerController || home == null || home == pc.myCombatant || !Game.IsNpc(home)) return false;
            SwapTo(pc, home);
            return true;
        }

        internal static void SwapTo(Controller_Player pc, Combatant_Base target)
        {
            // Controller_Base.LostConnection nulls myOwner and then unclaims its kill target, and
            // the claimer re-scores every remaining claimant by reading C.myOwner. A controller listed
            // twice (ClaimTarget does not check) is still there with a null owner and throws, halfway
            // through the swap. Release both controllers' claims first, as the game's KillMe does.
            // A frozen or guarding body holds a lock and a pinned rigidbody on the controller the swap
            // replaces; let go first, or the lock would move to whichever body gets that controller.
            FreezeFeature.Release(target);
            GuardFeature.Unguard(target);
            Game.PruneClaimants();
            Game.ReleaseClaims(target.myController);
            Game.ReleaseClaims(pc);
            try
            {
                DoSwap(pc, target);
            }
            catch (Exception e)
            {
                Game.Error("swap failed", e);
                Repair(pc, target);
                return;
            }
            target.FlashColor(Color.green, 8f, 2f);
            Game.Say("now controlling " + target.name + (target == home ? " (home)" : ""));
            if (keepUnaware) MakeOthersUnaware(false);
        }

        static void DoSwap(Controller_Player pc, Combatant_Base target)
        {
            var me = pc.myCombatant;
            var mySquad = me != null && me.myData != null ? me.myData.mySquad : null;
            if (mySquad != null && target.myData != null && target.myData.mySquad == mySquad)
            {
                // Same squad: the game's own swap (its core-roster check only lives in CanBePlayerControlled).
                pc.SwapControllersWithSquadmate(target);
            }
            else
            {
                // Another squad, or none (Playground spawns can have none): the core of the game's
                // swap without the squad-leader and arena-clone steps, which assume a shared squad.
                if (me != null)
                {
                    UI_Game.UnLinkCharacter(me);
                    me.CancelFlash();
                }
                target.AssignController(pc, overrideCurrentController: true);
                UI_Game.ApplySquadmates();
                Cam_Main.CancelZoom();
            }
        }

        // A swap that threw can leave a body holding a controller with no owner (frozen NPC)
        // or no controller at all. Reconnect what can be reconnected and say what happened.
        static void Repair(Controller_Player pc, Combatant_Base target)
        {
            try
            {
                foreach (var c in FindObjectsOfType<Combatant_Base>())
                {
                    var ctl = c.myController;
                    if (c.AmDead() || ctl == null || ctl.myCombatant == c) continue;
                    if (ctl.myCombatant == null)
                    {
                        ctl.SetConnection(c);
                        Game.Say("repaired controller on " + c.name);
                    }
                }
            }
            catch (Exception e)
            {
                Game.Error("repair failed", e);
            }
            Game.Say(pc.myCombatant == target ? "swap finished despite the error" : "swap failed; still controlling " + (pc.myCombatant != null ? pc.myCombatant.name : "nobody"));
        }

        // Every living NPC other than the player's character goes back to Unaware
        // and forgets its current target.
        static void MakeOthersUnaware(bool report)
        {
            var me = Game.Me();
            int n = 0;
            Game.PruneClaimants();
            foreach (var c in FindObjectsOfType<Combatant_Base>())
            {
                if (c == me || !Game.IsNpc(c)) continue;
                // Through the game's unclaim: nulling myKillTarget by hand left the controller in its
                // target's claimant list, the AI claimed again, and the duplicate broke the next swap.
                Game.ReleaseClaims(c.myController);
                Controller_Base.SetAlertStatus(c.myController, AI_Wakeup.AlertStatus.Unaware);
                n++;
            }
            if (report) Game.Say(n + " characters unaware");
        }

        internal static void Say(string msg) { Game.Say(msg); }
    }
}
