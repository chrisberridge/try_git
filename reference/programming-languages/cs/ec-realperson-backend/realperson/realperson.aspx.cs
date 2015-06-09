/*==========================================================================*/
/* Source File:   REALPERSON.ASPX.CS                                        */
/* Description:   Web form to validate through Jquery POST Ajax if user is  */
/*                a real person.                                            */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.14/2014                                               */
/* Last Modified: Feb.14/2014                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2014 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Feb.14/2014 COQ File created.
============================================================================*/

using System;
using EC.Utils;
using EC.Utils.Domain;
using Newtonsoft.Json;
namespace EC.Web {
    /// <summary>
    /// Web form to validate through Jquery POST Ajax if user is  a real person.
    /// </summary>
    public partial class RealPersonWeb : System.Web.UI.Page {
        /// <summary>
        /// Two values are of interest to this page. If same hash is obtained, then a real person
        /// submitted the form.
        /// </summary>
        /// <param name="sender">Sender object which fired the event</param>
        /// <param name="e">Parameters sent from the event manager.</param>
        protected void Page_Load(object sender, EventArgs e) {
            RealPersonHash rph = new RealPersonHash();
            RealPersonResponse rpr = new RealPersonResponse();
            string defaultRealValue = null;
            string defaultRealHashValue = null;
            
            if (Request.Form["defaultReal"] == null) {
                if (Request.QueryString["defaultReal"] == null) {
                    defaultRealValue = "";
                } else {
                    defaultRealValue = Request.QueryString["defaultReal"];
                }
            } else {
                defaultRealValue = Request.Form["defaultReal"];
            }
            if (Request.Form["defaultRealHash"] == null) {
                if (Request.QueryString["defaultRealHash"] == null) {
                    defaultRealHashValue = "";
                } else {
                    defaultRealHashValue = Request.QueryString["defaultRealHash"];
                }
            } else {
                defaultRealHashValue = Request.Form["defaultRealHash"];
            }
            Boolean b = rph.IsReal(defaultRealValue, defaultRealHashValue);
            if (b) {
                rpr.msg = "Valid";
                rpr.val = 1;
            }
            else {
                rpr.msg = "Not Valid";
                rpr.val = 0;
            }
            string s = JsonConvert.SerializeObject(rpr);            
            Response.Write(s);
            Response.AddHeader("Access-Control-Allow-Origin", "*");
        }
    }
}