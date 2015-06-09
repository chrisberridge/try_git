/*==========================================================================*/
/* Source File:   EVENTSERVICE.CS                                           */
/* Description:   When reading into Planepoly JSON structures, this class   */
/*                is the entry point in the structure defined therein.      */
/*                Contains the service structure.                           */
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
    /// Contains the service structure.
    /// </summary>
    public class EventService
    {
        public string inicio { get; set; }
        public string salaForo { get; set; }
        public List<EventLocation> ptos { get; set; }
        public string img { get; set; }
        public string fin { get; set; }
        public int estr { get; set; }
        public string urlVideo { get; set; }
        public string urlWww { get; set; }
        public string url { get; set; }
        public int id { get; set; }
        public string nombre { get; set; }
        public string genero { get; set; }
        public string sinopsis { get; set; }
        public int tipo { get; set; }
        public int calif { get; set; }
        public int califres { get; set; }
    }
}