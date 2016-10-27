using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public static class ChaosGodsUtilities
    {
        public static void DoEffectOn(Pawn target, MentalStateDef mdef)
        {
            if (target.Dead)
            {
                return;
            }
            Log.Message(mdef.ToString());
            target.mindState.mentalStateHandler.TryStartMentalState(mdef, null, false);
        }

        public static MentalStateDef SlaaneshEffects(Pawn pawn, Need_Soul soul)
        {
            int num = Rand.RangeInclusive(1, 9);

            if(num >7)
            {
                return CorruptionDefOfs.LustViolent;
            }
            if (num > 5)
            {
                return MentalStateDefOf.BingingDrugExtreme;
            }
            if (num > 3)
            {
                return MentalStateDefOf.BingingDrugMajor;
            }
            return CorruptionDefOfs.BingingFood;
        }

        public static MentalStateDef KhorneEffects(Pawn pawn, Need_Soul soul)
        {
            int num = Rand.RangeInclusive(1, 9);

            if (num > 6)
            {
                return MentalStateDefOf.Berserk;
            }
            return CorruptionDefOfs.KhorneKillWeak;
        }


    }
}
