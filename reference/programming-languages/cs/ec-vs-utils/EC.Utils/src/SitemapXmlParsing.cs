/*==========================================================================*/
/* Source File:   SITEMAPXMLPARSING.CS                                      */
/* Description:   Xml Parsing for Sitemap files.                            */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Jul.18/2013                                               */
/* Last Modified: Aug.13/2013                                               */
/* Version:       1.3                                                       */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Jul.18/2013 COQ File created.
============================================================================*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace EC.Utils {
    /// <summary>
    /// Xml Parsing for Sitemap files.
    /// </summary>
    public class SitemapXmlParsing {
        /// <summary>
        /// Complete path to file or a full URL where resource is located.
        /// </summary>
        /// 
        public string Source { get; set; }

        /// <summary>
        /// Names the resource name to inspect.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Holds the filter used (if any used).
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// Holds only the file name part when using a file path for source.
        /// </summary>
        public int SitemapType { get; set; }

        /// <summary>
        /// Reads Sitemap XML formed files or URLs and extracts the LOC tag.
        /// </summary>
        /// <param name="filter">If it is not empty then instructs to extract LOC tags given that given name</param>
        /// <returns>A string collection holding all of the Sitemap XML file LOC tag.</returns>
        public List<string> ParseSitemapFile() {
            List<string> urlList = new List<string>();
            XmlDocument rssXmlDoc = new XmlDocument();

            // Load the Sitemap file from the Sitemap URL
            rssXmlDoc.Load(Source);

            // Iterate through the top level nodes and find the "urlset" node. 
            foreach (XmlNode topNode in rssXmlDoc.ChildNodes) {
                if (topNode.Name.ToLower() == "urlset") {
                    // Use the Namespace Manager, so that we can fetch nodes using the namespace
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(rssXmlDoc.NameTable);
                    nsmgr.AddNamespace("ns", topNode.NamespaceURI);

                    // Get all URL nodes and iterate through it.
                    XmlNodeList urlNodes = topNode.ChildNodes;
                    foreach (XmlNode urlNode in urlNodes) {
                        // Get the "loc" node and retrieve the inner text.
                        XmlNode locNode = urlNode.SelectSingleNode("ns:loc", nsmgr);
                        string link = locNode != null ? locNode.InnerText : "";

                        if (Filter != "") {
                            if (link.Contains(Filter)) {
                                urlList.Add(link);
                            }
                        }
                        else {
                            urlList.Add(link);
                        }
                    }
                }
            }
            return urlList;
        }

        /// <summary>
        /// For every sitemap XML file, it is stored to the sitemap table onto the database.
        /// </summary>
        /// <param name="urlList">The list of sitemap XML files</param>
        public void SaveToDatabase(List<string> urlList) {
            if (urlList.Count > 0) {
                string sql = "insert into sitemap(url, urlHost, urlPath, urlParameters, source, filename, dateProcessed, sitemapType, filter) values(@url, @urlHost, @urlPath, @urlParameters, @source, @filename, @dateProcessed, @sitemapType, @filter)";
                string sqlDel = "delete from sitemap where filename = @filename";
                string sqlSel = "select count(1) cnt from sitemap with (index(ix_url)) where url = @url";
                HandleDatabase hdb = new HandleDatabase();
                SqlParameter param1 = new SqlParameter();
                param1.ParameterName = "@url";
                param1.Value = "";
                param1.SqlDbType = SqlDbType.VarChar;

                SqlParameter param2 = new SqlParameter();
                param2.ParameterName = "@urlHost";
                param2.Value = "";
                param2.SqlDbType = SqlDbType.VarChar;

                SqlParameter param3 = new SqlParameter();
                param3.ParameterName = "@urlPath";
                param3.Value = "";
                param3.SqlDbType = SqlDbType.VarChar;

                SqlParameter param4 = new SqlParameter();
                param4.ParameterName = "@urlParameters";
                param4.Value = "";
                param4.SqlDbType = SqlDbType.VarChar;

                SqlParameter param5 = new SqlParameter();
                param5.ParameterName = "@source";
                param5.Value = "";
                param5.SqlDbType = SqlDbType.VarChar;

                SqlParameter param6 = new SqlParameter();
                param6.ParameterName = "@filename";
                param6.Value = "";
                param6.SqlDbType = SqlDbType.VarChar;

                SqlParameter param7 = new SqlParameter();
                param7.ParameterName = "@dateProcessed";
                param7.Value = DateTime.Now;
                param7.SqlDbType = SqlDbType.DateTime;

                SqlParameter param8 = new SqlParameter();
                param8.ParameterName = "@sitemapType";
                param8.Value = 0;
                param8.SqlDbType = SqlDbType.Int;

                SqlParameter param9 = new SqlParameter();
                param9.ParameterName = "@filter";
                param9.Value = 0;
                param9.SqlDbType = SqlDbType.VarChar;

                hdb.Open();

                SqlTransaction transaction = hdb.BeginTransaction("sitemap");
                //SqlTransaction trnDelete = hdb.BeginTransaction("sitemapDelete");
                param6.Value = FileName;
                hdb.ExecSQLStmt(transaction, sqlDel, param6);
                //trnDelete.Commit();

                foreach (var url in urlList) {
                    Uri uri = new Uri(url);
                    param1.Value = url;
                    param2.Value = uri.Host;
                    param3.Value = uri.LocalPath;
                    param4.Value = uri.Query;
                    param5.Value = Source;
                    param6.Value = FileName;
                    param7.Value = DateTime.Now;
                    param8.Value = SitemapType;
                    param9.Value = Filter;

                    //SqlTransaction trnSel = hdb.BeginTransaction("sitemapSel");
                    int cnt = hdb.ExecuteSelectSQLStmtAsScalar(transaction, sqlSel, param1);
                    //trnSel.Commit();
                    if (cnt == 0) {
                        hdb.ExecSQLStmt(transaction, sql, param1, param2, param3, param4, param5, param6, param7, param8, param9);
                    }
                }
                transaction.Commit();
                hdb.Close();
            }
        }
    }
}
