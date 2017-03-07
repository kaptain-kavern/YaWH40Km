using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Object = UnityEngine.Object;
using System.Threading;
using RimWorld;
using RimWorld.Planet;
using Harmony;

namespace FactionColors
{
    public class ModInitializer : ITab
    {
        protected GameObject modInitializerControllerObject;

        public ModInitializer()
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                Log.Message("Initialized Faction Colors Mod");
                this.modInitializerControllerObject = new GameObject("FactionCol");
                this.modInitializerControllerObject.AddComponent<ModInitializerBehaviour>();
                this.modInitializerControllerObject.AddComponent<DoOnMainThread>();
                UnityEngine.Object.DontDestroyOnLoad(this.modInitializerControllerObject);
            }, "queueInject", false, null);
        }

        protected override void FillTab()
        {}
    }

    public class DoOnMainThread : MonoBehaviour
    {

        public static readonly Queue<Action> ExecuteOnMainThread = new Queue<Action>();

        public void Update()
        {
            while (ExecuteOnMainThread.Count > 0)
            {
                ExecuteOnMainThread.Dequeue().Invoke();
            }
        }
    }

    
    class ModInitializerBehaviour : MonoBehaviour
    {

     
        public void FixedUpdate()
        {
        }

        public void OnLevelWasLoaded()
        {
            WorldObjectsHolder holder = Find.World.worldObjects;
            if (holder != null)
            {
                //         Log.Message("FoundHolder");
                if (!holder.AllWorldObjects.Any(x => x.def == FactionColorsDefOf.PlayerFactionStoryTracker))
                {
                    //           Log.Message("Adding World Ojbect");
                    PlayerFactionStoryTracker Tracker = new PlayerFactionStoryTracker();
                    Tracker.def = FactionColorsDefOf.PlayerFactionStoryTracker;
                    Tracker.ID = -2;
                    Tracker.Tile = 1;
                    holder.Add(Tracker);
                }
            }

        }

        public void Start()
        {
            Log.Message("Initiated FactionColors Detours.");
//            MethodInfo method1a = typeof(Verse.PawnGraphicSet).GetMethod("ResolveApparelGraphics", BindingFlags.Instance | BindingFlags.Public);
//            MethodInfo method1b = typeof(FactionColors.ApparelGraphicSet).GetMethod("ResolveApparelGraphicsOriginal", BindingFlags.Instance | BindingFlags.Public);

//            MethodInfo method2a = typeof(Verse.PawnRenderer).GetMethod("DrawEquipmentAiming", BindingFlags.Instance | BindingFlags.Public);
//            MethodInfo method2b = typeof(FactionColors.FactionItemRenderer).GetMethod("DrawEquipmentAimingModded", BindingFlags.Instance | BindingFlags.Public);

            try
            {
                //            Detours.TryDetourFromTo(method1a, method1b);
                //            Detours.TryDetourFromTo(method2a, method2b);
                //            Detours.TryDetourFromTo(method4a, method4b);

                Log.Message("ResolveApparelGraphics method detoured!");
            }
            catch (Exception)
            {
                Log.Error("Could not detour Faction graphics");
                throw;
            }
        }
    }
}
