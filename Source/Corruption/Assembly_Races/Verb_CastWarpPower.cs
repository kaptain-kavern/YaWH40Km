using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using AlienRace;
using Verse.Sound;
using System.Reflection;

namespace Corruption
{
    public class Verb_CastWarpPower : Verb_LaunchProjectile
    {        
        public VerbProperties_WarpPower warpverbprops
        {
            get
            {
                return (VerbProperties_WarpPower)verbProps;
            }
        }

        public Need_Soul soul
        {
            get
            {
                return this.CasterPawn.needs.TryGetNeed<Need_Soul>();
            }
            set
            {

            }
        }
        
        public CompPsyker psycomp
        {
            get
            {
                return this.CasterPawn.TryGetComp<CompPsyker>();
            }
        }

        protected override bool TryCastShot()
        {
            for (int i = 0; i <= this.ShotsPerBurst; i++)
            {
                ShootLine shootLine;
                bool flag = base.TryFindShootLineFromTo(this.caster.Position, this.currentTarget, out shootLine);
                if (this.verbProps.stopBurstWithoutLos && !flag)
                {
                    return false;
                }
                Vector3 drawPos = this.caster.DrawPos;
                Projectile projectile = (Projectile)GenSpawn.Spawn(this.verbProps.projectileDef, shootLine.Source);
                projectile.FreeIntercept = (this.canFreeInterceptNow && !projectile.def.projectile.flyOverhead);                
                ShotReport shotReport = ShotReport.HitReportFor(this.caster, this, this.currentTarget);
                if (!this.warpverbprops.AlwaysHits)
                {
                    if (Rand.Value > shotReport.ChanceToNotGoWild_IgnoringPosture)
                    {
                        if (DebugViewSettings.drawShooting)
                        {
                            MoteMaker.ThrowText(this.caster.DrawPos, "ToWild", -1f);
                        }
                        shootLine.ChangeDestToMissWild();
                        if (this.currentTarget.HasThing)
                        {
                            projectile.ThingToNeverIntercept = this.currentTarget.Thing;
                        }
                        if (!projectile.def.projectile.flyOverhead)
                        {
                            projectile.InterceptWalls = true;
                        }
                        projectile.Launch(this.caster, drawPos, shootLine.Dest, this.ownerEquipment);
                        return true;
                    }
                    if (Rand.Value > shotReport.ChanceToNotHitCover)
                    {
                        if (DebugViewSettings.drawShooting)
                        {
                            MoteMaker.ThrowText(this.caster.DrawPos, "ToCover", -1f);
                        }
                        if (this.currentTarget.Thing != null && this.currentTarget.Thing.def.category == ThingCategory.Pawn)
                        {
                            Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
                            if (!projectile.def.projectile.flyOverhead)
                            {
                                projectile.InterceptWalls = true;
                            }
                            projectile.Launch(this.caster, drawPos, randomCoverToMissInto, this.ownerEquipment);
                            return true;
                        }
                    }
                }
                if (DebugViewSettings.drawShooting)
                {
                    MoteMaker.ThrowText(this.caster.DrawPos, "ToHit", -1f);
                }
                if (!projectile.def.projectile.flyOverhead)
                {
                    projectile.InterceptWalls = (!this.currentTarget.HasThing || this.currentTarget.Thing.def.Fillage == FillCategory.Full);
                }
                if (this.currentTarget.Thing != null)
                {
                    if (this.warpverbprops.DrawProjectileOnTarget)
                    {
                        Projectile_WarpPower wprojectile = projectile as Projectile_WarpPower;
                        wprojectile.selectedTarget = this.currentTarget.Thing;
                    }
                    projectile.Launch(this.caster, drawPos, this.currentTarget);
                }
                else
                {
                    if (this.warpverbprops.DrawProjectileOnTarget)
                    {
                        Projectile_WarpPower wprojectile = projectile as Projectile_WarpPower;
                        wprojectile.targetVec = shootLine.Dest.ToVector3();
                    }

                    projectile.Launch(this.caster, drawPos, shootLine.Dest);
                }

                if (soul != null)
                {
                    soul.GainNeed(0.01f * (-warpverbprops.CorruptionFactor));
                }
            }

            return true;
        }

        protected override int ShotsPerBurst
        {
            get
            {
                return this.verbProps.burstShotCount;
            }
        }

        public override void WarmupComplete()
        {
            this.burstShotsLeft = this.ShotsPerBurst;
            this.state = VerbState.Bursting;
            this.TryCastNextBurstShot();
        }
    }
}
