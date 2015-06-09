/*==========================================================================*/
/* Source File:   DATETIMEEXTENSION.CS                                      */
/* Description:   Extension method to the DATETIME class                    */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Mar.31/2015                                               */
/* Last Modified: Apr.01/2015                                               */
/* Version:       1.2                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Mar.31/2015 COQ File created.
============================================================================*/
using System;

namespace ELCOLOMBIANO.EcCines.Common.Extensions {
    /// <summary>
    /// Extension method to the DATETIME class 
    /// </summary>
    public static class DateTimeExtension {
        /// <summary>
        /// Maps a date time in terms of Colombia time zone.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTime MapToLocalTimeColombia(this DateTime dt) {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            DateTime time = DateTime.UtcNow;
            DateTime convertedTime = time;

            if (time.Kind == DateTimeKind.Local && !timeZone.Equals(TimeZoneInfo.Local))
                convertedTime = TimeZoneInfo.ConvertTime(time, TimeZoneInfo.Local, timeZone);
            else if (time.Kind == DateTimeKind.Utc && !timeZone.Equals(TimeZoneInfo.Utc))
                convertedTime = TimeZoneInfo.ConvertTime(time, TimeZoneInfo.Utc, timeZone);
            return convertedTime;
        }

        /// <summary>
        /// Maps a date time in terms of Colombia time zone but discards time part.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTime MapToLocalTimeColombiaTakeDatePart(this DateTime dt) {
            DateTime dtNowColombia = DateTime.Now.MapToLocalTimeColombia();
            return new DateTime(dtNowColombia.Year, dtNowColombia.Month, dtNowColombia.Day);
        }
    }
}
