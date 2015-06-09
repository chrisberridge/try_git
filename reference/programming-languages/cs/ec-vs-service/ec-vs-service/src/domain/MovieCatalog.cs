/*==========================================================================*/
/* Source File:   MOVIECATALOG.CS                                           */
/* Description:   The purpose for this class is to have properties to be    */
/*                used in the Movie Search functionality, by providing      */
/*                all the required information for the searching parameters */
/*                necessary for the task.                                   */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Jun.14/2013                                               */
/* Last Modified: Jun.18/2013                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Jun.14/2013 COQ File created.
============================================================================*/
using System;
using System.Collections.Generic;

namespace ElColombiano.Service.Domain
{
    /// <summary>
    /// The purpose for this class is to have properties to be   
    /// used in the Movie Search functionality, by providing     
    /// all the required information for the searching parameters
    /// necessary for the task.
    /// </summary>
    public class MovieCatalog
    {
        public List<String> theaters { get; set; }
        public List<String> genres { get; set; }
        public List<String> movies { get; set; }
        public Dictionary<string, List<string>> theaterMovies { get; set; }
    }
}