/*==========================================================================*/
/* Source File:   PARAMETROSISTEMADTO.CS                                    */
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
    public class ParametroSistemaDto : AbstractCommonDomain {
        public int idParametro { get; set; }
        public string nombreParametro { get; set; }
        public string valorParametro { get; set; }
        public string descValorParametro { get; set; }
        public char visible { get; set; }
    }
}
