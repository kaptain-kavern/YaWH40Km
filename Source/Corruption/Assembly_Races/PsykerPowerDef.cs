﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class PsykerPowerDef : ThingDef
    {
        public string DivineOrigin;

        public PsykerPowerLevel PowerLevel;

        public SoulAffliction MinAfflictionToGet = SoulAffliction.Warptouched;

        public SoulAffliction MaxAfflictionToGet = SoulAffliction.Lost;

        public int RechargeTicks;

        public int CastTime = 0;

        public SoulAffliction MinAfflictionCategory;

        public float CorruptionFactor;

        public string IconGraphicPath;

        public PsykerPowerTargetCategory PsykerPowerCategory = PsykerPowerTargetCategory.TargetSelf;

        public VerbProperties_WarpPower MainVerb;
    }
}
