using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GestioneAreeCritiche.Output;

namespace GestioneAreeCritiche
{
    public class Program
    {
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

                            Console.WriteLine("{0}= [{1}]", nometreno, cdb);
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

        /// <summary>
        /// Genera i vettori annotati di output sia su console che su file di log
        ///0 : nessuna azione
        //+ / - 1: aumenta o diminuisci contatore del numero di treni nella regione
        //+ / - 2: aumenta o diminuisci il contatore del numero di treni entrati da destra
        //+ / - 3: aumenta o diminuisci il contatore del numero di treni entrati da sinistra
        //+ / - 4: aumenta o diminuisci il contatore del numero di treni che entrano ed escono dalla stessa direzione.
        /// </summary>
        private static StrutturaOutput GeneraStrutturaOutput(List<AreaCriticaLineare> areeLineari, List<AreaCriticaCircolare> areeCircolari, List<MissioneTreno> missioni)
        {
            StrutturaOutput res = new StrutturaOutput();

            //--- vettore delle aree critiche
            List<int> limitiAree = areeLineari.Select(areaCriticaLineare => 0).ToList();
            foreach (AreaCriticaCircolare areaCriticaCircolare in areeCircolari)
            {
                //La lista di cdb contiene cdb a coppie quindi il numero di  cdb è la sua metà
                //esempio: 1,2,2,3,3,4,4,1 => 1,2,3,4
                limitiAree.Add((areaCriticaCircolare.ListaCdb.Count / 2) - 1);
            }
            res.LimitiAree = limitiAree;


            //--- Generazione della lista di vettori annotati
            foreach (MissioneTreno missioneTreno in missioni)
            {
                MissioneAnnotata missioneAnnotata = new MissioneAnnotata();
                missioneAnnotata.Trn = missioneTreno.NomeTreno;
                res.MissioniAnnotate.Add(missioneAnnotata);

                Dictionary<AreaCriticaCircolare, int> areeCorrenti = new Dictionary<AreaCriticaCircolare, int>();
                Dictionary<int, int> uscitaAree = new Dictionary<int, int>();
                for (int cdbIndex = 0; cdbIndex < missioneTreno.CdbList.Count; cdbIndex++)
                {
                    int cdb = missioneTreno.CdbList[cdbIndex];
                    int[] azioni = new int[areeLineari.Count + areeCircolari.Count];

                    //Se l'ingresso in questo cdb comporta l'uscita da un area precedente, inizializzo
                    //l'azione corrispondente
                    foreach (KeyValuePair<int, int> valoreUscita in uscitaAree)
                    {
                        azioni[valoreUscita.Key] = valoreUscita.Value;
                    }
                    uscitaAree = new Dictionary<int, int>();

                    for (int i = 0; i < areeLineari.Count; i++)
                    {
                        AreaCriticaLineare areaCriticaLineare = areeLineari[i];

                        if (areaCriticaLineare.TreniSinistra.Contains(missioneTreno.NomeTreno) &&
                            areaCriticaLineare.Cdb[0] == cdb)
                        {
                            //entra sinistra
                            azioni[i] = 3;
                        }
                        if (areaCriticaLineare.TreniSinistra.Contains(missioneTreno.NomeTreno) &&
                            areaCriticaLineare.Cdb[areaCriticaLineare.Cdb.Count - 1] == cdb)
                        {
                            //esci sinistra
                            uscitaAree[i] = -3;
                        }

                        if (areaCriticaLineare.TreniDestra.Contains(missioneTreno.NomeTreno) &&
                            areaCriticaLineare.Cdb[areaCriticaLineare.Cdb.Count - 1] == cdb)
                        {
                            //entra destra
                            azioni[i] = 2;
                        }
                        if (areaCriticaLineare.TreniDestra.Contains(missioneTreno.NomeTreno) &&
                            areaCriticaLineare.Cdb[0] == cdb)
                        {
                            //esci destra
                            uscitaAree[i] = -2;
                        }
                    }

                    for (int i = 0; i < areeCircolari.Count; i++)
                    {
                        AreaCriticaCircolare areaCriticaCircolare = areeCircolari[i];

                        if (areaCriticaCircolare.Treni.Contains(missioneTreno.NomeTreno) &&
                            areaCriticaCircolare.ListaCdb.Contains(cdb))
                        {
                            if (!areeCorrenti.ContainsKey(areaCriticaCircolare))
                            {
                                // Sono entrato dentro una area circolare nuova
                                areeCorrenti.Add(areaCriticaCircolare, i);
                                azioni[i + areeLineari.Count] = 1;
                            }

                            if (cdbIndex < missioneTreno.CdbList.Count - 1)
                            {
                                //Se non sono l'ultimo cdb della missione, controllo se sto uscendo dall'area (ovvero il prossimo cdb non fa parte dell'area corrente)
                                int nextcdb = missioneTreno.CdbList[cdbIndex + 1];
                                if (!areaCriticaCircolare.ListaCdb.Contains(nextcdb))
                                {
                                    //Esco dall'area circolare (Nota: nelle missioni circolari si esce con un passo in anticipo)
                                    azioni[areeCorrenti[areaCriticaCircolare] + areeLineari.Count] = -1;
                                    areeCorrenti.Remove(areaCriticaCircolare);
                                }
                            }
                        }
                    }

                    missioneAnnotata.AzioniCdb.Add(azioni);
                    missioneAnnotata.ListaCdb.Add(cdb);
                }
            }

            return res;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Utilizzo:");
            Console.WriteLine("GestioneAreeCritiche <nomefile>");
            Console.WriteLine("GestioneAreeCritiche --convert <nomefile.umc>|<nomefile.xml>");
            Console.WriteLine("Press any key to exit...");
            Console.Read();
        }

        private static void Main(string[] args)
        {
            string nomefile = null;

            Console.WriteLine();

            if (args.Length == 1)
            {
                if (!File.Exists(args[0]))
                {
                    Console.WriteLine("Il file " + args[0] + " non esiste");
                    PrintUsage();
                    return;
                }

                nomefile = args[0];
            }
            else
            {
                PrintUsage();
                return;
            }

            List<MissioneTreno> missioni = CaricaMissioni(nomefile);

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
                    string cdb = string.Join(",", areaCriticaLineare.Cdb);

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
            Console.WriteLine("-------");

            StrutturaOutput output = GeneraStrutturaOutput(areeLineari, areeCircolari, missioni);

            //Scrivo l'output sulla console
            GenerazioneOutput.ToConsoleOutput(output);

            //Generazione output per UMC
            string outfile = Path.GetFileNameWithoutExtension(nomefile) + ".umc";
            GenerazioneOutput.ToUmc(output, outfile);

            //Generazione output XML
            outfile = Path.GetFileNameWithoutExtension(nomefile) + ".xml";
            GenerazioneOutput.ToXml(output, outfile);

            Console.WriteLine("Press any key to exit...");
            Console.Read();
        }
    }
}
