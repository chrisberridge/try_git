/*==========================================================================*/
/* Source File:   RSSITEM.CS                                                */
/* Description:   Helper class to assist with general purpose operations.   */
/*                Represents a feed item.
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Mar.21/2014                                               */
/* Last Modified: Mar.21/2014                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2014 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Mar.21/2014 COQ File created.
============================================================================*/

using EC.Rss.Utils.Domain.Type;
using System;

namespace EC.Rss.Utils.Domain {
    /// <summary>
    /// Helper class to assist with general purpose operations.
    /// Represents a feed item.
    /// </summary>
    public class RssItem {
        public string Link { get; set; }
        public string LinkComments { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PublishDate { get; set; }
        public RssFeedType FeedType { get; set; }
        public string Creator { get; set; }
        public int NumComments { get; set; }        

        /// <summary>
        /// Default Constructor
        /// </summary>
        public RssItem()
        {
            this.Link = "";
            this.Title = "";
            this.Content = "";
            this.PublishDate = DateTime.Today;
            this.FeedType = RssFeedType.RSS;
        }
    }
}