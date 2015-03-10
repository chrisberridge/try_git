/*==========================================================================*/
/* Source File:   ENTIDAD.CS                                                */
/* Description:  Helper database access to ENTIDAD                          */
/* Author:        Leonardino Lima (LLI)                                     */
/* Date:          Feb.11/2015                                               */
/* Last Modified: Mar.03/2015                                               */
/* Version:       1.2                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
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
    /// Helper database access to ENTIDAD.
    /// </summary>
    public class Entidad
    {
        /// <summary>
        /// Puts info into DB.
        /// </summary>
        /// <param name="info">Record information to submit</param>
        /// <param name="op">Which kind to operation to make. 1:Insert, 2:update, 3:delete</param>
        /// <returns>Identity ID for just created record.</returns>
        public int crearEntidad(EntidadDto info, int op)
        {
            HandleDatabase hdb = null;
            SqlTransaction transaction = null;
            try
            {
                int rslt = 0;
                List<SqlParameter> paramList = new List<SqlParameter>();
                paramList.Add(new SqlParameter() { ParameterName = "@operacion", Value = op, SqlDbType = SqlDbType.Int });
                paramList.Add(new SqlParameter() { ParameterName = "@id", Value = info.idEntidad, SqlDbType = SqlDbType.Int });
                paramList.Add(new SqlParameter() { ParameterName = "@codigo", Value = info.codEntidad.ToString(), SqlDbType = SqlDbType.VarChar });
                paramList.Add(new SqlParameter() { ParameterName = "@nombre", Value = info.nombreEntidad, SqlDbType = SqlDbType.VarChar });
                paramList.Add(new SqlParameter() { ParameterName = "@valor", Value = info.valorEntidad, SqlDbType = SqlDbType.VarChar });
                paramList.Add(new SqlParameter() { ParameterName = "@descripcion", Value = info.descripcionEntidad, SqlDbType = SqlDbType.VarChar });

                String sql = "sp_crearActualizarEntidad @operacion, @id, @codigo, @nombre, @valor, @descripcion";
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                transaction = hdb.BeginTransaction("crearEntidad");
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
        /// Retrieves one record from DB given id.
        /// </summary>
        /// <param name="id">id to match</param>
        /// <returns>NULL if not found, else record information</returns>
        public EntidadDto obtenerValorEntidad(int id)
        {
            HandleDatabase hdb = null;
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;
            try
            {
                EntidadDto r = null;
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@id";
                param.Value = id;
                param.SqlDbType = SqlDbType.Int;
                String sql = "sp_obtenerValorEntidad @id";
                transaction = hdb.BeginTransaction("obtenerValorEntidad");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, param);
                if (rdr.HasRows)
                {
                    rdr.Read();
                    r = new EntidadDto();
                    r.idEntidad = Convert.ToInt32(rdr["IDENTIDAD"]);
                    r.codEntidad = Convert.ToInt32(rdr["CODENTIDAD"]);
                    r.nombreEntidad = rdr["NOMBREENTIDAD"].ToString();
                    r.valorEntidad = rdr["VALORENTIDAD"].ToString();
                    r.descripcionEntidad = rdr["DESCRIPCIONENTIDAD"].ToString();
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

        /// <summary>
        /// Get all records given name entity.
        /// </summary>
        /// <param name="nombreEntidad">The entity to retrieve from</param>
        /// <returns>A list of EntidadDto objects, empty if no records found</returns>
        public List<EntidadDto> obtenerValoresEntidad(string nombreEntidad)
        {
            HandleDatabase hdb = null;
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;
            try
            {
                List<EntidadDto> listaResultado = new List<EntidadDto>();
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@ne";
                param.Value = nombreEntidad.Trim().ToString();
                param.SqlDbType = SqlDbType.VarChar;
                String sql = "sp_obtenerValoresEntidad @ne";
                transaction = hdb.BeginTransaction(sql);
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, param);
                EntidadDto entidad;
                while (rdr.Read())
                {
                    entidad = new EntidadDto();
                    entidad.idEntidad = Convert.ToInt32(rdr["IDENTIDAD"]);
                    entidad.codEntidad = Convert.ToInt32(rdr["CODENTIDAD"]);
                    entidad.nombreEntidad = rdr["NOMBREENTIDAD"].ToString();
                    entidad.valorEntidad = rdr["VALORENTIDAD"].ToString();
                    entidad.descripcionEntidad = rdr["DESCRIPCIONENTIDAD"].ToString();
                    listaResultado.Add(entidad);
                }
                return listaResultado;
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
