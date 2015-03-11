/*==========================================================================*/
/* Source File:   MOVIESHOW.CS                                              */
/* Description:   A movie detailed information for JSON serialization.      */
/*                Specifies the show for movie (time and date).             */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: Mar.11/2015                                               */
/* Version:       1.2                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
============================================================================*/
using System;
using System.Collections.Generic;

namespace ELCOLOMBIANO.EcCines.Entities.Dtos.Movie {
    /// <summary>
    ///  A movie detailed information for JSON serialization.
    ///  Specifies the show for movie (time and date).       
    ///  </summary>
    public class MovieShow {
        public int id { get; set; }
        public DateTime dt { get; set; }
        public List<MovieShowHour> hours { get; set; }
    }
}