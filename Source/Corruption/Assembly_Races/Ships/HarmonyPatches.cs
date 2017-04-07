using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using Verse;
using RimWorld.Planet;
using RimWorld;
using System.Reflection;

namespace Corruption.Ships
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.ohu.ships.main");

            harmony.Patch(AccessTools.Method(typeof(RimWorld.FactionGenerator), "GenerateFactionsIntoWorld", new Type[] { typeof(string) }), null, new HarmonyMethod(typeof(HarmonyPatches), "GenerateFactionsIntoWorldPostFix"));

            harmony.Patch(AccessTools.Property(typeof(MapPawns), "AnyColonistTameAnimalOrPrisonerOfColony").GetGetMethod(false), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(AnyColonistTameAnimalOrPrisonerOfColonyPreFix)), null);
            
            harmony.Patch(AccessTools.Method(typeof(RimWorld.Planet.WorldSelector), "AutoOrderToTileNow", new Type[] { typeof(Caravan), typeof(int) }), new HarmonyMethod(typeof(HarmonyPatches), "AutoOrderToTileNowPrefix"), null);
        }

        public static void AnyColonistTameAnimalOrPrisonerOfColonyPreFix(ref bool __result, MapPawns __instance)
        {
            if (!__result)
            {
                Map map = Traverse.Create(__instance).Field("map").GetValue<Map>();
                if (map != null)
                {
                    List<Thing> list = map.listerThings.AllThings.FindAll(x => x is ShipBase_Traveling || x is ShipBase);
                    if (list.Count > 0)
                    {
                        __result = true;
                    }
      //              for (int i = 0; i < list.Count; i++)

                }
            }
        }        

        public static void GenerateFactionsIntoWorldPostFix()
        {
            Log.Message("GeneratingShipTracker");
            ShipTracker shipTracker = (ShipTracker)WorldObjectMaker.MakeWorldObject(ShipNamespaceDefOfs.ShipTracker);
            shipTracker.Tile = TileFinder.RandomStartingTile();
            Find.WorldObjects.Add(shipTracker);
        }

        public static bool AutoOrderToTileNowPrefix(Caravan c, int tile)
        {
            LandedShip ship = c as LandedShip;
            if (ship != null)
            {
                return false;
            }
            return true;
        }
    }
}
