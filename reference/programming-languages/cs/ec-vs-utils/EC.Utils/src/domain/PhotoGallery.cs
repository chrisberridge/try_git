/*==========================================================================*/
/* Source File:   PHOTOGALLERY.CS                                           */
/* Description:   Holds a Photo Gallery List from an HTML extraction        */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Sep.25/2013                                               */
/* Last Modified: Sep.25/2013                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Sep.25/2013 COQ File created.
============================================================================*/

namespace EC.Utils.Domain {
    /// <summary>
    /// Used to represent a list of big images as a photo gallery for extracted HTML.
    /// </summary>
    public class PhotoGallery {
        public string PhotoBigName { get; set; }
        public string PhotoBigContent { get; set; }
        public string PhotoSmallName { get; set; }
        public string PhotoSmallContent { get; set; }
        public string PhotoCreditName  { get; set; }
        public string PhotoCreditContent { get; set; }
        public string PhotoFooterName  { get; set; }
        public string PhotoFooterContent  { get; set; }
    }
}