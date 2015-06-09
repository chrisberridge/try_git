/*==========================================================================*/
/* Source File:   PELICULAFULLINFODTO.CS                                    */
/* Description:   Data transfer object                                      */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: Apr.24/2015                                               */
/* Version:       1.2                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
============================================================================*/

namespace ELCOLOMBIANO.EcCines.Entities.Dtos {
    /// <summary>
    /// Data Transfer Object
    /// </summary>
    public class PeliculaFullInfoDto : AbstractCommonDomain {
        public int idFormato { get; set; }
        public string nombreFormato { get; set; }
        public int idPelicula { get; set; }
        public int idHorarioPelicula { get; set; }
        public string annoHorarioPelicula { get; set; }
        public string mesHorarioPelicula { get; set; }
        public string diaHorarioPelicula { get; set; }
        public string nombreDiaSemanaHorarioPelicula { get; set; }
        public int idTeatro { get; set; }
        public string nombreTeatro { get; set; }
        public int frecuencia { get; set; }
        public string horaPelicula { get; set; }
        public string minutoPelicula { get; set; }
        public int sala { get; set; }
    }
}
