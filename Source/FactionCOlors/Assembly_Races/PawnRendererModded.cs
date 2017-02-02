using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using System.Reflection;

namespace FactionColors
{
    [StaticConstructorOnStartup]
    public class PawnRendererModded
    {

        private const float CarriedThingDrawAngle = 16f;

        private const float SubInterval = 0.005f;

        private const float YOffset_PrimaryEquipmentUnder = 0f;

        private const float YOffset_Body = 0.005f;

        private const float YOffsetInterval_Clothes = 0.005f;

        private const float YOffset_Wounds = 0.02f;

        private const float YOffset_Shell = 0.0249999985f;

        private const float YOffset_Head = 0.03f;

        private const float YOffset_OnHead = 0.035f;

        private const float YOffset_CarriedThing = 0.04f;

        private const float YOffset_PrimaryEquipmentOver = 0.04f;

        private const float YOffset_Status = 0.0449999981f;

        private const float UpHeadOffset = 0.34f;

        private Pawn pawn;

        public PawnGraphicSet graphics;

        public PawnDownedWiggler wiggler;

        private PawnHeadOverlays statusOverlays;
        
        private PawnWoundDrawer woundOverlays;

        private static readonly float[] HorHeadOffsets = new float[]
        {
            0f,
            0.04f,
            0.1f,
            0.09f,
            0.1f,
            0.09f
        };

        public PawnRendererModded(Pawn pawn)
        {
            this.pawn = pawn;
            this.wiggler = new PawnDownedWiggler(pawn);
            this.statusOverlays = new PawnHeadOverlays(pawn);
            this.woundOverlays = typeof(PawnRenderer).GetField("woundOverlays", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(pawn) as PawnWoundDrawer;
            this.graphics = new PawnGraphicSet(pawn);
        }
        

        public void RenderPortait()
        {
            Vector3 zero = Vector3.zero;
            Quaternion quat;
            if (this.pawn.Dead || this.pawn.Downed)
            {
                quat = Quaternion.Euler(0f, 85f, 0f);
                zero.x -= 0.18f;
                zero.z -= 0.18f;
            }
            else
            {
                quat = Quaternion.identity;
            }
            RotDrawMode bodyDrawType = RotDrawMode.Fresh;
            if (this.pawn.Dead && this.pawn.Corpse != null)
            {
                bodyDrawType = this.pawn.Corpse.CurRotDrawMode;
            }
            this.RenderPawnInternal(zero, quat, true, Rot4.South, Rot4.South, bodyDrawType, true);
        }

        private void RenderPawnInternal(Vector3 rootLoc, Quaternion quat, bool renderBody, RotDrawMode draw = RotDrawMode.Fresh)
        {
            this.RenderPawnInternal(rootLoc, quat, renderBody, this.pawn.Rotation, this.pawn.Rotation, draw, false);
        }

        private void RenderPawnInternal(Vector3 rootLoc, Quaternion quat, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType = RotDrawMode.Fresh, bool portrait = false)
        {
            if (!this.graphics.AllResolved)
            {
                this.graphics.ResolveAllGraphics();
            }
            Mesh mesh = null;
            if (renderBody)
            {
                Vector3 loc = rootLoc;
                loc.y += 0.005f;
                if (bodyDrawType == RotDrawMode.Dessicated && !this.pawn.RaceProps.Humanlike && this.graphics.dessicatedGraphic != null && !portrait)
                {
                    this.graphics.dessicatedGraphic.Draw(loc, bodyFacing, this.pawn);
                }
                else
                {
                    if (this.pawn.RaceProps.Humanlike)
                    {
                        mesh = MeshPool.humanlikeBodySet.MeshAt(bodyFacing);
                    }
                    else
                    {
                        mesh = this.graphics.nakedGraphic.MeshAt(bodyFacing);
                    }
                    List<Material> list = this.graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
                    for (int i = 0; i < list.Count; i++)
                    {
                        Material damagedMat = this.graphics.flasher.GetDamagedMat(list[i]);
                        GenDraw.DrawMeshNowOrLater(mesh, loc, quat, damagedMat, portrait);
                        loc.y += 0.005f;
                    }
                    if (bodyDrawType == RotDrawMode.Fresh)
                    {
                        Vector3 drawLoc = rootLoc;
                        drawLoc.y += 0.02f;
                        if (mesh == null) Log.Message("noMesh");
                        if (quat == null) Log.Message("noquat");
                        if (drawLoc == null) Log.Message("drawLoc");
                        this.woundOverlays.RenderOverBody(drawLoc, mesh, quat, portrait);
                    }
                }
            }
            Vector3 vector = rootLoc;
            Vector3 a = rootLoc;
            if (bodyFacing != Rot4.North)
            {
                a.y += 0.03f;
                vector.y += 0.0249999985f;
            }
            else
            {
                a.y += 0.0249999985f;
                vector.y += 0.03f;
            }
            if (this.graphics.headGraphic != null)
            {
                Vector3 b = quat * this.BaseHeadOffsetAt(headFacing);
                Mesh mesh2 = MeshPool.humanlikeHeadSet.MeshAt(headFacing);
                Material mat = this.graphics.HeadMatAt(headFacing, bodyDrawType);
                GenDraw.DrawMeshNowOrLater(mesh2, a + b, quat, mat, portrait);
                Vector3 loc2 = rootLoc + b;
                loc2.y += 0.035f;
                bool flag = false;
                Mesh mesh3 = this.graphics.HairMeshSet.MeshAt(headFacing);
                List<ApparelGraphicRecord> apparelGraphics = this.graphics.apparelGraphics;
                for (int j = 0; j < apparelGraphics.Count; j++)
                {
                    if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead)
                    {
                        flag = true;
                        Material material = apparelGraphics[j].graphic.MatAt(bodyFacing, null);
                        material = this.graphics.flasher.GetDamagedMat(material);
                        GenDraw.DrawMeshNowOrLater(mesh3, loc2, quat, material, portrait);
                    }
                }
                if (!flag && bodyDrawType != RotDrawMode.Dessicated)
                {
                    Mesh mesh4 = this.graphics.HairMeshSet.MeshAt(headFacing);
                    Material mat2 = this.graphics.HairMatAt(headFacing);
                    GenDraw.DrawMeshNowOrLater(mesh4, loc2, quat, mat2, portrait);
                }
            }
            if (renderBody)
            {
                for (int k = 0; k < this.graphics.apparelGraphics.Count; k++)
                {
                    ApparelGraphicRecord apparelGraphicRecord = this.graphics.apparelGraphics[k];
                    if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayer.Shell)
                    {
                        Material material2 = apparelGraphicRecord.graphic.MatAt(bodyFacing, null);
                        material2 = this.graphics.flasher.GetDamagedMat(material2);
                        GenDraw.DrawMeshNowOrLater(mesh, vector, quat, material2, portrait);
                    }
                }
            }
            if (!portrait && this.pawn.RaceProps.Animal && this.pawn.inventory != null && this.pawn.inventory.innerContainer.Count > 0)
            {
                Graphics.DrawMesh(mesh, vector, quat, this.graphics.packGraphic.MatAt(this.pawn.Rotation, null), 0);
            }
            if (!portrait)
            {
                this.DrawEquipment(rootLoc);
                if (this.pawn.apparel != null)
                {
                    List<Apparel> wornApparel = this.pawn.apparel.WornApparel;
                    for (int l = 0; l < wornApparel.Count; l++)
                    {
                        wornApparel[l].DrawWornExtras();
                    }
                }
                Vector3 bodyLoc = rootLoc;
                bodyLoc.y += 0.0449999981f;
                this.statusOverlays.RenderStatusOverlays(bodyLoc, quat, MeshPool.humanlikeHeadSet.MeshAt(headFacing));
            }
        }

        private void DrawEquipment(Vector3 rootLoc)
        {
            if (this.pawn.Dead || !this.pawn.Spawned)
            {
                return;
            }
            if (this.pawn.equipment == null || this.pawn.equipment.Primary == null)
            {
                return;
            }
            if (this.pawn.CurJob != null && this.pawn.CurJob.def.neverShowWeapon)
            {
                return;
            }
            Stance_Busy stance_Busy = this.pawn.stances.curStance as Stance_Busy;
            if (stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid)
            {
                Vector3 a;
                if (stance_Busy.focusTarg.HasThing)
                {
                    a = stance_Busy.focusTarg.Thing.DrawPos;
                }
                else
                {
                    a = stance_Busy.focusTarg.Cell.ToVector3Shifted();
                }
                float num = 0f;
                if ((a - this.pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                {
                    num = (a - this.pawn.DrawPos).AngleFlat();
                }
                Vector3 drawLoc = rootLoc + new Vector3(0f, 0f, 0.4f).RotatedBy(num);
                drawLoc.y += 0.04f;
                this.DrawEquipmentAiming(this.pawn.equipment.Primary, drawLoc, num);
            }
            else if (this.CarryWeaponOpenly())
            {
                if (this.pawn.Rotation == Rot4.South)
                {
                    Vector3 drawLoc2 = rootLoc + new Vector3(0f, 0f, -0.22f);
                    drawLoc2.y += 0.04f;
                    this.DrawEquipmentAiming(this.pawn.equipment.Primary, drawLoc2, 143f);
                }
                else if (this.pawn.Rotation == Rot4.North)
                {
                    Vector3 drawLoc3 = rootLoc + new Vector3(0f, 0f, -0.11f);
            //        drawLoc3.y = drawLoc3.y;
                    this.DrawEquipmentAiming(this.pawn.equipment.Primary, drawLoc3, 143f);
                }
                else if (this.pawn.Rotation == Rot4.East)
                {
                    Vector3 drawLoc4 = rootLoc + new Vector3(0.2f, 0f, -0.22f);
                    drawLoc4.y += 0.04f;
                    this.DrawEquipmentAiming(this.pawn.equipment.Primary, drawLoc4, 143f);
                }
                else if (this.pawn.Rotation == Rot4.West)
                {
                    Vector3 drawLoc5 = rootLoc + new Vector3(-0.2f, 0f, -0.22f);
                    drawLoc5.y += 0.04f;
                    this.DrawEquipmentAiming(this.pawn.equipment.Primary, drawLoc5, 217f);
                }
            }
        }

        public void DrawEquipmentAiming(Thing eq, Vector3 drawLoc, float aimAngle)
        {
            float num = aimAngle - 90f;
            Mesh mesh;
            if (aimAngle > 20f && aimAngle < 160f)
            {
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
            }
            else if (aimAngle > 200f && aimAngle < 340f)
            {
                mesh = MeshPool.plane10Flip;
                num -= 180f;
                num -= eq.def.equippedAngleOffset;
            }
            else
            {
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
            }
            num %= 360f;
            Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
            Material matSingle;
            if (graphic_StackCount != null)
            {
                matSingle = graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle;
            }
            else
            {
                matSingle = eq.Graphic.MatSingle;
            }
            Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(num, Vector3.up), matSingle, 0);
        }

        private bool CarryWeaponOpenly()
        {
            return (this.pawn.carryTracker == null || this.pawn.carryTracker.CarriedThing == null) && (this.pawn.Drafted || (this.pawn.CurJob != null && this.pawn.CurJob.def.alwaysShowWeapon) || (this.pawn.mindState.duty != null && this.pawn.mindState.duty.def.alwaysShowWeapon));
        }

        private Rot4 LayingFacing()
        {
            if (this.pawn.GetPosture() == PawnPosture.LayingFaceUp)
            {
                return Rot4.South;
            }
            if (this.pawn.RaceProps.Humanlike)
            {
                switch (this.pawn.thingIDNumber % 4)
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
            }
            else
            {
                switch (this.pawn.thingIDNumber % 4)
                {
                    case 0:
                        return Rot4.South;
                    case 1:
                        return Rot4.East;
                    case 2:
                        return Rot4.West;
                    case 3:
                        return Rot4.West;
                }
            }
            return Rot4.Random;
        }

        public Vector3 BaseHeadOffsetAt(Rot4 rotation)
        {
            float num = PawnRendererModded.HorHeadOffsets[(int)this.pawn.story.bodyType];
            switch (rotation.AsInt)
            {
                case 0:
                    return new Vector3(0f, 0f, 0.34f);
                case 1:
                    return new Vector3(num, 0f, 0.34f);
                case 2:
                    return new Vector3(0f, 0f, 0.34f);
                case 3:
                    return new Vector3(-num, 0f, 0.34f);
                default:
                    Log.Error("BaseHeadOffsetAt error in " + this.pawn);
                    return Vector3.zero;
            }
        }

        public void Notify_DamageApplied(DamageInfo dam)
        {
            this.graphics.flasher.Notify_DamageApplied(dam);
            this.wiggler.Notify_DamageApplied(dam);
        }
    }
    }
