using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestioneAreeCritiche
{
    class Options
    {
        [Option('d', "deadlock", Required = false, HelpText = "Trova deadlock da un file umc", MutuallyExclusiveSet = "comandi")]
        public string Deadlock { get; set; }

        [Option('a', "aree", Required = false, HelpText = "Trova aree critiche e deadlock da un file txt", MutuallyExclusiveSet = "comandi")]
        public string Aree { get; set; }

        [Option('x', "xml", Required = false, HelpText = "Trova aree critiche e deadlock dai file TabellaOrario.xml e ConfigurazioneItinerari.xml", MutuallyExclusiveSet = "comandi")]
        public bool Xml { get; set; }

        [Option('c', "convert", Required = false, HelpText = "Converti tra file umc e file xml", MutuallyExclusiveSet = "comandi")]
        public string Convert { get; set; }

        [Option("ignoraAree", Required = false, DefaultValue = false, HelpText = "Trova tutti i deadlock, anche quelli che si potrebbero evitare con le aree critiche")]
        public bool IgnoraAree { get; set; }

        [Option("ignoraFalsiPositivi", Required = false, DefaultValue = false, HelpText = "Non cerca falsi positivi (più veloce)")]
        public bool IgnoraFalsiPositivi { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
