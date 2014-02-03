using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace GestioneAreeCritiche
{
    internal class StrutturaOutput
    {
        internal StrutturaOutput()
        {
            MissioniAnnotate = new List<MissioneAnnotata>();
            LimitiAree = new List<int>();
        }

        internal List<int> LimitiAree { get; set; }

        internal List<MissioneAnnotata> MissioniAnnotate { get; private set; }
    }

    internal class MissioneAnnotata
    {
        internal MissioneAnnotata()
        {
            ListaCdb = new List<int>();
            AzioniCdb = new List<int[]>();
        }


        internal List<int> ListaCdb { get; private set; }

        internal List<int[]> AzioniCdb { get; private set; }

        internal String Trn { get; set; }
    }

    internal static class Output
    {
        internal static void GenerazioneUmc(StrutturaOutput output, string outfile)
        {
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
                return;
            }

            Console.WriteLine("Vettore aree critiche:");
            StringBuilder sb = new StringBuilder();
            sb.Append('[');
            sb.Append(string.Join(",", output.LimitiAree));
            sb.Append(']');
            Console.WriteLine(sb.ToString());

            sw.WriteLine(sb.ToString());

            
            
            foreach (MissioneAnnotata missione in output.MissioniAnnotate)
            {
                sb = new StringBuilder();
                sb.Append(missione.Trn + ":");
                for (int index = 0; index < missione.ListaCdb.Count; index++)
                {
                    int cdb = missione.ListaCdb[index];
                    int[] azioni = missione.AzioniCdb[index];
                    sb.AppendFormat(" [{0}] {1},", string.Join(",", azioni), cdb);
                }
                sw.WriteLine(sb.ToString());
            }


            sw.Flush();
            sw.Close();
            sw.Dispose();
            fs.Close();
            fs.Dispose();
        }

        internal static void GenerazioneXml(StrutturaOutput output, string outfile)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = Encoding.UTF8;

            XmlWriter writer = XmlWriter.Create(outfile, settings);

            writer.WriteStartElement("Document");

            writer.WriteStartElement("Aree");
            for (int index = 0; index < output.LimitiAree.Count; index++)
            {
                int limite = output.LimitiAree[index];
                writer.WriteStartElement("Area");
                writer.WriteAttributeString("Id", index.ToString());
                writer.WriteAttributeString("Limite", limite.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Missioni");
            foreach (MissioneAnnotata missione in output.MissioniAnnotate)
            {
                writer.WriteStartElement("Missione");
                writer.WriteAttributeString("Trn", missione.Trn);
                for (int index = 0; index < missione.ListaCdb.Count; index++)
                {
                    int cdb = missione.ListaCdb[index];
                    int[] azioni = missione.AzioniCdb[index];
                    writer.WriteStartElement("Cdb");
                    writer.WriteAttributeString("id", cdb.ToString());

                    for (int idAzione = 0; idAzione < azioni.Length; idAzione++)
                    {
                        int azione = azioni[idAzione];
                        writer.WriteStartElement("Azione");
                        writer.WriteAttributeString("IdArea", idAzione.ToString());
                        writer.WriteValue(azione);
                        //writer.WriteAttributeString("Azione", azione.ToString());
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();

                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteEndElement();
            writer.Flush();
        }
    }
}
