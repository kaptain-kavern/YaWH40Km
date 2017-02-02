﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Corruption
{
    public class CompEldarSpiritStone : ThingComp
    {
        private Pawn Eldar;

        private IntVec3 curpos = new IntVec3();

        private bool IsSpawned;

        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();
            if (this.parent is Pawn)
            {
                this.Eldar = this.parent as Pawn;
            }
        }

        public override void PostDeSpawn(Map map)
        {
            if (!IsSpawned)
            {
                Thing spiritstone = ThingMaker.MakeThing(CorruptionDefOfs.SpiritStone);
                GenSpawn.Spawn(spiritstone, curpos, map);
                IsSpawned = true;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.parent.Spawned)
            {
                this.curpos = Eldar.Position;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.LookValue<bool>(ref this.IsSpawned, "IsSpawned", true, false);
        }
    }
}
