/*==========================================================================*/
/* Source File:   MOVIESHORTCOMPACT.CS                                      */
/* Description:   A movie detailed information for JSON serialization.      */
/*                This holds a subset information for a MOVIESHORT class.   */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: Mar.11/2015                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Mar.05/2015 COQ File created.
============================================================================*/

namespace ELCOLOMBIANO.EcCines.Entities.Dtos.Movie {
    /// <summary>
    /// A movie detailed information for JSON serialization. 
    /// This holds a subset information for a MOVIESHORT class.
    /// </summary>
    public class MovieShortCompact {
        public int id { get; set; }
        public string name { get; set; }
    }
}
