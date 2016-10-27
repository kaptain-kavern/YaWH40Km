using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace WH40K_Corruption
{
    public class CompNature : ThingComp
    {
        private NatureCategory ccint = NatureCategory.Common;

        public NatureCategory Nature
        {
            get
            {
                return this.ccint;
            }
        }

        public void SetNature(NatureCategory ccor, ArtGenerationContext source)
        {
            this.ccint = ccor;
            CompArt compArt = this.parent.TryGetComp<CompArt>();
            if (compArt != null)
            {
                compArt.InitializeArt(source);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.LookValue<NatureCategory>(ref ccint, "nature", NatureCategory.Common, false);
        }

        public override bool AllowStackWith(Thing other)
        {
            NatureCategory NatureCategory;
            return other.TryGetNature(out NatureCategory) && this.ccint == Nature;
        }

        public override void PostSplitOff(Thing piece)
        {
            base.PostSplitOff(piece);
            piece.TryGetComp<CompNature>().ccint = this.ccint;
        }

        public override string CompInspectStringExtra()
        {
            return "NatureIs".Translate(new object[]
            {
                Nature.GetLabel().CapitalizeFirst()
            });
        }

        public static implicit operator CompNature(CompQuality cv)
        {
            throw new NotImplementedException();
        }
    }
}
