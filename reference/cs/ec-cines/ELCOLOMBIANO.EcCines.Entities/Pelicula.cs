/*==========================================================================*/
/* Source File:   PELICULA.CS                                               */
/* Description:   Helper database access to PELICULA                        */
/* Author:        Leonardino Lima (LLIMA)                                   */
/*                Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: Mar.11/2015                                               */
/* Version:       1.5                                                       */
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
using ELCOLOMBIANO.EcCines.Entities.Dtos.Movie;

namespace ELCOLOMBIANO.EcCines.Entities {
    public class Pelicula {
        public List<DetallePeliculaDto> getPeliculas(PeliculaDto peliculaObj) {
            SqlDataReader rdr = null;
            SqlTransaction transaction = null;
            HandleDatabase hdb = null;
            try {
                List<DetallePeliculaDto> lstPelicula = new List<DetallePeliculaDto>();
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                String sql = "sp_obtenerPeliculas";
                transaction = hdb.BeginTransaction("getPeliculas");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql);
                while (rdr.Read()) {
                    DetallePeliculaDto objPelicula = new DetallePeliculaDto();
                    int idPelicula = Convert.ToInt32(rdr["idPelicula"]);
                    int idDetallePelicula = Convert.ToInt32(rdr["idDetallePelicula"]);
                    string nombrePelicula = rdr["nombrePelicula"].ToString();
                    int idUsuarioCreador = Convert.ToInt32(rdr["idUsuarioCreador"].ToString());
                    string fechaCreacionPelicula = rdr["fechaCreacionPelicula"].ToString();
                    int idGeneroPelicula = Convert.ToInt32(rdr["idGeneroPelicula"].ToString());
                    string sinopsis = rdr["sinopsis"].ToString();
                    string url = rdr["urlArticuloEC"].ToString();
                    string activo = rdr["activo"].ToString();
                    objPelicula.idPelicula = idPelicula;
                    objPelicula.idDetallePelicula = idDetallePelicula;
                    objPelicula.nombrePelicula = nombrePelicula;
                    objPelicula.idUsuarioCreador = idUsuarioCreador;
                    objPelicula.fechaCreacionPelicula = Convert.ToDateTime(fechaCreacionPelicula);
                    objPelicula.idGeneroPelicula = idGeneroPelicula;
                    objPelicula.sinopsis = sinopsis;
                    objPelicula.urlArticuloEc = url;
                    if (activo.Equals("S")) {
                        objPelicula.enCartelera = "Si";
                    }
                    else {
                        objPelicula.enCartelera = "No";
                    }
                    lstPelicula.Add(objPelicula);
                }
                return lstPelicula;
            } catch (Exception) {
                return null;
            } finally {
                if (rdr != null) { rdr.Close(); }
                if (transaction != null) { transaction.Commit(); }
                if (hdb != null) { hdb.Close(); }
            }
        }

        public List<DetallePeliculaDto> getPelicula(PeliculaDto peliculaObj) {
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;
            HandleDatabase hdb = null;
            try {
                List<DetallePeliculaDto> lstPelicula = new List<DetallePeliculaDto>();
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@IDPELICULA";
                param.Value = peliculaObj.idPelicula;
                param.SqlDbType = SqlDbType.Int;
                String sql = "sp_obtenerPelicula @IDPELICULA";
                transaction = hdb.BeginTransaction("getPelicula");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, param);
                while (rdr.Read()) {
                    DetallePeliculaDto objPelicula = new DetallePeliculaDto();
                    int idPelicula = Convert.ToInt32(rdr["idPelicula"]);
                    int idDetallePelicula = Convert.ToInt32(rdr["idDetallePelicula"]);
                    string nombrePelicula = rdr["nombrePelicula"].ToString();
                    int idUsuarioCreador = Convert.ToInt32(rdr["idUsuarioCreador"].ToString());
                    string fechaCreacionPelicula = rdr["fechaCreacionPelicula"].ToString();
                    int idGeneroPelicula = Convert.ToInt32(rdr["idGeneroPelicula"].ToString());
                    string sinopsis = rdr["sinopsis"].ToString();
                    string url = rdr["urlArticuloEC"].ToString();
                    string activo = rdr["activo"].ToString();
                    string imagenCartelera = rdr["imagenCartelera"].ToString();
                    objPelicula.idPelicula = idPelicula;
                    objPelicula.idDetallePelicula = idDetallePelicula;
                    objPelicula.nombrePelicula = nombrePelicula;
                    objPelicula.idUsuarioCreador = idUsuarioCreador;
                    objPelicula.fechaCreacionPelicula = Convert.ToDateTime(fechaCreacionPelicula);
                    objPelicula.idGeneroPelicula = idGeneroPelicula;
                    objPelicula.imagenCartelera = imagenCartelera;
                    objPelicula.sinopsis = sinopsis;
                    objPelicula.urlArticuloEc = url;
                    objPelicula.enCartelera = activo;
                    lstPelicula.Add(objPelicula);
                }
                return lstPelicula;
            } catch (Exception) {
                return null;
            } finally {
                if (rdr != null) { rdr.Close(); }
                if (transaction != null) { transaction.Commit(); }
                if (hdb != null) { hdb.Close(); }
            }
        }


        public List<PeliculaDto> getPeliculasPorTeatro(int teatro) {
            HandleDatabase hdb = null;
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;
            try {
                List<PeliculaDto> lstPeliculasCine = new List<PeliculaDto>();
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                SqlParameter paramTeatro = new SqlParameter();
                paramTeatro.ParameterName = "@TEATRO";
                paramTeatro.Value = teatro;
                paramTeatro.SqlDbType = SqlDbType.Int;
                String sql = "sp_obtenerPeliculasPorTeatro @TEATRO";
                transaction = hdb.BeginTransaction("PeliculasPorTeatro");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, paramTeatro);
                while (rdr.Read()) {
                    PeliculaDto objPeliculaCine = new PeliculaDto();
                    int idPelicula = Convert.ToInt32(rdr["idPelicula"]);
                    int idTeatro = Convert.ToInt32(rdr["idTeatro"]);
                    objPeliculaCine.idPelicula = idPelicula;
                    objPeliculaCine.idTeatro = idTeatro;
                    lstPeliculasCine.Add(objPeliculaCine);
                }
                return lstPeliculasCine;
            } catch (Exception) {
                return null;
            } finally {
                if (rdr != null) { rdr.Close(); }
                if (transaction != null) { transaction.Commit(); }
                if (hdb != null) { hdb.Close(); }
            }
        }

        public int crearPeliculaTeatro(int teatro, string peliculas) {
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;
            HandleDatabase hdb = null;
            try {
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                SqlParameter paramPeliculas = new SqlParameter();
                SqlParameter paramTeatro = new SqlParameter();
                paramPeliculas.ParameterName = "@peliculas";
                paramTeatro.ParameterName = "@teatro";
                paramPeliculas.Value = peliculas;
                paramPeliculas.SqlDbType = SqlDbType.VarChar;
                paramTeatro.Value = teatro;
                paramTeatro.SqlDbType = SqlDbType.Int;
                String sql = "sp_crearPeliculasPorTeatro @teatro,@peliculas";
                transaction = hdb.BeginTransaction("crearPeliculaPorTeatro");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, paramTeatro, paramPeliculas);
                return 0;
            } catch (Exception) {
                return -1;
            } finally {
                if (rdr != null) { rdr.Close(); }
                if (transaction != null) { transaction.Commit(); }
                if (hdb != null) { hdb.Close(); }
            }
        }

        public List<PeliculaFullInfoDto> obtenerProgramacionPelicula(int idPelicula, int idTeatro) {
            HandleDatabase hdb = null;
            SqlDataReader rdr = null;
            SqlTransaction transaction = null;
            try {
                List<PeliculaFullInfoDto> lstResultado = new List<PeliculaFullInfoDto>();
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                SqlParameter paramPelicula = new SqlParameter();
                SqlParameter paramTeatro = new SqlParameter();
                paramPelicula.ParameterName = "@idPelicula";
                paramTeatro.ParameterName = "@idTeatro";
                paramPelicula.Value = idPelicula;
                paramPelicula.SqlDbType = SqlDbType.VarChar;
                paramTeatro.Value = idTeatro;
                paramTeatro.SqlDbType = SqlDbType.Int;
                String sql = "sp_obtenerProgramacionPelicula @idPelicula,@idTeatro";
                transaction = hdb.BeginTransaction("obtenerProgramacionPelicula");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, paramPelicula, paramTeatro);
                while (rdr.Read()) {
                    PeliculaFullInfoDto peliculaObj = new PeliculaFullInfoDto();
                    int idFormato = Convert.ToInt32(rdr["idFormato"]);
                    string nombreFormato = rdr["nombreFormato"].ToString();
                    int Pelicula = Convert.ToInt32(rdr["idPelicula"]);
                    int idHorarioPelicula = Convert.ToInt32(rdr["idHorarioPelicula"]);
                    string annoHorarioPelicula = rdr["annoHorarioPelicula"].ToString();
                    string mesHorarioPelicula = rdr["mesHorarioPelicula"].ToString();
                    string diaHorarioPelicula = rdr["diaHorarioPelicula"].ToString();
                    string nombreDiaSemanaHorarioPelicula = rdr["nombreDiaSemanaHorarioPelicula"].ToString();
                    int Teatro = Convert.ToInt32(rdr["idTeatro"]);
                    string nombreTeatro = rdr["nombreTeatro"].ToString();
                    int frecuencia = Convert.ToInt32(rdr["frecuencia"]);
                    string horaPelicula = rdr["horaPelicula"].ToString();
                    string minutoPelicula = rdr["minutoPelicula"].ToString();
                    int sala = Convert.ToInt32(rdr["sala"]);
                    peliculaObj.idFormato = idFormato;
                    peliculaObj.nombreFormato = nombreFormato;
                    peliculaObj.idPelicula = Pelicula;
                    peliculaObj.idHorarioPelicula = idHorarioPelicula;
                    peliculaObj.annoHorarioPelicula = annoHorarioPelicula;
                    peliculaObj.mesHorarioPelicula = mesHorarioPelicula;
                    peliculaObj.diaHorarioPelicula = diaHorarioPelicula;
                    peliculaObj.nombreDiaSemanaHorarioPelicula = nombreDiaSemanaHorarioPelicula;
                    peliculaObj.idTeatro = Teatro;
                    peliculaObj.nombreTeatro = nombreTeatro;
                    peliculaObj.frecuencia = frecuencia;
                    peliculaObj.horaPelicula = horaPelicula;
                    peliculaObj.minutoPelicula = minutoPelicula;
                    peliculaObj.sala = sala;
                    lstResultado.Add(peliculaObj);
                }
                return lstResultado;
            } catch (Exception) {
                return null;
            } finally {
                if (rdr != null) { rdr.Close(); }
                if (transaction != null) { transaction.Commit(); }
                if (hdb != null) { hdb.Close(); }
            }
        }

        public int crearPelicula(DetallePeliculaDto pelicula, int operacion) {
            HandleDatabase hdb = null;
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;

            try {
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                SqlParameter paramOperacion = new SqlParameter();
                SqlParameter paramPelicula = new SqlParameter();
                SqlParameter paramNombre = new SqlParameter();
                SqlParameter paramUsuario = new SqlParameter();
                SqlParameter paramGenero = new SqlParameter();
                SqlParameter paramSinopsis = new SqlParameter();
                SqlParameter paramImagen = new SqlParameter();
                SqlParameter paramUrl = new SqlParameter();
                SqlParameter paramActivo = new SqlParameter();
                paramOperacion.ParameterName = "@OPERACION";
                paramPelicula.ParameterName = "@PELICULA";
                paramNombre.ParameterName = "@NOMBRE";
                paramUsuario.ParameterName = "@USUARIO";
                paramGenero.ParameterName = "@GENERO";
                paramSinopsis.ParameterName = "@SINOPSIS";
                paramImagen.ParameterName = "@IMAGEN";
                paramUrl.ParameterName = "@URL";
                paramActivo.ParameterName = "@ACTIVO";
                paramOperacion.Value = operacion;
                paramOperacion.SqlDbType = SqlDbType.Int;
                paramPelicula.Value = pelicula.idPelicula;
                paramPelicula.SqlDbType = SqlDbType.Int;
                paramNombre.Value = pelicula.nombrePelicula;
                paramNombre.SqlDbType = SqlDbType.VarChar;
                paramUsuario.Value = pelicula.idUsuarioCreador;
                paramUsuario.SqlDbType = SqlDbType.Int;
                paramGenero.Value = pelicula.idGeneroPelicula;
                paramGenero.SqlDbType = SqlDbType.Int;
                paramSinopsis.Value = pelicula.sinopsis;
                paramSinopsis.SqlDbType = SqlDbType.VarChar;
                paramImagen.Value = pelicula.imagenCartelera;
                paramImagen.SqlDbType = SqlDbType.VarChar;
                paramUrl.Value = pelicula.urlArticuloEc;
                paramUrl.SqlDbType = SqlDbType.VarChar;
                paramActivo.Value = pelicula.enCartelera;
                paramActivo.SqlDbType = SqlDbType.VarChar;
                String sql = "sp_crearActualizarPelicula @OPERACION,@PELICULA,@NOMBRE,@USUARIO,@GENERO,@SINOPSIS,@IMAGEN,@URL,@ACTIVO";
                transaction = hdb.BeginTransaction("crearPelicula");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, paramOperacion, paramPelicula, paramNombre, paramUsuario,
                                                                    paramGenero, paramSinopsis, paramImagen, paramUrl, paramActivo);
                return 0;
            } catch (Exception) {
                return -1;
            } finally {
                if (rdr != null) { rdr.Close(); }
                if (transaction != null) { transaction.Commit(); }
                if (hdb != null) { hdb.Close(); }
            }
        }


        public int crearActualizarProgramacionPelicula(ProgramacionPeliculaDto datosProgramacion) {
            HandleDatabase hdb = null;
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;
            try {
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                SqlParameter paramIdHorario = new SqlParameter();
                SqlParameter paramIdFormato = new SqlParameter();
                SqlParameter paramIdPelicula = new SqlParameter();
                SqlParameter paramIdTeatro = new SqlParameter();
                SqlParameter paramAnnoHorario = new SqlParameter();
                SqlParameter paramMesHorario = new SqlParameter();
                SqlParameter paramDiaHorario = new SqlParameter();
                SqlParameter paramHoraMinuto = new SqlParameter();
                SqlParameter paramSala = new SqlParameter();
                SqlParameter paramNombreDia = new SqlParameter();
                SqlParameter paramFrecuencia = new SqlParameter();
                paramIdHorario.ParameterName = "@IDHORARIOPELICULA";
                paramIdFormato.ParameterName = "@IDFORMATO";
                paramIdPelicula.ParameterName = "@IDPELICULA";
                paramIdTeatro.ParameterName = "@IDTEATRO";
                paramAnnoHorario.ParameterName = "@ANNOHORARIO";
                paramMesHorario.ParameterName = "@MESHORARIO";
                paramDiaHorario.ParameterName = "@DIAHORARIO";
                paramHoraMinuto.ParameterName = "@HORAMINUTO";
                paramSala.ParameterName = "@SALA";
                paramNombreDia.ParameterName = "@NOMBREDIA";
                paramFrecuencia.ParameterName = "@FRECUENCIA";
                paramIdHorario.Value = datosProgramacion.idHorarioPelicula;
                paramIdHorario.SqlDbType = SqlDbType.Int;
                paramIdFormato.Value = datosProgramacion.idFormato;
                paramIdFormato.SqlDbType = SqlDbType.Int;
                paramIdPelicula.Value = datosProgramacion.idPelicula;
                paramIdPelicula.SqlDbType = SqlDbType.Int;
                paramIdTeatro.Value = datosProgramacion.idTeatro;
                paramIdTeatro.SqlDbType = SqlDbType.Int;
                paramAnnoHorario.Value = datosProgramacion.annoHorarioPelicula;
                paramAnnoHorario.SqlDbType = SqlDbType.Int;
                paramMesHorario.Value = datosProgramacion.mesHorarioPelicula;
                paramMesHorario.SqlDbType = SqlDbType.Int;
                paramDiaHorario.Value = datosProgramacion.diaHorarioPelicula;
                paramDiaHorario.SqlDbType = SqlDbType.Int;
                paramHoraMinuto.Value = datosProgramacion.horaMinutoPelicula;
                paramHoraMinuto.SqlDbType = SqlDbType.VarChar;
                paramSala.Value = datosProgramacion.sala;
                paramSala.SqlDbType = SqlDbType.VarChar;
                paramNombreDia.Value = datosProgramacion.nombreDiaSemanaHorarioPelicula;
                paramNombreDia.SqlDbType = SqlDbType.VarChar;
                paramFrecuencia.Value = datosProgramacion.frecuencia;
                paramFrecuencia.SqlDbType = SqlDbType.Int;
                String sql = "SP_CREARACTUALIZARPROGRAMACIONPELICULA @IDHORARIOPELICULA,@IDFORMATO,@IDPELICULA,@IDTEATRO,@ANNOHORARIO,@MESHORARIO, @DIAHORARIO,@HORAMINUTO,@SALA, @NOMBREDIA,@FRECUENCIA";
                transaction = hdb.BeginTransaction("crearProgramacionPelicula");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, paramIdHorario, paramIdFormato, paramIdPelicula, paramIdTeatro,
                                                                    paramAnnoHorario, paramMesHorario, paramDiaHorario, paramHoraMinuto, paramSala,
                                                                    paramNombreDia, paramFrecuencia);
                return 0;
            } catch (Exception) {
                return -1;
            } finally {
                if (rdr != null) { rdr.Close(); }
                if (transaction != null) { transaction.Commit(); }
                if (hdb != null) { hdb.Close(); }
            }
        }

        public List<MovieFullInfo> getMovieFullinfo() {
            string sql = "";
            List<MovieFullInfo> movieFullList = new List<MovieFullInfo>();
            sql += "select * from vw_datospelicula ";
            sql += "where activo = 'S' ";
            sql += " and DATEFROMPARTS (annoHorarioPelicula, mesHorarioPelicula, diaHorarioPelicula) >= ";
            sql += "     DATEFROMPARTS (YEAR(CURRENT_TIMESTAMP), MONTH(CURRENT_TIMESTAMP), DAY(CURRENT_TIMESTAMP)) ";
            sql += "order by idPelicula, idTeatro, frecuencia, idformato ";

            HandleDatabase hdb = null;
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;

            try {
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                transaction = hdb.BeginTransaction("getMovieFullinfo");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql);
                while (rdr.Read()) {
                    movieFullList.Add(new MovieFullInfo() {
                        id = Convert.ToInt32(rdr["idPelicula"]),
                        name = rdr["nombrePelicula"].ToString(),
                        nameFull = rdr["nombrePelicula"].ToString() + " " + rdr["nombreFormato"].ToString(),
                        img = rdr["imagenCartelera"].ToString(),
                        url = rdr["urlArticuloEC"].ToString(),
                        active = rdr["activo"].ToString(),
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
                        dt = new DateTime(Convert.ToInt32(rdr["annoHorarioPelicula"]), Convert.ToInt32(rdr["mesHorarioPelicula"]), Convert.ToInt32(rdr["diaHorarioPelicula"]))
                    });
                }
            } finally {
                if (rdr != null) { rdr.Close(); }
                if (transaction != null) { transaction.Commit(); }
                if (hdb != null) { hdb.Close(); }
            }
            return movieFullList;
        }

        /// <summary>
        /// Queries database to locate hours for a time period of time for a movie.
        /// </summary>
        /// <param name="id">Record to locate</param>
        /// <returns>List of hours for movie.</returns>
        public List<MovieShowHour> getMovieShowHoursFor(int id) {
            string sql = "select * from tbl_hora where idHorarioPelicula = @id order by horaPelicula, minutoPelicula ";
            List<MovieShowHour> movieHours = new List<MovieShowHour>();
            HandleDatabase hdb = null;
            SqlTransaction transaction = null;
            SqlDataReader rdr = null;
            try {
                hdb = new HandleDatabase(Settings.Connection);
                hdb.Open();
                transaction = hdb.BeginTransaction("getMovieHoursFor");
                rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, new SqlParameter() { ParameterName = "@id", Value = id, SqlDbType = SqlDbType.BigInt });
                while (rdr.Read()) {
                    movieHours.Add(new MovieShowHour() {
                        id = Convert.ToInt32(rdr["idHora"]),
                        timeHour = Convert.ToInt32(rdr["horaPelicula"]),
                        timeMinute = Convert.ToInt32(rdr["minutoPelicula"])
                    });
                }

                // Let's fill field timeFull
                movieHours.ForEach(h => {
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
            } finally {
                if (rdr != null) { rdr.Close(); }
                if (transaction != null) { transaction.Commit(); }
                if (hdb != null) { hdb.Close(); }
            }
            return movieHours;
        }
    }
}