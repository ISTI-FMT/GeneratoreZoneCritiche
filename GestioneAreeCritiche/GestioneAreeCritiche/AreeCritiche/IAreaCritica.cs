using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestioneAreeCritiche.AreeCritiche
{
    public enum TipoArea { Lineare, Circolare }

    /// <summary>
    /// Interfaccia generica che rappresenta una area critica di qualsiasi tipo
    /// </summary>
    public interface IAreaCritica : ICloneable
    {
        List<int> ListaCdb { get; set; }

        int Limite { get; }

        TipoArea TipoArea { get; }

        /// <summary>
        /// Genera la lista di cdb separati da virgola
        /// </summary>
        string GetListaCdbStr();

        bool entrataPermessa(string idTreno, int cdb, int tipoEntrata);
        void entrata(string idTreno, int cdb, int tipoEntrata);

        //Riporta l'area critica nel suo stato iniziale
        void Reset();
    }
}
