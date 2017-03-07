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
                Log.Message("FoundHolder");
                if (!holder.AllWorldObjects.Any(x => x.def == CorruptionDefOfs.CorruptionStoryTracker))
                {
                    Log.Message("Adding World Ojbect");
                    CorruptionStoryTracker corrTracker = new CorruptionStoryTracker();
                    corrTracker.def = CorruptionDefOfs.CorruptionStoryTracker;
                    corrTracker.Tile = 0;
                    holder.Add(corrTracker);
                    corrTracker.PostAdd();
                }
            }

        }
        
        public void Start()
        {
        }        
    }
}
