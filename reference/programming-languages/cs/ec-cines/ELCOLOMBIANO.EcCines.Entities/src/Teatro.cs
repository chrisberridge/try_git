/*==========================================================================*/
/* Source File:   TEATRO.CS                                                 */
/* Description:   Helper database access to TEATRO                          */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.21/2015                                               */
/* Last Modified: May.05/2015                                               */
/* Version:       1.6                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.21/2015 COQ File created.
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
    /// Helper database access to TEATRO 
    /// </summary>
    public class Teatro : AbstractCommonEntity {
        /// <summary>
        /// Puts info into DB.
        /// </summary>
        /// <param name="info">Record information to submit</param>
        /// <param name="op">Which kind to operation to make. 1:Insert, 2:update, 3:delete</param>
        /// <returns>Identity ID for just created record.</returns>
        public int crearTeatro(TeatroDto info, int op) {
            if (log.IsDebugEnabled) {
                log.Debug("crearTeatro Starts");
            }
            HandleDatabase hdb = null;
            SqlTransaction transaction = null;
            int rslt = 0;
            try {
                List<SqlParameter> paramList = new List<SqlParameter>() {
                    new SqlParameter() { ParameterName = "@operacion", Value = op, SqlDbType = SqlDbType.Int },
                    new SqlParameter() { ParameterName = "@id", Value = info.idTeatro, SqlDbType = SqlDbType.Int },
                    new SqlParameter() { ParameterName = "@idCine", Value = info.idCine, SqlDbType = SqlDbType.Int },
                    new SqlParameter() { ParameterName = "@nombreTeatro", Value = info.nombreTeatro.ToString(), SqlDbType = SqlDbType.VarChar },
                    new SqlParameter() { ParameterName = "@telefono1Teatro", Value = info.telefono1Teatro.ToString(), SqlDbType = SqlDbType.VarChar },
                    new SqlParameter() { ParameterName = "@telefono2Teatro", Value = info.telefono2Teatro.ToString(), SqlDbType = SqlDbType.VarChar },
                    new SqlParameter() { ParameterName = "@telefono3Teatro", Value = info.telefono3Teatro.ToString(), SqlDbType = SqlDbType.VarChar },
                    new SqlParameter() { ParameterName = "@idMunicipioTeatro", Value = info.idMunicipioTeatro, SqlDbType = SqlDbType.Int }, 
                    new SqlParameter() { ParameterName = "@idDepartamentoTeatro", Value = info.idDepeartamentoTeatro, SqlDbType = SqlDbType.Int }, 
                    new SqlParameter() { ParameterName = "@direccionTeatro", Value = info.direccionTeatro, SqlDbType = SqlDbType.VarChar }
                };

                String sql = "sp_crearActualizarTeatro @operacion, @id, @idCine, @nombreTeatro, @telefono1Teatro, @telefono2Teatro, @telefono3Teatro, @idMunicipioTeatro, @idDepartamentoTeatro, @direccionTeatro";
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
                transaction = hdb.BeginTransaction("crearCine");
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
                    if (log.IsFatalEnabled) {
                        log.Fatal("Exception occurred " + e.Message);
                        log.Fatal("Exception trace=[" + e.StackTrace + "]");
                        log.Fatal("Returns -1");
                    }
                    rslt = -1;
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("rslt=[" + rslt + "]");
                log.Debug("crearTeatro Ends");
            }
            return rslt;
        }

        /// <summary>
        /// Retrieves one record from DB.
        /// </summary>
        /// <param name="id">Filter to use</param>
        /// <returns>NULL if no record found.</returns>
        public TeatroDto getTeatro(int id) {
            if (log.IsDebugEnabled) {
                log.Debug("getTeatro Starts");
            }
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;
            HandleDatabase hdb = null;
            TeatroDto r = null;
            try {
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                SqlParameter param = new SqlParameter() { ParameterName = "@id", Value = id, SqlDbType = SqlDbType.Int };
                string sql = "sp_obtenerTeatro @id";
                if (log.IsDebugEnabled) {
                    log.Debug("SQL=[" + sql + "]");
                    var paramValues = "ParameterName=[" + param.ParameterName + "], Value=[" + param.Value + "], SqlDbType=[" + param.SqlDbType + "]";
                    log.Debug("Parameter val=[" + paramValues + "]");
                }
                transaction = hdb.BeginTransaction("getTeatro");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, param);
                if (rdr.HasRows) {
                    rdr.Read();
                    r = new TeatroDto() {
                        idTeatro = Convert.ToInt32(rdr["idteatro"]),
                        idCine = Convert.ToInt32(rdr["idcine"]),
                        nombreTeatro = rdr["nombreteatro"].ToString(),
                        telefono1Teatro = rdr["telefono1teatro"].ToString(),
                        telefono2Teatro = rdr["telefono2teatro"].ToString(),
                        telefono3Teatro = rdr["telefono3teatro"].ToString(),
                        idMunicipioTeatro = Convert.ToInt32(rdr["idMunicipioTeatro"]),
                        idDepeartamentoTeatro = Convert.ToInt32(rdr["idDepartamentoTeatro"]),
                        direccionTeatro = rdr["direccionteatro"].ToString()
                    };
                }
            } catch (Exception ex) {
                if (log.IsFatalEnabled) {
                    log.Fatal("Exception occurred " + ex.Message);
                    log.Fatal("Exception trace=[" + ex.StackTrace + "]");
                    log.Fatal("Result sets to null");
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
                        log.Fatal("Result sets to null");
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
                log.Debug("getTeatro Ends");
            }
            return r;
        }

        /// <summary>
        /// Get a list of TeatroDto objects
        /// </summary>
        /// <returns>A list of TeatroDto objects</returns>
        public List<TeatroDto> getTeatros() {
            if (log.IsDebugEnabled) {
                log.Debug("getTeatros Starts");
            }
            HandleDatabase hdb = null;
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;
            List<TeatroDto> lstTeatros = new List<TeatroDto>();
            try {                
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                String sql = "sp_obtenerTeatros";
                if (log.IsDebugEnabled) {
                    log.Debug("Sql=[" + sql + "]");
                }
                transaction = hdb.BeginTransaction("getTeatros");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql);
                while (rdr.Read()) {
                    lstTeatros.Add(new TeatroDto() {
                        idTeatro = Convert.ToInt32(rdr["idteatro"]),
                        idCine = Convert.ToInt32(rdr["idcine"]),
                        nombreTeatro = rdr["nombreteatro"].ToString(),
                        telefono1Teatro = rdr["telefono1teatro"].ToString(),
                        telefono2Teatro = rdr["telefono2teatro"].ToString(),
                        telefono3Teatro = rdr["telefono3teatro"].ToString(),
                        idMunicipioTeatro = Convert.ToInt32(rdr["idMunicipioTeatro"]),
                        idDepeartamentoTeatro = Convert.ToInt32(rdr["idDepartamentoTeatro"]),
                        direccionTeatro = rdr["direccionteatro"].ToString()
                    });
                }                
            } catch (Exception ex) {
                if (log.IsFatalEnabled) {
                    log.Fatal("Exception occurred " + ex.Message);
                    log.Fatal("Exception trace=[" + ex.StackTrace + "]");
                    log.Fatal("Result sets to empty");
                }
                lstTeatros = new List<TeatroDto>();
            } finally {
                try {
                    if (rdr != null) { rdr.Close(); }
                    if (transaction != null) { transaction.Commit(); }
                    if (hdb != null) { hdb.Close(); }    
                } catch (Exception e) {
                    if (log.IsFatalEnabled) {
                        log.Fatal("Exception occurred " + e.Message);
                        log.Fatal("Exception trace=[" + e.StackTrace + "]");
                        log.Fatal("Result sets to empty");
                    }
                    lstTeatros = new List<TeatroDto>();
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("getTeatros Ends");
            }
            return lstTeatros;
        }

        /// <summary>
        /// Extends the data transfer object to expose the theater name and movie name
        /// </summary>
        /// <returns>A list of TeatroExDto objects</returns>
        public List<TeatroExDto> getTeatrosEx() {
            if (log.IsDebugEnabled) {
                log.Debug("getTeatrosEx Starts");
            }
            SqlDataReader rdr = null;
            SqlTransaction transaction = null;
            HandleDatabase hdb = null;
            List<TeatroExDto> lstTeatros = new List<TeatroExDto>();
            try {                
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                String sql = "sp_obtenerExTeatros";
                if (log.IsDebugEnabled) {
                    log.Debug("Sql=[" + sql + "]");
                }
                transaction = hdb.BeginTransaction("getTeatrosEx");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql);
                while (rdr.Read()) {
                    lstTeatros.Add(new TeatroExDto() {
                        idTeatro = Convert.ToInt32(rdr["idteatro"]),
                        idCine = Convert.ToInt32(rdr["idcine"]),
                        nombreTeatro = rdr["nombreteatro"].ToString(),
                        telefono1Teatro = rdr["telefono1teatro"].ToString(),
                        telefono2Teatro = rdr["telefono2teatro"].ToString(),
                        telefono3Teatro = rdr["telefono3teatro"].ToString(),
                        idMunicipioTeatro = Convert.ToInt32(rdr["idMunicipioTeatro"]),
                        idDepeartamentoTeatro = Convert.ToInt32(rdr["idDepartamentoTeatro"]),
                        direccionTeatro = rdr["direccionteatro"].ToString(),
                        nombreCine = rdr["nombreCine"].ToString(),
                        nombreMunicipio = rdr["nombreMunicipio"].ToString(),
                        nombreDepartamento = rdr["nombreDepartamento"].ToString()
                    });
                }                
            } catch (Exception ex) {
                if (log.IsFatalEnabled) {
                    log.Fatal("Exception occurred " + ex.Message);
                    log.Fatal("Exception trace=[" + ex.StackTrace + "]");
                    log.Fatal("Result sets to empty");
                }
                lstTeatros = new List<TeatroExDto>();
            } finally {
                try {
                    if (rdr != null) { rdr.Close(); }
                    if (transaction != null) { transaction.Commit(); }
                    if (hdb != null) { hdb.Close(); }
                } catch (Exception e) {
                    if (log.IsFatalEnabled) {
                        log.Fatal("Exception occurred " + e.Message);
                        log.Fatal("Exception trace=[" + e.StackTrace + "]");
                        log.Fatal("Result sets to empty");
                    }
                    lstTeatros = new List<TeatroExDto>();
                }                
            }
            if (log.IsDebugEnabled) {
                log.Debug("getTeatrosEx Ends");
            }
            return lstTeatros;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Teatro()
            : base() {
        }
    }
}
