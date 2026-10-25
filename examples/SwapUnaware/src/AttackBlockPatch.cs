using HarmonyLib;

namespace SwapUnaware
{
    // The game has no "mouse is over a UI" guard: left click always attacks. While a mod tool owns
    // the mouse (placing or clicking patrol squares, or the Director menu waiting for a click),
    // skip the keyboard player's attack step for that frame. A player on a pad (local co-op) does
    // not use the mouse, so their attacks always go through.
    // Controller_Player.RunControls_Attacks() is protected, so it is named by string.
    [HarmonyPatch(typeof(Controller_Player), "RunControls_Attacks")]
    static class AttackBlockPatch
    {
        static bool Prefix(Controller_Player __instance)
        {
            if (__instance != Controller_Player.PlayerController) return true;
            return !Game.MouseBusy && !DirectorMenu.Picking;
        }
    }
}
