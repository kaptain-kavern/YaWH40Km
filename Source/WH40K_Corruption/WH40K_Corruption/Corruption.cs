using System;
using RimWorld;
using Verse;

namespace WH40K_Corruption
{
    public class Corruption : Need
    {

        private const float ThreshIntrigued = 0.15f;

        private const float ThreshWarptouched = 0.4f;

        private const float ThreshTainted = 0.75f;

        private const float ThreshCorrupted = 0.99f;

        public CorruptionCategory CurCategory
        {
            get
            {
                if (this.CurLevel > 0.99f)
                {
                    return CorruptionCategory.Corrupted;
                }
                if (this.CurLevel > 0.75f)
                {
                    return CorruptionCategory.Tainted;
                }
                if (this.CurLevel > 0.4f)
                {
                    return CorruptionCategory.Warptouched;
                }
                if (this.CurLevel > 0.15f)
                {
                    return CorruptionCategory.Intrigued;
                }
                   return CorruptionCategory.Pure;
            }
        }

        public Corruption(Pawn pawn) : base(pawn)
		{
            this.threshPercents = new List<float>();
            this.threshPercents.Add(0.15f);
            this.threshPercents.Add(0.4f);
            this.threshPercents.Add(0.75f);
        }


    }
}
