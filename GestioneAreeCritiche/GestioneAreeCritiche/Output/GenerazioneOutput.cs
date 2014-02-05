using System;
using System.IO;
using System.Text;
using System.Xml;

namespace GestioneAreeCritiche.Output
{
    public static class GenerazioneOutput
    {
        public static void ToConsoleOutput(StrutturaOutput output)
        {
            Console.WriteLine("Output:");
            GeneraUmc(output, Console.Out);
        }


        public static void ToUmc(StrutturaOutput output, string outfile)
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

            GeneraUmc(output, sw);

            sw.Flush();
            sw.Close();
            sw.Dispose();
            fs.Close();
            fs.Dispose();
        }

        /// <summary>
        /// Scrive la struttura di output in uno stream generico (file o console) in formato compatibile con UMC
        /// </summary>
        private static void GeneraUmc(StrutturaOutput output, TextWriter sw)
        {
            sw.Write('[');
            sw.Write(string.Join(",", output.LimitiAree));
            sw.WriteLine(']');

            foreach (MissioneAnnotata missione in output.MissioniAnnotate)
            {
                sw.Write(missione.Trn + ":");
                for (int index = 0; index < missione.ListaCdb.Count; index++)
                {
                    int cdb = missione.ListaCdb[index];
                    int[] azioni = missione.AzioniCdb[index];

                    sw.Write(" [{0}] {1}", string.Join(",", azioni), cdb);

                    if (index < missione.ListaCdb.Count - 1)
                    {
                        sw.Write(',');
                    }
                }
                sw.WriteLine();
            }
        }

        /// <summary>
        /// Scrive la struttura di oputput in un file XML
        /// </summary>
        public static void ToXml(StrutturaOutput output, string outfile)
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
                    writer.WriteAttributeString("Id", cdb.ToString());

                    for (int idAzione = 0; idAzione < azioni.Length; idAzione++)
                    {
                        int azione = azioni[idAzione];

                        //Se non c'è da fare nessuna azione sull'area, non la includo nell'output
                        if (azione == 0)
                            continue;

                        writer.WriteStartElement("Azione");
                        writer.WriteAttributeString("IdArea", idAzione.ToString());
                        writer.WriteValue(azione);
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
