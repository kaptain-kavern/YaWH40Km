using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;

namespace Corruption.Ships
{
    public class Building_ShipTurret : Building_TurretGun
    {
        public ShipBase parentShip;

        public string assignedSlotName;

        public ThingDef installedByWeaponSystem;

        public void SwitchTurret(bool active)
        {
            bool flag = (bool)typeof(Building_TurretGun).GetProperty("holdFire").GetValue(this, null);

            flag = active;
            if (flag)
            {
                LocalTargetInfo info = (LocalTargetInfo)typeof(Building_TurretGun).GetProperty("currentTargetInt").GetValue(this, null);
                int burstTicks = (int)typeof(Building_TurretGun).GetProperty("burstWarmupTicksLeft").GetValue(this, null);
                info = LocalTargetInfo.Invalid;
                burstTicks = 0;
            }
        }

        public ShipWeaponSlot Slot
        {
            get
            {
                if(this.parentShip != null)
                {
                    ShipWeaponSlot slot = parentShip.installedTurrets.First(x => x.Key.SlotName == this.assignedSlotName).Key;
                    if (slot != null)
                    {
                        return slot;
                    }
                    else
                    {
                        Log.Error("No slot found for " + this.ToString() + " on " + parentShip.ToString());
                        return null;
                    }
                }
                Log.Error("Requested ShipWeaponSlot on Turret without assigned Ship");
                return null;
            }
        }

        public override Vector3 DrawPos
        {
            get
            {
                KeyValuePair<ShipWeaponSlot, Building_ShipTurret> turretEntry = parentShip.installedTurrets.FirstOrDefault(x => x.Value == this);
                if (turretEntry.Key != null)
                {
                    Vector3 vector = this.parentShip.DrawPos + DropShipUtility.AdjustedIntVecForShip(this.parentShip, turretEntry.Key.turretPosOffset).ToVector3();
                    vector.y = Altitudes.AltitudeFor(turretEntry.Key.altitudeLayer);
                    return vector;
                }
                return base.DrawPos;
            }
        }

        public override void Draw()
        {
            this.top.DrawTurret();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator();
            while (enumerator.MoveNext())
            {
                Gizmo current = enumerator.Current;
                yield return current;
            }
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.LookReference<ShipBase>(ref this.parentShip, "parentShip");
            Scribe_Values.LookValue<string>(ref this.assignedSlotName, "assignedSlotName");
        }
    }
}
