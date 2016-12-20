/*==========================================================================*/
/* Source File:   REGEXUTILITIES.ASPX.CS                                    */
/* Description:   Utilities about email validation and other duties using   */
/*                regular expressions.                                      */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Dec.20/2016                                               */
/* Last Modified: Dec.20/2016                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2016 CSoftZ                                               */
/*==========================================================================*/

/*===========================================================================
History
Dec.20/2016 CFG  File Created.
Dec.20/2016 COQ  The documentation for the rationale about this class is 
                 extracted from the following URL.
            https://msdn.microsoft.com/en-us/library/01escwtf(v=vs.110).aspx
============================================================================*/

using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CSoftZ.Common.Utils
{
    /// <summary>
    /// Utilities about email validation and other duties using  regular expressions. 
    /// The documentation for the rationale about this class is 
    /// extracted from the following URL.
    /// https://msdn.microsoft.com/en-us/library/01escwtf(v=vs.110).aspx
    /// </summary>
    public class RegexUtilities
    {
        // Used to validate the domain in a domain mapper method.
        private bool invalid = false;

        /// <summary>
        /// Evaluates the input data to conform to a valid email syntax.
        /// </summary>
        /// <param name="strIn">Data to validate</param>
        /// <returns>TRUE if it is a valid email.</returns>
        public bool IsValidEmail(string strIn)
        {
            invalid = false;
            if (String.IsNullOrEmpty(strIn))
                return false;

            // Use IdnMapping class to convert Unicode domain names.
            try
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }

            if (invalid)
                return false;

            // Return true if strIn is in valid e-mail format.
            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        ///  Use IdnMapping class to convert Unicode domain names.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        private string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                invalid = true;
            }
            return match.Groups[1].Value + domainName;
        }
    }
}
