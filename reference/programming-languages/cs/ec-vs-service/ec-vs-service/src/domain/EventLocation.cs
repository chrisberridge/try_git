/*==========================================================================*/
/* Source File:   EVENTLOCATION.CS                                          */
/* Description:   When reading into Planepoly JSON structures, this class   */
/*                is the entry point in the structure defined therein.      */
/*                Contains the location structure.                          */
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
    /// When reading into Planepoly JSON structures, this class
    /// is the entry point in the structure defined therein.
    /// Contains the location structure. 
    /// </summary>
    public class EventLocation
    {
        public string nombre { get; set; }
        public int id { get; set; }
        public string direccion { get; set; }
        public List<EventShow> funcs { get; set; }
        public double calif { get; set; }
        public int califres { get; set; }
        public double @long { get; set; }
        public double lat { get; set; }
    }
}