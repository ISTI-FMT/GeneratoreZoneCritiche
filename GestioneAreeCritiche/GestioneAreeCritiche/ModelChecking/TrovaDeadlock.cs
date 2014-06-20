﻿using GestioneAreeCritiche.AreeCritiche;
using GestioneAreeCritiche.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestioneAreeCritiche.ModelChecking
{
    class TrovaDeadlock
    {
        private static void Trova(StatoTreni statoTreni, StatoAree statoAree, HashSet<StatoTreni> visitati, List<IAreaCritica> aree, out bool statoFinaleRaggiungibile, List<Deadlock> deadlocks)
        {
            statoFinaleRaggiungibile = false;

            if (visitati.Contains(statoTreni))
            {
                return;
            }
            visitati.Add(statoTreni);

            List<int> cdbs = new List<int>();
            for (int i = 0; i < statoTreni.Missioni.Count; i++)
            {
                StatoMissione missione = statoTreni.Missioni[i];
                int cdbCorrente = missione.Cdbs[missione.CurrentStep];
                cdbs.Add(cdbCorrente);
            }

            string listacdb = string.Join(",", cdbs);
            
            
            
            bool cannotAdvance = true;
            //bool bloccoArea = false;
            HashSet<string> bloccati = new HashSet<string>();
            for (int i = 0; i < statoTreni.Missioni.Count; i++)
            {
                StatoMissione missione = statoTreni.Missioni[i];

                //Se la missione è terminata, non c'è bisogno di provare ad evolvere
                if (missione.Terminata)
                    continue;

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

                            if (!bloccati.Contains(missione.Trn))
                                bloccati.Add(missione.Trn);

                            if (!bloccati.Contains(missione2.Trn))
                                bloccati.Add(missione2.Trn);
                        }
                    }
                }

                if (evolving)
                {
                    //controllo vincoli aree critich
                    if (!statoAree.EntrataPermessa(missione, missione.CurrentStep + 1, cdbNext))
                    {
                        //Console.WriteLine(missione.Trn + " bloccato: "+ cdbCorrente + "=>" + cdbNext);
                        evolving = false;
                        //bloccoArea = true;

                        StatoTreni stato2 = statoTreni.Clone();
                        stato2.Missioni[i].MoveNext();
                        Stack<KeyValuePair<string, int>> liveness = LivenessCheck.CheckLiveness(stato2, statoAree, false);

                        if (liveness != null)
                        {
                            string azioniAree = string.Join(",",missione.AzioniCdb[missione.CurrentStep + 1]);
                            Console.WriteLine(listacdb + ": FALSO POSITIVO: AreeCritica blocca il movimento " +   missione.Trn + ": " + cdbCorrente + "=>" + cdbNext + " azioni: [" + azioniAree + "]");
                        }
                    }
                }

                if (evolving)
                {
                    //Console.WriteLine(listacdb + ": " + missione.Trn + " Entrata: " + cdbCorrente + "=>" + cdbNext);

                    cannotAdvance = false;

                    StatoTreni stato2 = statoTreni.Clone();
                    stato2.Missioni[i].MoveNext();

                    StatoAree aree2 = statoAree.Clone();
                    aree2.Entrata(stato2.Missioni[i], stato2.Missioni[i].CurrentStep, cdbNext);

                    //se esiste un path che porta alla fine non vado oltre
                    bool statoFinaleRaggiunto;
                    Trova(stato2, aree2, visitati, aree, out statoFinaleRaggiunto, deadlocks);

                    if (statoFinaleRaggiunto)
                    {
                        statoFinaleRaggiungibile = true;
                    }
                }
            }

            if (bloccati.Count > 0)
            {
                //

                bool anyfinal = statoTreni.Missioni.Any(missione => missione.Terminata);
                if (!anyfinal)
                {
                    Stack<KeyValuePair<string, int>> liveness = LivenessCheck.CheckLiveness(statoTreni, statoAree, false);
                    if (liveness == null)
                    {
                        Deadlock deadlock = new Deadlock();
                        foreach (StatoMissione missioneDeadlock in statoTreni.Missioni)
                        {
                            deadlock.Positions.Add(missioneDeadlock.CurrentStep);
                        }
                        deadlock.Bloccati = bloccati;
                        deadlocks.Add(deadlock);
                    }
                }
            }

            if (cannotAdvance)
            {
                bool final = statoTreni.Missioni.All(missione => missione.Terminata);

                //Controllo se c'è un deadlock

                if (!final)
                {
                    //se nessun movimento è permesso a causa delle aree critiche, non significa che sono in un deadlock
                    bool nessunaPermessa = true;
                    foreach (StatoMissione missione in statoTreni.Missioni)
                    {
                        if (!missione.Terminata)
                        {
                            int cdbNext = missione.Cdbs[missione.CurrentStep + 1];
                            if (statoAree.EntrataPermessa(missione, missione.CurrentStep + 1, cdbNext))
                            {
                                nessunaPermessa = false;
                                break;
                            }
                        }
                    }

                    if (!nessunaPermessa)
                    {
                        bool anyfinal = statoTreni.Missioni.Any(missione => missione.Terminata);
                        if (!anyfinal)
                        {
                            Deadlock deadlock = new Deadlock();
                            foreach (StatoMissione missione in statoTreni.Missioni)
                            {
                                deadlock.Positions.Add(missione.CurrentStep);
                                deadlock.Bloccati.Add(missione.Trn);
                            }
                            deadlocks.Add(deadlock);
                        }
                    }
                }
                else if (final)
                {
                    statoFinaleRaggiungibile = true;
                }
            }
        }

        /// <summary>
        /// Ritorna una sequenza di coppie (Trn,Cdb), se esiste, che porta ad uno stato finale senza deadlock
        /// Se la sequenza non esiste ritorna NULL
        /// </summary>
        /// <param name="stato">lo stato dei treni</param>
        /// <param name="sequenza">sequenza dei movimenti effettuati dai treni (contiene i TRN dei treni mossi)</param>
        private static void Trova(StatoTreni statoTreni, StatoAree statoAree, out bool statoFinaleRaggiungibile, out List<Deadlock> deadlocks)
        {
            HashSet<StatoTreni> visitati = new HashSet<StatoTreni>();

            List<IAreaCritica> aree = new List<IAreaCritica>();
            deadlocks = new List<Deadlock>();
            TrovaDeadlock.Trova(statoTreni, statoAree, visitati, aree, out statoFinaleRaggiungibile, deadlocks);
        }

        public static void Trova(DatiAree dati, out bool statoFinaleRaggiungibile, out List<Deadlock> deadlocks)
        {
            StatoAree statoAree = new StatoAree();
            foreach (IAreaCritica area in dati.AreeCritiche)
            {
                statoAree.Aree.Add(area);
                area.Reset();
            }

            StatoTreni statoTreni = new StatoTreni();
            foreach (MissioneAnnotata missione in dati.MissioniAnnotate)
            {
                StatoMissione statoMissione = new StatoMissione(missione);
                statoTreni.Missioni.Add(statoMissione);
                if (statoAree.EntrataPermessa(statoMissione, 0, statoMissione.Cdbs[0]))
                {
                    statoAree.Entrata(statoMissione, 0, statoMissione.Cdbs[0]);
                }
                else
                {
                    Console.WriteLine("Attenzione: Lo stato iniziale non è corretto. Impossible far partire il treno {0} dal cdb {1}", statoMissione.Trn, statoMissione.Cdbs[0]);
                    //fine = true;
                    //break;
                }
            }

            //if (fine)
            //    break;

            TrovaDeadlock.Trova(statoTreni, statoAree, out statoFinaleRaggiungibile, out deadlocks);
        }

        //private static void AggiungiAreaLineare(List<IAreaCritica> aree, string trenoA, string trenoB, List<int> area)
        //{
        //    //Cerco se esiste già un'area con la stessa sequenza
        //    IAreaCritica areaLineare = aree.Find(arealista => arealista.ListaCdb.SequenceEqual(area));

        //    if (areaLineare == null)
        //    {
        //        //Cerco se l'area esiste già ma è stata trovata nel senso opposto.
        //        //NOTA: In questo caso A e B si inseriscono nell'ordine inverso (B a sinistra, A a destra)
        //        //per rispettare l'ordine dell'area originale
        //        List<int> areaReverse = area.ToList();
        //        areaReverse.Reverse();

        //        areaLineare = aree.Find(arealista => arealista.ListaCdb.SequenceEqual(areaReverse));

        //        if (areaLineare == null)
        //        {
        //            AreaCriticaLineare acl = new AreaCriticaLineare(area);
        //            acl.TreniSinistra.Add(trenoA);
        //            acl.TreniDestra.Add(trenoB);
        //            aree.Add(acl);
        //        }
        //        else
        //        {
        //            if (!areaLineare.TreniSinistra.Contains(trenoB))
        //            {
        //                areaLineare.TreniSinistra.Add(trenoB);
        //            }
        //            if (!areaLineare.TreniDestra.Contains(trenoA))
        //            {
        //                areaLineare.TreniDestra.Add(trenoA);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (!areaLineare.TreniSinistra.Contains(trenoA))
        //        {
        //            areaLineare.TreniSinistra.Add(trenoA);
        //        }
        //        if (!areaLineare.TreniDestra.Contains(trenoB))
        //        {
        //            areaLineare.TreniDestra.Add(trenoB);
        //        }
        //    }
        //}
    }
}
