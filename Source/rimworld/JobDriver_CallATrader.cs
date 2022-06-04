using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Multiplayer.API;

namespace Arakos.CallATrader
{

    public class JobDriver_CallATrader : JobDriver, ISynchronizable
    {

        int workDone = 0;

        public void Sync(SyncWorker sync)
        {
            sync.Bind(ref this.workDone);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref workDone, "workDone", 0);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Building_CommsConsole commsConsole = this.job.targetA.Thing as Building_CommsConsole;
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A)
                .FailOn(() => !commsConsole.CanUseCommsNow)
                .FailOn(() => CallATrader.State.TraderRequestActionDisabledUntil > Find.TickManager.TicksAbs);

            Toil talkToTrader = new Toil
            {
                defaultDuration = 200,
                defaultCompleteMode = ToilCompleteMode.Never
            };

            talkToTrader.WithProgressBar(TargetIndex.A, () => (float)this.workDone / (float)talkToTrader.defaultDuration);
            talkToTrader.AddEndCondition(() => this.workDone < talkToTrader.defaultDuration ? JobCondition.Ongoing : JobCondition.Succeeded);
            talkToTrader.tickAction = () => 
            {
                this.workDone++;
                if (this.workDone < talkToTrader.defaultDuration)
                {
                    return;
                }

                // call finished so disable the action to call again for a set amount of time
                int cooldown = CallATrader.Settings.cooldownRange.RandomInRange * GenDate.TicksPerDay;
                CallATrader.State.TraderRequestActionDisabledUntil = Find.TickManager.TicksAbs + cooldown;

                // queue the 'trader visiting offer' incident
                Find.Storyteller.incidentQueue.Add(
                    new QueuedIncident(
                        new FiringIncident(DefDatabase<IncidentDef>.GetNamed(Constants.INCIDENT_DEF_NAME), null, new IncidentParms() { target = base.Map, forced = true }),
                        Find.TickManager.TicksGame + Rand.Range(1000, 6000))
                    );

                // send info message so player knowns something is about to happen
                Messages.Message((Constants.MOD_PREFIX + ".job.infomessage.success").Translate(GenDate.ToStringTicksToPeriod(cooldown, allowSeconds: false, canUseDecimals: false)),
                    commsConsole, MessageTypeDefOf.NeutralEvent, false);
            };

            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            yield return talkToTrader;
            yield return Toils_Reserve.Release(TargetIndex.A);
            yield break;
        }

    }
}
