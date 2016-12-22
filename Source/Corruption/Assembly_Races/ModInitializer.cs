using System;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Injector40K;
using System.Collections.Generic;

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
        }
        
        public void Start()
        {
            Log.Message("Initiated Corruption Detours.");
            MethodInfo method3 = typeof(RimWorld.ThoughtHandler).GetMethod("CanGetThought", new Type[] { typeof(ThoughtDef) });
            MethodInfo method4 = typeof(Corruption.SituationalThoughtHandlerModded).GetMethod("CanGetThought", new Type[] { typeof(ThoughtDef) });
            
            try
            {
                if (method3 == null) Log.Message("No Ori");
                if (method4 == null) Log.Message("No Src");
                Detours.TryDetourFromTo(method3, method4);

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
