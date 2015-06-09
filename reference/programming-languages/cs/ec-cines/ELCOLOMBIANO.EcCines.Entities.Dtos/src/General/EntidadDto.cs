/*==========================================================================*/
/* Source File:   ENTIDADDTO.CS                                             */
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
    public class EntidadDto : AbstractCommonDomain {
        public int idEntidad { get; set; }
        public int codEntidad { get; set; }
        public string nombreEntidad { get; set; }
        public string valorEntidad { get; set; }
        public string descripcionEntidad { get; set; }
    }
}