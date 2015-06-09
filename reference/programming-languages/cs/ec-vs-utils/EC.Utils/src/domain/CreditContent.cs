/*==========================================================================*/
/* Source File:   CREDITCONTENT.CS                                          */
/* Description:   Holds a doc credit metadata field from an HTML extraction */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Sep.19/2013                                               */
/* Last Modified: Sep.19/2013                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Sep.19/2013 COQ File created.
============================================================================*/

namespace EC.Utils.Domain {
    /// <summary>
    /// Holds a doc credit metadata field from an HTML extraction 
    /// </summary>
    public class CreditContent {
        public string AuthorName { get; set; }
        public string AuthorText { get; set; }
        public string CityName { get; set; }
        public string CityText { get; set; }
        public string DisplayDateName { get; set; }
        public string DisplayDateText { get; set; }
    }
}