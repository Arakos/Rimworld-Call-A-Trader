using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
//using Multiplayer.API;

namespace Arakos.CallATrader
{

    [StaticConstructorOnStartup]
    class Main
    {

        static Main()
        {
            var harmony = new Harmony("com.github.arakos.hdk_callatrader");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            //if (MP.enabled)
            //{
            //    MP.RegisterAll(Assembly.GetExecutingAssembly());
            //}
        }
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Pawn), typeof(List<FloatMenuOption>) })]
    public static class FloatMenuMakerMap_AddHumanLikeOrders_Postfix
    {
        private static readonly FloatMenuOption callTraderOption = new FloatMenuOption((Constants.MOD_PREFIX + ".console.label").Translate(), null, Constants.ORBITAL_TRADER_HUB_ICON, Color.white, MenuOptionPriority.Default);

        // care: this method gets invoked permanently while the UI window is open
        [HarmonyPostfix]
        public static void AddHumanlikeOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            foreach (var thing in IntVec3.FromVector3(clickPos).GetThingList(pawn.Map))
            {
                if (thing is Building_CommsConsole)
                {
                    if(!opts.Contains(callTraderOption))
                    {
                        opts.Add(callTraderOption);
                    }

                    // must be done this way because the impl of disabled in FloatMenuOption is retarded
                    if(CallATrader.state.traderRequestActionDisabledUntil > Find.TickManager.TicksAbs)
                    {
                        callTraderOption.Disabled = true;
                    }
                    else
                    {
                        callTraderOption.Disabled = false;
                        callTraderOption.action = () => pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(Constants.JOB_DEF_NAME, true), thing), JobTag.MiscWork, true);
                    }
                    return;
                }
            }
        }
    }
}
