using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption
{
    public class JoyGiver_HoldSermon : JoyGiver
    {
        private BuildingAltar assignedAltar;

        public override Job TryGiveJob(Pawn pawn)
        {
            if (PawnUtility.WillSoonHaveBasicNeed(pawn))
            {
                return null;
            }
            if (assignedAltar.Map.dangerWatcher.DangerRating != StoryDanger.None)
            {
                return null;
            }

            Job job = new Job(CorruptionDefOfs.HoldSermon, assignedAltar, assignedAltar.InteractionCell);
            BuildingAltar.GetSermonFlock(assignedAltar);
            return job;
        }

        public override float GetChance(Pawn pawn)
        {
            if (PawnIsPreacher(pawn, SermonUtility.allAltars(pawn), out assignedAltar))
            {
                int devotion = pawn.needs.TryGetNeed<Need_Soul>().DevotionTrait.SDegree;
                if (devotion == -2)
                {
                    return 0f;
                }
                if (devotion == -1)
                {
                    return 1f;
                }
                if (devotion == 0)
                {
                    return 4f;
                }
                if (devotion == 1)
                {
                    return 10f;
                }
                if (devotion == 2)
                {
                    return 20f;
                }
                Log.Message("Getting Chance");

            }
            return 0f;
        }


        public static bool PawnIsPreacher(Pawn p, List<BuildingAltar> alts, out BuildingAltar altar)
        {
            foreach (BuildingAltar b in alts)
            {
                if (b.preacher == p && (b.DoMorningSermon || b.DoEveningSermon))
                {
                    altar = b;            
                    return true;
                }
            }
            altar = null;
            return false;
        }
    }
}
