/*==========================================================================*/
/* Source File:   MOVIELOCATION.CS                                          */
/* Description:   A movie detailed information for JSON serialization.      */
/*                Specifies the location where movie is shown.              */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: Mar.04/2015                                               */
/* Version:       1.2                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
============================================================================*/
using System.Collections.Generic;

namespace ELCOLOMBIANO.EcCines.Entities.Dtos.Movie
{
    /// <summary>
    /// A movie detailed information for JSON serialization.
    /// Specifies the location where movie is shown.
    /// </summary>
    public class MovieLocation : MovieLocationShort
    {
        public string address { get; set; }
        public List<MovieFormat> formats { get; set; }
    }
}
