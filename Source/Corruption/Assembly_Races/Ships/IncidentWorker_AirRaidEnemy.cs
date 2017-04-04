﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Corruption.Ships
{
    public class IncidentWorker_AirRaidEnemy : IncidentWorker_RaidEnemy
    {
        protected override bool TryResolveRaidFaction(IncidentParms parms)
        {
            if (base.TryResolveRaidFaction(parms))
            {
                if (parms.faction != null)
                {
                    if (DropShipUtility.FactionHasDropShips(parms.faction))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override bool TryExecute(IncidentParms parms)
        {
            {
                Map map = (Map)parms.target;
                this.ResolveRaidPoints(parms);
                if (!this.TryResolveRaidFaction(parms))
                {
                    return false;
                }
                this.ResolveRaidStrategy(parms);
                this.ResolveRaidArriveMode(parms);
                this.ResolveRaidSpawnCenter(parms);
                IncidentParmsUtility.AdjustPointsForGroupArrivalParams(parms);
                PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(parms);
                List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(PawnGroupKindDefOf.Normal, defaultPawnGroupMakerParms, true).ToList<Pawn>();
                if (list.Count == 0)
                {
                    Log.Error("Got no pawns spawning raid from parms " + parms);
                    return false;
                }
                TargetInfo target = TargetInfo.Invalid;
                if (parms.raidArrivalMode == PawnsArriveMode.CenterDrop || parms.raidArrivalMode == PawnsArriveMode.EdgeDrop)
                {
                    DropPodUtility.DropThingsNear(parms.spawnCenter, map, list.Cast<Thing>(), parms.raidPodOpenDelay, false, true, true);
                    target = new TargetInfo(parms.spawnCenter, map, false);
                }
                else
                {
                    foreach (Pawn current in list)
                    {
                        IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 8);
                        GenSpawn.Spawn(current, loc, map);
                        target = current;
                    }
                }
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("Points = " + parms.points.ToString("F0"));
                foreach (Pawn current2 in list)
                {
                    string str = (current2.equipment == null || current2.equipment.Primary == null) ? "unarmed" : current2.equipment.Primary.LabelCap;
                    stringBuilder.AppendLine(current2.KindLabel + " - " + str);
                }
                string letterLabel = this.GetLetterLabel(parms);
                string letterText = this.GetLetterText(parms, list);
                PawnRelationUtility.Notify_PawnsSeenByPlayer(list, ref letterLabel, ref letterText, this.GetRelatedPawnsInfoLetterText(parms), true);
                Find.LetterStack.ReceiveLetter(letterLabel, letterText, this.GetLetterType(), target, stringBuilder.ToString());
                if (this.GetLetterType() == LetterType.BadUrgent)
                {
                    TaleRecorder.RecordTale(TaleDefOf.RaidArrived, new object[0]);
                }
                Lord lord = LordMaker.MakeNewLord(parms.faction, parms.raidStrategy.Worker.MakeLordJob(parms, map), map, list);
                AvoidGridMaker.RegenerateAvoidGridsFor(parms.faction, map);
                LessonAutoActivator.TeachOpportunity(ConceptDefOf.EquippingWeapons, OpportunityType.Critical);
                if (!PlayerKnowledgeDatabase.IsComplete(ConceptDefOf.PersonalShields))
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        Pawn pawn = list[i];
                        if (pawn.apparel.WornApparel.Any((Apparel ap) => ap is PersonalShield))
                        {
                            LessonAutoActivator.TeachOpportunity(ConceptDefOf.PersonalShields, OpportunityType.Critical);
                            break;
                        }
                    }
                }
                if (DebugViewSettings.drawStealDebug && parms.faction.HostileTo(Faction.OfPlayer))
                {
                    Log.Message(string.Concat(new object[]
                    {
            "Market value threshold to start stealing: ",
            StealAIUtility.StartStealingMarketValueThreshold(lord),
            " (colony wealth = ",
            map.wealthWatcher.WealthTotal,
            ")"
                    }));
                }
            }
            Find.TickManager.slower.SignalForceNormalSpeedShort();
            Find.StoryWatcher.statsRecord.numRaidsEnemy++;
            return true;
        }
    }
}
