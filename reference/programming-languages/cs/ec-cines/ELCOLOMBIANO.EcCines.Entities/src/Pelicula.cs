/*==========================================================================*/
/* Source File:   PELICULA.CS                                               */
/* Description:   Helper database access to PELICULA                        */
/* Author:        Leonardino Lima (LLIMA)                                   */
/*                Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: May.05/2015                                               */
/* Version:       1.15                                                      */
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
using ELCOLOMBIANO.EcCines.Entities.Dtos.Movie;

namespace ELCOLOMBIANO.EcCines.Entities {
    /// <summary>
    /// Helper database access to PELICULA
    /// </summary>
    public class Pelicula : AbstractCommonEntity {
        /// <summary>
        /// Gets a list of active movies to work.
        /// </summary>
        /// <returns>A list of DetallePeliculaDto objects</returns>
        public List<DetallePeliculaDto> getPeliculasActivas() {
            if (log.IsDebugEnabled) {
                log.Debug("getPeliculasActivas Starts");
            }
            SqlDataReader rdr = null;
            SqlTransaction transaction = null;
            HandleDatabase hdb = null;
            List<DetallePeliculaDto> lstPelicula = new List<DetallePeliculaDto>();
            try {
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                String sql = "sp_obtenerPeliculasActivas";
                if (log.IsDebugEnabled) {
                    log.Debug("Sql=[" + sql + "]");
                }
                transaction = hdb.BeginTransaction("getPeliculasActivas");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql);
                while (rdr.Read()) {
                    lstPelicula.Add(new DetallePeliculaDto() {
                        idPelicula = Convert.ToInt32(rdr["idPelicula"]),
                        idDetallePelicula = Convert.ToInt32(rdr["idDetallePelicula"]),
                        nombrePelicula = rdr["nombrePelicula"].ToString(),
                        idUsuarioCreador = Convert.ToInt32(rdr["idUsuarioCreador"].ToString()),
                        fechaCreacionPelicula = Convert.ToDateTime(rdr["fechaCreacionPelicula"].ToString()),
                        idGeneroPelicula = Convert.ToInt32(rdr["idGeneroPelicula"].ToString()),
                        sinopsis = rdr["sinopsis"].ToString(),
                        urlArticuloEc = rdr["urlArticuloEC"].ToString(),
                        enCartelera = (rdr["activo"].ToString() == "S" ? "Si" : "No")
                    });
                }
            } catch (Exception ex) {
                if (log.IsFatalEnabled) {
                    log.Fatal("Exception occurred " + ex.Message);
                    log.Fatal("Exception trace=[" + ex.StackTrace + "]");
                    log.Fatal("Return sets to emtpy list");
                }
                lstPelicula = new List<DetallePeliculaDto>();
            } finally {
                try {
                    if (rdr != null) { rdr.Close(); }
                    if (transaction != null) { transaction.Commit(); }
                    if (hdb != null) { hdb.Close(); }
                } catch (Exception e) {
                    if (log.IsFatalEnabled) {
                        log.Fatal("Exception occurred " + e.Message);
                        log.Fatal("Exception trace=[" + e.StackTrace + "]");
                        log.Fatal("Return sets to emtpy list");
                    }
                    lstPelicula = new List<DetallePeliculaDto>();
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("getPeliculasActivas Ends");
            }
            return lstPelicula;
        }

        /// <summary>
        /// Gets a list of movies to work.
        /// </summary>
        /// <returns>A list of DetallePeliculaDto objects</returns>
        public List<DetallePeliculaDto> getPeliculas() {
            if (log.IsDebugEnabled) {
                log.Debug("getPeliculas Starts");
            }
            SqlDataReader rdr = null;
            SqlTransaction transaction = null;
            HandleDatabase hdb = null;
            List<DetallePeliculaDto> lstPelicula = new List<DetallePeliculaDto>();
            try {
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                String sql = "sp_obtenerPeliculas";
                if (log.IsDebugEnabled) {
                    log.Debug("Sql=[" + sql + "]");
                }
                transaction = hdb.BeginTransaction("getPeliculas");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql);
                while (rdr.Read()) {
                    lstPelicula.Add(new DetallePeliculaDto() {
                        idPelicula = Convert.ToInt32(rdr["idPelicula"]),
                        idDetallePelicula = Convert.ToInt32(rdr["idDetallePelicula"]),
                        nombrePelicula = rdr["nombrePelicula"].ToString(),
                        idUsuarioCreador = Convert.ToInt32(rdr["idUsuarioCreador"].ToString()),
                        fechaCreacionPelicula = Convert.ToDateTime(rdr["fechaCreacionPelicula"].ToString()),
                        idGeneroPelicula = Convert.ToInt32(rdr["idGeneroPelicula"].ToString()),
                        sinopsis = rdr["sinopsis"].ToString(),
                        urlArticuloEc = rdr["urlArticuloEC"].ToString(),
                        enCartelera = (rdr["activo"].ToString() == "S" ? "Si" : "No")
                    });
                }
            } catch (Exception ex) {
                if (log.IsFatalEnabled) {
                    log.Fatal("Exception occurred " + ex.Message);
                    log.Fatal("Exception trace=[" + ex.StackTrace + "]");
                    log.Fatal("Empty list returned");
                }
                lstPelicula = new List<DetallePeliculaDto>();
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
                    lstPelicula = new List<DetallePeliculaDto>();
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("getPeliculas Ends");
            }
            return lstPelicula;
        }

        /// <summary>
        /// Retrieves a record from DB about a PeliculaDto object.
        /// </summary>
        /// <param name="idPelicula">Parameter to retrieve.</param>
        /// <returns>One record with data or NULL if none found</returns>
        public DetallePeliculaDto getPelicula(int idPelicula) {
            if (log.IsDebugEnabled) {
                log.Debug("getPelicula Starts");
            }
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;
            HandleDatabase hdb = null;
            DetallePeliculaDto r = null;
            try {
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                String sql = "sp_obtenerPelicula @IDPELICULA";
                SqlParameter param = new SqlParameter() { ParameterName = "@IDPELICULA", Value = idPelicula, SqlDbType = SqlDbType.Int };
                if (log.IsDebugEnabled) {
                    log.Debug("SQL=[" + sql + "]");
                    var paramValues = "ParameterName=[" + param.ParameterName + "], Value=[" + param.Value + "], SqlDbType=[" + param.SqlDbType + "]";
                    log.Debug("Parameter val=[" + paramValues + "]");
                }
                transaction = hdb.BeginTransaction("getPelicula");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, param);
                if (rdr.HasRows) {
                    rdr.Read();
                    r = new DetallePeliculaDto() {
                        idPelicula = Convert.ToInt32(rdr["idPelicula"]),
                        idDetallePelicula = Convert.ToInt32(rdr["idDetallePelicula"]),
                        nombrePelicula = rdr["nombrePelicula"].ToString(),
                        idUsuarioCreador = Convert.ToInt32(rdr["idUsuarioCreador"].ToString()),
                        fechaCreacionPelicula = Convert.ToDateTime(rdr["fechaCreacionPelicula"].ToString()),
                        idGeneroPelicula = Convert.ToInt32(rdr["idGeneroPelicula"].ToString()),
                        imagenCartelera = rdr["imagenCartelera"].ToString(),
                        sinopsis = rdr["sinopsis"].ToString(),
                        urlArticuloEc = rdr["urlArticuloEC"].ToString(),
                        enCartelera = rdr["activo"].ToString(),
                        premiere = rdr["premiere"].ToString()
                    };
                }
            } catch (Exception ex) {
                if (log.IsFatalEnabled) {
                    log.Fatal("Exception occurred " + ex.Message);
                    log.Fatal("Exception trace=[" + ex.StackTrace + "]");
                    log.Fatal("Record return is null");
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
                        log.Fatal("Record return is null");
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
                log.Debug("getPelicula Ends");
            }
            return r;
        }

        /// <summary>
        /// Retrieves which movies are in which thater.
        /// </summary>
        /// <param name="teatro">The theater id to retrieve</param>
        /// <returns>A list of PeliculaDto objects</returns>
        public List<PeliculaDto> getPeliculasPorTeatro(int teatro) {
            if (log.IsDebugEnabled) {
                log.Debug("getPeliculasPorTeatro Starts");
            }
            HandleDatabase hdb = null;
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;
            List<PeliculaDto> lstPeliculasCine = new List<PeliculaDto>();
            try {
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                String sql = "sp_obtenerPeliculasPorTeatro @TEATRO";
                SqlParameter param = new SqlParameter() { ParameterName = "@TEATRO", Value = teatro, SqlDbType = SqlDbType.Int };
                if (log.IsDebugEnabled) {
                    log.Debug("SQL=[" + sql + "]");
                    var paramValues = "ParameterName=[" + param.ParameterName + "], Value=[" + param.Value + "], SqlDbType=[" + param.SqlDbType + "]";
                    log.Debug("Parameter val=[" + paramValues + "]");
                }
                transaction = hdb.BeginTransaction("PeliculasPorTeatro");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, param);
                while (rdr.Read()) {
                    lstPeliculasCine.Add(new PeliculaDto() {
                        idPelicula = Convert.ToInt32(rdr["idPelicula"]),
                        idTeatro = Convert.ToInt32(rdr["idTeatro"]),
                        nombrePelicula = rdr["nombrePelicula"].ToString(),
                        nombreTeatro = rdr["nombreTeatro"].ToString()
                    });
                }
            } catch (Exception ex) {
                if (log.IsFatalEnabled) {
                    log.Fatal("Exception occurred " + ex.Message);
                    log.Fatal("Exception trace=[" + ex.StackTrace + "]");
                    log.Fatal("List is empty");
                }
                lstPeliculasCine = new List<PeliculaDto>();
            } finally {
                try {
                    if (rdr != null) { rdr.Close(); }
                    if (transaction != null) { transaction.Commit(); }
                    if (hdb != null) { hdb.Close(); }
                } catch (Exception e) {
                    if (log.IsFatalEnabled) {
                        log.Fatal("Exception occurred " + e.Message);
                        log.Fatal("Exception trace=[" + e.StackTrace + "]");
                        log.Fatal("List is empty");
                    }
                    lstPeliculasCine = new List<PeliculaDto>();
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("getPeliculasPorTeatro Ends");
            }
            return lstPeliculasCine;
        }

        /// <summary>
        /// Creates a Movie in DB.
        /// </summary>
        /// <param name="teatro">The theater to reference</param>
        /// <param name="peliculas">The list of ids for movies referenced in this theater.</param>
        /// <returns>0 if successful, -1 if error</returns>
        public int createPeliculaTeatro(int teatro, string peliculas) {
            if (log.IsDebugEnabled) {
                log.Debug("createPeliculaTeatro Starts");
            }
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;
            HandleDatabase hdb = null;
            int rslt = 0;
            try {
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                List<SqlParameter> paramList = new List<SqlParameter>() {
                    new SqlParameter() {ParameterName = "@peliculas", Value = peliculas, SqlDbType = SqlDbType.VarChar},
                    new SqlParameter() {ParameterName = "@teatro", Value = teatro, SqlDbType = SqlDbType.Int}
                };
                String sql = "sp_crearPeliculasPorTeatro @teatro,@peliculas";
                var i = 1;
                if (log.IsDebugEnabled) {
                    log.Debug("SQL=[" + sql + "]");
                    paramList.ForEach(p => {
                        var paramValues = "ParameterName=[" + p.ParameterName + "], Value=[" + p.Value + "], SqlDbType=[" + p.SqlDbType + "]";
                        log.Debug("Parameter " + i++ + " val=[" + paramValues + "]");
                    });
                }
                transaction = hdb.BeginTransaction("crearPeliculaPorTeatro");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, paramList.ToArray());
                rslt = 1;
            } catch (Exception ex) {
                if (log.IsFatalEnabled) {
                    log.Fatal("Exception occurred " + ex.Message);
                    log.Fatal("Exception trace=[" + ex.StackTrace + "]");
                    log.Fatal("Result is set to -1");
                }
                rslt = -1;
            } finally {
                try {
                    if (rdr != null) { rdr.Close(); }
                    if (transaction != null) { transaction.Commit(); }
                    if (hdb != null) { hdb.Close(); }
                } catch (Exception e) {
                    if (log.IsFatalEnabled) {
                        log.Fatal("Exception occurred " + e.Message);
                        log.Fatal("Exception trace=[" + e.StackTrace + "]");
                        log.Fatal("Result is set to -1");
                    }
                    rslt = -1;
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("r=[" + rslt + "]");
                log.Debug("createPeliculaTeatro Ends");
            }
            return rslt;
        }

        /// <summary>
        /// Given a movie Id and a theater Id, retrieves the schedule for those params.
        /// </summary>
        /// <param name="idPelicula">Which movie to retrieve</param>
        /// <param name="idTeatro">Which theater to retrieve</param>
        /// <returns>A list of PeliculaFullInfoDto objecs </returns>
        public List<PeliculaFullInfoDto> getProgramacionPelicula(int idPelicula, int idTeatro) {
            if (log.IsDebugEnabled) {
                log.Debug("getProgramacionPelicula Starts");
            }
            HandleDatabase hdb = null;
            SqlDataReader rdr = null;
            SqlTransaction transaction = null;
            List<PeliculaFullInfoDto> lstResultado = new List<PeliculaFullInfoDto>();
            try {
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                List<SqlParameter> paramList = new List<SqlParameter>() {
                    new SqlParameter() { ParameterName = "@idPelicula", Value = idPelicula, SqlDbType = SqlDbType.Int}, 
                    new SqlParameter() { ParameterName = "@idTeatro", Value = idTeatro,SqlDbType = SqlDbType.Int}
                };
                String sql = "sp_obtenerProgramacionPelicula @idPelicula,@idTeatro";
                var i = 1;
                if (log.IsDebugEnabled) {
                    log.Debug("SQL=[" + sql + "]");
                    paramList.ForEach(p => {                        
                        var paramValues = "ParameterName=[" + p.ParameterName + "], Value=[" + p.Value + "], SqlDbType=[" + p.SqlDbType + "]";
                        log.Debug("Parameter " + i++ + " val=[" + paramValues + "]");
                    });
                }
                transaction = hdb.BeginTransaction("obtenerProgramacionPelicula");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, paramList.ToArray());
                while (rdr.Read()) {
                    lstResultado.Add(new PeliculaFullInfoDto() {
                        idFormato = Convert.ToInt32(rdr["idFormato"]),
                        nombreFormato = rdr["nombreFormato"].ToString(),
                        idPelicula = Convert.ToInt32(rdr["idPelicula"]),
                        idHorarioPelicula = Convert.ToInt32(rdr["idHorarioPelicula"]),
                        annoHorarioPelicula = rdr["annoHorarioPelicula"].ToString(),
                        mesHorarioPelicula = rdr["mesHorarioPelicula"].ToString(),
                        diaHorarioPelicula = rdr["diaHorarioPelicula"].ToString(),
                        nombreDiaSemanaHorarioPelicula = rdr["nombreDiaSemanaHorarioPelicula"].ToString(),
                        idTeatro = Convert.ToInt32(rdr["idTeatro"]),
                        nombreTeatro = rdr["nombreTeatro"].ToString(),
                        frecuencia = Convert.ToInt32(rdr["frecuencia"]),
                        horaPelicula = rdr["horaPelicula"].ToString(),
                        minutoPelicula = rdr["minutoPelicula"].ToString(),
                        sala = Convert.ToInt32(rdr["sala"]),
                    });
                }
            } catch (Exception ex) {
                if (log.IsFatalEnabled) {
                    log.Fatal("Exception occurred " + ex.Message);
                    log.Fatal("Exception trace=[" + ex.StackTrace + "]");
                    log.Fatal("Empty list returned");
                }
                lstResultado = new List<PeliculaFullInfoDto>();
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
                    lstResultado = new List<PeliculaFullInfoDto>();
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("getProgramacionPelicula Ends");
            }
            return lstResultado;
        }

        /// <summary>
        /// Creates a new record in DB.
        /// </summary>
        /// <param name="pelicula">Movie details</param>
        /// <param name="operacion">Operation to accomplish: 1:create, 2:update</param>
        /// <returns></returns>
        public int createPelicula(DetallePeliculaDto pelicula, int operacion) {
            if (log.IsDebugEnabled) {
                log.Debug("createPelicula Starts");
            }
            HandleDatabase hdb = null;
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;
            int rslt = 0;
            try {
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                List<SqlParameter> paramList = new List<SqlParameter>() { 
                    new SqlParameter() {ParameterName = "@OPERACION", Value = operacion, SqlDbType = SqlDbType.Int},
                    new SqlParameter() { ParameterName = "@PELICULA", Value = pelicula.idPelicula,  SqlDbType = SqlDbType.Int},
                    new SqlParameter() { ParameterName = "@NOMBRE",   Value = pelicula.nombrePelicula, SqlDbType = SqlDbType.VarChar},
                    new SqlParameter() { ParameterName = "@USUARIO",  Value = pelicula.idUsuarioCreador, SqlDbType = SqlDbType.Int},
                    new SqlParameter() { ParameterName = "@GENERO",   Value = pelicula.idGeneroPelicula, SqlDbType = SqlDbType.Int},
                    new SqlParameter() { ParameterName = "@SINOPSIS", Value = pelicula.sinopsis, SqlDbType = SqlDbType.VarChar},
                    new SqlParameter() { ParameterName = "@IMAGEN",   Value = pelicula.imagenCartelera, SqlDbType = SqlDbType.VarChar},
                    new SqlParameter() { ParameterName = "@URL",      Value = pelicula.urlArticuloEc, SqlDbType = SqlDbType.VarChar},
                    new SqlParameter() { ParameterName = "@ACTIVO",   Value = pelicula.enCartelera, SqlDbType = SqlDbType.VarChar}
                };
                String sql = "sp_crearActualizarPelicula @OPERACION,@PELICULA,@NOMBRE,@USUARIO,@GENERO,@SINOPSIS,@IMAGEN,@URL,@ACTIVO";
                var i = 1;
                if (log.IsDebugEnabled) {
                    log.Debug("SQL=[" + sql + "]");
                    paramList.ForEach(p => {
                        var paramValues = "ParameterName=[" + p.ParameterName + "], Value=[" + p.Value + "], SqlDbType=[" + p.SqlDbType + "]";
                        log.Debug("Parameter " + i++ + " val=[" + paramValues + "]");
                    });
                }
                transaction = hdb.BeginTransaction("crearPelicula");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, paramList.ToArray());
                rslt = 1;
            } catch (Exception ex) {
                if (log.IsFatalEnabled) {
                    log.Fatal("Exception occurred " + ex.Message);
                    log.Fatal("Exception trace=[" + ex.StackTrace + "]");
                    log.Fatal("Result sets to -1");
                }
                rslt = -1;
            } finally {
                try {
                    if (rdr != null) { rdr.Close(); }
                    if (transaction != null) { transaction.Commit(); }
                    if (hdb != null) { hdb.Close(); }
                } catch (Exception e) {
                    if (log.IsFatalEnabled) {
                        log.Fatal("Exception occurred " + e.Message);
                        log.Fatal("Exception trace=[" + e.StackTrace + "]");
                        log.Fatal("Result sets to -1");
                    }
                    rslt = -1;
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("rslt=[" + rslt + "]");
                log.Debug("createPelicula Ends");
            }
            return rslt;
        }

        /// <summary>
        /// Create or updates a schedule programming.
        /// </summary>
        /// <param name="datosProgramacion">What to update/create</param>
        /// <returns>0: Success, -1: Failure</returns>
        public int createUpdateProgramacionPelicula(ProgramacionPeliculaDto datosProgramacion) {
            if (log.IsDebugEnabled) {
                log.Debug("createUpdateProgramacionPelicula Starts");
            }
            HandleDatabase hdb = null;
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;
            int rslt = 0;
            try {
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                List<SqlParameter> paramList = new List<SqlParameter>() {
                    new SqlParameter() {ParameterName = "@IDHORARIOPELICULA", Value = datosProgramacion.idHorarioPelicula, SqlDbType = SqlDbType.Int},
                    new SqlParameter() {ParameterName = "@IDFORMATO", Value = datosProgramacion.idFormato, SqlDbType = SqlDbType.Int},
                    new SqlParameter() {ParameterName = "@IDPELICULA", Value = datosProgramacion.idPelicula, SqlDbType = SqlDbType.Int},
                    new SqlParameter() {ParameterName = "@IDTEATRO", Value = datosProgramacion.idTeatro, SqlDbType = SqlDbType.Int},
                    new SqlParameter() {ParameterName = "@ANNOHORARIO", Value = datosProgramacion.annoHorarioPelicula, SqlDbType = SqlDbType.Int},
                    new SqlParameter() {ParameterName = "@MESHORARIO", Value = datosProgramacion.mesHorarioPelicula, SqlDbType = SqlDbType.Int},
                    new SqlParameter() {ParameterName = "@DIAHORARIO", Value = datosProgramacion.diaHorarioPelicula, SqlDbType = SqlDbType.Int},
                    new SqlParameter() {ParameterName = "@HORAMINUTO", Value = datosProgramacion.horaMinutoPelicula, SqlDbType = SqlDbType.VarChar},
                    new SqlParameter() {ParameterName = "@SALA", Value = datosProgramacion.sala, SqlDbType = SqlDbType.VarChar},
                    new SqlParameter() {ParameterName = "@NOMBREDIA", Value = datosProgramacion.nombreDiaSemanaHorarioPelicula, SqlDbType = SqlDbType.VarChar},
                    new SqlParameter() {ParameterName = "@FRECUENCIA", Value = datosProgramacion.frecuencia, SqlDbType = SqlDbType.Int}
                };
                String sql = "SP_CREARACTUALIZARPROGRAMACIONPELICULA @IDHORARIOPELICULA,@IDFORMATO,@IDPELICULA,@IDTEATRO,@ANNOHORARIO,@MESHORARIO, @DIAHORARIO,@HORAMINUTO,@SALA, @NOMBREDIA,@FRECUENCIA";
                var i = 1;
                if (log.IsDebugEnabled) {
                    log.Debug("SQL=[" + sql + "]");
                    paramList.ForEach(p => {
                        var paramValues = "ParameterName=[" + p.ParameterName + "], Value=[" + p.Value + "], SqlDbType=[" + p.SqlDbType + "]";
                        log.Debug("Parameter " + i++ + " val=[" + paramValues + "]");
                    });
                }
                transaction = hdb.BeginTransaction("crUpdProgramacionPelicula");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, paramList.ToArray());
                rslt = 1;
            } catch (Exception ex) {
                if (log.IsFatalEnabled) {
                    log.Fatal("Exception occurred " + ex.Message);
                    log.Fatal("Exception trace=[" + ex.StackTrace + "]");
                    log.Fatal("Return sets to -1");
                }
                rslt = -1;
            } finally {
                try {
                    if (rdr != null) { rdr.Close(); }
                    if (transaction != null) { transaction.Commit(); }
                    if (hdb != null) { hdb.Close(); }
                } catch (Exception e) {
                    if (log.IsFatalEnabled) {
                        log.Fatal("Exception occurred " + e.Message);
                        log.Fatal("Exception trace=[" + e.StackTrace + "]");
                        log.Fatal("Return sets to -1");
                    }
                    rslt = -1;
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("rslt=[" + rslt + "]");
                log.Debug("createUpdateProgramacionPelicula Ends");
            }
            return rslt;
        }

        /// <summary>
        /// A core method in application. It has two tasks to make. First to update the premiere status, and second to
        /// retrieve all records scheduled for this week for billboard.
        /// </summary>
        /// <returns>A list of records for this week billboard.</returns>
        public List<MovieFullInfo> updateBillboardAndGetMovieFullInfo() {
            if (log.IsDebugEnabled) {
                log.Debug("updateBillboardAndGetMovieFullInfo Starts");
            }
            string sql = "";
            List<MovieFullInfo> movieFullList = new List<MovieFullInfo>();
            sql = "sp_actualizarcartelera";
            if (log.IsDebugEnabled) {
                log.Debug("SQL=[" + sql + "]");
            }
            HandleDatabase hdb = null;
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;

            try {
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                transaction = hdb.BeginTransaction("updBillboardAndGetMovieFullinfo");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql);
                while (rdr.Read()) {
                    movieFullList.Add(new MovieFullInfo() {
                        id = Convert.ToInt32(rdr["idPelicula"]),
                        name = rdr["nombrePelicula"].ToString(),
                        nameFull = rdr["nombrePelicula"].ToString() + " " + rdr["nombreFormato"].ToString(),
                        img = rdr["imagenCartelera"].ToString(),
                        url = rdr["urlArticuloEC"].ToString(),
                        active = rdr["activo"].ToString(),
                        premiere = rdr["premiere"].ToString(),
                        idGenre = Convert.ToInt32(rdr["idGeneroPelicula"]),
                        genre = rdr["nombreGenero"].ToString(),
                        idLocation = Convert.ToInt32(rdr["idTeatro"]),
                        nameLocation = rdr["nombreCine"].ToString(),
                        branchName = rdr["nombreTeatro"].ToString(),
                        nameFullLocation = rdr["nombreCine"].ToString() + " " + rdr["nombreTeatro"].ToString(),
                        address = rdr["direccionTeatro"].ToString(),
                        idFormat = Convert.ToInt32(rdr["idFormato"]),
                        nameFormat = rdr["nombreFormato"].ToString(),
                        idShow = Convert.ToInt32(rdr["idHorarioPelicula"]),
                        createDate = Convert.ToDateTime(rdr["fechaCreacionPelicula"]),
                        dt = new DateTime(Convert.ToInt32(rdr["annoHorarioPelicula"]), Convert.ToInt32(rdr["mesHorarioPelicula"]), Convert.ToInt32(rdr["diaHorarioPelicula"]))
                    });
                }
                // Before returning records it needs to normalize field MovieFullInfo.url 
                movieFullList.ForEach(m => {
                    if (!m.url.Contains(Settings.UrlMilleniumPrefix)) {
                        m.url = Settings.UrlMilleniumPrefix + m.url;
                    }
                });
            } catch (Exception ex) {
                if (log.IsFatalEnabled) {
                    log.Fatal("Exception occurred " + ex.Message);
                    log.Fatal("Exception trace=[" + ex.StackTrace + "]");
                    log.Fatal("Empty list returned");
                }
                movieFullList = new List<MovieFullInfo>();
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
                    movieFullList = new List<MovieFullInfo>();
                }               
            }
            if (log.IsDebugEnabled) {
                log.Debug("updateBillboardAndGetMovieFullInfo Ends");
            }
            return movieFullList;
        }

        /// <summary>
        /// Queries database to locate hours for a time period of time for a movie.
        /// </summary>
        /// <param name="id">Record to locate</param>
        /// <returns>List of hours for movie.</returns>
        public List<MovieShowHour> getMovieShowHoursFor(int id) {
            if (log.IsDebugEnabled) {
                log.Debug("getMovieShowHoursFor Starts");
            }
            string sql = "select * from tbl_hora where idHorarioPelicula = @id order by horaPelicula, minutoPelicula ";
            if (log.IsDebugEnabled) {
                log.Debug("SQL=[" + sql + "]");
            }
            List<MovieShowHour> movieHoursList = new List<MovieShowHour>();
            HandleDatabase hdb = null;
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;
            try {
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                transaction = hdb.BeginTransaction("getMovieHoursFor");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, new SqlParameter() { ParameterName = "@id", Value = id, SqlDbType = SqlDbType.BigInt });
                while (rdr.Read()) {
                    movieHoursList.Add(new MovieShowHour() {
                        id = Convert.ToInt32(rdr["idHora"]),
                        timeHour = Convert.ToInt32(rdr["horaPelicula"]),
                        timeMinute = Convert.ToInt32(rdr["minutoPelicula"])
                    });
                }

                // Let's fill field timeFull
                movieHoursList.ForEach(h => {
                    string ampm = "am";
                    int hour = h.timeHour;
                    if (hour > 12) {
                        ampm = "pm";
                        hour -= 12;
                    }
                    if (hour == 12) {
                        ampm = "pm";
                    }
                    h.timeFull = hour + ":" + h.timeMinute.ToString().PadLeft(2, '0') + " " + ampm;
                });
            } catch (Exception ex) {
                if (log.IsFatalEnabled) {
                    log.Fatal("Exception occurred " + ex.Message);
                    log.Fatal("Exception trace=[" + ex.StackTrace + "]");
                    log.Fatal("Empty list returned");
                }
                movieHoursList = new List<MovieShowHour>();
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
                    movieHoursList = new List<MovieShowHour>();
                }

            }
            if (log.IsDebugEnabled) {
                log.Debug("getMovieShowHoursFor Ends");
            }
            return movieHoursList;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Pelicula()
            : base() {
        }
    }
}