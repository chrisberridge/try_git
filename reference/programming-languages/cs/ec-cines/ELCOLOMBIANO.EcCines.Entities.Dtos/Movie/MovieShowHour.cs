/*==========================================================================*/
/* Source File:   MOVIESHOWHOUR.CS                                          */
/* Description:   A movie detailed information for JSON serialization.      */
/*                Specifies the show for movie (time and date).             */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: Mar.11/2015                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
============================================================================*/

namespace ELCOLOMBIANO.EcCines.Entities.Dtos.Movie {
    /// <summary>
    /// A movie detailed information for JSON serialization.
    /// Specifies the show for movie (time and date).
    /// </summary>
    public class MovieShowHour {
        public int id { get; set; }        
        public string timeFull { get; set; }
        public int timeHour { get; set; }
        public int timeMinute { get; set; }
    }
}
