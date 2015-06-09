/*==========================================================================*/
/* Source File:   GLOBALCONSTANTS.CS                                        */
/* Description:   Project constants                                         */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: Apr.08/2015                                               */
/* Version:       1.9                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
============================================================================*/

namespace ELCOLOMBIANO.EcCines.Common.Constants {
    /// <summary>
    /// Global application Constants. Used as a static class access only. This way it
    /// is assured that a change in the constant value is modified in one place.
    /// </summary>
    public sealed class GlobalConstants {
        // General
        public const string IsActive = "S";
        public const string IsNotActive = "N";
        public const string IsPremiere = "S";
        public const string IsNotPremiere = "N";

        // App Settings
        public const string ConnectionKey = "Connection";
        public const string FileMoviesKey = "FileMovies";
        public const string FileMoviesCatalogKey = "FileMoviesCatalog";
        public const string EncryptKey = "3E6C83A2-12FC-431D-A302-8D5B36DF51A9";
        public const string SysParamGenderKey = "SISTEMA_GENERO_PELICULA";
        public const string SysParamFormatKey = "SISTEMA_GENERO_PELICULA";
        public const string SysParamClassificationKey = "SISTEMA_CLASIFICACION_PELICULA";
        public const string SysParamCountyKey = "SISTEMA_MUNICIPIO";
        public const string SysParamEstateKey = "SISTEMA_DEPARTAMENTO";
        public const string SysParamCountryKey = "SISTEMA_PAIS";
        public const string ImageFolderKey = "ImageFolder";
        public const string ImageFolderSaveKey = "ImageFolderSave";
        public const string JSONFolderKey = "JSONFolder";
        public const string UserKey = "Usuario";
        public const string PasswordKey = "Contrasena";
        public const string UrlMilleniumPrefixKey = "urlMilleniumPrefix";
    }
}
