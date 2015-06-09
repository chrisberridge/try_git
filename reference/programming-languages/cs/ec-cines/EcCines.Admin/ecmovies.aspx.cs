/*==========================================================================*/
/* Source File:   ECMOVIES.ASPX.CS                                          */
/* Description:   Page to generate the movie catalog in JSON format         */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: May.05/2015                                               */
/* Version:       1.5                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
============================================================================*/

using System;
using ELCOLOMBIANO.EcCines.Business;
using ELCOLOMBIANO.EcCines.Common;
using ELCOLOMBIANO.EcCines.Web;

namespace EcCines.Admin {
    /// <summary>
    /// Compile All Movies Scheduled and store themn in JSON files.
    /// </summary>
    public partial class ecmovies : WebPageBase {
        /// <summary>
        /// Page load event
        /// </summary>
        /// <param name="sender">Object which fires the event</param>
        /// <param name="e">Event arguments</param>
        protected void Page_Load(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("Page_Load Starts");
                log.Debug("JSON files compiled");
            }
            string imgPathUrl = Request.Url.Scheme + "://" + Request.Url.Authority + "/" + Settings.ImageFolder + "/";
            ManageMovieCatalog mmc = new ManageMovieCatalog(Settings.JSONFolder + @"\" + Settings.FileMovieCatalog, Settings.JSONFolder + @"\" + Settings.FileMovies, imgPathUrl.Replace(@"\", "/"));
            mmc.CompileAllMoviesSchedule();
            Response.Write("Movie Schedule generated on " + DateTime.Now.ToString());
            Response.AddHeader("Access-Control-Allow-Origin", "*");
            if (log.IsDebugEnabled) {
                log.Debug("Page_Load Ends");                
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ecmovies()
            : base() {
        }
    }
}