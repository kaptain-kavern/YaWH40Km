using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FactionColors
{
    public class FactionItemRenderer : PawnRendererModded
    {
        public FactionItemRenderer(Pawn pawn) : base(pawn)
        {
        }

        public void DrawEquipmentAimingModded(Thing eq, Vector3 drawLoc, float aimAngle)
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
                matrix.SetTRS(drawLoc, Quaternion.AngleAxis(num, Vector3.up), facdef.ItemMeshSize*1.2f);
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
