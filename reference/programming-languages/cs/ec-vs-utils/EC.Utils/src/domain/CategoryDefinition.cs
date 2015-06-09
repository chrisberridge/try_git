/*==========================================================================*/
/* Source File:   CATEGORYDEFINITION.CS                                     */
/* Description:   Defines a Category in system                              */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          May.07/2014                                               */
/* Last Modified: Sep.06/2014                                               */
/* Version:       1.2                                                       */
/* Copyright (c), 2014 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
May.07/2014 COQ File created.
============================================================================*/

namespace EC.Utils.Domain {
    /// <summary>
    /// Defines a Category in system
    /// </summary>
    public class CategoryDefinition {
        public string Name { get; set; }
        public string NamePath { get; set; }
        public int Main { get; set; }
        public int SetAttribute {get; set;}
    }
}
