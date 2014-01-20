using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestioneAreeCritiche
{
    public class Program
    {
        private const string NomeFile = "missioni.txt";

        private static void Confronto(List<int> cdbA, List<int> cdbB)
        {
            for (int currentIdx = 0; currentIdx < cdbA.Count; currentIdx++)
            {
                int currentValue = cdbA[currentIdx];

                int bIdx = cdbB.IndexOf(currentValue);
                if (bIdx > 0)
                {
                    List<int> area = ConfrontoSingolo(cdbA, cdbB, currentIdx, bIdx);

                    if (area.Count > 1)
                    {
                        string maxArea = string.Join(",", area);
                        Console.WriteLine(maxArea);
                    }
                }
            }
           
        }

        private static List<int> ConfrontoSingolo(List<int> cdbA, List<int> cdbB, int startA, int startB)
        {
            List<int> res = new List<int>();

            int j = startB;
            for (int i = startA; i < cdbA.Count; i++)
            {
                if (j < 0)
                {
                    break;
                }

                int valA = cdbA[i];
                int valB = cdbB[j];

                if (valA == valB)
                {
                    res.Add(valA);
                }

                j--;
            }

            return res;
        }

        static void Main(string[] args)
        {
            List<MissioneTreno> missioni = new List<MissioneTreno>();

            if (File.Exists(NomeFile))
            {
                try
                {
                    FileStream stream = File.Open(NomeFile, FileMode.Open, FileAccess.Read);
                    StreamReader sr = new StreamReader(stream);

                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();

                        if (!string.IsNullOrEmpty(line))
                        {
                            string[] tokens = line.Split('=');
                            if (tokens.Length > 1)
                            {
                                string nometreno = tokens[0].Trim();

                                string cdb = tokens[1].TrimStart(new []{'[' , ' '});
                                cdb = cdb.TrimEnd(new[] { ']', ' ' });
                                List<string> cdbList = cdb.Split(',').ToList();
                                List<int> cdbListInt = cdbList.ConvertAll(Convert.ToInt32);

                                Console.WriteLine("Treno:" + nometreno);
                                Console.WriteLine("Cdb:"+ cdb);

                                missioni.Add(new MissioneTreno(nometreno, cdbListInt));
                            }
                        }
                    }

                    sr.Close();
                    stream.Close();
                    sr.Dispose();
                    stream.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                for (int i = 0; i < missioni.Count; i++)
                {

                    for (int j = 0; j < missioni.Count; j++)
                    {
                        Console.WriteLine("Confronto:" + missioni[i].NomeTreno + " - " + missioni[j].NomeTreno);
                        Confronto(missioni[i].CdbList, missioni[j].CdbList);
                    }
                }
               

                Console.WriteLine("Press any key to exit...");
                Console.Read();
            }
        }
    }
}
