/*==========================================================================*/
/* Source File:   ECMOVIESSEARCH.ASPX.CS                                    */
/* Description:   Microservice that returns a JSON when asked to search for */
/*                Movies.                                                   */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: Mar.11/2015                                               */
/* Version:       1.4                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
============================================================================*/

using System;
using ELCOLOMBIANO.EcCines.Business;

namespace EcCines {
    /// <summary>
    /// Microservice that returns a JSON when asked to search for Movies.
    /// </summary>
    public partial class ecmoviesearch : System.Web.UI.Page {
        /// <summary>
        /// Given parameters 't:for theater id', 'm:Movie Id', and 'g:Gender id', computes a lookup in the catalog
        /// matched records. All results must be guaranteed to be sorted.
        /// </summary>
        /// <param name="t">Theater Id. Possible value are '-1', or '2'</param>
        /// <param name="m">Movie Id. Possible value are '-1', or '1|29' this means movieId=1 and movieIdFormat=29</param>
        /// <param name="g">Gender Id. Possible value are '-1', or '2'</param>
        /// <returns>A list of 'Movie'  objects serialized as JSON. If list is empty, serialized should be empty.</returns>
        protected void Page_Load(object sender, EventArgs e) {
            ManageMovieCatalog mmc = new ManageMovieCatalog(Settings.JSONFolder + @"\" + Settings.FileMovieCatalog, Settings.JSONFolder + @"\" + Settings.FileMovies);
            string t = Request.QueryString["t"];
            string m = Request.QueryString["m"];
            string g = Request.QueryString["g"];

            if (t == null) {
                t = "-1";
            }
            if (m == null) {
                m = "-1";
            }
            if (g == null) {
                g = "-1";
            }

            // Check that parameters are OK
            int tst;
            try {
                tst = Convert.ToInt32(t);
            } catch (Exception) {
                t = "-1";
            }
            try {
                var aVal = m.Split('|');
                tst = Convert.ToInt32(aVal[0]);
            } catch (Exception) {
                m = "-1";
            }
            try {
                tst = Convert.ToInt32(g);
            } catch (Exception) {
                g = "-1";
            }
            Response.Write(mmc.Search(t, m, g));
            Response.AddHeader("Access-Control-Allow-Origin", "*");
        }
    }
}