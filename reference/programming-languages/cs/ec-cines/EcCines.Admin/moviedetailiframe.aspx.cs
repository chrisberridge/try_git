/*==========================================================================*/
/* Source File:   MOVIEDETAILIFRAME.ASPX.CS                                 */
/* Description:   Page to simulate article iframe which shows a movie       */
/*                schedule.                                                 */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: Mar.11/2015                                               */
/* Version:       1.3                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
============================================================================*/

using System;

namespace EcCines {
    /// <summary>
    /// Page to simulate article iframe which shows a movie schedule.  
    /// </summary>
    public partial class moviedetailiframe : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {
            string m = Request.QueryString["m"];
            string htmlApoyoText = "";
            htmlApoyoText += "<script language='javascript'>$(document).ready(function(){LoadMovie(" + m + ")});</script>";
            htmlApoyoText += "<hr id='hrMovie' style='margin:0px;'>";
            htmlApoyo.Text = htmlApoyoText;
        }
    }
}