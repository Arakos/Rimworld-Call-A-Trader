using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Arakos.CallATrader
{
    public class IncidentWorker_OrbitalTraderVisitingOffer : IncidentWorker
    {

        private static readonly String INCIDENT = ".incident.";

        private static readonly int HARDCODED_SHIP_LIMIT = 5;

        public enum IncidentTrigger
        {
            RANDOM_EVENT, PLAYER_EVENT, LETTER_EVENT
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return parms.forced || CallATrader.settings.randomEventAllowed;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            return TryExecuteWorkerInternal((Map)parms.target,
                DefDatabase<TraderKindDef>.AllDefsListForReading,
                parms.forced ? IncidentTrigger.PLAYER_EVENT : IncidentTrigger.RANDOM_EVENT);
        }

        public static bool TryExecuteWorkerInternal(Map map, IList<TraderKindDef> orbitalTraderDefs, IncidentTrigger eventTrigger)
        {
            String displayableError = GetDisplayableError(map, orbitalTraderDefs);
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

        public static String GetDisplayableError(Map map, IList<TraderKindDef> orbitalTraderDefs)
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
