/*==========================================================================*/
/* Source File:   MIGRATIONABSTRACT.CS                                      */
/* Description:   Base class for pages, used to share code.                 */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Jul.09/2014                                               */
/* Last Modified: Jul.11/2014                                               */
/* Version:       1.2                                                       */
/* Copyright (c), 2014 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Jul.09/2014 COQ File created.
============================================================================*/
using System.Configuration;
using System.Web.UI;
using System.Diagnostics;

namespace EC.SE4Migrate.Web.Utils {
    /// <summary>
    /// Base class for pages, used to share code.
    /// </summary>
    public abstract class MigrationAbstract : Page {

        /// <summary>
        /// Path to launch exe.
        /// </summary>
        protected string _exeLaunchPath = "";

        /// <summary>
        /// Path to logs.
        /// </summary>
        protected string _logPath = "";

        /// <summary>
        /// Retrieve a value for supplied key in the APP.CONFIG file.
        /// </summary>
        /// <param name="key">Which key to use to fine a value.</param>
        /// <returns>Value of key</returns>
        protected string RetrieveAppSetting(string key) {
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// Launches the exe using cmd.
        /// </summary>
        /// <param name="cmd">Exe name and command line settings</param>
        protected void RunApp(string cmd) {
            var astr = cmd.Split(' ');
            ProcessStartInfo p = new ProcessStartInfo();
            p.WorkingDirectory = _exeLaunchPath;
            p.FileName = astr[0];
            p.Arguments = astr[1];
            p.WindowStyle = ProcessWindowStyle.Hidden;//ProcessWindowStyle.Hidden;

            Process.Start(p);
        }
    }
}