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
        public override void Notify_PawnDied()
        {
            if (this.Pawn.corpse.Spawned)
            {
                GenExplosion.DoExplosion(this.Pawn.Position, 5, CorruptionDefOfs.RottenBurst ,null, null, null, null, ThingDefOf.FilthVomit, 1);
                Pawn.corpse.Destroy(DestroyMode.Vanish);
            }
        }
    }
}
