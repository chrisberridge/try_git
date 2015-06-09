/*==========================================================================*/
/* Source File:   UTIL.CS                                                   */
/* Description:   Helper class with utility methods.                        */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: Apr.17/2015                                               */
/* Version:       1.7                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
============================================================================*/

namespace ELCOLOMBIANO.EcCines.Common {
    /// <summary>
    /// Defines the types of messages that can be shown
    /// </summary>
    public enum MessageType {
        Info,
        Error,
        Warning,
        Success
    }

    /// <summary>
    /// Common util class.
    /// </summary>
    public static class Utils {
        /// <summary>
        /// Uses toastr (javascript library) to show a message on page load.
        /// </summary>
        /// <param name="t">Message type to use</param>
        /// <param name="msg">message to show</param>
        public static string showToastrMsg(MessageType t, string msg) {
            string rslt = "";
            switch (t) {
                case MessageType.Info:
                    rslt = string.Format("toastr.info('{0}', 'Información');", msg);
                    break;
                case MessageType.Error:
                    rslt = string.Format("toastr.error('{0}', 'Error');", msg);
                    break;
                case MessageType.Warning:
                    rslt = string.Format("toastr.warning('{0}', 'Advertencia');", msg);
                    break;
                case MessageType.Success:
                    rslt = string.Format("toastr.success('{0}');", msg);
                    break;
                default:
                    break;
            }
            return rslt;
        }

        /// <summary>
        /// Given a name in English converts to Spanish
        /// </summary>
        /// <param name="en">Day name in English</param>
        /// <returns>Spanish name for English name.</returns>
        public static string getDayNameSpanish(string en) {
            string rslt = null;
            switch (en) {
                case "Monday":
                    rslt = "Lunes";
                    break;
                case "Tuesday":
                    rslt = "Martes";
                    break;
                case "Wednesday":
                    rslt = "Miércoles";
                    break;
                case "Thursday":
                    rslt = "Jueves";
                    break;
                case "Friday":
                    rslt = "Viernes";
                    break;
                case "Saturday":
                    rslt = "Sábado";
                    break;
                case "Sunday":
                    rslt = "Domingo";
                    break;
                default:
                    rslt = en;
                    break;
            }
            return rslt;
        }

        /// <summary>
        /// Given day name in English translates to a numeric number for that day.
        /// </summary>
        /// <param name="en">Day name in English</param>
        /// <returns></returns>
        public static int getDayNameNumber(string en) {
            int rslt = 0;
            switch (en) {
                case "Monday":
                    rslt = 1;
                    break;
                case "Tuesday":
                    rslt = 2;
                    break;
                case "Wednesday":
                    rslt = 3;
                    break;
                case "Thursday":
                    rslt = 4;
                    break;
                case "Friday":
                    rslt = 5;
                    break;
                case "Saturday":
                    rslt = 6;
                    break;
                case "Sunday":
                    rslt = 7;
                    break;
            }
            return rslt;
        }

        /// <summary>
        /// Given parameter returns in the Month name in Spanish.
        /// </summary>
        /// <param name="p">Month count</param>
        /// <returns>String with Month name.</returns>
        public static string monthToName(int p) {
            string s = "";
            switch (p) {
                case 1:
                    s = "enero";
                    break;
                case 2:
                    s = "febrero";
                    break;
                case 3:
                    s = "marzo";
                    break;
                case 4:
                    s = "abril";
                    break;
                case 5:
                    s = "mayo";
                    break;
                case 6:
                    s = "junio";
                    break;
                case 7:
                    s = "julio";
                    break;
                case 8:
                    s = "agosto";
                    break;
                case 9:
                    s = "septiembre";
                    break;
                case 10:
                    s = "octubre";
                    break;
                case 11:
                    s = "noviembre";
                    break;
                case 12:
                    s = "diciembre";
                    break;
                default:
                    break;
            }
            return s;
        }
    }
}
