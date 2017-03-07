using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace FactionColors
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.ohu.factionColor.main");

            harmony.Patch(AccessTools.Method(typeof(Verse.PawnGraphicSet), "ResolveApparelGraphics", null),null , new HarmonyMethod(typeof(HarmonyPatches), "ResolveApparelGraphicsOriginal"));
            harmony.Patch(AccessTools.Method(typeof(Verse.PawnRenderer), "DrawEquipmentAiming", new Type[] { typeof(Thing), typeof(Vector3), typeof(float) }), null, new HarmonyMethod(typeof(HarmonyPatches), "DrawEquipmentAimingModded",null ));
        }

        public static void ResolveApparelGraphicsOriginal(PawnGraphicSet __instance)
        {
            __instance.ClearCache();
            __instance.apparelGraphics.Clear();
            List<Apparel> OriginalItems = new List<Apparel>();
            foreach (Apparel current in __instance.pawn.apparel.WornApparelInDrawOrder)
            {
                ApparelGraphicRecord item;
                if (current.GetComp<CompFactionColor>() != null)
                {
                    if ((ApparelGraphicGetterFC.TryGetGraphicApparelModded(current, __instance.pawn.story.bodyType, out item)))
                    {
                        if (current.GetComp<ApparelDetailDrawer>() != null && !current.Spawned)
                        {
                            OriginalItems.Add(current);
                        }

                        __instance.apparelGraphics.Add(item);
                    }
                }
                else if (ApparelGraphicRecordGetter.TryGetGraphicApparel(current, __instance.pawn.story.bodyType, out item))
                {
                    __instance.apparelGraphics.Add(item);
                }
            }
            //    Corruption.AfflictionDrawerUtility.DrawChaosOverlays(this.pawn);
            foreach (Apparel app in OriginalItems)
            {
                ApparelDetailDrawer.DrawDetails(__instance.pawn, app);
            }
        }

        public static void DrawEquipmentAimingModded(Thing eq, Vector3 drawLoc, float aimAngle)
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

            if (eq.GetType() == typeof(FactionItem))
            {
                FactionItemDef facdef = eq.def as FactionItemDef;
                Material Mat = eq.Graphic.MatAt(eq.Rotation);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(drawLoc, Quaternion.AngleAxis(num, Vector3.up), facdef.ItemMeshSize * 1.2f);
                Graphics.DrawMesh(mesh, matrix, matSingle, 0);

                //                Matrix4x4 matrix = default(Matrix4x4);
                //                matrix.SetTRS(drawLoc, Quaternion.AngleAxis(num, Vector3.up), facdef.ItemMeshSize);
                //                Graphics.DrawMesh(mesh, matrix, matSingle, 0);
                //               Graphics.DrawMesh()
            }

            else
            {
                Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(num, Vector3.up), matSingle, 0);
            }
        }
    }
}
