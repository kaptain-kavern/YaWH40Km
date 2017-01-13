using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FactionColors
{
    public class CompPauldronDrawer : ThingComp
    {
        public string graphicPath;
        public Shader shader;
        public Apparel apparel
        {
            get
            {
                return this.parent as Apparel;
            }
        }

        public Pawn pawn;

        public Graphic PauldronGraphic
        {
            get
            {
                return GraphicDatabase.Get<Graphic_Multi>(graphicPath, shader,  Vector2.one, this.parent.DrawColor, this.parent.DrawColorTwo);
            }
        }

        public CompProperties_PauldronDrawer pprops
        {
            get
            {
                return this.props as CompProperties_PauldronDrawer;
            }
        }

        public override void PostDraw()
        {

        }

        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();
            this.graphicPath = this.pprops.PadTexPath;
            this.shader = ShaderDatabase.ShaderFromType(this.pprops.shaderType);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.LookValue<string>(ref this.graphicPath, "graphicPath", null, false);
            Scribe_Values.LookValue<Shader>(ref this.shader, "shader", ShaderDatabase.Cutout, false);
        }


    }
}
