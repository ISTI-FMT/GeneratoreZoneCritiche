using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestioneAreeCritiche.ModelChecking
{
    static class LivenessCheck
    {
        private static HashSet<StatoTreni> visitati = new HashSet<StatoTreni>();

        private static bool CheckLiveness(StatoTreni statoTreni, StatoAree statoAree, Stack<KeyValuePair<string, int>> sequenza, /*HashSet<StatoTreni> visitati, */bool consideraAreeCritiche)
        {
            if (visitati.Contains(statoTreni))
            {
                return false;
            }

            bool res = false;
            bool cannotAdvance = true;
            for (int i = 0; i < statoTreni.Missioni.Count; i++)
            {
                StatoMissione missione = statoTreni.Missioni[i];
                if (missione.Terminata)
                {
                    KeyValuePair<string, int> movement = new KeyValuePair<string, int>(missione.Trn, missione.Cdbs[missione.CurrentStep]);
                    if (!sequenza.Contains(movement))
                    {
                        sequenza.Push(movement);
                    }
                }
                else
                {
                    int cdbCorrente = missione.Cdbs[missione.CurrentStep];
                    int cdbNext = missione.Cdbs[missione.CurrentStep + 1];

                    bool evolving = true;
                    for (int j = 0; j < statoTreni.Missioni.Count; j++)
                    {
                        if (j != i)
                        {
                            StatoMissione missione2 = statoTreni.Missioni[j];
                            int cdbCorrente2 = missione2.Cdbs[missione2.CurrentStep];
                            if (cdbNext == cdbCorrente2)
                            {
                                //il prossimo cdb è occupato, non posso avanzare
                                evolving = false;
                                break;
                            }
                        }
                    }

                    if (evolving)
                    {
                        if (consideraAreeCritiche)
                        {
                            //controllo vincoli aree critich
                            if (!statoAree.EntrataPermessa(missione, missione.CurrentStep + 1, cdbNext))
                            {
                                evolving = false;
                                break;
                            }
                        }
                    }

                    if (evolving)
                    {
                        cannotAdvance = false;

                        StatoTreni stato2 = statoTreni.Clone();
                        stato2.Missioni[i].MoveNext();

                        StatoAree aree2 = null;
                        if (consideraAreeCritiche)
                        {
                            aree2 = statoAree.Clone();
                            aree2.Entrata(stato2.Missioni[i], stato2.Missioni[i].CurrentStep, cdbNext);
                        }

                        //se esiste un path che porta alla fine non vado oltre
                        if (CheckLiveness(stato2, aree2, sequenza,/* visitati,*/ consideraAreeCritiche))
                        {
                            sequenza.Push(new KeyValuePair<string, int>(missione.Trn, missione.Cdbs[missione.CurrentStep]));
                            res = true;
                            break;
                        }
                        else
                        {
                            sequenza.Clear();
                        }
                    }
                }
            }

            //Non posso più andare oltre (tutti i treni hanno evolving a false)
            //Controllo se il motivo è un deadlock o se tutte le missioni sono terminate
            if (cannotAdvance)
            {
                bool final = statoTreni.Missioni.All(missione => missione.Terminata);
                if (final)
                {
                    res = true;
                }
            }
            if (!res)
            {
                visitati.Add(statoTreni);
            }
            return res;
        }

        /// <summary>
        /// Ritorna una sequenza di coppie (Trn,Cdb), se esiste, che porta ad uno stato finale senza deadlock
        /// Se la sequenza non esiste ritorna NULL
        /// </summary>
        /// <param name="stato">lo stato dei treni</param>
        /// <param name="sequenza">sequenza dei movimenti effettuati dai treni (contiene i TRN dei treni mossi)</param>
      internal static Stack<KeyValuePair<string, int>> CheckLiveness(StatoTreni statoTreni, StatoAree statoAree, bool consideraAreeCritiche)
        {
            Stack<KeyValuePair<string, int>> sequenza = new Stack<KeyValuePair<string, int>>();
            

            if (CheckLiveness(statoTreni, statoAree, sequenza,/* visitati, */consideraAreeCritiche))
            {
                return sequenza;
            }
            else
            {
                return null;
            }
        }


    }
}
