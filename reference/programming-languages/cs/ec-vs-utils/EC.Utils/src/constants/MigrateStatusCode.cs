/*==========================================================================*/
/* Source File:   MIGRATESTATUSCODE.CS                                      */
/* Description:   Project constants                                         */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Aug.13/2013                                               */
/* Last Modified: Nov.13/2013                                               */
/* Version:       1.9                                                       */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Aug.13/2013 COQ File created.
============================================================================*/

namespace EC.Utils.Constants {
    /// <summary>
    /// Error codes for migration. Stored in database.
    /// </summary>
    public sealed class MigrateStatusCode {
        // General

        /// <summary>
        /// Not generated/processed
        /// </summary>
        public const int OLD_DOC_STATUS_CODE_NOT_PROCESS = -1;

        /// <summary>
        /// do not generate in package
        /// </summary>
        public const int OLD_DOC_STATUS_CODE_ERROR_NO_GENERATE = 1;

        /// <summary>
        /// generate in package
        /// </summary>
        public const int OLD_DOC_STATUS_CODE_SUCCESS = 2;

        /// <summary>
        /// do not generate in package, needs to be checked manually
        /// </summary>
        public const int OLD_DOC_STATUS_CODE_WARNING = 3;

        /// <summary>
        /// Not an ASP extension.
        /// </summary>
        public const int OLD_DOC_STATUS_CODE_ERROR_NOT_ASP_PAGE = 4;

        /// <summary>
        /// Page being scanned presented an exception.
        /// </summary>
        public const int OLD_DOC_STATUS_CODE_ERROR_EXCEPTION = 5;

        /// <summary>
        /// Page downloaded complete but server replies as not found.
        /// </summary>
        public const int OLD_DOC_STATUS_CODE_ERROR_404 = 404;

        /// <summary>
        /// Page downloaded but access is forbidden.
        /// </summary>
        public const int OLD_DOC_STATUS_CODE_ERROR_403 = 403;

        /// <summary>
        /// Page downloaded complet but with errors.
        /// </summary>
        public const int OLD_DOC_STATUS_CODE_ERROR_500 = 500;

        /// <summary>
        /// Page cannot be fetched.
        /// </summary>
        public const int OLD_DOC_STATUS_CODE_ERROR_400 = 400;

        /// <summary>
        /// How many invalid scanned files are set.
        /// </summary>
        public const int OLD_DOC_STATUS_CODE_POST_VALIDATE_NO_TITLE = 6;

        /// <summary>
        /// Any old document with this status is set to not be processed at all.
        /// That is, it is discarded at all.
        /// </summary>
        public const int OLD_DOC_STATUS_CODE_MANUALLY_SET_DISCARDED = 7;

        /// <summary>
        /// If a given URL from Sitemap was scanned as a truly embeddable URL 
        /// then it must be discarded from being processed altogether.
        /// </summary>
        public const int OLD_DOC_STATUS_CODE_USED_AS_EMBEDDABLE_URL = 8;
    }
}
