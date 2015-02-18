/*==========================================================================*/
/* Source File:   GLOBALCONSTANTS.CS                                        */
/* Description:   Project constants                                         */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: Feb.12/2015                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
============================================================================*/
using System;

namespace EC.Utils.Constants {
    /// <summary>
    /// Global application Constants. Used as a static class access only. This way it
    /// is assured that a change in the constant value is modified in one place.
    /// </summary>
    public sealed class GlobalConstants {
        // General

        // App Settings
        public const string ConnectionKey = "Connection";
        public const string FileMoviesKey = "FileMovies";
        public const string FileMoviesCatalogKey = "FileMoviesCatalog";
    }
}
