/*==========================================================================*/
/* Source File:   TEATROEXDTO.CS                                            */
/* Description:   Data transfer object extended version for TEATRO          */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.21/2015                                               */
/* Last Modified: Mar.11/2015                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.21/2015 COQ File created.
============================================================================*/

namespace ELCOLOMBIANO.EcCines.Entities.Dtos {
    public class TeatroExDto : TeatroDto {
        public string nombreCine { get; set; }
        public string nombreMunicipio { get; set; }
        public string nombreDepartamento { get; set; }
    }
}
