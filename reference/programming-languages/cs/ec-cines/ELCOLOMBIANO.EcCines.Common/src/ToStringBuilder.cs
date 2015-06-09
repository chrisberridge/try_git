/*==========================================================================*/
/* Source File:   TOSTRINGBUILDER.CS                                        */
/* Description:   Prints all property values for object instance            */
/*                Inspired by Apache Commons Lang class ToStringBuilder     */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Apr.24/2015                                               */
/* Last Modified: Apr.24/2015                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Apr.24/2015 COQ File created.
============================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace ELCOLOMBIANO.EcCines.Common {
    /// <summary>
    /// Prints all property values for object instance         
    /// Inspired by Apache Commons Lang class ToStringBuilder  
    /// </summary>
    public static class ToStringBuilder {
        /// <summary>
        /// Given instance object it formats a string with class name and property/value.
        /// </summary>
        /// <remarks>DEPRECATED because it needs to be better implemented. It </remarks>
        /// <param name="o">Object to convert to string</param>
        /// <returns>A string with class name and property/value.</returns>
        public static String reflectionToStringDeprecated(Object o) {
            List<String> propertyValueList = new List<String>();
            PropertyInfo[] propertyInfos;
            propertyInfos = o.GetType().GetProperties();

            StringBuilder sb = new StringBuilder();
            sb.Append(o.GetType().Name);
            sb.Append("[");
            // write property names
            foreach (PropertyInfo propertyInfo in propertyInfos) {
                var s = propertyInfo.Name + "=" + propertyInfo.GetValue(o, null);
                propertyValueList.Add(s);
            }
            sb.Append(String.Join(",", propertyValueList.Select(x => x.ToString()).ToArray()));
            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// Given instance object it formats a string with class name and property/value.
        /// </summary>
        /// <remarks>Uses JSon.NET to serialize the object.</remarks>
        /// <param name="o">Object to convert to string</param>
        /// <returns>A string with class name and property/value.</returns>
        public static String reflectionToString(Object o) {
            return new StringBuilder().Append(o.GetType().Name).Append("[").
               Append(JsonConvert.SerializeObject(o)).
               Append("]").ToString();
        }
    }
}
