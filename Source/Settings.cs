using RimWorld;
using System;
using System.IO;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Arakos.CallATrader
{

    public class CallATraderSettings : ModSettings
    {
        // between 0 and max 2000 silver for a trader is configurable
        private static readonly IntRange boundsCostRange = new IntRange(0, 2000);
        // between 0 and 15 days (one quadrum) delay for tradeship events is configurabe
        private static readonly IntRange boundsDelayRange = new IntRange(0, 15);
        // between 0 and 60 day (one year) delay for tradeship events is configurabe
        private static readonly IntRange boundsCooldownRange = new IntRange(0, 60);

        public IntRange costRange = new IntRange(400, 600);
        public IntRange delayRange = new IntRange(2, 5);
        public IntRange cooldownRange = new IntRange(10, 30);
        public bool canSelectTraderType = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref costRange, Constants.COSTS_RANGE, new IntRange(400, 600), true);
            Scribe_Values.Look(ref delayRange, Constants.DELAY_RANGE, new IntRange(2, 5), true);
            Scribe_Values.Look(ref cooldownRange, Constants.COOLDOWN_RANGE, new IntRange(10, 30), true);
            Scribe_Values.Look(ref canSelectTraderType, Constants.CAN_SELECT_KIND, true, true);
        }

        public void DoSettingsWindowContents(Rect rect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(rect);
            list.CheckboxLabeled(Constants.CAN_SELECT_KIND.Translate(), ref canSelectTraderType, (Constants.CAN_SELECT_KIND + Constants.DESCRIPTION).Translate());
            list.Label(Constants.COSTS_RANGE.Translate(), -1, (Constants.COSTS_RANGE + Constants.DESCRIPTION).Translate());
            list.IntRange(ref costRange, boundsCostRange.min, boundsCostRange.max);
            list.Label(Constants.DELAY_RANGE.Translate(), -1, (Constants.DELAY_RANGE + Constants.DESCRIPTION).Translate());
            list.IntRange(ref delayRange, boundsDelayRange.min, boundsDelayRange.max);
            list.Label(Constants.COOLDOWN_RANGE.Translate(), -1, (Constants.COOLDOWN_RANGE + Constants.DESCRIPTION).Translate());
            list.IntRange(ref cooldownRange, boundsCooldownRange.min, boundsCooldownRange.max);
            list.End();
        }

    }
}
