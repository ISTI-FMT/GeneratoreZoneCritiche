using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestioneAreeCritiche.ModelChecking
{
    public class Deadlock
    {
        public Deadlock()
        {
            Positions = new List<int>();
        }
        public List<int> Positions { get; set; }
    }
}
