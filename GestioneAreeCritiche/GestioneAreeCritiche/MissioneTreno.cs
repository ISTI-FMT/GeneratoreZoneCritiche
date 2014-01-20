using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestioneAreeCritiche
{
    internal class MissioneTreno
    {
        internal MissioneTreno(string nomeTreno, List<int> cdbList)
        {
            NomeTreno = nomeTreno;
            CdbList = cdbList;
        }

        internal string NomeTreno { get; private set; }

        internal List<int> CdbList { get; private set; }
    }
}
