/*==========================================================================*/
/* Source File:   ECMOVIES.ASPX.CS                                          */
/* Description:   Project constants                                         */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: Feb.18/2015                                               */
/* Version:       1.2                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
============================================================================*/
using EC.Business;
using EC.Utils.Constants;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace moviedetail
{
    /// <summary>
    /// Compile All Movies Scheduled and store themn in JSON files.
    /// </summary>
    public partial class ecmovies : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ManageMovieCatalog mmc = new ManageMovieCatalog();
            mmc.catalogNameFileName = @ConfigurationManager.AppSettings[GlobalConstants.FileMoviesCatalogKey];
            mmc.moviesFileName = @ConfigurationManager.AppSettings[GlobalConstants.FileMoviesKey];
            mmc.dbConnection = @ConfigurationManager.AppSettings[GlobalConstants.ConnectionKey];
            mmc.CompileAllMoviesSchedule();
            Response.Write("Movie Schedule generated on " + DateTime.Now.ToString());
            Response.AddHeader("Access-Control-Allow-Origin", "*");
        }
    }
}