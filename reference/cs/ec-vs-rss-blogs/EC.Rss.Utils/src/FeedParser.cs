/*==========================================================================*/
/* Source File:   FEEDPARSER.CS                                             */
/* Description:   A simple RSS, RDF and ATOM feed parser.                   */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Mar.21/2014                                               */
/* Last Modified: Mar.26/2014                                               */
/* Version:       1.3                                                       */
/* Copyright (c), 2014 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Mar.21/2014 COQ File created.
============================================================================*/

////
// Originally it was borrowed from http://www.anotherchris.net/csharp/simplified-csharp-atom-and-rss-feed-parser/
// Set as reference.
////

using EC.Rss.Utils.Domain;
using EC.Rss.Utils.Domain.Type;
using System;
using System.Collections.Generic;
using System.Xml;

namespace EC.Rss.Utils {
    /// <summary>
    /// A simple RSS, RDF and ATOM feed parser.
    /// </summary>
    public class FeedParser {
        /// <summary>
        /// Parses the given <see cref="FeedType"/> and returns a <see cref="IList&amp;lt;Item&amp;gt;"/>.
        /// </summary>
        /// <returns></returns>
        public IList<RssItem> Parse(string url, RssFeedType feedType) {
            switch (feedType) {
                case RssFeedType.RSS:
                    return ParseRss(url, false);
                default:
                    throw new NotSupportedException(string.Format("{0} is not supported", feedType.ToString()));
            }
        }

        /// <summary>
        /// Parses an RSS feed and returns a <see cref="IList&amp;lt;Item&amp;gt;"/>.
        /// <param name="url">Location of data to match</param>
        /// <param name="retrieveAll">True if all items, when false only the first item is returned.</param>
        /// </summary>
        public IList<RssItem> ParseRss(string url, bool retrieveAll) {
            List<RssItem> rssList = new List<RssItem>();
            try {
                XmlDocument doc = new XmlDocument();
                doc.Load(url);
                XmlNodeList items = doc.GetElementsByTagName("item");

                foreach (XmlElement it in items) {
                    var children = it.ChildNodes;
                    RssItem rssIt = new RssItem();
                    foreach (XmlElement node in children) {
                        if (!node.IsEmpty) {
                            switch (node.Name) {
                                case "title":
                                    rssIt.Title = node.InnerText;
                                    break;
                                case "link":
                                    if (node.InnerText != "") {
                                        rssIt.Link = node.InnerText;
                                    }
                                    break;
                                case "comments":
                                    rssIt.LinkComments = node.InnerText;
                                    break;
                                case "pubDate":
                                    rssIt.PublishDate = ParseDate(node.InnerText);
                                    break;
                                case "dc:creator":
                                    rssIt.Creator = node.InnerText;
                                    break;
                                case "description":
                                    rssIt.Content = node.InnerText;
                                    break;
                                case "slash:comments":
                                    if (node.InnerText != "") {
                                        rssIt.NumComments = Convert.ToInt32(node.InnerText);
                                    }
                                    break;
                            }
                        }                        
                    }
                    rssList.Add(rssIt);
                    if (!retrieveAll) {
                        break;
                    }
                }
                return rssList;
            }
            catch {                
                return rssList;
            }
        }

        /// <summary>
        /// Converts a string date in RSS2 format to a DateTime object.
        /// </summary>
        /// <param name="date">String representation to convert</param>
        /// <returns>DateTime object</returns>
        private DateTime ParseDate(string date) {
            DateTime result;
            if (DateTime.TryParse(date, out result))
                return result;
            else
                return DateTime.MinValue;
        }
    }
}
