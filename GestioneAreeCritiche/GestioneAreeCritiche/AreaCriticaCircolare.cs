using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestioneAreeCritiche
{
    internal class AreaCriticaCircolare : IEquatable<AreaCriticaCircolare>
    {
        internal AreaCriticaCircolare()
        {
            ListaCdb = new List<int>();
            Treni = new HashSet<string>();
        }

        internal List<int> ListaCdb { get; set; }

        public HashSet<string> Treni { get; private set; }

        public bool Equals(AreaCriticaCircolare other)
        {
            bool res = false;
            if (ListaCdb != null && other.ListaCdb != null && ListaCdb.Count == other.ListaCdb.Count)
            {
                bool found = false;
                foreach (int cdbVisitato in ListaCdb)
                {
                    found = other.ListaCdb.Any(visitato => cdbVisitato == visitato);
                    if (!found)
                    {
                        break;
                    }
                }
                res = found;
            }
            return res;
        }
    }
}
