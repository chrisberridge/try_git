/*==========================================================================*/
/* Source File:   REALPERSONRESPONSE.CS                                     */
/* Description:   Domain object to hold JSon Response for realperson.aspx   */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.14/2014                                               */
/* Last Modified: Feb.14/2014                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2014 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Feb.14/2014 COQ File created.
============================================================================*/

using System;
namespace EC.Utils.Domain {
    /// <summary>
    /// Domain object to hold JSon Response for realperson.aspx
    /// </summary>
    public class RealPersonResponse {
        public string msg { get; set; }
        public int val { get; set; }
    }
}
