/*==========================================================================*/
/* Source File:   RECAPTCHAKEYHELPER.CS                                     */
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
    internal class RecaptchaKeyHelper
    {
        internal static string ParseKey(string key)
        {
            if (key.StartsWith("{") && key.EndsWith("}"))
            {
                return System.Configuration.ConfigurationManager.AppSettings[key.Trim().Substring(1, key.Length - 2)];
            }

            return key;
        }
    }
}
