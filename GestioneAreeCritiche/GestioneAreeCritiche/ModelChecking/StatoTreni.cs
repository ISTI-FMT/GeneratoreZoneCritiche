using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestioneAreeCritiche.ModelChecking
{
    class StatoTreni
    {
        public StatoTreni()
        {
            Missioni = new List<StatoMissione>();
        }
        public List<StatoMissione> Missioni { get; set; }

        public StatoTreni Clone()
        {
            StatoTreni clone = new StatoTreni();
            foreach (StatoMissione missione in Missioni)
            {
                clone.Missioni.Add(missione.Clone());
            }

            return clone;
        }

        public override bool Equals(object obj)
        {
            StatoTreni stato2 = (StatoTreni)obj;
            return Missioni.SequenceEqual(stato2.Missioni);
        }

        public override string ToString()
        {
            int[] steps = Missioni.Select(missione => missione.Cdbs[missione.CurrentStep]).ToArray();
            return string.Join(",", steps);
        }

        public override int GetHashCode()
        {
            return GetHashCode(Missioni);
        }

        static int GetHashCode(List<StatoMissione> values)
        {
            List<int> steps = values.Select(value => value.CurrentStep).ToList();
            string str = string.Join(",", steps);
            return str.GetHashCode();
        }
    }
}
