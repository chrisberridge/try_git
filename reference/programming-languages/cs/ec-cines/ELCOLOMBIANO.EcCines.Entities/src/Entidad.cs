/*==========================================================================*/
/* Source File:   ENTIDAD.CS                                                */
/* Description:   Helper database access to ENTIDAD                         */
/* Author:        Leonardino Lima (LLI)                                     */
/* Date:          Feb.11/2015                                               */
/* Last Modified: May.05/2015                                               */
/* Version:       1.6                                                       */
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
    /// Helper database access to ENTIDAD.
    /// </summary>
    public class Entidad : AbstractCommonEntity {
        /// <summary>
        /// Puts info into DB.
        /// </summary>
        /// <param name="info">Record information to submit</param>
        /// <param name="op">Which kind to operation to make. 1:Insert, 2:update, 3:delete</param>
        /// <returns>Identity ID for just created record.</returns>
        public int createEntidad(EntidadDto info, int op) {
            if (log.IsDebugEnabled) {
                log.Debug("createEntidad Starts");
            }
            HandleDatabase hdb = null;
            SqlTransaction transaction = null;
            int rslt = 0;
            try {                
                List<SqlParameter> paramList = new List<SqlParameter>() {
                    new SqlParameter() { ParameterName = "@operacion", Value = op, SqlDbType = SqlDbType.Int },
                    new SqlParameter() { ParameterName = "@id", Value = info.idEntidad, SqlDbType = SqlDbType.Int },
                    new SqlParameter() { ParameterName = "@codigo", Value = info.codEntidad.ToString(), SqlDbType = SqlDbType.VarChar },
                    new SqlParameter() { ParameterName = "@nombre", Value = info.nombreEntidad, SqlDbType = SqlDbType.VarChar },
                    new SqlParameter() { ParameterName = "@valor", Value = info.valorEntidad, SqlDbType = SqlDbType.VarChar },
                    new SqlParameter() { ParameterName = "@descripcion", Value = info.descripcionEntidad, SqlDbType = SqlDbType.VarChar }
                };

                String sql = "sp_crearActualizarEntidad @operacion, @id, @codigo, @nombre, @valor, @descripcion";
                var i = 1;
                if (log.IsDebugEnabled) {
                    log.Debug("SQL=[" + sql + "]");
                    paramList.ForEach(p => {
                        var paramValues = "ParameterName=[" + p.ParameterName + "], Value=[" + p.Value + "], SqlDbType=[" + p.SqlDbType + "]";
                        log.Debug("Parameter " + i++ + " val=[" + paramValues + "]");
                    });
                }
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                transaction = hdb.BeginTransaction("crearEntidad");
                rslt = hdb.ExecuteSelectSQLStmtAsScalar(transaction, sql, paramList.ToArray());                
            } catch (Exception ex) {
                if (log.IsFatalEnabled) {
                    log.Fatal("Exception occurred " + ex.Message);
                    log.Fatal("Exception trace=[" + ex.StackTrace + "]");
                    log.Fatal("Returns -1");
                }
                rslt = -1;                
            } finally {
                try {
                    if (transaction != null) { transaction.Commit(); }
                    if (hdb != null) { hdb.Close(); }
                } catch (Exception e) {
                    log.Fatal("Exception occurred " + e.Message);
                    log.Fatal("Exception trace=[" + e.StackTrace + "]");
                    log.Fatal("Returns -1");
                    rslt = -1;
                }                
            }            
            if (log.IsDebugEnabled) {
                log.Debug("Rslt=[" + rslt + "]");
                log.Debug("createEntidad Ends");
            }
            return rslt;
        }

        /// <summary>
        /// Retrieves one record from DB given id.
        /// </summary>
        /// <param name="id">id to match</param>
        /// <returns>NULL if not found, else record information</returns>
        public EntidadDto getValorEntidad(int id) {
            if (log.IsDebugEnabled) {
                log.Debug("getValorEntidad Starts");
            }
            HandleDatabase hdb = null;
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;
            EntidadDto r = null;
            try {                
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                SqlParameter param = new SqlParameter() { ParameterName = "@id", Value = id, SqlDbType = SqlDbType.Int };
                String sql = "sp_obtenerValorEntidad @id";
                if (log.IsDebugEnabled) {
                    log.Debug("SQL=[" + sql + "]");
                    var paramValues = "ParameterName=[" + param.ParameterName + "], Value=[" + param.Value + "], SqlDbType=[" + param.SqlDbType + "]";
                    log.Debug("Parameter val=[" + paramValues + "]");
                }
                transaction = hdb.BeginTransaction("obtenerValorEntidad");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, param);
                if (rdr.HasRows) {
                    rdr.Read();
                    r = new EntidadDto();
                    r.idEntidad = Convert.ToInt32(rdr["IDENTIDAD"]);
                    r.codEntidad = Convert.ToInt32(rdr["CODENTIDAD"]);
                    r.nombreEntidad = rdr["NOMBREENTIDAD"].ToString();
                    r.valorEntidad = rdr["VALORENTIDAD"].ToString();
                    r.descripcionEntidad = rdr["DESCRIPCIONENTIDAD"].ToString();
                }                
            } catch (Exception ex) {
                if (log.IsFatalEnabled) {
                    log.Fatal("Exception occurred " + ex.Message);
                    log.Fatal("Exception trace=[" + ex.StackTrace + "]");
                    log.Fatal("Returns null");
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
                log.Debug("getCine Ends");
            }
            return r;
        }

        /// <summary>
        /// Get all records given name entity.
        /// </summary>
        /// <param name="nombreEntidad">The entity to retrieve from</param>
        /// <returns>A list of EntidadDto objects, empty if no records found</returns>
        public List<EntidadDto> getValoresEntidad(string nombreEntidad) {
            if (log.IsDebugEnabled) {
                log.Debug("getValoresEntidad Starts");
            }
            HandleDatabase hdb = null;
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;
            List<EntidadDto> listaResultado = new List<EntidadDto>();
            try {
                
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@ne";
                param.Value = nombreEntidad.Trim().ToString();
                param.SqlDbType = SqlDbType.VarChar;
                String sql = "sp_obtenerValoresEntidad @ne";
                if (log.IsDebugEnabled) {
                    log.Debug("sql=[" + sql + "]");
                }
                transaction = hdb.BeginTransaction(sql);
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, param);
                EntidadDto entidad;
                while (rdr.Read()) {
                    entidad = new EntidadDto();
                    entidad.idEntidad = Convert.ToInt32(rdr["IDENTIDAD"]);
                    entidad.codEntidad = Convert.ToInt32(rdr["CODENTIDAD"]);
                    entidad.nombreEntidad = rdr["NOMBREENTIDAD"].ToString();
                    entidad.valorEntidad = rdr["VALORENTIDAD"].ToString();
                    entidad.descripcionEntidad = rdr["DESCRIPCIONENTIDAD"].ToString();
                    listaResultado.Add(entidad);
                }                
            } catch (Exception ex) {
                if (log.IsFatalEnabled) {
                    log.Fatal("Exception occurred " + ex.Message);
                    log.Fatal("Exception trace=[" + ex.StackTrace + "]");
                    log.Fatal("Empty list returned");
                }
                listaResultado = new List<EntidadDto>();
            } finally {
                try {
                    if (rdr != null) { rdr.Close(); }
                    if (transaction != null) { transaction.Commit(); }
                    if (hdb != null) { hdb.Close(); }
                } catch (Exception e) {
                    if (log.IsFatalEnabled) {
                        log.Fatal("Exception occurred " + e.Message);
                        log.Fatal("Exception trace=[" + e.StackTrace + "]");
                        log.Fatal("Empty list returned");
                    }
                    listaResultado = new List<EntidadDto>();
                }
                                
            }
            if (log.IsDebugEnabled) {
                log.Debug("getValoresEntidad Ends");
            }
            return listaResultado;
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Entidad() : base() {
        }
    }
}
