using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class SoulTrait : Trait
    {
        public SoulTraitDef SDef;

        public List<SoulTraitDegreeData> SoulDatas;
        
        public SoulTraitDegreeData CurrentSData;

        private int degree;

        public int SDegree
        {
            get
            {
                return this.degree;
            }
        }

        public static SoulTraitDef Named(string defName)
        {
            return DefDatabase<SoulTraitDef>.GetNamed(defName, true);
        }

        public SoulTraitDegreeData SoulCurrentData
        {
            get
            {
                return this.SDef.SDegreeDataAt(SDegree);
            }
        }

        public SoulTrait()
        { }

        public  SoulTrait(SoulTraitDef traitdef, int newdeg)
        {
            this.degree = newdeg;
            this.def = traitdef as TraitDef;
            this.SDef = traitdef;
            this.CurrentSData = traitdef.SDegreeDataAt(newdeg);
            this.NullifiedThoughtsInt = SDef.NullifiesThoughts;
        }


        public List<ThoughtDef> NullifiedThoughtsInt;

        public List<ThoughtDef> NullifiedThoughts
        {
            get
            {
                return this.NullifiedThoughtsInt;
            }
        }

        public string STipString(Pawn pawn)
        {
            StringBuilder stringBuilder = new StringBuilder();
            SoulTraitDegreeData currentData = this.CurrentSData;
            stringBuilder.Append(currentData.description.AdjustedFor(pawn));
            int count = this.CurrentData.skillGains.Count;
            if (count > 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
            }
            int num = 0;
            foreach (KeyValuePair<SkillDef, int> current in this.CurrentData.skillGains)
            {
                if (current.Value != 0)
                {
                    string value = "    " + current.Key.skillLabel + ":   " + current.Value.ToString("+##;-##");
                    if (num < count - 1)
                    {
                        stringBuilder.AppendLine(value);
                    }
                    else
                    {
                        stringBuilder.Append(value);
                    }
                    num++;
                }
            }
            return stringBuilder.ToString();
        }

        public new void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.LookDef<SoulTraitDef>(ref this.SDef, "SDef");
 //           Scribe_Collections.LookList<ThoughtDef>(ref this.NullifiedThoughtsInt, "NullifiedThoughtsInt", LookMode.Deep, new object[0]);
            Scribe_Values.LookValue<int>(ref this.degree, "degree", 0, false);
            Scribe_Deep.LookDeep<SoulTraitDegreeData>(ref this.CurrentSData, "CurrentSData", null, false);

            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs && this.def == null)
            {
                this.def = DefDatabase<SoulTraitDef>.GetRandom();
                this.degree = PawnGenerator.RandomTraitDegree(this.SDef);
            }
        }
    }
}
