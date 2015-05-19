/*==========================================================================*/
/* Source File:   PARAMETROSISTEMA.CS                                       */
/* Description:   Helper database access to PARAMETROS SISTEMA.             */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: Mar.11/2015                                               */
/* Version:       1.4                                                       */
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
using ELCOLOMBIANO.EcCines.Business;
using ELCOLOMBIANO.EcCines.Data;
using ELCOLOMBIANO.EcCines.Entities.Dtos;

namespace ELCOLOMBIANO.EcCines.Entities {
    /// <summary>
    /// Helper database access to PARAMETROS SISTEMA.
    /// </summary>
    public class ParametroSistema {
        /// <summary>
        /// Retrieves one record from DB.
        /// </summary>
        /// <param name="nombreParametro">Filter to use</param>
        /// <returns>NULL if no record found.</returns>
        public ParametroSistemaDto ObtenerValorParametroSistema(string nombreParametro) {
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;
            HandleDatabase hdb = null;
            try {
                ParametroSistemaDto r = null;
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@np";
                param.Value = nombreParametro;
                param.SqlDbType = SqlDbType.VarChar;
                string sql = "sp_obtenerValorParametroSistema @np";
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
                return r;
            } catch (Exception) {
                return null;
            } finally {
                if (rdr != null) { rdr.Close(); }
                if (transaction != null) { transaction.Commit(); }
                if (hdb != null) { hdb.Close(); }
            }
        }

        /// <summary>
        /// Retrieves all values for table PARAMETROSISTEMA.
        /// </summary>
        /// <returns>List of ParametroSistemaDto objects</returns>
        public List<ParametroSistemaDto> ObtenerValoresParametroSistema() {
            SqlDataReader rdr = null;
            SqlTransaction transaction = null;
            HandleDatabase hdb = null;
            try {
                string sql = "sp_obtenerValoresParametroSistema";
                List<ParametroSistemaDto> sysParamList = new List<ParametroSistemaDto>();
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
                return sysParamList;
            } catch (Exception) {
                return null;
            } finally {
                if (rdr != null) { rdr.Close(); }
                if (transaction != null) { transaction.Commit(); }
                if (hdb != null) { hdb.Close(); }
            }
        }
    }
}
