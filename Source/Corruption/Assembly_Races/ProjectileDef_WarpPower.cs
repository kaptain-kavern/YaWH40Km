using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class ProjectileDef_WarpPower : ThingDef
    {
        public bool IsMentalStateGiver = false;
        public MentalStateDef InducesMentalState;

        public bool IsBuffGiver = false;
        public HediffDef BuffDef;

        public bool IsHealer = false;
        public float HealFailChance = 0.3f;
    }
}
