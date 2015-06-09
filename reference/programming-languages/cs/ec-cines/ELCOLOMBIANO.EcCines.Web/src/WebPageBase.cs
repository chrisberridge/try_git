/*==========================================================================*/
/* Source File:   WEBPAGEBASE.CS                                            */
/* Description:   Base class for common methods for pages                   */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Apr.24/2015                                               */
/* Last Modified: May.05/2015                                               */
/* Version:       1.3                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Apr.24/2015 COQ File created.
============================================================================*/
using System;
using System.Reflection;
using System.Web.UI;
using ELCOLOMBIANO.EcCines.Common;
using log4net;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace ELCOLOMBIANO.EcCines.Web {
    /// <summary>
    /// Base class for common methods for pages
    /// </summary>
    public class WebPageBase : Page {
        protected readonly ILog log = null;

        /// <summary>
        /// Register a Toastr Javascript language on Page load.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="msg"></param>
        public void registerToastrMsg(MessageType t, string msg) {
            if (log.IsDebugEnabled) {
                log.Debug("registerToastrMsg");
            }
            var js = Utils.showToastrMsg(t, msg);
            ScriptManager.RegisterStartupScript(this, typeof(Page), Guid.NewGuid().ToString(), js, true);
            if (log.IsDebugEnabled) {
                log.Debug("registerToastrMsg Ends");
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public WebPageBase()
            : base() {
            this.log = LogManager.GetLogger(this.GetType());
        }
    }
}
