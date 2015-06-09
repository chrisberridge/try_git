/*==========================================================================*/
/* Source File:   IMAGEONLY.CS                                              */
/* Description:   Holds an one image an HTML extraction                     */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Sep.06/2013                                               */
/* Last Modified: Jun.26/2014                                               */
/* Version:       1.2                                                       */
/* Copyright (c), 2013, 2014 Arkix, El Colombiano                           */
/*==========================================================================*/

/*===========================================================================
History
Sep.06/2013 COQ File created.
============================================================================*/

namespace EC.Utils.Domain {
    /// <summary>
    /// Used to represent one image present in extracted HTML or it may be used where an image and cutline is required.
    /// </summary>
    public class ImageOnly {
        public string Name { get; set; }
        public string Content { get; set; }
    }
}
