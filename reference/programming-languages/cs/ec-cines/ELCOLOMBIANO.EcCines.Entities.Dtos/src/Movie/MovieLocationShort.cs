/*==========================================================================*/
/* Source File:   MOVIELOCATIONSHORT.CS                                     */
/* Description:   A movie detailed information for JSON serialization.      */
/*                Holds the theater part for a movie, where needed.         */
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

namespace ELCOLOMBIANO.EcCines.Entities.Dtos.Movie {
    /// <summary>
    /// A movie detailed information for JSON serialization. 
    /// Holds the theater part for a movie, where needed. 
    /// </summary>
    public class MovieLocationShort : AbstractCommonDomain {
        public int id { get; set; }
        public string name { get; set; }        
        public string branchName { get; set; }
        public string nameFull { get; set; }
    }
}
