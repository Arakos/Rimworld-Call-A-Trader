﻿using System;
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

        private int _traderRequestActionDisabledUntil = 0;
        public int TraderRequestActionDisabledUntil
        {
            get {
                int cooldownMax = Find.TickManager.TicksAbs + (CallATrader.Settings.cooldownRange.max * 60000);
                if (_traderRequestActionDisabledUntil >= cooldownMax)
                {
                    _traderRequestActionDisabledUntil = cooldownMax;
                }
                return _traderRequestActionDisabledUntil;
            }
            set
            {
                _traderRequestActionDisabledUntil = value;
            }
        }

        public CallATraderState(Game _)
        {
            instance = this;
        }
        public static CallATraderState GetInstance()
        {
            return instance;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref _traderRequestActionDisabledUntil, "traderRequestActionDisabledUntil");
        }

    }
}
