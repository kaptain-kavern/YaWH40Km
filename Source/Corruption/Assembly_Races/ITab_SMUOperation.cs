using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    [StaticConstructorOnStartup]
    public class ITab_SMUOperation : ITab
    {
        private static Vector2 billsScrollPosition = Vector2.zero;

        private static float billsScrollHeight = 1000f;

        private const float TitleHeight = 70f;
        
        private const float InfoHeight = 60f;
        
        private Building_MechanicusMedTable medTable
        {
            get
            {
                return this.SelThing as Building_MechanicusMedTable;
            }
        }

        private Pawn patient
        {
            get
            {
                if (this.medTable != null && this.medTable.patient != null)
                {
                    return this.medTable.patient;
                }
                return null;
                     
            }
        }

        protected Pawn currentOperator;


        public ITab_SMUOperation()
        {
            this.size = new Vector2(630f, 430f);
            this.labelKey = "TabMSU";
        }


        private bool ShouldAllowOperations()
        {
            Pawn pawnForHealth = medTable.patient;
            if (pawnForHealth.Dead)
            {
                return false;
            }
            return medTable.def.AllRecipes.Any((RecipeDef x) => x.AvailableNow) && (pawnForHealth.Faction == Faction.OfPlayer || (pawnForHealth.IsPrisonerOfColony || (pawnForHealth.HostFaction == Faction.OfPlayer && !pawnForHealth.health.capacities.CapableOf(PawnCapacityDefOf.Moving))) || ((!pawnForHealth.RaceProps.IsFlesh || pawnForHealth.Faction == null || !pawnForHealth.Faction.HostileTo(Faction.OfPlayer)) && (!pawnForHealth.RaceProps.Humanlike && pawnForHealth.Downed)));
        }

        protected override void FillTab()
        {
            Rect leftRect = new Rect(0f, 35f, 100f, this.size.y);
            Rect rightRect = new Rect(leftRect.xMax + 5f, 35f, 500f, this.size.y);
            Rect titleRect = new Rect(0f, 0f, this.size.x, 25f);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            string title = "SMU MK IV Interface";
            Widgets.Label(titleRect, title);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

        //    GUI.BeginGroup(leftRect);
            Rect rectSO = new Rect(leftRect.x, leftRect.y + 10f, 100f, 20f);
            float cury = 0f;
                DrawMedTableOperationsTab(rightRect, patient, patient, cury, this.medTable);
            
            Rect rectAO = new Rect(leftRect.x, rectSO.y + 10f, 100f, 20f);
   //         if (Widgets.ButtonText(rectSO, "AdvancedOperations".Translate()))
   //         {
   //             List<FloatMenuOption> advancedList = Building_MechanicusMedTable.GetRecipeFloatsFor(currentOperator, medTable.patient);
   //             Find.WindowStack.Add(new FloatMenu(advancedList, null, false));
   //         }

       //     GUI.EndGroup();

      //      GUI.BeginGroup(rightRect);
       //     GUI.EndGroup();
        }

        private static float DrawMedTableOperationsTab(Rect rect, Pawn patient, Thing thingForMedBills, float curY, Building_MechanicusMedTable medTable)
        {
            curY += 2f;
            Func<List<FloatMenuOption>> recipeOptionsMaker = delegate
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (RecipeDef current in thingForMedBills.def.AllRecipes)
                {
                    if (current.AvailableNow)
                    {
                        IEnumerable<ThingDef> enumerable = current.PotentiallyMissingIngredients(null, medTable.Map);
                        
                        if (enumerable != null && !enumerable.Any((ThingDef x) => x.isBodyPartOrImplant))
                        {
                            
                            if (!enumerable.Any((ThingDef x) => x.IsDrug))
                            {
                                
                                if (current.targetsBodyPart)
                                {
                                    foreach (BodyPartRecord current2 in current.Worker.GetPartsToApplyOn(patient, current))
                                    {                                        
                                        list.Add(Building_MechanicusMedTable.GenerateSurgeryOptionMedTable(patient, medTable, current, enumerable, current2));
                                    }
                                }
                                else
                                {
                                    list.Add(Building_MechanicusMedTable.GenerateSurgeryOptionMedTable(patient, medTable, current, enumerable, null));
                                }
                            }
                        }
                    }
                }
                return list;
            };
            
            Rect rect2 = new Rect(rect.x - 9f, curY, rect.width, rect.height - curY - 20f);
            ((IBillGiver)medTable).BillStack.DoListing(rect, recipeOptionsMaker, ref ITab_SMUOperation.billsScrollPosition, ref ITab_SMUOperation.billsScrollHeight);
            Log.Message(medTable.BillStack.Count.ToString());
            return curY;
        }


    }
}
