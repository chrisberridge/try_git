/*==========================================================================*/
/* Source File:   PROGRAMACIONPELICULADTO.CS                                */
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
    public class ProgramacionPeliculaDto : AbstractCommonDomain {
        public int idFormato { get; set; }
        public int idPelicula { get; set; }
        public int idHorarioPelicula { get; set; }
        public int annoHorarioPelicula { get; set; }
        public int mesHorarioPelicula { get; set; }
        public int diaHorarioPelicula { get; set; }
        public string nombreDiaSemanaHorarioPelicula { get; set; }
        public int idTeatro { get; set; }
        public string horaMinutoPelicula { get; set; }
        public int sala { get; set; }
        public int frecuencia { get; set; }       
    }
}
