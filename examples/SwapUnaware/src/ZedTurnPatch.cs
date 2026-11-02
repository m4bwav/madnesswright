using System;
using System.Linq;
using HarmonyLib;

namespace SwapUnaware
{
    // A zed's grab marks its victim to rise as a zed (Combatant_Base.PrepareForCoversion); a moment
    // after death the game morphs it into the zed's card, keeping the clothes on screen only. Before
    // that swap, note the card and armor the victim had, so a scene save can bring the same look back.
    [HarmonyPatch(typeof(Combatant_Base), nameof(Combatant_Base.AttemptConvertToStoredCharacter))]
    static class ZedTurnPatch
    {
        static void Prefix(Combatant_Base __instance)
        {
            try
            {
                var d = __instance.myData;
                if (!__instance.WillConvertOnDeath() || d == null || d.myCard == null) return;
                var armor = d.armorSlots.Values.Where(a => a != null && !string.IsNullOrEmpty(a.myCardName))
                    .Select(a => SceneMemory.ArmorEntry(a.myArmorSlot.ToString(), a.myCardName));
                SceneMemory.Turn(__instance, d.myCard.name, armor);
                Game.Log("zed: " + __instance.name + " (" + d.myCard.name + ") turns");
            }
            catch (Exception e) { Game.Error("zed: note " + __instance.name, e); }
        }
    }
}
