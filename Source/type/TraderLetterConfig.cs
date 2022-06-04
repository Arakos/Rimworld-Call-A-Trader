using Multiplayer.API;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace Arakos.CallATrader

{
    public class TraderLetterConfig : IExposable, ISynchronizable
    {

        public Map map;

        public int fee;

        public int delay;

        public List<TraderKindDef> orbitalTraderDefs;

        public TraderLetterConfig() { }

        public TraderLetterConfig(Map map)
        {
            this.map = map;
            this.fee = (CallATrader.Settings.costRange.RandomInRange / 10) * 10;
            this.delay = CallATrader.Settings.delayRange.RandomInRange * GenDate.TicksPerDay;
            List<TraderKindDef> orbitalTraderDefs = DefDatabase<TraderKindDef>.AllDefsListForReading.FindAll(def => def.orbital);

            if (!CallATrader.Settings.canSelectTraderType)
            {
                TraderKindDef tmp = orbitalTraderDefs.RandomElement();
                orbitalTraderDefs.RemoveAll(def => def != tmp);
            }
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref this.map, "map");
            Scribe_Collections.Look(ref this.orbitalTraderDefs, "selectableTraderDefs");
            Scribe_Values.Look(ref this.fee, "fee");
            Scribe_Values.Look(ref this.delay, "delay");
        }

        public void Sync(SyncWorker sync)
        {
            sync.Bind(ref this.orbitalTraderDefs);
            sync.Bind(ref this.map);
            sync.Bind(ref this.fee);
            sync.Bind(ref this.delay);
        }
    }
}
