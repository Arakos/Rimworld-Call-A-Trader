using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Arakos.CallATrader
{

    public class ICommunicable_OrbitalTradersHub : ICommunicable
    {

        // Note: this method gets invoked permanently while the comms console options menu is open
        public FloatMenuOption CommFloatMenuOption(Building_CommsConsole console, Pawn negotiator)
        {
            // get ticks the call trader action is disabled for
            int disabledForTicks = CallATrader.state.traderRequestActionDisabledUntil - Find.TickManager.TicksAbs;

            FloatMenuOption callTraderOption = new FloatMenuOption(
                (Constants.MOD_PREFIX + ".console.label").Translate(),
                () => negotiator.jobs.TryTakeOrderedJob(
                        JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(Constants.JOB_DEF_NAME, true), console), 
                        JobTag.MiscWork
                    ),
                Textures.ORBITAL_TRADER_HUB_ICON, 
                Color.white, 
                MenuOptionPriority.Default
            );

            // if still disabled don't show the option
            if (disabledForTicks > 0)
            {
                callTraderOption.Disabled = true;
                callTraderOption.Label = (Constants.MOD_PREFIX + ".console.label.disabled")
                    .Translate(GenDate.ToStringTicksToPeriod(disabledForTicks, shortForm: false));
            }
            else
            {
                // checks if the coms console is already reserved by another pawn
                // and displays that info on the menu option accordingly
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
