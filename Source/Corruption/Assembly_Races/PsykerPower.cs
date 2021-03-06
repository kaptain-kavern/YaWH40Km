﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    public class PsykerPower : ThingWithComps
    {
        public PsykerPower(Pawn user, PsykerPowerDef pdef)
        {
            this.pawn = user;
            this.powerdef = pdef;
            this.def = pdef;
            this.PowerButton = pdef.uiIcon;
            this.InitializePawnComps(user);
        }

        public void InitializePawnComps(Pawn parent)
        {
            //           Log.Message("Initializng Pawn Comps");
            //           Log.Message(parent.ToString());
            for (int i = 0; i < this.def.comps.Count; i++)
            {
                ThingComp thingComp = (ThingComp)Activator.CreateInstance(this.def.comps[i].compClass);
                //              if (thingComp == null) Log.Message("NoTHingComp");
                thingComp.parent = parent;
                // if (this.comps == null) Log.Message("NoCompslist");

                thingComp.Initialize(this.def.comps[i]);
                this.comps.Add(thingComp);
            }
        }

        public override void PostMake()
        {
            ThingIDMaker.GiveIDTo(this);
        }

        public Pawn pawn;

        private List<ThingComp> comps = new List<ThingComp>();

        public PsykerPowerDef powerdef;

        public CompProperties MainEffectProps;

        public Texture2D PowerButton;

    }
}

    

