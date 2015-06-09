/*==========================================================================*/
/* Source File:   HORADTO.CS                                                */
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
    public class HorarioPeliculaDto : AbstractCommonDomain {
        public int idHorarioPelicula { get; set; }
        public int idFormato { get; set; }
        public int idPelicula { get; set; }
        public int idTeatro { get; set; }
        public int annoHorarioPelicula { get; set; }
        public int mesHorarioPelicula { get; set; }
        public int diaHorarioPelicula { get; set; }
        public string nombreDiaSemanaPelicula { get; set; }
        public int frecuencia { get; set; }
    }
}