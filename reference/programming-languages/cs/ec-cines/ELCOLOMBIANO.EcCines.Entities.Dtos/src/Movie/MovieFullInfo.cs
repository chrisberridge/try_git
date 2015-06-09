/*==========================================================================*/
/* Source File:   MOVIEFULLINFO.CS                                          */
/* Description:   A movie detailed information for JSON serialization       */
/*                Holds the data for a complete movie with its theater,     */
/*                its format, its show dates.                               */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Mar.04/2015                                               */
/* Last Modified: Apr.24/2015                                               */
/* Version:       1.5                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Mar.04/2015 COQ File created.
============================================================================*/

using System;

namespace ELCOLOMBIANO.EcCines.Entities.Dtos.Movie {
    /// <summary>
    /// A movie detailed information for JSON serialization  
    /// Holds the data for a complete movie with its theater,
    /// its format, its show dates.                          
    /// </summary>
    public class MovieFullInfo : AbstractCommonDomain {
        // Movie detail part.
        public int id { get; set; }
        public string name { get; set; }
        public string nameFull { get; set; }
        public string img { get; set; }
        public string url { get; set; }
        public string active { get; set; }
        public string premiere { get; set; }
        public int idGenre { get; set; }
        public string genre { get; set; }
        public DateTime createDate { get; set; }

        // Movie location part.
        public int idLocation { get; set; }
        public string nameLocation { get; set; }
        public string branchName { get; set; }
        public string nameFullLocation { get; set; }
        public string address { get; set; }

        // Movie format part.
        public int idFormat { get; set; }
        public string nameFormat { get; set; }

        // Movie show part.
        public int idShow { get; set; }
        public DateTime dt { get; set; }

    }
}
