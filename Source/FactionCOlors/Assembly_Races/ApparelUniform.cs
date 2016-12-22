using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FactionColors
{
    public class ApparelUniform : Apparel
    {
        public bool FirstSpawned = true;

        public Color Col1 = Color.white;
        public Color Col2 = Color.black;
        public Graphic Detail;

        public override Color DrawColor
        {
            get
            {
;                if (FirstSpawned)
                {
                    FactionDefUniform udef = this.wearer.Faction.def as FactionDefUniform;
                    CompFactionColor compF;
                    if (udef != null)
                    {
                        if ((compF = this.GetComp<CompFactionColor>()) != null && compF.CProps.UseCamouflageColor)
                        {
                //            Log.Message("GettingCamoColor");
                            Col1 = CamouflageColorsUtility.CamouflageColors[0];
                        }
                        else
                        {
          //                  Log.Message("StandardColor");
          //                  Log.Message(udef.FactionColor1.ToString());
                            Col1 = udef.FactionColor1;
                        }
                    }
                    else
                    {
                        CompColorable comp = this.GetComp<CompColorable>();
                        if (comp != null && comp.Active)
                        {
                            Col1 = comp.Color;
                        }
                        else
                        {
                            Col1 = Color.white; 
                        }
                        
                    }                    
                }
                return Col1;
            }

            set
            {
                this.SetColor(value, true);
            }
        }

        public override Color DrawColorTwo
        {
            get
            {
                CompFactionColor compF;
                if (FirstSpawned)
                {
                    FactionDefUniform udef = this.wearer.Faction.def as FactionDefUniform;
                    if (udef != null)
                    {
                        if ((compF = this.GetComp<CompFactionColor>()) != null && compF.CProps.UseCamouflageColor)
                        {
                            Col2 = CamouflageColorsUtility.CamouflageColors[1];
                        }
                        else
                        {
                            Col2 = udef.FactionColor2;
                        }
                    }
                    else
                    {
                        CompColorable comp = this.GetComp<CompColorable>();
                        if (comp != null && comp.Active)
                        {
                            Col2 = comp.Color;
                        }
                        Col2 = Color.white;
                    }
                }
                FirstSpawned = false;
                return Col2;
            }
        }

        public override Graphic Graphic
        {
            get
            {
                return GraphicDatabase.Get<Graphic_Single>(this.def.graphicData.texPath, ShaderDatabase.CutoutComplex, this.def.graphicData.drawSize, this.DrawColor, this.DrawColorTwo);
            }
        }

        public override void PostMake()
        {
            base.PostMake();            
        }

        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);
            if (this.wearer != null)
            {
            }
        }

        public override void DrawWornExtras()
        {
           FirstSpawned = false;
        }

        public override void TickRare()
        {
            base.TickRare();
        }        

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<bool>(ref FirstSpawned, "FirstSpawned", false, false);
            Scribe_Values.LookValue<Color>(ref Col1, "Col1", Color.white, false);
            Scribe_Values.LookValue<Color>(ref Col2, "Col2", Color.white, false);
        }
    }
}
