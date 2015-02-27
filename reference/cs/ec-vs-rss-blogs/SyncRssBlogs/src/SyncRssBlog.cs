/*==========================================================================*/
/* Source File:   SYNCRSSBLOG.CS                                            */
/* Description:   System RSS Blog Syncrhonizer.                             */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Mar.21/2014                                               */
/* Last Modified: Mar.25/2014                                               */
/* Version:       1.3                                                       */
/* Copyright (c), 2014 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Mar.21/2014 COQ File created.
============================================================================*/

using EC.Rss.Utils;
using EC.Rss.Utils.Constants;
using EC.Rss.Utils.Db;
using EC.Rss.Utils.Domain;
using EC.Rss.Utils.Domain.Type;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SyncRssBlogs {
    /// <summary>
    ///  System RSS Blog Syncrhonizer.
    /// </summary>
    public class SyncRssBlog {
        private string _connection;

        /// <summary>
        /// Retrieve a value for supplied key in the APP.CONFIG file.
        /// </summary>
        /// <param name="key">Which key to use to fine a value.</param>
        /// <returns></returns>
        private string RetrieveAppSetting(string key) {
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// Retrieves all the RSS urls to parse from database store.
        /// </summary>
        /// <returns>List of Urls to match</returns>
        private List<RssUrlInfo> LoadRSSUrls() {
            RssUrlInfo info = null;
            List<RssUrlInfo> lst = new List<RssUrlInfo>();
            HandleDatabase hdb = new HandleDatabase(_connection);
            string sqlToUse =
                "select orden, codObjetoSe, texto AS rss " +
                "from objetosecontenido ,objetose " +
                "where id_objetose = codobjetose AND nomobjetose = 'EC-Bloguer' AND atributo = 'rutaRssBlog' " +
                "order by orden ";
            hdb.Open();
            SqlTransaction transaction = hdb.BeginTransaction("loadRssUrls");
            SqlDataReader rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sqlToUse);
            while (rdr.Read()) {
                info = new RssUrlInfo();
                string url = rdr["rss"].ToString();
                if (url != null && url != "") {
                    info.Url = url;
                    info.Order = Convert.ToInt32(rdr["orden"].ToString());
                    info.ObjectSECode = Convert.ToInt32(rdr["codObjetoSe"].ToString());
                    lst.Add(info);
                }
            }
            rdr.Close();
            transaction.Commit();
            hdb.Close();
            return lst;
        }

        /// <summary>
        /// Puts in database all of collected information about RSS feeds.
        /// </summary>
        /// <param name="items">Information to process</param>
        private void SaveToDb(List<RssUrlInfo> items) {            
            string sqlIns = "INSERT INTO publicacionBlog (orden, codobjetose, rutaRSS, titulo, link, linkcomentario, fechapublica, creador, descripcion, totalcomentarios, UltProcesado) VALUES  " +
                             "(@orden, @objectsecode, @url, @titulo, @link, @linkcomentario, @fechapublica, @creador, @descripcion, @totalcomentarios, @ultprocesado)";
            string sqlUpd = "update publicacionBlog " +
                            "set orden = @orden, " +
                            "    titulo = @titulo, " +
                            "    link = @link," + 
                            "    linkcomentario = @linkcomentario, " +
                            "    fechapublica = @fechapublica, " +
                            "    creador = @creador, " +
                            "    descripcion = @descripcion, " +
                            "    totalcomentarios = @totalcomentarios, " +
                            "    UltProcesado = @ultprocesado " +
                            "where rutarss = @url and codobjetose = @objectsecode";
            string sqlSel = "select  count(*) as exist from publicacionblog where rutarss = @url and codobjetose = @objectsecode";
            HandleDatabase hdb = new HandleDatabase(_connection);

            hdb.Open();
            SqlTransaction transaction = hdb.BeginTransaction("savetodb");
            SqlParameter param1 = new SqlParameter();
            SqlParameter param2 = new SqlParameter();
            SqlParameter param3 = new SqlParameter();
            SqlParameter param4 = new SqlParameter();
            SqlParameter param5 = new SqlParameter();
            SqlParameter param6 = new SqlParameter();
            SqlParameter param7 = new SqlParameter();
            SqlParameter param8 = new SqlParameter();
            SqlParameter param9 = new SqlParameter();
            SqlParameter param10 = new SqlParameter();
            SqlParameter param11 = new SqlParameter();
            try {
                foreach (var item in items) {
                    string s = "";
                    if (item.DocItemList == null) {
                        Console.WriteLine("RSS Not generated for [" + item.Url + "]");
                        continue;
                    }

                    // Let's find out if record already exists.
                    param1.ParameterName = "@url";
                    param1.Value = item.Url;
                    param1.SqlDbType = SqlDbType.VarChar;

                    param2.ParameterName = "@objectsecode";
                    param2.Value = item.ObjectSECode;
                    param2.SqlDbType = SqlDbType.Int;

                    param3.ParameterName = "@orden";
                    param3.Value = item.Order;
                    param3.SqlDbType = SqlDbType.Int;

                    param4.ParameterName = "@titulo";
                    s = item.DocItemList[0].Title;
                    if (s.Length > 253) {
                        s = s.Substring(1, 253);
                    }
                    param4.Value = s;
                    param4.SqlDbType = SqlDbType.VarChar;

                    param5.ParameterName = "@link";
                    param5.Value = item.DocItemList[0].Link;
                    param5.SqlDbType = SqlDbType.VarChar;

                    param6.ParameterName = "@linkcomentario";
                    param6.Value = item.DocItemList[0].LinkComments;
                    param6.SqlDbType = SqlDbType.VarChar;

                    param7.ParameterName = "@fechapublica";
                    param7.Value = item.DocItemList[0].PublishDate;
                    param7.SqlDbType = SqlDbType.DateTime;

                    param8.ParameterName = "@creador";
                    param8.Value = item.DocItemList[0].Creator;
                    param8.SqlDbType = SqlDbType.VarChar;

                    param9.ParameterName = "@descripcion";
                    s = item.DocItemList[0].Content;
                    if (s.Length > 498) {
                        s = s.Substring(1, 498);
                    }
                    param9.Value = s;
                    param9.SqlDbType = SqlDbType.VarChar;

                    param10.ParameterName = "@totalComentarios";
                    param10.Value = item.DocItemList[0].NumComments;
                    param10.SqlDbType = SqlDbType.Int;

                    param11.ParameterName = "@UltProcesado";
                    param11.Value = DateTime.Now;
                    param11.SqlDbType = SqlDbType.DateTime;

                    int cnt = hdb.ExecuteSelectSQLStmtAsScalar(transaction, sqlSel, param1, param2);
                    if (cnt == 0) {
                        // Create new record
                        hdb.ExecSQLStmt(transaction, sqlIns, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11);
                    }
                    else {
                        // Update existing record
                        hdb.ExecSQLStmt(transaction, sqlUpd, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11);
                    }
                }
            }
            catch (Exception) {
                transaction.Rollback();
                throw;
            }
            finally {
                transaction.Commit();
            }
            hdb.Close();
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SyncRssBlog() {
            _connection = RetrieveAppSetting(GlobalConstants.ConnectionKey);
        }

        /// <summary>
        /// Loads RSS for Blogs and sync to DB.
        /// </summary>
        public void Synchronize() {
            Console.WriteLine("Loading URls...");
            List<RssUrlInfo> urlInfoList = LoadRSSUrls();
            FeedParser parser = new FeedParser();
            foreach (var urlInfo in urlInfoList) {
                var items = parser.Parse(urlInfo.Url, RssFeedType.RSS);
                RssItem rssIt = null;

                // Here for sure there is one object in 'items'.
                if (items.Count > 0) {
                    List<RssItem> docItemList = new List<RssItem>();
                    rssIt = items[0];
                    docItemList.Add(rssIt);
                    urlInfo.DocItemList = docItemList;
                }
            }
            Console.WriteLine("Saving to DB...");
            SaveToDb(urlInfoList);
        }
    }
}
