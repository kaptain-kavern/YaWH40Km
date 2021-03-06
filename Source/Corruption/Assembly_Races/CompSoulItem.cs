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
    public class CompSoulItem : ThingComp
    {
        private float sign;

        public Pawn Owner = new Pawn();

        private Need_Soul soul;

        private bool PsykerPowerAdded = false;

        private CompPsyker LastOwner;

        private Graphic Overlay;

        public CompProperties_SoulItem SProps
        {
            get
            {
                return (CompProperties_SoulItem)this.props;
            }
        }

        public List<PsykerPower> psykerItemPowers = new List<PsykerPower>();

        public void GetOverlayGraphic()
        {
            if (SProps == null)
                Log.Message("NoSprops");

            if (SProps.Category == SoulItemCategories.Corruption)
            {
     //           Log.Message("CorruptionItem");
                this.Overlay = GraphicDatabase.Get<Graphic_Single>("UI/Glow_Corrupt", ShaderDatabase.MetaOverlay, Vector2.one, Color.white);
            }
            else if (SProps.Category == SoulItemCategories.Redemption)
            {
  //              Log.Message("RedemptionItem");
                this.Overlay = GraphicDatabase.Get<Graphic_Single>("UI/Glow_Holy", ShaderDatabase.MetaOverlay, Vector2.one, Color.white);
            }
            else
            {
                Log.Message("NoGraphic");
                return;
            }
        }

        public void UpdatePsykerUnlocks(Need_Soul soul)
        {
            List<PsykerPowerDef> list = SProps.UnlockedPsykerPowers;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].PowerLevel <= soul.PsykerPowerLevel)
                {
                    this.parent.TryGetComp<CompPsyker>().PowerManager.AddPsykerPower(list[i]);
                }
            }            
        }

        public void UpdatePsykerVerbs()
        {
            if (Owner != null)
            {
                ThingWithComps equipment = this.parent as ThingWithComps;
                List<VerbProperties_WarpPower> list = SProps.UnlockedPsykerVerbs;
                for (int i = 0; i < SProps.UnlockedPsykerVerbs.Count; i++)
                {
                    if (list.Any(x => x.ReplacesStandardAttack))
                    {
                        List<VerbProperties> verbs = typeof(ThingDef).GetField("verbs", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(equipment) as List<VerbProperties>;
                        verbs.Clear();
                        if (list[i].ReplacesStandardAttack)
                        {
                            if (verbs != null)
                            {
                                verbs.Add(list[i]);
                            }
                        }
                    }

                }
            }
        }

        public override void PostSpawnSetup()
        {
            if (this.parent.def.tickerType == TickerType.Never)
            {
                this.parent.def.tickerType = TickerType.Rare;
            }
            base.PostSpawnSetup();
        //    Log.Message("GettingOverlay");
            GetOverlayGraphic();
            Find.TickManager.RegisterAllTickabilityFor(this.parent);
        }

        public void CheckForOwner()
        {
            CompEquippable tempcomp;
            Apparel tempthing;
            if (this.parent != null && !this.parent.Spawned)
            {
                //         Log.Message("Begin Check");
                if (this.parent is Apparel)
                {
                    //             Log.Message("Soul item is Apparel");
                    tempthing = this.parent as Apparel;
                    this.Owner = tempthing.wearer;
                }
                else if ((tempcomp = this.parent.TryGetComp<CompEquippable>()) != null && tempcomp.PrimaryVerb.CasterPawn != null)
                {
                    //         Log.Message("IsGun");
                    this.Owner = tempcomp.PrimaryVerb.CasterPawn;
                }
                if ((this.Owner != null))
                {
                    if ((soul = this.Owner.needs.TryGetNeed<Need_Soul>()) != null)
                    {
                        this.CalculateSoulChanges(soul);
                    }
                    if (!PsykerPowerAdded)
                    {
                        CompPsyker compPsyker;
                        if ((compPsyker = Owner.TryGetComp<CompPsyker>()) != null)
                        {
                            for (int i = 0; i < SProps.UnlockedPsykerPowers.Count; i++)
                            {
                                if (soul.PsykerPowerLevel <= SProps.UnlockedPsykerPowers[i].PowerLevel)
                                {
                                    compPsyker.temporaryWeaponPowers.Add(new PsykerPower(Owner, SProps.UnlockedPsykerPowers[i]));
                                }
                            }
                            compPsyker.UpdatePowers();
                            this.LastOwner = compPsyker;
                        }
                        PsykerPowerAdded = true;
                    }

                }
            }
            if (this.parent.Spawned && this.LastOwner != null)
            {
                LastOwner.temporaryWeaponPowers.Clear();
                LastOwner.UpdatePowers();
                PsykerPowerAdded = false;
            }
        }

        public  void CalculateSoulChanges(Need_Soul nsoul)
        {
            float num;
            switch (SProps.Category)
            {
                case (SoulItemCategories.Neutral):
                    {
            //            Log.Message("NeutralItem");
                        sign = 0;
                        break;
                    }
                case (SoulItemCategories.Corruption):
                    {
             //           Log.Message("Corrupted Item");
                        sign = -1;
                        break;
                    }
                case (SoulItemCategories.Redemption):
                    {
      //                  Log.Message("Redemptive Item");
                        sign = 0.5f;
                        break;
                    }
                default:
                    {
                        Log.Error("No Soul Item Category Found");
                        break;
                    }
            }
            num = sign * this.SProps.GainRate * 0.2f / 14000;
 //           Log.Message(num.ToString());
            nsoul.GainNeed(num);
        }       

        public override void PostDrawExtraSelectionOverlays()
        {
            if (Overlay == null) Log.Message("NoOverlay");
            if (Overlay != null)
            {
                Vector3 drawPos = this.parent.DrawPos;
                drawPos.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
                Vector3 s = new Vector3(2.0f, 2.0f, 2.0f);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(drawPos, Quaternion.AngleAxis(0, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, this.Overlay.MatSingle, 0);
            }
        }

        public override void CompTickRare()
        {
      //      Log.Message("CompTick");
            this.CheckForOwner();     
        }
    }
}
