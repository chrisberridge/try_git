/*==========================================================================*/
/* Source File:   SETTINGS.CS                                               */
/* Description:   Helper class to retrieve system settings (usually stored  */
/*                in a configuration file, i.e., web.config.                */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.19/2015                                               */
/* Last Modified: Feb.26/2015                                               */
/* Version:       1.5                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.19/2015 COQ File created.
============================================================================*/

using ELCOLOMBIANO.EcCines.Constants;
using System.Configuration;

namespace ELCOLOMBIANO.EcCines.Business
{
    /// <summary>
    /// Helper class to retrieve system settings (usually stored in a configuration file, i.e., web.config.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Retrieves the connection key from configuration file.
        /// </summary>
        public static string Connection
        {
            get
            {
                return Crypto.Decrypt(ConfigurationManager.AppSettings[GlobalConstants.ConnectionKey], GlobalConstants.EncryptKey);
            }
        }

        /// <summary>
        /// Retrieves the file movies key from configuration file.
        /// </summary>
        public static string FileMovies
        {
            get
            {
                return Crypto.Decrypt(ConfigurationManager.AppSettings[GlobalConstants.FileMoviesKey], GlobalConstants.EncryptKey);
            }
        }

        /// <summary>
        /// Retrieves the file catalog movies key from configuration file.
        /// </summary>
        public static string FileMovieCatalog
        {
            get
            {
                return Crypto.Decrypt(ConfigurationManager.AppSettings[GlobalConstants.FileMoviesCatalogKey], GlobalConstants.EncryptKey);
            }
        }

        public static string ImageFolder
        {
            get
            {
                return Crypto.Decrypt(ConfigurationManager.AppSettings[GlobalConstants.ImageFolderKey], GlobalConstants.EncryptKey);
            }
        }

        public static string JSONFolder
        {
            get
            {
                return Crypto.Decrypt(ConfigurationManager.AppSettings[GlobalConstants.JSONFolderKey], GlobalConstants.EncryptKey);
            }
        }

        public static string SysParamGender
        {
            get
            {
                return GlobalConstants.SysParamGenderKey;
            }
        }

        public static string SysParamFormat
        {
            get
            {
                return GlobalConstants.SysParamFormatKey;
            }
        }

        public static string SysParamCounty
        {
            get
            {
                return GlobalConstants.SysParamCountyKey;
            }
        }

        public static string SysParamEstate
        {
            get
            {
                return GlobalConstants.SysParamEstateKey;
            }
        }

        public static string SysParamCountry
        {
            get
            {
                return GlobalConstants.SysParamCountryKey;
            }
        }

        public static string SysParamClassification
        {
            get
            {
                return GlobalConstants.SysParamClassificationKey;
            }
        }

        public static string Usuario
        {
            get
            {
                return Crypto.Decrypt(ConfigurationManager.AppSettings[GlobalConstants.Usuario], GlobalConstants.EncryptKey);
            }
        }

        public static string Contrasena
        {
            get
            {
                return Crypto.Decrypt(ConfigurationManager.AppSettings[GlobalConstants.Contrasena], GlobalConstants.EncryptKey);
            }
        }
        
        public static string ImageFolderSave
        {
            get
            {
                return Crypto.Decrypt(ConfigurationManager.AppSettings[GlobalConstants.ImageFolderSaveKey], GlobalConstants.EncryptKey);
            }
        }
    }
}