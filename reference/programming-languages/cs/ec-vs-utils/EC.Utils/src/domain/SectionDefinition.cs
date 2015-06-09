/*==========================================================================*/
/* Source File:   SECTIONDEFINITION.CS                                      */
/* Description:   Holds a title from an HTML extraction                     */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          May.07/2014                                               */
/* Last Modified: Jun.23/2014                                               */
/* Version:       1.2                                                       */
/* Copyright (c), 2014 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Sep.06/2013 COQ File created.
============================================================================*/

using System;

namespace EC.Utils.Domain {
    public class SectionDefinition {
        public string ApplyTo { get; set; }
        public string PageTemplate { get; set; }
        public string Qualification { get; set; }
        public string Url { get; set; }
        public bool DefaultSection { get; set; }
    }
}
