/*==========================================================================*/
/* Source File:   UTILS.CS                                                  */
/* Description:   When reading into Planepoly JSON structures, this class   */
/*                is the entry point in the structure defined therein.      */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Jun.17/2013                                               */
/* Last Modified: Jun.18/2013                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Jun.17/2013 COQ File created.
============================================================================*/
using System.IO;
using System.Net;

namespace ElColombiano.Service.Helper
{
    /// <summary>
    /// When reading into Planepoly JSON structures, this class
    /// is the entry point in the structure defined therein.   
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// Given the url, retrieves its contents (usually HTML code, JSON, or any other text form from the web server).
        /// </summary>
        /// <param name="url">Page name to visit</param>
        /// <returns>The contents of the page to visit or empty if none found.</returns>
        public string ReadHtmlPageContent(string url)
        {
            WebClient client = new WebClient();
            Stream data = client.OpenRead(url);
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();
            data.Close();
            reader.Close();
            return (s);
        }
    }
}