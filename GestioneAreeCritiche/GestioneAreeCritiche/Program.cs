using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GestioneAreeCritiche
{
    public class Program
    {
        private static void Confronto(MissioneTreno trenoA, MissioneTreno trenoB, List<AreaCriticaLineare> aree )
        {
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
                        //Cerco se esiste già un'area con la stessa sequenza
                        AreaCriticaLineare areaLineare = aree.Find(arealista => arealista.Cdb.SequenceEqual(area));

                        if (areaLineare == null)
                        {
                            AreaCriticaLineare acl = new AreaCriticaLineare("xx", area);
                            acl.TreniSinistra.Add(trenoA.NomeTreno);
                            acl.TreniDestra.Add(trenoB.NomeTreno);
                            aree.Add(acl);
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

                        //Dato che ho trovato un'area comune di lunghezza maggiore a 2, 
                        //posso proseguire dal primo cdb successivo all'area
                        aIdx = aIdx + area.Count - 1;

                        //Stampo informazioni di debug
                        //string maxArea = string.Join(",", area);
                        //Console.WriteLine(maxArea);
                    }

                    //Se il vettore B ha un secondo elemento che corrisponde a quello corrente di A,
                    //sposto l'indice su quell'elemento
                    bIdx = cdbB.IndexOf(currentValue, bIdx + 1);
                }
            }

        }

        private static void CercaSequenzeCircolari(List<MissioneTreno> missioni )
        {
            foreach (MissioneTreno missioneTreno in missioni)
            {
                for (int i = 0; i < missioneTreno.CdbList.Count; i++)
                {
                    int cdb = missioneTreno.CdbList[i];
                    
                    if (i < missioneTreno.CdbList.Count - 1)
                    {
                        int cdb2 = missioneTreno.CdbList[i + 1];

                        List<CdbVisitato> visitati = new List<CdbVisitato>();
                        List<MissioneTreno> missioniNuovo = missioni.ToList();
                        missioniNuovo.Remove(missioneTreno);

                        int loopDepth = RicercaLoop(missioniNuovo, missioneTreno, new[] { cdb, cdb2 }, visitati);
                        if (loopDepth > 2 && visitati.Count > 2 && visitati[0].Cdb == visitati[visitati.Count - 1].Cdb)
                        {
                            string trenoCorrente = null;
                            StringBuilder output = new StringBuilder();
                            foreach (CdbVisitato cdbVisitato in visitati)
                            {
                                if (string.Equals(cdbVisitato.NomeTreno, trenoCorrente))
                                {
                                    output.Append("," + cdbVisitato.Cdb );
                                }
                                else
                                {
                                    output.Append(" " + cdbVisitato.NomeTreno + ": " + cdbVisitato.Cdb);
                                    trenoCorrente = cdbVisitato.NomeTreno;
                                }
                            }
                             Console.WriteLine(output);
                            //string visitatiStr = string.Join(",", visitati);
                            //Console.WriteLine(loopDepth + ":" + visitatiStr);
                        }
                    }
                }
            }
        }

        private static int RicercaLoop(List<MissioneTreno> missioni, MissioneTreno corrente, int[] valori, List<CdbVisitato> visitati )
        {
            List<CdbVisitato> visitatiCurr = new List<CdbVisitato>(); 
            visitatiCurr.Add(new CdbVisitato {Cdb = valori[0], NomeTreno = corrente.NomeTreno});
            visitatiCurr.Add(new CdbVisitato {Cdb = valori[1], NomeTreno = corrente.NomeTreno });

            if (visitati.Any(cdbv => cdbv.Cdb == valori[1]))
            {
                if (visitati.Count > 4)
                {
                    visitati.AddRange(visitatiCurr);
                    return 1;
                }
                else
                {
                    return 0;
                }
            }

            visitati.AddRange(visitatiCurr);
            
            foreach (MissioneTreno missioneTreno in missioni)
            {
                if (missioneTreno == corrente)
                    continue;

                int bIdx = missioneTreno.CdbList.IndexOf(valori[1]);
                while (bIdx >= 0 && bIdx < missioneTreno.CdbList.Count - 1)
                {
                    List<MissioneTreno> missioniNuovo = missioni.ToList();
                    missioniNuovo.Remove(missioneTreno);
                    int ciclo = RicercaLoop(missioniNuovo, missioneTreno, new[] { missioneTreno.CdbList[bIdx], missioneTreno.CdbList[bIdx + 1] }, visitati);

                    if (ciclo > 0)
                    {
                        return ciclo + 1;
                    }

                    //Se il vettore B ha un secondo elemento che corrisponde a quello corrente di A,
                    //sposto l'indice su quell'elemento
                    bIdx = missioneTreno.CdbList.IndexOf(valori[1], bIdx+1);
                }
            }

            foreach (CdbVisitato cdbVisitato in visitatiCurr)
            {
                visitati.Remove(cdbVisitato);
            }
            return 0;
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

        private static List<MissioneTreno> CaricaMissioni(string nomefile)
        {
            List<MissioneTreno> missioni = new List<MissioneTreno>();

            if (!File.Exists(nomefile)) 
                return missioni;

            FileStream stream = null;
            StreamReader sr = null;
            try
            {
                stream = File.Open(nomefile, FileMode.Open, FileAccess.Read);
                sr = new StreamReader(stream);

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();

                    //Formato di una riga
                    // nometreno = [ x,y,z ]
                    if (!string.IsNullOrEmpty(line))
                    {
                        string[] tokens = line.Split('=');
                        if (tokens.Length > 1)
                        {
                            string nometreno = tokens[0].Trim();

                            string cdb = tokens[1].TrimStart(new[] { '[', ' ' });
                            cdb = cdb.TrimEnd(new[] { ']', ' ' });
                            cdb = cdb.Replace(" ", "");
                            List<string> cdbList = cdb.Split(',').ToList();
                            List<int> cdbListInt = cdbList.ConvertAll(Convert.ToInt32);

                            missioni.Add(new MissioneTreno(nometreno, cdbListInt));

                            Console.WriteLine("{0}: {1}", nometreno, cdb);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                if (sr != null)
                {
                    sr.Close();
                    sr.Dispose();
                }
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
            return missioni;
        }

        static void Main(string[] args)
        {
            Console.WriteLine();
            if (args.Length == 0)
            {
                Console.WriteLine("Utilizzo: GestioneAreeCritiche <nomefile>");
                return;
            }
            else if (!File.Exists(args[0]))
            {
                Console.WriteLine("Il file " + args[0] + " non esiste");
                Console.WriteLine("Utilizzo: GestioneAreeCritiche <nomefile>");
                return;
            }

            string nomefile = args[0];

            List<MissioneTreno> missioni = CaricaMissioni(nomefile);
            List<AreaCriticaLineare> aree = new List<AreaCriticaLineare>();

            //Confronto ogni vettore con quelli successivi
            for (int i = 0; i < missioni.Count; i++)
            {
                for (int j = i + 1; j < missioni.Count; j++)
                {
                    //Console.WriteLine("Confronto:" + missioni[i].NomeTreno + " - " + missioni[j].NomeTreno);
                    Confronto(missioni[i], missioni[j], aree);
                }
            }

            Console.WriteLine("-------");
            foreach (AreaCriticaLineare areaCriticaLineare in aree)
            {
                string trenisx = string.Join(",", areaCriticaLineare.TreniSinistra);
                string trenidx = string.Join(",", areaCriticaLineare.TreniDestra);
                string cdb = string.Join(",", areaCriticaLineare.Cdb);

                Console.WriteLine("{0,10} -> {1,10} <- {2,10}", trenisx, cdb, trenidx);
            }

            Console.WriteLine("-------");

            CercaSequenzeCircolari(missioni);


            Console.WriteLine("Press any key to exit...");
            Console.Read();

        }
    }
}
