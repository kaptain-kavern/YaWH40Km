using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Corruption
{
    public class ChaosFollowerPawnKindDef : PawnKindDef
    {
        public AfflictionProperty AfflictionProperty;

        public bool RenamePawns = false;

        public bool UseFixedGender = false;

        public Gender FixedGender;

        public RulePackDef OverridingNameRulePack;
    }
}
