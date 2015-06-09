/*==========================================================================*/
/* Source File:   PHOTOONLYGALLERY.CS                                       */
/* Description:   Holds a list of images and a footer for that list from    */
/*                an HTML extraction                                        */
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


using System.Collections.Generic;
namespace EC.Utils.Domain {
    /// <summary>
    /// Holds a list of images and a footer for that list from an HTML extraction
    /// </summary>
    public class PhotoOnlyGallery {

        /// <summary>
        /// Default constructor
        /// </summary>
        public PhotoOnlyGallery() {
            this.PhotoList = new List<PhotoOnly>();
            this.Clean();
        }

        /// <summary>
        /// Helper method to clean this object  contents.
        /// </summary>
        public void Clean() {
            this.PhotoList.Clear();
            this.Footer = "";
            this.FooterName = "";
            this.FooterTitleName = "";
            this.FoooterTitle = "";
        }

        /// <summary>
        /// Examines its properties to infer if it is empty
        /// </summary>
        /// <returns>True if empty</returns>
        public bool IsEmpty() {
            var photoListCnt = PhotoList.Count;
            var footerEmpty = (Footer == "" && FooterName == "" &&
                               FoooterTitle == "" && FooterTitleName == "");

            return (photoListCnt == 0 && footerEmpty);
        }

        public List<PhotoOnly> PhotoList { get; set; }
        public string FooterName { get; set; }
        public string Footer { get; set; }
        public string FooterTitleName { get; set; }
        public string FoooterTitle { get; set; }
    }
}