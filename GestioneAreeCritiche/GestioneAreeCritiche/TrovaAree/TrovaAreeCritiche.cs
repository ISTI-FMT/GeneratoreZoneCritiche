﻿using GestioneAreeCritiche.AreeCritiche;
using GestioneAreeCritiche.ModelChecking;
using GestioneAreeCritiche.Output;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GestioneAreeCritiche.TrovaAree
{
    public class TrovaAreeCritiche
    {
        private const int NessunaAzione = 0;
        private const int EntraSinistra = 3;
        private const int EsciSinistra = -3;
        private const int EntraDestra = 2;
        private const int EsciDestra = -2;
        private const int EntraStessaDirezione = 4;
        private const int EsciStessaDirezione = -4;

        private const int EntrataCircolare = 1;
        private const int UscitaCircolare = -1;

        public static MissioneTreno CaricaMissione(string line)
        {
            MissioneTreno missione = null;
            //Formato di una riga
                    // nometreno = [ x,y,z ]
                    if (string.IsNullOrEmpty(line) || line[0] == '#')
                        return null;

                    string[] tokens = line.Split('=');
                    if (tokens.Length > 1)
                    {
                        string nometreno = tokens[0].Trim();

                        string cdb = tokens[1].TrimStart(new[] { '[', ' ' });
                        cdb = cdb.TrimEnd(new[] { ']', ' ' });
                        cdb = cdb.Replace(" ", "");
                        List<string> cdbList = cdb.Split(',').ToList();
                        List<int> cdbListInt = cdbList.ConvertAll(Convert.ToInt32);

                        missione = new MissioneTreno(nometreno, cdbListInt);

                        Console.WriteLine("{0}= [{1}]", nometreno, cdb);
                    }
               return missione;
        }

        public static List<MissioneTreno> CaricaMissioni(string missioniFull)
        {
            List<MissioneTreno> missioni = new List<MissioneTreno>();

            string[] missioniArr = missioniFull.Replace("\r","").Split(new[] { '\n' });

            foreach (string line in missioniArr)
            {
                MissioneTreno missione = CaricaMissione(line);
                if (missione != null)
                {
                    missioni.Add(missione);
                }
            }
            return missioni;
        }

        /// <summary>
        /// Ritorna una lista di missioni (lista di ID di CDB) a partire da un file di testo
        /// </summary>
        public static List<MissioneTreno> CaricaFileMissioni(string nomefile)
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

                    MissioneTreno missione = CaricaMissione(line);
                    if (missione != null)
                    {
                        missioni.Add(missione);
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

        public static IEnumerable<int> StartingIndex(List<int> x, List<int> y)
        {
            IEnumerable<int> index = Enumerable.Range(0, x.Count - y.Count + 1);
            for (int i = 0; i < y.Count; i++)
            {
                index = index.Where(n => x[n + i] == y[i]).ToArray();
            }
            return index;
        }

        internal static DatiAree GeneraStrutturaOutput(List<AreaCriticaLineare> areeLineari, List<AreaCriticaCircolare> areeCircolari, List<MissioneTreno> missioni)
        {
            DatiAree res = new DatiAree();
            res.AreeCritiche.AddRange(areeLineari);
            res.AreeCritiche.AddRange(areeCircolari);

            //--- Generazione della lista di vettori annotati
            foreach (MissioneTreno missioneTreno in missioni)
            {
                MissioneAnnotata missioneAnnotata = new MissioneAnnotata();
                //Inizializzazione del nome treno
                missioneAnnotata.Trn = missioneTreno.NomeTreno;
                //Inizializzazione della lista di cdb
                missioneAnnotata.ListaCdb.AddRange(missioneTreno.CdbList);
                //Inizializzazione delle azioni (zero di default)
                for (int i = 0; i < missioneTreno.CdbList.Count; i++)
                {
                    missioneAnnotata.AzioniCdb.Add(new int[res.AreeCritiche.Count]);
                }
                res.MissioniAnnotate.Add(missioneAnnotata);

                //annotazione aree lineari
                for (int i = 0; i < areeLineari.Count; i++)
                {
                    AreaCriticaLineare areaCriticaLineare = areeLineari[i];

                    //treni che entrano da sinistra
                    //if (areaCriticaLineare.TreniSinistra.Contains(missioneTreno.NomeTreno))
                    //{
                        //cerco tutti gli indici di inizio dell'area critica all'interno della missione
                        foreach (int indiceCdb in StartingIndex(missioneTreno.CdbList, areaCriticaLineare.ListaCdb))
                        {
                            missioneAnnotata.AzioniCdb[indiceCdb][i] = EntraSinistra;

                            if (indiceCdb + areaCriticaLineare.ListaCdb.Count < missioneAnnotata.AzioniCdb.Count)
                            {
                                missioneAnnotata.AzioniCdb[indiceCdb + areaCriticaLineare.ListaCdb.Count][i] = EsciSinistra;
                            }
                        }
                    //}

                    //treni che entrano da destra
                    //if (areaCriticaLineare.TreniDestra.Contains(missioneTreno.NomeTreno))
                    //{
                        //visto che i treni entrano da destra, devo cercare i cdb in ordine inverso
                        List<int> cdbInvertiti = areaCriticaLineare.ListaCdb.ToList();
                        cdbInvertiti.Reverse();

                        //cerco tutti gli indici di inizio dell'area critica all'interno della missione
                        foreach (int indiceCdb in StartingIndex(missioneTreno.CdbList, cdbInvertiti))
                        {
                            missioneAnnotata.AzioniCdb[indiceCdb][i] = EntraDestra;

                            if (indiceCdb + areaCriticaLineare.ListaCdb.Count < missioneAnnotata.AzioniCdb.Count)
                            {
                                missioneAnnotata.AzioniCdb[indiceCdb + areaCriticaLineare.ListaCdb.Count][i] = EsciDestra;
                            }
                        }
                    //}
                }

                //Annotazione aree circolari
                Dictionary<AreaCriticaCircolare, int> areeCorrenti = new Dictionary<AreaCriticaCircolare, int>();
                for (int cdbIndex = 0; cdbIndex < missioneTreno.CdbList.Count; cdbIndex++)
                {
                    int cdb = missioneTreno.CdbList[cdbIndex];

                    //Se l'ingresso in questo cdb comporta l'uscita da un area precedente, inizializzo
                    //l'azione corrispondente
                    for (int i = 0; i < areeCircolari.Count; i++)
                    {
                        AreaCriticaCircolare areaCriticaCircolare = areeCircolari[i];

                        //il cdb e il treno sono dell'area critica corrente
                        if (/*areaCriticaCircolare.Treni.Contains(missioneTreno.NomeTreno) &&*/
                            areaCriticaCircolare.ListaCdb.Contains(cdb))
                        {
                            bool entrato = false;
                            if (!areeCorrenti.ContainsKey(areaCriticaCircolare))
                            {
                                // Sono entrato dentro una area circolare nuova
                                areeCorrenti.Add(areaCriticaCircolare, i);
                                missioneAnnotata.AzioniCdb[cdbIndex][i + areeLineari.Count] = EntrataCircolare;
                                entrato = true;
                            }

                            if (cdbIndex < missioneTreno.CdbList.Count - 1)
                            {
                                //Se non sono l'ultimo cdb della missione, controllo se sto uscendo dall'area (ovvero il prossimo cdb non fa parte dell'area corrente)
                                int nextcdb = missioneTreno.CdbList[cdbIndex + 1];
                                if (!areaCriticaCircolare.ListaCdb.Contains(nextcdb))
                                {
                                    //Sto uscendo dall'area critica...
                                    //Se ero appena entrato nell'area => annullo l'entrata
                                    //altrimenti => registro l'uscita
                                    //Nota: nelle missioni circolari si esce con un passo in anticipo
                                    if (entrato)
                                    {
                                        missioneAnnotata.AzioniCdb[cdbIndex][areeCorrenti[areaCriticaCircolare] + areeLineari.Count] = 0;
                                    }
                                    else
                                    {
                                        missioneAnnotata.AzioniCdb[cdbIndex][areeCorrenti[areaCriticaCircolare] + areeLineari.Count] = UscitaCircolare;
                                    }
                                    areeCorrenti.Remove(areaCriticaCircolare);
                                }
                            }
                        }
                    }
                }
            }

            return res;
        }

        public static DatiAree Trova(List<MissioneTreno> missioni, bool ignoraFalsiPositivi, bool trovaDeadlock)
        {
            //------- aree lineari
            List<AreaCriticaLineare> areeLineari = RicercaAreeLineari.Ricerca(missioni);

            if (areeLineari.Count > 0)
            {
                Console.WriteLine("-------");
                Console.WriteLine("Aree Lineari:");

                foreach (AreaCriticaLineare areaCriticaLineare in areeLineari)
                {
                    string trenisx = string.Join(",", areaCriticaLineare.TreniSinistra);
                    string trenidx = string.Join(",", areaCriticaLineare.TreniDestra);
                    string cdb = string.Join(",", areaCriticaLineare.ListaCdb);

                    Console.WriteLine("{0,10} -> {1,10} <- {2,10}", trenisx, cdb, trenidx);
                }

                Console.WriteLine();
            }

            //------- aree circolari
            List<AreaCriticaCircolare> areeCircolari = RicercaAreeCircolari.Ricerca(missioni);

            if (areeCircolari.Count > 0)
            {
                Console.WriteLine("-------");
                Console.WriteLine("Aree Circolari:");

                foreach (AreaCriticaCircolare areaCriticaCircolare in areeCircolari)
                {
                    Console.WriteLine("{0,10} : {1,10}", string.Join(",", areaCriticaCircolare.Treni),
                        string.Join(",", areaCriticaCircolare.ListaCdb.Distinct()));
                }

                Console.WriteLine();
            }
            
            DatiAree output;
            output = GeneraStrutturaOutput(areeLineari, areeCircolari, missioni);

            if (trovaDeadlock)
            {
                Console.WriteLine("-------");
                Console.WriteLine("Identifying Deadlocks....");
                Console.WriteLine();

                bool statoFinaleRaggiungibile;
                List<Deadlock> deadlocks;
                TrovaDeadlock.Trova(output, out statoFinaleRaggiungibile, out deadlocks, false, ignoraFalsiPositivi);
                output.DeadlockConosciuti = deadlocks;
                if (deadlocks.Count > 0)
                {
                    foreach (Deadlock dl in deadlocks)
                    {
                        Console.WriteLine("Deadlock: " + dl.ToString());
                    }
                }

            }
            return output;
        }

        internal static void Trova(string nomefile, bool ignoraFalsiPositivi)
        {
            List<MissioneTreno> missioni = CaricaFileMissioni(nomefile);

            DatiAree output = Trova(missioni, ignoraFalsiPositivi, true);

            GeneraOutput(output, nomefile);
        }

        public static void GeneraOutput(DatiAree output, string nomefile)
        {
            Console.WriteLine("-------");
            Console.WriteLine("Generating Output....");

            //Generazione output per UMC
            string outfile = Path.GetFileNameWithoutExtension(nomefile) + ".umc";
            Console.WriteLine("Generating {0}", outfile);
            GenerazioneOutput.ToUmc(output, outfile);

            //Generazione output XML
            outfile = Path.GetFileNameWithoutExtension(nomefile) + ".xml";
            Console.WriteLine("Generating {0}", outfile);
            GenerazioneOutput.ToXml(output, outfile);
        }


    }
}
