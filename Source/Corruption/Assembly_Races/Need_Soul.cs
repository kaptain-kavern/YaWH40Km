﻿using FactionColors;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace Corruption
{
    public class Need_Soul : Need
    {
        private bool IsImmune;

        private const float ThreshCorrupted = 0.1f;

        private const float ThreshTainted = 0.3f;

        private const float ThreshWarptouched = 0.5f;

        private const float ThreshIntrigued = 0.7f;

        private const float ThreshPure = 0.9f;

        public CompPsyker compPsyker = new CompPsyker();

        public PsykerPowerLevel PsykerPowerLevel;

        public List<SoulTrait> SoulTraits = new List<SoulTrait>();

        public List<PsykerPower> psykerPowers = new List<PsykerPower>();

        public ChaosGods Patron;

        public CulturalToleranceCategory CulturalTolerance;

        public PatronInfo patronInfo = new PatronInfo();

        public AfflictionProperty PawnAfflictionProps;

        public SoulTrait DevotionTrait;

        public SoulTrait PatronTrait;

        public bool NoPatron = true;

        public List<Pawn> OpposingDevotees = new List<Pawn>();

        private ChaosFollowerPawnKindDef cdef
        {
            get
            {
                return this.pawn.kindDef as ChaosFollowerPawnKindDef;
            }
        }        

        public SoulAffliction CurCategory
        {
            get
            {
                if (this.curLevelInt <= 0.0f)
                {
                    return SoulAffliction.Lost;
                }
                if (this.curLevelInt < 0.3f)
                {
                    return SoulAffliction.Corrupted;
                }
                if (this.curLevelInt < 0.4f)
                {
                    return SoulAffliction.Tainted;
                }
                if (this.curLevelInt < 0.65f)
                {
                    return SoulAffliction.Warptouched;
                }
                if (this.curLevelInt < 0.85f)
                {
                    return SoulAffliction.Intrigued;
                }
                if (this.curLevelInt <= 0.85f)
                {
                    return SoulAffliction.Pure;
                }
                return SoulAffliction.Pure;
            }
        }

        public Need_Soul(Pawn pawn) : base(pawn)
        {
            this.threshPercents = new List<float>();
            this.threshPercents.Add(0.1f);
            this.threshPercents.Add(0.3f);
            this.threshPercents.Add(0.5f);
            this.threshPercents.Add(0.7f);
            this.threshPercents.Add(0.9f);
        }

        public override void SetInitialLevel()
        {
            InitiatePsykerComp();
 //           try
 //           {
 //               AddPsykerPower(PsykerPowerDefOf.PsykerPower_Berserker);
 //               AddPsykerPower(PsykerPowerDefOf.PsykerPower_WarpBolt);
 //               AddPsykerPower(PsykerPowerDefOf.PsykerPower_Temptation);
 //               AddPsykerPower(PsykerPowerDefOf.PsykerPower_Purgatus);
 //           }
 //           catch
 //           { }
            ChaosFollowerPawnKindDef pdef = this.pawn.kindDef as ChaosFollowerPawnKindDef;

            if (pdef != null && pdef.AfflictionProperty != null)
            {
                PawnAfflictionProps = new AfflictionProperty();
                this.PawnAfflictionProps = pdef.AfflictionProperty;
                int pllow = (int)this.PawnAfflictionProps.LowerPsykerPowerLimit;
                int plup = (int)this.PawnAfflictionProps.UpperAfflictionLimit;
                this.PsykerPowerLevel = (PsykerPowerLevel)Rand.RangeInclusive(pllow, plup);

                if (PawnAfflictionProps.IsImmune)
                {
                    this.curLevelInt = 0.99f;
                    this.DevotionTrait = new SoulTrait(CorruptionDefOfs.Devotion, PawnAfflictionProps.ImmuneDevotionDegree);
                    this.IsImmune = true;
                }
                else if (this.PsykerPowerLevel == PsykerPowerLevel.Omega)
                {
                    this.IsImmune = true;
                    this.curLevelInt = Rand.Range(0.86f, 0.99f);
                    this.NoPatron = true;
                }
                else
                {
                    float afup = pdef.AfflictionProperty.UpperAfflictionLimit;
                    float afdown = pdef.AfflictionProperty.LowerAfflictionLimit;
                    this.curLevelInt = (Rand.Range(afup, afdown));
                }
                if (PawnAfflictionProps.UseOtherFaith)
                {
                    this.patronInfo.PatronName = PawnAfflictionProps.IsofFaith.ToString();
                }
                this.CulturalTolerance = PawnAfflictionProps.PrimaryToleranceCategory;
            }
            else
            {
                PawnAfflictionProps = new AfflictionProperty();
                this.PsykerPowerLevel = (PsykerPowerLevel)Rand.RangeInclusive(0, Enum.GetValues(typeof(PsykerPowerLevel)).Length - 1);
                this.CulturalTolerance = (CulturalToleranceCategory)Rand.RangeInclusive(0, 2);
                if (this.PsykerPowerLevel == PsykerPowerLevel.Omega)
                {
                    this.IsImmune = true;
                    this.curLevelInt = Rand.Range(0.86f, 0.99f);
                    this.NoPatron = true;
                }
                else
                {
                    this.curLevelInt = Rand.Range(0.01f, 0.99f);
                }
            }

            if (this.PawnAfflictionProps.CommmonPsykerPowers != null)
            {
                for (int i=0; i < this.PawnAfflictionProps.CommmonPsykerPowers.Count; i++)
                {
                    
                    try
                    {
                        this.compPsyker.PowerManager.AddPsykerPower(this.PawnAfflictionProps.CommmonPsykerPowers[i]);
                    }
                    catch
                    { }
                }
            }

            if (CorruptionDefOfs.Devotion.SDegreeDatas == null) Log.Message("No stdata");

            if (this.SoulTraits.NullOrEmpty())
            {
                if ((PawnAfflictionProps != null && PawnAfflictionProps.IsImmune))
                {
                    this.DevotionTrait = new SoulTrait(CorruptionDefOfs.Devotion, PawnAfflictionProps.ImmuneDevotionDegree);
                    this.IsImmune = true;
                }
                else
                {
                    this.DevotionTrait = new SoulTrait(CorruptionDefOfs.Devotion, Rand.RangeInclusive(-2, 2));
                }
                this.SoulTraits.Insert(0, DevotionTrait);
            }
            if (PawnAfflictionProps.CommonSoulTrait != null && !this.SoulTraits.Any(x => x.SDef == PawnAfflictionProps.CommonSoulTrait))
            {
                this.SoulTraits.Add(new SoulTrait(PawnAfflictionProps.CommonSoulTrait, 0));
            }

            if (this.curLevelInt < 0.3f && NoPatron == true)
            {
                GainPatron(ChaosGods.Undivided, false);

            }
            if (NoPatron == false)
            {
                if (curLevelInt > 0.3f)
                {
                    curLevelInt = 0.3f;
                }
            }
            if (this.PsykerPowerLevel == PsykerPowerLevel.Omega)
            {
                this.IsImmune = true;
            }
        }

        public override void NeedInterval()
        {
            if (this.curLevelInt < 0f)
            {
                this.curLevelInt = 0f;
            }
            if (this.curLevelInt < 0.3f && NoPatron == true)
            {
                GainPatron(ChaosGods.Undivided, false);

                if (pawn.Faction == Faction.OfPlayer)
                {
                    string label = "LetterPatronGained".Translate();
                    string text2 = "LetterPatronGained_Content".Translate(new object[]
                    {
                    this.pawn.LabelShort,
                    this.Patron.ToString()
                    });
                    Find.LetterStack.ReceiveLetter(label, text2, LetterType.BadNonUrgent, this.pawn, null);
                }
            }
            if (NoPatron == false)
            {                
                if (curLevelInt > 0.3f)
                {
                    curLevelInt = 0.3f;
                }
                if (Rand.Range(0f, 0.4f) > (this.pawn.needs.mood.CurLevel + this.CurLevel) && !this.pawn.InMentalState)
                {
                    MentalStateDef mdef;
                    switch (this.patronInfo.PatronName)
                    {
                        case "Khorne":
                            {
                                mdef = ChaosGodsUtilities.KhorneEffects(this.pawn, this);
                                ChaosGodsUtilities.DoEffectOn(this.pawn, mdef);
                                break;
                            }
                        case "Slaanesh":
                            {
                                mdef = ChaosGodsUtilities.SlaaneshEffects(this.pawn, this);
                                ChaosGodsUtilities.DoEffectOn(this.pawn, mdef);
                                break;
                            }

                        default:
                            {
                                break;
                            }
                    }
                }
            }
            if (curLevelInt > 1f)
            {
                curLevelInt = 0.99999f;
            }            
        }
        
        public void GainNeed(float amount)
        {
            if (this.cdef != null)
            {
                if (this.IsImmune)
                {
                    return;
                }              
                amount = cdef.AfflictionProperty.ResolveFactor * amount;
            }

            this.curLevelInt += amount;
        }

        public void InitiatePsykerComp()
        {
            CompPsyker compPsyker = new CompPsyker();
            compPsyker.parent = this.pawn;
            CompProperties_PsykerVerb cprops = new CompProperties_PsykerVerb();
            cprops.compClass = typeof(CompPsyker);
            compPsyker.Initialize(cprops);
            FieldInfo info = typeof(ThingWithComps).GetField("comps", BindingFlags.NonPublic | BindingFlags.Instance);
            if (info != null)
            {
                List<ThingComp> list = info.GetValue(this.pawn) as List<ThingComp>;
                list.Add(compPsyker);
                typeof(ThingWithComps).GetField("comps", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this.pawn, list);
            }
            compPsyker.PostSpawnSetup();
            this.compPsyker = compPsyker;
        }



        public void GeneratePatronTraits(Pawn tpawn)
        {
            patronInfo.GetPatronTraits(patronInfo.PatronName);
            this.PatronTrait = patronInfo.PatronTraits[0];
        }

        public void GainPatron(ChaosGods forcedPatron, bool UseForcedPatron)
        {

            if (PawnAfflictionProps == null)
            {
                PawnAfflictionProps = new AfflictionProperty();
                PawnAfflictionProps.Patron = ChaosGods.Undivided;
            }

            if (UseForcedPatron)
            {
                PawnAfflictionProps.Patron = forcedPatron;
                this.Patron = PawnAfflictionProps.Patron;
            }
            else
            {

                if (pawn.Faction.def.GetType() == typeof(FactionDefUniform))
                {
                    FactionDefUniform Facdef = this.pawn.Faction.def as FactionDefUniform;

                    if (Facdef.PreferredChaosGod == ChaosGods.Undivided)
                    {
                        PawnAfflictionProps.Patron = ChaosGods.Undivided;
                    }
                    else
                    {
                        PawnAfflictionProps.Patron = Facdef.PreferredChaosGod;
                    }
                }

                if (PawnAfflictionProps == null)
                {
                    PawnAfflictionProps = new AfflictionProperty();
                    PawnAfflictionProps.Patron = ChaosGods.Undivided;
                }

                if (PawnAfflictionProps.Patron == ChaosGods.Undivided)
                {
                    if (Rand.Range(0.1f, 1f) > 0.5f)
                    {
                        this.Patron = ChaosGods.Undivided;
                    }
                    else
                    {
                        this.Patron = (ChaosGods)Rand.RangeInclusive(1, 4);
                    }
                }
                else
                {
                    this.Patron = PawnAfflictionProps.Patron;
                }
            }

            patronInfo.PatronName = Patron.ToString();
            GeneratePatronTraits(pawn);
            this.SoulTraits.Remove(DevotionTrait);
            DevotionTrait = new SoulTrait(CorruptionDefOfs.Devotion, Rand.RangeInclusive(0, 2));
            this.SoulTraits.Add(DevotionTrait);
            if (pawn.story == null)
            {
                pawn.story = new Pawn_StoryTracker(pawn);
            }
   //         Hediff chaosmark = HediffMaker.MakeHediff(ChaosMarkDef(this.patronInfo.PatronName), this.pawn,null);
   //         this.pawn.health.hediffSet.AddHediffDirect(chaosmark);
            if (this.pawn.Drawer != null && this.pawn.story != null)
            {
                if (this.pawn.Drawer.renderer != null)
                {
                    if (this.pawn.Drawer.renderer.graphics != null && this.pawn.apparel != null)
                    {
                        LongEventHandler.ExecuteWhenFinished(delegate
                        {
                            this.pawn.Drawer.renderer.graphics.ResolveApparelGraphics();
                            PortraitsCache.SetDirty(this.pawn);
                        });
                    }
                }
            }
            this.NoPatron = false;
        }

        public string PatronName(Pawn pawn)
        {

            if (NoPatron)
            {
                return "Emperor";
            }
            else
            {
                return pawn.needs.TryGetNeed<Need_Soul>().Patron.ToString();
            }
        }

        public override void DrawOnGUI(Rect rect)
        {
            if (rect.height > 70f)
            {
                float num = (rect.height - 70f) / 2f;
                rect.height = 70f;
                rect.y += num;
            }
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
            }

            if (Event.current.type == EventType.MouseDown && Mouse.IsOver(rect))
            {
                Find.WindowStack.Add(new MainTabWindow_Alignment());
            }
            TooltipHandler.TipRegion(rect, new TipSignal(() => this.GetTipString(), rect.GetHashCode()));
            float num2 = 14f;
            float num3 = num2 + 15f;
            if (rect.height < 50f)
            {
                num2 *= Mathf.InverseLerp(0f, 50f, rect.height);
            }
            Text.Font = ((rect.height <= 55f) ? GameFont.Tiny : GameFont.Small);
            Text.Anchor = TextAnchor.LowerLeft;
            Rect rect2 = new Rect(rect.x + num3 + rect.width * 0.1f, rect.y, rect.width - num3 - rect.width * 0.1f, rect.height / 2f);
            Widgets.Label(rect2, this.LabelCap);
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect3 = new Rect(rect.x, rect.y + rect.height / 2f, rect.width, rect.height / 2f);
            rect3 = new Rect(rect3.x + num3, rect3.y, rect3.width - num3 * 2f, rect3.height - num2);
            Widgets.FillableBar(rect3, this.CurLevelPercentage);
            Widgets.FillableBarChangeArrows(rect3, this.GUIChangeArrow);
            if (this.threshPercents != null)
            {
                for (int i = 0; i < this.threshPercents.Count; i++)
                {
                    this.DrawBarThreshold(rect3, this.threshPercents[i]);
                }
            }
            float curInstantLevelPercentage = this.CurInstantLevelPercentage;
            if (curInstantLevelPercentage >= 0f)
            {
                this.DrawBarInstantMarkerAt(rect3, curInstantLevelPercentage);
            }
            Text.Font = GameFont.Small;
        }

        private void DrawBarThreshold(Rect barRect, float threshPct)
        {
            float num = (float)((barRect.width <= 60f) ? 1 : 2);
            Rect position = new Rect(barRect.x + barRect.width * threshPct - (num - 1f), barRect.y + barRect.height / 2f, num, barRect.height / 2f);
            Texture2D image;
            if (threshPct < this.CurLevelPercentage)
            {
                image = BaseContent.BlackTex;
                GUI.color = new Color(1f, 1f, 1f, 0.9f);
            }
            else
            {
                image = BaseContent.GreyTex;
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
            }
            GUI.DrawTexture(position, image);
            GUI.color = Color.white;
        }

        public override void ExposeData()
        {
            base.ExposeData();
    //        Scribe_Deep.LookDeep<PatronInfo>(ref this.patronInfo, "patronInfo", new object());
            Scribe_Values.LookValue<bool>(ref this.NoPatron, "NoPatron", true, false);
            Scribe_Values.LookValue<bool>(ref this.IsImmune, "IsImmune", false, false);
            Scribe_Values.LookValue<string>(ref this.patronInfo.PatronName, "PatronName", "Emperor", false);


            //        Scribe_Collections.LookList<SoulTrait>(ref this.SoulTraits, "SoulTraits", LookMode.Deep, new object[0]);
            Scribe_Collections.LookList<Pawn>(ref this.OpposingDevotees, "OpposingDevotees", LookMode.Deep, new object[0]);

            Scribe_Values.LookValue<ChaosGods>(ref this.Patron, "Patron", ChaosGods.Undivided, false);

   //         Scribe_Values.LookValue<PsykerPowerLevel>(ref this.PsykerPowerLevel, "PsykerPowerLevel", PsykerPowerLevel.Rho, false);
            Scribe_Values.LookValue<CulturalToleranceCategory>(ref this.CulturalTolerance, "CulturalTolerance", CulturalToleranceCategory.Neutral, false);

    //        Scribe_Deep.LookDeep<AfflictionProperty>(ref this.PawnAfflictionProps, "PawnAfflictionProps", null);
            Scribe_Deep.LookDeep<SoulTrait>(ref this.DevotionTrait, "DevotionTrait", new object[]
            {
                CorruptionDefOfs.Devotion,
                this.DevotionTrait.SDegree
                
            });
        }
    }
}
