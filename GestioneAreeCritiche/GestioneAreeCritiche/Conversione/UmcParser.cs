using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GestioneAreeCritiche.AreeCritiche;
using GestioneAreeCritiche.Output;

namespace GestioneAreeCritiche.Conversione
{
    internal class UmcParser
    {
        enum SezioneUmc
        {
            Aree, Missioni, Constraints, Nessuna
        }

        /// <summary>
        /// Legge un file compatibile con umc contenente vettore dei limiti di area e lista di missioni annotate
        /// Ritorna un oggetto contenente i valori letti dal file
        /// </summary>
        internal static DatiAree ParseUmc(string filename)
        {
            DatiAree res = new DatiAree();
            Dictionary<string, MissioneAnnotata> missioniAnnotate = new Dictionary<string, MissioneAnnotata>();
            try
            {
                FileStream sr = File.OpenRead(filename);
                StreamReader streamReader = new StreamReader(sr);

                SezioneUmc sezioneCorrente = SezioneUmc.Nessuna;
                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine();

                    try
                    {
                        if (string.IsNullOrEmpty(line) || line.Trim() == string.Empty)
                        {
                            continue;
                        }

                        if (line.StartsWith("#"))
                        {
                            if (line.StartsWith("#Aree", StringComparison.InvariantCultureIgnoreCase))
                            {
                                sezioneCorrente = SezioneUmc.Aree;
                            }
                            else if (line.StartsWith("#Missioni", StringComparison.InvariantCultureIgnoreCase))
                            {
                                sezioneCorrente = SezioneUmc.Missioni;
                            }
                            else if (line.StartsWith("#Constraints", StringComparison.InvariantCultureIgnoreCase))
                            {
                                sezioneCorrente = SezioneUmc.Constraints;
                            }
                        }
                        else
                        {
                            switch (sezioneCorrente)
                            {
                                case SezioneUmc.Aree:
                                    //formato:
                                    //0: [7,8], 0
                                    //1: [1,6], 0
                                    {
                                        string[] areaStr = line.Split(':');
                                        if (areaStr.Length != 2)
                                        {
                                            Console.WriteLine("Linea non valida, ignorata:" + line);
                                        }
                                        else
                                        {
                                            //La regex cerca la sottostringa nel formato 
                                            //[ <numeri> <spazi> <virgole> ]
                                            Regex regex = new Regex(@" \[(\d|\s|,)+[^[]*\]", RegexOptions.Compiled);
                                            if (regex.IsMatch(areaStr[1]))
                                            {
                                                Match match = regex.Match(areaStr[1]);
                                                string[] cdbs = match.Value.Trim(new[] { ']', '[', ' ' }).Split(',');
                                                List<int> cdbsInt = cdbs.Select(cdb => Convert.ToInt32(cdb)).ToList();

                                                //Tolgo la regex dalla stringa, quindi mi rimane solo ,0
                                                string remaining = regex.Replace(areaStr[1], string.Empty);
                                                string limiteStr = remaining.Trim(new[] { ' ', ',' });
                                                int limiteInt = Convert.ToInt32(limiteStr);

                                                IAreaCritica area;
                                                if (limiteInt > 0)
                                                {
                                                    area = new AreaCriticaCircolare();
                                                    area.ListaCdb = cdbsInt;
                                                    ((AreaCriticaCircolare)area).Limite = limiteInt;
                                                }
                                                else
                                                {
                                                    area = new AreaCriticaLineare(cdbsInt);
                                                }
                                                res.AreeCritiche.Add(area);
                                            }
                                        }

                                        break;
                                    }
                                case SezioneUmc.Missioni:
                                    //formato:
                                    //R: 9,8,6,1
                                    {
                                        string[] missioniStr = line.Split(':');
                                        if (missioniStr.Length != 2)
                                        {
                                            Console.WriteLine("Linea non valida, ignorata:" + line);
                                        }
                                        else
                                        {
                                            //primo token = nome del treno
                                            string nometreno = missioniStr[0].Trim(new[] { ' ', ':' });

                                            //secondo token = lista di cdb
                                            string[] cdbs = missioniStr[1].Split(',');
                                            List<int> cdbsList = cdbs.Select(cdb => Convert.ToInt32(cdb)).ToList();

                                            MissioneAnnotata missione = new MissioneAnnotata();
                                            missione.Trn = nometreno;
                                            missione.ListaCdb.AddRange(cdbsList);

                                            missioniAnnotate[nometreno] = missione;
                                        }
                                        break;
                                    }
                                case SezioneUmc.Constraints:
                                    //formato:
                                    //G: [0,3,0,0],[0,0,3,0],[3,-3,0,0],[0,0,-3,0]
                                    {
                                        string[] missioniStr = line.Split(':');
                                        if (missioniStr.Length != 2)
                                        {
                                            Console.WriteLine("Linea non valida, ignorata:" + line);
                                        }
                                        else
                                        {
                                            string nometreno = missioniStr[0].Trim(new[] { ' ', ':' });

                                            //La regex cerca sottostringhe nel formato 
                                            //[ <numeri> <spazi> <virgole> <+> <-> ]
                                            //questa parte [^\[]* indica di non essere greedy (si ferma quando trova una [)
                                            Regex regex = new Regex(@"\[(\d|\s|,|\+|\-)+[^\[]*\]", RegexOptions.Compiled);
                                            if (regex.IsMatch(missioniStr[1]))
                                            {
                                                MatchCollection matches = regex.Matches(missioniStr[1]);

                                                foreach (Match match in matches)
                                                {
                                                    string matchStr = match.Value.Trim(new[] { '[', ']', ' ' });
                                                    string[] azioniStr = matchStr.Split(',');
                                                    int[] azioniArray = azioniStr.Select(azioneStr => Convert.ToInt32(azioneStr)).ToArray();

                                                    if (missioniAnnotate.ContainsKey(nometreno))
                                                    {
                                                        missioniAnnotate[nometreno].AzioniCdb.Add(azioniArray);
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Errore durante il parsing del file. Missione del treno {0} non trovata.", nometreno);
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case SezioneUmc.Nessuna:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error while parsing the file");
                        Console.WriteLine("Error:" + ex.Message);
                        Console.WriteLine("Line:" + line);
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error while reading the file: " + ex.ToString());
            }

            res.MissioniAnnotate.AddRange(missioniAnnotate.Values);
            return res;
        }
    }
}
