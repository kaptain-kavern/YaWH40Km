using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    public class HediffComp_DemonicPossession : HediffComp
    {
        private Pawn Demon;

        public override void CompPostMake()
        {
            base.CompPostMake();

            this.Demon = GenerateDemon();
        }

        protected virtual Pawn GenerateDemon()
        {
            PawnKindDef pdef = DemonDefOfs.Demon_Undivided;
            int num = Rand.RangeInclusive(0, 4);
            switch (num)
            {
                case 0:
                    {
                        pdef = DemonDefOfs.Demon_Bloodletter;
                        break;
                    }
                case 1:
                    {
                        pdef = DemonDefOfs.Demon_Plaguebearer;
                        break;
                    }
                case 2:
                    {
                        pdef = DemonDefOfs.Demon_Daemonette;
                        break;
                    }
                case 3:
                    {
                        pdef = DemonDefOfs.Demon_Horror;
                        break;
                    }
                case 4:
                    {
                        pdef = DemonDefOfs.Demon_Undivided;
                        break;
                    }
            }

            PawnGenerationRequest request = new PawnGenerationRequest(pdef, null);
            Pawn pawn = null;
            try
            {
                pawn = PawnGenerator.GeneratePawn(request);
            }
            catch (Exception arg)
            {
                Log.Error("There was an exception thrown by the PawnGenerator during generating a starting pawn. Trying one more time...\nException: " + arg);
                pawn = PawnGenerator.GeneratePawn(request);
            }
            pawn.relations.everSeenByPlayer = true;
            PawnComponentsUtility.AddComponentsForSpawn(pawn);
            return pawn;
        }

        private Need_Soul soul
        {
            get
            {
                Need_Soul soulInt;
                if ((soulInt = this.Pawn.needs.TryGetNeed<Need_Soul>()) != null)
                    return soulInt;
                else
                {
                    return new Need_Soul(this.Pawn);
                }
            }
        }

        public override void CompPostTick()
        {
            base.CompPostTick();
            if (this.Pawn.def.race.Humanlike)
            {
                soul.GainNeed(-0.00005f);
            }
        }

        public override void Notify_PawnDied()
        {
            string label = "LetterDemonicPossessionResolve".Translate();
            string text2 = "LetterDemonicPossessionResolve_Content".Translate(new object[]
            {
                    this.Pawn.LabelShort,
            });
            Find.LetterStack.ReceiveLetter(label, text2, LetterType.BadUrgent, this.Pawn, null);

            GenSpawn.Spawn(Demon, Pawn.Position);
            Demon.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);

            if (this.Pawn.corpse.Spawned)
            {
                this.Pawn.corpse.Destroy(DestroyMode.Vanish);
            }

        }        
    }
}
