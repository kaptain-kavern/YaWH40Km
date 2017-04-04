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
        public Shader shader = ShaderDatabase.Cutout;
        public ShoulderPadType padType;
        public Apparel apparel
        {
            get
            {
                return this.parent as Apparel;
            }
        }

        public Pawn pawn
        {
            get
            {
                return this.apparel.wearer;
            }
        }

        public Graphic PauldronGraphic
        {
            get
            {
                return GraphicDatabase.Get<Graphic_Multi>(graphicPath + "_" + pawn.story.bodyType.ToString(), shader,  Vector2.one, this.parent.DrawColor, this.parent.DrawColorTwo);
            }
        }

        public CompProperties_PauldronDrawer pprops
        {
            get
            {
                return this.props as CompProperties_PauldronDrawer;
            }
        }

        public static bool ShouldDrawPauldron(Pawn pawn, Apparel curr, Rot4 bodyFacing, out Material pauldronMaterial)
        {
            pauldronMaterial = null;
            try
            {
                if (pawn.needs != null && pawn.story != null)
                {
                    CompPauldronDrawer drawer;
                    if ((drawer = curr.TryGetComp<CompPauldronDrawer>()) != null)
                    {
                        drawer.PostSpawnSetup();
                        if (drawer.PauldronGraphic != null)
                        {
                            pauldronMaterial = drawer.PauldronGraphic.MatAt(bodyFacing);
                            return true;
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }

        }

        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();
            this.graphicPath = this.pprops.PadTexPath;
            this.shader = ShaderDatabase.ShaderFromType(this.pprops.shaderType);
            this.padType = this.pprops.shoulderPadType;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.LookValue<string>(ref this.graphicPath, "graphicPath", null, false);
            Scribe_Values.LookValue<Shader>(ref this.shader, "shader", ShaderDatabase.Cutout, false);
        }


    }
}
