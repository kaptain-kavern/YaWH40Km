using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld
using Verse

namespace WH40K_Corruption
{
    class CorruptionProperties
    {
        private const float ThreshCommon = 0.1f;

        private const float ThreshCorrupted = 0.8f;

        public NatureCategory CurCategory
        {
            get
            {
                if (this.CurLevel > 0.8f)
                {
                    return NatureCategory.Corrupted;
                }
                if (this.CurLevel > 0.1f)
                {
                    return NatureCategory.Common;
                }
                return NatureCategory.Holy;
            }
        }

        public CorruptionItem(Pawn pawn) : base(pawn)
		{
            this.threshPercents = new List<float>();
            this.threshPercents.Add(0.1f);
            this.threshPercents.Add(0.8f);
        }




        public float CorruptionFactor = 0f


    }
}
