using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption
{
    public class JobDriver_HoldSermon : JobDriver
    {
        private TargetIndex AltarIndex = TargetIndex.A;
        private TargetIndex AltarInteractionCell = TargetIndex.B;

        public override void ExposeData()
        {
            base.ExposeData();
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.EndOnDespawnedOrNull(AltarIndex, JobCondition.Incompletable);
            yield return Toils_Reserve.Reserve(AltarIndex, 1);
            yield return Toils_Reserve.Reserve(AltarInteractionCell, 1);
            Toil gotoAltarToil;
            
                gotoAltarToil = Toils_Goto.GotoThing(AltarInteractionCell, PathEndMode.OnCell);

            yield return gotoAltarToil;

            List<Pawn> Listeners = this.Map.mapPawns.AllPawnsSpawned.FindAll(x => x.CurJob.def == CorruptionDefOfs.AttendSermon);



            var altarToil = new Toil();
            altarToil.defaultCompleteMode = ToilCompleteMode.Delay;
            altarToil.defaultDuration = this.CurJob.def.joyDuration;
            altarToil.AddPreTickAction(() =>
            {
                this.pawn.Drawer.rotator.FaceCell(this.TargetA.Cell);
                this.pawn.GainComfortFromCellIfPossible();                
            });
            yield return altarToil;
            this.AddFinishAction(() =>
            {
                if (this.TargetA.HasThing)
                {
                    this.Map.reservationManager.Release(this.CurJob.targetA.Thing, pawn);
                }
                else
                {
                    this.Map.reservationManager.Release(this.CurJob.targetA.Cell, this.pawn);
                }

                SermonUtility.HoldSermonTickCheckEnd(this.pawn, this.TargetA.Thing as BuildingAltar);


            });
        }
    }
}
