/*==========================================================================*/
/* Source File:   KEYVALUE.CS                                               */
/* Description:   Helper class                                              */
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
    /// Data Transfer Object, helper class
    /// </summary>
    public class KeyValue : AbstractCommonDomain {
        public string key { get; set; }
        public string value { get; set; }
    }
}
