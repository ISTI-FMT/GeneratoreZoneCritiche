using GestioneAreeCritiche.AreeCritiche;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestioneAreeCritiche.ModelChecking
{
    public class StatoAree
    {
        public List<IAreaCritica> Aree { get; private set; }

        public StatoAree()
        {
            Aree = new List<IAreaCritica>();
        }

        public bool EntrataPermessa(StatoMissione missione, int idx, int cdb)
        {
            bool entrataValida = true;

            int[] azioni = missione.AzioniCdb[idx];

            for (int i = 0; i < Aree.Count; i++)
            {
                int azione = azioni[i];

                if (azione != 0)
                {
                    IAreaCritica area = Aree[i];
                    if (!area.entrataPermessa(missione.Trn, cdb, azione))
                    {
                        entrataValida = false;
                        break;
                    }
                }
            }

            return entrataValida;
        }

        public bool Entrata(StatoMissione missione, int idx, int cdb)
        {
            bool entrataValida = true;
            int[] azioni = missione.AzioniCdb[idx];

            for (int i = 0; i < Aree.Count; i++)
            {
                int azione = azioni[i];
                if (azione != 0)
                {
                    IAreaCritica area = Aree[i];
                    area.entrata(missione.Trn, cdb, azione);
                }
            }
            return entrataValida;
        }

        public StatoAree Clone()
        {
            StatoAree areeClone = new StatoAree();
            foreach (IAreaCritica area in Aree)
            {
                IAreaCritica areaClone = (IAreaCritica)area.Clone();
                areeClone.Aree.Add(areaClone);
            }
            return areeClone;
        }


    }
}
