/*==========================================================================*/
/* Source File:   EVENTSHOW.CS                                              */
/* Description:   When reading into Planepoly JSON structures, this class   */
/*                is the entry point in the structure defined therein.      */
/*                Contains the Show structure.                              */
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
namespace ElColombiano.Service.Domain
{
    /// <summary>
    /// When reading into Planepoly JSON structures, this class
    /// is the entry point in the structure defined therein.   
    /// Contains the Show structure.                           
    /// </summary>
    public class EventShow
    {
        public string hora { get; set; }
        public int dia { get; set; }
    }
}