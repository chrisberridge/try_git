/*==========================================================================*/
/* Source File:   RECAPTCHATHEME.CS                                         */
/* Description:   Web Control for ASP.NET pages that uses Google Recaptcha  */
/*                API Version 1                                             */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Jan.14/2015                                               */
/* Last Modified: Jan.14/2015                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Jan.14/2015 COQ File created.
============================================================================*/


/* ============================================================================================================================
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
 * LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.
 * =========================================================================================================================== */

/* Taken from source at http://recaptchanet.codeplex.com/ starting at version 1.6 */

namespace EC.Recaptcha.Web
{
    /// <summary>
    /// Represents the theme of an ASP.NET Recaptcha control.
    /// </summary>
    public enum RecaptchaTheme
    {
        /// <summary>
        /// Red theme of the control.
        /// </summary>
        Red = 0,
        /// <summary>
        /// Blackglass theme of the control.
        /// </summary>
        Blackglass = 1,
        /// <summary>
        /// White theme of the control.
        /// </summary>
        White = 2,
        /// <summary>
        /// Clean theme of the control.
        /// </summary>
        Clean = 3
    }
}
