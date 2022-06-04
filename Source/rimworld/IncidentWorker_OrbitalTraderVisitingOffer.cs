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

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return parms.forced || CallATrader.Settings.randomEventAllowed;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            TraderLetterConfig config = new TraderLetterConfig((Map)parms.target);
            Find.LetterStack.ReceiveLetter(new ChoiceLetter_SelectTrader(config));
            return true;
        }
    }
}
