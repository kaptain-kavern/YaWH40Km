using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    public static class AfflictionDrawerUtility
    {
        
        public static Graphic GetHeadGraphic(Pawn p, string patronname)
        {
            string crownType;
            if (p.story == null)
            {
                crownType = "Head_Average";
            }

            if (p.story.HeadGraphicPath.Contains("Average"))
            {
                crownType = "Head_Average";
            }
            else if (p.story.HeadGraphicPath.Contains("Narrow"))
            {
                crownType = "Head_Narrow";
            }
            else
            {
                Log.Error("Found no CrownType, returning to average");
                crownType = "Head_Average";
            }

            string path = "Things/Chaos/BodyOverlays/" + patronname + "_" + crownType;

            return GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.Mote, Vector2.one, Color.white);
        }

        public static Graphic GetBodyOverlay(BodyType bodyType, string patronname)
        {
            if (bodyType == BodyType.Undefined)
            {
                bodyType = BodyType.Male;
            }
            string str = patronname + "_" + bodyType.ToString();
            string path = "Things/Chaos/BodyOverlays/" + str;
            return GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.CutoutComplex, Vector2.one, Color.white);
        }

        public static void DrawChaosOverlays(Pawn pawn)
        {
                if (pawn.needs != null && pawn.story != null && !pawn.kindDef.factionLeader && pawn.Drawer.renderer.graphics.AllResolved)
                {
                    Need_Soul soul = pawn.needs.TryGetNeed<Need_Soul>();
                    if (soul != null && !soul.NoPatron && soul.patronInfo.PatronName != "Slaanesh")
                    {
                        ApparelGraphicRecord bodyOverlay;
                        ApparelGraphicRecord headOverlay;
                        ApparelGraphicRecord hairExtension;
                        Corruption.AfflictionDrawerUtility.TryGetAfflictionDrawer(pawn, soul, soul.patronInfo.PatronName, pawn.story.bodyType, out bodyOverlay, out headOverlay, out hairExtension);
                        pawn.Drawer.renderer.graphics.apparelGraphics.Insert(0, bodyOverlay);
                        pawn.Drawer.renderer.graphics.apparelGraphics.Insert(1, headOverlay);
                        if (pawn.Drawer.renderer.graphics.apparelGraphics.FindAll(x => x.sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead).Count == 1)
                        {
                            pawn.Drawer.renderer.graphics.apparelGraphics.Insert(2, hairExtension);
                        }
                    }
                }
        }

        public static bool TryGetAfflictionDrawer(Pawn pawn, Need_Soul soul, string patronName, BodyType bodyType, out ApparelGraphicRecord recBody, out ApparelGraphicRecord recHead, out ApparelGraphicRecord recHair)
        {
            if (bodyType == BodyType.Undefined)
            {
                Log.Error("Getting overlay graphic with undefined body type.");
                bodyType = BodyType.Male;
            }
            soul = pawn.needs.TryGetNeed<Need_Soul>();
            Graphic headgraphic = AfflictionDrawerUtility.GetHeadGraphic(pawn, patronName);
            Graphic bodygraphic = AfflictionDrawerUtility.GetBodyOverlay(pawn.story.bodyType, patronName);
            string hairpath = pawn.Drawer.renderer.graphics.hairGraphic.path;
            Graphic oldhairgraphic = GraphicDatabase.Get<Graphic_Multi>(hairpath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);
            Apparel temp1 = new Apparel();
            Apparel temp2 = new Apparel();
            Apparel temp3 = new Apparel();
            temp1.def = CorruptionDefOfs.Overlay_Head;
            temp2.def = CorruptionDefOfs.Overlay_Hair;
            temp3.def = AfflictionDrawerUtility.GetOverlayDef(patronName);
            recHead = new ApparelGraphicRecord(headgraphic, temp1);
            recHair = new ApparelGraphicRecord(oldhairgraphic, temp2);
            recBody = new ApparelGraphicRecord(bodygraphic, temp3);
            return true;            
        }
        
        public static void DrawHeadOverlay(Pawn pawn, string patronName)
        {
            Graphic headmark = AfflictionDrawerUtility.GetHeadGraphic(pawn, patronName);
            Rot4 rot = pawn.Rotation;
            Quaternion quat = pawn.Rotation.AsQuat;
            Vector3 b = quat * pawn.Drawer.renderer.BaseHeadOffsetAt(rot);
            b.y += 0.01f;
            Mesh mesh4 = headmark.MeshAt(rot);
            Material mat2 = headmark.MatAt(rot);
            GenDraw.DrawMeshNowOrLater(mesh4, pawn.DrawPos + b, quat, mat2, false);
        }

        private static ThingDef GetOverlayDef(string patronName)
        {
            switch(patronName)
            {
                case "Undivided":
                    {
                        return CorruptionDefOfs.Overlay_Undivided;
                    }
                case "Khorne":
                    {
                        return CorruptionDefOfs.Overlay_Khorne;
                    }
                case "Nurgle":
                    {
                        return CorruptionDefOfs.Overlay_Nurgle;
                    }
                case "Tzeentch":
                    {
                        return CorruptionDefOfs.Overlay_Tzeentch;
                    }
            }

            return CorruptionDefOfs.Overlay_Undivided;

        }
        
    }
}
