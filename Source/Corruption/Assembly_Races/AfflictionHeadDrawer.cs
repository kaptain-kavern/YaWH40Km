using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    public class AfflictionHeadDrawer : ThingComp
    {
        private Apparel app
        {
            get
            {
                return this.parent as Apparel;
            }
        }

        private Need_Soul soul
        {
            get
            {
                return app.wearer.needs.TryGetNeed<Need_Soul>();
            }
        }


        public override void PostDraw()
        {
            base.PostDraw();
            AfflictionDrawerUtility.DrawHeadOverlay(app.wearer, soul.patronInfo.PatronName);
        }




    }
}
