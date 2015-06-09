/*==========================================================================*/
/* Source File:   CONFIG.ASP.CS                                             */
/* Description:   Configuration Page                                        */
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
using System.IO;
using System.Text;
using System.Web.UI;

namespace EC.SE4Migrate.Web {
    /// <summary>
    /// Configuration Page
    /// </summary>
    public partial class config : MigrationAbstract {
        /// <summary>
        /// Load page contents.
        /// </summary>
        /// <param name="sender">Object reference</param>
        /// <param name="e">Parameters to object reference</param>
        protected void Page_Load(object sender, EventArgs e) {
            _exeLaunchPath = RetrieveAppSetting("ExeLaunchPath");
            if (!Page.IsPostBack) {
                string file = _exeLaunchPath + "\\" + "cmdparams.properties";
                using (StreamReader st = new StreamReader(file)) {
                    txtConfigFile.Text = st.ReadToEnd().Replace("    ", "\t");
                }
            }
        }

        /// <summary>
        /// Event for button to Save file contents.
        /// </summary>
        /// <param name="s">Object reference</param>
        /// <param name="e">Parameter to object</param>
        protected void Save_Click(object s, EventArgs e) {
            string file = _exeLaunchPath + "\\" + "cmdparams.properties";

            using (StreamWriter st = new StreamWriter(file, false, Encoding.UTF8)) {
                st.Write(txtConfigFile.Text);
            }
        }
    }
}