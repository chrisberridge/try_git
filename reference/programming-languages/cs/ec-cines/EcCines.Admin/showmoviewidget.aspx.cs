/*==========================================================================*/
/* Source File:   SHOWMOVIEWIDGET.CS                                        */
/* Description:   Page to visualize the Movie Schedule Widget               */
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
    /// Page to visualize the Movie Schedule Widget
    /// </summary>
    public partial class showmoviewidget : WebPageBase {
        /// <summary>
        /// Page Load event
        /// </summary>
        /// <param name="sender">Object which fires the event</param>
        /// <param name="e">Event arguments</param>
        protected void Page_Load(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("Page_load Starts");
            }
            if (log.IsDebugEnabled) {
                log.Debug("Page_load Ends");
            }
        }

        /// <summary>
        ///  Default Constructor
        /// </summary>
        public showmoviewidget()
            : base() {
        }
    }
}