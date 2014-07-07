using System;
using System.IO;
using GestioneAreeCritiche.Output;

namespace GestioneAreeCritiche.Conversione
{
    internal class Convertitore
    {
        private static void UmcToXml(string sourceFile, string destFile)
        {
            Console.WriteLine("Parsing UMC...");
            DatiAree strutturaOutput = UmcParser.ParseUmc(sourceFile);
            
            string outfile = Path.GetFileNameWithoutExtension(sourceFile) + ".xml";
            Console.WriteLine("Generating XML...");
            GenerazioneOutput.ToXml(strutturaOutput, outfile);
        }

        private static void XmlToUmc(string sourceFile, string destFile)
        {
            Console.WriteLine("Parsing XML...");
            DatiAree strutturaOutput = XmlAreeParser.ParseXml(sourceFile);

            string outfile = Path.GetFileNameWithoutExtension(sourceFile) + ".umc";
            Console.WriteLine("Generating UMC...");
            GenerazioneOutput.ToUmc(strutturaOutput, outfile);
        }

        internal static void Converti(string nomefile)
        {
            Console.WriteLine();

            if (!File.Exists(nomefile))
            {
                Console.WriteLine("File " + nomefile + " does not exist");
                Program.PrintUsage();
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
            else
            {
                Console.WriteLine("Unknown file type. Supported extensions are: .xml, .umc");
            }
        }
    }
}
