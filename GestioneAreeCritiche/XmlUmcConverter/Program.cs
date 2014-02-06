using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using GestioneAreeCritiche.AreeCritiche;
using GestioneAreeCritiche.Output;

namespace XmlUmcConverter
{
    class Program
    {
        /// <summary>
        /// Legge un file compatibile con umc contenente vettore dei limiti di area e lista di missioni annotate
        /// Ritorna un oggetto contenente i valori letti dal file
        /// </summary>
        private static StrutturaOutput LeggiUmc(string filename)
        {
            StrutturaOutput res = new StrutturaOutput();
            try
            {
                FileStream sr = File.OpenRead(filename);
                StreamReader streamReader = new StreamReader(sr);
                
                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine();
                    if (string.IsNullOrEmpty(line) || line.Trim() == string.Empty || line.StartsWith("#"))
                    {
                        continue;
                    }

                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        line = line.TrimStart('[');
                        line = line.TrimEnd(']');
                      //  res.LimitiAree = line.Split(',').Select(limite => Convert.ToInt32(limite.Trim())).ToList();
                    }
                    else
                    {
                        MissioneAnnotata missione = new MissioneAnnotata();
                        res.MissioniAnnotate.Add(missione);

                        //controllo se line è nella forma
                        //V: [2,0,0,0] 8, [0,0,0,0] 7, [-2,0,0,0] 5
                        string[] missioneStr = line.Split(':');
                        if (missioneStr.Length != 2)
                        {
                            Console.WriteLine("Invalid line:" + line);
                        }
                        else
                        {
                            missione.Trn = missioneStr[0];

                            //listacdb contiene stringhe nella forma [0,0,0] 4
                            Regex regex = new Regex(@"\[(\d|\s|,|-|\+)+\](\w|\s)+", RegexOptions.Compiled);
                            MatchCollection matches = regex.Matches(missioneStr[1]);
                            foreach (Match match in matches)
                            {
                                string token = match.Value;
                                try
                                {
                                    string[] cdbStrTokens = token.Split(']');
                                    string cdb = cdbStrTokens[1].Trim();
                                    string[] azioniStrTokens = cdbStrTokens[0].Trim(new[] { '[', ']',' ' }).Split(',');
                                    IEnumerable<int> azioniInt = azioniStrTokens.Select(azioneStr => Convert.ToInt32(azioneStr));

                                    missione.ListaCdb.Add(Convert.ToInt32(cdb));
                                    missione.AzioniCdb.Add(azioniInt.ToArray());
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine("Token non corretto: " + token);
                                }
                            }
                        }
                    }

                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Errore durante la lettura del file: " + ex.ToString());
            }
            return res;
        }

        /// <summary>
        /// Legge un file xml contenente vettore dei limiti di area e lista di missioni annotate
        /// Ritorna un oggetto contenente i valori letti dal file
        /// </summary>
        private static StrutturaOutput LeggiXml(string sourcefile)
        {
            StrutturaOutput res = new StrutturaOutput();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            settings.IgnoreComments = true;
            XmlReader myReader = XmlReader.Create(sourcefile,settings);
            SortedDictionary<int, int> limiti = new SortedDictionary<int, int>();
            MissioneAnnotata missioneCorrente = null;
            while (myReader.Read())
            {
                if (myReader.NodeType != XmlNodeType.Element) 
                    continue;


                if (myReader.Name == "Area")
                {
                    int id = Convert.ToInt32(myReader.GetAttribute("Id"));
                    //NB:Nel caso di aree circolari, il limite deve essere coerente con i cdb dell'area
                    int limite = Convert.ToInt32(myReader.GetAttribute("Limite"));
                    limiti[id] = Convert.ToInt32(limite);
                    
                    List<int>listaCdb = new List<int>();
                    XmlReader cdbReader = myReader.ReadSubtree();
                    while ( cdbReader.Read())
                    {
                        if (cdbReader.NodeType == XmlNodeType.Element 
                            && cdbReader.Name == "Cdb")
                        {
                            int cdbId = Convert.ToInt32(myReader.GetAttribute("Id"));
                            listaCdb.Add(cdbId);
                        }
                    }

                    IAreaCritica area;
                    if (limite == 0)
                    {
                        area = new AreaCriticaLineare(listaCdb);
                    }
                    else
                    {
                        area = new AreaCriticaCircolare();
                        area.ListaCdb = listaCdb;
                    }
                    res.AreeCritiche.Add(area);
                }
                else if (myReader.Name == "Missione")
                {
                    missioneCorrente = new MissioneAnnotata();
                    missioneCorrente.Trn = myReader.GetAttribute("Trn");
                    res.MissioniAnnotate.Add(missioneCorrente);
                }
                else if (myReader.Name == "Cdb")
                {
                    SortedDictionary<int, int> azioniCdbCorrente = new SortedDictionary<int, int>();
                    if (missioneCorrente != null)
                    {
                        int cdbId = Convert.ToInt32(myReader.GetAttribute("Id"));
                        missioneCorrente.ListaCdb.Add(cdbId);
                    }

                    XmlReader azioniReader = myReader.ReadSubtree();

                    //leggo il contenuto di Cdb => una lista di elementi Azione
                    while (!azioniReader.EOF)
                    {
                        if (azioniReader.NodeType == XmlNodeType.Element 
                            && azioniReader.Name == "Azione")
                        {
                            string idAreaStr = azioniReader.GetAttribute("IdArea");
                            //NOTA: Quando faccio read element content sto effettivamente muovendo il 
                            //reader, quindi se faccio un secondo Read() mi salto un pezzo
                            int azione = azioniReader.ReadElementContentAsInt();

                            azioniCdbCorrente[Convert.ToInt32(idAreaStr)] = azione;
                        }
                        else
                        {
                            azioniReader.Read();
                        }
                    }

                    if (missioneCorrente != null)
                    {
                        //Creo, settate a zero (nessuna azione deve essere compiuta sulle aree corrispondenti), tutte le azioni che mancano
                        foreach (KeyValuePair<int, int> keyValuePair in limiti)
                        {
                            if (!azioniCdbCorrente.ContainsKey(keyValuePair.Key))
                            {
                                azioniCdbCorrente[keyValuePair.Key] = 0;
                            }
                        }

                        missioneCorrente.AzioniCdb.Add(azioniCdbCorrente.Values.ToArray());
                    }
                }
            }
            return res;
        }


        private static void UmcToXml(string sourceFile, string destFile)
        {
            StrutturaOutput strutturaOutput = LeggiUmc(sourceFile);

            string outfile = Path.GetFileNameWithoutExtension(sourceFile) + "_2.umc";
            GenerazioneOutput.ToUmc(strutturaOutput, outfile);
        }

        private static void XmlToUmc(string sourceFile, string destFile)
        {
            StrutturaOutput strutturaOutput = LeggiXml(sourceFile);

            string outfile = Path.GetFileNameWithoutExtension(sourceFile) + "_2.xml";
            GenerazioneOutput.ToXml(strutturaOutput, outfile);
        }


        private static void PrintUsage()
        {
            Console.WriteLine("Utilizzo:");
            Console.WriteLine("XmlUmcConverter <nomefile>.umc|.xml");
            Console.WriteLine("Press any key to exit...");
            Console.Read();
        }

        static void Main(string[] args)
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

            string extension = Path.GetExtension(nomefile);
            if (extension == ".xml")
            {
                string outfile = Path.GetFileNameWithoutExtension(nomefile) + ".umc";
                XmlToUmc(nomefile, outfile);
            }
            else if (extension == ".umc")
            {
                string outfile = Path.GetFileNameWithoutExtension(nomefile) + ".xml";
                UmcToXml(nomefile, outfile);
            }
        }
    }
}
