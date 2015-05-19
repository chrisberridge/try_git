/*==========================================================================*/
/* Source File:   EVENTLOOKUPSHOW.CS                                        */
/* Description:   This is a remapping in the structure for Planepoly JSON   */
/*                namely for El Colombiano own requirements.                */
/*                Contains the Show structure.                              */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Jun.18/2013                                               */
/* Last Modified: Jun.18/2013                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Jun.18/2013 COQ File created.
============================================================================*/
namespace ElColombiano.Planepoly
{
    /// <summary>
    ///  This is a remapping in the structure for Planepoly JSON
    ///  namely for El Colombiano own requirements.             
    ///  Contains the Show structure.
    /// </summary>
    public class EventLookupShow
    {
        public int frequency { get; set; }
        public string name { get; set; }
        public string hours { get; set; }
    }
}