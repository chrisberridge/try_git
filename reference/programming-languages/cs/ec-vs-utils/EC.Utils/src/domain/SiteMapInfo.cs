/*==========================================================================*/
/* Source File:   SITEMAPINFO.CS                                            */
/* Description:   Collects information about the files to process.          */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Jul.18/2013                                               */
/* Last Modified: Aug.13/2013                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Jul.18/2013 COQ File created.
============================================================================*/
namespace EC.Utils.Domain {
    /// <summary>
    /// Collects information about the files to process.
    /// </summary>
    public class SiteMapInfo {
        /// <summary>
        /// Path to the file or an URL
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Represents a source sitemap for 1:site, 2:news, 3:video, 4: ObjetoSE source.
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Holds only the file name part when using a file path for source.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Holds the filter used (if any used).
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// Default constructro
        /// </summary>
        /// <param name="source">Path to the file or an URL</param>
        /// <param name="fileName">Holds only the file name part when using a file path for source</param>
        /// <param name="type">Represents a source sitemap for 1:site, 2:news, 3:video</param>
        /// <param name="filter">When scanning file, gather only those url containing filter value.</param>
        public SiteMapInfo(string source, string fileName, int type, string filter) {
            this.Source = source;
            this.FileName = fileName;
            this.Type = type;
            this.Filter = filter;
        }
    }
}
