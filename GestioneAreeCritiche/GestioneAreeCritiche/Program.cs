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
            Console.WriteLine("GestioneAreeCritiche -d <nomefile.umc> \t Trova i deadlock che possono verificarsi");
            Console.WriteLine("Press any key to exit...");
            Console.Read();
        }

        private static void Main(string[] args)
        {
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


                        bool statoFinaleRaggiunto;
                        List<Deadlock> deadlock;
                        //Trovo la lista dei deadlock che possono verificarsi
                        TrovaDeadlock.Trova(dati, out statoFinaleRaggiunto, out deadlock);

                        Console.WriteLine();
                        if (deadlock.Count > 0)
                        {
                            foreach (Deadlock dl in deadlock)
                            {
                                StringBuilder sb = new StringBuilder();
                                for (int i = 0; i < dati.MissioniAnnotate.Count; i++)
                                {
                                    MissioneAnnotata missione = dati.MissioniAnnotate[i];
                                    int posizione = dl.Positions[i];
                                    sb.AppendFormat("{0}:{1} ", missione.Trn, missione.ListaCdb[posizione]);
                                }

                                string treniBloccati = string.Join(",", dl.Bloccati);
                                Console.WriteLine("Deadlock: " + sb.ToString() + " Bloccati: " + treniBloccati);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Nessun Deadlock trovato");
                        }

                        break;
                    }
                default:
                    PrintUsage();
                    break;
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.Read();
        }

    }
}
