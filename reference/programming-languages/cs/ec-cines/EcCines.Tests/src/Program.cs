/*==========================================================================*/
/* Source File:   PROGRAM.CS                                                */
/* Description:   Entry point to Tests for ECCines                          */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Apr.23/2015                                               */
/* Last Modified: Apr.24/2015                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Apr.23/2015 COQ File created.
============================================================================*/

using System;
using ELCOLOMBIANO.EcCines.Tests.Test;

// Configure log4net using the .config file
[assembly: log4net.Config.XmlConfigurator(Watch = true)]
// This will cause log4net to look for a configuration file
// called [APPName].exe.config in the application base
// directory (i.e. the directory containing .exe)
namespace ELCOLOMBIANO.EcCines.Tests {
    /// <summary>
    /// Entry point to Test for EcCines
    /// NOTE: This is very arcaic to make test. Must use a framework like NUNIT or one suitable for .NET
    /// </summary>
    class Program {
        static void Main(string[] args) {
            TestApp tst = new TestApp();
            
            //for (int i = 0; i < 20; i++) {
            //    Console.Write(i + " ");
            //    int rslt = tst.executeSaveScheduleTest();
            //    Console.WriteLine("With result set to " + rslt + " (=> 0:ok, -1: error)");
            //}

            Console.WriteLine(" Only one " + tst.executeSaveScheduleSingleTest() + " ");
            tst.executeLoadMovieFileAndLog();
        }
    }
}
