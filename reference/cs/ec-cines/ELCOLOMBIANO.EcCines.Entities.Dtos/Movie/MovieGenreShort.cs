/*==========================================================================*/
/* Source File:   MOVIEGENRESHORT.CS                                        */
/* Description:   Holds basic genre info for movie                          */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: Mar.05/2015                                               */
/* Version:       1.3                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
============================================================================*/

namespace ELCOLOMBIANO.EcCines.Entities.Dtos
{
    /// <summary>
    /// Holds basic genre info for movie
    /// </summary>
    public class MovieGenreShort
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}
