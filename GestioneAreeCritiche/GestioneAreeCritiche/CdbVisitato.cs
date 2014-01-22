using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestioneAreeCritiche
{
    internal class CdbVisitato
    {
        internal String NomeTreno { get; set; }

        internal int Cdb { get; set; }
        

        public override string ToString()
        {
            return NomeTreno + ":" + Cdb;
        }
    }
}
