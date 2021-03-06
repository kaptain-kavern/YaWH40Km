﻿using RimWorld;
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
        public PsykerPowerManager PowerManager;

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

        public Material CastingDrawMat
        {
            get
            {                
               return GraphicDatabase.Get<Graphic_Single>(curPower.uiIconPath, ShaderDatabase.MoteGlow, Vector2.one, Color.white).MatSingle;
            }
        }

        public TargetInfo CurTarget;

        public PsykerPowerDef curPower;

        public Verb_CastWarpPower curVerb;

        public Rot4 curRotation;

        public float TicksToCastPercentage = 1;

        public int TicksToCastMax = 100;

        public int TicksToCast = 0;

        public bool IsActive;

        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();
            this.PowerManager = new PsykerPowerManager(this);
        }

        public List<PsykerPower> Powers = new List<PsykerPower>();

        public List<PsykerPower> temporaryWeaponPowers = new List<PsykerPower>();

        public List<PsykerPower> temporaryApparelPowers = new List<PsykerPower>();

        public List<PsykerPower> allPowers = new List<PsykerPower>();

        public List<Verb_CastWarpPower> PowerVerbs = new List<Verb_CastWarpPower>();

        public void UpdatePowers()
        {

            PowerVerbs.Clear();
            List<PsykerPower> psylist = new List<PsykerPower>();

            psylist.AddRange(this.Powers);

            psylist.AddRange(this.temporaryWeaponPowers);

            this.allPowers = psylist;

            for (int i = 0; i < allPowers.Count; i++)
            {
                Verb_CastWarpPower newverb = (Verb_CastWarpPower)Activator.CreateInstance(psylist[i].powerdef.MainVerb.verbClass);
                if (!PowerVerbs.Any(item => item.verbProps == newverb.verbProps))
                {
                    newverb.caster = this.psyker;
                    newverb.verbProps = psylist[i].powerdef.MainVerb;
                    PowerVerbs.Add(newverb);
                }
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

        public static Action TryCastPowerAction(Pawn pawn, TargetInfo target, CompPsyker compPsyker, Verb_CastWarpPower verb, PsykerPowerDef psydef)
        {
            Action act = new Action(delegate
            {
                //            compPsyker.TicksToCast = verb.warpverbprops.TicksToRecharge;
                //            compPsyker.TicksToCastMax = verb.warpverbprops.TicksToRecharge;
                compPsyker.CurTarget = target;
                compPsyker.curVerb = verb;
                compPsyker.curPower = psydef; 
                compPsyker.curRotation = target.Thing.Rotation;
                Job job = CompPsyker.PsykerJob(verb.warpverbprops.PsykerPowerCategory, target);             
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

        public static Job PsykerJob(PsykerPowerTargetCategory cat, TargetInfo target)
        {
            switch(cat)
            {
                case PsykerPowerTargetCategory.TargetSelf:
                    {
                        return new Job(CorruptionDefOfs.CastPsykerPowerSelf, target);
                    }
                case PsykerPowerTargetCategory.TargetAoE:
                    {
                        return new Job(CorruptionDefOfs.CastPsykerPowerSelf, target);
                    }
                case PsykerPowerTargetCategory.TargetThing:
                    {
                        return new Job(CorruptionDefOfs.CastPsykerPowerVerb, target);
                    }
                default:
                    {
                        return new Job(CorruptionDefOfs.CastPsykerPowerVerb, target);
                    }
            }            
        }

        public IEnumerable<Command_CastPower> GetPsykerVerbsNew()
        {
            //     Log.Message("Found temp powers: " + this.temporaryPowers.Count.ToString() + " while finding Verbs: " + temporaryPowers.Count.ToString());
            //     Log.Message(this.PowerVerbs.Count.ToString());
            List<Verb_CastWarpPower> temp = new List<Verb_CastWarpPower>();
            temp.AddRange(this.PowerVerbs);
            for (int i = 0; i < temp.Count; i++)
            {
                int j = i;
                Verb_CastWarpPower newverb = temp[j];
                newverb.caster = this.psyker;
                newverb.verbProps = temp[j].verbProps;

                Command_CastPower command_CastPower = new Command_CastPower(this);
                command_CastPower.verb = newverb;
                command_CastPower.defaultLabel = allPowers[j].def.LabelCap;
                command_CastPower.defaultDesc = allPowers[j].def.description;
                command_CastPower.targetingParams = TargetingParameters.ForAttackAny();
                if (newverb.warpverbprops.PsykerPowerCategory == PsykerPowerTargetCategory.TargetSelf || newverb.warpverbprops.PsykerPowerCategory == PsykerPowerTargetCategory.TargetAoE)
                {
                    command_CastPower.targetingParams = TargetingParameters.ForSelf(this.psyker);
                }
                command_CastPower.icon = allPowers[j].def.uiIcon;
                string str;
                if (FloatMenuUtility.GetAttackAction(this.psyker, TargetInfo.Invalid, out str) == null)
                {
                    command_CastPower.Disable(str.CapitalizeFirst() + ".");
                }
                command_CastPower.action = delegate (Thing target)
                {
                        Action attackAction = CompPsyker.TryCastPowerAction(psyker, target, this, newverb, allPowers[j].def as PsykerPowerDef);
                        if (attackAction != null)
                    {
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
            yield break;
        }   

        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            if (psyker.Drafted)
            {
                foreach (Command_Target comm in GetPsykerVerbsNew().ToList())
                {
                    yield return comm;
                }
            }                            
        }

        public void DrawPsykerTargetReticule()
        {
            if (psyker.stances.curStance.GetType() == typeof(Stance_Warmup) && (this.CurTarget != null && this.curVerb != null))
            {
                //    curRotation.Rotate(RotationDirection.Clockwise);  
                float scale = 2f;
                if (this.curVerb.warpverbprops.PsykerPowerCategory == PsykerPowerTargetCategory.TargetAoE)
                {
                    scale = this.curVerb.verbProps.range * 2;
                }
                Vector3 s = new Vector3(scale, 1f, scale);
                Matrix4x4 matrix = default(Matrix4x4);
                Vector3 drawPos = this.CurTarget.Thing.DrawPos;
                drawPos.y -= 1f;
                matrix.SetTRS(drawPos, Quaternion.AngleAxis(curRotation.AsInt, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, this.CastingDrawMat, 0);
            }
        }

        public override void PostDraw()
        {
            if (psyker.stances != null)
            {
                DrawPsykerTargetReticule();
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.LookList<PsykerPower>(ref this.allPowers, "allPowers", LookMode.Deep, new object[0]);
            Scribe_Collections.LookList<PsykerPower>(ref this.temporaryApparelPowers, "temporaryApparelPowers", LookMode.Deep, new object[0]);
            Scribe_Collections.LookList<PsykerPower>(ref this.temporaryWeaponPowers, "temporaryWeaponPowers", LookMode.Deep, new object[0]);
            Scribe_Collections.LookList<PsykerPower>(ref this.Powers, "Powers", LookMode.Deep, new object[0]);
            Scribe_Collections.LookList<PsykerPower>(ref this.allPowers, "allPowers", LookMode.Deep, new object[0]);

            Scribe_Values.LookValue<int>(ref this.TicksToCast, "TicksToCast", 0, false);
            Scribe_Values.LookValue<int>(ref this.TicksToCastMax, "TicksToCastMax", 1, false);
            Scribe_Values.LookValue<float>(ref this.TicksToCastPercentage, "TicksToCastPercentage", 1, false);
            Scribe_Values.LookValue<bool>(ref this.IsActive, "IsActive", false, false);
            Scribe_Values.LookValue<bool>(ref this.ShotFired, "ShotFired", true, false);
  //          Scribe_Deep.LookDeep<Verb_CastWarpPower>(ref this.curVerb, "curVerb", null);
  //          Scribe_TargetInfo.LookTargetInfo(ref this.CurTarget, "CurTarget", null);

   //         Scribe_Deep.LookDeep<PsykerPowerManager>(ref this.PowerManager, "PowerManager", new object[]
   //             {
   //               this
   //             });
            



        }
    }
}
