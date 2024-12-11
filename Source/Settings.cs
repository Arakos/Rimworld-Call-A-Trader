using RimWorld;
using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Arakos.CallATrader
{

    public class CallATraderSettings : ModSettings
    {
        // between 0 and 15 days (one quadrum) delay for tradeship events is configurabe
        private static readonly IntRange boundsDelayRange = new IntRange(0, 15);
        // between 0 and 60 day (one year) delay for tradeship events is configurabe
        private static readonly IntRange boundsCooldownRange = new IntRange(0, 60);
        // between 1 and 72h timout for an offer
        private static readonly IntRange boundsTimeoutRange = new IntRange(1, 72);


        public IntRange absolutCostRange = new IntRange(400, 800);
        public FloatRange relativeCostRange = new FloatRange(0.5f,2f);
        public IntRange delayRange = new IntRange(2, 5);
        public IntRange cooldownRange = new IntRange(10, 30);
        public IntRange timoutRange = new IntRange(12, 42);
        public bool timeoutActive = true;
        public bool canSelectTraderType = true;
        public bool randomEventAllowed = true;
        public bool requireActiveCommsConosle = true;
        public bool considerColonyWealth = false;

        private string minBuff = "";
        private string maxBuff = "";
        private bool wealthToggled = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref absolutCostRange, Constants.COSTS_RANGE_ABS, absolutCostRange, true);
            Scribe_Values.Look(ref relativeCostRange, Constants.COSTS_RANGE_REL, relativeCostRange, true);
            Scribe_Values.Look(ref delayRange, Constants.DELAY_RANGE, delayRange, true);
            Scribe_Values.Look(ref cooldownRange, Constants.COOLDOWN_RANGE, cooldownRange, true);
            Scribe_Values.Look(ref timoutRange, Constants.TIMEOUT_RANGE, timoutRange, true);
            Scribe_Values.Look(ref timeoutActive, Constants.TIMEOUT_ACTIVE, timeoutActive, true);
            Scribe_Values.Look(ref canSelectTraderType, Constants.CAN_SELECT_KIND, canSelectTraderType, true);
            Scribe_Values.Look(ref requireActiveCommsConosle, Constants.REQUIRE_ACTIVE_COMMS_CONSOLE, requireActiveCommsConosle, true);
            Scribe_Values.Look(ref randomEventAllowed, Constants.RANDOM_EVENT_ALLOWED, randomEventAllowed, true);
            Scribe_Values.Look(ref considerColonyWealth, Constants.CONSIDER_COLONY_WEALTH, considerColonyWealth, true);
            minBuff = considerColonyWealth ? relativeCostRange.min.ToString() : absolutCostRange.min.ToString();
            maxBuff = considerColonyWealth ? relativeCostRange.max.ToString() : absolutCostRange.max.ToString();
            wealthToggled = true;
        }

        public void DoSettingsWindowContents(Rect rect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(rect);

            list.CheckboxLabeled(Constants.RANDOM_EVENT_ALLOWED.Translate(), ref randomEventAllowed, (Constants.RANDOM_EVENT_ALLOWED + Constants.DESCRIPTION).Translate());
            list.CheckboxLabeled(Constants.REQUIRE_ACTIVE_COMMS_CONSOLE.Translate(), ref requireActiveCommsConosle, (Constants.REQUIRE_ACTIVE_COMMS_CONSOLE + Constants.DESCRIPTION).Translate());
            list.CheckboxLabeled(Constants.CAN_SELECT_KIND.Translate(), ref canSelectTraderType, (Constants.CAN_SELECT_KIND + Constants.DESCRIPTION).Translate());
            list.CheckboxLabeled(Constants.TIMEOUT_ACTIVE.Translate(), ref timeoutActive, (Constants.TIMEOUT_ACTIVE + Constants.DESCRIPTION).Translate());

            wealthToggled = considerColonyWealth;
            list.CheckboxLabeled(Constants.CONSIDER_COLONY_WEALTH.Translate(), ref considerColonyWealth, (Constants.CONSIDER_COLONY_WEALTH + Constants.DESCRIPTION).Translate());
            wealthToggled = wealthToggled != considerColonyWealth;

            list.GapLine();
            doCostSettingsSection(list.GetRect(Text.LineHeight));
            list.GapLine();

            list.Label(Constants.DELAY_RANGE.Translate(), tooltip: (Constants.DELAY_RANGE + Constants.DESCRIPTION).Translate());
            list.IntRange(ref delayRange, boundsDelayRange.min, boundsDelayRange.max);

            list.Label(Constants.COOLDOWN_RANGE.Translate(), tooltip: (Constants.COOLDOWN_RANGE + Constants.DESCRIPTION).Translate());
            list.IntRange(ref cooldownRange, boundsCooldownRange.min, boundsCooldownRange.max);

            if (timeoutActive)
            {
                list.Label(Constants.TIMEOUT_RANGE.Translate(), tooltip: (Constants.TIMEOUT_RANGE + Constants.DESCRIPTION).Translate());
                list.IntRange(ref timoutRange, boundsTimeoutRange.min, boundsTimeoutRange.max);
            }

            list.End();
        }

        private void doCostSettingsSection(Rect r)
        {
            string label = Constants.COSTS_RANGE_ABS.Translate();
            string tooltip = (Constants.COSTS_RANGE_ABS + Constants.DESCRIPTION).Translate();
            if (wealthToggled)
            {
                minBuff = absolutCostRange.min.ToString();
                maxBuff = absolutCostRange.max.ToString();
            }

            if (considerColonyWealth)
            {
                label = Constants.COSTS_RANGE_REL.Translate();
                tooltip = (Constants.COSTS_RANGE_REL + Constants.DESCRIPTION).Translate();
                if (wealthToggled)
                {
                    minBuff = relativeCostRange.min.ToString();
                    maxBuff = relativeCostRange.max.ToString();
                }
            }

            float width = r.width / 3f;

            Rect labelRect = new Rect(r.x, r.y, width, r.height);
            Widgets.Label(labelRect, label);
            TooltipHandler.TipRegion(labelRect, tooltip);

            Rect minRect = new Rect(r.x + width, r.y, width, r.height);
            string oldBuf = minBuff;
            minBuff = Widgets.TextField(minRect, minBuff);
            bool minInputChanged = oldBuf != minBuff;

            Rect maxRect = new Rect(r.x + 2 * width, r.y, width, r.height);
            maxBuff = Widgets.TextField(maxRect, maxBuff);


            if (considerColonyWealth)
            {
                if (!float.TryParse(minBuff, out float min))
                {
                    min = relativeCostRange.min;
                }
                if (!float.TryParse(maxBuff, out float max))
                {
                     max = relativeCostRange.max;
                }
                if (min < 0)
                {
                    min = 0;
                    minBuff = "0";
                }
                if (min > max)
                {
                    if (minInputChanged)
                    {
                        max = min;
                        maxBuff = minBuff;
                    }
                    else
                    {
                        min = max;
                        minBuff = maxBuff;
                    }
                }
                relativeCostRange.min = min;
                relativeCostRange.max = max;
            }
            else
            {
                if (!int.TryParse(minBuff, out int min))
                {
                    min = absolutCostRange.min;
                }
                if (!int.TryParse(maxBuff, out int max))
                {
                    max = absolutCostRange.max;
                }
                if (min < 0)
                {
                    min = 0;
                    minBuff = "0";
                }
                if (min > max)
                {
                    if (minInputChanged)
                    {
                        max = min;
                        maxBuff = minBuff;
                    }
                    else
                    {
                        min = max;
                        minBuff = maxBuff;
                    }
                }
                absolutCostRange.min = min;
                absolutCostRange.max = max;
            }

        }
    }
}
