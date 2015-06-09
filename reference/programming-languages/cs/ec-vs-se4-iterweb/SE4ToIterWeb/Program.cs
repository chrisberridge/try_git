/*==========================================================================*/
/* Source File:   PROGRAM.CS                                                */
/* Description:   Main entry point                                          */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Jul.19/2013                                               */
/* Last Modified: Nov.24/2014                                               */
/* Version:       1.83                                                      */
/* Copyright (c), 2013,2014 Arkix, El Colombiano                            */
/*==========================================================================*/

/*===========================================================================
History
Jul.19/2013 COQ File created.
============================================================================*/

/*
 * NOTE: Use http://dotnetzip.codeplex.com/ for zip purposes.
 * 
 * This project references two nugget packages in order to work.
 * - DotNetZip V1.9.1.8
 * - log4net V2.0.0
 * - HtmlAgilityPack V1.4.6
 * - Newtonsoft.Json V5.0.6
 */
using EC.IterwebMigrate;
using EC.Utils;
using EC.Utils.Extensions;
using log4net;
using System;
using System.Reflection;

// Configure log4net using the .config file
[assembly: log4net.Config.XmlConfigurator(Watch = true)]
// This will cause log4net to look for a configuration file
// called [APPName].exe.config in the application base
// directory (i.e. the directory containing .exe)
namespace MigrateToIterweb {
    /// <summary>
    /// Main entry point.
    /// </summary>
    public class Program {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Program main entry point.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args) {
            string liner = "".PadLeft(900, '=');
            DateTime dtStartGlobal, dtEndGlobal;
            string s = "";

            s = "SE4 to Iterweb packager (c) EL Colombiano, 2013, 2014, V1.8.110.380 Nov.24/2014";
            Console.WriteLine(s);
            if (log.IsWarnEnabled) log.Warn(s);

            SE4DocMigrate se4Mig = null;
            string cmdParamsPropertiesPath = args[0];
            if (args.Length > 0) {
                se4Mig = new SE4DocMigrate(cmdParamsPropertiesPath);
            }
            else {
                se4Mig = new SE4DocMigrate();
            }
            dtStartGlobal = DateTime.Now;

            s = "Start Time: " + dtStartGlobal;
            Console.WriteLine(s);
            if (log.IsWarnEnabled) log.Warn(s);
            s = "Evaluating command line parameter overrides from file [" + cmdParamsPropertiesPath + "]";
            Console.WriteLine(s);
            if (log.IsWarnEnabled) log.Warn(s);
            if (se4Mig.IsOldDocScanSet()) {
                se4Mig.OldDocScan();
                Console.WriteLine();
            }
              
            try {
                se4Mig.Execute();      
            }
            catch (Exception e) {
                Console.WriteLine("Exception caught. See log for details");
                if (log.IsFatalEnabled) {
                    log.Fatal(e.Message);
                    log.Fatal(e.StackTrace);
                }
            }
            se4Mig.SaveCounters();
            dtEndGlobal = DateTime.Now;

            s = "End Time: " + dtEndGlobal;
            Console.WriteLine(s);
            if (log.IsWarnEnabled) log.Warn(s);
            s = "Elapsed time: " + (dtEndGlobal - dtStartGlobal);
            Console.WriteLine();
            Console.WriteLine(s);
            if (log.IsWarnEnabled) log.Warn(s);
            if (log.IsWarnEnabled) log.Warn(liner);
        }
    }
}
