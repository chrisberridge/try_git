/*==========================================================================*/
/* Source File:   CXENSE.ASPX.CS                                            */
/* Description:   Using this service page where the EL COLOMBIANO search    */
/*                functinality is nurtured.                                 */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          May.14/2013                                               */
/* Last Modified: Jun.18/2013                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
May.14/2013 COQ File created.
============================================================================*/
using System;
using System.IO;

namespace ElColombiano.Service
{
    public partial class readjson : System.Web.UI.Page
    {
        /// <summary>
        /// The purpose of this page is to service JSON Files. The files are already generated 
        /// elsewhere and are loaded from file system.
        /// 
        /// A query string parameter is sent to this page. Given name of 'm' and has values of '1:Get movies' file,
        /// '2:Get Movies Catalog', 3:Get events.
        /// 
        /// </summary>
        /// <param name="sender">Object which fired the event</param>
        /// <param name="e">Parameters to the event</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            string[] fileNameList = {@"D:\SitiosWeb\Sitio\EC100A_Servicios\EC100A_PlanepolyWidget\planepoly-movies.json",
            @"D:\SitiosWeb\Sitio\EC100A_Servicios\EC100A_PlanepolyWidget\planepoly-movies-catalog.json", 
            @"D:\SitiosWeb\Sitio\EC100A_Servicios\EC100A_PlanepolyWidget\planepoly-events.json"};
            string fileName = "";
            string m = Request.QueryString["m"];

            if (m == null || (m != "1" && m != "2" && m != "3"))
            {
                Response.Write("Invalid");
                return;
            }
            switch (m)
            {
                case "1":
                    fileName = fileNameList[0];
                    break;
                case "2":
                    fileName = fileNameList[1];
                    break;
                case "3":
                    fileName = fileNameList[2];
                    break;
                default:
                    break;
            }
            string s;
            using (StreamReader reader = new StreamReader(fileName))
            {
                s = reader.ReadToEnd();
            }
            Response.Write(s);
            Response.AddHeader("Access-Control-Allow-Origin", "*");
        }
    }
}