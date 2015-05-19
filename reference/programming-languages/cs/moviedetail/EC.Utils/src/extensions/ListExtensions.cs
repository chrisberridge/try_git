/*==========================================================================*/
/* Source File:   LISTEXTENSIONS.CS                                         */
/* Description:   A collection of extensions for List.                      */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Jul.24/2014                                               */
/* Last Modified: Jul.25/2014                                               */
/* Version:       1.2                                                       */
/* Copyright (c), 2014 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Jul.24/2014 COQ File created.
============================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;

namespace EC.Utils.Extensions {
    /// <summary>
    /// A collection of extensions for List.
    /// </summary>
    public static class ListExtensions {
        /// <summary>
        /// Convert a list of longs to a 'separator' delimited string.
        /// </summary>
        /// <param name="data">List of longs to use</param>
        /// <param name="separator">Delimiter to use</param>
        /// <returns>String with items separated by 'separator'</returns>
        public static String ToStringDelimited(this List<long> data, String separator) {           
            return string.Join(separator, data.Select(i => i.ToString()).ToArray());
        }
    }
}
