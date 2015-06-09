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
using System.Collections.Generic;

namespace ELCOLOMBIANO.EcCines.Entities.Dtos {
    /// <summary>
    /// Data Transfer Object
    /// </summary>
    public class DetalleProgramacion : AbstractCommonDomain {
        public int id { get; set; }
        public List<F> fs { get; set; }
    }
}
