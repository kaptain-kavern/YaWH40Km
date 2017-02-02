using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.AI.Group;
using RimWorld.Planet;
using UnityEngine;

namespace Corruption
{
    public class CorruptionStoryTracker : WorldObject
    {

        public static List<PawnKindDef> DemonPawnKinds = new List<PawnKindDef>();

        public Faction PatronFaction;

        public string SubsectorName;

        public bool activeRaid;
        public bool PlayerIsEnemyOfMankind;

        public Pawn Astropath;

        public Faction ChaosCult;
        public Faction DarkEldarKabal;
        public Faction EldarWarhost;
        public Faction ImperialGuard;
        public Faction Orks;
        public Faction Mechanicus;
        public Faction Tau;
        public Faction AdeptusSororitas;

        public List<Faction> ImperialFactions = new List<Faction>();
        public List<Faction> XenoFactions = new List<Faction>();

        public List<StarMapObject> SubSectorObjects = new List<StarMapObject>();

        public bool FactionCanHelp;
        public int DaysAfterHelp;

        public float ColonyCorruptionAvg;

        public override void Tick()
        {
            for (int i = 0; i < Find.Maps.Count; i++)

            {
                if (Find.Maps[i].lordManager.lords.Any(x => x.LordJob.GetType() == typeof(LordJob_AssaultColony)))
                {
                    if (activeRaid == false)
                    {
                        activeRaid = true;
                        if (!PlayerIsEnemyOfMankind && this.PatronFaction != null)
                        {
                            this.PatronFactionAssaultTick(this.PatronFaction);
                        }
                    }
                }
            }

            if (GenLocalDate.HourOfDay(Find.VisibleMap) == 4)
            {
                CalculateColonyCorruption();
                EldarTicksDaily();
                CorruptionTicksDaily();
            }
        }

        public override void PostAdd()
        {
            this.ChaosCult = Find.FactionManager.FirstFactionOfDef(CorruptionDefOfs.ChaosCult);
            this.DarkEldarKabal = Find.FactionManager.FirstFactionOfDef(CorruptionDefOfs.DarkEldarKabal);
            this.EldarWarhost = Find.FactionManager.FirstFactionOfDef(CorruptionDefOfs.EldarWarhost);
            this.ImperialGuard = Find.FactionManager.FirstFactionOfDef(CorruptionDefOfs.ImperialGuard);
            this.Orks = Find.FactionManager.FirstFactionOfDef(CorruptionDefOfs.Orks);
            this.AdeptusSororitas = Find.FactionManager.FirstFactionOfDef(CorruptionDefOfs.AdeptusSororitas);
            this.Mechanicus = Find.FactionManager.FirstFactionOfDef(CorruptionDefOfs.Mechanicus);
            this.Tau = Find.FactionManager.FirstFactionOfDef(CorruptionDefOfs.TauVanguard);
            this.ImperialFactions.Add(this.ImperialGuard);
            this.ImperialFactions.Add(this.Mechanicus);
            this.ImperialFactions.Add(this.AdeptusSororitas);
            this.XenoFactions.Add(this.EldarWarhost);
            this.XenoFactions.Add(this.Tau);
            this.XenoFactions.Add(this.ChaosCult);

            List<Faction> list = new List<Faction>();
            list.AddRange(this.ImperialFactions);
            list.AddRange(this.XenoFactions);
            foreach (Faction current in list)
            {
                if (current.leader == null)
                {
                  //  Log.Message("NoLeader for "+ current.GetCallLabel());
                    PawnKindDef kinddef = DefDatabase<PawnKindDef>.AllDefsListForReading.FirstOrDefault(x => x.defaultFactionType == current.def && x.factionLeader);
                    if (kinddef != null)
                    {
                //        Log.Message("Generating Leader with: " + kinddef.defName);
                        PawnGenerationRequest request = new PawnGenerationRequest(kinddef, current, PawnGenerationContext.NonPlayer, null, false, false, false, false, true, false, 1f, false, true, true, null, null, null, null, null, null);

                        Pawn pawn = PawnGenerator.GeneratePawn(request);

                        if (kinddef.defName.Contains("Alien_"))
                        {
                            AlienRace.AlienPawn apawn = pawn as AlienRace.AlienPawn;
                            apawn.SpawnSetupAlien();
                            current.leader = apawn;
                        }
                        else
                        {
                            current.leader = pawn;
                        }
                        if (current.leader.RaceProps.IsFlesh)
                        {
                            current.leader.relations.everSeenByPlayer = true;
                        }
                        if (!Find.WorldPawns.Contains(current.leader))
                        {
                            Find.WorldPawns.PassToWorld(current.leader, PawnDiscardDecideMode.KeepForever);
                        }
                    }
                }
            }
            CreateSubSector();
  //          Log.Message("Objects created : " + this.SubSectorObjects.Count.ToString());
            base.PostAdd();
        }

        public void CreateSubSector()
        {
            Vector2 center = new Vector2(360f, 300f);
            int p = Rand.RangeInclusive(1, 2);
            this.CreateObjects(p, StarMapObjectType.PlanetMedium, center);
            int s = Rand.RangeInclusive(1, 2);
            this.CreateObjects(s, StarMapObjectType.PlanetSmall, center);
            int m = Rand.RangeInclusive(1, 2);
            this.CreateObjects(m, StarMapObjectType.Moon, center);

            List<string> planetName = new List<string>();
            planetName.Add(Find.World.info.name);
            this.SubsectorName = NameGenerator.GenerateName(RulePackDefOf.NamerWorld, planetName, false);
        }
        
        private void CreateObjects(int num, StarMapObjectType type, Vector2 center)
        {
            int angle = 0;
            for (int i=0; i < num; i++)
            {
                List<string> existingObjectNames = new List<string>();
                foreach(StarMapObject current in this.SubSectorObjects)
                {
                    existingObjectNames.Add(current.objectName);
                }
                StarMapObject newEntry = new StarMapObject(angle, out angle, center, existingObjectNames, type);
                angle += 40;
                this.SubSectorObjects.Add(newEntry);
            }
        }

        public override void PostMake()
        {
            this.ChaosCult = Find.FactionManager.FirstFactionOfDef(CorruptionDefOfs.ChaosCult);
            this.DarkEldarKabal = Find.FactionManager.FirstFactionOfDef(CorruptionDefOfs.DarkEldarKabal);
            this.EldarWarhost = Find.FactionManager.FirstFactionOfDef(CorruptionDefOfs.EldarWarhost);
            this.ImperialGuard = Find.FactionManager.FirstFactionOfDef(CorruptionDefOfs.ImperialGuard);
            this.Orks = Find.FactionManager.FirstFactionOfDef(CorruptionDefOfs.Orks);
            this.AdeptusSororitas = Find.FactionManager.FirstFactionOfDef(CorruptionDefOfs.AdeptusSororitas);
            this.Mechanicus = Find.FactionManager.FirstFactionOfDef(CorruptionDefOfs.Mechanicus);
            this.ImperialFactions.Add(this.ImperialGuard);
            this.ImperialFactions.Add(this.Mechanicus);
            this.ImperialFactions.Add(this.AdeptusSororitas);
      //      Log.Message("Imperial Factions: "+this.ImperialFactions.Count.ToString());
        }

        public void CalculateColonyCorruption()
        {
            List<Pawn> ColonyPawns = Find.VisibleMap.mapPawns.FreeColonistsAndPrisonersSpawned.ToList<Pawn>();
            float totalCorruption = 0f;
            foreach (Pawn cpawn in ColonyPawns)
            {
                if (cpawn.needs != null && cpawn.needs.TryGetNeed<Need_Soul>() != null)
                    totalCorruption += cpawn.needs.TryGetNeed<Need_Soul>().CurLevel;
            }
            ColonyCorruptionAvg = totalCorruption / ColonyPawns.Count;
        }

        public void HelpTickReset()
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
            if (CheckForSpiritStones() > 0)
            {
                if (!EldarWarhost.HostileTo(Faction.OfPlayer))
                {
                    IncidentParms parms = new IncidentParms();
                    parms.faction = EldarWarhost;
                    parms.points = 500;
                    IncidentDef EldarVisitorIncident = DefDatabase<IncidentDef>.AllDefs.First(x => x.defName == "VisitorGroup");
                    EldarVisitorIncident.workerClass = typeof(IncidentWorker_VisitorGroup);
                    EldarVisitorIncident.Worker.TryExecute(parms);
                }
                else
                {
                    IncidentParms parms = new IncidentParms();
                    parms.faction = EldarWarhost;
                    parms.points = 1000;
                    IncidentDef EldarRaidIncident = DefDatabase<IncidentDef>.AllDefs.First(x => x.defName == "Assault");
                    EldarRaidIncident.workerClass = typeof(IncidentWorker_VisitorGroup);
                    EldarRaidIncident.Worker.TryExecute(parms);
                }
            }
        }

        public int CheckForSpiritStones()
        {
            List<Thing> list = Find.VisibleMap.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways);
            int num = 0;
            for (int i = 0; i < list.Count; i++)
            {
                Thing thing = list[i];
                if (!thing.Position.Fogged(Find.VisibleMap) && thing.def == CorruptionDefOfs.SpiritStone)
                {
                    num += 1;
                }
            }
            return num;
        }

        public void CorruptionTicksDaily()
        {
            if (ColonyCorruptionAvg < 0.4)
            {
                EldarWarhost.SetHostileTo(Faction.OfPlayer, true);
                if (EldarWarhost.def.raidCommonality < 100)
                {
                    EldarWarhost.def.raidCommonality += 10;
                }
                AdeptusSororitas.SetHostileTo(Faction.OfPlayer, true);
                if (this.PatronFaction == AdeptusSororitas)
                {
                    this.PatronFaction = null;
                }
                if (AdeptusSororitas.def.raidCommonality < 100)
                {
                    AdeptusSororitas.def.raidCommonality += 10;
                }
            }
        }

        public void SororitasAssist()
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
        }

        public void IGAssist()
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
        }

        public void MechanicusAssist()
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
        }


        public void PatronFactionAssaultTick(Faction patronFaction)
        {
            switch(patronFaction.def.defName)
            {
                case ("Mechanicus"):
                    {
                        MechanicusAssist();
                        break;
                    }
                case ("ImperialGuard"):
                    {
                        IGAssist();
                        break;
                    }
                case ("AdeptusSororitas"):
                    {
                        SororitasAssist();
                        break;
                    }
            }
        }

        public override void ExposeData()
        {
            Scribe_References.LookReference<Faction>(ref this.PatronFaction, "PatronFaction");
            Scribe_References.LookReference<Faction>(ref this.ImperialGuard, "ImperialGuard");
            Scribe_References.LookReference<Faction>(ref this.AdeptusSororitas, "AdeptusSororitas");
            Scribe_References.LookReference<Faction>(ref this.Mechanicus, "Mechanicus");
            Scribe_References.LookReference<Faction>(ref this.EldarWarhost, "EldarWarhost");
            Scribe_References.LookReference<Faction>(ref this.DarkEldarKabal, "DarkEldarKabal");
            Scribe_References.LookReference<Faction>(ref this.ChaosCult, "ChaosCult");
            Scribe_References.LookReference<Faction>(ref this.Tau, "Tau");
            Scribe_Collections.LookList<Faction>(ref this.ImperialFactions, "ImperialFactions", LookMode.Reference, new object[0]);
            Scribe_Collections.LookList<Faction>(ref this.XenoFactions, "XenoFactions", LookMode.Reference, new object[0]);
            Scribe_Collections.LookList<StarMapObject>(ref this.SubSectorObjects, "SubSectorObjects", LookMode.Deep, new object[0]);
            Scribe_Values.LookValue<bool>(ref this.FactionCanHelp, "FactionCanHelp", false, true);
            Scribe_Values.LookValue<bool>(ref this.activeRaid, "activeRaid", false, true);
            Scribe_Values.LookValue<bool>(ref this.PlayerIsEnemyOfMankind, "PlayerIsEnemyOfMankind", false, true);
            Scribe_Values.LookValue<int>(ref this.DaysAfterHelp, "DaysAfterHelp", 4, false);
            Scribe_Values.LookValue<float>(ref this.ColonyCorruptionAvg, "ColonyCorruptionAvg", 0.8f, false);
            Scribe_Values.LookValue<string>(ref this.SubsectorName, "SubsectorName", "Aurelia", false);
            base.ExposeData();
        }
    }
}
