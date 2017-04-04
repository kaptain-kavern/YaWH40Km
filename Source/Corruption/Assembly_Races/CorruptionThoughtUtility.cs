using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Corruption.DefOfs;

namespace Corruption
{
    public class CorruptionThoughtUtility
    {

        public static void SetNullifyingTraits()
        {
            List<ThoughtDef> tdeflist = DefDatabase<ThoughtDef>.AllDefsListForReading;
            foreach (ThoughtDef t in tdeflist)
            {
                if (t.defName == "KnowGuestExecuted")
                {
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Khorne_Fervor);
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Nurgle_Fervor);
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Tzeentch_Fervor);
                }

                if (t.defName == "KnowColonistExecuted")
                {
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Khorne_Fervor);
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Nurgle_Fervor);
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Tzeentch_Fervor);
                }

                if (t.defName == "KnowPrisonerDiedInnocent")
                {
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Khorne_Fervor);
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Nurgle_Fervor);
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Tzeentch_Fervor);
                }

                if (t.defName == "PawnWithGoodOpinionDied")
                {
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Khorne_Fervor);
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Nurgle_Fervor);
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Tzeentch_Fervor);
                }

                if (t.defName == "ButcheredHumanlikeCorpse")
                {
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Khorne_Fervor);
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Nurgle_Fervor);
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Tzeentch_Fervor);
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Slaanesh_Fervor);

                }

                if (t.defName == "KnowButcheredHumanlikeCorpse")
                {
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Khorne_Fervor);
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Nurgle_Fervor);
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Tzeentch_Fervor);
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Slaanesh_Fervor);
                }

                if (t.defName == "ObservedLayingCorpse")
                {
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Khorne_Fervor);
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Nurgle_Fervor);
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Tzeentch_Fervor);
                }

                if (t.defName == "ObservedLayingRottingCorpse")
                {
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Khorne_Fervor);
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Nurgle_Fervor);
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Tzeentch_Fervor);
                }

                if (t.defName == "WitnessedDeathAlly")
                {
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Khorne_Fervor);
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Nurgle_Fervor);
                }

                if (t.defName == "WitnessedDeathNonAlly")
                {
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Khorne_Fervor);
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Nurgle_Fervor);
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Tzeentch_Fervor);
                    t.nullifyingTraits.Add(C_SoulTraitDefOf.Slaanesh_Fervor);
                }

            }


        }
    }
}
