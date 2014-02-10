using System;
using System.IO;
using GestioneAreeCritiche.Output;

namespace XmlUmcConverter
{
    class Program
    {
        private static void UmcToXml(string sourceFile, string destFile)
        {
            Console.WriteLine("Parsing UMC...");
            StrutturaOutput strutturaOutput = UmcParser.ParseUmc(sourceFile);

            
            string outfile = Path.GetFileNameWithoutExtension(sourceFile) + ".umc_2";
            Console.WriteLine("Generazione XML...");
            GenerazioneOutput.ToUmc(strutturaOutput, outfile);
        }

        private static void XmlToUmc(string sourceFile, string destFile)
        {
            Console.WriteLine("Parsing XML...");
            StrutturaOutput strutturaOutput = XmlAreeParser.ParseXml(sourceFile);

            string outfile = Path.GetFileNameWithoutExtension(sourceFile) + ".xml_2";
            Console.WriteLine("Generazione UMC...");
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
            string nomefile;
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
            else
            {
                Console.WriteLine("Tipo di file non riconosciuto. Estensioni supportate .xml, .umc");
            }
        }
    }
}
