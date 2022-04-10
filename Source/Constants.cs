using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Arakos.CallATrader
{
    public static class Constants
    {

        internal static readonly string MOD_PREFIX = "acat";
        internal static readonly string DESCRIPTION = ".description";

        internal static readonly string MODNAME = MOD_PREFIX + ".modname";
        internal static readonly string COSTS_RANGE = MOD_PREFIX + ".costsrange";
        internal static readonly string DELAY_RANGE = MOD_PREFIX + ".delayrange";
        internal static readonly string COOLDOWN_RANGE = MOD_PREFIX + ".cooldownrange";
        internal static readonly string CAN_SELECT_KIND = MOD_PREFIX + ".canselectkind";
        internal static readonly string RANDOM_EVENT_ALLOWED = MOD_PREFIX + ".randomeventallowed";

        internal static readonly string JOB_DEF_NAME = MOD_PREFIX + "_CallTraderJob";
        internal static readonly string LETTER_DEF_NAME = MOD_PREFIX + "_SelectTraderLetter";
        internal static readonly string INCIDENT_DEF_NAME = MOD_PREFIX + "_OrbitalTraderVisitingOffer";
    }

    [StaticConstructorOnStartup()]
    public static class Textures
    {
        public static readonly Texture2D ORBITAL_TRADER_HUB_ICON = ContentFinder<Texture2D>.Get("orbital_trader_hub_icon", true);
    }
}
