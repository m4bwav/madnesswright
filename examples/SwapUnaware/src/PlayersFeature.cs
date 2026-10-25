using UnityEngine;

namespace SwapUnaware
{
    // Local co-op: a pad joins with Start (Joystick_Manager), takes a free squadmate of yours, or
    // waits for the next door, where story stages add a bonus character. One Player.log line each
    // time a player joins, leaves or changes character, so a two-player play can be read back.
    // No keys: Director's keys act for the keyboard player.
    public class PlayersFeature : Feature
    {
        readonly Combatant_Base[] last = new Combatant_Base[4];
        readonly bool[] active = new bool[4];

        protected override string Name { get { return "players"; } }
        protected override bool RunWhilePaused { get { return true; } }

        protected override void Tick()
        {
            var pcs = Controller_Player.PlayerControllers;
            for (int i = 0; i < last.Length && i < pcs.Length; i++)
            {
                bool on = i == 0 || Joystick_Manager.ReturnPlayerActive(i);
                if (on != active[i] && i > 0)
                    Game.Log("player " + (i + 1) + (on ? " joined on pad " + Joystick_Manager.ReturnJoystickOfPlayer(i) : " left"));
                active[i] = on;
                var c = pcs[i] != null ? pcs[i].myCombatant : null;
                if (c == last[i]) continue;
                last[i] = c;
                if (i == 0 && c == null) continue;
                Game.Log("player " + (i + 1) + (c != null ? " now plays " + c.name + " (" + SceneData.SquadOf(c) + ")" : " has no character"));
            }
        }
    }
}
