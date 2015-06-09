/*==========================================================================*/
/* Source File:   TEATRODTO.CS                                              */
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
    public class TeatroDto : AbstractCommonDomain {
        public int idTeatro { get; set; }
        public int idCine { get; set; }
        public string nombreTeatro { get; set; }
        public string telefono1Teatro { get; set; }
        public string telefono2Teatro { get; set; }
        public string telefono3Teatro { get; set; }
        public int idMunicipioTeatro { get; set; }
        public int idDepeartamentoTeatro { get; set; }
        public string direccionTeatro { get; set; }
    }
}
