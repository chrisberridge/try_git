/*==========================================================================*/
/* Source File:   DATAREADEREXTENSIONS.CS                                   */
/* Description:   A collection of extensions for IDataReader.               */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Jul.22/2014                                               */
/* Last Modified: Jul.25/2014                                               */
/* Version:       1.2                                                       */
/* Copyright (c), 2014 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Jul.22/2014 COQ File created.
============================================================================*/

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace EC.Utils.Extensions {
    /// <summary>
    /// A collection of extensions for DataReader.
    /// </summary>
    public static class DataReaderExtensions {
        /// <summary>
        /// Export DataReader to CSV (List<String>). Basic example that exports data to csv from a datareader. 
        /// Handle value if it contains the separator and/or double quotes but can be easily be expended to 
        /// include culture (date, etc...) , max errors, and more. 
        /// NOTE: This extension was taken from http://extensionmethod.net/csharp/list-string/datareader-to-csv
        /// As stated there it is written by Thierry Fierens on Jun.10/2013, thus, modifictions have been made so far.        
        /// </summary>
        /// <param name="dataReader">Data to export</param>
        /// <param name="includeHeaderAsFirstRow">Include field names in first row</param>
        /// <param name="separator">How to saparate fields</param>
        /// <returns></returns>
        public static List<String> ToCSV(this SqlDataReader dataReader, bool includeHeaderAsFirstRow, string separator) {
            List<String> csvRows = new List<string>();
            StringBuilder sb = null;

            if (includeHeaderAsFirstRow) {
                sb = new StringBuilder();
                for (int index = 0; index < dataReader.FieldCount; index++) {
                    if (dataReader.GetName(index) != null)
                        sb.Append(dataReader.GetName(index));

                    if (index < dataReader.FieldCount - 1)
                        sb.Append(separator);
                }
                csvRows.Add(sb.ToString());
            }

            while (dataReader.Read()) {
                sb = new StringBuilder();
                for (int index = 0; index < dataReader.FieldCount - 1; index++) {
                    if (!dataReader.IsDBNull(index)) {
                        string value = dataReader.GetValue(index).ToString();
                        if (dataReader.GetFieldType(index) == typeof(String)) {
                            //If double quotes are used in value, ensure each are replaced but 2.
                            if (value.IndexOf("\"") >= 0)
                                value = value.Replace("\"", "\"\"");

                            //If separtor are is in value, ensure it is put in double quotes.
                            if (value.IndexOf(separator) >= 0)
                                value = "\"" + value + "\"";
                        }
                        sb.Append(value);
                    }

                    if (index < dataReader.FieldCount - 1)
                        sb.Append(separator);
                }

                if (!dataReader.IsDBNull(dataReader.FieldCount - 1))
                    sb.Append(dataReader.GetValue(dataReader.FieldCount - 1).ToString().Replace(separator, " "));

                csvRows.Add(sb.ToString());
            }
            dataReader.Close();
            sb = null;
            return csvRows;
        }
    }
}
