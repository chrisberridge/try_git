/*==========================================================================*/
/* Source File:   READERHELP.CS                                             */
/* Description:   Used to represent a box for title/text to help user       */
/*                understand the document he is reading.                    */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Sep.09/2013                                               */
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
    /// Used to represent a box for title/text to help user understand the document he is reading.
    /// </summary>
    public class ReaderHelp {
        public string NameTitle { get; set; }
        public string NameTitleContent { get; set; }
        public string NameText { get; set; }
        public string NameTextContent { get; set; }
    }
}
