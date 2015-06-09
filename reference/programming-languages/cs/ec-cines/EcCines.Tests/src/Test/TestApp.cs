/*==========================================================================*/
/* Source File:   TESTAPP.CS                                                */
/* Description:   Helper class to rudimentary tests                         */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Apr.23/2015                                               */
/* Last Modified: May.05/2015                                               */
/* Version:       1.3                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Apr.23/2015 COQ File created.
============================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ELCOLOMBIANO.EcCines.Business;
using ELCOLOMBIANO.EcCines.Common;
using ELCOLOMBIANO.EcCines.Common.Extensions;
using ELCOLOMBIANO.EcCines.Entities;
using ELCOLOMBIANO.EcCines.Entities.Dtos;
using ELCOLOMBIANO.EcCines.Entities.Dtos.Movie;
using log4net;
using Newtonsoft.Json;

namespace ELCOLOMBIANO.EcCines.Tests.Test {
    /// <summary>
    /// Helper class to rudimentary tests.
    /// </summary>
    public class TestApp {
        private DetalleProgramacion movieSchedule;
        private Int32 idTheater;
        protected readonly ILog log = null;

        /// <summary>
        /// Configuration setup.
        /// </summary>
        private void setup() {
            if (log.IsDebugEnabled) {
                log.Debug("Executing setup");
            }
            idTheater = 12;
            String fileName = Settings.JSONFolder + @"\" + "movie-schedule.json";
            String s = "";
            using (StreamReader reader = new StreamReader(fileName)) {
                s = reader.ReadToEnd();
            }
            movieSchedule = JsonConvert.DeserializeObject<DetalleProgramacion>(s);
            if (log.IsDebugEnabled) {
                log.Debug("Executing setup End");
            }
        }

        /// <summary>
        /// This method is named 'Horario.guardarProgramacion' and must be synced if needed.
        /// This method returns an int. That in Horario is void.
        /// </summary>
        /// <param name="datosProgramacion">What to save</param>
        private int saveSchedule(DetalleProgramacion datosProgramacion) {
            if (log.IsDebugEnabled) {
                log.Debug("Executing saveSchedule");
            }
            int rslt = 0;
            if (datosProgramacion == null) {
                return -1;
            }
            Pelicula peliculaDao = new Pelicula();
            ProgramacionPeliculaDto programacionDto = null;
            foreach (var itemFechas in datosProgramacion.fs) {
                foreach (var itemFormatos in itemFechas.fms) {
                    programacionDto = new ProgramacionPeliculaDto();
                    programacionDto.idHorarioPelicula = itemFormatos.idh;
                    String[] hhmm = itemFormatos.h.Split(',');
                    List<string> hhmmList = new List<string>();
                    foreach (var shhmm in hhmm) {
                        if (shhmm != "00:00") {
                            hhmmList.Add(shhmm);
                        }
                    }
                    programacionDto.horaMinutoPelicula = hhmmList.ToStringDelimited(",");
                    if (!String.IsNullOrEmpty(programacionDto.horaMinutoPelicula) || programacionDto.idHorarioPelicula != 0) {
                        if (string.IsNullOrEmpty(itemFechas.f)) {
                            continue;
                        }

                        DateTime fecha = itemFechas.f.DDMMYYYYToDateTime();
                        programacionDto.idFormato = itemFormatos.idf;
                        programacionDto.idPelicula = datosProgramacion.id;
                        programacionDto.idTeatro = idTheater;
                        programacionDto.mesHorarioPelicula = fecha.Month;
                        programacionDto.annoHorarioPelicula = fecha.Year;
                        programacionDto.diaHorarioPelicula = fecha.Day;
                        programacionDto.nombreDiaSemanaHorarioPelicula = Utils.getDayNameSpanish(fecha.DayOfWeek.ToString());
                        programacionDto.frecuencia = Utils.getDayNameNumber(fecha.DayOfWeek.ToString());
                        rslt = peliculaDao.createUpdateProgramacionPelicula(programacionDto);
                        if (log.IsDebugEnabled) {
                            log.Debug("createUpdateProgramacionPelicula rslt=["+ rslt + "] with data=[" + programacionDto.ToString() + "]");
                        }
                        programacionDto = null;
                    }
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("Executing saveSchedule End");
            }
            return rslt;            
        }

        /// <summary>
        /// Test to execute saves movieSchedule to DB. 
        /// </summary>
        public int executeSaveScheduleTest() {
            if (log.IsDebugEnabled) {
                log.Debug("Executing executeSaveScheduleTest");
            }
            return this.saveSchedule(movieSchedule);
        }

        /// <summary>
        /// Test for reading a file from JSON for Movie List and print to log as a ToString.
        /// </summary>
        public void executeLoadMovieFileAndLog() {
            if (log.IsDebugEnabled) {
                log.Debug("executeLoadMovieFileAndLog");
            }
            ManageMovieCatalog mmc = new ManageMovieCatalog(Settings.JSONFolder + @"\" + Settings.FileMovieCatalog, Settings.JSONFolder + @"\" + Settings.FileMovies, "");
            List<Movie> movieList = mmc.RetrieveMovieList();
            if (log.IsInfoEnabled) {
                movieList.ForEach(m => log.Info(m.ToString()));
                log.Info(movieList[0].ToString());
            }

            if (log.IsDebugEnabled) {
                log.Debug("executeLoadMovieFileAndLog ends");
            }
        }

        /// <summary>
        /// Creates an object instance and save only one record.
        /// </summary>
        /// <returns></returns>
        public int executeSaveScheduleSingleTest() {
            if (log.IsDebugEnabled) {
                log.Debug("Executing executeSaveScheduleSingleTest");
            }
            Pelicula peliculaDao = new Pelicula();
            ProgramacionPeliculaDto programacionDto = new ProgramacionPeliculaDto();
            int rslt = 0;
            DateTime fecha = DateTime.Now;
            programacionDto.idHorarioPelicula = 0;
            programacionDto.idFormato = 1;
            programacionDto.idPelicula = 14;
            programacionDto.idTeatro = idTheater;
            programacionDto.mesHorarioPelicula = fecha.Month;
            programacionDto.annoHorarioPelicula = fecha.Year;
            programacionDto.diaHorarioPelicula = fecha.Day;
            programacionDto.nombreDiaSemanaHorarioPelicula = Utils.getDayNameSpanish(fecha.DayOfWeek.ToString());
            programacionDto.frecuencia = Utils.getDayNameNumber(fecha.DayOfWeek.ToString());
            programacionDto.horaMinutoPelicula = "14:40,17:30,20:50";
            programacionDto.sala = 2;
            rslt = peliculaDao.createUpdateProgramacionPelicula(programacionDto);
            if (log.IsDebugEnabled) {
                log.Debug("executeSaveScheduleSingleTest rslt=[" + rslt + "] with data=[" + programacionDto.ToString() + "]");
                log.Debug("Executing executeSaveScheduleSingleTest Ends");
            }
            return rslt;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public TestApp() {
            this.log = LogManager.GetLogger(this.GetType());
            if (log.IsDebugEnabled) {
                log.Debug("Executing TestApp() Constructor");
            }
            setup();
            if (log.IsDebugEnabled) {
                log.Debug("Executing TestApp() Constructor Ends");
            }
        }
    }
}
