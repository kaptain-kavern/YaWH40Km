using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption
{
    [DefOf]
    public class CorruptionDefOfs
    {
        // Patron and Soul related Defs
        public static SoulTraitDef Devotion;

        public static SoulTraitDef Undivided_Fervor;

        public static SoulTraitDef Khorne_Fervor;

        public static SoulTraitDef Nurgle_Fervor;

        public static SoulTraitDef Slaanesh_Fervor;

        public static SoulTraitDef Tzeentch_Fervor;

        public static NeedDef Need_Soul;


        // Sermon Related Defs

        public static JobDef HoldSermon;

        public static JobDef AttendSermon;

        public static ThingDef Overlay_Khorne;

        public static ThingDef Overlay_Nurgle;

        public static ThingDef Overlay_Undivided;

        public static ThingDef Overlay_Tzeentch;

        public static ThingDef Overlay_Head;

        public static ThingDef Overlay_Hair;

        // Mental State Defs

        public static JobDef Khorne_KillWeakling;       

        public static MentalStateDef BingingFood;

        public static MentalStateDef LustViolent;

        public static MentalStateDef KhorneKillWeak;

        // Automaton Stuff Defs

        public static NeedDef Beauty;

        public static NeedDef Comfort;

        public static NeedDef Space;

        public static HediffDef DE_Toxin;

        public static HediffDef NurglesRot;

        public static HediffDef MarkNurgle;

        // DeepStriker Defs

        public static ThingDef Valkyrie;

        // BoneManufacturing Defs

        public static ThingDef TableButcher;

        // Thought Defs

        public static ThoughtDef MasochistPain;

        public static ThoughtDef Pain;

        // Damage Defs

        public static DamageDef RottenBurst;

        // PsykerDefs

        public static JobDef CastPsykerPowerVerb;

        public static JobDef CastPsykerPowerSelf;

        public static HediffDef DemonicPossession;

        public static ThingDef SpiritStone;

        // FactionDefs

        public static FactionDef ChaosCult;
        public static FactionDef DarkEldarKabal;
        public static FactionDef EldarWarhost;
        public static FactionDef ImperialGuard;
        public static FactionDef Orks;
        public static FactionDef AdeptusSororitas;
        public static FactionDef Mechanicus;

        // MapConditions

        public static MapConditionDef CorruptiveDrone;

        public static JobDef SummonDemon;
        public static JobDef SummoningTribute;
        public static ThingDef WarpRift;


    }
}
