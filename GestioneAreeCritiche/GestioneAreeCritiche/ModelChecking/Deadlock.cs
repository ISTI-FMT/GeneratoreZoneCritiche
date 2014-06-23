using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestioneAreeCritiche.ModelChecking
{
    internal class Deadlock
    {
        public List<KeyValuePair<string, int>> Positions { get; private set; }
        public Deadlock()
        {
            Positions = new List<KeyValuePair<string, int>>();
        }

        public void AggiungiPosizione(string trn, int cdb)
        {
            Positions.Add(new KeyValuePair<string, int>(trn, cdb));
        }

        internal bool VerificaDeadlock(StatoTreni statoTreni)
        {
            if (Positions.Count == 0)
                return false;

            bool allInPlace = true;
            foreach (KeyValuePair<string, int> pair in Positions)
            {
                foreach (StatoMissione missione in statoTreni.Missioni)
                {
                    int cdbMissione = missione.Cdbs[missione.CurrentStep];

                    if (pair.Key == missione.Trn)
                    {
                        int cdbPosizione = pair.Value;

                        if (cdbMissione != cdbPosizione)
                        {
                            allInPlace = false;
                            break;
                        }
                    }
                }
            }
            return allInPlace;
        }        

        public override string ToString()
        {
            return string.Join(", ", Positions.Select(position => position.Key + ":" + position.Value));
        }
    }
}
