using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;
using System;
using System.Collections.Generic;
using Corruption;
using System.Runtime.CompilerServices;

namespace FactionColors
{

    public class ApparelGraphicSet : PawnGraphicSet
    {
        public ApparelGraphicSet(Pawn pawn) : base(pawn)
        {
        }

        private BodyType btype;

        public void ResolveApparelGraphicsOriginal()
        {
            this.ClearCache();
            this.apparelGraphics.Clear();
            List<Apparel> OriginalItems = new List<Apparel>();
            foreach (Apparel current in this.pawn.apparel.WornApparelInDrawOrder)
            {
                ApparelGraphicRecord item;
                if (current.GetComp<CompFactionColor>() != null)
                {
                    if ((ApparelGraphicGetterFC.TryGetGraphicApparelModded(current, this.pawn.story.BodyType, out item)))
                    {
                        if (current.GetComp<ApparelDetailDrawer>() != null && !current.Spawned)
                        {
                            OriginalItems.Add(current);
                        }

                        this.apparelGraphics.Add(item);
                    }



                }

                else if (ApparelGraphicRecordGetter.TryGetGraphicApparel(current, this.pawn.story.BodyType, out item))
                {
                    this.apparelGraphics.Add(item);
                }
            }
            Corruption.AfflictionDrawerUtility.DrawChaosOverlays(this.pawn);
            foreach (Apparel app in OriginalItems)
            {
                ApparelDetailDrawer.DrawDetails(this.pawn, app);
            }
        }


        public void ResolveApparelGraphicsModded()
        {
            this.ClearCache();
            this.apparelGraphics.Clear();


            List<ApparelGraphicRecord> ApparelDetails = new List<ApparelGraphicRecord>();
            List<Apparel> OriginalItems = new List<Apparel>();
            foreach (Apparel current in this.pawn.apparel.WornApparelInDrawOrder)
            {
                ApparelGraphicRecord item;
                if (current.AllComps.Any(i => i.GetType() == typeof(CompFactionColor)))
                {
                    if (this.pawn.def.race.intelligence <= Intelligence.ToolUser)
                    {
                        btype = BodyType.Male;
                    }
                    else
                    {
                        btype = this.pawn.story.BodyType;
                    }
                    if (ApparelGraphicGetterFC.TryGetGraphicApparelModded(current, btype, out item))
                    {
                        this.apparelGraphics.Add(item);
                        if (current.GetComp<ApparelDetailDrawer>() != null && !current.Spawned)
                        {
                            //         current.GetComp<ApparelDetailDrawer>().PostSpawnSetup();
                            //         ApparelGraphicRecord detail;
                            //         ApparelDetailDrawer.ReturnApparelDetails(current, out detail);
                            //         this.apparelGraphics.Add(detail);
                            OriginalItems.Add(current);
                        }
                    }
                }
                else if (current.AllComps.Any(i => i.GetType() == typeof(CompRenderToolUserApparel)))
                {
                    btype = BodyType.Male;

                 //   ApparelGraphicGetterFC.TryGetGraphicApparelModded(current, btype, out item, current.DrawColor, current.DrawColorTwo);
                 //   this.apparelGraphics.Add(item);

                }
                else if (ApparelGraphicRecordGetter.TryGetGraphicApparel(current, this.pawn.story.BodyType, out item))
                {
                    this.apparelGraphics.Add(item);
                }
            }
      //      Corruption.AfflictionDrawerUtility.DrawChaosOverlays(this.pawn);
            foreach (Apparel app in OriginalItems)
            {
                ApparelDetailDrawer.DrawDetails(this.pawn, app);
            }
        }

        public void test()
        {

            List<ApparelGraphicRecord> ApparelDetails = new List<ApparelGraphicRecord>();
            if (ApparelDetails != null)
            {
                for (int i = 0; i < ApparelDetails.Count; i++)
                {
                    Log.Message(ApparelDetails[i].graphic.path);
                    this.apparelGraphics.Add(ApparelDetails[i]);
                }
            }

        }


        public static Rot4 LayingFacingDet(Pawn DownedWearer)
        {
            if (DownedWearer.GetPosture() == PawnPosture.LayingFaceUp)
            {
                return Rot4.South;
            }
            switch (DownedWearer.thingIDNumber % 4)
            {
                case 0:
                    return Rot4.South;
                case 1:
                    return Rot4.South;
                case 2:
                    return Rot4.East;
                case 3:
                    return Rot4.West;
            }
            return Rot4.Random;
        }
    }
}
