/*==========================================================================*/
/* Source File:   STRINGEXTENSION.CS                                        */
/* Description:   A collection of extensions for String.                    */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Apr.01/2015                                               */
/* Last Modified: Apr.01/2015                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Apr.01/2015 COQ File created.
============================================================================*/
using System;

namespace ELCOLOMBIANO.EcCines.Common.Extensions {
    /// <summary>
    /// A collection of extensions for String. 
    /// </summary>
    public static class StringExtension {
        /// <summary>
        /// Given a string in the format DD/MM/YYYY (note, must be present). Extracts DD, MM, YYYY parts and
        /// creates a new DateTime object without the minutes part.
        /// </summary>
        /// <param name="ddMMYYYY">String to evaluate in DD/MM/YYYY</param>
        /// <returns>Date time representation</returns>
        public static DateTime DDMMYYYYToDateTime(this String ddMMYYYY) {
            var s = ddMMYYYY.Split('/');
            DateTime dt = new DateTime(int.Parse(s[2]), int.Parse(s[1]), int.Parse(s[0]));
            return dt;
        }
    }
}
