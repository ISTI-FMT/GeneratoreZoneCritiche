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

        private List<int> _listaCdb;
        public List<int> ListaCdb { get { return _listaCdb; } set { _listaCdb = value; } }

        public TipoArea TipoArea { get { return AreeCritiche.TipoArea.Circolare; } }

        public int Limite
        {
            get;
            set;
        }

        public HashSet<string> Treni { get; private set; }
        private int treni = 0;

        public void Reset()
        {
            treni = 0;
        }

        public bool entrataPermessa(string idTreno, int cdb, int tipoEntrata)
        {
            bool res = true;
            //l'ingresso è negato se:
            //- il cdb è uno di quelli dell'area
            //- il treno non è già dentro l'area
            //- l'area ha già il numero massimo di treni
            if (tipoEntrata == 1)
            {
                if (treni >= Limite)
                {
                    res = false;
                }
            }
            return res;
        }

        public void entrata(string idTreno, int cdb, int tipoEntrata)
        {
            if (tipoEntrata == 1)
            {
                treni++;
            }
            else if (tipoEntrata == -1)
            {
                treni--;
            }
        }

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
            string res = string.Join(",", ListaCdb);
            return res;
        }

        public object Clone()
        {
            AreaCriticaCircolare areaClone = new AreaCriticaCircolare();
            areaClone.ListaCdb = ListaCdb;
            areaClone.Treni = Treni;
            areaClone.treni = treni;
            areaClone.Limite = Limite;
            return areaClone;
        }
    }
}
