using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestioneAreeCritiche.ModelChecking
{
    public class Deadlock
    {
        public List<KeyValuePair<string, int>> Positions { get; private set; }
        public Deadlock()
        {
            Positions = new List<KeyValuePair<string, int>>();
        }

        public int Getposition(string trn)
        {
            KeyValuePair<string, int>  found = Positions.FirstOrDefault(pair => pair.Key == trn);
            if (found.Key == null)
            {
                return -1;
            }
            else
            {
                return found.Value;
            }
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

        public override bool Equals(object obj)
        {
            Deadlock dl = obj as Deadlock;
            if (dl == null)
                return false;

            return this.ToString().Equals(dl.ToString());
        }

        /// <summary>
        /// Ritorna TRUE se il deadlock ricevuto è un sotto-deadlock dell'oggetto corrente
        /// </summary>
        public bool IsSubDeadlock(Deadlock deadlock2)
        {
            List<KeyValuePair<string, int>> lista2 = deadlock2.Positions.OrderBy(pair => pair.Key).ToList();
            List<KeyValuePair<string, int>> lista = Positions.OrderBy(pair => pair.Key).ToList();

            if (lista2.Intersect(lista).SequenceEqual(lista2))
            {
                return true;
            }

            return false;
        }
    }
}
