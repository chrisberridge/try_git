/*==========================================================================*/
/* Source File:   CINEDTO.CS                                                */
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
    public class CineDto : AbstractCommonDomain {
        public int idCine { get; set; }
        public String nit { get; set; }
        public DateTime fechaCreacionCine { get; set; }
        public String nombreCine { get; set; }
    }
}