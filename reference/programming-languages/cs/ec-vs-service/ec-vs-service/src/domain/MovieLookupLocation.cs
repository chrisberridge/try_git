/*==========================================================================*/
/* Source File:   MOVIELOOKUPLOCATION.CS                                    */
/* Description:   This is a remapping in the structure for Planepoly JSON   */
/*                namely for El Colombiano own requirements.                */
/*                Contains the Location Structure.                          */
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
using System.Collections.Generic;

namespace ElColombiano.Service.Domain
{
    /// <summary>
    /// This is a remapping in the structure for Planepoly JSON
    /// namely for El Colombiano own requirements.
    /// Contains the Location Structure.
    /// </summary>
    public class MovieLookupLocation
    {
        public string name { get; set; }
        public string address { get; set; }
        public List<MovieLookupShow> schedule { get; set; }
    }
}