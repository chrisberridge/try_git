/*==========================================================================*/
/* Source File:   TITLE.CS                                                  */
/* Description:   Holds a title from an HTML extraction                     */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Sep.06/2013                                               */
/* Last Modified: Sep.10/2013                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Sep.06/2013 COQ File created.
============================================================================*/

namespace EC.Utils.Domain {
    /// <summary>
    /// Used to represent a title extracted HTML.
    /// </summary>
    public class Title {
        public string Name { get; set; }
        public string Content { get; set; }
    }
}
