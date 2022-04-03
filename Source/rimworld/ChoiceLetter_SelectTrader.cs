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
                if (base.ArchivedOnly)
                {
                    yield return base.Option_Close;
                    yield break;
                }

                List<TraderKindDef> orbitalTraderDefs = DefDatabase<TraderKindDef>.AllDefsListForReading.FindAll(def => def.orbital);

                if (!this.canSelectTraderType)
                {
                    TraderKindDef tmp = orbitalTraderDefs.RandomElement();
                    orbitalTraderDefs.RemoveAll(def => def != tmp);
                }

                foreach (TraderKindDef traderKindDef in orbitalTraderDefs)
                {
                    DiaOption payForTrader = new DiaOption((Constants.MOD_PREFIX + TRADER_LETTER + "accept").Translate(traderKindDef.label, fee));
                    payForTrader.action = () =>
                    {
                        Find.Storyteller.incidentQueue.Add(new QueuedIncident(
                            new FiringIncident(IncidentDefOf.OrbitalTraderArrival, null,
                                new IncidentParms()
                                {
                                    target = map,
                                    traderKind = traderKindDef
                                }),
                            Find.TickManager.TicksGame + delay));

                        TradeUtility.LaunchSilver(map, fee);
                        Messages.Message((Constants.MOD_PREFIX + TRADER_LETTER + "payed").Translate(fee), null, MessageTypeDefOf.NeutralEvent, true);

                        Find.LetterStack.RemoveLetter(this);
                    };
                    int availableSilver = TradeUtility.AllLaunchableThingsForTrade(map)
                        .Where(t => t.def == ThingDefOf.Silver)
                        .Sum(t => t.stackCount);
                    if (availableSilver < fee)
                    {
                        payForTrader.Disable((Constants.MOD_PREFIX + TRADER_LETTER + "disabled").Translate(availableSilver, fee));
                    }
                    payForTrader.resolveTree = true;
                    yield return payForTrader;
                }

                yield return new DiaOption((Constants.MOD_PREFIX + TRADER_LETTER + "refuse").Translate())
                {
                    action = () => Find.LetterStack.RemoveLetter(this),
                    resolveTree = true
                };

                yield break;
            }
        }
    }
}
