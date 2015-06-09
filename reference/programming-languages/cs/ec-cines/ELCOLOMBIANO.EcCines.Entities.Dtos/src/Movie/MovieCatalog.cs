/*==========================================================================*/
/* Source File:   MOVIECATALOG.CS                                           */
/* Description:   Domain object to help serialize data in JSON format       */
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
    /// Domain object to help serializa data in JSON format.
    /// Holds the catalog as a helper.
    /// </summary>
    public class MovieCatalog : AbstractCommonDomain {
        public List<MovieLocationShort> theaters { get; set; }
        public List<MovieFormatShort> formats { get; set; }
        public List<MovieGenreShort> genres { get; set; }
        public List<MovieShortFormat> movies { get; set; }
        public Dictionary<string, List<MovieShortFormat>> theaterMovies { get; set; }
        public Dictionary<int,   List<MovieLocationShort>> movieInTheaters {get; set;}
    }
}