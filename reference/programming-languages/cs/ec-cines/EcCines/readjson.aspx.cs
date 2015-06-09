/*==========================================================================*/
/* Source File:   READJSON.ASPX.CS                                          */
/* Description:   Microservice to return the requested JSON file.           */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: May.05/2015                                               */
/* Version:       1.9                                                       */
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

namespace EcCines {
    /// <summary>
    /// Allows as microservice application to consume JSON files for system. Use with a parameter
    /// in URL. 
    /// 
    /// A query string parameter is sent to this page. Given name of 'm' and has values of '1:Get movies' file,
    /// '2:Get Movies Catalog' and so on.
    /// </summary>
    public partial class readjson : WebPageBase {
        /// <summary>
        /// Page Load Event
        /// </summary>
        /// <param name="sender">Object which fires the event</param>
        /// <param name="e">Event argument</param>
        protected void Page_Load(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("Page_Load Starts");
            }

            ManageMovieCatalog mmc = new ManageMovieCatalog(Settings.JSONFolder + @"\" + Settings.FileMovieCatalog, Settings.JSONFolder + @"\" + Settings.FileMovies);

            string m = Request.QueryString["m"];
            if (m == null) {
                m = "";
            }
            if (log.IsDebugEnabled) {
                log.Debug("Parameter received m=[" + m + "]");
            }
            Response.AddHeader("Access-Control-Allow-Origin", "*");
            if (m == null || (m != "1" && m != "2")) {
                Response.Write("Información incorrecta");
                if (log.IsDebugEnabled) {
                    log.Debug("No info returned");
                }
                return;
            }
            Response.Write(mmc.RetrieveJSonContentsForModule(m));
            if (log.IsDebugEnabled) {
                log.Debug("Page_Load Ends");
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public readjson()
            : base() {
        }
    }
}