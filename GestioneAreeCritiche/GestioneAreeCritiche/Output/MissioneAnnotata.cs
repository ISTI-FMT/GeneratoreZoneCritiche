using System;
using System.Collections.Generic;

namespace GestioneAreeCritiche.Output
{
    public class MissioneAnnotata
    {
        public MissioneAnnotata()
        {
            ListaCdb = new List<int>();
            AzioniCdb = new List<int[]>();
        }


        public List<int> ListaCdb { get; private set; }

        public List<int[]> AzioniCdb { get; private set; }

        public String Trn { get; set; }
    }
}