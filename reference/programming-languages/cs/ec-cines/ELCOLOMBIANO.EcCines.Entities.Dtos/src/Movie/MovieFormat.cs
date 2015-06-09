/*==========================================================================*/
/* Source File:   MOVIEFORMAT.CS                                            */
/* Description:   A movie detailed information for JSON serialization.      */
/*                Specifies the format of the movie.                        */
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

namespace ELCOLOMBIANO.EcCines.Entities.Dtos.Movie {
    /// <summary>
    /// A movie detailed information for JSON serialization. 
    //  Specifies the format of the movie.
    /// </summary>
    public class MovieFormat : MovieFormatShort {
        public List<MovieShow> shows { get; set; }
    }
}
