/*==========================================================================*/
/* Source File:   HORARIO.ASPX.CS                                           */
/* Description:   Management for hours in the schedule in movies            */
/* Author:        Leonardino Lima (LLIMA)                                   */
/*                Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: May.05/2015                                               */
/* Version:       1.12                                                      */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 LLIMA File created.
Mar.26/2015 COQ   Init collaboration on web form.
============================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using ELCOLOMBIANO.EcCines.Business;
using ELCOLOMBIANO.EcCines.Common;
using ELCOLOMBIANO.EcCines.Common.Extensions;
using ELCOLOMBIANO.EcCines.Entities;
using ELCOLOMBIANO.EcCines.Entities.Dtos;
using ELCOLOMBIANO.EcCines.Web;
using Newtonsoft.Json;

namespace EcCines.Admin {
    /// <summary>
    /// Management for hours in the schedule in movies 
    /// </summary>
    public partial class Horario : WebPageBase {
        /// <summary>
        /// Loads movies to use to make schedule on it.
        /// </summary>
        /// <param name="teatro"></param>
        private void poblarPeliculas(int teatro) {
            if (log.IsDebugEnabled) {
                log.Debug("poblarPeliculas Starts");
            }
            Pelicula peliculaDao = new Pelicula();
            List<PeliculaDto> listaPeliculas = peliculaDao.getPeliculasPorTeatro(teatro);
            listaPeliculas = listaPeliculas.OrderBy(x => x.nombrePelicula).ToList<PeliculaDto>();
            ListBoxPeliculas.DataSource = listaPeliculas;
            ListBoxPeliculas.DataTextField = "nombrePelicula";
            ListBoxPeliculas.DataValueField = "idPelicula";
            ListBoxPeliculas.DataBind();
            if (log.IsDebugEnabled) {
                log.Debug("poblarPeliculas Ends");
            }
        }

        /// <summary>
        /// Loads the schedule for movie and generates the HTML to be used as a basis for its schedule.
        /// </summary>
        private void pintarProgramacion() {
            if (log.IsDebugEnabled) {
                log.Debug("pintarProgramacion Starts");
            }
            if (Session["Formatos"] == null) {
                Session["Formatos"] = new Entidad().getValoresEntidad("SISTEMA_FORMATO_PELICULA");
            }            
            List<EntidadDto> formatos = (List<EntidadDto>)Session["Formatos"];
            int idPelicula = Convert.ToInt32(ListBoxPeliculas.SelectedValue);
            int idTeatro = Convert.ToInt32(teatroSeleccionado.Value.ToString());
            List<PeliculaFullInfoDto> listaProgramacion = obtenerProgramacion(idPelicula, idTeatro);
            List<DateTime> fechas = listaProgramacion.Select(x => new DateTime(int.Parse(x.annoHorarioPelicula), int.Parse(x.mesHorarioPelicula), int.Parse(x.diaHorarioPelicula))).Distinct().OrderByDescending(x => x).ToList();
            ltFechas.Text = "";
            int cont = 1;
            foreach (var item in fechas) {
                ltFechas.Text += string.Format("<div class='fechas'>");

                List<PeliculaFullInfoDto> info_x_fecha = listaProgramacion.Where(x => (new DateTime(int.Parse(x.annoHorarioPelicula), int.Parse(x.mesHorarioPelicula), int.Parse(x.diaHorarioPelicula))) == item).ToList();
                ltFechas.Text += string.Format("<div class='fecha' ><input class='txt_fecha' type='text' value='{0}'/><span class='mas'><img id='imgExpand' title='Ver Elementos' class='imgFechas' src='images/{2}.png'/></span><span class='clon'><img class='imgFechas' title='Clonar Fecha' src='images/Clonar.png'/></span></div><div class='horas{1}'>", String.Format("{0:dddd, dd/MM/yyyy}", item), cont == 1 ? " expanded" : "", cont == 1 ? "FlechaAba" : "FlechaDer");
                foreach (var formato in formatos) {
                    var horarioPelicula = info_x_fecha.Where(x => x.idFormato == formato.idEntidad).ToList();
                    var idHorarioPelicula = horarioPelicula.Select(x => x.idHorarioPelicula).Distinct().FirstOrDefault();
                    ltFechas.Text += string.Format("<div class='formatos' idFormato='{0}' idHorarioPelicula='{2}'><span class='nombre_formato'>Formato - {1}  </span><span class='masHora'><img class='imgFechas'  title='Adicionar Hora' src='images/MasHora.png'/></span><br>", formato.idEntidad, formato.valorEntidad, idHorarioPelicula);
                    foreach (var hora in horarioPelicula) {
                        if (!string.IsNullOrEmpty(hora.horaPelicula) && !string.IsNullOrEmpty(hora.minutoPelicula))
                            ltFechas.Text += string.Format("<input class='txt_hora' type='text' value='{0}:{1}'/>", int.Parse(hora.horaPelicula) < 10 ? "0" + hora.horaPelicula : hora.horaPelicula, int.Parse(hora.minutoPelicula) < 10 ? "0" + hora.minutoPelicula : hora.minutoPelicula);
                    }
                    ltFechas.Text += "</div><hr>";  //Cierra div formato_{0}
                }
                ltFechas.Text += "</div></div>";//Cierra div horas  
                cont++;
            }
            //Llenar plantilla vacia para adicionar fecha
            foreach (var formato in formatos) {
                codNuevaFecha.Value += string.Format("<div class='formatos' idFormato='{0}' idHorarioPelicula='{2}'><span class='nombre_formato'>Formato - {1}</span><span class='masHora_nuevo'><img class='imgFechas'  title='Adicionar Hora' src='images/MasHora.png'/></span><br></div><hr>", formato.idEntidad, formato.valorEntidad, 0);
            }
            codNuevaFecha.Value = string.Format("<div class='fechas'><div class='fecha'><input class='txt_fecha' type='text' value=''/><span class='mas_nuevo'><img id='imgExpand' title='Ver Elementos' class='imgFechas' src='images/FlechaAba.png'/></span><span class='clon_nuevo'><img class='imgFechas' title='Clonar Fecha' src='images/Clonar.png'/></span></div><div class='horas expanded'>{0}</div></div>", codNuevaFecha.Value);
            if (log.IsDebugEnabled) {
                log.Debug("HTML generated is [" + codNuevaFecha.Value + "]");
                log.Debug("pintarProgramacion Ends");
            }
        }

        /// <summary>
        /// Retrieves the schedule so far for the supplied parameters.
        /// </summary>
        /// <param name="idPelicula">Movie Id</param>
        /// <param name="idTeatro">Theater Id</param>
        /// <returns>A list of PeliculaFullInfoDto</returns>
        private List<PeliculaFullInfoDto> obtenerProgramacion(int idPelicula, int idTeatro) {
            if (log.IsDebugEnabled) {
                log.Debug("obtenerProgramacion Starts");
                log.Debug("idPelicula=[" + idPelicula + "]");
                log.Debug("idTeatro=[" + idTeatro + "]");
            }
            infoProgramacion.Value = ListBoxPeliculas.SelectedValue;
            List<PeliculaFullInfoDto> listResultado = new Pelicula().getProgramacionPelicula(idPelicula, idTeatro);
            if (log.IsDebugEnabled) {
                log.Debug("obtenerProgramacion Ends");
            }
            return listResultado;            
        }

        /// <summary>
        /// Saves the schedule for selected movie/theater.
        /// </summary>
        private void guardarHorario() {
            if (log.IsDebugEnabled) {
                log.Debug("guardarHorario Starts");
            }
            DetalleProgramacion programacionObj;
            try {
                programacionObj = JsonConvert.DeserializeObject<DetalleProgramacion>(infoProgramacion.Value);
            } catch (Exception) {
                programacionObj = null;
            }
            if (programacionObj != null) {
                if (log.IsDebugEnabled) {
                    log.Debug("JSon Format to save is [" + infoProgramacion.Value + "]");
                }
                guardarProgramacion(programacionObj);
            }
            if (log.IsDebugEnabled) {
                log.Debug("guardarHorario Ends");
            }
        }

        /// <summary>
        /// Saves all information about the schedule for movie/theater back to disk.
        /// </summary>
        /// <param name="datosProgramacion">An object representing the information to save from JSON format</param>
        private void guardarProgramacion(DetalleProgramacion datosProgramacion) {
            if (log.IsDebugEnabled) {
                log.Debug("guardarProgramacion Starts");
            }
            if (datosProgramacion == null) {
                if (log.IsDebugEnabled) {
                    log.Debug("Supplied parameter is not set");
                }
                return;
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
                        programacionDto.idTeatro = Convert.ToInt32(teatroSeleccionado.Value.ToString());
                        programacionDto.mesHorarioPelicula = fecha.Month;
                        programacionDto.annoHorarioPelicula = fecha.Year;
                        programacionDto.diaHorarioPelicula = fecha.Day;
                        programacionDto.nombreDiaSemanaHorarioPelicula = Utils.getDayNameSpanish(fecha.DayOfWeek.ToString());
                        programacionDto.frecuencia = Utils.getDayNameNumber(fecha.DayOfWeek.ToString());
                        peliculaDao.createUpdateProgramacionPelicula(programacionDto);                                    
                        programacionDto = null;
                    }
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("guardarProgramacion Starts");
            }
        }

        /// <summary>
        /// Saves current state for schedule and updates the billboard.
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void OnButtonGuardar(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonGuardar Starts");
            }
            guardarHorario();
            string imgPathUrl = Request.Url.Scheme + "://" + Request.Url.Authority + "/" + Settings.ImageFolder + "/";
            ManageMovieCatalog mmc = new ManageMovieCatalog(Settings.JSONFolder + @"\" + Settings.FileMovieCatalog, Settings.JSONFolder + @"\" + Settings.FileMovies, imgPathUrl.Replace(@"\", "/"));
            mmc.CompileAllMoviesSchedule();
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonGuardar Ends and redirects to CinesHorarios.aspx");
            }
            Response.Redirect("~/CinesHorarios.aspx");
        }

        /// <summary>
        /// Saves current schedule and loads the new one requested.
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void ObtenerProgramacion(object sender, EventArgs e) {
            guardarHorario();
            pintarProgramacion();
        }

        /// <summary>
        /// Page Load Event
        /// </summary>
        /// <param name="sender">Object which fires the event</param>
        /// <param name="e">Event argument</param>
        protected void Page_Load(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("Page_Load Starts");
            }
            if (Session["autenticado"] == null || !(bool)Session["autenticado"]) {
                if (log.IsDebugEnabled) {
                    log.Debug("Not authenticated, redirects to Default.aspx (aka login page)");
                }
                Response.Redirect("Default.aspx");
            }            
            if (!IsPostBack) {
                teatroSeleccionado.Value = Request["t"].ToString();
                lblTeatroSeleccionado.Text = Request["nt"].ToString();
                if (string.IsNullOrEmpty(teatroSeleccionado.Value)) {
                    Response.Redirect("~/CinesHorarios.aspx", true);
                }
                poblarPeliculas(Convert.ToInt32(teatroSeleccionado.Value));                
                ListBoxPeliculas.SelectedIndex = 0;
                pintarProgramacion();
            }
            if (log.IsDebugEnabled) {
                log.Debug("Page_Load Starts");
            }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Horario()
            : base() {
        }
    }
}