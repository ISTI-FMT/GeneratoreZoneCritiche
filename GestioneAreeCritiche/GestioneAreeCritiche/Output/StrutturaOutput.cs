using System.Collections.Generic;

namespace GestioneAreeCritiche.Output
{
    public class StrutturaOutput
    {
        public StrutturaOutput()
        {
            MissioniAnnotate = new List<MissioneAnnotata>();
            LimitiAree = new List<int>();
        }

        public List<int> LimitiAree { get; set; }

        public List<MissioneAnnotata> MissioniAnnotate { get; private set; }
    }
}