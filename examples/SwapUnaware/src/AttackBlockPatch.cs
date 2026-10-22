using HarmonyLib;

namespace SwapUnaware
{
    // The game has no "mouse is over a UI" guard: left click always attacks. While a mod tool owns
    // the mouse (placing or clicking patrol squares, or the Director menu waiting for a click),
    // skip the player's attack step for that frame.
    // Controller_Player.RunControls_Attacks() is protected, so it is named by string.
    [HarmonyPatch(typeof(Controller_Player), "RunControls_Attacks")]
    static class AttackBlockPatch
    {
        static bool Prefix()
        {
            return !Game.MouseBusy && !DirectorMenu.Picking;
        }
    }
}
