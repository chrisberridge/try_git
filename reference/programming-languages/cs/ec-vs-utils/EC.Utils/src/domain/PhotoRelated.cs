/*==========================================================================*/
/* Source File:   PHOTORELATED.CS                                           */
/* Description:   Holds an image with is foother from an HTML extraction    */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Sep.06/2013                                               */
/* Last Modified: Nov.26/2013                                               */
/* Version:       1.2                                                       */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Sep.06/2013 COQ File created.
============================================================================*/

namespace EC.Utils.Domain {
    /// <summary>
    /// Used to represent an image with its footer for extracted HTML.
    /// </summary>
    public class PhotoRelated {
        public string ImageName { get; set; }
        public string ImageSrc { get; set; }
        public string FooterName { get; set; }
        public string Footer { get; set; }
    }
}