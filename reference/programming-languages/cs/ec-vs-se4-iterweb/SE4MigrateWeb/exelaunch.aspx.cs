/*==========================================================================*/
/* Source File:   EXELAUNCH.ASP.CS                                          */
/* Description:   EXE Launch Application Page                               */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Jul.09/2014                                               */
/* Last Modified: Jul.10/2014                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2014 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Jul.09/2014 COQ File created.
============================================================================*/
using EC.SE4Migrate.Web.Utils;
using System;
using System.Web.UI;

namespace EC.SE4Migrate.Web {
    /// <summary>
    /// EXE Launch Application Page 
    /// </summary>
    public partial class exelaunch : MigrationAbstract {
        /// <summary>
        /// Load page contents.
        /// </summary>
        /// <param name="sender">Object reference</param>
        /// <param name="e">Parameters to object reference</param>
        protected void Page_Load(object sender, EventArgs e) {
            _exeLaunchPath = RetrieveAppSetting("ExeLaunchPath");
            if (!Page.IsPostBack) {
                txtCommand.Text = "SE4ToIterWeb.exe cmdparams.properties";                
            }
        }

        /// <summary>
        /// Event for button to launch EXE.
        /// </summary>
        /// <param name="s">Object reference</param>
        /// <param name="e">Parameter to object</param>
        protected void ExeLaunch_Click(object s, EventArgs e) {
            lblInfo.Text = "Executing..." + txtCommand.Text;
            RunApp(txtCommand.Text);
        }
    }
}