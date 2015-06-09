/*==========================================================================*/
/* Source File:   TOONDIRINFO.CS                                            */
/* Description:   Control table for toon from directory process.            */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Jun.24/2014                                               */
/* Last Modified: Nov.20/2014                                               */
/* Version:       1.3                                                       */
/* Copyright (c), 2014 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Jun.24/2014 COQ File created.
============================================================================*/

using System;

namespace EC.Utils.Domain {
    /// <summary>
    /// Control structure for toons taken from directory file.
    /// </summary>
    public class ToonDirInfo {
        public string FromDir { get; set; }
        public string FileName { get; set; }
        public DateTime DateProcessed { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime ComputedCreatedDate { get; set; }
        public int UseComputedCreateDate { get; set; }

        /// <summary>
        /// Marks a record to be processed. 0: not processed, 1: processed.
        /// NOTE: Only records not processed are loaded into memory.
        /// By using this field we intend to run the application as many times as required
        /// to circumvent memory issues or hard drive space.
        /// </summary>
        public int Processed { get; set; }

        public string IdArticle { get; set; }
        public long Id { get; set; }
    }
}
