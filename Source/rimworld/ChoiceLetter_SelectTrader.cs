using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Arakos.CallATrader
{
    public class ChoiceLetter_SelectTrader : ChoiceLetter
    {
        static readonly string TRADER_LETTER = ".traderletter.";

        private TraderLetterConfig config;

        public ChoiceLetter_SelectTrader()
        {
        }

        public ChoiceLetter_SelectTrader(TraderLetterConfig config)
        {
            this.config = config;

            base.def = DefDatabase<LetterDef>.GetNamed(Constants.LETTER_DEF_NAME);
            base.ID = Find.UniqueIDsManager.GetNextLetterID();
            base.label = (Constants.MOD_PREFIX + TRADER_LETTER + "label").Translate();
            base.text = (Constants.MOD_PREFIX + TRADER_LETTER + "text").Translate(GenDate.ToStringTicksToPeriod(config.delay, allowSeconds: false, canUseDecimals: false), config.fee);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref config, "config");
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

                foreach (TraderKindDef traderKindDef in config.orbitalTraderDefs)
                {
                    DiaOption payForTrader = new DiaOption((Constants.MOD_PREFIX + TRADER_LETTER + "accept").Translate(traderKindDef.label, config.fee))
                    {
                        action = () =>
                        {
                            Find.LetterStack.RemoveLetter(this);

                        // hardcoded limit of 5 in the basegame
                        if (config.map.passingShipManager.passingShips.Count >= 5)
                            {
                            // send info message so player knowns too many ships present already
                            Messages.Message((Constants.MOD_PREFIX + TRADER_LETTER + "toomanytraders").Translate(), null, MessageTypeDefOf.NeutralEvent, true);
                                return;
                            }

                            Find.Storyteller.incidentQueue.Add(new QueuedIncident(
                                new FiringIncident(IncidentDefOf.OrbitalTraderArrival, null,
                                    new IncidentParms()
                                    {
                                        target = config.map,
                                        traderKind = traderKindDef
                                    }),
                                Find.TickManager.TicksGame + config.delay));

                            if (config.fee > 0)
                            {
                                TradeUtility.LaunchSilver(config.map, config.fee);
                                Messages.Message((Constants.MOD_PREFIX + TRADER_LETTER + "payed").Translate(config.fee), null, MessageTypeDefOf.NeutralEvent, true);
                            }

                        }
                    };
                    int availableSilver = TradeUtility.AllLaunchableThingsForTrade(config.map)
                        .Where(t => t.def == ThingDefOf.Silver)
                        .Sum(t => t.stackCount);
                    if (availableSilver < config.fee)
                    {
                        payForTrader.Disable((Constants.MOD_PREFIX + TRADER_LETTER + "disabled").Translate(availableSilver, config.fee));
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
