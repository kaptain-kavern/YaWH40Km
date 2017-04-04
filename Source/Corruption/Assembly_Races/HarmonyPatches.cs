using Corruption.DefOfs;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Corruption
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.ohu.corruption.main");

            harmony.Patch(AccessTools.Method(typeof(RimWorld.ThoughtHandler), "CanGetThought", new Type[] { typeof(ThoughtDef) }), new HarmonyMethod(typeof(HarmonyPatches), "CanGetThought", new Type[] { typeof(ThoughtDef) }), null);
            harmony.Patch(AccessTools.Method(typeof(RimWorld.MainTabWindow_Inspect), "DoInspectPaneButtons", null), null, new HarmonyMethod(typeof(HarmonyPatches), "DoInspectPaneButtons", null));
            
            harmony.Patch(AccessTools.Method(typeof(RimWorld.FactionGenerator), "GenerateFactionsIntoWorld", new Type[] { typeof(string) }), null, new HarmonyMethod(typeof(HarmonyPatches), "GenerateFactionsIntoWorldPostFix"), null);

        }

        public static void GenerateFactionsIntoWorldPostFix()
        {
            Log.Message("Generating Corruption Story Tracker");
            CorruptionStoryTracker corrTracker = (CorruptionStoryTracker)WorldObjectMaker.MakeWorldObject(DefOfs.C_WorldObjectDefOf.CorruptionStoryTracker);
            corrTracker.Tile = TileFinder.RandomStartingTile();
            Find.WorldObjects.Add(corrTracker);
        }

        private static Texture2D patronIcon
        {
            get
            {
                Pawn selPawn = Find.Selector.SingleSelectedThing as Pawn;

                if (selPawn != null)
                {
                    Need_Soul soul = selPawn.needs.TryGetNeed<Need_Soul>();
                    if (soul != null)
                    {
                        return ChaosGodsUtilities.GetPatronIcon(soul.patronInfo.PatronName);
                    }
                    return new Texture2D(20, 20);
                }
                return new Texture2D(20, 20);
            }
        }

        public static void DoInspectPaneButtons(Rect rect, ref float lineEndWidth, MainTabWindow_Inspect __instance)
        {
            if (Find.Selector.NumSelected == 1)
            {
                Thing singleSelectedThing = Find.Selector.SingleSelectedThing;
                if (singleSelectedThing != null)
                {
                    Widgets.InfoCardButton(rect.width - 48f, 0f, Find.Selector.SingleSelectedThing);
                    lineEndWidth += 24f;
                    Pawn pawn = singleSelectedThing as Pawn;
                    if (pawn != null && pawn.playerSettings != null && pawn.playerSettings.UsesConfigurableHostilityResponse)
                    {
                        HostilityResponseModeUtility.DrawResponseButton(new Vector2(rect.width - 72f, 0f), pawn);
                        lineEndWidth += 24f;
                    }

                    if (pawn != null)
                    {
                        Need_Soul soul;
                        if ((soul = pawn.needs.TryGetNeed<Need_Soul>()) != null)
                        {
                            float num = rect.height - 48;
                            Widgets.ListSeparator(ref num, rect.width, "PawnAlignment".Translate());
                            ColorInt colorInt = new ColorInt(65, 25, 25);
                            Texture2D soultex = SolidColorMaterials.NewSolidColorTexture(colorInt.ToColor);
                            ColorInt colorInt2 = new ColorInt(10, 10, 10);
                            Texture2D bgtex = SolidColorMaterials.NewSolidColorTexture(colorInt2.ToColor);
                            WidgetRow row = new WidgetRow(0f, rect.height - 24f);
                            row.FillableBar(93f, 16f, soul.CurLevelPercentage, soul.CurCategory.ToString(), soultex, bgtex);
                            String desc = "PawnAlignmentButtonDescription".Translate();
                            if (row.ButtonIcon(HarmonyPatches.patronIcon, desc))
                            {
                                Find.WindowStack.Add(new MainTabWindow_Alignment());
                            }
                            string culturalTolerance = "Cultural Tolerance: " + soul.CulturalTolerance.ToString();
                            Widgets.Label(new Rect(rect.width / 2, rect.height - 24, rect.width / 2, 16f), culturalTolerance);
                        }
                    }
                }
            }
        }
        public static bool IsAutomaton(Pawn pawn)
        {
            return pawn.AllComps.Any(i => i.GetType() == typeof(CompThoughtlessAutomaton));
        }

        public static bool HasSoulTraitRequirements(ThoughtDef thdef, Pawn p)
        {

            if (p.needs.TryGetNeed<Need_Soul>() == null)

            {
                HarmonyPatches.CreateNewSoul(p);
            }

            PatronInfo pinfo = p.needs.TryGetNeed<Need_Soul>().patronInfo;
            List<TraitDef> lvt = thdef.requiredTraits;
            if (lvt == null) Log.Message("No Required Traits");
            SoulTraitDef stdef = pinfo.PatronSpecificTrait(pinfo.PatronName);
            int tt = 0;
            foreach (TraitDef tdef in lvt)
            {
                if (stdef.defName == tdef.defName)
                {
                    tt += 1;
                }
            }
            if (tt > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool HasSoulTraitNullyfyingTraits(ThoughtDef def, Pawn p)
        {
            if (p == null) Log.Message("NoPawn");
            if (def == null) Log.Message("NoDef");

            if (p.needs.TryGetNeed<Need_Soul>() == null)

            {
                Log.Message("New Soul");
                HarmonyPatches.CreateNewSoul(p);
            }

            List<SoulTrait> straitlist = p.needs.TryGetNeed<Need_Soul>().SoulTraits;
            if (straitlist == null) Log.Message("Nolist");
            int tt = 0;
            foreach (SoulTrait strait in straitlist)
            {
                foreach (ThoughtDef tdef in strait.SDef.NullifiesThoughts)
                {
                    if (tdef.defName == def.defName)
                    {
                        tt += 1;
                    }
                }
            }
            if (tt > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static ThoughtDefAutomaton tdef = new ThoughtDefAutomaton();

        public static bool CanGetThought(ThoughtDef def, SituationalThoughtHandler __instance)
        {
            ProfilerThreadCheck.BeginSample("CanGetThought()");
            try
            {
                if (!def.validWhileDespawned && !__instance.pawn.Spawned && !def.IsMemory)
                {
                    bool result = false;
                    return result;
                }
                if (def.nullifyingTraits != null)
                {
                    for (int i = 0; i < def.nullifyingTraits.Count; i++)
                    {
                        if (__instance.pawn.story.traits.HasTrait(def.nullifyingTraits[i]))
                        {
                            bool result = false;
                            return result;
                        }
                    }
                }
                if (def.requiredTraits != null)
                {
                    for (int j = 0; j < def.requiredTraits.Count; j++)
                    {
                        if (!__instance.pawn.story.traits.HasTrait(def.requiredTraits[j]))
                        {
                            if (def != null && __instance.pawn != null)
                            {
                                return HasSoulTraitRequirements(def, __instance.pawn);
                            }
                            else return false;
                        }

                        if (!__instance.pawn.story.traits.HasTrait(def.requiredTraits[j]))
                        {
                            bool result = false;
                            return result;
                        }
                        if (def.RequiresSpecificTraitsDegree && def.requiredTraitsDegree != __instance.pawn.story.traits.DegreeOfTrait(def.requiredTraits[j]))
                        {
                            bool result = false;
                            return result;
                        }
                    }
                }
                if (def.nullifiedIfNotColonist && !__instance.pawn.IsColonist)
                {
                    bool result = false;
                    return result;
                }
                if (ThoughtUtility.IsSituationalThoughtNullifiedByHediffs(def, __instance.pawn))
                {
                    bool result = false;
                    return result;
                }
                if (ThoughtUtility.IsThoughtNullifiedByOwnTales(def, __instance.pawn))
                {
                    bool result = false;
                    return result;
                }
                if (HasSoulTraitNullyfyingTraits(def, __instance.pawn))
                {
                    return false;
                }
                if (IsAutomaton(__instance.pawn))
                {
                    ThoughtDefAutomaton Tdef = new ThoughtDefAutomaton();
                    if ((Tdef = def as ThoughtDefAutomaton) != null && Tdef.IsAutomatonThought)
                    {
                        return true;
                    }

                    return false;
                }
            }
            finally
            {
                ProfilerThreadCheck.EndSample();
            }
            return true;
        }

        public static void CreateNewSoul(Pawn pepe)
        {
            {
                Need need = (Need)Activator.CreateInstance(typeof(Need_Soul), new object[]
            {
                pepe
            });
                need.def = C_NeedDefOf.Need_Soul;
                int x = pepe.needs.AllNeeds.Count;
                pepe.needs.AllNeeds.Insert(x - 1, need);
                //          pepe.needs.AllNeeds.Add(need);
            }
        }
    }
}