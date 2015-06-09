/*==========================================================================*/
/* Source File:   LISTEXTENSION.CS                                          */
/* Description:   A collection of extensions for List.                      */
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
using System.Collections.Generic;
using System.Linq;

namespace ELCOLOMBIANO.EcCines.Common.Extensions {

    /// <summary>
    /// A collection of extensions for List.
    /// </summary>
    public static class ListExtensions {
        /// <summary>
        /// Convert a list of LONGS to a 'separator' delimited string.
        /// </summary>
        /// <param name="data">List of longs to use</param>
        /// <param name="separator">Delimiter to use</param>
        /// <returns>String with items separated by 'separator'</returns>
        public static String ToStringDelimited(this List<long> data, String separator) {
            return string.Join(separator, data.Select(i => i.ToString()).ToArray());
        }

        /// <summary>
        /// Convert a list of STRINGS to a 'separator' delimited string.
        /// </summary>
        /// <param name="data">List of longs to use</param>
        /// <param name="separator">Delimiter to use</param>
        /// <returns>String with items separated by 'separator'</returns>
        public static String ToStringDelimited(this List<string> data, String separator) {
            return string.Join(separator, data.Select(i => i.ToString()).ToArray());
        }
    }
}
