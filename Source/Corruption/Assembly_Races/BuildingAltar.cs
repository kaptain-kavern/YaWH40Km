using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Corruption
{
    public class BuildingAltar : Building
    {
        public bool OptionMorning = true;

        public bool OptionEvening = true;

        private bool HeldSermon;

        public string RoomName;

        public Pawn preacher;             

        public bool DoMorningSermon
        {
            get
            {
                return (OptionMorning && (GenDate.HourInt < 6 && GenDate.HourInt > 10));
            }
        }

        public bool DoEveningSermon
        {
            get
            {
                return (OptionEvening && (GenDate.HourInt < 18 && GenDate.HourInt > 22));
            }
        }

        public BuildingAltarDef bdef
        {
            get
            {
                return this.def as BuildingAltarDef;
            }
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            this.preacher = Find.MapPawns.FreeColonistsSpawned.RandomElement<Pawn>();
            RoomName = "Temple";
            TickManager f = Find.TickManager;

            f.RegisterAllTickabilityFor(this);
           
        }
        
        public override void Tick()
        {
            base.Tick();
            if (this.OptionMorning)
            {
                if (Rand.RangeInclusive(6, 10) == GenDate.HourInt)
                {
                    if (!HeldSermon)
                    {
                        SermonUtility.ForceSermon(this);
                        this.HeldSermon = true;
                    }
                }
            }

            if (this.OptionMorning)
            {
                if (Rand.RangeInclusive(18, 22) == GenDate.HourInt)
                {
                    if (!HeldSermon)
                    {
                        SermonUtility.ForceSermon(this);
                        this.HeldSermon = true;
                    }
                }
            }

            if (this.preacher.CurJob.def == CorruptionDefOfs.HoldSermon)
            {
                GetSermonFlock(this);
            }

            if (GenDate.HourInt == 1 || GenDate.HourInt == 12)
            {
                this.HeldSermon = false;
            }
        }

        public static void GetSermonFlock(BuildingAltar altar)
        {
            Room room = altar.GetRoom();

            if (room.Role != RoomRoleDefOf.PrisonBarracks && room.Role != RoomRoleDefOf.PrisonCell)
            {
                List<Pawn> listeners = Find.MapPawns.AllPawnsSpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike);
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
                List<Pawn> prisoners = Find.MapPawns.PrisonersOfColonySpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike);
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
            Scribe_Deep.LookDeep<Pawn>(ref this.preacher, "preacher", new object[0]);
            Scribe_Values.LookValue<string>(ref this.RoomName, "RoomName", "Temple", false);
            Scribe_Values.LookValue<bool>(ref this.OptionEvening, "OptionEvening", true, false);
            Scribe_Values.LookValue<bool>(ref this.OptionMorning, "OptionMorning", true, false);
        }
    }
}
