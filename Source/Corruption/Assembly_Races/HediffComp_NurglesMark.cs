using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Corruption
{
    public class HediffComp_NurglesMark : HediffComp
    {
        private int lastTick;

        public override void Notify_PawnDied()
        {
            if (this.Pawn.corpse.Spawned)
            {
                GenExplosion.DoExplosion(this.Pawn.Position, 5, CorruptionDefOfs.RottenBurst ,null, null, null, null, ThingDefOf.FilthVomit, 1);
                Pawn.corpse.Destroy(DestroyMode.Vanish);
            }
        }

        public override void CompPostTick()
        {
            base.CompPostTick();
            if (lastTick < Find.TickManager.TicksGame + 600)
            {
                FilthMaker.MakeFilth(this.Pawn.DrawPos.ToIntVec3(), ThingDefOf.FilthVomit, 1);
                lastTick = Find.TickManager.TicksGame;
            }
        }
    }
}
