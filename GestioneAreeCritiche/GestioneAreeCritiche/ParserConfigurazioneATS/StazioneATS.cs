using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestioneAreeCritiche.ParserConfigurazioneATS
{
    internal class Itinerario
    {
        internal int Id { get; set; }

        internal int PrevCdb { get; set; }
        internal int NextCdb { get; set; }
        internal List<int> Cdbs = new List<int>();
    }

    internal class Stazione
    {
        internal string Nome { get; set; }
        internal int Id { get; set; }

        internal List<Itinerario> Entrata = new List<Itinerario>();
        internal List<Itinerario> Uscita = new List<Itinerario>();
    }
}
