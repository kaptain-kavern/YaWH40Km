using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using UnityEngine;
using AlienRace;

namespace FactionColors
{
    public class MenuMakerMapRestricted
    {

        private static bool RaceRestricted(Pawn pawn, Apparel app)
        {
            if (app.GetComp<CompRestritctedRace>() != null)
            {
                CompRestritctedRace rcomp = app.GetComp<CompRestritctedRace>();

                if (rcomp.Props.RestrictedToRace != null)
                {
                    //   Log.Message(pawn.kindDef.race.defName.ToString());
                    //   Log.Message(rcomp.Props.RestrictedToRace);
                    if (pawn.kindDef.race.ToString() == rcomp.Props.RestrictedToRace)
                    {
                        return false;
                    }
                    else if (pawn.GetType() == typeof(AlienRace.AlienPawn))
                    {
                        AlienPawn alpawn = pawn as AlienPawn;

                        //          Log.Message("Alpawn :"+ alpawn.kindDef.race.defName.ToString());
                        if (alpawn.kindDef.race.ToString() == rcomp.Props.RestrictedToRace)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }
                    return true;
                }
                else return false;
            }
            else return false;
        }

        private static void AddHumanlikeOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            IntVec3 c = IntVec3.FromVector3(clickPos);
            foreach (Thing current in c.GetThingList(pawn.Map))
            {
                Thing t = current;
                if (t.def.ingestible != null && pawn.RaceProps.CanEverEat(t) && t.IngestibleNow)
                {
                    string text;
                    if (t.def.ingestible.ingestCommandString.NullOrEmpty())
                    {
                        text = "ConsumeThing".Translate(new object[]
                        {
                    t.LabelShort
                        });
                    }
                    else
                    {
                        text = string.Format(t.def.ingestible.ingestCommandString, t.LabelShort);
                    }
                    FloatMenuOption item5;
                    if (t.def.IsPleasureDrug && pawn.IsTeetotaler())
                    {
                        item5 = new FloatMenuOption(text + " (" + TraitDefOf.DrugDesire.DataAtDegree(-1).label + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                    }
                    else if (!pawn.CanReach(t, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
                    {
                        item5 = new FloatMenuOption(text + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                    }
                    else if (!pawn.CanReserve(t, 1))
                    {
                        item5 = new FloatMenuOption(text + " (" + "ReservedBy".Translate(new object[]
                        {
                    pawn.Map.reservationManager.FirstReserverOf(t, pawn.Faction, true).LabelShort
                        }) + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                    }
                    else
                    {
                        MenuOptionPriority priority = (!(t is Corpse)) ? MenuOptionPriority.Default : MenuOptionPriority.Low;
                        item5 = new FloatMenuOption(text, delegate
                        {
                            t.SetForbidden(false, true);
                            Job job = new Job(JobDefOf.Ingest, t);
                            job.count = FoodUtility.WillIngestStackCountOf(pawn, t.def);
                            pawn.jobs.TryTakeOrderedJob(job);
                        }, priority, null, null, 0f, null, null);
                    }
                    opts.Add(item5);
                }
            }
            if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                foreach (LocalTargetInfo current2 in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), true))
                {
                    Pawn victim = (Pawn)current2.Thing;
                    if (!victim.InBed() && pawn.CanReserveAndReach(victim, PathEndMode.OnCell, Danger.Deadly, 1))
                    {
                        if ((victim.Faction == Faction.OfPlayer && victim.MentalStateDef == null) || (victim.Faction != Faction.OfPlayer && victim.MentalStateDef == null && !victim.IsPrisonerOfColony && (victim.Faction == null || !victim.Faction.HostileTo(Faction.OfPlayer))))
                        {
                            Pawn victim2 = victim;
                            opts.Add(new FloatMenuOption("Rescue".Translate(new object[]
                            {
                        victim.LabelCap
                            }), delegate
                            {
                                Building_Bed building_Bed = RestUtility.FindBedFor(victim, pawn, false, false, false);
                                if (building_Bed == null)
                                {
                                    string str2;
                                    if (victim.RaceProps.Animal)
                                    {
                                        str2 = "NoAnimalBed".Translate();
                                    }
                                    else
                                    {
                                        str2 = "NoNonPrisonerBed".Translate();
                                    }
                                    Messages.Message("CannotRescue".Translate() + ": " + str2, victim, MessageSound.RejectInput);
                                    return;
                                }
                                Job job = new Job(JobDefOf.Rescue, victim, building_Bed);
                                job.count = 1;
                                pawn.jobs.TryTakeOrderedJob(job);
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Rescuing, KnowledgeAmount.Total);
                            }, MenuOptionPriority.RescueOrCapture, null, victim2, 0f, null, null));
                        }
                        if (victim.RaceProps.Humanlike && (victim.MentalStateDef != null || victim.Faction != Faction.OfPlayer || (victim.Downed && victim.guilt.IsGuilty)))
                        {
                            Pawn victim2 = victim;
                            opts.Add(new FloatMenuOption("Capture".Translate(new object[]
                            {
                        victim.LabelCap
                            }), delegate
                            {
                                Building_Bed building_Bed = RestUtility.FindBedFor(victim, pawn, true, false, false);
                                if (building_Bed == null)
                                {
                                    Messages.Message("CannotCapture".Translate() + ": " + "NoPrisonerBed".Translate(), victim, MessageSound.RejectInput);
                                    return;
                                }
                                Job job = new Job(JobDefOf.Capture, victim, building_Bed);
                                job.count = 1;
                                pawn.jobs.TryTakeOrderedJob(job);
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Capturing, KnowledgeAmount.Total);
                            }, MenuOptionPriority.RescueOrCapture, null, victim2, 0f, null, null));
                        }
                    }
                }
                foreach (LocalTargetInfo current3 in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), true))
                {
                    LocalTargetInfo localTargetInfo = current3;
                    Pawn victim = (Pawn)localTargetInfo.Thing;
                    if (victim.Downed && pawn.CanReserveAndReach(victim, PathEndMode.OnCell, Danger.Deadly, 1) && Building_CryptosleepCasket.FindCryptosleepCasketFor(victim, pawn) != null)
                    {
                        string label = "CarryToCryptosleepCasket".Translate(new object[]
                        {
                    localTargetInfo.Thing.LabelCap
                        });
                        JobDef jDef = JobDefOf.CarryToCryptosleepCasket;
                        Action action = delegate
                        {
                            Building_CryptosleepCasket building_CryptosleepCasket = Building_CryptosleepCasket.FindCryptosleepCasketFor(victim, pawn);
                            if (building_CryptosleepCasket == null)
                            {
                                Messages.Message("CannotCarryToCryptosleepCasket".Translate() + ": " + "NoCryptosleepCasket".Translate(), victim, MessageSound.RejectInput);
                                return;
                            }
                            Job job = new Job(jDef, victim, building_CryptosleepCasket);
                            job.count = 1;
                            pawn.jobs.TryTakeOrderedJob(job);
                        };
                        Pawn victim2 = victim;
                        opts.Add(new FloatMenuOption(label, action, MenuOptionPriority.Default, null, victim2, 0f, null, null));
                    }
                }
            }
            foreach (LocalTargetInfo current4 in GenUI.TargetsAt(clickPos, TargetingParameters.ForStrip(pawn), true))
            {
                LocalTargetInfo stripTarg = current4;
                FloatMenuOption item2;
                if (!pawn.CanReach(stripTarg, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
                {
                    item2 = new FloatMenuOption("CannotStrip".Translate(new object[]
                    {
                stripTarg.Thing.LabelCap
                    }) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                }
                else if (!pawn.CanReserveAndReach(stripTarg, PathEndMode.ClosestTouch, Danger.Deadly, 1))
                {
                    item2 = new FloatMenuOption("CannotStrip".Translate(new object[]
                    {
                stripTarg.Thing.LabelCap
                    }) + " (" + "ReservedBy".Translate(new object[]
                    {
                pawn.Map.reservationManager.FirstReserverOf(stripTarg, pawn.Faction, true).LabelShort
                    }) + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                }
                else
                {
                    item2 = new FloatMenuOption("Strip".Translate(new object[]
                    {
                stripTarg.Thing.LabelCap
                    }), delegate
                    {
                        stripTarg.Thing.SetForbidden(false, false);
                        pawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.Strip, stripTarg));
                    }, MenuOptionPriority.Default, null, null, 0f, null, null);
                }
                opts.Add(item2);
            }
            if (pawn.equipment != null)
            {
                ThingWithComps equipment = null;
                List<Thing> thingList = c.GetThingList(pawn.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    if (thingList[i].TryGetComp<CompEquippable>() != null)
                    {
                        equipment = (ThingWithComps)thingList[i];
                        break;
                    }
                }
                if (equipment != null)
                {
                    string labelShort = equipment.LabelShort;
                    FloatMenuOption item3;
                    if (equipment.def.IsWeapon && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
                    {
                        item3 = new FloatMenuOption("CannotEquip".Translate(new object[]
                        {
                    labelShort
                        }) + " (" + "IsIncapableOfViolenceLower".Translate(new object[]
                        {
                    pawn.LabelShort
                        }) + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                    }
                    else if (!pawn.CanReach(equipment, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
                    {
                        item3 = new FloatMenuOption("CannotEquip".Translate(new object[]
                        {
                    labelShort
                        }) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                    }
                    else if (!pawn.CanReserve(equipment, 1))
                    {
                        item3 = new FloatMenuOption("CannotEquip".Translate(new object[]
                        {
                    labelShort
                        }) + " (" + "ReservedBy".Translate(new object[]
                        {
                    pawn.Map.reservationManager.FirstReserverOf(equipment, pawn.Faction, true).LabelShort
                        }) + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                    }
                    else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                    {
                        item3 = new FloatMenuOption("CannotEquip".Translate(new object[]
                        {
                    labelShort
                        }) + " (" + "Incapable".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                    }
                    else
                    {
                        string text2 = "Equip".Translate(new object[]
                        {
                    labelShort
                        });
                        if (equipment.def.IsRangedWeapon && pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.Brawler))
                        {
                            text2 = text2 + " " + "EquipWarningBrawler".Translate();
                        }
                        item3 = new FloatMenuOption(text2, delegate
                        {
                            equipment.SetForbidden(false, true);
                            pawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.Equip, equipment));
                            MoteMaker.MakeStaticMote(equipment.DrawPos, equipment.Map, ThingDefOf.Mote_FeedbackEquip, 1f);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.EquippingWeapons, KnowledgeAmount.Total);
                        }, MenuOptionPriority.High, null, null, 0f, null, null);
                    }
                    opts.Add(item3);
                }
            }
            if (pawn.apparel != null)
            {
                Apparel apparel = pawn.Map.thingGrid.ThingAt<Apparel>(c);
                if (apparel != null)
                {
                    FloatMenuOption item4;
                    if (!pawn.CanReach(apparel, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
                    {
                        item4 = new FloatMenuOption("CannotWear".Translate(new object[]
                        {
                    apparel.Label
                        }) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                    }
                    else if (!pawn.CanReserve(apparel, 1))
                    {
                        Pawn pawn2 = pawn.Map.reservationManager.FirstReserverOf(apparel, pawn.Faction, true);
                        item4 = new FloatMenuOption("CannotWear".Translate(new object[]
                        {
                    apparel.Label
                        }) + " (" + "ReservedBy".Translate(new object[]
                        {
                    pawn2.LabelShort
                        }) + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                    }
                    else if (!ApparelUtility.HasPartsToWear(pawn, apparel.def))
                    {
                        item4 = new FloatMenuOption("CannotWear".Translate(new object[]
                        {
                    apparel.Label
                        }) + " (" + "CannotWearBecauseOfMissingBodyParts".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                    }
                    else if (RaceRestricted(pawn, apparel))
                    {

                        item4 = new FloatMenuOption("CannotWear".Translate(new object[]
                        {
                    apparel.Label
                        }) + " (" + "CannotWearBecauseOfWrongRace".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null);
                    }
                    else
                    {
                        item4 = new FloatMenuOption("ForceWear".Translate(new object[]
                        {
                    apparel.LabelShort
                        }), delegate
                        {
                            apparel.SetForbidden(false, true);
                            Job job = new Job(JobDefOf.Wear, apparel);
                            pawn.jobs.TryTakeOrderedJob(job);
                        }, MenuOptionPriority.High, null, null, 0f, null, null);
                    }
                    opts.Add(item4);
                }
            }
            if (!pawn.Map.IsPlayerHome)
            {
                Thing item = c.GetFirstItem(pawn.Map);
                if (item != null && item.def.EverHaulable)
                {
                    if (!pawn.CanReach(item, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
                    {
                        opts.Add(new FloatMenuOption("CannotPickUp".Translate(new object[]
                        {
                    item.Label
                        }) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
                    }
                    else if (!pawn.CanReserve(item, 1))
                    {
                        Pawn pawn3 = pawn.Map.reservationManager.FirstReserverOf(item, pawn.Faction, true);
                        opts.Add(new FloatMenuOption("CannotPickUp".Translate(new object[]
                        {
                    item.Label
                        }) + " (" + "ReservedBy".Translate(new object[]
                        {
                    pawn3.LabelShort
                        }) + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
                    }
                    else if (MassUtility.WillBeOverEncumberedAfterPickingUp(pawn, item, 1))
                    {
                        opts.Add(new FloatMenuOption("CannotPickUp".Translate(new object[]
                        {
                    item.Label
                        }) + " (" + "TooHeavy".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
                    }
                    else if (item.stackCount == 1)
                    {
                        opts.Add(new FloatMenuOption("PickUp".Translate(new object[]
                        {
                    item.Label
                        }), delegate
                        {
                            item.SetForbidden(false, false);
                            Job job = new Job(JobDefOf.TakeInventory, item);
                            job.count = 1;
                            pawn.jobs.TryTakeOrderedJob(job);
                        }, MenuOptionPriority.High, null, null, 0f, null, null));
                    }
                    else
                    {
                        if (MassUtility.WillBeOverEncumberedAfterPickingUp(pawn, item, item.stackCount))
                        {
                            opts.Add(new FloatMenuOption("CannotPickUpAll".Translate(new object[]
                            {
                        item.Label
                            }) + " (" + "TooHeavy".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
                        }
                        else
                        {
                            opts.Add(new FloatMenuOption("PickUpAll".Translate(new object[]
                            {
                        item.Label
                            }), delegate
                            {
                                item.SetForbidden(false, false);
                                Job job = new Job(JobDefOf.TakeInventory, item);
                                job.count = item.stackCount;
                                pawn.jobs.TryTakeOrderedJob(job);
                            }, MenuOptionPriority.High, null, null, 0f, null, null));
                        }
                        opts.Add(new FloatMenuOption("PickUpSome".Translate(new object[]
                        {
                    item.Label
                        }), delegate
                        {
                            int to = Mathf.Min(MassUtility.CountToPickUpUntilOverEncumbered(pawn, item), item.stackCount);
                            Dialog_Slider window = new Dialog_Slider("PickUpCount".Translate(new object[]
                            {
                        item.LabelShort
                            }), 1, to, delegate (int count)
                            {
                                item.SetForbidden(false, false);
                                Job job = new Job(JobDefOf.TakeInventory, item);
                                job.count = count;
                                pawn.jobs.TryTakeOrderedJob(job);
                            }, -2147483648);
                            Find.WindowStack.Add(window);
                        }, MenuOptionPriority.High, null, null, 0f, null, null));
                    }
                }
            }
            if (!pawn.Map.IsPlayerHome)
            {
                Thing item = c.GetFirstItem(pawn.Map);
                if (item != null && item.def.EverHaulable)
                {
                    Pawn bestPackAnimal = GiveToPackAnimalUtility.PackAnimalWithTheMostFreeSpace(pawn.Map, pawn.Faction);
                    if (bestPackAnimal != null)
                    {
                        if (!pawn.CanReach(item, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
                        {
                            opts.Add(new FloatMenuOption("CannotGiveToPackAnimal".Translate(new object[]
                            {
                        item.Label
                            }) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
                        }
                        else if (!pawn.CanReserve(item, 1))
                        {
                            Pawn pawn4 = pawn.Map.reservationManager.FirstReserverOf(item, pawn.Faction, true);
                            opts.Add(new FloatMenuOption("CannotGiveToPackAnimal".Translate(new object[]
                            {
                        item.Label
                            }) + " (" + "ReservedBy".Translate(new object[]
                            {
                        pawn4.LabelShort
                            }) + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
                        }
                        else if (MassUtility.WillBeOverEncumberedAfterPickingUp(bestPackAnimal, item, 1))
                        {
                            opts.Add(new FloatMenuOption("CannotGiveToPackAnimal".Translate(new object[]
                            {
                        item.Label
                            }) + " (" + "TooHeavy".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
                        }
                        else if (item.stackCount == 1)
                        {
                            opts.Add(new FloatMenuOption("GiveToPackAnimal".Translate(new object[]
                            {
                        item.Label
                            }), delegate
                            {
                                item.SetForbidden(false, false);
                                Job job = new Job(JobDefOf.GiveToPackAnimal, item);
                                job.count = 1;
                                pawn.jobs.TryTakeOrderedJob(job);
                            }, MenuOptionPriority.High, null, null, 0f, null, null));
                        }
                        else
                        {
                            if (MassUtility.WillBeOverEncumberedAfterPickingUp(bestPackAnimal, item, item.stackCount))
                            {
                                opts.Add(new FloatMenuOption("CannotGiveToPackAnimalAll".Translate(new object[]
                                {
                            item.Label
                                }) + " (" + "TooHeavy".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
                            }
                            else
                            {
                                opts.Add(new FloatMenuOption("GiveToPackAnimalAll".Translate(new object[]
                                {
                            item.Label
                                }), delegate
                                {
                                    item.SetForbidden(false, false);
                                    Job job = new Job(JobDefOf.GiveToPackAnimal, item);
                                    job.count = item.stackCount;
                                    pawn.jobs.TryTakeOrderedJob(job);
                                }, MenuOptionPriority.High, null, null, 0f, null, null));
                            }
                            opts.Add(new FloatMenuOption("GiveToPackAnimalSome".Translate(new object[]
                            {
                        item.Label
                            }), delegate
                            {
                                int to = Mathf.Min(MassUtility.CountToPickUpUntilOverEncumbered(bestPackAnimal, item), item.stackCount);
                                Dialog_Slider window = new Dialog_Slider("GiveToPackAnimalCount".Translate(new object[]
                                {
                            item.LabelShort
                                }), 1, to, delegate (int count)
                                {
                                    item.SetForbidden(false, false);
                                    Job job = new Job(JobDefOf.GiveToPackAnimal, item);
                                    job.count = count;
                                    pawn.jobs.TryTakeOrderedJob(job);
                                }, -2147483648);
                                Find.WindowStack.Add(window);
                            }, MenuOptionPriority.High, null, null, 0f, null, null));
                        }
                    }
                }
            }
            if (!pawn.Map.IsPlayerHome)
            {
                foreach (LocalTargetInfo current5 in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), true))
                {
                    Pawn p = (Pawn)current5.Thing;
                    if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer)
                    {
                        if (!pawn.CanReach(p, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
                        {
                            opts.Add(new FloatMenuOption("CannotCarryToExit".Translate(new object[]
                            {
                        p.Label
                            }) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
                        }
                        else if (!pawn.CanReserve(p, 1))
                        {
                            Pawn pawn5 = pawn.Map.reservationManager.FirstReserverOf(p, pawn.Faction, true);
                            opts.Add(new FloatMenuOption("CannotCarryToExit".Translate(new object[]
                            {
                        p.Label
                            }) + " (" + "ReservedBy".Translate(new object[]
                            {
                        pawn5.LabelShort
                            }) + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
                        }
                        else
                        {
                            IntVec3 exitSpot;
                            if (!RCellFinder.TryFindBestExitSpot(pawn, out exitSpot, TraverseMode.ByPawn))
                            {
                                opts.Add(new FloatMenuOption("CannotCarryToExit".Translate(new object[]
                                {
                            p.Label
                                }) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
                            }
                            else
                            {
                                opts.Add(new FloatMenuOption("CarryToExit".Translate(new object[]
                                {
                            p.Label
                                }), delegate
                                {
                                    Job job = new Job(JobDefOf.CarryDownedPawnToExit, p, exitSpot);
                                    job.count = 1;
                                    pawn.jobs.TryTakeOrderedJob(job);
                                }, MenuOptionPriority.High, null, null, 0f, null, null));
                            }
                        }
                    }
                }
            }
            if (pawn.equipment != null && pawn.equipment.Primary != null && GenUI.TargetsAt(clickPos, TargetingParameters.ForSelf(pawn), true).Any<LocalTargetInfo>())
            {
                Action action2 = delegate
                {
                    pawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.DropEquipment, pawn.equipment.Primary));
                };
                opts.Add(new FloatMenuOption("Drop".Translate(new object[]
                {
            pawn.equipment.Primary.Label
                }), action2, MenuOptionPriority.Default, null, null, 0f, null, null));
            }
            foreach (LocalTargetInfo current6 in GenUI.TargetsAt(clickPos, TargetingParameters.ForTrade(), true))
            {
                LocalTargetInfo dest = current6;
                if (!pawn.CanReach(dest, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
                {
                    opts.Add(new FloatMenuOption("CannotTrade".Translate() + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                else if (!pawn.CanReserve(dest.Thing, 1))
                {
                    opts.Add(new FloatMenuOption("CannotTrade".Translate() + " (" + "Reserved".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                else
                {
                    Pawn pTarg = (Pawn)dest.Thing;
                    Action action3 = delegate
                    {
                        Job job = new Job(JobDefOf.TradeWithPawn, pTarg);
                        job.playerForced = true;
                        pawn.jobs.TryTakeOrderedJob(job);
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.InteractingWithTraders, KnowledgeAmount.Total);
                    };
                    string str = string.Empty;
                    if (pTarg.Faction != null)
                    {
                        str = " (" + pTarg.Faction.Name + ")";
                    }
                    Thing thing = dest.Thing;
                    opts.Add(new FloatMenuOption("TradeWith".Translate(new object[]
                    {
                pTarg.LabelShort + ", " + pTarg.TraderKind.label
                    }) + str, action3, MenuOptionPriority.InitiateSocial, null, thing, 0f, null, null));
                }
            }
            foreach (Thing current7 in pawn.Map.thingGrid.ThingsAt(c))
            {
                foreach (FloatMenuOption current8 in current7.GetFloatMenuOptions(pawn))
                {
                    opts.Add(current8);
                }
            }
        }
    }      
}
