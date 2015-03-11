
/*==========================================================================*/
/* Source File:   MOVIEFORMATSHORT.CS                                       */
/* Description:   Transfer data for movie format                            */
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
    /// Transfer data for movie format.
    /// </summary>
    public class MovieFormatShort {
        public int id { get; set; }
        public string name { get; set; }
    }
}
