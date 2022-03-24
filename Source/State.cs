using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Arakos.CallATrader
{
    public class CallATraderState : GameComponent
    {
        private static CallATraderState instance;

        public int traderRequestActionDisabledUntil = 0;

        public CallATraderState(Game game)
        {
            instance = this;
        }
        public static CallATraderState getInstance()
        {
            return instance;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref traderRequestActionDisabledUntil, "traderRequestActionDisabledUntil");
        }

    }
}
