using System;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Injector40K;

namespace Corruption
{
    public class ModInitializer : ITab
    {
        protected GameObject modInitializerControllerObject;
        

        public ModInitializer()
        {
            Log.Message("Initialized 40k Corruption Mod");
            this.modInitializerControllerObject = new GameObject("Corruptoid");
            this.modInitializerControllerObject.AddComponent<ModInitializerBehaviour>();
            UnityEngine.Object.DontDestroyOnLoad(this.modInitializerControllerObject);
        }

        protected override void FillTab()
        { }

        public override void TabTick()
        {
            Log.Message("TryingTOStart");
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
