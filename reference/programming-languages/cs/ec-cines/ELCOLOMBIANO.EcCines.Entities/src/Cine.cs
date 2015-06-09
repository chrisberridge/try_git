/*==========================================================================*/
/* Source File:   CINE.CS                                                   */
/* Description:   Helper database access to CINE                            */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.20/2015                                               */
/* Last Modified: May.05/2015                                               */
/* Version:       1.6                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.20/2015 COQ File created.
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
    /// Helper database access to CINE 
    /// </summary>
    public class Cine : AbstractCommonEntity {
        /// <summary>
        /// Puts info into DB.
        /// </summary>
        /// <param name="info">Record information to submit</param>
        /// <param name="op">Which kind to operation to make. 1:Insert, 2:update, 3:delete</param>
        /// <returns>Identity ID for just created record.</returns>
        public int createCine(CineDto info, int op) {
            if (log.IsDebugEnabled) {
                log.Debug("createCine Starts");
            }
            SqlTransaction transaction = null;
            HandleDatabase hdb = null;
            int rslt = 0;
            try {
                List<SqlParameter> paramList = new List<SqlParameter>();
                paramList.Add(new SqlParameter() { ParameterName = "@operacion", Value = op, SqlDbType = SqlDbType.Int });
                paramList.Add(new SqlParameter() { ParameterName = "@id", Value = info.idCine, SqlDbType = SqlDbType.Int });
                paramList.Add(new SqlParameter() { ParameterName = "@nombreCine", Value = info.nombreCine.ToString(), SqlDbType = SqlDbType.VarChar });
                paramList.Add(new SqlParameter() { ParameterName = "@fechaCreacionCine", Value = info.fechaCreacionCine, SqlDbType = SqlDbType.DateTime });
                paramList.Add(new SqlParameter() { ParameterName = "@nit", Value = info.nit, SqlDbType = SqlDbType.VarChar });
                String sql = "sp_crearActualizarCine @operacion, @id, @nombreCine, @fechaCreacionCine, @nit";
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
                if (log.IsDebugEnabled) {                    
                    log.Debug("createCine Ends");
                }                                
            } catch (Exception ex) {
                if (log.IsFatalEnabled) {
                    log.Fatal("Exception occurred " + ex.Message);
                    log.Fatal("Exception trace=[" + ex.StackTrace +  "]");
                    log.Fatal("Returns 0");
                }
                rslt = 0;                
            } finally {
                try {
                    if (transaction != null) { transaction.Commit(); }
                    if (hdb != null) { hdb.Close(); }
                } catch (Exception e) {
                    if (log.IsFatalEnabled) {
                        log.Fatal("Exception occurred " + e.Message);
                        log.Fatal("Exception trace=[" + e.StackTrace + "]");
                        log.Fatal("Returns 0");
                    }
                    rslt = 0;
                }                
            }
            log.Debug("Rslt=[" + rslt + "]");
            if (log.IsDebugEnabled) {
                log.Debug("createCine Ends");
            }
            return rslt;
        }

        /// <summary>
        /// Retrieves one record from DB.
        /// </summary>
        /// <param name="id">Filter to use</param>
        /// <returns>NULL if no record found.</returns>
        public CineDto getCine(int id) {
            if (log.IsDebugEnabled) {
                log.Debug("getCine Starts");
                log.Debug("id=[" + id + "]");
            }
            SqlDataReader rdr = null;
            SqlTransaction transaction = null;
            HandleDatabase hdb = null;
            CineDto r = null;
            try {                
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                SqlParameter param = new SqlParameter() { ParameterName = "@id", Value = id, SqlDbType = SqlDbType.Int};
                string sql = "sp_obtenerCine @id";                
                if (log.IsDebugEnabled) {
                    log.Debug("SQL=[" + sql + "]");                    
                    var paramValues = "ParameterName=[" + param.ParameterName + "], Value=[" + param.Value + "], SqlDbType=[" + param.SqlDbType + "]";
                    log.Debug("Parameter val=[" + paramValues + "]");
                }
                transaction = hdb.BeginTransaction("getCine");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, param);
                if (rdr.HasRows) {
                    rdr.Read();
                    r = new CineDto() {
                        idCine = Convert.ToInt32(rdr["idCine"]),
                        nit = rdr["nit"].ToString(),
                        fechaCreacionCine = Convert.ToDateTime(rdr["fechaCreacionCine"]),
                        nombreCine = rdr["nombreCine"].ToString()
                    };
                    if (log.IsDebugEnabled) {
                        log.Debug("Record retrieved =[" + r.ToString() + "]");
                    }
                }
            } catch (Exception ex) {
                if (log.IsFatalEnabled) {
                    log.Fatal("Exception occurred " + ex.Message);
                    log.Fatal("Exception trace=[" + ex.StackTrace + "]");
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
        /// Retrieves all values for table CINE.
        /// </summary>
        /// <returns>List of CineDto objects</returns>
        public List<CineDto> getCines() {
            if (log.IsDebugEnabled) {
                log.Debug("getCines Starts");
            }
            HandleDatabase hdb = null;
            SqlDataReader rdr = null;
            SqlTransaction transaction = null;
            List<CineDto> movieList = new List<CineDto>();
            try {                
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                string sql = "sp_obtenerCines";
                if (log.IsDebugEnabled) {
                    log.Debug("Sql=[" + sql + "]");
                }
                transaction = hdb.BeginTransaction("getCines");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql);
                while (rdr.Read()) {
                    movieList.Add(new CineDto() {
                        idCine = Convert.ToInt32(rdr["idCine"]),
                        nit = rdr["nit"].ToString(),
                        fechaCreacionCine = Convert.ToDateTime(rdr["fechaCreacionCine"]),
                        nombreCine = rdr["nombreCine"].ToString()
                    });
                }
            } catch (Exception ex) {
                if (log.IsFatalEnabled) {
                    log.Fatal("Exception occurred " + ex.Message);
                    log.Fatal("Exception trace=[" + ex.StackTrace + "]");
                    log.Fatal("Empty list returned");
                }
                movieList = new List<CineDto>();
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
                    movieList = new List<CineDto>();
                }                           
            }
            if (log.IsDebugEnabled) {
                log.Debug("getCines Ends");
            }
            return movieList;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Cine()
            : base() {
        }
    }
}
