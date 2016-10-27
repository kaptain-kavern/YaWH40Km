using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Corruption
{
    public static class SermonUtility
    {
        private const float SermonAreaIfNotInside = 15f;

        private const int MaxRoomCellsCountToUseWholeRoom = 324;
               
        public static bool TryFindRandomCellInSermonArea(BuildingAltar altar, Pawn pawn, out IntVec3 result)
        {
            if (pawn.mindState.duty.focus.Cell == null) Log.Message("NoFocusCell");
            IntVec3 cell;
            Building chair;

            WatchBuildingUtility.TryFindBestWatchCell(altar, pawn, true, out cell, out chair);


            if (chair != null)
            {
                result = chair.Position;
                return true;
            }

                result = cell;
            if (cell == null)
            {
                return false;
            }
                return true;       
        }

        public static List<Building> FreeChairsInRoom(Room room)
        {
            List<Building> chairs = new List<Building>();

            foreach (Building t in room.AllContainedThings)
            {
                if (t.def.building.isSittable)
                {
                    chairs.Add(t);
                }
            }
            return chairs;
        }

        public static bool IsInside(IntVec3 sermonSpot)
        {
            Room room = sermonSpot.GetRoom();
            return room != null && !room.PsychologicallyOutdoors && room.CellCount <= 400;
        }

        public static ThoughtDef GetSermonThoughts(Pawn preacher, Pawn listener)
        {
            Need_Soul s1 = preacher.needs.TryGetNeed<Need_Soul>();
            Need_Soul s2 = listener.needs.TryGetNeed<Need_Soul>();

            bool flag1 = s1.NoPatron;
            bool flag2 = s2.NoPatron;

            if (flag1)
            {

                if (flag2)
                {
                    if (s2.DevotionTrait.SDegree == -2)
                    {
                        if (listener.IsPrisonerOfColony)
                        {
                            return SermonThoughtDefOf.AttendedSermonPureAtheistForced;
                        }

                        return SermonThoughtDefOf.AttendedSermonPureAtheist;
                    }
                    else if (s1.DevotionTrait.SDegree == -1)
                    {
                        if (SermonUtility.movingSermon(preacher))
                        {
                            return SermonThoughtDefOf.AttendedSermonPureMoving;
                        }
                        return SermonThoughtDefOf.AttendedSermonPureAgnostic;
                    }
                    else
                    {
                        if (movingSermon(preacher))
                        {
                            return SermonThoughtDefOf.AttendedSermonPureMoving;
                        }
                        return SermonThoughtDefOf.AttendedSermonPureNice;
                    }
                }
                else
                {
                    if (listener.IsPrisonerOfColony)
                    {
                        return SermonThoughtDefOf.AttendedSermonDarkPureForced;
                    }
                    s2.OpposingDevotees.Add(preacher);
                    return SermonThoughtDefOf.AttendedSermonDarkPure;
                }
            }
            else
            {
                if (flag2)
                {
                    if (listener.IsPrisonerOfColony)
                    {
                        return SermonThoughtDefOf.AttendedSermonPureHereticalForced;
                    }
                    if (movingSermon(preacher))
                    {
                        s2.OpposingDevotees.Add(preacher);
                        return SermonThoughtDefOf.AttendedSermonPureHeretical;
                    }
                    s2.OpposingDevotees.Add(preacher);
                    return SermonThoughtDefOf.AttendedSermonPureUnholy;
                }
                else
                {
                    if (movingSermon(preacher))
                    {
                        return SermonThoughtDefOf.AttendedSermonDarkGlorious;
                    }
                    return SermonThoughtDefOf.AttendedSermonDarkGood;
                }
            }
        }

        private static bool movingSermon(Pawn pr)
        {
            var f = pr.skills.GetSkill(SkillDefOf.Social).level;
            int x = Rand.RangeInclusive(0, 35);
            if ((x + f * 2) > 40)
            {
                return true;
            }
            return false;
        }

        public static void AttendSermonTickCheckEnd(Pawn pawn, Pawn preacher)
        {
            var soul = pawn.needs.TryGetNeed<Need_Soul>();
            if (soul == null)
            {
                return;
            }

            float num = 0f;
            if (movingSermon(preacher))
            {
                num += 0.01f;
            }
            else
            {
                num += 0.005f;
            }

            soul.GainNeed(num);
          
            pawn.needs.mood.thoughts.memories.TryGainMemoryThought(SermonUtility.GetSermonThoughts(preacher, pawn));
        }

        public static void HoldSermonTickCheckEnd(Pawn preacher, BuildingAltar altar)
        {
            var soul = preacher.needs.TryGetNeed<Need_Soul>();
            if (soul == null)
            {
                return;
            }

            float num = 0f;

            foreach (Pawn l in Find.MapPawns.AllPawnsSpawned.FindAll(x => x.Position.InHorDistOf(preacher.Position, 20f) == true))
            {
                num += 0.002f;
            }

            if (movingSermon(preacher))
            {
                num += 0.01f;
            }

            soul.GainNeed(num);
        }

        public static bool ShouldAttendSermon(Pawn p, Pawn preacher)
        {
            int num = 0;
            Need_Soul soul = p.needs.TryGetNeed<Need_Soul>();

            switch(soul.DevotionTrait.SDegree)
            {
                case -2:
                    {
                        num = 0;
                        break;
                    }
                case -1:
                    {
                        num = 5;
                        break;
                    }
                case 0:
                    {
                        num = 10;
                        break;
                    }
                case 1:
                    {
                        num = 15;
                        break;
                    }
                case 2:
                    {
                        num = 20;
                        break;
                    }
            }

            if (p.CurJob.playerForced)
            {
                num = 0;
                if(soul.DevotionTrait.SDegree == 2)
                {
                    num = 10;
                }
            }
            
            if(p.CurJob.def == CorruptionDefOfs.AttendSermon)
            {
                num = 0;
            }

            if(!SermonUtility.IsBestPreacher(p, preacher))
            {
                num = 0;
            }
            if((Rand.RangeInclusive(0, 15) + num) >= 20)
            {
                return true;
            }

            return false;
        }

        public static void GiveAttendSermonJob(BuildingAltar altar, Pawn attendee)
        {
            if (!SermonUtility.IsPreacher(attendee))
            {
                IntVec3 result;
                Building chair;
                if (!WatchBuildingUtility.TryFindBestWatchCell(altar, attendee, true, out result, out chair))
                {

                    if (!WatchBuildingUtility.TryFindBestWatchCell(altar as Thing, attendee, false, out result, out chair))
                    {
                        return;
                    }
                }
                if (chair != null)
                {
                    Job J = new Job(CorruptionDefOfs.AttendSermon, altar.preacher, altar, chair);
                    attendee.QueueJob(J);
                    attendee.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
                else
                {
                    Job J = new Job(CorruptionDefOfs.AttendSermon, altar.preacher, altar, result);
                    attendee.QueueJob(J);
                    attendee.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
            }
        }

        public static void ForceSermon(BuildingAltar altar)
        {
            IntVec3 b = altar.def.interactionCellOffset.RotatedBy(altar.Rotation) + altar.Position;
            Job job = new Job(CorruptionDefOfs.HoldSermon, altar, b);
            altar.preacher.QueueJob(job);
            altar.preacher.jobs.EndCurrentJob(JobCondition.InterruptForced);
            BuildingAltar.GetSermonFlock(altar);
        }

        public static bool IsPreacher(Pawn p)
        {
            List<Thing> list = Find.ListerThings.AllThings.FindAll(s => s.GetType() == typeof(BuildingAltar));
            foreach (BuildingAltar b in list)
            {
                if (b.preacher == p) return true;
            }
            return false;
        }

        public static bool GetBestPreacher(Pawn p, out Pawn bestPreacher, out BuildingAltar altar)
        {
            List<Pawn> opposingDevotees = p.needs.TryGetNeed<Need_Soul>().OpposingDevotees;
            if (opposingDevotees == null) opposingDevotees = new List<Pawn>();
            List<Pawn> availablePreachers = Find.MapPawns.FreeColonistsSpawned.ToList<Pawn>().FindAll(s => s.CurJob.def == CorruptionDefOfs.HoldSermon);

            //Select best preacher of colony

            bestPreacher = availablePreachers.Aggregate((i1, i2) => i1.skills.GetSkill(SkillDefOf.Social).level > i2.skills.GetSkill(SkillDefOf.Social).level ? i1 : i2);
            altar = SermonUtility.chosenAltar(bestPreacher);
            //Check if pawn has listened to this preacher before and if he is of an opposing faith. If so, another preacher will be chosen

            while (opposingDevotees.Contains(bestPreacher))
            {
                if (availablePreachers.Count > 1)
                {
                    availablePreachers.Remove(bestPreacher);
                    bestPreacher = availablePreachers.Aggregate((i1, i2) => i1.skills.GetSkill(SkillDefOf.Social).level > i2.skills.GetSkill(SkillDefOf.Social).level ? i1 : i2);
                    altar = chosenAltar(bestPreacher);
                }
                else
                {
                    bestPreacher = null;
                    altar = null;
                }
                
            }
            if (bestPreacher != null && altar != null)
            {
                return true;
            }
            return false;
        }

        public static bool IsBestPreacher(Pawn p, Pawn preacher)
        {
            List<Pawn> opposingDevotees = p.needs.TryGetNeed<Need_Soul>().OpposingDevotees;
            if (opposingDevotees == null) opposingDevotees = new List<Pawn>();
            List<Pawn> availablePreachers = Find.MapPawns.FreeColonistsSpawned.ToList<Pawn>().FindAll(s => s.CurJob.def == CorruptionDefOfs.HoldSermon);
            Pawn bestcurrentPreacher;

            bestcurrentPreacher = availablePreachers.Aggregate((i1, i2) => i1.skills.GetSkill(SkillDefOf.Social).level > i2.skills.GetSkill(SkillDefOf.Social).level ? i1 : i2);

            if (bestcurrentPreacher == preacher && !opposingDevotees.Contains(preacher))
            {
                return true;
            }
            return false;
        }

        public static BuildingAltar chosenAltar(Pawn preacher)
        {
            return SermonUtility.allAltars.Find(x => x.preacher == preacher);            
        }

        public static List<BuildingAltar> allAltars
        {
            get
            {
                List<BuildingAltar> y = Find.ListerThings.AllThings.FindAll(a => a.GetType() == typeof(BuildingAltar)).Cast<BuildingAltar>().ToList<BuildingAltar>();
                return y;
            }
        }

    }

}
