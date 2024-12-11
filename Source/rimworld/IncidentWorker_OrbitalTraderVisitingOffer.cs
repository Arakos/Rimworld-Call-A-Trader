using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;
using Verse.Noise;

namespace Arakos.CallATrader
{
    public class IncidentWorker_OrbitalTraderVisitingOffer : IncidentWorker
    {

        private static readonly String INCIDENT = ".incident.";

        private static readonly String CommsConsoleDef = "CommsConsole";

        private static readonly int HARDCODED_SHIP_LIMIT = 5;

        public enum IncidentTrigger
        {
            RANDOM_EVENT, PLAYER_EVENT, LETTER_EVENT
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            // if the event is passively fired (randomly triggered by game engine, not 
            // actively by the player) don't fire if the random event option is disabled.
            if (!parms.forced && !CallATrader.settings.randomEventAllowed)
            {
                return false;
            }
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            // if the event target is not of type map we cannot exec the event 
            Map map = parms.target as Map;
            if (map == null)
            {
                return false;
            }

            IncidentTrigger trigger = parms.forced ? IncidentTrigger.PLAYER_EVENT : IncidentTrigger.RANDOM_EVENT;

            // create the correct type of letter
            return SendSelectTraderLetterOrRefusal(map, DefDatabase<TraderKindDef>.AllDefsListForReading, trigger);
        }

        public static bool SendSelectTraderLetterOrRefusal(Map map, IList<TraderKindDef> orbitalTraderDefs, IncidentTrigger eventTrigger)
        {
            // if there is no active comms console don't send a letter
            // (because logically colony should not be able to retrieve it)
            if (eventTrigger == IncidentTrigger.PLAYER_EVENT
                && CallATrader.settings.requireActiveCommsConosle
                && !AnyCommsConsoleActive(map))
            {
                // send info message so player knowns that there will be no response from the orbital traders hub
                Messages.Message((Constants.MOD_PREFIX + INCIDENT + "infomessage.noactivecommsconsole").
                    Translate(), null, MessageTypeDefOf.NeutralEvent, false);
                return false;
            }

            String displayableError = GetDisplayableLetterError(map, orbitalTraderDefs);
            Letter letter = null;

            // if error but from non-interactive event trigger - ignore silently to not confuse player with random refusal messages
            if (displayableError != null && eventTrigger != IncidentTrigger.RANDOM_EVENT)
            { 
                letter = LetterMaker.MakeLetter(
                    (Constants.MOD_PREFIX + INCIDENT + "refusal.label").Translate(),
                    (Constants.MOD_PREFIX + INCIDENT + "refusal.base").Translate() + "\n\n" + displayableError,
                    LetterDefOf.NeutralEvent);
            }
            
            // if no error and not triggered by a letter event already send the choice letter
            if (displayableError == null && eventTrigger != IncidentTrigger.LETTER_EVENT)
            {
                letter = new ChoiceLetter_SelectTrader(map);
            }

            if (letter != null)
            {
                Find.LetterStack.ReceiveLetter(letter);
                return true;
            }

            return false;
        }

        private static bool AnyCommsConsoleActive(Map map)
        {
            // if we don't know the def of the comms console we have an issue anyways
            ThingDef commsDef = DefDatabase<ThingDef>.GetNamed(CommsConsoleDef);
            if (commsDef == null)
            {
                return false;
            }
            // event can fire if there is at least one powered comms console in the colony
            if (!map.listerBuildings.ColonistsHaveBuildingWithPowerOn(commsDef))
            {
                return false;
            }
            return true;
        }

        private static String GetDisplayableLetterError(Map map, IList<TraderKindDef> orbitalTraderDefs)
        {
            // colony situation causing no orbital traders to be available
            if (!orbitalTraderDefs.Any(def => def.orbital))
            {
                return (Constants.MOD_PREFIX + INCIDENT + "refusal.noorbitaltradersavailable").Translate();
            }

            // check the limit of ships in the basegame
            if (map.passingShipManager.passingShips.Count >= HARDCODED_SHIP_LIMIT)
            {
                return (Constants.MOD_PREFIX + INCIDENT + "refusal.toomanytraders").Translate(HARDCODED_SHIP_LIMIT);
            }

            return null;
        }
    }
}
