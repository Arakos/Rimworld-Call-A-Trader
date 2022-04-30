using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Arakos.CallATrader
{

    [HarmonyPatch(typeof(Building_CommsConsole), nameof(Building_CommsConsole.GetCommTargets))]
    public static class Building_CommsConsole_Patch
    {
        private static readonly ICommunicable callTraderCommunicable = new ICommunicable_OrbitalTradersHub();

        // injects additional option into the ICommunicable list on the comms console
        [HarmonyPostfix]
        public static void GetCommTargetsPatch(Pawn myPawn, ref IEnumerable<ICommunicable> __result)
        {
            __result = __result.AddItem(callTraderCommunicable);
        }
    }

}
