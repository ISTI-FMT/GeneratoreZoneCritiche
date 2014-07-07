using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using GestioneAreeCritiche.AreeCritiche;
using GestioneAreeCritiche.Output;
using GestioneAreeCritiche.ModelChecking;
using GestioneAreeCritiche.TrovaAree;
using GestioneAreeCritiche.Conversione;
using GestioneAreeCritiche.ParserConfigurazioneATS;

namespace GestioneAreeCritiche
{
    public class Program
    {
        internal static void PrintUsage()
        {
            Console.WriteLine("Utilizzo:");
            Console.WriteLine("GestioneAreeCritiche -t <nomefile.txt> \t Trova le aree critiche nelle missioni elencate nel file e genera i file nomefile.xml e nomefile.umc");
            Console.WriteLine("GestioneAreeCritiche -x \t Trova le aree critiche nei file TabellaOrario.xml e ConfigurazioneItinerari.xml e genera i file AreeCritiche.xml e AreeCritiche.umc");
            Console.WriteLine("GestioneAreeCritiche -c <nomefile.umc>|<nomefile.xml> \t Converte un file da formato umc a xml e viceversa");
            Console.WriteLine("GestioneAreeCritiche -d <nomefile.umc> (-ignoraAree) \t Trova i deadlock che possono verificarsi. Se ignoraAree è settato, trova i deadlock ignorando i controlli di entrata nelle aree critiche");
            Console.WriteLine("Press any key to exit...");
            Console.Read();
        }        

        private static void Main(string[] args)
        {
            DateTime start = DateTime.Now;
            Console.WriteLine();

            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            switch (args[0])
            {
                case "-t":
                    {
                        if (args.Length < 2)
                        {
                            PrintUsage();
                            return;
                        }
                        string filename = args[1];
                        if (!File.Exists(filename))
                        {
                            Console.WriteLine("File {0} does not exist", filename);
                            return;
                        }
                        
                        TrovaAreeCritiche.Trova(filename);
                        break;
                    }
                case "-x":
                    {
                        ParserATS.Parse();
                        break;
                    }
                case "-c":
                    {
                        if (args.Length < 2)
                        {
                            PrintUsage();
                            return;
                        }
                        string filename = args[1];
                        if (!File.Exists(filename))
                        {
                            Console.WriteLine("File {0} does not exist", filename);
                            return;
                        }
                        Convertitore.Converti(filename);
                        break;
                    }
                case "-d":
                    {
                        if (args.Length < 2)
                        {
                            PrintUsage();
                            return;
                        }

                        string filename = args[1];
                        if (!File.Exists(filename))
                        {
                            Console.WriteLine("File {0} does not exist", filename);
                            return;
                        }

                        bool ignoraAree = false;
                        if (args.Length > 2)
                        {
                            if (string.Equals(args[2],"-ignoraAree", StringComparison.InvariantCultureIgnoreCase))
                            {
                                ignoraAree = true;
                            }
                        }


                        DatiAree dati = UmcParser.ParseUmc(filename);

                        //Ricalcolo quali sono le aree circolari e quali le lineari
                        //allo scopo di ricalcolare le missioni annotate
                        //In questo modo è possibile modificare il file UMC senza aggiornare a mano le annotazioni
                        List<AreaCriticaLineare> areeLineari = new List<AreaCriticaLineare>();
                        List<AreaCriticaCircolare> areeCircolari = new List<AreaCriticaCircolare>();
                        List<MissioneTreno> missioni = new List<MissioneTreno>();
                        foreach (IAreaCritica area in dati.AreeCritiche)
                        {
                            if (area is AreaCriticaCircolare)
                            {
                                areeCircolari.Add((AreaCriticaCircolare)area);
                            }
                            if (area is AreaCriticaLineare)
                            {
                                areeLineari.Add((AreaCriticaLineare)area);
                            }
                        }
                        foreach (MissioneAnnotata missione in dati.MissioniAnnotate)
                        {
                            missioni.Add(new MissioneTreno(missione.Trn, missione.ListaCdb));
                        }
                        dati = TrovaAreeCritiche.GeneraStrutturaOutput(areeLineari, areeCircolari, missioni);

                        //-----------------------
                        //Identifico deadlock

                        bool statoFinaleRaggiunto;
                        List<Deadlock> deadlock;
                        //Trovo la lista dei deadlock che possono verificarsi
                        TrovaDeadlock.Trova(dati, out statoFinaleRaggiunto, out deadlock, ignoraAree);

                        Console.WriteLine("Identified Deadlocks:");
                        if (deadlock.Count > 0)
                        {
                            //-------stampa in versione CVS
                            //foreach (MissioneAnnotata missione in dati.MissioniAnnotate)
                            //{
                            //    Console.Write(missione.Trn);
                            //    Console.Write(",");
                            //}
                            //Console.WriteLine();
                            //Console.WriteLine();

                            //foreach (Deadlock dl in deadlock)
                            //{
                            //    foreach (MissioneAnnotata missione in dati.MissioniAnnotate)
                            //    {
                            //        int pos = dl.Getposition(missione.Trn);
                            //        if (pos != -1)
                            //            Console.Write(pos);
                            //        Console.Write(",");
                            //    }
                            //    Console.WriteLine();
                            //}
                            //-------

                            Console.WriteLine();
                            foreach (Deadlock dl in deadlock)
                            {
                               Console.WriteLine(dl.ToString());
                            }
                        }
                        else
                        {
                            Console.WriteLine("No deadlock found");
                        }
                        break;
                    }
                default:
                    PrintUsage();
                    break;
            }

            DateTime end = DateTime.Now;

            TimeSpan elapsedTime = end - start;

            Console.WriteLine();
            Console.WriteLine("Elapsed time: {0}s", elapsedTime.TotalSeconds);
            Console.WriteLine("Press any key to exit...");
            Console.Read();
        }

    }
}
