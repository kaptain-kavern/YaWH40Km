using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class HediffComp_TurnServitor : HediffComp_Disappears
    {
        public override bool CompShouldRemove
        {
            get
            {
                if(base.CompShouldRemove)
                {
                    List<Building> list = this.Pawn.Map.listerBuildings.allBuildingsColonist.FindAll(x => x is Building_MechanicusMedTable);
                    foreach (Building_MechanicusMedTable current in list)
                    {
                        Building_MechanicusMedTable medTable = (Building_MechanicusMedTable)current;
                        if (medTable.patient == this.Pawn)
                        {
                            AlienRace.AlienPawn apawn = AlienRace.AlienPawnGenerator.GeneratePawn(CorruptionDefOfs.ServitorColonist) as AlienRace.AlienPawn;
                            apawn.story.childhood = this.Pawn.story.childhood;
                            apawn.story.adulthood = this.Pawn.story.adulthood;
                            apawn.SpawnSetupAlien();
                            medTable.patient.Destroy(DestroyMode.Vanish);
                            medTable.patient = apawn;
                            return true;
                        }
                    }

                    return false;
                }
                return false;
            }
        }

        public override void CompPostMake()
        {
            base.CompPostMake();

        }
    }
}
