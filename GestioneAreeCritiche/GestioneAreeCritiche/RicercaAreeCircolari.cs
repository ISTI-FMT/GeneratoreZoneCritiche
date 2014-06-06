using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GestioneAreeCritiche.AreeCritiche;

namespace GestioneAreeCritiche
{
    internal class RicercaAreeCircolari
    {
        internal static List<AreaCriticaCircolare> Ricerca(List<MissioneTreno> missioni)
        {
            List<AreaCriticaCircolare> aree = new List<AreaCriticaCircolare>();
            foreach (MissioneTreno missioneTreno in missioni)
            {
                for (int i = 0; i < missioneTreno.CdbList.Count -1 ; i++)
                {
                    int cdb = missioneTreno.CdbList[i];
                    int cdb2 = missioneTreno.CdbList[i + 1];

                    List<CdbVisitato> visitati = new List<CdbVisitato>();
                    List<MissioneTreno> missioniNuovo = missioni.ToList();
                    missioniNuovo.Remove(missioneTreno);

                    int loopDepth = RicercaLoop(missioniNuovo, missioneTreno, new[] { cdb, cdb2 }, visitati);
                    if (loopDepth > 2 && visitati.Count > 4 && visitati[0].Cdb == visitati[visitati.Count - 1].Cdb)
                    {
                        AreaCriticaCircolare circolare = new AreaCriticaCircolare();

                        //Visitati contiene due volte ogni cdb visitato 
                        List<int> visitatiInt = visitati.Select(visitato => visitato.Cdb).ToList();

                        List<int> cdbs = new List<int>();
                        for (int v = 0; v < visitatiInt.Count; v = v + 2)
                        {
                            cdbs.Add(visitatiInt[v]);
                        }

                        circolare.ListaCdb = cdbs;
                        circolare.Limite = cdbs.Count - 1;

                        AreaCriticaCircolare circolareEsistente = aree.Find(area => area.Equals(circolare));
                        if (circolareEsistente != null)
                        {
                            circolare = circolareEsistente;
                        }
                        else
                        {
                            aree.Add(circolare);
                        }

                        foreach (CdbVisitato cdbVisitato in visitati)
                        {
                            if (!circolare.Treni.Contains(cdbVisitato.NomeTreno))
                            {
                                circolare.Treni.Add(cdbVisitato.NomeTreno);
                            }
                        }
                    }
                    
                }
            }
            return aree;
        }

        private static int RicercaLoop(List<MissioneTreno> missioni, MissioneTreno corrente, int[] valori, List<CdbVisitato> visitati)
        {
            List<CdbVisitato> visitatiCurr = new List<CdbVisitato>();
            visitatiCurr.Add(new CdbVisitato { Cdb = valori[0], NomeTreno = corrente.NomeTreno });
            visitatiCurr.Add(new CdbVisitato { Cdb = valori[1], NomeTreno = corrente.NomeTreno });

            //Se il secondo cdb è già nella lista dei visitati, ho trovato un loop
            if (visitati.Any(cdbv => cdbv.Cdb == valori[1]))
            {
                //Se il loop è maggiore di 4 (ovvero 2 nodi), ritorno 1 (loop trovato)
                //Altrimenti ritorno 0 (loop non trovato) perchè ho trovato un loop di soli 2 nodi ovvero un'area lineare
                if (visitati.Count > 4)
                {
                    visitati.AddRange(visitatiCurr);
                    return 1;
                }
                else
                {
                    return 0;
                }
            }

            visitati.AddRange(visitatiCurr);

            foreach (MissioneTreno missioneTreno in missioni)
            {
                if (missioneTreno == corrente)
                    continue;

                //Cerco l'elemento valori[1] nella lista di cdb di ogni treno
                int bIdx = missioneTreno.CdbList.IndexOf(valori[1]);
                while (bIdx >= 0 && bIdx < missioneTreno.CdbList.Count - 1)
                {
                    List<MissioneTreno> missioniNuovo = missioni.ToList();
                    missioniNuovo.Remove(missioneTreno);
                    int ciclo = RicercaLoop(missioniNuovo, missioneTreno, new[] { missioneTreno.CdbList[bIdx], missioneTreno.CdbList[bIdx + 1] }, visitati);

                    if (ciclo > 0)
                    {
                        return ciclo + 1;
                    }

                    //Se il vettore B ha un secondo elemento che corrisponde a quello corrente di A,
                    //sposto l'indice su quell'elemento
                    bIdx = missioneTreno.CdbList.IndexOf(valori[1], bIdx + 1);
                }
            }

            foreach (CdbVisitato cdbVisitato in visitatiCurr)
            {
                visitati.Remove(cdbVisitato);
            }
            return 0;
        }
    }
}
