using RimWorld;
using System.Reflection;
using UnityEngine;
using Verse;

namespace Arakos.CallATrader
{

    public class CallATrader : Mod
    {

        private static CallATrader instance;

        public static CallATraderSettings settings
        {
            get { return instance.GetSettings<CallATraderSettings>(); }
            private set { }
        }
        public static CallATraderState state
        {
            get { return CallATraderState.getInstance(); }
            private set { }
        }

        public CallATrader(ModContentPack pack) : base(pack)
        {
            instance = this;
            var harmony = new HarmonyLib.Harmony("arakos.callatrader.acat.callatrader");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoSettingsWindowContents(inRect);
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return Constants.MODNAME.Translate();
        }

    }

}