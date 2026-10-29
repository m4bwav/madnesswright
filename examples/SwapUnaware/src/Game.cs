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

        // A body any player controls: the keyboard player (Me) or one who joined on a pad (local
        // co-op: Controller_Player.PlayerControllers 1 to 3). Director's keys act for the keyboard
        // player; every feature leaves the others' bodies alone.
        internal static bool IsPlayer(Combatant_Base c)
        {
            return c != null && c.myController != null && c.myController.AmType_Player();
        }

        // 1 to 4 for a player's body, 0 for anyone else.
        internal static int PlayerNumber(Combatant_Base c)
        {
            var pc = c != null ? c.myController as Controller_Player : null;
            return pc != null ? pc.PlayerNumber + 1 : 0;
        }

        // The living bodies of the players other than the keyboard player.
        internal static List<Combatant_Base> OtherPlayers()
        {
            var list = new List<Combatant_Base>();
            var me = Me();
            foreach (var pc in Controller_Player.PlayerControllers)
                if (pc != null && pc.myCombatant != null && pc.myCombatant != me && Alive(pc.myCombatant)) list.Add(pc.myCombatant);
            return list;
        }

        // A combatant the game has not set up yet has an empty corpus track, and AmDead() then
        // throws IndexOutOfRangeException (seen during a story-level save).
        internal static bool Alive(Combatant_Base c)
        {
            if (c == null || c.myData == null) return false;
            var track = c.MyCorpusTrack;
            return track != null && track.Length > 0 && !c.AmDead();
        }

        // A body the game has finished killing and not yet despawned (the developer console's test).
        internal static bool Corpse(Combatant_Base c)
        {
            if (c == null || c.myData == null || c.AmDespawning) return false;
            var track = c.MyCorpusTrack;
            return track != null && track.Length > 0 && c.HaveRunKillMe;
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
            if (CareerCharacter(c)) return true;
            return d.myCard != null && d.myCard.DefaultFaction == Factions.Player;
        }

        // A player character or one of the career's core roster: its squad is the career's business.
        internal static bool CareerCharacter(Combatant_Base c)
        {
            var d = c != null ? c.myData : null;
            if (d == null) return false;
            if (d.ReturnAmPlayerCharacter()) return true;
            var career = Game_Manager.currentCareer;
            return career != null && career.chars_Core != null && career.chars_Core.Contains(d);
        }

        internal static Camera Cam()
        {
            return Cam_Main.gameCam != null && Cam_Main.gameCam.myCamera != null ? Cam_Main.gameCam.myCamera : Camera.main;
        }

        // The living NPC drawn nearest the mouse pointer on screen, in any room. The game's own
        // Combat_Manager.ReturnClosestTarget only accepts targets in the caller's room.
        internal static Combatant_Base NpcUnderMouse() { return UnderMouse(IsNpc); }

        // The same for any living character, players included.
        internal static Combatant_Base CharacterUnderMouse() { return UnderMouse(Alive); }

        static Combatant_Base UnderMouse(Func<Combatant_Base, bool> which)
        {
            var cam = Cam();
            if (cam == null) return null;
            Vector2 mouse = Input.mousePosition;
            float best = PickPixels * Screen.height / 1080f;
            Combatant_Base target = null;
            foreach (var c in UnityEngine.Object.FindObjectsOfType<Combatant_Base>())
            {
                if (!which(c)) continue;
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

        internal static void Log(string msg)
        {
            Debug.Log("[" + ModEntry.Id + "] " + msg);
        }

        internal static void Error(string where, Exception e)
        {
            Debug.LogError("[" + ModEntry.Id + "] " + where + ": " + e);
        }
    }
}
