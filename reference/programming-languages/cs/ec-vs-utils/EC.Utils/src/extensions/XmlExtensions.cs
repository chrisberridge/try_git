/*==========================================================================*/
/* Source File:   XMLDOCUMENTEXTENSIONS.CS                                  */
/* Description:   Project constants                                         */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Aug.13/2013                                               */
/* Last Modified: Aug.14/2013                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Aug.13/2013 COQ File created.
============================================================================*/

using System.Text;
using System.Xml;

namespace EC.Utils.Extensions {
    /// <summary>
    /// A collection of extensions for XmlDocument Class.
    /// </summary>
    public static class XmlDocumentExtensions {

        /// <summary>
        /// The inner XML contents in any XmlDocument object is formatted for human readable way.
        /// NOTE: Read this http://www.undermyhat.org/blog/2009/08/tip-force-utf8-or-other-encoding-for-xmlwriter-with-stringbuilder/
        /// A must. Because it says why XmlWriter using a StringBuilder uses UTF-16 by default and a way
        /// to circumvent it. Well, not done here.
        /// </summary>
        /// <param name="doc">XML document to format</param>
        /// <returns>A formatted XML version</returns>
        public static string Beautify(this XmlDocument doc) {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";
            settings.NewLineChars = "\r\n";
            settings.NewLineHandling = NewLineHandling.Replace;
            using (XmlWriter writer = XmlWriter.Create(sb, settings)) {
                doc.Save(writer);
            }
            return sb.ToString();
        }
    }
}
