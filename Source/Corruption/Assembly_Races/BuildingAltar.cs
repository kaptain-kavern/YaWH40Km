using Corruption.DefOfs;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Corruption
{
    public class BuildingAltar : Building
    {
        public bool OptionMorning = false;

        public bool OptionEvening = false;

        private bool HeldSermon;

        public string RoomName;

        public bool CalledInFlock = false;

        public Pawn preacher = null;             

        public bool DoMorningSermon
        {
            get
            {
                return (OptionMorning && (GenLocalDate.HourInt(this.Map) < 6 && GenLocalDate.HourInt(this.Map) > 10));
            }
        }

        public bool DoEveningSermon
        {
            get
            {
                return (OptionEvening && (GenLocalDate.HourInt(this.Map) < 18 && GenLocalDate.HourInt(this.Map) > 22));
            }
        }

        public BuildingAltarDef bdef
        {
            get
            {
                return this.def as BuildingAltarDef;
            }
        }

        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);
            this.preacher = Map.mapPawns.FreeColonistsSpawned.RandomElement<Pawn>();
            RoomName = "Temple";
            TickManager f = Find.TickManager;

            f.RegisterAllTickabilityFor(this);
           
        }
        
        public override void Tick()
        {
            base.Tick();
            if (this.OptionMorning)
            {
                if (Rand.RangeInclusive(6, 10) == GenLocalDate.HourInt(this.Map))
                {
                    if (!HeldSermon)
                    {
                //        Log.Message("starting morning sermon");
                        SermonUtility.ForceSermon(this);
                        this.HeldSermon = true;
                    }
                }
            }

            if (this.OptionEvening)
            {
                if (Rand.RangeInclusive(18, 22) == GenLocalDate.HourInt(this.Map))
                {
                    if (!HeldSermon)
                    {
                        SermonUtility.ForceSermon(this);
                        this.HeldSermon = true;
                    }
                }
            }

            if (this.preacher.CurJob.def == C_JobDefOf.HoldSermon && !this.CalledInFlock)
            {
                GetSermonFlock(this);
                this.CalledInFlock = true;
            }

            if (GenLocalDate.HourInt(this.Map) == 1 || GenLocalDate.HourInt(this.Map) == 12)
            {
                this.HeldSermon = false;
            }
        }

        public static void GetSermonFlock(BuildingAltar altar)
        {
            Room room = altar.GetRoom();

            if (room.Role != RoomRoleDefOf.PrisonBarracks && room.Role != RoomRoleDefOf.PrisonCell)
            {
                List<Pawn> listeners = altar.Map.mapPawns.AllPawnsSpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike);
                bool[] flag = new bool[listeners.Count];
                for (int i = 0; i < listeners.Count; i++)
                {
                    if (!flag[i] && SermonUtility.ShouldAttendSermon(listeners[i], altar.preacher))
                    {
                        SermonUtility.GiveAttendSermonJob(altar, listeners[i]);
                        flag[i] = true;
                    }
                }
            }
            else
            {
                List<Pawn> prisoners = altar.Map.mapPawns.PrisonersOfColonySpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike);
                bool[] flag2 = new bool[prisoners.Count];
                for (int i = 0; i < prisoners.Count; i++)
                {
                    if (!flag2[i] && SermonUtility.ShouldAttendSermon(prisoners[i], altar.preacher))
                    {
                        SermonUtility.GiveAttendSermonJob(altar, prisoners[i]);
                        flag2[i] = true;
                    }
                }
            }

        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_References.LookReference<Pawn>(ref this.preacher, "preacher", false);
            Scribe_Values.LookValue<string>(ref this.RoomName, "RoomName", "Temple", false);
            Scribe_Values.LookValue<bool>(ref this.OptionEvening, "OptionEvening", false, false);
            Scribe_Values.LookValue<bool>(ref this.OptionMorning, "OptionMorning", false, false);
            Scribe_Values.LookValue<bool>(ref this.HeldSermon, "HeldSermon", true, false);
            Scribe_Values.LookValue<bool>(ref this.CalledInFlock, "CalledInFlock", false, false);

        }        
    }
}
