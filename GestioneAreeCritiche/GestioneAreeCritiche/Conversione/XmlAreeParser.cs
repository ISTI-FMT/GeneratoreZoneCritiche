using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using GestioneAreeCritiche.AreeCritiche;
using GestioneAreeCritiche.Output;

namespace GestioneAreeCritiche.Conversione
{
    internal  static class XmlAreeParser
    {
        /// <summary>
        /// Legge un file xml contenente vettore dei limiti di area e lista di missioni annotate
        /// Ritorna un oggetto contenente i valori letti dal file
        /// </summary>
        internal static DatiAree ParseXml(string sourcefile)
        {
            DatiAree res = new DatiAree();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            settings.IgnoreComments = true;
            XmlReader myReader = XmlReader.Create(sourcefile, settings);
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

                    List<int> listaCdb = new List<int>();
                    XmlReader cdbReader = myReader.ReadSubtree();
                    while (cdbReader.Read())
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
                        ((AreaCriticaCircolare)area).Limite = listaCdb.Count - 1;
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
    }
}
