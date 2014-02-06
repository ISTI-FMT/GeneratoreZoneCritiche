using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestioneAreeCritiche.AreeCritiche
{
    public class AreaCriticaCircolare : IEquatable<AreaCriticaCircolare>, IAreaCritica
    {
        public AreaCriticaCircolare()
        {
            ListaCdb = new List<int>();
            Treni = new HashSet<string>();
        }

        public List<int> ListaCdb { get; set; }

        public int Limite
        {
            get
            {
                if (ListaCdb.Count == 0)
                {
                    return 0;
                }
                //La lista di cdb contiene cdb a coppie quindi il numero di  cdb è la sua metà
                //esempio: 1,2,2,3,3,4,4,1 => 1,2,3,4
                return (ListaCdb.Count / 2) - 1;
            }
        }

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

        public string GetListaCdbStr()
        {
            //La lista di cdb contiene cdb a coppie quindi il numero di  cdb è la sua metà
            //esempio: 1,2,2,3,3,4,4,1 => 1,2,3,4
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ListaCdb.Count; i = i + 2)
            {
                sb.Append(ListaCdb[i]);

                if (i < ListaCdb.Count - 2)
                {
                    sb.Append(',');
                }
            }
            return sb.ToString();
        }
    }
}
