using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FactionColors
{
    [StaticConstructorOnStartup]
    public class ApparelDetailDrawer : ThingComp
    {
        private bool FirstSpawn = true;

        private ApparelDetail appDetailInt;

        public ApparelDetail AppDetail
        {
            get
            {
                if (FirstSpawn)
                {
                    HasDetail = AppProps.DetailChance >= Rand.Range(0.1f, 0.9f);
                    //          Log.Message("CheckingDetail");
                    if (HasDetail)
                    { 
                        appDetailInt = AppProps.ApparelDetails.RandomElementByWeight((ApparelDetail hd) => hd.Commonality);
                        FirstSpawn = false;
                        return appDetailInt;
                    }
                }
                return appDetailInt;
            }
        }

        private Graphic detailGraphicInt;

        public Graphic DetailGraphic
        {
            get
            {
                if(this.AppDetail!= null && this.apparel.wearer == null)
                {
                    detailGraphicInt = GraphicDatabase.Get<Graphic_Multi>(AppDetail.DetailGraphicPath, ShaderDatabase.CutoutComplex, drawSize, parent.DrawColor, parent.DrawColorTwo);                    
                }
                else if(this.AppDetail != null && this.apparel.wearer != null)
                {
                    string path;
                    if (this.apparel.def.apparel.LastLayer == ApparelLayer.Overhead)
                    {
                        path = this.AppDetail.DetailGraphicPath;
                    }
                    else
                    {
                        path = this.AppDetail.DetailGraphicPath + "_" + this.apparel.wearer.story.bodyType.ToString();
                    }
                    detailGraphicInt = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.CutoutComplex, drawSize, parent.DrawColor, parent.DrawColorTwo);
                }
                return detailGraphicInt;
            }
        }

        public bool HasDetail = false;

        private Vector2 drawSize = new Vector2(2f, 2f);

        private string texPath;


        public ApparelDetailProps AppProps
        {
            get
            {
                return (ApparelDetailProps)this.props;
            }
        }
        
        private Apparel apparel
        {
            get
            {
               return this.parent as Apparel;
            }
        }        

        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();
            if (this.AppDetail == null) Log.Message("NoAppdetail");
            if (this.DetailGraphic == null) Log.Message("NoAppGraphic");
            InitiateDetails();
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if ((this.apparel!= null) && (apparel.wearer == null))
            {
                Vector3 vector = this.parent.DrawPos;
                vector.y += 0.005f;
                Vector3 s = new Vector3(1.4f, 1f, 1.4f);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(vector, Quaternion.AngleAxis(this.parent.Rotation.AsInt, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, DetailGraphic.MatSingle, 0);
            }
        }

        public void InitiateDetails()
        {
            if (FirstSpawn)
            {
                HasDetail = AppProps.DetailChance >= Rand.Range(0.1f, 0.9f);
      //          Log.Message("CheckingDetail");
                if (HasDetail)
                {
                    appDetailInt = AppProps.ApparelDetails.RandomElementByWeight((ApparelDetail hd) => hd.Commonality);
     //               this.DetailGraphic = GraphicDatabase.Get<Graphic_Multi>(AppDetail.DetailGraphicPath, ShaderDatabase.CutoutComplex, drawSize, parent.DrawColor, parent.DrawColorTwo);
     //               Log.Message("HasDetail");
                }
            }
            FirstSpawn = false;
        }
 

        public static bool ReturnApparelDetails(Apparel curr, out ApparelGraphicRecord result)
        {
            ApparelDetailDrawer drawer;
            if((drawer = curr.TryGetComp<ApparelDetailDrawer>()) != null)
            {
          //      Log.Message("Checking Available Details");
                if (drawer.HasDetail)
                {
        //            Log.Message("Found Detail");
                    ApparelGraphicRecord recDetail;
                    if (ApparelDetailDrawer.TryGetApparelDetails(curr, drawer.DetailGraphic, out recDetail))
                    {
          //              Log.Message("Gotten ApparelDetailRecord");
                        result = recDetail;
                        return true;
                    }
                }
            }
            result = new ApparelGraphicRecord();
            return false;
        }

        public static bool TryGetApparelDetails(Apparel curr, Graphic detailgraphic, out ApparelGraphicRecord recDetail)
        {
  //          Log.Message("Trying to get GraphicRecord");
            Apparel temp1 = new Apparel();
            if (curr.def.apparel.LastLayer == ApparelLayer.Overhead)
            {
                temp1.def = FactionColorsDefOf.Overlay_Headgear;
            }
            else
            {
                temp1.def = FactionColorsDefOf.Overlay_Body;
            }
 //           Log.Message("GraphicRecord of DEF: "+ temp1.def.ToString());
            recDetail = new ApparelGraphicRecord(detailgraphic, temp1);
            return true;
        }


        public static void DrawDetails(Pawn pawn, Apparel curr)
        {
            try
            {
                if (pawn.needs != null && pawn.story != null && !pawn.kindDef.factionLeader)
                {
                    ApparelDetailDrawer drawer;
                    if ((drawer = curr.TryGetComp<ApparelDetailDrawer>()) != null)
                    {
                        drawer.PostSpawnSetup();
                        if (drawer.HasDetail)
                        {
                            ApparelGraphicRecord recDetail;
                            if (ApparelDetailDrawer.TryGetApparelDetails(curr, drawer.DetailGraphic, out recDetail))
                            {
  //                              Log.Message("Inserting Detail");
                                pawn.Drawer.renderer.graphics.apparelGraphics.Add(recDetail);
                            }
                        }
                    }
                }
            }
            catch
            {
            }

        }


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.LookValue<bool>(ref HasDetail, "HasDetail", true, false);
            Scribe_Values.LookValue<bool>(ref FirstSpawn, "FirstSpawn", false, false);
            Scribe_Values.LookValue<string>(ref this.texPath, "texPath", null, false);
        }     

    }
}

