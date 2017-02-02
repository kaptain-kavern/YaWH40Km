using System;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Injector40K;
using System.Collections.Generic;
using RimWorld.Planet;

namespace Corruption
{
    public class ModInitializer : ITab
    {
        protected GameObject modInitializerControllerObject;
        

        public ModInitializer()
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                Log.Message("Initialized 40k Corruption Mod");
                this.modInitializerControllerObject = new GameObject("Corruptoid");
                this.modInitializerControllerObject.AddComponent<ModInitializerBehaviour>();
                this.modInitializerControllerObject.AddComponent<DoOnMainThread>();
                UnityEngine.Object.DontDestroyOnLoad(this.modInitializerControllerObject);
            }, "queueInject", false, null);
        }

        protected override void FillTab()
        { }

        public override void TabTick()
        {
           // Log.Message("TryingTOStart");
        }
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
                if (!holder.AllWorldObjects.Any(x => x.def == CorruptionDefOfs.CorruptionStoryTracker))
                {
         //           Log.Message("Adding World Ojbect");
                    CorruptionStoryTracker corrTracker = new CorruptionStoryTracker();
                    corrTracker.def = CorruptionDefOfs.CorruptionStoryTracker;
                    corrTracker.Tile = 0;
                    holder.Add(corrTracker);
                }
            }

        }
        
        public void Start()
        {
            Log.Message("Initiated Corruption Detours.");
            MethodInfo method1a = typeof(RimWorld.ThoughtHandler).GetMethod("CanGetThought", new Type[] { typeof(ThoughtDef) });
            MethodInfo method1b = typeof(Corruption.SituationalThoughtHandlerModded).GetMethod("CanGetThought", new Type[] { typeof(ThoughtDef) });

            MethodInfo method2a = typeof(RimWorld.MainTabWindow_Inspect).GetMethod("DoInspectPaneButtons", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo method2b = typeof(Corruption.MainTabWindow_InspectModded).GetMethod("DoInspectPaneButtons", BindingFlags.Public | BindingFlags.Instance);


            MethodInfo method3a = typeof(RimWorld.FloatMenuMakerMap).GetMethod("AddUndraftedOrders", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo method3b = typeof(Corruption.Building_MechanicusMedTable).GetMethod("AddUndraftedOrders", BindingFlags.NonPublic | BindingFlags.Static);

            try
            {
                Detours.TryDetourFromTo(method1a, method1b);
                Detours.TryDetourFromTo(method2a, method2b);
           //     Detours.TryDetourFromTo(method3a, method3b);


                Log.Message("Corruption methods detoured!");
            }
            catch (Exception)
            {
                Log.Error("Could not detour thoughts");
                throw;
            }

        }        
    }
}
