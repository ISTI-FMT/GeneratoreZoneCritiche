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

        private int treniDestra = 0;
        private int treniSinistra = 0;

        public void Reset()
        {
            treniSinistra = 0;
            treniDestra = 0;
        }

        public bool entrataPermessa(string idTreno, int cdb, int tipoEntrata)
        {
            bool res = true;
            if (tipoEntrata > 0)
            {
                if (tipoEntrata == 3) //Entrata da sinistra (se il cdb è il primo della lista e non ero già entrato da destra)
                {
                    if (treniDestra > 0)
                    {
                        res = false;
                    }
                }
                else if (tipoEntrata == 2) //Entrata da sinistra (se il cdb è l'ultimo della lista e non ero già entrato da destra)
                {
                    if (treniSinistra > 0)
                    {
                        res = false;
                    }
                }
            }
            return res;
        }

        public void entrata(string idTreno, int cdb, int tipoEntrata)
        {
            if (tipoEntrata > 0)
            {
                if (tipoEntrata == 3) //entrata da sinistra
                {
                    treniSinistra++;
                }
                else if (tipoEntrata == 2) //entrata da destra
                {
                    treniDestra++;
                }
            }
            else //sto entrando in un cdb che non è di questa area. Rimuovo il treno
            {
                if (tipoEntrata == -3)
                {
                    treniSinistra--;
                }
                else if (tipoEntrata == -2)
                {
                    treniDestra--;
                }
            }
        }

        public string GetListaCdbStr()
        {
            return string.Join(",", ListaCdb);
        }

        public object Clone()
        {
            AreaCriticaLineare areaClone = new AreaCriticaLineare(ListaCdb);
            areaClone.treniSinistra = treniSinistra;
            areaClone.treniDestra = treniDestra;
            areaClone.TreniSinistra = TreniSinistra;
            areaClone.TreniDestra = TreniDestra;
            return areaClone;
        }
    }
}
