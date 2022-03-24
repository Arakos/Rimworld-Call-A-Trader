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

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Find.LetterStack.ReceiveLetter(new ChoiceLetter_SelectTrader((Map)parms.target));
            return true;
        }
    }
}
