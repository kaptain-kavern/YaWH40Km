using System;
using RimWorld
using Verse

namespace WH40K_Corruption
{
    public class SoulDef : Def
    {
        public Type SoulClass;

        public Intelligence minIntelligence;

        public bool colonistAndPrisonersOnly;

        public float baseLevel = 0.0f;



        [DebuggerHidden]
        public override IEnumerable<string> ConfigErrors()
        {
            NeedDef.< ConfigErrors > c__Iterator73 < ConfigErrors > c__Iterator = new NeedDef.< ConfigErrors > c__Iterator73();

            < ConfigErrors > c__Iterator.<> f__this = this;
            NeedDef.< ConfigErrors > c__Iterator73 expr_0E = < ConfigErrors > c__Iterator;
            expr_0E.$PC = -2;
            return expr_0E;
        }
    }
}
