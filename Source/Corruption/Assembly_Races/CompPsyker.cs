using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Corruption
{
    public class CompPsyker : CompUseEffect
    {
        public bool ShotFired = true;

        public int ticksToImpact = 500;

        public Pawn psyker
        {
            get
            {
                return this.parent as Pawn;
            }
        }

        public Need_Soul soul
        {
            get
            {
                Need_Soul s = psyker.needs.TryGetNeed<Need_Soul>();
                if (s != null)
                {
                    return s;
                }
                return null;
            }
        }

        public float TicksToCastPercentage = 1;

        public int TicksToCastMax = 100;

        public int TicksToCast = 0;

        public bool IsActive;
        
        public Verb_CastWarpPower selectedVerb;

        public List<CompSoulItem> EquippedSoulItems
        {
            get
            {
 //               Log.Message("Getting EQ");
                List<CompSoulItem> templist = new List<CompSoulItem>();
                CompSoulItem tcomp;

                List<Apparel> applist = psyker.apparel.WornApparel;
                for (int i = 0; i < applist.Count; i++)
                {
                    if ((tcomp = applist[i].GetComp<CompSoulItem>())!= null)
                    {
                        templist.Add(tcomp);
                    }
                }
                List<ThingWithComps> eq = psyker.equipment.AllEquipment.ToList();
                for (int j = 0; j < eq.Count; j++)
                {
   //                 Log.Message("Found Weapon :" + eq[j].ToString());
                    if ((tcomp = eq[j].GetComp<CompSoulItem>()) != null)
                    {
 //                       Log.Message("AddingComp");
                        templist.Add(tcomp);
                    }
                }
                return templist;
            }
        }

        public List<PsykerPower> Powers = new List<PsykerPower>();

        public List<PsykerPower> temporaryPowers;

        public List<Verb_CastWarpPower> PowerVerbs
        {
            get
            {
                List<Verb_CastWarpPower> list = new List<Verb_CastWarpPower>();
                List<PsykerPower> allPowers = Powers;
                //     allPowers.AddRange(temporaryPowers);
      //          Log.Message(temporaryPowers.Count.ToString() + " TempPowers found");
                foreach(PsykerPower pow in allPowers)
                {
                    Verb_CastWarpPower newverb = (Verb_CastWarpPower)Activator.CreateInstance(pow.powerdef.MainVerb.verbClass);
                    if (!list.Any(item => item.verbProps == newverb.verbProps))
                    {
                        newverb.caster = this.psyker;
                        newverb.verbProps = pow.powerdef.MainVerb;
                        list.Add(newverb);
                    }
                }
                return list;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            this.TicksToCast--;
            if (this.TicksToCast <0)
            {
                this.IsActive = true;
                this.ShotFired = true;
                this.TicksToCast = 0;
            }
            this.TicksToCastPercentage = (1 - (this.TicksToCast / this.TicksToCastMax));
        }

        public static Action TryCastPowerAction(Pawn pawn, TargetInfo target, CompPsyker compPsyker, Verb_CastWarpPower verb)
        {
            Action act = new Action(delegate
            {
                compPsyker.TicksToCast = verb.warpverbprops.TicksToRecharge;
                compPsyker.TicksToCastMax = verb.warpverbprops.TicksToRecharge;
                Job job = new Job(CorruptionDefOfs.CastPsykerPower, target);
                job.playerForced = true;
                job.verbToUse = verb;
                Pawn pawn2 = target.Thing as Pawn;
                if (pawn2 != null)
                {
                    job.killIncappedTarget = pawn2.Downed;
                }
                pawn.drafter.TakeOrderedJob(job);
            });
            return act;
        }

        public IEnumerable<Command_CastPower> GetPsykerVerbsNew(List<PsykerPower> list)
        {
            for (int i = 0; i < PowerVerbs.Count; i++)
            {
                Verb_CastWarpPower newverb = PowerVerbs[i];
                newverb.caster = this.psyker;
                newverb.verbProps = PowerVerbs[i].verbProps;

                Command_CastPower command_CastPower = new Command_CastPower(this);
                command_CastPower.defaultLabel = Powers[i].def.label;
                command_CastPower.defaultDesc = Powers[i].def.description;
                command_CastPower.targetingParams = TargetingParameters.ForAttackAny();
                command_CastPower.hotKey = KeyBindingDefOf.Misc1;
                command_CastPower.icon = Powers[i].def.uiIcon;
                string str;
                if (FloatMenuUtility.GetAttackAction(this.psyker, TargetInfo.Invalid, out str) == null)
                {
                    command_CastPower.Disable(str.CapitalizeFirst() + ".");
                }
                command_CastPower.action = delegate (Thing target)
                {
                        Action attackAction = CompPsyker.TryCastPowerAction(psyker, target, this, newverb);
                        if (attackAction != null)
                        {
                        this.selectedVerb = newverb;
                            this.IsActive = true;
                            attackAction();
                        }                    
                };
                if (newverb.caster.Faction != Faction.OfPlayer)
                {
                    command_CastPower.Disable("CannotOrderNonControlled".Translate());
                }
                if (newverb.CasterIsPawn)
                {
                    if (newverb.CasterPawn.story.DisabledWorkTags.Contains(WorkTags.Violent))
                    {
                        command_CastPower.Disable("IsIncapableOfViolence".Translate(new object[]
                        {
                            newverb.CasterPawn.NameStringShort
                        }));
                    }
                    else if (!newverb.CasterPawn.drafter.Drafted)
                    {
                        command_CastPower.Disable("IsNotDrafted".Translate(new object[]
                        {
                            newverb.CasterPawn.NameStringShort
                        }));
                    }
                    else if (!this.IsActive)
                    {
                        command_CastPower.Disable("PsykerPowerRecharging".Translate(new object[]
                            {
                                newverb.CasterPawn.NameStringShort
                            }));
                    }
                }
                yield return command_CastPower;
            }
        }   
    
        public bool TryStartCasting(TargetInfo targ)
        {
            if (this.psyker.stances.FullBodyBusy)
            {
                return false;
            }
            if (this.psyker.story != null && this.psyker.story.DisabledWorkTags.Contains(WorkTags.Violent))
            {
                return false;
            }
      //      bool allowManualCastWeapons = !this.psyker.IsColonist;
            Verb_CastWarpPower verbPower = this.selectedVerb;
            return verbPower != null && verbPower.TryStartCastOn(targ, false, true);
        }

        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            List<PsykerPower> verblist = this.Powers.FindAll(x => x.powerdef.MainVerb != null);
            if (psyker.Drafted)
            {
                foreach (Command_Target comm in this.GetPsykerVerbsNew(verblist))
                {
                    yield return comm;
                }
            }
                            
        }
    }
}
