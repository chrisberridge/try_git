/*==========================================================================*/
/* Source File:   READJSON.ASPX.CS                                          */
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
using System.Configuration;

namespace moviedetail
{
    /// <summary>
    /// Allows as microservice application to consume JSON files for system. Use with a parameter
    /// in URL. 
    /// 
    /// A query string parameter is sent to this page. Given name of 'm' and has values of '1:Get movies' file,
    /// '2:Get Movies Catalog' and so on.
    /// </summary>
    public partial class readjson : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ManageMovieCatalog mmc = new ManageMovieCatalog();
            mmc.catalogNameFileName = @ConfigurationManager.AppSettings[GlobalConstants.FileMoviesCatalogKey];
            mmc.moviesFileName = @ConfigurationManager.AppSettings[GlobalConstants.FileMoviesKey];
            mmc.dbConnection = @ConfigurationManager.AppSettings[GlobalConstants.ConnectionKey];

            string m = Request.QueryString["m"];
            Response.AddHeader("Access-Control-Allow-Origin", "*");
            if (m == null || (m != "1" && m != "2"))
            {
                Response.Write("Invalid");
                return;
            }
            Response.Write(mmc.RetrieveJSonContentsForModule(m));            
        }
    }
}