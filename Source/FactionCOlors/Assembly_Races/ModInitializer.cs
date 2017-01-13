using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Injector40K;
using UnityEngine;
using Verse;
using Object = UnityEngine.Object;
using System.Threading;
using RimWorld;

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
        }

        public void Start()
        {
            Log.Message("Initiated FactionColors Detours.");
            MethodInfo method1a = typeof(Verse.PawnGraphicSet).GetMethod("ResolveApparelGraphics", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo method1b = typeof(FactionColors.ApparelGraphicSet).GetMethod("ResolveApparelGraphicsOriginal", BindingFlags.Instance | BindingFlags.Public);

            MethodInfo method2a = typeof(Verse.PawnRenderer).GetMethod("DrawEquipmentAiming", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo method2b = typeof(FactionColors.FactionItemRenderer).GetMethod("DrawEquipmentAimingModded", BindingFlags.Instance | BindingFlags.Public);

            MethodInfo method3a = typeof(RimWorld.FloatMenuMakerMap).GetMethod("AddHumanlikeOrders", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo method3b = typeof(FactionColors.MenuMakerMapRestricted).GetMethod("AddHumanlikeOrders", BindingFlags.Static | BindingFlags.NonPublic);

            MethodInfo method4a = typeof(Verse.PawnRenderer).GetMethod("RenderPawnInternal", BindingFlags.NonPublic | BindingFlags.ExactBinding | BindingFlags.Instance, Type.DefaultBinder, CallingConventions.HasThis, new Type[] { typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool) }, null);

            MethodInfo method4b = typeof(FactionColors.PawnRendererModded).GetMethod("RenderPawnInternal", BindingFlags.NonPublic | BindingFlags.ExactBinding | BindingFlags.Instance, Type.DefaultBinder, CallingConventions.HasThis, new Type[] { typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool) }, null);
            

            if (method1a == null) Log.Message("No Method1A");
            if (method1b == null) Log.Message("No Method1B");
            if (method2a == null) Log.Message("No Method2A");
            if (method2b == null) Log.Message("No Method2b");
            if (method3a == null) Log.Message("No Method3A");
            if (method3b == null) Log.Message("No Method3B");
            if (method4a == null) Log.Message("No Method4A");
            if (method4b == null) Log.Message("No Method4B");
            try
            {                
                Detours.TryDetourFromTo(method1a, method1b);
                Detours.TryDetourFromTo(method2a, method2b);
                Detours.TryDetourFromTo(method3a, method3b);
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
