using System;
using System.Collections.Generic;

namespace GestioneAreeCritiche.AreeCritiche
{
    public class AreaCriticaLineare : IAreaCritica
    {
        public AreaCriticaLineare(List<int> listaCdb)
        {
            ListaCdb = listaCdb;
            TreniSinistra = new HashSet<string>();
            TreniDestra = new HashSet<string>();
        }

        public int Limite { get { return 0; } }

        public List<int> ListaCdb { get; set; }

        public HashSet<string> TreniSinistra { get; private set; }

        public HashSet<string> TreniDestra { get; private set; }

        public string GetListaCdbStr()
        {
            return string.Join(",", ListaCdb);
        }
    }
}
