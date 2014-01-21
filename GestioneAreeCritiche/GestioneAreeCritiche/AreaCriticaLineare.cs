using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestioneAreeCritiche
{
    internal class AreaCriticaLineare
    {
        internal AreaCriticaLineare(string nome, List<int> cdb)
        {
            Nome = nome;
            Cdb = cdb;
            TreniSinistra = new HashSet<string>();
            TreniDestra = new HashSet<string>();
        }

        public String Nome { get; private set; }

        public List<int> Cdb { get; private set; }

        public HashSet<string> TreniSinistra { get; private set; }

        public HashSet<string> TreniDestra { get; private set; }


    }
}
