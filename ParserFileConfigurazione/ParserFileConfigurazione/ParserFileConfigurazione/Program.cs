using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ParserFileConfigurazione
{
    class Program
    {
        /// <summary>
        /// Legge un file xml contenente vettore dei limiti di area e lista di missioni annotate
        /// Ritorna un oggetto contenente i valori letti dal file
        /// </summary>
        internal static void ParseTabellaOrario(string sourcefile, List<Stazione> stazioni)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            settings.IgnoreComments = true;
            XmlReader myReader = XmlReader.Create(sourcefile, settings);
            Dictionary<int, List<int>> missioni = new Dictionary<int, List<int>>();
            List<int> puntiEntrata = null;
            List<int> puntiUscita = null;
            int trenoCorrente = -1;
            while (myReader.Read())
            {
                if (myReader.NodeType != XmlNodeType.Element)
                    continue;

                if (myReader.Name == "treno")
                {
                    trenoCorrente = Convert.ToInt32(myReader.GetAttribute("id"));
                    missioni.Add(trenoCorrente, new List<int>());

                    XmlReader stazioneReader = myReader.ReadSubtree();
                    int stazioneCorrente = -1;
                    
                    while (stazioneReader.Read())
                    {
                        if (stazioneReader.NodeType != XmlNodeType.Element)
                            continue;

                        if (stazioneReader.Name == "stazione")
                        {
                            stazioneCorrente = Convert.ToInt32(myReader.GetAttribute("id"));
                        }
                        else if (stazioneReader.Name == "itinerarioEntrata")
                        {
                            int idItinerario = Convert.ToInt32(myReader.GetAttribute("id"));
                            Stazione stazione = stazioni.First(st => st.Id == stazioneCorrente);
                            Itinerario itinerario = stazione.Entrata.First(it => it.Id == idItinerario);

                            List<int> puntiTrenoCorrente = missioni[trenoCorrente];

                            //se non c'è già, iserisco il punto di stazionamento
                            if (puntiTrenoCorrente.Count == 0 || puntiTrenoCorrente[puntiTrenoCorrente.Count - 1] != itinerario.PrevCdb)
                            {
                                puntiTrenoCorrente.Add(itinerario.PrevCdb);
                            }

                            //ultimo cdb dell'itinerario
                            puntiTrenoCorrente.Add(itinerario.Cdbs[itinerario.Cdbs.Count - 1]);
                        }
                        else if (stazioneReader.Name == "itinerarioUscita")
                        {
                            int idItinerario = Convert.ToInt32(myReader.GetAttribute("id"));
                            Stazione stazione = stazioni.First(st => st.Id == stazioneCorrente);
                            Itinerario itinerario = stazione.Uscita.First(it => it.Id == idItinerario);

                            List<int> puntiTrenoCorrente = missioni[trenoCorrente];

                            //se non c'è già, iserisco il punto di stazionamento
                            if (puntiTrenoCorrente.Count == 0 || puntiTrenoCorrente[puntiTrenoCorrente.Count - 1] != itinerario.PrevCdb)
                            {
                                puntiTrenoCorrente.Add(itinerario.PrevCdb);
                            }

                            //ultimo cdb dell'itinerario
                            puntiTrenoCorrente.Add(itinerario.Cdbs[itinerario.Cdbs.Count - 1]);
                        }
                    }


                }
            }

            foreach (int treno in missioni.Keys)
            {
                Console.Write(treno + ": ");
                Console.WriteLine(string.Join(",", missioni[treno]));
            }

        }

        internal static List<Stazione> CaricaStazioni(string sourcefile)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            settings.IgnoreComments = true;
            XmlReader myReader = XmlReader.Create(sourcefile, settings);
            List<Stazione> stazioni = new List<Stazione>();
            Stazione corrente = null;
            while (myReader.Read())
            {
                if (myReader.NodeType != XmlNodeType.Element)
                    continue;

                if (myReader.Name == "stazione")
                {
                    int id_stazione = Convert.ToInt32(myReader.GetAttribute("id_offset"));

                    corrente = new Stazione();
                    corrente.Id = id_stazione;
                    stazioni.Add(corrente);
                }
                else if (myReader.Name == "ingresso")
                {
                    Itinerario itinerario = new Itinerario();
                    int id = Convert.ToInt32(myReader.GetAttribute("id"));
                    int prevcdb = Convert.ToInt32(myReader.GetAttribute("prevcdb"));
                    int nextcdb = Convert.ToInt32(myReader.GetAttribute("nextcdb"));
                    itinerario.PrevCdb = prevcdb;
                    itinerario.NextCdb = nextcdb;
                    itinerario.Id = id;
                    XmlReader reader = myReader.ReadSubtree();
                    while (!reader.EOF)
                    {
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "cdb")
                        {
                            itinerario.Cdbs.Add(reader.ReadElementContentAsInt());
                        }
                        else
                        {
                            reader.Read();
                        }
                    }
                    corrente.Entrata.Add(itinerario);
                }
                else if (myReader.Name == "partenza")
                {
                    Itinerario itinerario = new Itinerario();
                    int id = Convert.ToInt32(myReader.GetAttribute("id"));
                    int prevcdb = Convert.ToInt32(myReader.GetAttribute("prevcdb"));
                    int nextcdb = Convert.ToInt32(myReader.GetAttribute("nextcdb"));
                    itinerario.PrevCdb = prevcdb;
                    itinerario.NextCdb = nextcdb;
                    itinerario.Id = id;
                    XmlReader reader = myReader.ReadSubtree();
                    while (!reader.EOF)
                    {
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "cdb")
                        {
                            itinerario.Cdbs.Add(reader.ReadElementContentAsInt());
                        }
                        else
                        {
                            reader.Read();
                        }
                    }
                    corrente.Uscita.Add(itinerario);
                }
            }
            return stazioni;
        }

        static void Main(string[] args)
        {
            var stazioni = CaricaStazioni("ConfigurazioneItinerari.xml");
            ParseTabellaOrario("TabellaOrario.xml", stazioni);
        }
    }
}
