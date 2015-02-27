/*==========================================================================*/
/* Source File:   RSSFEEDTYPE.CS                                            */
/* Description:   Helper enumeration for RSS types                          */
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

namespace EC.Rss.Utils.Domain.Type {
    /// <summary>
    /// Represents the XML format of a feed.
    /// </summary>
    public enum RssFeedType {
        /// <summary>
        /// Really Simple Syndication format.
        /// </summary>
        RSS,
        /// <summary>
        /// RDF site summary format.
        /// </summary>
        RDF,
        /// <summary>
        /// Atom Syndication format.
        /// </summary>
        Atom
    }
}