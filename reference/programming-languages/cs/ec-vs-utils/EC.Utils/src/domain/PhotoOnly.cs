/*==========================================================================*/
/* Source File:   PHOTOONLY.CS                                              */
/* Description:   Holds am image from an HTML extraction                    */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Nov.26/2013                                               */
/* Last Modified: Nov.26/2013                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Sep.06/2013 COQ File created.
============================================================================*/

namespace EC.Utils.Domain {
    /// <summary>
    /// Used to represent an image for extracted HTML.
    /// </summary>
    public class PhotoOnly {
        public string ImageName { get; set; }
        public string ImageSrc { get; set; }
    }
}