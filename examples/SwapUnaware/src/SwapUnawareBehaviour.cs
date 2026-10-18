using System;
using UnityEngine;

namespace SwapUnaware
{
    // Polls the keys each frame. Every game member used here was found in the
    // decompile of beta build 25615196 (v2.20.a); re-check after game updates.
    public class SwapUnawareBehaviour : MonoBehaviour
    {
        public KeyCode SwapKey = KeyCode.F5;
        public KeyCode UnawareKey = KeyCode.F6;
        public KeyCode KeepUnawareKey = KeyCode.F7;
        public KeyCode HomeKey = KeyCode.F8;

        // Pick radius in pixels at 1080p, scaled to the screen height.
        const float PickPixels = 90f;
        const float KeepInterval = 0.5f;

        // The body the player had before the first swap. Unity turns it null when the scene unloads.
        static Combatant_Base home;
        static bool keepUnaware;
        float nextKeep;

        void Update()
        {
            try
            {
                if (Input.GetKeyDown(SwapKey)) SwapToCharacterUnderMouse();
                if (Input.GetKeyDown(HomeKey)) GoHome();
                if (Input.GetKeyDown(UnawareKey)) MakeOthersUnaware(true);
                if (Input.GetKeyDown(KeepUnawareKey))
                {
                    keepUnaware = !keepUnaware;
                    Say("keep unaware " + (keepUnaware ? "ON" : "OFF"));
                    if (keepUnaware) MakeOthersUnaware(true);
                }
                if (keepUnaware && Time.unscaledTime >= nextKeep)
                {
                    nextKeep = Time.unscaledTime + KeepInterval;
                    MakeOthersUnaware(false);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[" + ModEntry.Id + "] " + e);
            }
        }

        static Combatant_Base Me()
        {
            var pc = Controller_Player.PlayerController;
            return pc != null ? pc.myCombatant : null;
        }

        static bool Controllable(Combatant_Base c)
        {
            return c != null && !c.AmDead() && c.myController != null && c.myController.AmType_NPC();
        }

        // Picks the living NPC drawn nearest the mouse pointer on screen, in any room,
        // so a swap can reach back out of a locked arena. Works while the player is dead too.
        static void SwapToCharacterUnderMouse()
        {
            var pc = Controller_Player.PlayerController;
            if (pc == null || pc.myCombatant == null) return;
            var cam = Cam_Main.gameCam != null && Cam_Main.gameCam.myCamera != null ? Cam_Main.gameCam.myCamera : Camera.main;
            if (cam == null) return;

            Vector2 mouse = Input.mousePosition;
            float best = PickPixels * Screen.height / 1080f;
            Combatant_Base target = null;
            foreach (var c in FindObjectsOfType<Combatant_Base>())
            {
                if (!Controllable(c)) continue;
                Vector3 sp = cam.WorldToScreenPoint(c.Position + Vector3.up);
                if (sp.z <= 0f) continue;
                float d = Vector2.Distance(mouse, sp);
                if (d < best) { best = d; target = c; }
            }
            if (target == null)
            {
                Say("no character under the mouse");
                return;
            }
            if (home == null || home.AmDead()) home = pc.myCombatant;
            SwapTo(pc, target);
        }

        static void GoHome()
        {
            var pc = Controller_Player.PlayerController;
            if (pc == null) return;
            if (home == null) { Say("no home body to return to"); return; }
            if (home == pc.myCombatant) { Say("already home"); return; }
            if (!Controllable(home)) { Say("your home body is dead; F5 another character"); return; }
            SwapTo(pc, home);
        }

        // Called by the patch when the controlled body dies or leaves the squad.
        internal static bool TryReturnHomeOnLoss(Controller_Player pc)
        {
            if (pc != Controller_Player.PlayerController || home == null || home == pc.myCombatant || !Controllable(home)) return false;
            SwapTo(pc, home);
            return true;
        }

        static void SwapTo(Controller_Player pc, Combatant_Base target)
        {
            // Controller_Base.LostConnection nulls myOwner and then unclaims its kill target, and
            // the claimer re-scores every remaining claimant by reading C.myOwner. A controller listed
            // twice (ClaimTarget does not check) is still there with a null owner and throws, halfway
            // through the swap. Release both controllers' claims first, as the game's KillMe does.
            PruneClaimants();
            ReleaseClaims(target.myController);
            ReleaseClaims(pc);
            try
            {
                DoSwap(pc, target);
            }
            catch (Exception e)
            {
                Debug.LogError("[" + ModEntry.Id + "] swap failed: " + e);
                Repair(pc, target);
                return;
            }
            target.FlashColor(Color.green, 8f, 2f);
            Say("now controlling " + target.name + (target == home ? " (home)" : ""));
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

        // Takes the controller out of its kill target's claimant lists (every copy), then lets
        // the game's own UnclaimMyTarget fix the counts, re-score the rest and clear the target.
        static void ReleaseClaims(Controller_Base c)
        {
            if (c == null || c.myKillTarget == null) return;
            var claimer = c.myKillTarget.myTargetClaimer;
            if (claimer != null)
            {
                claimer.myClaimants.RemoveAll(x => x == c);
                claimer.myMeleeClaimants.RemoveAll(x => x == c);
            }
            AI_TargetClaimer.UnclaimMyTarget(c);
        }

        // Drops claimants whose body is gone and duplicate entries, which make the claimer throw.
        static void PruneClaimants()
        {
            foreach (var claimer in FindObjectsOfType<AI_TargetClaimer>())
            {
                Prune(claimer.myClaimants);
                Prune(claimer.myMeleeClaimants);
            }
        }

        static void Prune(System.Collections.Generic.List<Controller_Base> list)
        {
            var seen = new System.Collections.Generic.HashSet<Controller_Base>();
            list.RemoveAll(x => x == null || x.myOwner == null || !seen.Add(x));
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
                        Say("repaired controller on " + c.name);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[" + ModEntry.Id + "] repair failed: " + e);
            }
            Say(pc.myCombatant == target ? "swap finished despite the error" : "swap failed; still controlling " + (pc.myCombatant != null ? pc.myCombatant.name : "nobody"));
        }

        // Every living NPC other than the player's character goes back to Unaware
        // and forgets its current target.
        static void MakeOthersUnaware(bool report)
        {
            var me = Me();
            int n = 0;
            PruneClaimants();
            foreach (var c in FindObjectsOfType<Combatant_Base>())
            {
                if (c == me || !Controllable(c)) continue;
                // Through the game's unclaim: nulling myKillTarget by hand left the controller in its
                // target's claimant list, the AI claimed again, and the duplicate broke the next swap.
                ReleaseClaims(c.myController);
                Controller_Base.SetAlertStatus(c.myController, AI_Wakeup.AlertStatus.Unaware);
                n++;
            }
            if (report) Say(n + " characters unaware");
        }

        internal static void Say(string msg)
        {
            Debug.Log("[" + ModEntry.Id + "] " + msg);
        }
    }
}
