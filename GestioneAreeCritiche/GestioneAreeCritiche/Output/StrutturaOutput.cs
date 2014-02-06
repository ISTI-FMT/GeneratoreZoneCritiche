using System.Collections.Generic;
using GestioneAreeCritiche.AreeCritiche;

namespace GestioneAreeCritiche.Output
{
    public class StrutturaOutput
    {
        public StrutturaOutput()
        {
            MissioniAnnotate = new List<MissioneAnnotata>();
            AreeCritiche = new List<IAreaCritica>();
        }
        
        public List<IAreaCritica> AreeCritiche { get; set; }

        public List<MissioneAnnotata> MissioniAnnotate { get; private set; }
    }
}