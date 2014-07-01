using System.Collections.Generic;
using GestioneAreeCritiche.AreeCritiche;
using GestioneAreeCritiche.ModelChecking;

namespace GestioneAreeCritiche.Output
{
    public class DatiAree
    {
        public DatiAree()
        {
            MissioniAnnotate = new List<MissioneAnnotata>();
            AreeCritiche = new List<IAreaCritica>();
        }
        
        public List<IAreaCritica> AreeCritiche { get; set; }

        public List<MissioneAnnotata> MissioniAnnotate { get; private set; }

        public List<Deadlock> DeadlockConosciuti { get; set; }
    }
}