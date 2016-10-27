using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace WH40K_Corruption
{
    public static class CorruptionItemUtility
    {
        public static List<NatureCategory> AllNatureCategories;

        static CorruptionItemUtility()
        {
            CorruptionItemUtility.AllNatureCategories = new List<NatureCategory>();
            IEnumerator enumerator = Enum.GetValues(typeof(NatureCategory)).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    NatureCategory item = (NatureCategory)((byte)enumerator.Current);
                    CorruptionItemUtility.AllNatureCategories.Add(item);
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        public static bool TryGetNature(this Thing t, out NatureCategory cc)
        {
            MinifiedThing minifiedThing = t as MinifiedThing;
            CompNature compCorruption = (minifiedThing == null) ? t.TryGetComp<CompNature>() : minifiedThing.InnerThing.TryGetComp<CompNature>();
            if (compCorruption == null)
            {
                cc = NatureCategory.Common;
                return false;
            }
            cc = compCorruption.Nature;
            return true;
        }

        public static string GetLabel(this NatureCategory cat)
        {
            switch (cat)
            {
                case NatureCategory.Holy:
                    return "CorruptionItemCategory_Holy".Translate();
                case NatureCategory.Common:
                    return "CorruptionItemCategory_Common".Translate();
                case NatureCategory.Corrupted:
                    return "CorruptionItemCategory_Corrupted".Translate();
                default:
                    throw new ArgumentException();
            }
        }

        public static string GetLabelShort(this NatureCategory cat)
        {
            switch (cat)
            {
                case NatureCategory.Holy:
                    return "CorruptionItemCategoryShort_Holy".Translate();
                case NatureCategory.Common:
                    return "CorruptionItemCategoryShort_Common".Translate();
                case NatureCategory.Corrupted:
                    return "CorruptionItemCategoryShort_Corrupted".Translate();
                default:
                    throw new ArgumentException();
            }
        }

        public static bool FollowNatureThingFilter(this ThingDef def)
        {
            return def.stackLimit == 1 || def.HasComp(typeof(CompNature));
        }

        public static NatureCategory RandomNature()
        {
            return CorruptionItemUtility.AllNatureCategories.RandomElement<NatureCategory>();
        }

        public static NatureCategory RandomCreationNature(int corruptionlevel)
        {
            float centerX = -1f;

            {
                if (corruptionlevel < 0.1f)
                {
                    centerX = 1f;
                }
                if (corruptionlevel < 0.5f)
                {
                    centerX = 1.2f;
                }
                if (corruptionlevel > 0.8f)
                {
                    centerX = 1.4f;
                }
                if (corruptionlevel > 0.95f)
                {
                    centerX = 1.7f;
                }

            }

                       


            float num = Rand.Gaussian(centerX, 0.55f);
            num = Mathf.Clamp(num, 0f, (float)CorruptionItemUtility.AllNatureCategories.Count);
            return (NatureCategory)((int)num);
        }

        public static NatureCategory RandomTraderItemQuality()
        {
            float centerX = 3f;
            float num = Rand.Gaussian(centerX, 2f);
            num = Mathf.Clamp(num, 0f, (float)CorruptionItemUtility.AllNatureCategories.Count - 0.5f);
            return (NatureCategory)((int)num);
        }

        public static NatureCategory RandomGeneratedGearQuality(PawnKindDef pawnKind)
        {
            if (pawnKind.forceNormalGearQuality)
            {
                return NatureCategory.Common;
            }
            float centerX = (float)pawnKind.itemQuality;
            float num = Rand.GaussianAsymmetric(centerX, 1.5f, 1.07f);
            num = Mathf.Clamp(num, 0f, (float)CorruptionItemUtility.AllNatureCategories.Count);
            return (NatureCategory)((int)num);
        }


    }
