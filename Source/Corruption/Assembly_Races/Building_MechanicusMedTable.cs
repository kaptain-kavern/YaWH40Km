using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Corruption
{
    public class Building_MechanicusMedTable : Building_Casket, IBillGiver, IBillGiverWithTickAction
    {
        
        private CompPowerTrader powerComp;

        private CompBreakdownable breakdownableComp;

        public Building_MechanicusMedTable()
        {
            this.medOpStack = new BillStack(this);
        }

        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);
            this.powerComp = base.GetComp<CompPowerTrader>();
            this.breakdownableComp = base.GetComp<CompBreakdownable>();
        }

        public BillStack medOpStack;

        public Pawn patient
        {
            get
            {
                return this.ContainedThing as Pawn;
            }
        }

        private Material toolMat
        {
            get
            {
                return GraphicDatabase.Get<Graphic_Single>("Things/Mechanicus/MechanicusTables/MechanicusTable_Medical_Tools", ShaderDatabase.Cutout, this.Graphic.data.drawSize, Color.white).MatAt(this.Rotation);
            }
        }

        public BillStack BillStack
        {
            get
            {
                return this.medOpStack;
            }
        }

        public IEnumerable<IntVec3> IngredientStackCells
        {
            get
            {
                return GenAdj.CellsOccupiedBy(this);
            }
        }

        public static List<FloatMenuOption> GetRecipeFloatsFor(Building_MechanicusMedTable medTable)
        {
            Pawn patient = medTable.patient;
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (RecipeDef current in patient.def.AllRecipes)
            {
                if (current.AvailableNow)
                {
                    IEnumerable<ThingDef> enumerable = current.PotentiallyMissingIngredients(null, medTable.patient.Map);
                    if (!enumerable.Any((ThingDef x) => x.isBodyPartOrImplant))
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
        }        

        public static FloatMenuOption GenerateSurgeryOptionMedTable(Pawn pawn, Building_MechanicusMedTable medTable, RecipeDef recipe, IEnumerable<ThingDef> missingIngredients, BodyPartRecord part = null)
        {
            Log.Message("Generatung Option");
            string text = recipe.Worker.GetLabelWhenUsedOn(pawn, part);
            if (part != null && !recipe.hideBodyPartNames)
            {
                text = text + " (" + part.def.label + ")";
            }
            if (missingIngredients.Any<ThingDef>())
            {
                text += " (";
                bool flag = true;
                foreach (ThingDef current in missingIngredients)
                {
                    if (!flag)
                    {
                        text += ", ";
                    }
                    flag = false;
                    text += "MissingMedicalBillIngredient".Translate(new object[]
                    {
                current.label
                    });
                }
                text += ")";
                return new FloatMenuOption(text, null, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            Action action = delegate
            {
                if (medTable == null)
                {
                    return;
                }
                Bill_MedicalTable bill_Medical = new Bill_MedicalTable(recipe);
                medTable.medOpStack.AddBill(bill_Medical);
                bill_Medical.Part = part;
                if (recipe.conceptLearned != null)
                {
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(recipe.conceptLearned, KnowledgeAmount.Total);
                }
                Map map = medTable.Map;
                if (!map.mapPawns.FreeColonists.Any((Pawn col) => recipe.PawnSatisfiesSkillRequirements(col)))
                {
                    Bill.CreateNoPawnsWithSkillDialog(recipe);
                }
                if (medTable.patient.Faction != null && !medTable.patient.Faction.HostileTo(Faction.OfPlayer) && recipe.Worker.IsViolationOnPawn(medTable.patient, part, Faction.OfPlayer))
                {
                    Messages.Message("MessageMedicalOperationWillAngerFaction".Translate(new object[]
                    {
                medTable.Faction
                    }), medTable, MessageSound.Negative);
                }
                MethodInfo info = typeof(HealthCardUtility).GetMethod("GetMinRequiredMedicine", BindingFlags.Static | BindingFlags.NonPublic);
                if (info == null) Log.Message("NoInfo");
                Log.Message("tryingToInvoke");
                ThingDef minRequiredMedicine = (ThingDef)info.Invoke(null, new object[] {recipe });
                if (minRequiredMedicine != null && medTable.patient.playerSettings != null && !medTable.patient.playerSettings.medCare.AllowsMedicine(minRequiredMedicine))
                {
                    Messages.Message("MessageTooLowMedCare".Translate(new object[]
                    {
                minRequiredMedicine.label,
                medTable.LabelShort,
                medTable.patient.playerSettings.medCare.GetLabel()
                    }), medTable, MessageSound.Negative);
                }
                Log.Message("C3");
            };
            return new FloatMenuOption(text, action, MenuOptionPriority.Default, null, null, 0f, null, null);
        }

        //      private List<ThingAmount> necessaryIngredients(RecipeDef recipe)
        //       {
        //            List<ThingAmount> list = new List<ThingAmount>();
        //          foreach (ThingDef )
        //      }

        public override void Draw()
        {
            base.Draw();
            Vector3 vector = this.DrawPos;
            vector.y = Altitudes.AltitudeFor(AltitudeLayer.Building) + 0.1f;
            float angle = this.Rotation.AsAngle;
            Vector3 s = new Vector3(this.Graphic.data.drawSize.x, 1f, this.Graphic.data.drawSize.y);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, this.toolMat, 0);
            if (this.patient != null)
            {
                Material bodymat = this.patient.Drawer.renderer.graphics.nakedGraphic.MatFront;
                Material headmat = this.patient.Drawer.renderer.graphics.headGraphic.MatFront;
                Material hairmat = this.patient.Drawer.renderer.graphics.hairGraphic.MatFront;
                Vector3 sBody = new Vector3(1.0f, 1f, 1.0f);
                Matrix4x4 matrixBody = default(Matrix4x4);
                vector.y -= 0.05f;
                matrixBody.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), sBody);

                Graphics.DrawMesh(MeshPool.humanlikeBodySet.MeshAt(this.Rotation), matrixBody, bodymat, 0);

                Matrix4x4 matrixHead = default(Matrix4x4);
                Vector3 headVec = vector + new Vector3(Mathf.Sin(angle) * 0.2f, 0.03f, Mathf.Cos(angle) * 0.2f);
                matrixHead.SetTRS(headVec, Quaternion.AngleAxis(angle, Vector3.up), new Vector3(1.0f, 1f, 1.0f));
                Graphics.DrawMesh(MeshPool.humanlikeHeadSet.MeshAt(this.Rotation), matrixHead, headmat, 0);
                Graphics.DrawMesh(MeshPool.humanlikeHairSetAverage.MeshAt(this.Rotation), matrixHead, hairmat, 0);


                //        Log.Message("patient: " + this.patient.Rotation.ToString() + "   Bed:  " + this.Rotation.ToString());

            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator();
            while (enumerator.MoveNext())
            {
                Gizmo current = enumerator.Current;
                yield return current;
            }
            if (base.Faction == Faction.OfPlayer && this.innerContainer.Count > 0 && this.def.building.isPlayerEjectable)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.action = new Action(this.EjectContents);
                command_Action.defaultLabel = "CommandPodEject".Translate();
                command_Action.defaultDesc = "CommandPodEjectDesc".Translate();
                if (this.innerContainer.Count == 0)
                {
                    command_Action.Disable("CommandPodEjectFailEmpty".Translate());
                }
                command_Action.hotKey = KeyBindingDefOf.Misc1;
                command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject", true);
                yield return command_Action;
            }
            yield break;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            IEnumerator<FloatMenuOption> enumerator = base.GetFloatMenuOptions(selPawn).GetEnumerator();
            while (enumerator.MoveNext())
            {
                FloatMenuOption current = enumerator.Current;
                yield return current;
            }

            if (this.innerContainer.Count == 0)
            {
                if (!selPawn.CanReserve(this, 1))
                {
                    FloatMenuOption floatMenuOption = new FloatMenuOption("CannotUseReserved".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                    yield return floatMenuOption;
                }
                if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly, false, TraverseMode.ByPawn))
                {
                    FloatMenuOption floatMenuOption2 = new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                    yield return floatMenuOption2;
                }
                string label = "EnterSurgicalUnit".Translate();
                Action action = delegate
                {
                    Job job = new Job(CorruptionDefOfs.EnterMecMedTable, this);
                    selPawn.jobs.TryTakeOrderedJob(job);
                };
                yield return new FloatMenuOption(label, action, MenuOptionPriority.Default, null, null, 0f, null, null);

                if (this.patient != null && this.BillStack.Count > 0)
                {

                }

                foreach (Pawn prisoner in selPawn.Map.mapPawns.PrisonersOfColonySpawned)
                {
                    string label2 = "CarryPrisonerSurgicalUnit".Translate(new object[]
                        {
                            prisoner.LabelShort
                        });
                    Action action2 = delegate
                    {
                        Job job = new Job(CorruptionDefOfs.CarryToMecMedTable, prisoner, this);
                        selPawn.jobs.TryTakeOrderedJob(job);
                    };
                    yield return new FloatMenuOption(label2, action2, MenuOptionPriority.Default, null, null, 0f, null, null);
                }
            }
            yield break;
        }

        public override void EjectContents()
        {
            ThingDef filthSlime = ThingDefOf.FilthSlime;
            foreach (Thing current in this.innerContainer)
            {
                Pawn pawn = current as Pawn;
                if (pawn != null)
                {
                    PawnComponentsUtility.AddComponentsForSpawn(pawn);
                    pawn.filth.GainFilth(filthSlime);
                    pawn.health.AddHediff(HediffDefOf.CryptosleepSickness, null, null);
                }
            }
            if (!base.Destroyed)
            {
                SoundDef.Named("CryptosleepCasketEject").PlayOneShot(SoundInfo.InMap(new TargetInfo(base.Position, base.Map, false), MaintenanceType.None));
            }
            base.EjectContents();
        }

        public bool CanWorkWithoutPower
        {
            get
            {
                return this.powerComp == null || this.def.building.unpoweredWorkTableWorkSpeedFactor > 0f;
            }
        }

        public bool CurrentlyUsable()
        {
            return (this.CanWorkWithoutPower || (this.powerComp != null && this.powerComp.PowerOn)) && (this.breakdownableComp == null || !this.breakdownableComp.BrokenDown);
        }

        public void UsedThisTick()
        {
            throw new NotImplementedException();
        }

        private static void AddUndraftedOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            IntVec3 clickCell = IntVec3.FromVector3(clickPos);
            bool flag = false;
            bool flag2 = false;
            foreach (Thing current in pawn.Map.thingGrid.ThingsAt(clickCell))
            {
                flag2 = true;
                if (pawn.CanReach(current, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
                {
                    flag = true;
                    break;
                }
            }
            if (flag2 && !flag)
            {
                opts.Add(new FloatMenuOption("(" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
                return;
            }
            JobGiver_Work jobGiver_Work = pawn.thinker.TryGetMainTreeThinkNode<JobGiver_Work>();
            if (jobGiver_Work != null)
            {
                foreach (Thing current2 in pawn.Map.thingGrid.ThingsAt(clickCell))
                {
                    Pawn pawn2 = pawn.Map.reservationManager.FirstReserverOf(current2, pawn.Faction, true);
                    if (pawn2 != null && pawn2 != pawn)
                    {
                        opts.Add(new FloatMenuOption("IsReservedBy".Translate(new object[]
                        {
                    current2.LabelShort.CapitalizeFirst(),
                    pawn2.LabelShort
                        }), null, MenuOptionPriority.Default, null, null, 0f, null, null));
                    }
                    else
                    {
                        foreach (WorkTypeDef current3 in DefDatabase<WorkTypeDef>.AllDefsListForReading)
                        {
                            for (int i = 0; i < current3.workGiversByPriority.Count; i++)
                            {
                                WorkGiver_Scanner workGiver_Scanner = current3.workGiversByPriority[i].Worker as WorkGiver_Scanner;
                                if (workGiver_Scanner != null && workGiver_Scanner.def.directOrderable && !workGiver_Scanner.ShouldSkip(pawn))
                                {
                                    JobFailReason.Clear();
                                    Log.Message("FoundWOrkGiver");
                                    if (workGiver_Scanner.PotentialWorkThingRequest.Accepts(current2) || (workGiver_Scanner.PotentialWorkThingsGlobal(pawn) != null && workGiver_Scanner.PotentialWorkThingsGlobal(pawn).Contains(current2)))
                                    {
                                        Job job;
                                        if (!workGiver_Scanner.HasJobOnThingForced(pawn, current2))
                                        {
                                            job = null;
                                        }
                                        else
                                        {
                                            job = workGiver_Scanner.JobOnThingForced(pawn, current2);
                                        }
                                        if (job == null)
                                        {
                                            if (JobFailReason.HaveReason)
                                            {
                                                string label3 = "CannotGenericWork".Translate(new object[]
                                                {
                                            workGiver_Scanner.def.verb,
                                            current2.LabelShort
                                                }) + " (" + JobFailReason.Reason + ")";
                                                opts.Add(new FloatMenuOption(label3, null, MenuOptionPriority.Default, null, null, 0f, null, null));
                                            }
                                        }
                                        else
                                        {
                                            WorkTypeDef workType = workGiver_Scanner.def.workType;
                                            Action action = null;
                                            PawnCapacityDef pawnCapacityDef = workGiver_Scanner.MissingRequiredCapacity(pawn);
                                            string label;
                                            if (pawnCapacityDef != null)
                                            {
                                                label = "CannotMissingHealthActivities".Translate(new object[]
                                                {
                                            pawnCapacityDef.label
                                                });
                                            }
                                            else if (pawn.jobs.curJob != null && pawn.jobs.curJob.JobIsSameAs(job))
                                            {
                                                label = "CannotGenericAlreadyAm".Translate(new object[]
                                                {
                                            workType.gerundLabel,
                                            current2.LabelShort
                                                });
                                            }
                                            else if (pawn.workSettings.GetPriority(workType) == 0)
                                            {
                                                if (pawn.story.WorkTypeIsDisabled(workType))
                                                {
                                                    label = "CannotPrioritizeWorkTypeDisabled".Translate(new object[]
                                                    {
                                                workType.gerundLabel
                                                    });
                                                }
                                                else if ("CannotPrioritizeNotAssignedToWorkType".CanTranslate())
                                                {
                                                    label = "CannotPrioritizeNotAssignedToWorkType".Translate(new object[]
                                                    {
                                                workType.gerundLabel
                                                    });
                                                }
                                                else
                                                {
                                                    label = "CannotPrioritizeIsNotA".Translate(new object[]
                                                    {
                                                pawn.NameStringShort,
                                                workType.pawnLabel
                                                    });
                                                }
                                            }
                                            else if (job.def == JobDefOf.Research && current2 is Building_ResearchBench)
                                            {
                                                label = "CannotPrioritizeResearch".Translate();
                                            }
                                            else if (current2.IsForbidden(pawn))
                                            {
                                                if (!current2.Position.InAllowedArea(pawn))
                                                {
                                                    label = "CannotPrioritizeForbiddenOutsideAllowedArea".Translate(new object[]
                                                    {
                                                current2.Label
                                                    });
                                                }
                                                else
                                                {
                                                    label = "CannotPrioritizeForbidden".Translate(new object[]
                                                    {
                                                current2.Label
                                                    });
                                                }
                                            }
                                            else if (!pawn.CanReach(current2, workGiver_Scanner.PathEndMode, Danger.Deadly, false, TraverseMode.ByPawn))
                                            {
                                                label = current2.Label + ": " + "NoPath".Translate();
                                            }
                                            else
                                            {
                                                label = "PrioritizeGeneric".Translate(new object[]
                                                {
                                            workGiver_Scanner.def.gerund,
                                            current2.Label
                                                });
                                                Job localJob = job;
                                                WorkGiver_Scanner localScanner = workGiver_Scanner;
                                                action = delegate
                                                {
                                                    pawn.jobs.TryTakeOrderedJobPrioritizedWork(localJob, localScanner, clickCell);
                                                };
                                            }
                                            if (!opts.Any((FloatMenuOption op) => op.Label == label.TrimEnd(new char[0])))
                                            {
                                                opts.Add(new FloatMenuOption(label, action, MenuOptionPriority.Default, null, null, 0f, null, null));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                Pawn pawn3 = pawn.Map.reservationManager.FirstReserverOf(clickCell, pawn.Faction, true);
                if (pawn3 != null && pawn3 != pawn)
                {
                    opts.Add(new FloatMenuOption("IsReservedBy".Translate(new object[]
                    {
                "AreaLower".Translate(),
                pawn3.LabelShort
                    }).CapitalizeFirst(), null, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                else
                {
                    foreach (WorkTypeDef current4 in DefDatabase<WorkTypeDef>.AllDefsListForReading)
                    {
                        for (int j = 0; j < current4.workGiversByPriority.Count; j++)
                        {
                            WorkGiver_Scanner workGiver_Scanner2 = current4.workGiversByPriority[j].Worker as WorkGiver_Scanner;
                            if (workGiver_Scanner2 != null && workGiver_Scanner2.def.directOrderable && !workGiver_Scanner2.ShouldSkip(pawn))
                            {
                                JobFailReason.Clear();
                                if (workGiver_Scanner2.PotentialWorkCellsGlobal(pawn).Contains(clickCell))
                                {
                                    Job job2;
                                    if (!workGiver_Scanner2.HasJobOnCell(pawn, clickCell))
                                    {
                                        job2 = null;
                                    }
                                    else
                                    {
                                        job2 = workGiver_Scanner2.JobOnCell(pawn, clickCell);
                                    }
                                    if (job2 == null)
                                    {
                                        if (JobFailReason.HaveReason)
                                        {
                                            string label2 = "CannotGenericWork".Translate(new object[]
                                            {
                                        workGiver_Scanner2.def.verb,
                                        "AreaLower".Translate()
                                            }) + " (" + JobFailReason.Reason + ")";
                                            opts.Add(new FloatMenuOption(label2, null, MenuOptionPriority.Default, null, null, 0f, null, null));
                                        }
                                    }
                                    else
                                    {
                                        WorkTypeDef workType2 = workGiver_Scanner2.def.workType;
                                        Action action2 = null;
                                        PawnCapacityDef pawnCapacityDef2 = workGiver_Scanner2.MissingRequiredCapacity(pawn);
                                        string label;
                                        if (pawnCapacityDef2 != null)
                                        {
                                            label = "CannotMissingHealthActivities".Translate(new object[]
                                            {
                                        pawnCapacityDef2.label
                                            });
                                        }
                                        else if (pawn.jobs.curJob != null && pawn.jobs.curJob.JobIsSameAs(job2))
                                        {
                                            label = "CannotGenericAlreadyAm".Translate(new object[]
                                            {
                                        workType2.gerundLabel,
                                        "AreaLower".Translate()
                                            });
                                        }
                                        else if (pawn.workSettings.GetPriority(workType2) == 0)
                                        {
                                            if (pawn.story.WorkTypeIsDisabled(workType2))
                                            {
                                                label = "CannotPrioritizeWorkTypeDisabled".Translate(new object[]
                                                {
                                            workType2.gerundLabel
                                                });
                                            }
                                            else if ("CannotPrioritizeNotAssignedToWorkType".CanTranslate())
                                            {
                                                label = "CannotPrioritizeNotAssignedToWorkType".Translate(new object[]
                                                {
                                            workType2.gerundLabel
                                                });
                                            }
                                            else
                                            {
                                                label = "CannotPrioritizeIsNotA".Translate(new object[]
                                                {
                                            pawn.NameStringShort,
                                            workType2.pawnLabel
                                                });
                                            }
                                        }
                                        else if (!pawn.CanReach(clickCell, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
                                        {
                                            label = "AreaLower".Translate().CapitalizeFirst() + ": " + "NoPath".Translate();
                                        }
                                        else
                                        {
                                            label = "PrioritizeGeneric".Translate(new object[]
                                            {
                                        workGiver_Scanner2.def.gerund,
                                        "AreaLower".Translate()
                                            });
                                            Job localJob = job2;
                                            WorkGiver_Scanner localScanner = workGiver_Scanner2;
                                            action2 = delegate
                                            {
                                                pawn.jobs.TryTakeOrderedJobPrioritizedWork(localJob, localScanner, clickCell);
                                            };
                                        }
                                        if (!opts.Any((FloatMenuOption op) => op.Label == label.TrimEnd(new char[0])))
                                        {
                                            opts.Add(new FloatMenuOption(label, action2, MenuOptionPriority.Default, null, null, 0f, null, null));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
