/*==========================================================================*/
/* Source File:   PROGRAM.CS                                                */
/* Description:   Program to excercise RSS feeds.                           */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Mar.21/2014                                               */
/* Last Modified: Mar.25/2014                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2014 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Mar.21/2014 COQ File created.
============================================================================*/

using System;
using System.Xml;

namespace SyncRssBlogs {
    public class Program {
        static void Main(string[] args) {
            DateTime dtStartGlobal, dtEndGlobal;
            string s = "RSS Blogs Sync (c) EL Colombiano, 2014, V1.0.0.00 Mar.21/2014";

            dtStartGlobal = DateTime.Now;
            Console.WriteLine(s);

            s = "Start Time: " + dtStartGlobal;
            Console.WriteLine(s);

            new SyncRssBlog().Synchronize();
            dtEndGlobal = DateTime.Now;

            s = "End Time: " + dtEndGlobal;
            Console.WriteLine(s);
            s = "Elapsed time: " + (dtEndGlobal - dtStartGlobal);
            Console.WriteLine();
            Console.WriteLine(s);
            Console.ReadLine();
        }
    }
}
