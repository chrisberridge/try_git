/*==========================================================================*/
/* Source File:   GLOBALCONSTANTS.CS                                        */
/* Description:   Project constants                                         */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Aug.13/2013                                               */
/* Last Modified: Oct.28/2014                                               */
/* Version:       1.4                                                      */
/* Copyright (c), 2013, 2014 Arkix, El Colombiano                           */
/*==========================================================================*/

/*===========================================================================
History
Aug.13/2013 COQ File created.
============================================================================*/
using System;

namespace EC.Utils.Constants {
    /// <summary>
    /// Global application Constants. Used as a static class access only. This way it
    /// is assured that a change in the constant value is modified in one place.
    /// </summary>
    public sealed class GlobalConstants {
        // General
        public const String IMAGE_NAME_TOON = "Infografico";
        public const String IMAGE_NAME_GENERAL = "Image";
        public const String IMAGE_NAME_INPHOGRAFIC = "Infografico";
        public const String EXT_JPG = ".jpg";
        public const int LIMIT_IMAGE_NAME = 70;
        public const int OLD_DOC_CONTENT_THRESHOLD = 200;
        public const int SE4_DOC_CONTENT_THRESHOLD = 200;    

        public const int DOC_PROCESSING_MODE_ALL = 0;
        public const int DOC_PROCESSING_MODE_SE4_ONLY = 1;
        public const int DOC_PROCESSING_MODE_OLD_ONLY = 2;
        public const int DOC_PROCESSING_MODE_TOON_FROM_DIR = 3;
        public const int DOC_PROCESSING_MODE_NONE = 4;
        public const int DOC_PROCESSING_MODE_SYNC_DB = 5;
        public const int DOC_PROCESSING_MODE_EXEC_SQLEXECSTMT = 6;
        public const int DOC_PROCESSING_MODE_VALIDATE_URL_CHARS = 7;
        public const int DOC_PROCESSING_MODE_USER_EXPORT = 8;
        public const int DOC_PROCESSING_MODE_PRINT_VERSION = 9;
        public const int DOC_PROCESSING_MODE_COMPUTE_URLTITLE = 10;
        public const int DOC_PROCESSING_MODE_UPDATE_CREATE_DATE_OLD_DOCS_ONLY = 11;
        public const int DOC_PROCESSING_MODE_UPDATE_CREATE_SQL_TO_CSV = 12;
        public const int DOC_PROCESSING_MODE_EXEC_SQL_BATCH = 13;
        public const int DOC_PROCESSING_MODE_UPDATE_CREATE_SQL_TO_CSV_SQL_IN_FILE = 14;
        public const int DOC_PROCESSING_MODE_ANALYZE_OLDOCS_CONTENT_SIZE = 15;
        public const int DOC_PROCESSING_MODE_ANALYZE_SE4DOCS_CONTENT_SIZE = 16;
        public const int DOC_PROCESSING_MODE_IDENTIFY_BRIGHTCOVE_VIDEO = 17;
        public const int DOC_PROCESSING_MODE_LOOKUP_OLD_DOCS_BANCOCONOCIMIENTO_IN_TEXT = 18;

        public const int SE4ATTR_MULTIMEDIA_NONE = 0;
        public const int SE4ATTR_MULTIMEDIA_IMAGE = 1;
        public const int SE4ATTR_MULTIMEDIA_VIDEO = 2;
        public const int SE4ATTR_MULTIMEDIA_AUDIO = 3;

        // Iterweb XML manifest constants (new format, the one which uses ARTICLES).
        public const string ARTICLE_HISTORICAL_GENERAL = "HistoricalGeneral";
        public const string ARTICLE_HISTORICAL_IMAGE_GALLERY = "HistoricalImageGallery";
        public const string ARTICLE_HISTORICAL_VIDEO_GALLERY = "HistoricalVideoGallery";
        public const string ARTICLE_HISTORICAL_INFOGRAPHIC_GALLERY = "HistoricalInfographicGallery";
        public const string ARTICLE_HISTORICAL_TOON_GALLERY = "HistoricalToonGallery";
        public const string ARTICLE_HISTORICAL_AGGREGATOR = "HistoricalAggregator";

        // App Settings
        public const string StoreFolderKey = "StoreFolder";
        public const string ConnectionKey = "Connection";
        public const string SitemapConnectionKey = "SitemapConnection";
        public const string LimitToKey = "LimitTo";
        public const string UseOldDocLimitToKey = "UseOldDocLimitTo";
        public const string SE4LayoutToFilterKey = "SE4LayoutToFilter";
        public const string XmlBeautifyKey = "XmlBeautify";
        public const string ZipFolderKey = "ZipFolder";
        public const string Se4MediaSourceFolderKey = "Se4MediaSourceFolder";
        public const string Se4MediaImageGallerySourceFolderKey = "Se4MediaImageGallerySourceFolder";
        public const string Se4DocSourceFolderKey = "Se4DocSourceFolder";
        public const string OldDocFilterKey = "OldDocFilter";
        public const string SE4DocFilterKey = "SE4DocFilter";
        public const string TempFolderKey = "TempFolder";
        public const string DocLimitKey = "DocLimit";
        public const string DocumentProcessingModeKey = "DocumentProcessingMode";
        public const string UpdateCreateDateKey = "UpdateCreateDate";
        public const string OldDocNewURlScanKey = "OldDocNewURlScan";
        public const string KnownDomain001Key = "KnownDomain001";
        public const string KnownDomain002Key = "KnownDomain002";
        public const string KnownDomain003Key = "KnownDomain003";
        public const string KnownDomain004Key = "KnownDomain004";
        public const string KnownDomain005Key = "KnownDomain005";
        public const string ReplaceKnownDomain001Key = "ReplaceKnownDomain001";
        public const string ReplaceKnownDomain002Key = "ReplaceKnownDomain002";
        public const string ReplaceKnownDomain003Key = "ReplaceKnownDomain003";
        public const string ReplaceKnownDomain004Key = "ReplaceKnownDomain004";
        public const string ReplaceKnownDomain005Key = "ReplaceKnownDomain005";
        public const string OutputStructureKey = "OutputStructure";
        public const string ResetJSonFilesKey = "ResetJSonFiles";
        public const string SectionsJSONFileKey = "SectionsJSONFile";
        public const string VocabulariesJSONFileKey = "VocabulariesJSONFile";
        public const string UseDateFilterKey = "UseDateFilter";
        public const string DateStartFilterKey = "DateStartFilter";
        public const string DateEndFilterKey = "DateEndFilter";
        public const string ByPassToonCreationKey = "ByPassToonCreation";

        // Manifest Settings
        public const string IterWebLayoutNameKey = "IterWebLayoutName";
        public const string GlobalSiteNameKey = "GlobalSiteName";
        public const string DefaultImageMaxWidthKey = "DefaultImageMaxWidth";
        public const string DefaultImageMaxHeightKey = "DefaultImageMaxHeight";
        public const string TitleLimitToKey = "TitleLimitTo";
        public const string QualificationKey = "Qualification";
        public const string LayoutKey = "Layout";
        public const string ResetCountersKey = "ResetCounters";
        public const string CounterJSONFilerKey = "CounterJSONFile";
        public const string SQLExecStmtKey = "SQLExecStmt";        
        public const string CSVExportFileKey = "CSVExportFile";
        public const string SQLBatchFileKey = "SQLBatchFile";
    }
}
