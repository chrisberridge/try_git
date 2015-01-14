/*==========================================================================*/
/* Source File:   RECAPTCHAVERIFICATIONRESULT.CS                            */
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
    /// Represents the result value of recaptcha verification process.
    /// </summary>
    public enum RecaptchaVerificationResult
    {
        /// <summary>
        /// Verification failed but the exact reason is not known.
        /// </summary>
        UnknownError = 0,
        /// <summary>
        /// Verification succeeded.
        /// </summary>
        Success = 1,
        /// <summary>
        /// The user's response to recaptcha challenge is incorrect.
        /// </summary>
        IncorrectCaptchaSolution = 2,
        /// <summary>
        /// The request parameters in the client-side cookie are invalid.
        /// </summary>
        InvalidCookieParameters = 3,
        /// <summary>
        /// The private supplied at the time of verification process is invalid.
        /// </summary>
        InvalidPrivateKey = 4,
        /// <summary>
        /// The user's response to the recaptcha challenge is null or empty.
        /// </summary>
        NullOrEmptyCaptchaSolution = 5,
        /// <summary>
        /// The recaptcha challenge could not be retrieved.
        /// </summary>
        ChallengeNotProvided = 6
    }
}
