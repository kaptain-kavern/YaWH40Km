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
            IntVec3 c2 = IntVec3.FromVector3(clickPos);
            foreach (Thing current in c2.GetThingList())
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
                    FloatMenuOption item;
                    if (t.def.IsPleasureDrug && pawn.story != null && pawn.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire) < 0)
                    {
                        item = new FloatMenuOption(text + " (" + TraitDefOf.DrugDesire.DataAtDegree(-1).label + ")", null, MenuOptionPriority.Medium, null, null, 0f, null);
                    }
                    else if (!pawn.CanReach(t, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
                    {
                        item = new FloatMenuOption(text + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Medium, null, null, 0f, null);
                    }
                    else if (!pawn.CanReserve(t, 1))
                    {
                        item = new FloatMenuOption(text + " (" + "ReservedBy".Translate(new object[]
                        {
                    Find.Reservations.FirstReserverOf(t, pawn.Faction, true).LabelShort
                        }) + ")", null, MenuOptionPriority.Medium, null, null, 0f, null);
                    }
                    else
                    {
                        item = new FloatMenuOption(text, delegate
                        {
                            t.SetForbidden(false, true);
                            Job job = new Job(JobDefOf.Ingest, t);
                            job.maxNumToCarry = FoodUtility.WillIngestStackCountOf(pawn, t.def);
                            pawn.drafter.TakeOrderedJob(job);
                        }, MenuOptionPriority.Medium, null, null, 0f, null);
                    }
                    opts.Add(item);
                }
            }
            if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                foreach (TargetInfo current2 in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), true))
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
                                job.maxNumToCarry = 1;
                                job.playerForced = true;
                                pawn.drafter.TakeOrderedJob(job);
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Rescuing, KnowledgeAmount.Total);
                            }, MenuOptionPriority.Medium, null, victim2, 0f, null));
                        }
                        if (victim.MentalStateDef != null || (victim.RaceProps.Humanlike && victim.Faction != Faction.OfPlayer))
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
                                job.maxNumToCarry = 1;
                                job.playerForced = true;
                                pawn.drafter.TakeOrderedJob(job);
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Capturing, KnowledgeAmount.Total);
                            }, MenuOptionPriority.Medium, null, victim2, 0f, null));
                        }
                    }
                }
                foreach (TargetInfo current3 in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), true))
                {
                    TargetInfo targetInfo = current3;
                    Pawn victim = (Pawn)targetInfo.Thing;
                    if (victim.Downed && pawn.CanReserveAndReach(victim, PathEndMode.OnCell, Danger.Deadly, 1) && Building_CryptosleepCasket.FindCryptosleepCasketFor(victim, pawn) != null)
                    {
                        string label = "CarryToCryptosleepCasket".Translate(new object[]
                        {
                    targetInfo.Thing.LabelCap
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
                            job.maxNumToCarry = 1;
                            job.playerForced = true;
                            pawn.drafter.TakeOrderedJob(job);
                        };
                        Pawn victim2 = victim;
                        opts.Add(new FloatMenuOption(label, action, MenuOptionPriority.Medium, null, victim2, 0f, null));
                    }
                }
            }
            foreach (TargetInfo current4 in GenUI.TargetsAt(clickPos, TargetingParameters.ForStrip(pawn), true))
            {
                TargetInfo stripTarg = current4;
                FloatMenuOption item2;
                if (!pawn.CanReach(stripTarg, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
                {
                    item2 = new FloatMenuOption("CannotStrip".Translate(new object[]
                    {
                stripTarg.Thing.LabelCap
                    }) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Medium, null, null, 0f, null);
                }
                else if (!pawn.CanReserveAndReach(stripTarg, PathEndMode.ClosestTouch, Danger.Deadly, 1))
                {
                    item2 = new FloatMenuOption("CannotStrip".Translate(new object[]
                    {
                stripTarg.Thing.LabelCap
                    }) + " (" + "ReservedBy".Translate(new object[]
                    {
                Find.Reservations.FirstReserverOf(stripTarg, pawn.Faction, true).LabelShort
                    }) + ")", null, MenuOptionPriority.Medium, null, null, 0f, null);
                }
                else
                {
                    item2 = new FloatMenuOption("Strip".Translate(new object[]
                    {
                stripTarg.Thing.LabelCap
                    }), delegate
                    {
                        stripTarg.Thing.SetForbidden(false, false);
                        Job job = new Job(JobDefOf.Strip, stripTarg);
                        job.playerForced = true;
                        pawn.drafter.TakeOrderedJob(job);
                    }, MenuOptionPriority.Medium, null, null, 0f, null);
                }
                opts.Add(item2);
            }
            if (pawn.equipment != null)
            {
                ThingWithComps equipment = null;
                List<Thing> thingList = c2.GetThingList();
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
                    string label2 = equipment.Label;
                    FloatMenuOption item3;
                    if (!pawn.CanReach(equipment, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
                    {
                        item3 = new FloatMenuOption("CannotEquip".Translate(new object[]
                        {
                    label2
                        }) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Medium, null, null, 0f, null);
                    }
                    else if (!pawn.CanReserve(equipment, 1))
                    {
                        item3 = new FloatMenuOption("CannotEquip".Translate(new object[]
                        {
                    label2
                        }) + " (" + "ReservedBy".Translate(new object[]
                        {
                    Find.Reservations.FirstReserverOf(equipment, pawn.Faction, true).LabelShort
                        }) + ")", null, MenuOptionPriority.Medium, null, null, 0f, null);
                    }
                    else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                    {
                        item3 = new FloatMenuOption("CannotEquip".Translate(new object[]
                        {
                    label2
                        }) + " (" + "Incapable".Translate() + ")", null, MenuOptionPriority.Medium, null, null, 0f, null);
                    }
                    else
                    {
                        string text2 = "Equip".Translate(new object[]
                        {
                    label2
                        });
                        if (equipment.def.IsRangedWeapon && pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.Brawler))
                        {
                            text2 = text2 + " " + "EquipWarningBrawler".Translate();
                        }
                        item3 = new FloatMenuOption(text2, delegate
                        {
                            equipment.SetForbidden(false, true);
                            Job job = new Job(JobDefOf.Equip, equipment);
                            job.playerForced = true;
                            pawn.drafter.TakeOrderedJob(job);
                            MoteMaker.MakeStaticMote(equipment.DrawPos, ThingDefOf.Mote_FeedbackEquip, 1f);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.EquippingWeapons, KnowledgeAmount.Total);
                        }, MenuOptionPriority.Medium, null, null, 0f, null);
                    }
                    opts.Add(item3);
                }
            }
            if (pawn.apparel != null)
            {
                Apparel apparel = Find.ThingGrid.ThingAt<Apparel>(c2);
                if (apparel != null)
                {
                    FloatMenuOption item4;
                    if (!pawn.CanReach(apparel, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
                    {
                        item4 = new FloatMenuOption("CannotWear".Translate(new object[]
                        {
                    apparel.Label
                        }) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Medium, null, null, 0f, null);
                    }
                    else if (!pawn.CanReserve(apparel, 1))
                    {
                        Pawn pawn2 = Find.Reservations.FirstReserverOf(apparel, pawn.Faction, true);
                        item4 = new FloatMenuOption("CannotWear".Translate(new object[]
                        {
                    apparel.Label
                        }) + " (" + "ReservedBy".Translate(new object[]
                        {
                    pawn2.LabelShort
                        }) + ")", null, MenuOptionPriority.Medium, null, null, 0f, null);
                    }
                    else if (!ApparelUtility.HasPartsToWear(pawn, apparel.def))
                    {
                        item4 = new FloatMenuOption("CannotWear".Translate(new object[]
                        {
                    apparel.Label
                        }) + " (" + "CannotWearBecauseOfMissingBodyParts".Translate() + ")", null, MenuOptionPriority.Medium, null, null, 0f, null);
                    }
                    else if (RaceRestricted(pawn, apparel))
                    {

                        item4 = new FloatMenuOption("CannotWear".Translate(new object[]
                        {
                    apparel.Label
                        }) + " (" + "CannotWearBecauseOfWrongRace".Translate() + ")", null, MenuOptionPriority.Medium, null, null, 0f, null);
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
                            job.playerForced = true;
                            pawn.drafter.TakeOrderedJob(job);
                        }, MenuOptionPriority.Medium, null, null, 0f, null);
                    }
                    opts.Add(item4);
                }
            }
            if (pawn.equipment != null && pawn.equipment.Primary != null)
            {
                Thing thing = Find.ThingGrid.ThingAt(c2, ThingDefOf.EquipmentRack);
                if (thing != null)
                {
                    if (!pawn.CanReach(thing, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
                    {
                        opts.Add(new FloatMenuOption("CannotDeposit".Translate(new object[]
                        {
                    pawn.equipment.Primary.LabelCap,
                    thing.def.label
                        }) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Medium, null, null, 0f, null));
                    }
                    else
                    {
                        foreach (IntVec3 c in GenAdj.CellsOccupiedBy(thing))
                        {
                            if (c.GetStorable() == null && pawn.CanReserveAndReach(c, PathEndMode.ClosestTouch, Danger.Deadly, 1))
                            {
                                Action action2 = delegate
                                {
                                    ThingWithComps t;
                                    if (pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out t, pawn.Position, true))
                                    {
                                        t.SetForbidden(false, true);
                                        Job job = new Job(JobDefOf.HaulToCell, t, c);
                                        job.haulMode = HaulMode.ToCellStorage;
                                        job.maxNumToCarry = 1;
                                        job.playerForced = true;
                                        pawn.drafter.TakeOrderedJob(job);
                                    }
                                };
                                opts.Add(new FloatMenuOption("Deposit".Translate(new object[]
                                {
                            pawn.equipment.Primary.LabelCap,
                            thing.def.label
                                }), action2, MenuOptionPriority.Medium, null, null, 0f, null));
                                break;
                            }
                        }
                    }
                }
                if (pawn.equipment != null && GenUI.TargetsAt(clickPos, TargetingParameters.ForSelf(pawn), true).Any<TargetInfo>())
                {
                    Action action3 = delegate
                    {
                        ThingWithComps thingWithComps;
                        pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out thingWithComps, pawn.Position, true);
                        pawn.drafter.TakeOrderedJob(new Job(JobDefOf.Wait, 20, false));
                    };
                    opts.Add(new FloatMenuOption("Drop".Translate(new object[]
                    {
                pawn.equipment.Primary.Label
                    }), action3, MenuOptionPriority.Medium, null, null, 0f, null));
                }
            }
            foreach (TargetInfo current5 in GenUI.TargetsAt(clickPos, TargetingParameters.ForTrade(), true))
            {
                TargetInfo dest = current5;
                if (!pawn.CanReach(dest, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
                {
                    opts.Add(new FloatMenuOption("CannotTrade".Translate() + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Medium, null, null, 0f, null));
                }
                else if (!pawn.CanReserve(dest.Thing, 1))
                {
                    opts.Add(new FloatMenuOption("CannotTrade".Translate() + " (" + "Reserved".Translate() + ")", null, MenuOptionPriority.Medium, null, null, 0f, null));
                }
                else
                {
                    Pawn pTarg = (Pawn)dest.Thing;
                    Action action4 = delegate
                    {
                        Job job = new Job(JobDefOf.TradeWithPawn, pTarg);
                        job.playerForced = true;
                        pawn.drafter.TakeOrderedJob(job);
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.InteractingWithTraders, KnowledgeAmount.Total);
                    };
                    string str = string.Empty;
                    if (pTarg.Faction != null)
                    {
                        str = " (" + pTarg.Faction.Name + ")";
                    }
                    Thing thing2 = dest.Thing;
                    opts.Add(new FloatMenuOption("TradeWith".Translate(new object[]
                    {
                pTarg.LabelShort + ", " + pTarg.TraderKind.label
                    }) + str, action4, MenuOptionPriority.Medium, null, thing2, 0f, null));
                }
            }
            foreach (Thing current6 in Find.ThingGrid.ThingsAt(c2))
            {
                foreach (FloatMenuOption current7 in current6.GetFloatMenuOptions(pawn))
                {
                    opts.Add(current7);
                }
            }
        }

    }
}
