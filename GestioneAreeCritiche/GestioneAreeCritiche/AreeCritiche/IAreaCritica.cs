﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestioneAreeCritiche.AreeCritiche
{
    /// <summary>
    /// Interfaccia generica che rappresenta una area critica di qualsiasi tipo
    /// </summary>
    public interface IAreaCritica
    {
        List<int> ListaCdb { get; set; }

        int Limite { get; }

        /// <summary>
        /// Genera la lista di cdb separati da virgola
        /// </summary>
        string GetListaCdbStr();
    }
}
