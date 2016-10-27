using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

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
                    t.nullifyingTraits.Add(CorruptionDefOfs.Khorne_Fervor);
                    t.nullifyingTraits.Add(CorruptionDefOfs.Nurgle_Fervor);
                    t.nullifyingTraits.Add(CorruptionDefOfs.Tzeentch_Fervor);
                }

                if (t.defName == "KnowColonistExecuted")
                {
                    t.nullifyingTraits.Add(CorruptionDefOfs.Khorne_Fervor);
                    t.nullifyingTraits.Add(CorruptionDefOfs.Nurgle_Fervor);
                    t.nullifyingTraits.Add(CorruptionDefOfs.Tzeentch_Fervor);
                }

                if (t.defName == "KnowPrisonerDiedInnocent")
                {
                    t.nullifyingTraits.Add(CorruptionDefOfs.Khorne_Fervor);
                    t.nullifyingTraits.Add(CorruptionDefOfs.Nurgle_Fervor);
                    t.nullifyingTraits.Add(CorruptionDefOfs.Tzeentch_Fervor);
                }

                if (t.defName == "PawnWithGoodOpinionDied")
                {
                    t.nullifyingTraits.Add(CorruptionDefOfs.Khorne_Fervor);
                    t.nullifyingTraits.Add(CorruptionDefOfs.Nurgle_Fervor);
                    t.nullifyingTraits.Add(CorruptionDefOfs.Tzeentch_Fervor);
                }

                if (t.defName == "ButcheredHumanlikeCorpse")
                {
                    t.nullifyingTraits.Add(CorruptionDefOfs.Khorne_Fervor);
                    t.nullifyingTraits.Add(CorruptionDefOfs.Nurgle_Fervor);
                    t.nullifyingTraits.Add(CorruptionDefOfs.Tzeentch_Fervor);
                    t.nullifyingTraits.Add(CorruptionDefOfs.Slaanesh_Fervor);

                }

                if (t.defName == "KnowButcheredHumanlikeCorpse")
                {
                    t.nullifyingTraits.Add(CorruptionDefOfs.Khorne_Fervor);
                    t.nullifyingTraits.Add(CorruptionDefOfs.Nurgle_Fervor);
                    t.nullifyingTraits.Add(CorruptionDefOfs.Tzeentch_Fervor);
                    t.nullifyingTraits.Add(CorruptionDefOfs.Slaanesh_Fervor);
                }

                if (t.defName == "ObservedLayingCorpse")
                {
                    t.nullifyingTraits.Add(CorruptionDefOfs.Khorne_Fervor);
                    t.nullifyingTraits.Add(CorruptionDefOfs.Nurgle_Fervor);
                    t.nullifyingTraits.Add(CorruptionDefOfs.Tzeentch_Fervor);
                }

                if (t.defName == "ObservedLayingRottingCorpse")
                {
                    t.nullifyingTraits.Add(CorruptionDefOfs.Khorne_Fervor);
                    t.nullifyingTraits.Add(CorruptionDefOfs.Nurgle_Fervor);
                    t.nullifyingTraits.Add(CorruptionDefOfs.Tzeentch_Fervor);
                }

                if (t.defName == "WitnessedDeathAlly")
                {
                    t.nullifyingTraits.Add(CorruptionDefOfs.Khorne_Fervor);
                    t.nullifyingTraits.Add(CorruptionDefOfs.Nurgle_Fervor);
                }

                if (t.defName == "WitnessedDeathNonAlly")
                {
                    t.nullifyingTraits.Add(CorruptionDefOfs.Khorne_Fervor);
                    t.nullifyingTraits.Add(CorruptionDefOfs.Nurgle_Fervor);
                    t.nullifyingTraits.Add(CorruptionDefOfs.Tzeentch_Fervor);
                    t.nullifyingTraits.Add(CorruptionDefOfs.Slaanesh_Fervor);
                }

            }


        }
    }
}
