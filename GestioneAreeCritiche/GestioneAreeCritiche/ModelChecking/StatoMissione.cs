using GestioneAreeCritiche.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestioneAreeCritiche.ModelChecking
{
    public class StatoMissione
    {
        private MissioneAnnotata missione;

        public StatoMissione(MissioneAnnotata missione)
        {
            this.missione = missione;
        }

        public string Trn { get { return missione.Trn; } }
        public List<int> Cdbs { get { return missione.ListaCdb; } }
        public List<int[]> AzioniCdb { get { return missione.AzioniCdb; } }
        public int CurrentStep { get; private set; }

        public void MoveNext()
        {
            CurrentStep++;
        }

        public bool Terminata
        {
            get
            {
                return CurrentStep == Cdbs.Count - 1;
            }
        }

        public StatoMissione Clone()
        {
            StatoMissione clone = new StatoMissione(missione);
            clone.CurrentStep = CurrentStep;

            return clone;
        }

        public override bool Equals(object obj)
        {
            StatoMissione missione2 = (StatoMissione)obj;
            return
                CurrentStep == missione2.CurrentStep &&
                missione2.Trn == Trn &&
                missione2.AzioniCdb.SequenceEqual(AzioniCdb) &&
                missione2.Cdbs.SequenceEqual(Cdbs);
        }
    }
}
