/*==========================================================================*/
/* Source File:   ITERWEBMAPINFO.CS                                         */
/* Description:   Computes what is needed to generate the Iterweb migration */
/*                processing.                                               */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Aug.12/2013                                               */
/* Last Modified: Sep.25/2014                                               */
/* Version:       1.11                                                      */
/* Copyright (c), 2013, 2014 Arkix, El Colombiano                           */
/*==========================================================================*/

/*===========================================================================
History
Aug.12/2013 COQ File created.
============================================================================*/

using System;

namespace EC.Utils.Domain {
    /// <summary>
    /// Computes what is needed to generate the Iterweb migration processing.
    /// </summary>
    public class IterwebMapInfo {
        /// <summary>
        /// Target Iterweb layout name
        /// </summary>
        public string IterwebLayoutName { get; set; }

        /// <summary>
        /// Holds the title for document.
        /// </summary>
        public string SEName { get; set; }

        /// <summary>
        /// Full url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Full url title
        /// </summary>
        public string UrlTitle { get; set; }

        /// <summary>
        /// Maps to legacy url.
        /// </summary>
        public string UrlPath { get; set; }

        /// <summary>
        /// Type of sitemap being used 
        /// </summary>
        public int SitemapType { get; set; }

        /// <summary>
        /// A named global id for SE4
        /// </summary>
        public string IDSE4 { get; set; }

        /// <summary>
        /// A named artigle id for SE4
        /// </summary>
        public string IDSE4ArticleId { get; set; }

        /// <summary>
        /// Layout being used when processing its metadata attributes.
        /// </summary>
        public string Layout { get; set; }

        /// <summary>
        /// ID for old document (not in SE4)
        /// </summary>
        public string IDOldDoc { get; set; }

        /// <summary>
        /// SE4 object id.
        /// </summary>
        public long IdObjetoSE { get; set; }

        /// <summary>
        /// Use a date to display for document.
        /// </summary>
        public DateTime DisplayDate { get; set; }

        /// <summary>
        /// Usa a date for update field in document.
        /// </summary>
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// For the OLD document this field containts its compiled structured.
        /// It it is null, then it must be compiled.
        /// </summary>
        public string JsonContent { get; set; }

        /// <summary>
        /// What to do for document.
        /// @see MigrateStatuscode.cs for code meaning
        /// </summary>
        public int OldDocStatus { get; set; }

        /// <summary>
        /// Althoug this value is stored in the JSonContent field,
        /// it is saved apart in order to make SQL queries speedy as
        /// JsonContent is saved as a TEXT field which is slow for
        /// LIKE or other comparisons.
        /// </summary>
        public int OldDocTemplateType { get; set; }

        /// <summary>
        /// Marks a record to be processed. 0: not processed, 1: processed.
        /// NOTE: Only records not processed are loaded into memory.
        /// By using this field we intend to run the application as many times as required
        /// to circumvent memory issues or hard drive space.
        /// </summary>
        public int Processed { get; set; }

        /// <summary>
        /// Sitemap driving data table identity field.
        /// </summary>
        public long IdSitemap { get; set; }

        /// <summary>
        /// Brightcove id for video.
        /// </summary>
        public String IdBrightCove { get; set; }

        /// <summary>
        /// Indicates if article is marked as an especial case of Infographic to upload.
        /// </summary>
        public int IsInfographic { get; set; }
    }
}
