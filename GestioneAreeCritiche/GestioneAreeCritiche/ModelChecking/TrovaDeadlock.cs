using GestioneAreeCritiche.AreeCritiche;
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
        private static void Trova(StatoTreni statoTreni, StatoAree statoAree, HashSet<StatoTreni> visitati, List<IAreaCritica> aree, out bool statoFinaleRaggiungibile, List<Deadlock> deadlockDaEvitare, List<Deadlock> deadlockTrovati)
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
                    StatoTreni stato2 = statoTreni.Clone();
                    stato2.Missioni[i].MoveNext();

                    foreach (Deadlock deadlock in deadlockDaEvitare)
                    {
                        if (deadlock.VerificaDeadlock(stato2))
                        {
                            Console.WriteLine(listacdb + ": " + missione.Trn + ": " + cdbCorrente + "=>" + cdbNext + " Evoluzione bloccata da deadlock noto"); 
                            evolving = false;
                            Stack<KeyValuePair<string, int>> liveness = LivenessCheck.CheckLiveness(stato2, statoAree, false);

                            if (liveness != null)
                            {
                                string azioniAree = string.Join(",", missione.AzioniCdb[missione.CurrentStep + 1]);
                                Console.WriteLine(listacdb + ": FALSO POSITIVO: deadlock noto blocca il movimento " + missione.Trn + ": " + cdbCorrente + "=>" + cdbNext + " azioni: [" + azioniAree + "]");
                            }

                            break;
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
                    Trova(stato2, aree2, visitati, aree, out statoFinaleRaggiunto, deadlockDaEvitare, deadlockTrovati);

                    if (statoFinaleRaggiunto)
                    {
                        statoFinaleRaggiungibile = true;
                    }
                }
            }

            //if (bloccati.Count > 0)
            //{
            //    bool anyfinal = statoTreni.Missioni.Any(missione => missione.Terminata);
            //    if (!anyfinal)
            //    {
            //        Stack<KeyValuePair<string, int>> liveness = LivenessCheck.CheckLiveness(statoTreni, statoAree, false);
            //        if (liveness == null)
            //        {
            //            Deadlock deadlock = new Deadlock();
            //            foreach (StatoMissione missioneDeadlock in statoTreni.Missioni)
            //            {
            //                int cdb = missioneDeadlock.Cdbs[missioneDeadlock.CurrentStep];
            //                if (statoAree.InArea(cdb))
            //                {
            //                    deadlock.AggiungiPosizione(missioneDeadlock.Trn, cdb);
            //                }
            //            }
            //            if (deadlock.Positions.Count > 0)
            //            {
            //                if (!deadlockTrovati.Contains(deadlock))
            //                {
            //                    deadlockTrovati.Add(deadlock);
            //                }
            //            }
            //        }
            //    }
            //}

            if (cannotAdvance)
            {
                bool final = statoTreni.Missioni.All(missione => missione.Terminata);

                //Controllo se c'è un deadlock

                if (!final)
                {
                    ////se nessun movimento è permesso a causa delle aree critiche, non significa che sono in un deadlock
                    //bool nessunaPermessa = true;
                    //foreach (StatoMissione missione in statoTreni.Missioni)
                    //{
                    //    if (!missione.Terminata)
                    //    {
                    //        int cdbNext = missione.Cdbs[missione.CurrentStep + 1];
                    //        if (statoAree.EntrataPermessa(missione, missione.CurrentStep + 1, cdbNext))
                    //        {
                    //            nessunaPermessa = false;
                    //            break;
                    //        }
                    //    }
                    //}

                    //if (!nessunaPermessa)
                    //{
                    //    bool anyfinal = statoTreni.Missioni.Any(missione => missione.Terminata);
                    //    if (!anyfinal)
                    //    {
                            Deadlock deadlock = new Deadlock();
                            foreach (StatoMissione missione in statoTreni.Missioni)
                            {
                                int cdb = missione.Cdbs[missione.CurrentStep];
                                if (statoAree.InArea(cdb))
                                {
                                    deadlock.AggiungiPosizione(missione.Trn, cdb);
                                }
                            }
                            if (deadlock.Positions.Count > 0)
                            {
                                if (!deadlockTrovati.Contains(deadlock) && !deadlockDaEvitare.Contains(deadlock))
                                {
                                    deadlockTrovati.Add(deadlock);
                                }
                            }
                    //    }
                    //}
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
        private static void Trova(StatoTreni statoTreni, StatoAree statoAree, out bool statoFinaleRaggiungibile, out List<Deadlock> deadlockTrovati)
        {
            deadlockTrovati = new List<Deadlock>();
            List<Deadlock> deadlockDaEvitare = new List<Deadlock>();

            int contatoreIterazioni = 1;
            do
            {
                Console.WriteLine();
                Console.WriteLine("Iterazione " + contatoreIterazioni + "...");

                deadlockDaEvitare.AddRange(deadlockTrovati);
                deadlockTrovati = new List<Deadlock>();

                HashSet<StatoTreni> visitati = new HashSet<StatoTreni>();
                List<IAreaCritica> aree = new List<IAreaCritica>();
                TrovaDeadlock.Trova(statoTreni.Clone(), statoAree.Clone(), visitati, aree, out statoFinaleRaggiungibile, deadlockDaEvitare, deadlockTrovati);

                Console.WriteLine("Iterazione {0} terminata. Trovati  {1} nuovi deadlock", contatoreIterazioni, deadlockTrovati.Count);
                contatoreIterazioni++;

            } while (deadlockTrovati.Count > 0);

            deadlockTrovati = deadlockDaEvitare;
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


            Console.WriteLine();
            Console.WriteLine("Riduzione...");

            //Elimino i deadlock che possono essere ridotti ad un caso più semplice
            List<Deadlock> deadlockRidotti = new List<Deadlock>();
            foreach (Deadlock dl in deadlocks)
            {
                Deadlock padre;
                if (IsNewDeadlock(deadlocks, dl, out padre))
                {
                    deadlockRidotti.Add(dl);
                }
                else
                {
                    Console.WriteLine("Deadlock Ridotto: {0} => {1}", dl, padre);
                }
            }
            deadlocks = deadlockRidotti;
        }
        
        /// <summary>
        /// Ritorna TRUE se il deadlock passato non è un caso più specifico di altri deadlock nella lista
        /// </summary>
        private static bool IsNewDeadlock(List<Deadlock> deadlocks, Deadlock deadlock, out Deadlock padre)
        {
            padre = null;
            foreach (Deadlock dl in deadlocks)
            {
                if (dl != deadlock && deadlock.IsSubDeadlock(dl))
                {
                    padre = dl;
                    return false;
                }
            }
            return true;
        }


    }
}
