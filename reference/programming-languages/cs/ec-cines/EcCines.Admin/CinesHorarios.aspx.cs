/*==========================================================================*/
/* Source File:   CINESHORARIOS.ASPX.CS                                     */
/* Description:   Select a theater and movies to assign schedule later      */
/* Author:        Leonardino Lima (LLIMA)                                   */
/*                Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: May.05/2015                                               */
/* Version:       1.10                                                      */
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
using System.Web.UI.WebControls;
using ELCOLOMBIANO.EcCines.Common;
using ELCOLOMBIANO.EcCines.Entities;
using ELCOLOMBIANO.EcCines.Entities.Dtos;
using ELCOLOMBIANO.EcCines.Web;

namespace EcCines.Admin {
    /// <summary>
    /// Select a theater and movies to assign schedule later
    /// </summary>
    public partial class CinesHorarios : WebPageBase {
        /// <summary>
        /// Loads theaters in combobox
        /// </summary>
        protected void poblarTeatros() {
            if (log.IsDebugEnabled) {
                log.Debug("poblarTeatros Starts");
            }
            List<TeatroDto> listaTeatros = new Teatro().getTeatros();
            listaTeatros = listaTeatros.OrderBy(x => x.nombreTeatro).ToList<TeatroDto>();
            lbTeatros.DataSource = listaTeatros;
            lbTeatros.DataTextField = "nombreTeatro";
            lbTeatros.DataValueField = "idTeatro";
            lbTeatros.DataBind();
            if (log.IsDebugEnabled) {
                log.Debug("poblarTeatros Ends");
            }
        }

        /// <summary>
        /// Loads Active Movies to relate to a theater which use them.
        /// </summary>
        protected void poblarPeliculas() {
            if (log.IsDebugEnabled) {
                log.Debug("poblarPeliculas Starts");
            }
            List<DetallePeliculaDto> listaPeliculas = new Pelicula().getPeliculasActivas();
            listaPeliculas = listaPeliculas.OrderBy(x => x.nombrePelicula).ToList<DetallePeliculaDto>();
            checkBoxPeliculas.DataSource = listaPeliculas;
            checkBoxPeliculas.DataTextField = "nombrePelicula";
            checkBoxPeliculas.DataValueField = "idPelicula";
            checkBoxPeliculas.DataBind();
            if (log.IsDebugEnabled) {
                log.Debug("poblarPeliculas Ends ");
            }
        }

        /// <summary>
        /// Load movies for selected theater to check which movies it has selected.
        /// </summary>
        protected void checkPeliculas() {
            if (log.IsDebugEnabled) {
                log.Debug("checkPeliculas Starts");
            }
            int teatroSeleccionado = Convert.ToInt32(lbTeatros.SelectedValue);
            List<PeliculaDto> lstPeliculasPorTeatroMemoria =  new Pelicula().getPeliculasPorTeatro(teatroSeleccionado);

            foreach (var item in lstPeliculasPorTeatroMemoria) {
                foreach (ListItem pelicula in checkBoxPeliculas.Items) {
                    if (pelicula.Value.Equals(item.idPelicula.ToString())) {
                        pelicula.Selected = true;
                    }
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("checkPeliculas Ends");
            }
        }

        /// <summary>
        /// Clear checkbox flags.
        /// </summary>
        protected void limpiarCheckPeliculas() {
            if (log.IsDebugEnabled) {
                log.Debug("limpiarCheckPeliculas Starts");
            }
            int i = 0;
            foreach (ListItem pelicula in checkBoxPeliculas.Items) {
                checkBoxPeliculas.Items[i++].Selected = false;
            }
            if (log.IsDebugEnabled) {
                log.Debug("limpiarCheckPeliculas Ends");
            }
        }

        /// <summary>
        /// Select movies to be related to a  selected theater.
        /// </summary>
        protected void seleccionarPeliculas() {
            if (log.IsDebugEnabled) {
                log.Debug("seleccionarPeliculas Starts");
            }
            limpiarCheckPeliculas();
            checkPeliculas();
            if (log.IsDebugEnabled) {
                log.Debug("seleccionarPeliculas Ends");
            }
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
            if (IsPostBack) {
                if (log.IsDebugEnabled) {
                    log.Debug("Page_Load Starts");
                }
                return;
            }
            poblarTeatros();
            poblarPeliculas();
            lbTeatros.SelectedIndex = 0;
            seleccionarPeliculas();
            if (log.IsDebugEnabled) {
                log.Debug("Page_Load Starts");
            }
        }

        /// <summary>
        /// Saves the checkboxes for a movie and proceeds to create the movie schedule for selected theater.
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void submitGuardar(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("submitGuardar Starts");
            }
            int cnt = 0;

            foreach (ListItem t in checkBoxPeliculas.Items) {
                if (t.Selected) {
                    cnt++;
                }
            }
            if (cnt > 0) {
                if (log.IsDebugEnabled) {
                    log.Debug("Redirects to Horario.aspx with theater Id=[" + lbTeatros.SelectedValue + "] and theater name =[" + lbTeatros.SelectedItem + "]");
                }
                Response.Redirect("~/Horario.aspx?t=" + lbTeatros.SelectedValue + "&nt=" + lbTeatros.SelectedItem);
            }
            if (log.IsDebugEnabled) {
                log.Debug("No theater selected");
            }
            registerToastrMsg(MessageType.Warning, "Debe seleccionar al menos una película");
            if (log.IsDebugEnabled) {
                log.Debug("submitGuardar Ends");
            }
        }

        /// <summary>
        /// When a theater index is changed.
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void lbTeatros_SelectedIndexChanged(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("lbTeatros_SelectedIndexChanged Starts");
            }
            seleccionarPeliculas();
            if (log.IsDebugEnabled) {
                log.Debug("lbTeatros_SelectedIndexChanged Ends");
            }
        }

        /// <summary>
        /// When a checkbox is changed.
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void checkBoxPeliculas_SelectedIndexChanged(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("checkBoxPeliculas_SelectedIndexChanged Starts");
            }
            string peliculas = "";
            foreach (ListItem pelicula in checkBoxPeliculas.Items) {
                if (pelicula.Selected) {
                    if (peliculas == "") {
                        peliculas = pelicula.Value.ToString();
                    }
                    else {
                        peliculas = peliculas + "," + pelicula.Value.ToString();
                    }
                }
            }
            new Pelicula().createPeliculaTeatro(Convert.ToInt32(lbTeatros.SelectedValue), peliculas);
            if (log.IsDebugEnabled) {
                log.Debug("checkBoxPeliculas_SelectedIndexChanged Ends");
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public CinesHorarios()
            : base() {
        }
    }
}