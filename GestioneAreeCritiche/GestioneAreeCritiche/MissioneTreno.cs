using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestioneAreeCritiche
{
    public class MissioneTreno
    {
        public MissioneTreno(string nomeTreno, List<int> cdbList)
        {
            NomeTreno = nomeTreno;
            CdbList = cdbList;
        }

        public string NomeTreno { get; private set; }

        public List<int> CdbList { get; private set; }

        public List<int> Visitati { get; set; }
    }
}
