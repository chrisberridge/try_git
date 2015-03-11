/*==========================================================================*/
/* Source File:   MOVIESHORTFORMAT.CS                                       */
/* Description:   A movie detailed information for JSON serialization.      */
/*                This holds a subset information for a MOVIE class.        */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: Mar.09/2015                                               */
/* Version:       1.2                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
============================================================================*/

using System.Collections.Generic;

namespace ELCOLOMBIANO.EcCines.Entities.Dtos.Movie {
    /// <summary>
    /// A movie detailed information for JSON serialization.
    /// This holds a subset information for a MOVIE class.
    /// </summary>
    public class MovieShortFormat {
        public int id { get; set; }
        public string name { get; set; }
        public List<MovieFormatShort> formats { get; set; }
    }
}
