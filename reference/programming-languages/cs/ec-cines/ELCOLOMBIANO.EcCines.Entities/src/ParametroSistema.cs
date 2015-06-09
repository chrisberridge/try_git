/*==========================================================================*/
/* Source File:   PARAMETROSISTEMA.CS                                       */
/* Description:   Helper database access to PARAMETROS SISTEMA.             */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: May.05/2015                                               */
/* Version:       1.8                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
============================================================================*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ELCOLOMBIANO.EcCines.Common;
using ELCOLOMBIANO.EcCines.Data;
using ELCOLOMBIANO.EcCines.Entities.Dtos;

namespace ELCOLOMBIANO.EcCines.Entities {
    /// <summary>
    /// Helper database access to PARAMETROS SISTEMA.
    /// </summary>
    public class ParametroSistema : AbstractCommonEntity {
        /// <summary>
        /// Retrieves one record from DB.
        /// </summary>
        /// <param name="nombreParametro">Filter to use</param>
        /// <returns>NULL if no record found.</returns>
        public ParametroSistemaDto getValorParametroSistema(string nombreParametro) {
            if (log.IsDebugEnabled) {
                log.Debug("getValorParametroSistema Starts");
            }
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;
            HandleDatabase hdb = null;
            ParametroSistemaDto r = null;
            try {                
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                SqlParameter param = new SqlParameter() { ParameterName = "@np", Value = nombreParametro, SqlDbType = SqlDbType.VarChar };
                string sql = "sp_obtenerValorParametroSistema @np";
                if (log.IsDebugEnabled) {
                    log.Debug("SQL=[" + sql + "]");
                    var paramValues = "ParameterName=[" + param.ParameterName + "], Value=[" + param.Value + "], SqlDbType=[" + param.SqlDbType + "]";
                    log.Debug("Parameter val=[" + paramValues + "]");
                }
                transaction = hdb.BeginTransaction("ObtenerValorParametroSistema");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, param);
                if (rdr.HasRows) {
                    rdr.Read();
                    r = new ParametroSistemaDto() {
                        idParametro = Convert.ToInt32(rdr["idParametro"]),
                        nombreParametro = rdr["nombreParametro"].ToString(),
                        valorParametro = rdr["valorParametro"].ToString(),
                        descValorParametro = rdr["descValorParametro"].ToString(),
                        visible = Convert.ToChar(rdr["visible"])
                    };
                }
            } catch (Exception ex) {
                if (log.IsFatalEnabled) {
                    log.Fatal("Exception occurred " + ex.Message);
                    log.Fatal("Exception trace=[" + ex.StackTrace + "]");
                    log.Fatal("Return null");
                }
                r = null;                
            } finally {
                try {
                    if (rdr != null) { rdr.Close(); }
                    if (transaction != null) { transaction.Commit(); }
                    if (hdb != null) { hdb.Close(); }   
                } catch (Exception e) {
                    if (log.IsFatalEnabled) {
                        log.Fatal("Exception occurred " + e.Message);
                        log.Fatal("Exception trace=[" + e.StackTrace + "]");
                        log.Fatal("Return null");
                    }
                    r = null;  
                }                             
            }
            if (log.IsDebugEnabled) {
                if (r == null) {
                    log.Debug("Result is NULL");
                }
                else {
                    log.Debug("Result sets to [" + r.ToString() + "]");
                }
                log.Debug("getValorParametroSistema Ends");
            }
            return r;
        }

        /// <summary>
        /// Retrieves all values for table PARAMETROSISTEMA.
        /// </summary>
        /// <returns>List of ParametroSistemaDto objects</returns>
        public List<ParametroSistemaDto> getValoresParametroSistema() {
            if (log.IsDebugEnabled) {
                log.Debug("getValoresParametroSistema Starts");
            }
            SqlDataReader rdr = null;
            SqlTransaction transaction = null;
            HandleDatabase hdb = null;
            List<ParametroSistemaDto> sysParamList = new List<ParametroSistemaDto>();
            try {
                string sql = "sp_obtenerValoresParametroSistema";
                if (log.IsDebugEnabled) {
                    log.Debug("SQL=[" + sql + "]");                    
                }
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                transaction = hdb.BeginTransaction("obtenerValoresParametroSistema");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql);
                while (rdr.Read()) {
                    sysParamList.Add(new ParametroSistemaDto() {
                        idParametro = Convert.ToInt32(rdr["idParametro"]),
                        nombreParametro = rdr["nombreParametro"].ToString(),
                        valorParametro = rdr["valorParametro"].ToString(),
                        descValorParametro = rdr["descValorParametro"].ToString(),
                        visible = Convert.ToChar(rdr["visible"])
                    });
                }                
            } catch (Exception ex) {
                if (log.IsFatalEnabled) {
                    log.Fatal("Exception occurred " + ex.Message);
                    log.Fatal("Exception trace=[" + ex.StackTrace + "]");
                    log.Fatal("List returned is empty");
                }
                sysParamList = new List<ParametroSistemaDto>();
            } finally {
                try {
                    if (rdr != null) { rdr.Close(); }
                    if (transaction != null) { transaction.Commit(); }
                    if (hdb != null) { hdb.Close(); }  
                } catch (Exception e) {
                    if (log.IsFatalEnabled) {
                        log.Fatal("Exception occurred " + e.Message);
                        log.Fatal("Exception trace=[" + e.StackTrace + "]");
                        log.Fatal("List returned is empty");
                    }
                    sysParamList = new List<ParametroSistemaDto>();
                }                              
            }
            if (log.IsDebugEnabled) {
                log.Debug("getValoresParametroSistema Ends");
            }
            return sysParamList;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ParametroSistema()
            : base() {
        }
    }
}
