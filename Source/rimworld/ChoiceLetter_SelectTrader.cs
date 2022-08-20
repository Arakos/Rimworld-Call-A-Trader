using RimWorld;
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

            base.label = (Constants.MOD_PREFIX + TRADER_LETTER + "label").Translate();
            base.text = (Constants.MOD_PREFIX + TRADER_LETTER + "text").Translate(GenDate.ToStringTicksToPeriod(delay, allowSeconds: false, canUseDecimals: false), fee);
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

                        // hardcoded limit of 5 in the basegame
                        if (this.map.passingShipManager.passingShips.Count >= 5)
                        {
                            // send info message so player knowns too many ships present already
                            Messages.Message((Constants.MOD_PREFIX + TRADER_LETTER + "toomanytraders").Translate(), null, MessageTypeDefOf.NeutralEvent, true);
                            return;
                        }

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
