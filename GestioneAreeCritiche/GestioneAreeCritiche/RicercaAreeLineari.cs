using System.Collections.Generic;
using System.Linq;

namespace GestioneAreeCritiche
{
    internal static class RicercaAreeLineari
    {

        private static void AggiungiAreaLineare(List<AreaCriticaLineare> aree, MissioneTreno trenoA, MissioneTreno trenoB, List<int> area )
        {
            //Cerco se esiste già un'area con la stessa sequenza
            AreaCriticaLineare areaLineare = aree.Find(arealista => arealista.Cdb.SequenceEqual(area));

            if (areaLineare == null)
            {
                //Cerco se l'area esiste già ma è stata trovata nel senso opposto.
                //NOTA: In questo caso A e B si inseriscono nell'ordine inverso (B a sinistra, A a destra)
                //per rispettare l'ordine dell'area originale
                List<int> areaReverse = area.ToList();
                areaReverse.Reverse();

                areaLineare = aree.Find(arealista => arealista.Cdb.SequenceEqual(areaReverse));

                if (areaLineare == null)
                {
                    AreaCriticaLineare acl = new AreaCriticaLineare("xx", area);
                    acl.TreniSinistra.Add(trenoA.NomeTreno);
                    acl.TreniDestra.Add(trenoB.NomeTreno);
                    aree.Add(acl);
                }
                else
                {
                    if (!areaLineare.TreniSinistra.Contains(trenoB.NomeTreno))
                    {
                        areaLineare.TreniSinistra.Add(trenoB.NomeTreno);
                    }
                    if (!areaLineare.TreniDestra.Contains(trenoA.NomeTreno))
                    {
                        areaLineare.TreniDestra.Add(trenoA.NomeTreno);
                    }
                }
            }
            else
            {
                if (!areaLineare.TreniSinistra.Contains(trenoA.NomeTreno))
                {
                    areaLineare.TreniSinistra.Add(trenoA.NomeTreno);
                }
                if (!areaLineare.TreniDestra.Contains(trenoB.NomeTreno))
                {
                    areaLineare.TreniDestra.Add(trenoB.NomeTreno);
                }
            }
        }


        internal static List<AreaCriticaLineare> Ricerca(MissioneTreno trenoA, MissioneTreno trenoB)
        {
            List<AreaCriticaLineare> aree = new List<AreaCriticaLineare>();
            List<int> cdbA = trenoA.CdbList;
            List<int> cdbB = trenoB.CdbList;

            for (int aIdx = 0; aIdx < cdbA.Count; aIdx++)
            {
                int currentValue = cdbA[aIdx];

                //cerco l'elemento corrente di A in B
                int bIdx = cdbB.IndexOf(currentValue);
                while (bIdx >= 0)
                {
                    List<int> area = SequenzaMassima(cdbA, cdbB, aIdx, bIdx);

                    //Il numero minimo di punti per avere un'area critica è due
                    if (area.Count > 1)
                    {
                        AggiungiAreaLineare(aree, trenoA, trenoB, area);

                        //Dato che ho trovato un'area comune di lunghezza maggiore a 2, 
                        //posso proseguire dal primo cdb successivo all'area
                        aIdx = aIdx + area.Count - 1;
                    }

                    //Se il vettore B ha un secondo elemento che corrisponde a quello corrente di A,
                    //sposto l'indice su quell'elemento
                    bIdx = cdbB.IndexOf(currentValue, bIdx + 1);
                }
            }
            return aree;
        }

        /// <summary>
        /// Ritorna una lista con la sequenza massima di cdb in comune.
        /// Cerca nella lista di cdbA a partire dalla posizione startA.
        /// Cerca nella lista di cdbB a partire dalla posizione startB in direzione opposta.
        /// </summary>
        private static List<int> SequenzaMassima(List<int> cdbA, List<int> cdbB, int startA, int startB)
        {
            List<int> res = new List<int>();

            int bIdx = startB;
            for (int aIdx = startA; aIdx < cdbA.Count; aIdx++)
            {
                if (bIdx < 0)
                {
                    break;
                }

                int valA = cdbA[aIdx];
                int valB = cdbB[bIdx];

                if (valA == valB)
                {
                    res.Add(valA);
                }

                bIdx--;
            }

            return res;
        }
    }
}
