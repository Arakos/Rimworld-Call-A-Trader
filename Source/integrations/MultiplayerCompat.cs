using System;
using Multiplayer.API;
using Verse;

namespace Arakos.CallATrader
{
    /*
     * enables multiplayer compatability.
     * see https://hackmd.io/Gd_gueokTNui_fqzOSG2Tg#A-Small-Example
     */

    [StaticConstructorOnStartup]
    public static class MultiplayerCompat
    {
        static MultiplayerCompat()
        {
            if (!MP.enabled) return;

            // This is where the magic happens and your attributes
            // auto register, similar to Harmony's PatchAll.
            MP.RegisterAll();

            // You can choose to not auto register and do it manually
            // with the MP.Register* methods.

            // Use MP.IsInMultiplayer to act upon it in other places
            // user can have it enabled and not be in session
        }
    }
}
