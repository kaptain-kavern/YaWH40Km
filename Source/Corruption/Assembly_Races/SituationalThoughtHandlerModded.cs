using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Corruption
{
    public class SituationalThoughtHandlerModded
    {
        public SituationalThoughtHandler SituationalThoughtHandler { get; set; }

        public ThoughtHandler ThoughtHandler { get; set; }

        public Pawn npawn { get { return SituationalThoughtHandler.pawn; } }

        public Pawn tpawn { get { return ThoughtHandler.pawn; } }

        public static bool IsAutomaton(Pawn pawn)
        {            
            return pawn.AllComps.Any(i => i.GetType() == typeof(CompThoughtlessAutomaton));          
        }

        public static bool HasSoulTraitRequirements(ThoughtDef thdef, Pawn p)
        {

            if (p.needs.TryGetNeed<Need_Soul>() == null)

            {
                SituationalThoughtHandlerModded.CreateNewSoul(p);
            }

            PatronInfo pinfo = p.needs.TryGetNeed<Need_Soul>().patronInfo;
            List<TraitDef> lvt = thdef.requiredTraits;
            if (lvt == null) Log.Message("No Required Traits");
            SoulTraitDef stdef = pinfo.PatronSpecificTrait(pinfo.PatronName);
            int tt = 0;
            foreach (TraitDef tdef in lvt)
            {
                if (stdef.defName == tdef.defName)
                {
                    tt += 1;
                }
            }
            if (tt > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool HasSoulTraitNullyfyingTraits(ThoughtDef def, Pawn p)
        {
            if (p == null) Log.Message("NoPawn");
            if (def == null) Log.Message("NoDef");

            if (p.needs.TryGetNeed<Need_Soul>() == null)

            {
                Log.Message("New Soul");
                SituationalThoughtHandlerModded.CreateNewSoul(p);
            }

            List<SoulTrait> straitlist = p.needs.TryGetNeed<Need_Soul>().SoulTraits;
            if (straitlist == null) Log.Message("Nolist");
            int tt = 0;
            foreach (SoulTrait strait in straitlist)
            {
                foreach (ThoughtDef tdef in strait.SDef.NullifiesThoughts)
                {
                    if (tdef.defName == def.defName)
                    {
                        tt += 1;
                    }
                }
            }
            if (tt > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        private ThoughtDefAutomaton tdef = new ThoughtDefAutomaton();

        public bool CanGetThought(ThoughtDef def)
        {
            ProfilerThreadCheck.BeginSample("CanGetThought()");
            try
            {
                if (!def.validWhileDespawned && !this.tpawn.Spawned && !def.IsMemory)
                {
                    bool result = false;
                    return result;
                }
                if (def.nullifyingTraits != null)
                {
                    for (int i = 0; i < def.nullifyingTraits.Count; i++)
                    {
                        if (this.tpawn.story.traits.HasTrait(def.nullifyingTraits[i]))
                        {
                            bool result = false;
                            return result;
                        }
                    }
                }
                if (def.requiredTraits != null)
                {
                    for (int j = 0; j < def.requiredTraits.Count; j++)
                    {
                        if (!this.tpawn.story.traits.HasTrait(def.requiredTraits[j]))
                        {
                            if (def != null && tpawn != null)
                            {
                                return HasSoulTraitRequirements(def, tpawn);
                            }
                            else return false;
                        }

                            if (!this.tpawn.story.traits.HasTrait(def.requiredTraits[j]))
                        {
                            bool result = false;
                            return result;
                        }
                        if (def.RequiresSpecificTraitsDegree && def.requiredTraitsDegree != this.tpawn.story.traits.DegreeOfTrait(def.requiredTraits[j]))
                        {
                            bool result = false;
                            return result;
                        }
                    }
                }
                if (def.nullifiedIfNotColonist && !this.tpawn.IsColonist)
                {
                    bool result = false;
                    return result;
                }
                if (ThoughtUtility.IsSituationalThoughtNullifiedByHediffs(def, this.tpawn))
                {
                    bool result = false;
                    return result;
                }
                if (ThoughtUtility.IsThoughtNullifiedByOwnTales(def, this.tpawn))
                {
                    bool result = false;
                    return result;
                }
                            if (HasSoulTraitNullyfyingTraits(def, tpawn))
                            {
                                return false;
                            }
                if (IsAutomaton(this.tpawn))
                {
                    ThoughtDefAutomaton Tdef = new ThoughtDefAutomaton();
                    if ((Tdef = def as ThoughtDefAutomaton) != null && Tdef.IsAutomatonThought)
                    {
                        return true;
                    }

                    return false;
                }
            }
            finally
            {
                ProfilerThreadCheck.EndSample();
            }
            return true;
        }

        public bool CanGetThoughtV2(ThoughtDef def)
        {
            ProfilerThreadCheck.BeginSample("CanGetThought()");
            try
            {
                if (!def.validWhileDespawned && !this.npawn.Spawned && !def.IsMemory)
                {
                    bool result = false;
                    return result;
                }
                if (def.nullifyingTraits != null)
                {
                    for (int i = 0; i < def.nullifyingTraits.Count; i++)
                    {
                        if (this.npawn.story.traits.HasTrait(def.nullifyingTraits[i]))
                        {
                            bool result = false;
                            return result;
                        }
                    }
                }
                if (def.requiredTraits != null)
                {
                    for (int j = 0; j < def.requiredTraits.Count; j++)
                    {
                        if (!this.tpawn.story.traits.HasTrait(def.requiredTraits[j]))
         //               {
         //                   if (def != null && tpawn != null)
         //                   {
         //                       return HasSoulTraitRequirements(def, tpawn);
         //                   }
         //                   else return false;
         //               }
                        if (!this.npawn.story.traits.HasTrait(def.requiredTraits[j]))
                        {
                            bool result = false;
                            return result;
                        }
                        if (def.RequiresSpecificTraitsDegree && def.requiredTraitsDegree != this.npawn.story.traits.DegreeOfTrait(def.requiredTraits[j]))
                        {
                            bool result = false;
                            return result;
                        }
                    }
                }
                if (def.nullifiedIfNotColonist && !this.npawn.IsColonist)
                {
                    bool result = false;
                    return result;
                }
                if (ThoughtUtility.IsSituationalThoughtNullifiedByHediffs(def, this.npawn))
                {
                    bool result = false;
                    return result;
                }
                if (ThoughtUtility.IsThoughtNullifiedByOwnTales(def, this.npawn))
                {
                    bool result = false;
                    return result;
                }
    //            if (HasSoulTraitNullyfyingTraits(def, tpawn))
    //            {
    //                return false;
    //            }
                if (IsAutomaton(this.tpawn))
                {
                    ThoughtDefAutomaton Tdef = new ThoughtDefAutomaton();
                    if ((Tdef = def as ThoughtDefAutomaton) != null && Tdef.IsAutomatonThought)
                    {
                        return true;
                    }

                    return false;
                }
            }
            finally
            {
                ProfilerThreadCheck.EndSample();
            }
            return true;
        }       

        public static void CreateNewSoul(Pawn pepe)
        {
            {
                Need need = (Need)Activator.CreateInstance(typeof(Need_Soul), new object[]
            {
                pepe
            });
                need.def = CorruptionDefOfs.Need_Soul;
                int x = pepe.needs.AllNeeds.Count;
                pepe.needs.AllNeeds.Insert(x - 1, need);
      //          pepe.needs.AllNeeds.Add(need);
            }
        }
    }
}
