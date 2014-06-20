using System.Collections.Generic;
using GestioneAreeCritiche.AreeCritiche;
using System.Linq;

namespace GestioneAreeCritiche.Output
{
    public class DatiAree
    {
        private const int NessunaAzione = 0;
        private const int EntraSinistra = 3;
        private const int EsciSinistra = -3;
        private const int EntraDestra = 2;
        private const int EsciDestra = -2;
        private const int EntraStessaDirezione = 4;
        private const int EsciStessaDirezione = -4;

        private const int EntrataCircolare = 1;
        private const int UscitaCircolare = -1;

        public DatiAree(List<AreaCriticaCircolare> areeCircolari, List<AreaCriticaLineare> areeLineari, List<MissioneTreno> missioni)
        {
            AreeCircolari = areeCircolari;
            AreeLineari = areeLineari;
            Missioni = missioni;
            MissioniAnnotate = new List<MissioneAnnotata>();
            AreeCritiche = new List<IAreaCritica>();
            AreeCritiche.AddRange(areeLineari);
            AreeCritiche.AddRange(areeCircolari);
            

            CalcolaMissioniAnnotate();
        }

        public DatiAree(List<AreaCriticaCircolare> areeCircolari, List<AreaCriticaLineare> areeLineari, List<MissioneAnnotata> missioniAnnotate)
        {
            Missioni = new List<MissioneTreno>();
            AreeCritiche = new List<IAreaCritica>();
            AreeCritiche.AddRange(areeLineari);
            AreeCritiche.AddRange(areeCircolari);
            AreeCircolari = areeCircolari;
            AreeLineari = areeLineari;
            MissioniAnnotate = missioniAnnotate;
            
        }

        public List<IAreaCritica> AreeCritiche { get; private set; }
        private List<AreaCriticaCircolare> AreeCircolari { get; set; }
        private List<AreaCriticaLineare> AreeLineari { get; set; }
        private List<MissioneTreno> Missioni { get; set; }
        public List<MissioneAnnotata> MissioniAnnotate { get; private set; }


        public static IEnumerable<int> StartingIndex(List<int> x, List<int> y)
        {
            IEnumerable<int> index = Enumerable.Range(0, x.Count - y.Count + 1);
            for (int i = 0; i < y.Count; i++)
            {
                index = index.Where(n => x[n + i] == y[i]).ToArray();
            }
            return index;
        }

        public void CalcolaMissioniAnnotate()
        {
            //--- Generazione della lista di vettori annotati
            foreach (MissioneTreno missioneTreno in Missioni)
            {
                MissioneAnnotata missioneAnnotata = new MissioneAnnotata();
                //Inizializzazione del nome treno
                missioneAnnotata.Trn = missioneTreno.NomeTreno;
                //Inizializzazione della lista di cdb
                missioneAnnotata.ListaCdb.AddRange(missioneTreno.CdbList);
                //Inizializzazione delle azioni (zero di default)
                for (int i = 0; i < missioneTreno.CdbList.Count; i++)
                {
                    missioneAnnotata.AzioniCdb.Add(new int[AreeCritiche.Count]);
                }
                MissioniAnnotate.Add(missioneAnnotata);

                //annotazione aree lineari
                for (int i = 0; i < AreeLineari.Count; i++)
                {
                    AreaCriticaLineare areaCriticaLineare = AreeLineari[i];

                    //treni che entrano da sinistra
                    if (areaCriticaLineare.TreniSinistra.Contains(missioneTreno.NomeTreno))
                    {
                        //cerco tutti gli indici di inizio dell'area critica all'interno della missione
                        foreach (int indiceCdb in StartingIndex(missioneTreno.CdbList, areaCriticaLineare.ListaCdb))
                        {
                            missioneAnnotata.AzioniCdb[indiceCdb][i] = EntraSinistra;

                            if (indiceCdb + areaCriticaLineare.ListaCdb.Count < missioneAnnotata.AzioniCdb.Count)
                            {
                                missioneAnnotata.AzioniCdb[indiceCdb + areaCriticaLineare.ListaCdb.Count][i] = EsciSinistra;
                            }
                        }
                    }

                    //treni che entrano da destra
                    if (areaCriticaLineare.TreniDestra.Contains(missioneTreno.NomeTreno))
                    {
                        //visto che i treni entrano da destra, devo cercare i cdb in ordine inverso
                        List<int> cdbInvertiti = areaCriticaLineare.ListaCdb.ToList();
                        cdbInvertiti.Reverse();

                        //cerco tutti gli indici di inizio dell'area critica all'interno della missione
                        foreach (int indiceCdb in StartingIndex(missioneTreno.CdbList, cdbInvertiti))
                        {
                            missioneAnnotata.AzioniCdb[indiceCdb][i] = EntraDestra;

                            if (indiceCdb + areaCriticaLineare.ListaCdb.Count < missioneAnnotata.AzioniCdb.Count)
                            {
                                missioneAnnotata.AzioniCdb[indiceCdb + areaCriticaLineare.ListaCdb.Count][i] = EsciDestra;
                            }
                        }
                    }
                }

                //Annotazione aree circolari
                Dictionary<AreaCriticaCircolare, int> areeCorrenti = new Dictionary<AreaCriticaCircolare, int>();
                for (int cdbIndex = 0; cdbIndex < missioneTreno.CdbList.Count; cdbIndex++)
                {
                    int cdb = missioneTreno.CdbList[cdbIndex];

                    //Se l'ingresso in questo cdb comporta l'uscita da un area precedente, inizializzo
                    //l'azione corrispondente
                    for (int i = 0; i < AreeCircolari.Count; i++)
                    {
                        AreaCriticaCircolare areaCriticaCircolare = AreeCircolari[i];

                        //il cdb e il treno sono dell'area critica corrente
                        if (areaCriticaCircolare.Treni.Contains(missioneTreno.NomeTreno) &&
                            areaCriticaCircolare.ListaCdb.Contains(cdb))
                        {
                            bool entrato = false;
                            if (!areeCorrenti.ContainsKey(areaCriticaCircolare))
                            {
                                // Sono entrato dentro una area circolare nuova
                                areeCorrenti.Add(areaCriticaCircolare, i);
                                missioneAnnotata.AzioniCdb[cdbIndex][i + AreeLineari.Count] = EntrataCircolare;
                                entrato = true;
                            }

                            if (cdbIndex < missioneTreno.CdbList.Count - 1)
                            {
                                //Se non sono l'ultimo cdb della missione, controllo se sto uscendo dall'area (ovvero il prossimo cdb non fa parte dell'area corrente)
                                int nextcdb = missioneTreno.CdbList[cdbIndex + 1];
                                if (!areaCriticaCircolare.ListaCdb.Contains(nextcdb))
                                {
                                    //Sto uscendo dall'area critica...
                                    //Se ero appena entrato nell'area => annullo l'entrata
                                    //altrimenti => registro l'uscita
                                    //Nota: nelle missioni circolari si esce con un passo in anticipo
                                    if (entrato)
                                    {
                                        missioneAnnotata.AzioniCdb[cdbIndex][areeCorrenti[areaCriticaCircolare] + AreeLineari.Count] = 0;
                                    }
                                    else
                                    {
                                        missioneAnnotata.AzioniCdb[cdbIndex][areeCorrenti[areaCriticaCircolare] + AreeLineari.Count] = UscitaCircolare;
                                    }
                                    areeCorrenti.Remove(areaCriticaCircolare);
                                }
                            }
                        }
                    }
                }
            }
        }
            
    }
}