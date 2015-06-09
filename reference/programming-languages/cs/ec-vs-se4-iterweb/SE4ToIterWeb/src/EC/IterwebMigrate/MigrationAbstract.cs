/*==========================================================================*/
/* Source File:   MIGRATIONABSTRACT.CS                                      */
/* Description:   Abstract class to hold common methods for migration       */
/*                purposes.                                                 */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Aug.13/2013                                               */
/* Last Modified: Nov.10/2014                                               */
/* Version:       1.40                                                      */
/* Copyright (c), 2013, 2014 Arkix, El Colombiano                           */
/*==========================================================================*/

/*===========================================================================
History
Aug.13/2013 COQ File created.
============================================================================*/

using EC.Utils;
using EC.Utils.Constants;
using EC.Utils.Domain;
using EC.Utils.Extensions;
using Ionic.Zip;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace EC.IterwebMigrate {
    /// <summary>
    /// Abstract class to hold common methods for migration purposes.
    /// </summary>
    public abstract class MigrationAbstract {
        protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// True if Old Document list is already in memory.
        /// NOTE: This is only handled in method 'LoadOldDocuments()'
        /// </summary>
        protected bool _isOldDocsLoaded = false;

        /// <summary>
        /// Location to save generated Iterweb manifest files uncompressed. 
        /// </summary>
        protected string _migrationStoreFolder = "";

        /// <summary>
        /// Location to save compressed files in ZIP format.
        /// </summary>
        protected string _zipFolder = "";

        /// <summary>
        /// Points to the SE4 media folder
        /// </summary>
        protected string _se4MediaSourceFolder = "";

        /// <summary>
        /// Points to the SE4 media image gallery folder.
        /// </summary>
        protected string _se4MediaImageGallerySourceFolder = "";

        /// <summary>
        /// Points to the SE4 document folder
        /// </summary>
        protected string _se4DocSourceFolder = "";

        /// <summary>
        /// Microsoft SQL Server Connection string for ADO.NET 
        /// </summary>
        protected string _connStr = "";

        /// <summary>
        /// Microsoft SQL Server Connection string for ADO.NET (Points to database which holds 'sitemap' table.
        /// </summary>
        protected string _sitemapConnStr = "";

        /// <summary>
        /// Used to filter a top record quantity out of the SQL statement 
        /// executed to retrieve the SE4 documents to process. If it is zero,
        /// it means that no top record matching is required.
        /// </summary>
        protected int _limitTo = 0;

        /// <summary>
        /// Used to filter a top record quantity out of the SQL statement
        /// executed to retrieve the Old Documents to process. If it is zero,
        /// it means that no top record matching is required.
        /// </summary>
        protected int _useOldDocLimitTo = 0;

        /// <summary>
        /// Used to limit the number of documents stored in a package (zip file).
        /// </summary>
        protected int _docLimit = 0;

        /// <summary>
        /// Determines the list of layouts for SE4 to filter when retrieving
        /// documents to process. They must be comma separated if more than one 
        /// is required.
        /// </summary>
        protected string _se4LayoutToFilter = "";

        /// <summary>
        /// Used to filter old documents criteria. It is mutual exclusive to _se4LayoutToFilter
        /// </summary>
        protected string _se4DocFilter = "";

        /// <summary>
        /// Used to filter old documents criteria
        /// </summary>
        protected string _oldDocFilter = "";

        /// <summary>
        /// Target layout to be used in the Iterweb system
        /// </summary>
        protected string _iterWebLayoutName = "";

        /// <summary>
        // The following option controls the document processing with the following values  
        // 0: Process everything.
        // 1: Process only SE4 documents
        // 2: Process only Old documents
        // 3: Process only toons from files (namely from 'Se4MediaSourceFolder' at 'caricaturas2'
        // 4: No processing at all.
        // 5: Do a Database sync by calling sp 'sp_syncmigrationdb'
        // 6: Execute SQLExecStmt command
        // 7: Validate non-ascii characters in URL
        // 8: Package Commentary user.
        // 9: Print Version
        // 10: Compute URLTitle
        // 11: Update Create Date Old Docs Only
        // 12: SQL to CSV file generation using 'SQLExecStmt'.
        // 13: SQL Batch Execute. NOTE: Reads sql statements using Key 'SQLBatchFile' 
        // 14: SQL to CSV file generation using key 'SQLBatchFile'
        // 15: Analyze Old Doc Content Size
        // 16: Analyze SE4 Doc Content Size
        // 17: Fill [DocBrightCove] table indicating which are video and notes with video.
        /// </summary>
        protected int _docProcessingMode = -1;

        /// <summary>
        /// Determines if you need the XML output as it is or indented for human reading.
        /// Holds the 'yes' or 'no' values.
        /// </summary>
        protected string _xmlBeautify = "";

        /// <summary>
        /// Used when you may not wish to generate toons altogether.
        /// 0: false, 1: true.
        /// </summary>
        protected int _byPassToonCreation = 0;

        /// <summary>
        /// Names the global Liferay Site name or the portal site.
        /// </summary>
        protected string _globalSiteName = "";

        /// <summary>
        /// Manifest file default image width, used for default image cropping
        /// </summary>
        protected string _defaultImageMaxWidth = "";

        /// <summary>
        /// Manifest file default image height, used for default image cropping
        /// </summary>
        protected string _defaultImageMaxHeight = "";

        /// <summary>
        ///  Title constraints to up to this option.
        /// </summary>
        protected int _titleLimitTo = 0;

        /// <summary>
        /// Position in page to be set up.
        /// </summary>
        protected string _qualification = "";

        /// <summary>
        /// Names the template or disposition to use for a page.
        /// </summary>
        protected string _layout = "";

        /// <summary>
        /// Used to reset counters for package generation.
        /// </summary>
        protected int _resetCounters = 0;

        /// <summary>
        /// File to save the counters data. It is a JSON file.
        /// </summary>
        protected string _counterJSONFile = "";

        /// <summary>
        /// Use 0 to not update the create date for document, 1 to update.
        /// </summary>
        protected int _updateCreateDate = 0;

        /// <summary>
        /// When set to 1, the program scans a group of Old Documents where OldDocStatus is NULL
        /// to infer if they point URLs not already stored in Sitemap table.
        /// </summary>
        protected int _oldDocNewURlScan = 0;

        /// <summary>
        /// Snapshot List taken from  Sitemap table.
        /// </summary>
        protected List<IterwebMapInfo> _se4DocList = null;

        /// <summary>
        /// Snapshot list to process Toon documents.
        /// </summary>
        protected List<ToonDirInfo> _toonList = null;

        /// <summary>
        /// The contents of this list consists of KnownDomain001... App Settings keys.
        /// </summary>
        protected List<String> _knownDomainList = null;

        /// <summary>
        /// The contents of this list consists of ReplaceKnownDomain001... App Settings keys.
        /// </summary>
        protected List<String> _replaceKnownDomainList = null;

        /// <summary>
        /// There are two XML formats possible to generate.
        /// When 1 then uses long version (the one which uses <iter><list><pool><item></item></pool></list></iter>) format.
        /// When 2 then uses short version (the one which uses <articles><article></article></articles> format.
        /// </summary>
        protected int _outputStructure = 0;

        /// <summary>
        /// Indicates if a full JSon files are to be created again.
        /// 1: true, 0:false
        /// </summary>
        protected int _resetJSonFiles = 0;

        /// <summary>
        /// Path to Sections JSON file.
        /// </summary>
        protected String _sectionsJSONFile;

        /// <summary>
        /// Path to Vocabularies JSON file.
        /// </summary>
        protected String _vocabulariesJSONFile;

        /// <summary>
        /// When set to 1 (true) it processes '_dateStart' and '_dateEnd'.
        /// </summary>
        protected int _useDateFilter = 0;

        /// <summary>
        /// Date filtering to process using creation date.
        /// Start part. Format must be yyyymmdd.
        /// </summary>
        protected string _dateStartFilter = "0";

        /// <summary>
        /// Date filtering to process using creation date.
        /// End part. Format must be yyyymmdd.
        /// If it is defined with '-1', it indicates, then, to use today date and start date must be below.
        /// If it is not like so then start date is normalized to Jan.01 of current year.
        /// </summary>
        protected string _dateEndFilter = "-1";

        /// <summary>
        /// Special SQL to execute against index database. NOTE: Can only be execute if it is not empty and using a command value of 6
        /// </summary>
        protected string _sqlExecStmt = "";

        /// <summary>
        /// Points to a file name which holds one instance of latest execution of an SQL exported as CSV.
        /// </summary>
        protected string _csvExportFile = "";

        /// <summary>
        /// This file contains all SQL command statements to execute. NOTE: One SQL statement per line only and take care
        /// that file is UTF-8 encoded.
        /// </summary>
        protected string _sqlBatchFile = "";

        /// <summary>
        /// Default Constructor
        /// </summary>
        public MigrationAbstract() {
            _se4DocList = new List<IterwebMapInfo>();
            _toonList = new List<ToonDirInfo>();

            _migrationStoreFolder = RetrieveAppSetting(GlobalConstants.StoreFolderKey);
            _connStr = RetrieveAppSetting(GlobalConstants.ConnectionKey);
            _sitemapConnStr = RetrieveAppSetting(GlobalConstants.SitemapConnectionKey);
            _limitTo = Convert.ToInt32(RetrieveAppSetting(GlobalConstants.LimitToKey));
            _useOldDocLimitTo = Convert.ToInt32(RetrieveAppSetting(GlobalConstants.UseOldDocLimitToKey));
            _docLimit = Convert.ToInt32(RetrieveAppSetting(GlobalConstants.DocLimitKey));
            _se4LayoutToFilter = RetrieveAppSetting(GlobalConstants.SE4LayoutToFilterKey);
            _iterWebLayoutName = RetrieveAppSetting(GlobalConstants.IterWebLayoutNameKey);
            _xmlBeautify = RetrieveAppSetting(GlobalConstants.XmlBeautifyKey).ToLower();
            _zipFolder = RetrieveAppSetting(GlobalConstants.ZipFolderKey);
            _se4MediaSourceFolder = RetrieveAppSetting(GlobalConstants.Se4MediaSourceFolderKey);
            _se4MediaImageGallerySourceFolder = RetrieveAppSetting(GlobalConstants.Se4MediaImageGallerySourceFolderKey);
            _se4DocSourceFolder = RetrieveAppSetting(GlobalConstants.Se4DocSourceFolderKey);
            _oldDocFilter = RetrieveAppSetting(GlobalConstants.OldDocFilterKey);
            _se4DocFilter = RetrieveAppSetting(GlobalConstants.SE4DocFilterKey);
            _docProcessingMode = Convert.ToInt32(RetrieveAppSetting(GlobalConstants.DocumentProcessingModeKey));
            _updateCreateDate = Convert.ToInt32(RetrieveAppSetting(GlobalConstants.UpdateCreateDateKey));
            _oldDocNewURlScan = Convert.ToInt32(RetrieveAppSetting(GlobalConstants.OldDocNewURlScanKey));

            // Manifest file keys
            _globalSiteName = RetrieveAppSetting(GlobalConstants.GlobalSiteNameKey);
            _defaultImageMaxWidth = RetrieveAppSetting(GlobalConstants.DefaultImageMaxWidthKey);
            _defaultImageMaxHeight = RetrieveAppSetting(GlobalConstants.DefaultImageMaxHeightKey);
            _titleLimitTo = Convert.ToInt32(RetrieveAppSetting(GlobalConstants.TitleLimitToKey));
            _qualification = RetrieveAppSetting(GlobalConstants.QualificationKey);
            _layout = RetrieveAppSetting(GlobalConstants.LayoutKey);
            _resetCounters = Convert.ToInt32(RetrieveAppSetting(GlobalConstants.ResetCountersKey));
            _counterJSONFile = RetrieveAppSetting(GlobalConstants.CounterJSONFilerKey);
            _outputStructure = Convert.ToInt32(RetrieveAppSetting(GlobalConstants.OutputStructureKey));
            _resetJSonFiles = Convert.ToInt32(RetrieveAppSetting(GlobalConstants.ResetJSonFilesKey));
            _sectionsJSONFile = RetrieveAppSetting(GlobalConstants.SectionsJSONFileKey);
            _vocabulariesJSONFile = RetrieveAppSetting(GlobalConstants.VocabulariesJSONFileKey);
            _useDateFilter = Convert.ToInt32(RetrieveAppSetting(GlobalConstants.UseDateFilterKey));
            _dateStartFilter = RetrieveAppSetting(GlobalConstants.DateStartFilterKey);
            _dateEndFilter = RetrieveAppSetting(GlobalConstants.DateEndFilterKey);
            _byPassToonCreation = Convert.ToInt32(RetrieveAppSetting(GlobalConstants.ByPassToonCreationKey));

            if (_useDateFilter == 1) {
                NormalizeDateFilterValues();
            }

            // Other settings setup.            
            _knownDomainList = new List<string>();
            _replaceKnownDomainList = new List<string>();

            _knownDomainList.Add(RetrieveAppSetting(GlobalConstants.KnownDomain001Key));
            _knownDomainList.Add(RetrieveAppSetting(GlobalConstants.KnownDomain002Key));
            _knownDomainList.Add(RetrieveAppSetting(GlobalConstants.KnownDomain003Key));
            _knownDomainList.Add(RetrieveAppSetting(GlobalConstants.KnownDomain004Key));
            _knownDomainList.Add(RetrieveAppSetting(GlobalConstants.KnownDomain005Key));

            _replaceKnownDomainList.Add(RetrieveAppSetting(GlobalConstants.ReplaceKnownDomain001Key));
            _replaceKnownDomainList.Add(RetrieveAppSetting(GlobalConstants.ReplaceKnownDomain002Key));
            _replaceKnownDomainList.Add(RetrieveAppSetting(GlobalConstants.ReplaceKnownDomain003Key));
            _replaceKnownDomainList.Add(RetrieveAppSetting(GlobalConstants.ReplaceKnownDomain004Key));
            _replaceKnownDomainList.Add(RetrieveAppSetting(GlobalConstants.ReplaceKnownDomain005Key));

            _sqlExecStmt = RetrieveAppSetting(GlobalConstants.SQLExecStmtKey);
            if (_sqlExecStmt != "") {
                _sqlExecStmt = _sqlExecStmt.Replace("eq", "=");
            }
            _csvExportFile = RetrieveAppSetting(GlobalConstants.CSVExportFileKey);
            _sqlBatchFile = RetrieveAppSetting(GlobalConstants.SQLBatchFileKey);
        }

        /// <summary>
        /// Analizes the start and end date parameters to discover if they are in valid range.
        /// If end date is defined with -1, it indicates, then, to use today date and start date must be below.
        /// If it is not like so then start date is normalized to Jan.01 of current year.
        /// </summary>
        private void NormalizeDateFilterValues() {
            if (_dateEndFilter == "-1") {
                DateTime dt = DateTime.Now;
                _dateEndFilter = dt.ToString("yyyyMMdd");
                DateTime dtStartCompare = DateTime.ParseExact(_dateStartFilter, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None);
                if (dtStartCompare.CompareTo(dt) > 0) {
                    DateTime dtStart = new DateTime(dt.Year, 1, 1);
                    _dateStartFilter = dtStart.ToString("yyyyMMdd");
                }
            }
        }

        /// <summary>
        /// Reads a file to override parameters for application.
        /// </summary>
        /// <param name="configFilename">Points to a properties file which holds some of the </param>
        protected void OverrideParameters(string configFilename) {
            if (log.IsDebugEnabled) {
                log.Debug("OverrideParameters start");
            }
            if (!File.Exists(configFilename)) {
                return;
            }
            if (log.IsWarnEnabled) {
                log.Warn("Dumping contents of file [" + configFilename + "] Start");
            }

            // Read the file.
            using (StreamReader configFile = new StreamReader(configFilename)) {
                while (configFile.Peek() >= 0) {
                    string line = configFile.ReadLine();

                    if (line != "") {
                        var data = line.Split('=');
                        var key = data[0];
                        var val = data[1];
                        switch (key) {
                            case "LimitTo":
                                _limitTo = Convert.ToInt32(val);
                                break;
                            case "UseOldDocLimitTo":
                                _useOldDocLimitTo = Convert.ToInt32(val);
                                break;
                            case "DocLimit":
                                _docLimit = Convert.ToInt32(val);
                                break;
                            case "UpdateCreateDate":
                                _updateCreateDate = Convert.ToInt32(val);
                                break;
                            case "OldDocFilter":
                                _oldDocFilter = val;
                                break;
                            case "SE4DocFilter":
                                _se4DocFilter = val;
                                break;
                            case "SE4LayoutToFilter":
                                _se4LayoutToFilter = val;
                                break;
                            case "DocumentProcessingMode":
                                _docProcessingMode = Convert.ToInt32(val);
                                break;
                            case "UseDateFilter":
                                _useDateFilter = Convert.ToInt32(val);
                                break;
                            case "DateStartFilter":
                                _dateStartFilter = val;
                                break;
                            case "DateEndFilter":
                                _dateEndFilter = val;
                                break;
                            case "ResetCounters":
                                _resetCounters = Convert.ToInt32(val);
                                break;
                            case "DefaultImageMaxWidth":
                                _defaultImageMaxWidth = val;
                                break;
                            case "DefaultImageMaxHeight":
                                _defaultImageMaxHeight = val;
                                break;
                            case "ByPassToonCreation":
                                _byPassToonCreation = Convert.ToInt32(val);
                                break;
                            case "SQLExecStmt":
                                _sqlExecStmt = val;
                                if (_sqlExecStmt != "") {
                                    _sqlExecStmt = _sqlExecStmt.Replace("eq", "=");
                                }
                                break;
                        }
                        if (log.IsWarnEnabled) {
                            var s = "Overriding property " + key + "=[" + val + "]";
                            log.Warn(s);
                        }
                    }
                }
            }
            if (log.IsWarnEnabled) {
                log.Warn("Dumping contents of file [" + configFilename + "] End");
            }
            if (_useDateFilter == 1) {
                NormalizeDateFilterValues();
            }

            if (log.IsDebugEnabled) {
                log.Debug("OverrideParameters end");
            }
        }

        /// <summary>
        /// Retrieve a value for supplied key in the APP.CONFIG file.
        /// </summary>
        /// <param name="key">Which key to use to fine a value.</param>
        /// <returns></returns>
        private string RetrieveAppSetting(string key) {
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// Execute a Query to the database to populate a list of documents to be processed.
        /// </summary>
        protected void LoadOldDocuments() {
            if (log.IsDebugEnabled) {
                log.Debug("LoadOldDocuments start");
            }
            if (_isOldDocsLoaded) {
                if (log.IsDebugEnabled) {
                    log.Debug("List already in memory");
                }
                return;
            }
            if (log.IsWarnEnabled) {
                log.Warn("Loading list in memory");
            }
            string dateFilter = "";
            string sql =
                "select @useTop@ " +
                "     iterwebtemplate, ObjetoSEName, url, urlPath, sitemapType, processed, idSitemap, " +
                "	 idSE4, idSE4ArticleId, layout, idOld, idObjetoSE, CreateDate as DisplayDate, UpdateDate, JsonContent, oldDocStatus, oldDocTemplateType, urlTitle " +
                "from sitemap " +
                "where idOld is not null and idSE4 is null and processed = 0 and url is not null  @datefilter@ @oldDocFilter@" +
                " order by createdate desc, idOld ";

            if (_oldDocFilter == "") {
                _oldDocFilter = " and oldDocStatus = 2 ";
            }

            string sqlToUse = sql.Replace("@oldDocFilter@", _oldDocFilter);
            sqlToUse = sqlToUse.Replace("eq", " = ");
            if (_useDateFilter == 1) {
                dateFilter += " and (createdate between convert(datetime, '" + _dateStartFilter + "') ";
                dateFilter += " and convert(datetime, '" + _dateEndFilter + "')) ";
            }
            sqlToUse = sqlToUse.Replace("@datefilter@", dateFilter);
            if (_useOldDocLimitTo == 0) {
                sqlToUse = sqlToUse.Replace("@useTop@", "");
            }
            else {
                sqlToUse = sqlToUse.Replace("@useTop@", "top " + _useOldDocLimitTo);
            }
            if (log.IsWarnEnabled) {
                log.Warn("Using sql=[" + sqlToUse + "]");
            }

            HandleDatabase hdb = new HandleDatabase(_sitemapConnStr);
            hdb.Open();

            SqlTransaction transaction = hdb.BeginTransaction("migratetoiterweb");
            SqlDataReader rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sqlToUse);

            int i = 1;
            while (rdr.Read()) {
                IterwebMapInfo iwm = new IterwebMapInfo();
                iwm.IterwebLayoutName = rdr["iterwebtemplate"].ToString();
                iwm.SEName = rdr["ObjetoSEName"].ToString();
                iwm.Url = rdr["url"].ToString();
                iwm.UrlTitle = StringUtils.UppercaseFirst(rdr["urlTitle"].ToString().Replace('_', ' '));
                iwm.UrlPath = rdr["urlPath"].ToString();
                iwm.SitemapType = Convert.ToInt32(rdr["sitemapType"].ToString());
                iwm.IDSE4 = rdr["idSE4"].ToString();
                iwm.IDSE4ArticleId = rdr["idSE4ArticleId"].ToString();
                iwm.Layout = rdr["layout"].ToString();
                iwm.IDOldDoc = rdr["idOld"].ToString();
                iwm.Processed = Convert.ToInt32(rdr["processed"].ToString());
                iwm.IdSitemap = Convert.ToInt32(rdr["idSitemap"].ToString());
                if (rdr["idObjetoSE"] == DBNull.Value) {
                    iwm.IdObjetoSE = -1;
                }
                else {
                    Convert.ToInt64(rdr["idObjetoSE"].ToString());
                }
                iwm.DisplayDate = (DateTime)rdr["DisplayDate"];
                iwm.UpdateDate = (DateTime)rdr["UpdateDate"];
                if (rdr["JsonContent"] == DBNull.Value) {
                    iwm.JsonContent = null;
                }
                else {
                    iwm.JsonContent = rdr["JsonContent"].ToString();
                }
                if (rdr["oldDocStatus"] == DBNull.Value) {
                    iwm.OldDocStatus = MigrateStatusCode.OLD_DOC_STATUS_CODE_NOT_PROCESS;
                }
                else {
                    iwm.OldDocStatus = Convert.ToInt32(rdr["oldDocStatus"].ToString());
                }
                if (rdr["oldDocTemplateType"] == DBNull.Value) {
                    iwm.OldDocTemplateType = MigrateStatusCode.OLD_DOC_STATUS_CODE_NOT_PROCESS;
                }
                else {
                    iwm.OldDocTemplateType = Convert.ToInt32(rdr["oldDocTemplateType"].ToString());
                }
                _se4DocList.Add(iwm);
                Console.Write("\rLoadind record " + iwm.IDOldDoc + " Num document   " + (i++));
            }
            rdr.Close();
            transaction.Commit();
            hdb.Close();
            _isOldDocsLoaded = true;
            if (log.IsDebugEnabled) {
                log.Debug("LoadOldDocuments end");
            }
        }

        /// <summary>
        /// Execute a Query to the database to populate a list of documents to be processed.
        /// </summary>
        protected void LoadSE4Documents() {
            if (log.IsDebugEnabled) log.Debug("LoadSE4Documents start");
            string sql =
                "select @useTop@ " +
                "     iterwebtemplate, ObjetoSEName, url, urlPath, sitemapType, processed, idSitemap, " +
                "	 idSE4, idSE4ArticleId, layout, idOld, idObjetoSE, CreateDate as DisplayDate, Updatedate, idBrightCove, esinfografia, urlTitle  " +
                " from sitemap " +
                " where idSE4 is not null  and url is not null @datefilter@ @layoutfilter@ and urlParameters = '' and processed = 0 " +
                "@SE4DocFilter@" +
                " order by createdate desc, idObjetoSE ";

            string layoutFilter = "";
            string dateFilter = "";
            string se4LayoutList = _se4LayoutToFilter;

            if (_useDateFilter == 1) {
                dateFilter += " and (createdate between convert(datetime, '" + _dateStartFilter + "') ";
                dateFilter += " and convert(datetime, '" + _dateEndFilter + "')) ";
            }
            if (se4LayoutList == "") {
                layoutFilter = "";
            }
            else {
                layoutFilter = "and layout in (@se4layoutlist@) ";
                layoutFilter = layoutFilter.Replace("@se4layoutlist@", se4LayoutList);
            }

            string sqlToUse = sql.Replace("@layoutfilter@", layoutFilter);
            sqlToUse = sqlToUse.Replace("@SE4DocFilter@", _se4DocFilter);
            sqlToUse = sqlToUse.Replace("@datefilter@", dateFilter);
            if (_limitTo == 0) {
                sqlToUse = sqlToUse.Replace("@useTop@", "");
            }
            else {
                sqlToUse = sqlToUse.Replace("@useTop@", "top " + _limitTo);
            }
            if (log.IsWarnEnabled) {
                log.Warn("Using sql=[" + sqlToUse + "]");
            }

            HandleDatabase hdb = new HandleDatabase(_sitemapConnStr);
            hdb.Open();

            SqlTransaction transaction = hdb.BeginTransaction("migratetoiterweb");
            SqlDataReader rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sqlToUse);

            int i = 1;
            while (rdr.Read()) {
                IterwebMapInfo iwm = new IterwebMapInfo();
                iwm.IterwebLayoutName = rdr["iterwebtemplate"].ToString();
                iwm.SEName = rdr["ObjetoSEName"].ToString();
                iwm.Url = rdr["url"].ToString();
                iwm.UrlPath = rdr["urlPath"].ToString();
                iwm.SitemapType = Convert.ToInt32(rdr["sitemapType"].ToString());
                iwm.IDSE4 = rdr["idSE4"].ToString();
                iwm.IDSE4ArticleId = rdr["idSE4ArticleId"].ToString();
                iwm.Layout = rdr["layout"].ToString();
                iwm.IDOldDoc = rdr["idOld"].ToString();
                iwm.IdObjetoSE = Convert.ToInt64(rdr["idObjetoSE"].ToString());
                iwm.DisplayDate = (DateTime)rdr["DisplayDate"];
                iwm.UpdateDate = (DateTime)rdr["UpdateDate"];
                iwm.Processed = Convert.ToInt32(rdr["processed"].ToString());
                iwm.IdSitemap = Convert.ToInt32(rdr["idSitemap"].ToString());
                iwm.IdBrightCove = rdr["idBrightCove"].ToString();
                if (iwm.IdBrightCove == "") {
                    iwm.IdBrightCove = null;
                }
                iwm.IsInfographic = Convert.ToInt32(rdr["esinfografia"]);
                _se4DocList.Add(iwm);
                Console.Write("\rLoadind record " + iwm.IDOldDoc + " Num document   " + (i++));
            }
            rdr.Close();
            transaction.Commit();
            hdb.Close();
            if (log.IsDebugEnabled) log.Debug("LoadSE4Documents end");
        }

        /// <summary>
        /// The sole purpose is to update the fields jSonContent and CreateDate, UpdateDate 
        /// exclusively for Old Documents.
        /// </summary>
        /// <param name="it">Data to update</param>
        protected void UpdateOldDoc(IterwebMapInfo it) {
            if (log.IsDebugEnabled) {
                log.Debug("UpdateOldDoc start");
            }
            string sqlToUse = "update sitemap " +
                              "set jsoncontent = @json, " +
                              "    CreateDate = @ct, " +
                              "    UpdateDate = @ut, " +
                              "    oldDocStatus = @odstatus, " +
                              "    oldDocTemplateType = @odttype " +
                              "where idOld = @id ";
            SqlParameter param1 = new SqlParameter();
            param1.ParameterName = "@json";
            param1.Value = it.JsonContent;
            param1.SqlDbType = SqlDbType.VarChar;

            SqlParameter param2 = new SqlParameter();
            param2.ParameterName = "@ct";
            param2.Value = it.DisplayDate;
            param2.SqlDbType = SqlDbType.DateTime;

            SqlParameter param3 = new SqlParameter();
            param3.ParameterName = "@id";
            param3.Value = it.IDOldDoc;
            param3.SqlDbType = SqlDbType.VarChar;

            SqlParameter param4 = new SqlParameter();
            param4.ParameterName = "@odstatus";
            param4.Value = it.OldDocStatus;
            param4.SqlDbType = SqlDbType.Int;

            SqlParameter param5 = new SqlParameter();
            param5.ParameterName = "@odttype";
            if (it.OldDocTemplateType == -1) {
                param5.Value = DBNull.Value;
            }
            else {
                if (it.OldDocStatus == MigrateStatusCode.OLD_DOC_STATUS_CODE_WARNING) {
                    param5.Value = DBNull.Value;
                }
                else {
                    param5.Value = it.OldDocTemplateType;
                }
            }
            param5.SqlDbType = SqlDbType.Int;

            SqlParameter param6 = new SqlParameter();
            param6.ParameterName = "@ut";
            param6.Value = it.UpdateDate;
            param6.SqlDbType = SqlDbType.DateTime;
            HandleDatabase hdb = new HandleDatabase(_sitemapConnStr);
            hdb.Open();

            SqlTransaction transaction = hdb.BeginTransaction("SiteMapUpdate");
            hdb.ExecSQLStmt(transaction, sqlToUse, param1, param2, param3, param4, param5, param6);
            transaction.Commit();
            hdb.Close();
            if (log.IsDebugEnabled) {
                log.Debug("UpdateOldDoc end");
            }
        }

        /// <summary>
        /// Parameter urlToUse comes with URLs info as 'http://www.elcolombiano.com/BancoConocimiento/P/portada_reinas_trajefantasia/portada_reinas_trajefantasia.asp'.
        /// As this is an ASP web site, the ASP file is found following the server map path for that path. 
        /// Thus, given the latter URL the file is located in the path [SERVERPATH]\BancoConocimiento\P\portada_reinas_trajefantasia\portada_reinas_trajefantasia.asp'.
        /// As this is a console application, we cannot have access to a mapping from URL to SERVERMapPath, but as such we must simulate it by means of a
        /// dedicated string that points to that route to supply this [SERVERPATH].
        /// </summary>
        /// <remarks>If file is not found then a default date of May.06/2008 is set. This date is extracted from min creation date for
        /// SE4 documents minus 2 days and they are set for Old Documents scanning when necessary.</remarks>
        /// <param name="urlToUse">URL to infer for ASP file.</param>
        /// <returns>Current timestamp if file not found and ASP file's timestamp if one found.</returns>
        protected DateTime ExtractOldDocFileDateTime(string urlName) {
            string s = "bancoconocimiento";
            DateTime dt = new DateTime(1900, 01, 01); // If file does not exist, sets this date. Used for OLD docs scanning.
            string urlInfo = urlName.ToLower();

            int inx = urlInfo.IndexOf(s) + s.Length + 1;
            string urlInfoPath = urlInfo.Substring(inx).Replace('/', '\\');
            string filePath = _se4DocSourceFolder + "\\" + urlInfoPath;

            if (File.Exists(filePath)) {
                //dt = File.GetLastWriteTime(filePath);
                dt = File.GetCreationTime(filePath);
            }
            return dt;
        }

        /// <summary>
        /// If program option is set as 1 then the scanner can be used.
        /// </summary>
        /// <returns>True if scan to be used </returns>
        public bool IsOldDocScanSet() {
            return (_oldDocNewURlScan == 1);
        }

        /// <summary>
        /// Save document to database.
        /// </summary>
        /// <param name="oldDocNewURL">Data to save</param>
        private void OldDocNewURLSave(OldDocNewURL item) {
            if (log.IsDebugEnabled) {
                log.Debug("OldDocNewURLSave start");
            }
            string sqlToUse = "insert into OldDocsNewUrls(idOld, url, urlHost, urlPath, urlParameters, urlProcess, urlProcessHost, urlProcessPath, urlProcessParameters, processed) " +
                              "values (@idOld, @url, @urlHost, @urlPath, @urlParameters, @urlProcess, @urlProcessHost, @urlProcessPath, @urlProcessParameters, @processed)";
            SqlParameter param1 = new SqlParameter();
            param1.ParameterName = "@url";
            param1.Value = item.Url;
            param1.SqlDbType = SqlDbType.VarChar;

            SqlParameter param2 = new SqlParameter();
            param2.ParameterName = "@urlHost";
            param2.Value = item.UrlHost;
            param2.SqlDbType = SqlDbType.VarChar;

            SqlParameter param3 = new SqlParameter();
            param3.ParameterName = "@urlPath";
            param3.Value = item.UrlPath;
            param3.SqlDbType = SqlDbType.VarChar;

            SqlParameter param4 = new SqlParameter();
            param4.ParameterName = "@urlParameters";
            param4.Value = item.UrlParameters;
            param4.SqlDbType = SqlDbType.VarChar;

            SqlParameter param5 = new SqlParameter();
            param5.ParameterName = "@urlProcess";
            param5.Value = item.UrlProcess;
            param5.SqlDbType = SqlDbType.VarChar;

            SqlParameter param6 = new SqlParameter();
            param6.ParameterName = "@urlProcessHost";
            param6.Value = item.UrlProcessHost;
            param6.SqlDbType = SqlDbType.VarChar;

            SqlParameter param7 = new SqlParameter();
            param7.ParameterName = "@urlProcessPath";
            param7.Value = item.UrlProcessPath;
            param7.SqlDbType = SqlDbType.VarChar;

            SqlParameter param8 = new SqlParameter();
            param8.ParameterName = "@urlProcessParameters";
            param8.Value = item.UrlProcessParameters;
            param8.SqlDbType = SqlDbType.VarChar;

            SqlParameter param9 = new SqlParameter();
            param9.ParameterName = "@processed";
            param9.Value = item.Processed;
            param9.SqlDbType = SqlDbType.Int;

            SqlParameter param10 = new SqlParameter();
            param10.ParameterName = "@idold";
            param10.Value = item.IDOld;
            param10.SqlDbType = SqlDbType.VarChar;

            HandleDatabase hdb = new HandleDatabase(_sitemapConnStr);
            hdb.Open();

            SqlTransaction transaction = hdb.BeginTransaction("OldDocNewURLSave");
            try {
                hdb.ExecSQLStmt(transaction, sqlToUse, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10);
                transaction.Commit();
            }
            catch (Exception) {
                transaction.Rollback();
            }

            hdb.Close();
            if (log.IsDebugEnabled) {
                log.Debug("OldDocNewURLSave end");
            }
        }

        /// <summary>
        /// Checks if known URL and URL to process are not equal.
        /// </summary>
        /// <param name="oldDocNewURL"></param>
        /// <returns>True if record can be saved</returns>
        private bool CanSaveOldDocNewURL(OldDocNewURL oldDocNewURL) {
            Uri uri = new Uri(oldDocNewURL.Url);
            Uri uriProcess = new Uri(oldDocNewURL.UrlProcess);
            var urlConcat = uri.Host + uri.LocalPath;
            var urlProcessConcat = uriProcess.Host + uriProcess.LocalPath;

            return (urlConcat != urlProcessConcat);
        }

        /// <summary>
        /// Loads documents to scan. The idea is to look for URLs not already stored in
        /// 'sitemap' table from existing URLS that point to valid document files aka one 
        /// that has the 'bancoconocimiento' as reference.
        /// If one is found then it is inserted in OldDocsNewUrls as new, if it exists 
        /// then it is discarded
        /// </summary>
        public void OldDocScan() {
            if (log.IsDebugEnabled) {
                log.Debug("Scan start");
            }
            LoadOldDocuments();
            if (_se4DocList.Count == 0) {
                string msg = "No documents to check, check log in DEBUG mode";
                Console.WriteLine(msg);
                if (log.IsInfoEnabled) {
                    log.Info(msg);
                }
                return;
            }

            var lineInfo = "Scanning for Embeddable URLs  " + _se4DocList.Count + " documents";
            Console.WriteLine();
            Console.WriteLine(lineInfo);
            if (log.IsInfoEnabled) {
                log.Info(lineInfo);
            }

            int i = 1;
            ExtractHTML eHtml = new ExtractHTML();
            string urlToUse = "";

            foreach (var it in _se4DocList) {
                Console.Write("\rProcessing " + it.IDOldDoc + " Num document   " + (i++));
                if (it.OldDocStatus == MigrateStatusCode.OLD_DOC_STATUS_CODE_NOT_PROCESS) {
                    try {
                        urlToUse = it.Url;
                        var extractedURL = eHtml.ExtractURLFromOldDocUsing(urlToUse, _knownDomainList, _replaceKnownDomainList);
                        if (extractedURL != "") {
                            it.OldDocStatus = MigrateStatusCode.OLD_DOC_STATUS_CODE_USED_AS_EMBEDDABLE_URL;
                            it.JsonContent = "";
                            UpdateOldDoc(it);

                            var s = extractedURL.ToUpper();
                            if (s.Contains("BANCOCONOCIMIENTO")) {
                                Uri uri = new Uri(it.Url);
                                Uri uriProcess = new Uri(extractedURL);
                                OldDocNewURL oldDocNewURL = new OldDocNewURL() {
                                    IDOld = it.IDOldDoc,
                                    Url = it.Url,
                                    UrlHost = uri.Host,
                                    UrlPath = uri.LocalPath,
                                    UrlParameters = uri.Query,
                                    UrlProcess = extractedURL,
                                    UrlProcessHost = uriProcess.Host,
                                    UrlProcessPath = uriProcess.LocalPath,
                                    UrlProcessParameters = uriProcess.Query,
                                    Processed = 0,
                                    DateProcessed = DateTime.Now
                                };

                                // We only save if URL is different from URLProcess
                                if (CanSaveOldDocNewURL(oldDocNewURL)) {
                                    OldDocNewURLSave(oldDocNewURL);
                                }

                                // NOTE: In an SQL command in SQL Server console, you must manually insert
                                // new records from table OldDocsNewUrls table to sitemap.
                                // It is not done here to avoid ambiguity and possible reprocessing.
                            }
                        }
                    }
                    catch (Exception ex) {
                        OldDocProcessShowExMsg(it, urlToUse, ex);
                    }
                }
            }

            if (log.IsDebugEnabled) {
                log.Debug("Scan end");
            }
        }

        /// <summary>
        /// Only applies to old document. It assumes that in the document folder
        /// there is the .ASP to it to retrieve its creation time and it be used as 
        /// the DisplayDate field.
        /// Iterates through a list of documents scanning if a Json version.
        /// </summary>
        protected void CheckOldDocumentsToJSon() {
            if (log.IsDebugEnabled) {
                log.Debug("CheckOldDocumentsToJSon start");
            }

            if (_se4DocList.Count == 0) {
                string msg = "No documents to check, check log in DEBUG mode";
                Console.WriteLine(msg);
                if (log.IsInfoEnabled) {
                    log.Info(msg);
                }
                return;
            }

            var lineInfo = "Checking " + _se4DocList.Count + " documents";
            Console.WriteLine(lineInfo);
            if (log.IsInfoEnabled) {
                log.Info(lineInfo);
            }

            int i = 1;
            ExtractHTML eHtml = new ExtractHTML();
            foreach (var it in _se4DocList) {
                Console.Write("\rProcessing " + it.IDOldDoc + " Num document   " + (i++));
                if (it.OldDocStatus == MigrateStatusCode.OLD_DOC_STATUS_CODE_NOT_PROCESS) {
                    string urlToUse = it.Url.ReconfigureHostNameFrom(_knownDomainList, _replaceKnownDomainList);
                    string json = "";

                    try {
                        json = eHtml.ExtractToJson(it.Url, urlToUse, _knownDomainList, _replaceKnownDomainList);
                        int status = eHtml.Status;
                        int templateType = eHtml.TemplateType;

                        // Let's evaluate if returned json is usable.
                        it.JsonContent = json;
                        it.OldDocStatus = status;
                        it.OldDocTemplateType = templateType;

                        // Get File.ASP from URL create time as display document timestamp
                        it.DisplayDate = ExtractOldDocFileDateTime(urlToUse);
                        UpdateOldDoc(it);
                    }
                    catch (Exception ex) {
                        OldDocProcessShowExMsg(it, urlToUse, ex);
                    }
                }
            }
            Console.WriteLine("\r");
            if (log.IsDebugEnabled) {
                log.Debug("CheckOldDocumentsToJSon end");
            }
        }

        private void OldDocProcessShowExMsg(IterwebMapInfo it, string urlToUse, Exception ex) {
            string lineInfo = "An exception occurred in " + it.IDOldDoc + " using URL=[" + urlToUse + "]";
            Console.Write("\r");
            Console.WriteLine("An exception occurred in " + it.IDOldDoc);
            if (log.IsErrorEnabled) {
                log.Error(lineInfo, ex);
            }
            it.JsonContent = "";
            it.OldDocStatus = MigrateStatusCode.OLD_DOC_STATUS_CODE_ERROR_EXCEPTION;

            var exMsg = ex.Message;
            if (exMsg.Contains("404")) {
                it.OldDocStatus = MigrateStatusCode.OLD_DOC_STATUS_CODE_ERROR_404;
            }
            else {
                if (exMsg.Contains("500")) {
                    it.OldDocStatus = MigrateStatusCode.OLD_DOC_STATUS_CODE_ERROR_500;
                }
                else {
                    if (exMsg.Contains("400")) {
                        it.OldDocStatus = MigrateStatusCode.OLD_DOC_STATUS_CODE_ERROR_400;
                    }
                    else {
                        if (exMsg.Contains("403")) {
                            it.OldDocStatus = MigrateStatusCode.OLD_DOC_STATUS_CODE_ERROR_403;
                        }
                    }
                }
            }
            UpdateOldDoc(it);
        }

        /// <summary>
        /// Stores all contents in 'path' folder to 'zipFileName' file. Then deletes soure directory.
        /// </summary>
        /// <param name="path">Folder to compress in ZIP format</param>
        /// <param name="zipFileName"></param>
        protected void CompressToZip(string path, string zipFileName) {
            if (log.IsDebugEnabled) log.Debug("CompressToZip start");
            if (log.IsDebugEnabled) log.Debug("Using path=[" + path + "], zipFileName=[" + zipFileName + "]");
            File.Delete(zipFileName);

            using (var zip = new ZipFile()) {
                zip.AddDirectory(path);
                zip.Save(zipFileName);
            }
            Directory.Delete(path, true);
            if (log.IsDebugEnabled) log.Debug("CompressToZip end");
        }

        /// <summary>
        /// Write to 'DocSize' table to analyze if document is content correct.
        /// </summary>
        /// <param name="idSitemap">Id for sitemap table</param>
        /// <param name="idSE4Article">Article ID SE4 Doc</param>
        /// <param name="idOldDoc">Article ID Old Doc</param>
        /// <param name="templateType">Template used</param>
        /// <param name="size">Threshold size used</param>
        /// <param name="docContent">All Content set</param>
        protected void LogToDocSizeTable(long idSitemap, string idSE4Article, string idOldDoc, string templateType, int size, string docContent) {
            if (log.IsDebugEnabled) {
                log.Debug("UpdateOldDoc start");
            }
            string sqlToUse = "insert into  DocSize(idSitemap, idSE4ArticleId,  idOld, size, content, templateType) values (" +
                              "@idSitemap, @idSE4ArticleId, @idOldDoc, @size, @content, @templateType)";
            SqlParameter param1 = new SqlParameter();
            param1.ParameterName = "@idSitemap";
            param1.Value = idSitemap;
            param1.SqlDbType = SqlDbType.Int;

            SqlParameter param2 = new SqlParameter();
            param2.ParameterName = "@idSE4ArticleId";
            if (idSE4Article == "") {
                param2.Value = DBNull.Value;
            }
            else {
                param2.Value = idSE4Article;
            }
            param2.SqlDbType = SqlDbType.VarChar;

            SqlParameter param3 = new SqlParameter();
            param3.ParameterName = "@idOldDoc";
            if (idOldDoc == "") {
                param3.Value = DBNull.Value;
            }
            else {
                param3.Value = idOldDoc;
            }
            param3.SqlDbType = SqlDbType.VarChar;

            SqlParameter param4 = new SqlParameter();
            param4.ParameterName = "@size";
            param4.Value = size;
            param4.SqlDbType = SqlDbType.Int;

            SqlParameter param5 = new SqlParameter();
            param5.ParameterName = "@content";
            param5.Value = docContent;
            param5.SqlDbType = SqlDbType.VarChar;

            SqlParameter param6 = new SqlParameter();
            param6.ParameterName = "@templateType";
            param6.Value = templateType;
            param6.SqlDbType = SqlDbType.VarChar;

            HandleDatabase hdb = new HandleDatabase(_sitemapConnStr);
            hdb.Open();

            SqlTransaction transaction = hdb.BeginTransaction("LogToDocSizeTable");
            hdb.ExecSQLStmt(transaction, sqlToUse, param1, param2, param3, param4, param5, param6);
            transaction.Commit();
            hdb.Close();
            if (log.IsDebugEnabled) {
                log.Debug("UpdateOldDoc end");
            }
        }
    }
}