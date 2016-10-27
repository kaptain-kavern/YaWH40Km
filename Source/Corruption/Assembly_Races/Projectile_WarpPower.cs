using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Corruption
{
    public class Projectile_WarpPower : Projectile
    {
        public Thing selectedTarget;

        public int TicksToImpact
        {
            get
            {
               return  this.ticksToImpact;
            }
        }

        public Vector3 targetVec;

        public Vector3 ProjectileDrawPos
        {
            get
            {
                if (selectedTarget != null)
                {
                    return selectedTarget.DrawPos;
                }
                else if (targetVec != null)
                {
                    return targetVec;
                }
                return this.ExactPosition;
            }
        }

        public ProjectileDef_WarpPower mpdef
        {
            get
            {
               return (ProjectileDef_WarpPower)def;
            }
        }

        public override void Draw()
        {
            if (selectedTarget != null || targetVec != null)
            {
                Vector3 vector = this.ProjectileDrawPos;
                Vector3 distance = this.destination - this.origin;
                Vector3 curpos = this.destination - this.Position.ToVector3();
                var num = 1 - (Mathf.Sqrt(Mathf.Pow(curpos.x, 2) + Mathf.Pow(curpos.z, 2)) / (Mathf.Sqrt(Mathf.Pow(distance.x, 2) + Mathf.Pow(distance.z, 2))));
                float angle = 0f;
                Material mat = this.Graphic.MatSingle;
                Vector3 s = new Vector3(num*2, 1f, num*2);
                Matrix4x4 matrix = default(Matrix4x4);
                vector.y = 3;
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0);
            }
            else
            {
                Graphics.DrawMesh(MeshPool.plane10, this.DrawPos, this.ExactRotation, this.def.DrawMatSingle, 0);
            }
            base.Comps_PostDraw();
        }

        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);
            if (hitThing != null)
            {

                Pawn victim = hitThing as Pawn;
                if (victim != null)
                {
                    if (mpdef.IsMentalStateGiver)
                    {
                       string str = "MentalStateByPsyker".Translate(new object[]
                        {
                            victim.NameStringShort,
                        });
                        if (mpdef.InducesMentalState == MentalStateDefOf.Berserk && victim.RaceProps.intelligence < Intelligence.Humanlike)
                        {
                            victim.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, str, true);
                        }
                        else
                        {
                            victim.mindState.mentalStateHandler.TryStartMentalState(mpdef.InducesMentalState, str, true);
                        }
                    }
                    else if (mpdef.IsBuffGiver)
                    {
                        victim.health.AddHediff(mpdef.BuffDef);
                    }
                    else if (mpdef.IsHealer)
                    {
                        BodyPartRecord part = victim.health.hediffSet.GetInjuredParts().RandomElement();
                        if (part != null)
                        {
                            DamageDef damdef;
                            if (mpdef.HealFailChance < Rand.Range(0f, 1f))
                            {
                                damdef = this.def.projectile.damageDef;
                            }
                            else
                            {
                                damdef = DefDatabase<DamageDef>.AllDefsListForReading.RandomElement();
                            }
                            List<HediffDef> healHediff = DefDatabase<HediffDef>.AllDefsListForReading;
                            BodyPartDamageInfo value = new BodyPartDamageInfo(part, false, healHediff);
                            victim.TakeDamage(new DamageInfo(damdef, this.def.projectile.damageAmountBase, null, new BodyPartDamageInfo?(value), null));
                        }
                        else
                        {
                            SoundDefOf.Pawn_Melee_Punch_Miss.PlayOneShot(base.Position);
                            MoteMaker.MakeStaticMote(this.ExactPosition, ThingDefOf.Mote_ShotHit_Dirt, 1f);
                        }
                    }
                    else
                    {
                        int damageAmountBase = this.def.projectile.damageAmountBase;
                        BodyPartDamageInfo value = new BodyPartDamageInfo(null, null);
                        DamageInfo dinfo = new DamageInfo(this.def.projectile.damageDef, damageAmountBase, this.launcher, this.ExactRotation.eulerAngles.y, new BodyPartDamageInfo?(value), this.equipmentDef);
                        hitThing.TakeDamage(dinfo);
                    }
                }
            }
            else
            {
                SoundDefOf.PowerOffSmall.PlayOneShot(base.Position);
                MoteMaker.MakeStaticMote(this.ExactPosition, ThingDefOf.Mote_ShotHit_Spark, 1f);
            }
            
        }
    }
}
