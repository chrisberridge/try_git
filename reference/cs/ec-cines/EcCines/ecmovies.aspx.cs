/*==========================================================================*/
/* Source File:   ECMOVIES.ASPX.CS                                          */
/* Description:   Page to generate the movie catalog in JSON format         */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: Mar.06/2015                                               */
/* Version:       1.6                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
============================================================================*/
using ELCOLOMBIANO.EcCines.Business;
using System;

namespace EcCines
{
    /// <summary>
    /// Compile All Movies Scheduled and store themn in JSON files.
    /// </summary>
    public partial class ecmovies : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string imgPathUrl = Request.Url.Scheme + "://" + Request.Url.Authority + "/" + Settings.ImageFolder + "/";
            ManageMovieCatalog mmc = new ManageMovieCatalog(Settings.JSONFolder + @"\" + Settings.FileMovieCatalog, Settings.JSONFolder + @"\" + Settings.FileMovies, imgPathUrl.Replace(@"\", "/"));
            mmc.CompileAllMoviesSchedule();
            Response.Write("Movie Schedule generated on " + DateTime.Now.ToString());
            Response.AddHeader("Access-Control-Allow-Origin", "*");
        }
    }
}