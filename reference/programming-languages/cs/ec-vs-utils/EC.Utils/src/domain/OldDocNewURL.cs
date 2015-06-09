/*==========================================================================*/
/* Source File:   OLDDOCNEWURL.CS                                           */
/* Description:   Holds the data for OldDocsUrls table                      */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Nov.13/2013                                               */
/* Last Modified: Nov.15/2013                                               */
/* Version:       1.3                                                       */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Nov.13/2013 COQ File created.
============================================================================*/

using System;

namespace EC.Utils.Domain {
    /// <summary>
    /// Holds the data for OldDocsUrls table
    /// </summary>
    public class OldDocNewURL {
        public int IdOldDocsNewUrls { get; set; }
        public string IDOld { get; set; }
        public string Url { get; set; }
        public string UrlHost { get; set; }
        public string UrlPath { get; set; }
        public string UrlParameters { get; set; }
        public string UrlProcess { get; set; }
        public string UrlProcessHost { get; set; }
        public string UrlProcessPath { get; set; }
        public string UrlProcessParameters { get; set; }
        public int Processed { get; set; }
        public DateTime DateProcessed { get; set; }
    }
}
