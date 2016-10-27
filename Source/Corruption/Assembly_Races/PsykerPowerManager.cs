using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Corruption
{
   public class PsykerPowerManager
    {
        public PsykerPowerManager(CompPsyker compPsyker)
        {
            this.compPsyker = compPsyker;
        }

        public void Initialize()
        {
            this.PowerLevelSlots = new Dictionary<PsykerPowerLevel, int>();
            
        }

        private CompPsyker compPsyker;

        public Dictionary<PsykerPowerLevel, int> PowerLevelSlots;

        public Verb_CastWarpPower selectedVerb;

        public void PsykerPowerManagerTick()
        {

        }

        public void UpdatePowers(PsykerPower newPower)
        {

        }

        public void CheckPowerSlots()
        {

        }

        public List<PsykerPower> powersint = new List<PsykerPower>();

        public List<PsykerPower> Powers = new List<PsykerPower>();

        public List<Verb_CastWarpPower> PowerVerbs
        {
            get
            {
                List<Verb_CastWarpPower> list = new List<Verb_CastWarpPower>();
                foreach (PsykerPower pow in Powers)
                {
                    Verb_CastWarpPower newverb = (Verb_CastWarpPower)Activator.CreateInstance(pow.powerdef.MainVerb.verbClass);
                    newverb.caster = this.compPsyker.psyker;
                    newverb.verbProps = pow.powerdef.MainVerb;
                    list.Add(newverb);
                }
                return list;
            }
        }
    }
}
