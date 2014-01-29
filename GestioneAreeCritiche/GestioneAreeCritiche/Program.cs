using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        private static void GeneraOutput(List<AreaCriticaLineare> areeLineari, List<AreaCriticaCircolare> areeCircolari, List<MissioneTreno> missioni, StreamWriter sw)
        {
            Console.WriteLine("Vettore aree critiche:");
            StringBuilder builderAree = new StringBuilder();
            builderAree.Append('[');
            foreach (AreaCriticaLineare areaCriticaLineare in areeLineari)
            {
                builderAree.Append('0');
                builderAree.Append(',');
            }
            foreach (AreaCriticaCircolare areaCriticaCircolare in areeCircolari)
            {
                //La lista di cdb contiene cdb a coppie quindi il numero di  cdb è la sua metà
                //esempio: 1,2,2,3,3,4,4,1 => 1,2,3,4
                builderAree.Append((areaCriticaCircolare.ListaCdb.Count / 2) - 1);
                builderAree.Append(',');
            }
            builderAree.Remove(builderAree.Length - 1, 1);
            builderAree.Append(']');
            Console.WriteLine(builderAree.ToString());

            if (sw != null)
            {
                sw.WriteLine(builderAree.ToString());
            }

            Console.WriteLine();

            Console.WriteLine("Vettori annotati:");
            //0 : nessuna azione
            //+ / - 1: aumenta o diminuisci contatore del numero di treni nella regione
            //+ / - 2: aumenta o diminuisci il contatore del numero di treni entrati da destra
            //+ / - 3: aumenta o diminuisci il contatore del numero di treni entrati da sinistra
            //+ / - 4: aumenta o diminuisci il contatore del numero di treni che entrano ed escono dalla stessa direzione.
            foreach (MissioneTreno missioneTreno in missioni)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(missioneTreno.NomeTreno + ":");

                Dictionary<AreaCriticaCircolare, int> areeCorrenti = new Dictionary<AreaCriticaCircolare, int>();
                Dictionary<int, int> uscitaAree = new Dictionary<int, int>();
                foreach (int cdb in missioneTreno.CdbList)
                {
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
                            areaCriticaCircolare.ListaCdb.Contains(cdb) &&
                            !areeCorrenti.ContainsKey(areaCriticaCircolare))
                        {
                            // Sono entrato dentro una area circolare nuova
                            areeCorrenti.Add(areaCriticaCircolare, i);
                            azioni[i + areeLineari.Count] = 1;
                        }
                        else
                        {
                            List<AreaCriticaCircolare> toremove = new List<AreaCriticaCircolare>();

                            foreach (AreaCriticaCircolare criticaCircolare in areeCorrenti.Keys)
                            {
                                if (!criticaCircolare.ListaCdb.Contains(cdb))
                                {
                                    //Sono uscito dall'area circolare
                                    azioni[areeCorrenti[criticaCircolare] + areeLineari.Count] = -1;

                                    toremove.Add(criticaCircolare);
                                }
                            }

                            //Rimuovo le aree dalle quali sono uscito
                            foreach (AreaCriticaCircolare criticaCircolare in toremove)
                            {
                                areeCorrenti.Remove(criticaCircolare);
                            }
                        }
                    }
                    

                    sb.AppendFormat(" [{0}] {1},",string.Join(",", azioni), cdb);
                }

                string vettoreOut = sb.ToString().TrimEnd(',');
                Console.WriteLine(vettoreOut);

                if (sw != null)
                {
                    sw.WriteLine(vettoreOut);
                }
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine();
            if (args.Length == 0)
            {
                Console.WriteLine("Utilizzo: GestioneAreeCritiche <nomefile>");
                Console.WriteLine("Press any key to exit...");
                Console.Read();
                return;
            }
            else if (!File.Exists(args[0]))
            {
                Console.WriteLine("Il file " + args[0] + " non esiste");
                Console.WriteLine("Utilizzo: GestioneAreeCritiche <nomefile>");
                Console.WriteLine("Press any key to exit...");
                Console.Read();
                return;
            }

            string nomefile = args[0];

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

            string outfile = Path.GetFileNameWithoutExtension(nomefile) + ".umc";
            StreamWriter sw = null;
            FileStream fs = null;
            try
            {
                if (File.Exists(outfile))
                {
                    File.Delete(outfile);
                }
                fs = File.OpenWrite(outfile);
                sw = new StreamWriter(fs);
            }
            catch (Exception)
            {
                Console.WriteLine("Errore: Impossibile scrivere sul file " + outfile);
            }
            GeneraOutput(areeLineari, areeCircolari, missioni, sw);


            if (sw != null)
            {
                sw.Flush();
                sw.Close();
                sw.Dispose();
            }
            if (fs != null)
            {
                fs.Close();
                fs.Dispose();
            }


            Console.WriteLine("Press any key to exit...");
            Console.Read();

        }
    }
}
