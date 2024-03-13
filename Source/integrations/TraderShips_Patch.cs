using HarmonyLib;
using RimWorld;
using Verse;
using System;
using System.Linq;
using System.Reflection;

namespace Arakos.CallATrader
{
    public static class TraderShips_Patch
    {
        private static Type type_CompShip = null;
        private static MethodInfo method_CompShip_GenerateInternalTradeShip = null;
        private static MethodInfo method_IncidentWorkerTraderShip_LandShip = null;

        public static void TryPatch(Harmony harmony)
        {
            ModContentPack mod_TraderShips = null;
            Assembly assembly_TraderShips = null;

            foreach (ModContentPack mod in LoadedModManager.RunningMods)
            {
                foreach (Assembly assembly in mod.assemblies.loadedAssemblies)
                {
                    if (assembly.GetName().Name.Equals("TraderShips"))
                    {
                        assembly_TraderShips = assembly;
                        mod_TraderShips = mod;
                        break;
                    }
                }
            }

            if (assembly_TraderShips != null)
            {
                try
                {
                    type_CompShip = assembly_TraderShips.GetType("TraderShips.CompShip", true);
                    method_CompShip_GenerateInternalTradeShip = type_CompShip.GetMethod("GenerateInternalTradeShip") 
                        ?? throw new Exception("Method TraderShips.CompShip.GenerateInternalTradeShip not found");
                    method_IncidentWorkerTraderShip_LandShip = assembly_TraderShips.GetType("TraderShips.IncidentWorkerTraderShip", true).GetMethod("LandShip") 
                        ?? throw new Exception("Method TraderShips.IncidentWorkerTraderShip.LandShip not found");

                    MethodInfo method_TryExecuteWorker = assembly_TraderShips.GetType("TraderShips.IncidentWorkerTraderShip", true)
                        .GetMethod("TryExecuteWorker", BindingFlags.Instance | BindingFlags.NonPublic, Type.DefaultBinder, new[] { typeof(IncidentParms) }, null)
                        ?? throw new Exception("Method TraderShips.IncidentWorkerTraderShip.TryExecuteWorker not found");

                    harmony.Patch(method_TryExecuteWorker, prefix: new HarmonyMethod(typeof(TraderShips_Patch).GetMethod(nameof(TraderShips_Patch.TryExecuteWorker_Patch))));

                    Log.Message($"Call A Trader successfully patched { mod_TraderShips.Name }.");
                }
                catch (Exception ex)
                {
                    Log.Warning($"Mod Call A Trader detected but failed to integrate with with Mod { mod_TraderShips.Name }. " +
                        $"Method patch was not applied, hence TraderShips will not necessarily fit to requested trader type. Reason: { ex }");
                }
            }
        }

        public static bool TryExecuteWorker_Patch(ref bool __result, IncidentParms parms)
        {
            try
            {
                Map map = (Map) parms.target;
                TraderKindDef traderKindDef = parms.traderKind;

                ThingWithComps ship = (ThingWithComps) ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("TraderShipsShip"));
                Object comp = ship.GetComps<ThingComp>().Where(c => Object.Equals(c.GetType(), type_CompShip)).First();
                method_CompShip_GenerateInternalTradeShip.Invoke(comp, new Object[] { map, traderKindDef });
                method_IncidentWorkerTraderShip_LandShip.Invoke(null, new Object[] { map, ship });

                Log.Message("Successfully executed patched 'TraderShips.IncidentWorkerTraderShip.TryExecuteWorker(..)' method");
                __result = true;
                return false;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to invoke patched 'TraderShips.IncidentWorkerTraderShip.TryExecuteWorker(..)' method! Invoking original method instead. " + ex);
                return true;
            }
        }

    }
}
