//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace GestioneAreeCritiche.AreeCritiche
//{
//    class GeneratoreAreeComposte
//    {
//        internal void GeneraAreeComposte(List<IAreaCritica> areeCritiche)
//        {
//                  foreach (IAreaCritica area in areeCritiche)
//                    {
//                        foreach (IAreaCritica area2 in areeCritiche)
//                        {
//                            if (area == area2)
//                                continue;

//                            List<int> intersection = area.ListaCdb.Intersect(area2.ListaCdb).ToList();

//                            if (!intersection.SequenceEqual(area.ListaCdb) &&
//                                !intersection.SequenceEqual(area2.ListaCdb))
//                            {
//                                HashSet<string> treni = new HashSet<string>();

//                                if (area is AreaCriticaCircolare)
//                                {
//                                    AreaCriticaCircolare c = area as AreaCriticaCircolare;
//                                    foreach (string trn in c.Treni)
//                                    {
//                                        treni.Add(trn);
//                                    }
//                                }
//                                else if (area is AreaCriticaLineare)
//                                {
//                                    AreaCriticaLineare l = area as AreaCriticaLineare;
//                                    foreach (string sx in l.TreniSinistra)
//                                    {
//                                        treni.Add(sx);
//                                    }
//                                    foreach (string sx in l.TreniDestra)
//                                    {
//                                        treni.Add(sx);
//                                    }
//                                }

//                                if (area2 is AreaCriticaCircolare)
//                                {
//                                    AreaCriticaCircolare c = area2 as AreaCriticaCircolare;
//                                    foreach (string trn in c.Treni)
//                                    {
//                                        treni.Add(trn);
//                                    }
//                                }
//                                else if (area2 is AreaCriticaLineare)
//                                {
//                                    AreaCriticaLineare l = area2 as AreaCriticaLineare;
//                                    foreach (string sx in l.TreniSinistra)
//                                    {
//                                        treni.Add(sx);
//                                    }
//                                    foreach (string sx in l.TreniDestra)
//                                    {
//                                        treni.Add(sx);
//                                    }
//                                }


//                                AreaCriticaCircolare circolare = null;
//                                if (area is AreaCriticaCircolare)
//                                {
//                                    circolare = area as AreaCriticaCircolare;
//                                }
//                                else if (area2 is AreaCriticaCircolare)
//                                {
//                                    circolare = area2 as AreaCriticaCircolare;
//                                }

//                                if (circolare != null)
//                                {
//                                    AreaCriticaCircolare nuova = new AreaCriticaCircolare();
//                                    List<int> cdbNuova = new List<int>();
//                                    cdbNuova.AddRange(area.ListaCdb);
//                                    cdbNuova.AddRange(area2.ListaCdb);
//                                    cdbNuova = cdbNuova.Distinct().ToList();

//                                    nuova.Limite = circolare.Limite;
//                                    nuova.ListaCdb = cdbNuova;

//                                    foreach (string trn in treni)
//                                    {
//                                        nuova.Treni.Add(trn);
//                                    }
                                    

//                                    bool presente = false;
//                                    foreach (AreaCriticaCircolare area3 in areeCircolari)
//                                    {
//                                        AreaCriticaCircolare area3Circolare = area3 as AreaCriticaCircolare;
//                                        List<int> cdbs1 = area3Circolare.ListaCdb.ToList();
//                                        cdbs1.Sort();

//                                        List<int> cdbs2 = nuova.ListaCdb.ToList();
//                                        cdbs2.Sort();

//                                        if (cdbs1.SequenceEqual(cdbs2))
//                                        {
//                                            presente = true;
//                                        }
//                                    }

//                                    if (!presente)
//                                    {
//                                        areeCircolari.Add(nuova);
//                                        Console.WriteLine("Nuova area: " + nuova.GetListaCdbStr() + " limite: " + nuova.Limite);
//                                    }
//                                }
//                            }
//        }
//                  }
//    }
//}
