using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Arakos.CallATrader
{
    public class ChoiceLetter_SelectTrader : ChoiceLetter
    {
        static readonly string TRADER_LETTER = ".traderletter.";

        private Map map;

        private int fee;

        private int delay;

        private bool canSelectTraderType;

        public ChoiceLetter_SelectTrader()
        {
        }

        public ChoiceLetter_SelectTrader(Map map)
        {
            base.def = DefDatabase<LetterDef>.GetNamed(Constants.LETTER_DEF_NAME);
            base.ID = Find.UniqueIDsManager.GetNextLetterID();

            this.map = map;
            this.fee = (CallATrader.settings.costRange.RandomInRange / 10) * 10;
            this.delay = CallATrader.settings.delayRange.RandomInRange * GenDate.TicksPerDay;
            this.canSelectTraderType = CallATrader.settings.canSelectTraderType;

            base.Label = (Constants.MOD_PREFIX + TRADER_LETTER + "label").Translate();
            base.Text = (Constants.MOD_PREFIX + TRADER_LETTER + "text").Translate(GenDate.ToStringTicksToPeriod(delay, allowSeconds: false, canUseDecimals: false), fee);

            if (CallATrader.settings.timeoutActive)
            {
                int letterTimeout = CallATrader.settings.timoutRange.RandomInRange * GenDate.TicksPerHour;
                base.Text = base.Text + "\n\n"
                    + (Constants.MOD_PREFIX + TRADER_LETTER + "timeout").Translate(GenDate.ToStringTicksToPeriod(letterTimeout, allowSeconds: false, canUseDecimals: false));
                StartTimeout(letterTimeout);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref map, "map");
            Scribe_Values.Look(ref fee, "fee");
            Scribe_Values.Look(ref delay, "delay");
            Scribe_Values.Look(ref canSelectTraderType, "canSelectTraderType");
        }


        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                // letter is opend from letter archive by player just display the default close option
                if (base.ArchivedOnly)
                {
                    yield return base.Option_Close;
                    yield break;
                }

                IList<TraderKindDef> orbitalTraderDefs = DefDatabase<TraderKindDef>.AllDefsListForReading.FindAll(def => def.orbital);

                // check game conditions still allow triggering the offer event now to determine if trader selection can continue
                // if not a refusal notice will be send here as well
                if (IncidentWorker_OrbitalTraderVisitingOffer.TryExecuteWorkerInternal(
                    this.map, orbitalTraderDefs, IncidentWorker_OrbitalTraderVisitingOffer.IncidentTrigger.LETTER_EVENT))
                {
                    yield return CreateDiaOption((Constants.MOD_PREFIX + TRADER_LETTER + "error.ok").Translate());
                    yield break;
                }

                if (!this.canSelectTraderType)
                {
                    orbitalTraderDefs = new TraderKindDef[] { orbitalTraderDefs.RandomElement() };
                }

                foreach (TraderKindDef traderKindDef in orbitalTraderDefs)
                {
                    DiaOption payForTrader = CreateDiaOption((Constants.MOD_PREFIX + TRADER_LETTER + "accept").Translate(traderKindDef.label, fee));
                    payForTrader.action = () =>
                    {
                        Find.LetterStack.RemoveLetter(this);

                        Find.Storyteller.incidentQueue.Add(new QueuedIncident(
                            new FiringIncident(IncidentDefOf.OrbitalTraderArrival, null,
                                new IncidentParms()
                                {
                                    target = map,
                                    traderKind = traderKindDef
                                }),
                            Find.TickManager.TicksGame + delay));

                        if (fee > 0)
                        {
                            TradeUtility.LaunchSilver(map, fee);
                            Messages.Message((Constants.MOD_PREFIX + TRADER_LETTER + "payed").Translate(fee), null, MessageTypeDefOf.NeutralEvent, true);
                        }

                    };

                    int availableSilver = TradeUtility.AllLaunchableThingsForTrade(map)
                        .Where(t => t.def == ThingDefOf.Silver)
                        .Sum(t => t.stackCount);
                    if (availableSilver < fee)
                    {
                        payForTrader.Disable((Constants.MOD_PREFIX + TRADER_LETTER + "disabled").Translate(availableSilver, fee));
                    }

                    yield return payForTrader;
                }

                yield return CreateDiaOption((Constants.MOD_PREFIX + TRADER_LETTER + "refuse").Translate());
                yield return base.Option_Postpone;
                yield break;
            }
        }

        private DiaOption CreateDiaOption(TaggedString label)
        {
            return new DiaOption(label)
            {
                action = () => Find.LetterStack.RemoveLetter(this),
                resolveTree = true
            };
        }

    }
}
