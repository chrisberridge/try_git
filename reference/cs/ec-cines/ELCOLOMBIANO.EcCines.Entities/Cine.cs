/*==========================================================================*/
/* Source File:   CINE.CS                                                   */
/* Description:   Helper database access to CINE                            */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.20/2015                                               */
/* Last Modified: Mar.11/2015                                               */
/* Version:       1.2                                                       */
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
using ELCOLOMBIANO.EcCines.Business;
using ELCOLOMBIANO.EcCines.Data;
using ELCOLOMBIANO.EcCines.Entities.Dtos;

namespace ELCOLOMBIANO.EcCines.Entities {
    /// <summary>
    /// Helper database access to CINE 
    /// </summary>
    public class Cine {
        /// <summary>
        /// Puts info into DB.
        /// </summary>
        /// <param name="info">Record information to submit</param>
        /// <param name="op">Which kind to operation to make. 1:Insert, 2:update, 3:delete</param>
        /// <returns>Identity ID for just created record.</returns>
        public int crearCine(CineDto info, int op) {
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
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                transaction = hdb.BeginTransaction("crearCine");
                rslt = hdb.ExecuteSelectSQLStmtAsScalar(transaction, sql, paramList.ToArray());
                return rslt;
            } catch (Exception) {
                return 0;
            } finally {
                if (transaction != null) { transaction.Commit(); }
                if (hdb != null) { hdb.Close(); }
            }
        }

        /// <summary>
        /// Retrieves one record from DB.
        /// </summary>
        /// <param name="id">Filter to use</param>
        /// <returns>NULL if no record found.</returns>
        public CineDto getCine(int id) {
            SqlDataReader rdr = null;
            SqlTransaction transaction = null;
            HandleDatabase hdb = null;
            try {
                CineDto r = null;
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@id";
                param.Value = id;
                param.SqlDbType = SqlDbType.Int;
                string sql = "sp_obtenerCine @id";
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
        /// Retrieves all values for table CINE.
        /// </summary>
        /// <returns>List of CineDto objects</returns>
        public List<CineDto> getCines() {
            HandleDatabase hdb = null;
            SqlDataReader rdr = null;
            SqlTransaction transaction = null;
            try {
                List<CineDto> movieList = new List<CineDto>();
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                string sql = "sp_obtenerCines";
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
                return movieList;
            } catch (Exception) {
                return null;
            } finally {
                if (rdr != null) { rdr.Close(); }
                if (transaction != null) { transaction.Commit(); }
                if (hdb != null) { hdb.Close(); }
            }
        }

        // NOTE: COQ Feb.21/2015, this method must be refactored to TEATO.CS file.
        // Left here in order to compile.
        public List<TeatroDto> getTeatros() {
            HandleDatabase hdb = null;
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;
            try {
                List<TeatroDto> lstTeatros = new List<TeatroDto>();
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                String sql = "sp_obtenerTeatros";
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
                return lstTeatros;
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
