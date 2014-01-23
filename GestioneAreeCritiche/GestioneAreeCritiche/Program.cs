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
            List<AreaCriticaLineare> listaAree = new List<AreaCriticaLineare>();

            //Confronto ogni vettore con quelli successivi
            for (int i = 0; i < missioni.Count; i++)
            {
                for (int j = i + 1; j < missioni.Count; j++)
                {
                    //Console.WriteLine("Confronto:" + missioni[i].NomeTreno + " - " + missioni[j].NomeTreno);
                    listaAree.AddRange(RicercaAreeLineari.Ricerca(missioni[i], missioni[j]));
                }
            }

            Console.WriteLine("-------");
            foreach (AreaCriticaLineare areaCriticaLineare in listaAree)
            {
                string trenisx = string.Join(",", areaCriticaLineare.TreniSinistra);
                string trenidx = string.Join(",", areaCriticaLineare.TreniDestra);
                string cdb = string.Join(",", areaCriticaLineare.Cdb);

                Console.WriteLine("{0,10} -> {1,10} <- {2,10}", trenisx, cdb, trenidx);
            }

            Console.WriteLine("-------");

            RicercaAreeCircolari.Ricerca(missioni);


            Console.WriteLine("Press any key to exit...");
            Console.Read();

        }
    }
}
