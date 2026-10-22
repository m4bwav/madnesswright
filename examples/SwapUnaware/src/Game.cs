using System;
using System.Collections.Generic;
using UnityEngine;

namespace SwapUnaware
{
    // Helpers every feature shares. Every game member used here was found in the decompile of
    // beta build 25615196 (v2.20.a); re-check after game updates.
    static class Game
    {
        // Pick radius in pixels at 1080p, scaled to the screen height.
        const float PickPixels = 90f;

        // True while a mod tool uses the left mouse button; AttackBlockPatch then skips the player's attack.
        internal static bool MouseBusy;

        internal static Combatant_Base Me()
        {
            var pc = Controller_Player.PlayerController;
            return pc != null ? pc.myCombatant : null;
        }

        // A combatant the game has not set up yet has an empty corpus track, and AmDead() then
        // throws IndexOutOfRangeException (seen during a story-level save).
        internal static bool Alive(Combatant_Base c)
        {
            if (c == null || c.myData == null) return false;
            var track = c.MyCorpusTrack;
            return track != null && track.Length > 0 && !c.AmDead();
        }

        internal static bool IsNpc(Combatant_Base c)
        {
            return Alive(c) && c.myController != null && c.myController.AmType_NPC();
        }

        // Spared by F1: anyone a player controls, the career's core roster, and characters whose card
        // belongs to the Player faction (Sanford, Deimos, Hank and the other player characters).
        internal static bool Playable(Combatant_Base c)
        {
            if (c.amPlayer) return true;
            var d = c.myData;
            if (d == null) return false;
            if (d.ReturnAmPlayerCharacter()) return true;
            var career = Game_Manager.currentCareer;
            if (career != null && career.chars_Core != null && career.chars_Core.Contains(d)) return true;
            return d.myCard != null && d.myCard.DefaultFaction == Factions.Player;
        }

        internal static Camera Cam()
        {
            return Cam_Main.gameCam != null && Cam_Main.gameCam.myCamera != null ? Cam_Main.gameCam.myCamera : Camera.main;
        }

        // The living NPC drawn nearest the mouse pointer on screen, in any room. The game's own
        // Combat_Manager.ReturnClosestTarget only accepts targets in the caller's room.
        internal static Combatant_Base NpcUnderMouse()
        {
            var cam = Cam();
            if (cam == null) return null;
            Vector2 mouse = Input.mousePosition;
            float best = PickPixels * Screen.height / 1080f;
            Combatant_Base target = null;
            foreach (var c in UnityEngine.Object.FindObjectsOfType<Combatant_Base>())
            {
                if (!IsNpc(c)) continue;
                Vector3 sp = cam.WorldToScreenPoint(c.Position + Vector3.up);
                if (sp.z <= 0f) continue;
                float d = Vector2.Distance(mouse, sp);
                if (d < best) { best = d; target = c; }
            }
            return target;
        }

        // Takes the controller out of its kill target's claimant lists (every copy), then lets
        // the game's own UnclaimMyTarget fix the counts, re-score the rest and clear the target.
        // Never null myKillTarget by hand: the stale claimant broke later swaps (L-002).
        internal static void ReleaseClaims(Controller_Base c)
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
        internal static void PruneClaimants()
        {
            foreach (var claimer in UnityEngine.Object.FindObjectsOfType<AI_TargetClaimer>())
            {
                Prune(claimer.myClaimants);
                Prune(claimer.myMeleeClaimants);
            }
        }

        static void Prune(List<Controller_Base> list)
        {
            var seen = new HashSet<Controller_Base>();
            list.RemoveAll(x => x == null || x.myOwner == null || !seen.Add(x));
        }

        internal static void Say(string msg)
        {
            Debug.Log("[" + ModEntry.Id + "] " + msg);
            Hud.Show(msg);
        }

        internal static void Error(string where, Exception e)
        {
            Debug.LogError("[" + ModEntry.Id + "] " + where + ": " + e);
        }
    }
}
