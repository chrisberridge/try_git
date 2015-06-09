/*==========================================================================*/
/* Source File:   MOVIEDETAILIFRAME.ASPX.CS                                 */
/* Description:   Page to simulate article iframe which shows a movie       */
/*                schedule.                                                 */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: May.05/2015                                               */
/* Version:       1.6                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
============================================================================*/

using System;
using ELCOLOMBIANO.EcCines.Web;

namespace EcCines.Admin {
    /// <summary>
    /// Page to simulate article iframe which shows a movie schedule.  
    /// </summary>
    public partial class moviedetailiframe : WebPageBase {
        /// <summary>
        /// Page load for page
        /// </summary>
        /// <param name="sender">Object which fires the event</param>
        /// <param name="e">Event arguments</param>
        protected void Page_Load(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("Page_Load starts");
            }
            string m = Request.QueryString["m"];
            string htmlApoyoText = "";
            htmlApoyoText += "<script language='javascript'>$(document).ready(function(){LoadMovie(" + m + ")});</script>";
            htmlApoyoText += "<hr id='hrMovie' style='margin:0px;'>";
            htmlApoyo.Text = htmlApoyoText;
            if (log.IsDebugEnabled) {
                log.Debug("Page_Load ends");
            }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public moviedetailiframe()
            : base() {
        }
    }
}