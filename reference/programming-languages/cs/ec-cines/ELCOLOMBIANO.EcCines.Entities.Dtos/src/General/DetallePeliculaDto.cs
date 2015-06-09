/*==========================================================================*/
/* Source File:   DETALLEPELICULADTO.CS                                     */
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
using System;

namespace ELCOLOMBIANO.EcCines.Entities.Dtos {
    /// <summary>
    /// Data Transfer Object
    /// </summary>
    public class DetallePeliculaDto : AbstractCommonDomain {
        public int idDetallePelicula { get; set; }
        public int idPelicula { get; set; }
        public string nombrePelicula { get; set; }
        public int idUsuarioCreador { get; set; }
        public DateTime fechaCreacionPelicula { get; set; }
        public int idGeneroPelicula { get; set; }
        public string sinopsis { get; set; }
        public string imagenCartelera { get; set; }
        public string urlArticuloEc { get; set; }
        public string enCartelera { get; set; }
        public string premiere { get; set; }

        public DetallePeliculaDto() {
            idDetallePelicula = 0;
            idPelicula = 0;
            nombrePelicula = "";
            idUsuarioCreador = 0;
            fechaCreacionPelicula = DateTime.Now;
            idGeneroPelicula = 0;
            sinopsis = "";
            imagenCartelera = "";
            enCartelera = "N";
            premiere = "N";
        }
    }
}