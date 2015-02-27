/*==========================================================================*/
/* Source File:   RSSURLINFO.CS                                             */
/* Description:   Helper class to assist with general purpose operations.   */
/*                Represents an URL feed item.                              */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Mar.25/2014                                               */
/* Last Modified: Mar.25/2014                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2014 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Mar.25/2014 COQ File created.
============================================================================*/

using System.Collections.Generic;

namespace EC.Rss.Utils.Domain {
    /// <summary>
    /// Helper class to assist with general purpose operations.
    /// Represents an URL feed item.
    /// </summary>
    public class RssUrlInfo {
        public int Order { get; set; }
        public int ObjectSECode { get; set; }
        public string Url { get; set; }
        public List<RssItem> DocItemList { get; set; }
    }
}
