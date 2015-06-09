/*==========================================================================*/
/* Source File:   SETTINGS.CS                                               */
/* Description:   Helper class to retrieve system settings (usually stored  */
/*                in a configuration file, i.e., web.config.                */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.19/2015                                               */
/* Last Modified: Apr.08/2015                                               */
/* Version:       1.9                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.19/2015 COQ File created.
============================================================================*/

using System.Configuration;
using ELCOLOMBIANO.EcCines.Common.Constants;

namespace ELCOLOMBIANO.EcCines.Common {
    /// <summary>
    /// Helper class to retrieve system settings (usually stored in a configuration file, i.e., web.config.
    /// </summary>
    public class Settings {
        /// <summary>
        /// Retrieves the connection key from configuration file.
        /// </summary>
        public static string Connection {
            get {
                return Crypto.Decrypt(ConfigurationManager.AppSettings[GlobalConstants.ConnectionKey], GlobalConstants.EncryptKey);
            }
        }

        /// <summary>
        /// Retrieves the file movies key from configuration file.
        /// </summary>
        public static string FileMovies {
            get {
                return Crypto.Decrypt(ConfigurationManager.AppSettings[GlobalConstants.FileMoviesKey], GlobalConstants.EncryptKey);
            }
        }

        /// <summary>
        /// Retrieves the file catalog movies key from configuration file.
        /// </summary>
        public static string FileMovieCatalog {
            get {
                return Crypto.Decrypt(ConfigurationManager.AppSettings[GlobalConstants.FileMoviesCatalogKey], GlobalConstants.EncryptKey);
            }
        }

        /// <summary>
        /// A part of folder path where poster images are to be saved.
        /// </summary>
        public static string ImageFolder {
            get {
                return Crypto.Decrypt(ConfigurationManager.AppSettings[GlobalConstants.ImageFolderKey], GlobalConstants.EncryptKey);
            }
        }

        /// <summary>
        /// A part of folder path where JSON files are to be saved.
        /// </summary>
        public static string JSONFolder {
            get {
                return Crypto.Decrypt(ConfigurationManager.AppSettings[GlobalConstants.JSONFolderKey], GlobalConstants.EncryptKey);
            }
        }

        /// <summary>
        /// Gender parameter to ENTIDAD table
        /// </summary>
        public static string SysParamGender {
            get {
                return GlobalConstants.SysParamGenderKey;
            }
        }

        /// <summary>
        /// Format parameter to ENTIDAD table
        /// </summary>
        public static string SysParamFormat {
            get {
                return GlobalConstants.SysParamFormatKey;
            }
        }

        /// <summary>
        /// County parameter to ENTIDAD table
        /// </summary>
        public static string SysParamCounty {
            get {
                return GlobalConstants.SysParamCountyKey;
            }
        }

        /// <summary>
        /// Estate parameter to ENTIDAD table
        /// </summary>
        public static string SysParamEstate {
            get {
                return GlobalConstants.SysParamEstateKey;
            }
        }

        /// <summary>
        /// Country parameter to ENTIDAD table
        /// </summary>
        public static string SysParamCountry {
            get {
                return GlobalConstants.SysParamCountryKey;
            }
        }

        /// <summary>
        /// Classification parameter to ENTIDAD table
        /// </summary>
        public static string SysParamClassification {
            get {
                return GlobalConstants.SysParamClassificationKey;
            }
        }

        /// <summary>
        /// Default authentication User
        /// </summary>
        public static string User {
            get {
                return Crypto.Decrypt(ConfigurationManager.AppSettings[GlobalConstants.UserKey], GlobalConstants.EncryptKey);
            }
        }

        /// <summary>
        /// Default authentication User pwd
        /// </summary>
        public static string Password {
            get {
                return Crypto.Decrypt(ConfigurationManager.AppSettings[GlobalConstants.PasswordKey], GlobalConstants.EncryptKey);
            }
        }

        /// <summary>
        /// Path to folder where poster images are to be saved.
        /// </summary>
        public static string ImageFolderSave {
            get {
                return Crypto.Decrypt(ConfigurationManager.AppSettings[GlobalConstants.ImageFolderSaveKey], GlobalConstants.EncryptKey);
            }
        }

        /// <summary>
        /// URL movie prefix,  i.e., the movie article in El colombiano is written using the Millenium Editor software
        /// which generates an url like so: http://www.elcolombiano.com/cartelera-de-cine/cincuenta-sombras-de-grey-IK1387864
        /// thus this property is set to return http://www.elcolombiano.com/cartelera-de-cine/ only.
        /// </summary>
        public static string UrlMilleniumPrefix {
            get {
                return Crypto.Decrypt(ConfigurationManager.AppSettings[GlobalConstants.UrlMilleniumPrefixKey], GlobalConstants.EncryptKey);
            }
        }
    }
}