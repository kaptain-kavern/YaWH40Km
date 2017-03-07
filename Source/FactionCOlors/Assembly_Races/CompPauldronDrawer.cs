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

        public override void PostDraw()
        {
            if (this.PauldronGraphic != null)
            {
                Mesh mesh = MeshPool.plane10;
                float x = 0f;
                float y = 0.1f;
                float z = 0f;
                if (this.pawn.Rotation == Rot4.North)
                {
                    y += 0.2f;
                }
                else if (this.pawn.Rotation == Rot4.East)
                {
                    mesh = MeshPool.plane10Flip;
                    if (this.padType == ShoulderPadType.Left)
                    {
                        y -= 0.2f;
                    }
                }
                else if (this.pawn.Rotation == Rot4.West && this.padType == ShoulderPadType.Right)
                {
                    y -= 0.2f;
                }

                Vector3 vector = new Vector3(x, y, z);
                GenDraw.DrawMeshNowOrLater(mesh, this.pawn.DrawPos + vector, Quaternion.AngleAxis(0f, Vector3.up), this.PauldronGraphic.MatAt(this.pawn.Rotation, null), false);
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
