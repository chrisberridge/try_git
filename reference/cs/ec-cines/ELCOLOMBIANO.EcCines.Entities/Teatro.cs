/*==========================================================================*/
/* Source File:   TEATRO.CS                                                 */
/* Description:   Helper database access to TEATRO                          */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.21/2015                                               */
/* Last Modified: Mar.03/2015                                               */
/* Version:       1.2                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.21/2015 COQ File created.
============================================================================*/

using ELCOLOMBIANO.EcCines.Business;
using ELCOLOMBIANO.EcCines.Data;
using ELCOLOMBIANO.EcCines.Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ELCOLOMBIANO.EcCines.Entities
{
    /// <summary>
    /// Helper database access to TEATRO 
    /// </summary>
    public class Teatro
    {
        /// <summary>
        /// Puts info into DB.
        /// </summary>
        /// <param name="info">Record information to submit</param>
        /// <param name="op">Which kind to operation to make. 1:Insert, 2:update, 3:delete</param>
        /// <returns>Identity ID for just created record.</returns>
        public int crearTeatro(TeatroDto info, int op)
        {
            HandleDatabase hdb = null;
            SqlTransaction transaction = null;
            try
            {
                int rslt = 0;
                List<SqlParameter> paramList = new List<SqlParameter>();
                paramList.Add(new SqlParameter() { ParameterName = "@operacion", Value = op, SqlDbType = SqlDbType.Int });
                paramList.Add(new SqlParameter() { ParameterName = "@id", Value = info.idTeatro, SqlDbType = SqlDbType.Int });
                paramList.Add(new SqlParameter() { ParameterName = "@idCine", Value = info.idCine, SqlDbType = SqlDbType.Int });
                paramList.Add(new SqlParameter() { ParameterName = "@nombreTeatro", Value = info.nombreTeatro.ToString(), SqlDbType = SqlDbType.VarChar });
                paramList.Add(new SqlParameter() { ParameterName = "@telefono1Teatro", Value = info.telefono1Teatro.ToString(), SqlDbType = SqlDbType.VarChar });
                paramList.Add(new SqlParameter() { ParameterName = "@telefono2Teatro", Value = info.telefono2Teatro.ToString(), SqlDbType = SqlDbType.VarChar });
                paramList.Add(new SqlParameter() { ParameterName = "@telefono3Teatro", Value = info.telefono3Teatro.ToString(), SqlDbType = SqlDbType.VarChar });
                paramList.Add(new SqlParameter() { ParameterName = "@idMunicipioTeatro", Value = info.idMunicipioTeatro, SqlDbType = SqlDbType.Int });
                paramList.Add(new SqlParameter() { ParameterName = "@idDepartamentoTeatro", Value = info.idDepeartamentoTeatro, SqlDbType = SqlDbType.Int });
                paramList.Add(new SqlParameter() { ParameterName = "@direccionTeatro", Value = info.direccionTeatro, SqlDbType = SqlDbType.VarChar });

                String sql = "sp_crearActualizarTeatro @operacion, @id, @idCine, @nombreTeatro, @telefono1Teatro, @telefono2Teatro, @telefono3Teatro, @idMunicipioTeatro, @idDepartamentoTeatro, @direccionTeatro";
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                transaction = hdb.BeginTransaction("crearCine");
                rslt = hdb.ExecuteSelectSQLStmtAsScalar(transaction, sql, paramList.ToArray());
                return rslt;
            }
            catch (Exception)
            {
                return -1;
            }
            finally
            {
                if (transaction != null) { transaction.Commit(); }
                if (hdb != null) { hdb.Close(); }
            }
        }

        /// <summary>
        /// Retrieves one record from DB.
        /// </summary>
        /// <param name="id">Filter to use</param>
        /// <returns>NULL if no record found.</returns>
        public TeatroDto getTeatro(int id)
        {
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;
            HandleDatabase hdb = null;
            try
            {
                TeatroDto r = null;
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@id";
                param.Value = id;
                param.SqlDbType = SqlDbType.Int;
                string sql = "sp_obtenerTeatro @id";
                transaction = hdb.BeginTransaction("getTeatro");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, param);
                if (rdr.HasRows)
                {
                    rdr.Read();
                    r = new TeatroDto()
                    {
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
                return r;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (rdr != null) { rdr.Close(); }
                if (transaction != null) { transaction.Commit(); }
                if (hdb != null) { hdb.Close(); }
            }
        }

        public List<TeatroDto> getTeatros()
        {
            HandleDatabase hdb = null;
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;
            try
            {
                List<TeatroDto> lstTeatros = new List<TeatroDto>();
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                String sql = "sp_obtenerTeatros";
                transaction = hdb.BeginTransaction("getTeatros");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql);
                while (rdr.Read())
                {
                    lstTeatros.Add(new TeatroDto()
                    {
                        idTeatro = Convert.ToInt32(rdr["idteatro"]),
                        idCine = Convert.ToInt32(rdr["idcine"]),
                        nombreTeatro = rdr["nombreteatro"].ToString(),
                        telefono1Teatro = rdr["telefono1teatro"].ToString(),
                        telefono2Teatro = rdr["telefono2teatro"].ToString(),
                        telefono3Teatro = rdr["telefono3teatro"].ToString(),
                        idMunicipioTeatro = Convert.ToInt32(rdr["idMunicipioTeatro"]),
                        idDepeartamentoTeatro = Convert.ToInt32(rdr["idDepeartamentoTeatro"]),
                        direccionTeatro = rdr["direccionteatro"].ToString()
                    });
                }               
                return lstTeatros;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (rdr != null) { rdr.Close(); }
                if (transaction != null) { transaction.Commit(); }
                if (hdb != null) { hdb.Close(); }
            }
        }

        public List<TeatroExDto> getTeatrosEx()
        {
            SqlDataReader rdr = null;
            SqlTransaction transaction = null;
            HandleDatabase hdb = null;
            try
            {
                List<TeatroExDto> lstTeatros = new List<TeatroExDto>();
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                String sql = "sp_obtenerExTeatros";
                transaction = hdb.BeginTransaction("getTeatrosEx");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql);
                while (rdr.Read())
                {
                    lstTeatros.Add(new TeatroExDto()
                    {
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
                return lstTeatros;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (rdr != null) { rdr.Close(); }
                if (transaction != null) { transaction.Commit(); }
                if (hdb != null) { hdb.Close(); }
            }
        }
    }
}
