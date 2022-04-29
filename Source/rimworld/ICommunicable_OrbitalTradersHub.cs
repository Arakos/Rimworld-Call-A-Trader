using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Arakos.CallATrader
{

    public class ICommunicable_OrbitalTradersHub : ICommunicable
    {
        private readonly FloatMenuOption callTraderOption = new FloatMenuOption("", null, Textures.ORBITAL_TRADER_HUB_ICON, Color.white, MenuOptionPriority.Default);

        private Building_CommsConsole cachedConsole;

        private Pawn cachedPawn;

        // this method gets invoked permanently while the comms console options menu is open - hence use caching
        public FloatMenuOption CommFloatMenuOption(Building_CommsConsole console, Pawn negotiator)
        {

            bool cacheValid = System.Object.Equals(cachedConsole, console) && System.Object.Equals(cachedPawn, negotiator);
            cachedPawn = negotiator;
            cachedConsole = console;

            // get ticks the call trader action is disabled for
            int disabledForTicks = CallATrader.state.traderRequestActionDisabledUntil - Find.TickManager.TicksAbs;

            // must be done this way because the impl of disabled in FloatMenuOption is retarded
            if (disabledForTicks > 0)
            {
                callTraderOption.Disabled = true;
                callTraderOption.Label = (Constants.MOD_PREFIX + ".console.label.disabled").Translate(GenDate.ToStringTicksToPeriod(disabledForTicks, shortForm: false));
            }
            else if (callTraderOption.Disabled || !cacheValid)
            {
                callTraderOption.Disabled = false;
                callTraderOption.Label = (Constants.MOD_PREFIX + ".console.label").Translate();
                callTraderOption.action = () => negotiator.jobs.TryTakeOrderedJob(JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(Constants.JOB_DEF_NAME, true), console), JobTag.MiscWork);
                FloatMenuUtility.DecoratePrioritizedTask(callTraderOption, negotiator, console);
            }
            return callTraderOption;
        }

        public string GetCallLabel()
        {
            return "";
        }

        public Faction GetFaction()
        {
            Log.Message("getfactioncalled");
            return null;
        }

        public string GetInfoText()
        {
            return "";
        }

        public void TryOpenComms(Pawn negotiator)
        {
            Log.Message("tryopencomms " + negotiator);
        }
    }
}
