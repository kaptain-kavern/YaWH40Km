using Corruption.DefOfs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption.Ships
{
    public class JobGiver_LeaveInShip : ThinkNode_JobGiver
    {       

        protected override Job TryGiveJob(Pawn pawn)
        {
            ShipBase ship = (ShipBase)DropShipUtility.CurrentFactionShips(pawn).RandomElement();

            if (ship != null)
            {
                Job job = new Job(C_JobDefOf.LeaveInShip, pawn, ship);

                return job;
            }
            return null;
        }
    }
}
