using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.AI.Group;

namespace Corruption
{
    public class CorruptionStoryTracker : Thing
    {
        public static List<PawnKindDef> DemonPawnKinds = new List<PawnKindDef>();

        public  Faction ChaosCult;
        public  Faction DarkEldarKabal;
        public  Faction EldarWarhost;
        public  Faction ImperialGuard;
        public  Faction Orks;

        public  Faction AdeptusSororitas;

        public bool FactionCanHelp;
        public int DaysAfterHelp;

        public static float ColonyCorruptionAvg;


        public override void TickRare()
        {
            base.TickRare();
            if (GenDate.HourOfDay == 4)
            {
                EldarTicksDaily();
                SororitasTickDaily();
            }
        }

        public override void PostMapInit()
        {
            base.PostMapInit();
            this.ChaosCult = Find.FactionManager.FirstFactionOfDef(CorruptionDefOfs.ChaosCult);
            this.DarkEldarKabal = Find.FactionManager.FirstFactionOfDef(CorruptionDefOfs.DarkEldarKabal);
            this.EldarWarhost = Find.FactionManager.FirstFactionOfDef(CorruptionDefOfs.EldarWarhost);
            this.ImperialGuard = Find.FactionManager.FirstFactionOfDef(CorruptionDefOfs.ImperialGuard);
            this.Orks = Find.FactionManager.FirstFactionOfDef(CorruptionDefOfs.Orks);
            this.AdeptusSororitas = Find.FactionManager.FirstFactionOfDef(CorruptionDefOfs.AdeptusSororitas);
        }

        public void CalculateColonyCorruption()
        {
            List<Pawn> ColonyPawns = Find.MapPawns.FreeColonistsAndPrisonersSpawned.ToList<Pawn>();
            float totalCorruption = 0f;
            foreach (Pawn cpawn in ColonyPawns)
            {
                if (cpawn.needs != null && cpawn.needs.TryGetNeed<Need_Soul>() != null)
                    totalCorruption += cpawn.needs.TryGetNeed<Need_Soul>().CurLevel;
            }
            ColonyCorruptionAvg = totalCorruption / ColonyPawns.Count;
        }

        public void TickDaily()
        {
            this.DaysAfterHelp += 1;
            if (this.DaysAfterHelp > 7)
            {
                this.FactionCanHelp = true;
            }
        }

        public void EldarTicksDaily()
        {
            if (ColonyCorruptionAvg < 0.4)
            {
                EldarWarhost.SetHostileTo(Faction.OfPlayer, true);
                if (EldarWarhost.def.raidCommonality < 100)
                {
                    EldarWarhost.def.raidCommonality += 10;
                }
            }
            else if (Rand.Range(0, 100) < 2)
            {
                if (EldarWarhost.HostileTo(Faction.OfPlayer))
                {
                    EldarWarhost.SetHostileTo(Faction.OfPlayer, false);
                }
                else
                {
                    EldarWarhost.SetHostileTo(Faction.OfPlayer, true);
                }
            }
            if (CheckForSpiritStones() > 0 && !EldarWarhost.HostileTo(Faction.OfPlayer))
            {
                IncidentParms parms = new IncidentParms();
                parms.faction = EldarWarhost;
                parms.points = 500;
                IncidentDef EldarVisitorIncident = DefDatabase<IncidentDef>.AllDefs.First(x => x.defName == "VisitorGroup");
                EldarVisitorIncident.workerClass = typeof(IncidentWorker_VisitorGroup);
                EldarVisitorIncident.Worker.TryExecute(parms);
            }
        }

        public int CheckForSpiritStones()
        {
            List<Thing> list = Find.ListerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways);
            int num = 0;
            for (int i = 0; i < list.Count; i++)
            {
                Thing thing = list[i];
                if (!thing.Position.Fogged() && thing.def == CorruptionDefOfs.SpiritStone)
                {
                    num += 1;
                }
            }
            return num;
        }

        public void SororitasTickDaily()
        {
            if (ColonyCorruptionAvg < 0.4)
            {
                EldarWarhost.SetHostileTo(Faction.OfPlayer, true);
                if (EldarWarhost.def.raidCommonality < 100)
                {
                    EldarWarhost.def.raidCommonality += 10;
                }
            }
        }
        public void SororitasTick()
        {
            if (!AdeptusSororitas.HostileTo(Faction.OfPlayer) && FactionCanHelp && Rand.Range(0, 100) < 10) 
            {
                foreach (Lord current in Find.LordManager.lords)
                {
                    if (current.CurLordToil is LordToil_AssaultColony && current.faction != AdeptusSororitas && current.faction != ImperialGuard)
                    {
                        IncidentParms parms = new IncidentParms();
                        parms.faction = AdeptusSororitas;
                        parms.points = 1000;
                        parms.raidArrivalMode = PawnsArriveMode.CenterDrop;
                        parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
                        IncidentDef relief = new IncidentDef();
                        relief.workerClass = typeof(IncidentWorker_RaidFriendly);
                        relief.Worker.TryExecute(parms);
                        this.FactionCanHelp = false; 
                        break;
                    }
                }
            }
        }
    }
}
