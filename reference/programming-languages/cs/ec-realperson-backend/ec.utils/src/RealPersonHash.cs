/*==========================================================================*/
/* Source File:   REALPERSONHASH.CS                                         */
/* Description:   Site verification for Captcha solution                    */
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
namespace EC.Utils {
    /// <summary>
    /// Utility to map a real person for a web application.
    /// Refer to http://keith-wood.name/realPerson.html.
    /// </summary>
    public class RealPersonHash {
        /// <summary>
        /// Checksum to validate real person logic
        /// </summary>
        /// <param name="value">Test value</param>
        /// <returns>Converted hashed value for test value</returns>
        private string GetStringHash(string value) {
            int hash = 5381;
            value = value.ToUpper();
            for(int i = 0; i < value.Length; i++) {
                hash = ((hash << 5) + hash) + value[i];
            }
            return hash.ToString();
        }

        /// <summary>
        /// Given parameters compute if sent value via HTTP Post form is equal to hashedValue.
        /// </summary>
        /// <param name="value">Typed value for human</param>
        /// <param name="hashedValue">Hash value for what human must have typed</param>
        /// <returns>True if equal</returns>
        public Boolean IsReal(string value, string hashedValue) {
            if((value == null || value == "") && (hashedValue == null || hashedValue == "")) {
                return false;
            }
            string hs = GetStringHash(value);
            return (hs == hashedValue);
        }
    }
}
