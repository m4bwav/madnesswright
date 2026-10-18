using HarmonyLib;

namespace SwapUnaware
{
    // Controller_Player.SwapControllersWithSquadmate(bool) runs with overrideChecks=true when the
    // controlled body dies (Combatant_Base.KillMe) or leaves the player's squad (Squad_Main.RemoveMember).
    // It reads myCombatant.myData.mySquad without a null check, and Playground spawns can have no
    // squad, so taking over one and dying threw inside KillMe and left the death half-processed.
    [HarmonyPatch(typeof(Controller_Player), nameof(Controller_Player.SwapControllersWithSquadmate), new[] { typeof(bool) })]
    static class LossSwapPatch
    {
        static bool Prefix(Controller_Player __instance, bool overrideChecks, ref bool __result)
        {
            if (!overrideChecks) return true;
            try
            {
                if (SwapUnawareBehaviour.TryReturnHomeOnLoss(__instance))
                {
                    __result = true;
                    return false;
                }
                var body = __instance.myCombatant;
                if (body == null || body.myData == null || body.myData.mySquad == null)
                {
                    SwapUnawareBehaviour.Say("controlled body had no squad; press F5 on someone or F8 to go home");
                    __result = false;
                    return false;
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError("[" + ModEntry.Id + "] " + e);
            }
            return true;
        }
    }
}
