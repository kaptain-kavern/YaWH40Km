using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace Corruption
{
    public class HediffComp_NeuroController : HediffComp, IThingContainerOwner
    {
        public ThingContainer innerContainer;

        public HediffComp_NeuroController()
        {
            this.innerContainer = new ThingContainer(this, false);
        }

        public bool Spawned
        {
            get
            {
                return this.Pawn.Spawned;
            }
        }

        public ThingContainer GetInnerContainer()
        {
            return this.innerContainer;
        }
        
        public IntVec3 GetPosition()
        {
            return this.Pawn.PositionHeld;
        }

        public Map GetMap()
        {
            return this.Pawn.MapHeld;
        }
    }
}
