/*==========================================================================*/
/* Source File:   INDEX.ASPX.CS                                             */
/* Description:   CRUD for TBL_DETALLEPELICULA                              */
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
Feb.23/2015 COQ   Init collaboration on web form.
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

namespace EcCines.Admin {
    /// <summary>
    /// Main entry point for web application
    /// </summary>
    public partial class Index : WebPageBase {
        /// <summary>
        /// Loads data info to Gender combobox
        /// </summary>
        protected void PoblarDropGenero() {
            if (log.IsDebugEnabled) {
                log.Debug("PoblarDropGenero Starts");
            }
            Entidad daoEnt = new Entidad();
            List<EntidadDto> entityList = daoEnt.getValoresEntidad(Settings.SysParamGender);
            List<KeyValue> l = new List<KeyValue>();
            l.Add(new KeyValue() { key = "-1", value = "-- SELECCIONE --" });
            entityList.ForEach(t => l.Add(new KeyValue() { key = t.idEntidad.ToString(), value = t.valorEntidad }));
            listGenero.DataSource = l;
            listGenero.DataTextField = "value";
            listGenero.DataValueField = "key";
            listGenero.DataBind();
            if (log.IsDebugEnabled) {
                log.Debug("PoblarDropGenero Ends");
            }
        }

        /// <summary>
        /// Logic to validate all required input control values.
        /// </summary>
        /// <returns>TRUE if all required fields are input.</returns>
        private bool ValidarControles() {
            if (log.IsDebugEnabled) {
                log.Debug("ValidarCampos Starts");
            }
            bool rslt = true;

            if (nombre.Text == "") {
                rslt = false;
            }
            if (listGenero.SelectedValue == "-1") {
                rslt = false;
            }
            if (log.IsDebugEnabled) {
                log.Debug("ValidarCampos Ends");
                log.Debug("Result of computation is [" + rslt + "]");
            }
            return rslt;
        }

        /// <summary>
        /// Clears input data controls.
        /// </summary>
        private void LimpiarCampos() {
            if (log.IsDebugEnabled) {
                log.Debug("LimpiarCampos Starts");
            }
            nombre.Text = "";
            url.Text = "";
            sinopsis.Text = "";
            checkEnCartelera.Checked = false;
            listGenero.SelectedValue = "-1";
            imgPoster.Visible = false;
            if (log.IsDebugEnabled) {
                log.Debug("LimpiarCampos Ends");
            }
        }

        /// <summary>
        /// Loads a movie record to edit/update/remove.
        /// </summary>
        /// <param name="item"></param>
        private void PoblarCamposPelicula(DetallePeliculaDto item) {
            if (log.IsDebugEnabled) {
                log.Debug("PoblarCamposPelicula Starts");
            }
            nombre.Text = item.nombrePelicula;
            url.Text = item.urlArticuloEc;
            sinopsis.Text = item.sinopsis;
            listGenero.SelectedValue = item.idGeneroPelicula.ToString();

            if (item.imagenCartelera != "") {
                Uri u = Request.Url;
                string imageUrl = u.Scheme + "://" + u.Authority + "/" + Settings.ImageFolder + "/" + item.imagenCartelera;

                imgPoster.ImageUrl = imageUrl.Replace(@"\", "/");
                imgPoster.Width = 221;
                imgPoster.Height = 309;
                imgPoster.Visible = true;
            }
            else {
                imgPoster.Visible = false;
            }
            if (item.enCartelera.Equals("S")) {
                checkEnCartelera.Checked = true;
            }
            else {
                checkEnCartelera.Checked = false;
            }
            if (log.IsDebugEnabled) {
                log.Debug("PoblarCamposPelicula Ends");
            }
        }

        /// <summary>
        /// Loads Movies into Grid View
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        private void CargarGridInfoData(string filtro = null, Object newPage = null) {
            if (log.IsDebugEnabled) {
                log.Debug("CargarGridInfoData Starts");
            }
            var listaPeliculas = new Pelicula().getPeliculas();
            if (!string.IsNullOrEmpty(filtro)) {
                listaPeliculas = listaPeliculas.Where(x => x.nombrePelicula.ToLower().Contains(filtro.ToLower())).ToList();
            }
            if (listaPeliculas.Count > 0) {
                btnActualizar.Visible = false;
                if (!string.IsNullOrEmpty(filtro)) {
                    if (log.IsDebugEnabled) {
                        log.Debug("Search used with filter=[" + filtro + "] with " + listaPeliculas.Count + " matches found");
                    }
                    registerToastrMsg(MessageType.Info, string.Format("Se encontraron <b>{0}</b> con el criterio de búsqueda: <b>{1}</b> ", listaPeliculas.Count, filtro.ToString()));
                }
            }
            else {
                if (!string.IsNullOrEmpty(filtro)) {
                    if (log.IsDebugEnabled) {
                        log.Debug("No records matched");
                    }
                    registerToastrMsg(MessageType.Info, "No hay películas con el criterio de búsqueda: " + filtro.ToString());
                }
            }
            grdInfo.DataSource = listaPeliculas;
            if (newPage != null) {
                grdInfo.PageIndex = Convert.ToInt32(newPage);
            }
            grdInfo.DataBind();
            if (log.IsDebugEnabled) {
                log.Debug("CargarGridInfoData Ends");
            }
        }

        /// <summary>
        /// Operation to create or update a record in database.
        /// </summary>
        /// <param name="operacion">1: create, 2: update</param>
        /// <returns>0 if successful</returns>
        protected int CrearPelicula(int operacion) {
            if (log.IsDebugEnabled) {
                log.Debug("CrearPelicula Starts");
                log.Debug("Selected operation [" + operacion + "]");
            }
            Pelicula peliculaDao = new Pelicula();
            DetallePeliculaDto datosPelicula = new DetallePeliculaDto();
            if (operacion == 1) {
                datosPelicula.fechaCreacionPelicula = DateTime.Now.MapToLocalTimeColombia();
            }
            if (operacion == 2) {
                if (log.IsDebugEnabled) {
                    log.Debug("id to update [" + grdInfo.DataKeys[grdInfo.SelectedIndex].Value + "]");
                }
                datosPelicula = peliculaDao.getPelicula(Convert.ToInt32(grdInfo.DataKeys[grdInfo.SelectedIndex].Value));
            }

            // The following code has no effect if you use Updatepanel.
            // to upload an image it must be sent first to server and then somehow referenced for inserting/updating record.
            // Left here for compatibility.
            if (imgUpload.HasFile) {
                string imgFolder = Settings.ImageFolderSave;
                string imgName = imgFolder + @"\" + imgUpload.FileName;

                imgUpload.SaveAs(imgName);
                datosPelicula.imagenCartelera = imgUpload.FileName;
            }
            else {
                if (!String.IsNullOrEmpty(posterImageName.Value)) {
                    datosPelicula.imagenCartelera = posterImageName.Value;
                }
            }

            datosPelicula.nombrePelicula = nombre.Text;
            datosPelicula.idUsuarioCreador = 1;
            datosPelicula.idGeneroPelicula = Convert.ToInt32(listGenero.SelectedValue);
            datosPelicula.sinopsis = sinopsis.Text;
            datosPelicula.urlArticuloEc = url.Text;
            if (checkEnCartelera.Checked) {
                datosPelicula.enCartelera = "S";
            }
            else {
                datosPelicula.enCartelera = "N";
            }
            if (log.IsDebugEnabled) {
                log.Debug("Record data [" + datosPelicula.ToString() + "]");
            }
            int resultado = peliculaDao.createPelicula(datosPelicula, operacion);
            if (resultado == -1) {
                if (log.IsDebugEnabled) {
                    log.Debug("Record not created");
                }
                registerToastrMsg(MessageType.Error, "Registro no creado con éxito.");
            }
            txtBuscar.Text = "";
            if (log.IsDebugEnabled) {
                log.Debug("CrearPelicula Ends");
                log.Debug("Result is [" + resultado + "]");
            }
            return resultado;
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
                btnNuevo.Visible = true;
                btnVistaPrevia.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
                PoblarDropGenero();
                CargarGridInfoData();
            }
            if (log.IsDebugEnabled) {
                log.Debug("Page_Load Starts");
            }
        }

        /// <summary>
        /// Fires when a Selected index in grid changes.
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void OnGridInfoSelectedIndexChanged(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnGridInfoSelectedIndexChanged Starts");
            }
            var idToLocate = Convert.ToInt32(grdInfo.DataKeys[grdInfo.SelectedIndex].Value);
            DetallePeliculaDto r = (new Pelicula()).getPelicula(idToLocate);
            idPelicula.Value = idToLocate.ToString();
            if (r != null) {
                PoblarCamposPelicula(r);
                btnNuevo.Visible = false;
                btnActualizar.Visible = btnCancelar.Visible = true;
                btnVistaPrevia.Visible = true;
            }
            else {
                grdInfo.SelectedIndex = -1;
            }
            if (log.IsDebugEnabled) {
                log.Debug("OnGridInfoSelectedIndexChanged Ends");
            }
        }

        /// <summary>
        /// Fires when a page on grid is changed.
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void OnGridInfoPageIndexChanging(object sender, System.Web.UI.WebControls.GridViewPageEventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnGridInfoPageIndexChanging Starts");
            }
            CargarGridInfoData(txtBuscar.Text, e.NewPageIndex);
            if (log.IsDebugEnabled) {
                log.Debug("OnGridInfoPageIndexChanging Ends");
            }
        }

        /// <summary>
        /// Clears page content for controls.
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void OnButtonLimpiar(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonLimpiar Starts");
            }
            txtBuscar.Text = "";
            CargarGridInfoData();
            grdInfo.SelectedIndex = -1;
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonLimpiar Ends");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void OnButtonActualizarCartelera(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonActualizarCartelera Starts");
            }
            string imgPathUrl = Request.Url.Scheme + "://" + Request.Url.Authority + "/" + Settings.ImageFolder + "/";
            ManageMovieCatalog mmc = new ManageMovieCatalog(Settings.JSONFolder + @"\" + Settings.FileMovieCatalog, Settings.JSONFolder + @"\" + Settings.FileMovies, imgPathUrl.Replace(@"\", "/"));
            mmc.CompileAllMoviesSchedule();
            registerToastrMsg(MessageType.Success, "Cartelera actualizada con éxito");
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonActualizarCartelera Ends");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void OnButtonFiltrar(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonFiltrar Starts");
            }
            CargarGridInfoData(txtBuscar.Text);
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonFiltrar Ends");
            }
        }

        /// <summary>
        /// Event fired to create a new record
        /// </summary>
        /// <param name="sender">object which fires the event</param>
        /// <param name="e">Event arguments</param>
        protected void OnButtonNuevo(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonNuevo Starts");
            }
            if (!ValidarControles()) {
                registerToastrMsg(MessageType.Error, "No ha ingresado datos para crear.");
            }
            else {
                int r = CrearPelicula(1);
                LimpiarCampos();
                btnNuevo.Visible = true;
                btnActualizar.Visible = btnCancelar.Visible = false;
                btnVistaPrevia.Visible = false;
                grdInfo.SelectedIndex = -1;
                if (r == -1) {
                    registerToastrMsg(MessageType.Error, "Registro no ingresado con éxito");
                    if (log.IsDebugEnabled) {
                        log.Debug("Record created");
                    }
                }
                else {
                    registerToastrMsg(MessageType.Success, "Nuevo registro realizado con éxito.");
                    if (log.IsDebugEnabled) {
                        log.Debug("Record not created");
                    }
                }
                CargarGridInfoData();
            }
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonNuevo Starts");
            }
        }

        /// <summary>
        /// Updates a record
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void OnButtonActualizar(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonActualizar Starts");
            }
            if (!ValidarControles()) {
                registerToastrMsg(MessageType.Error, "No ha ingresado datos para crear.");
            }
            else {
                CrearPelicula(2);
                LimpiarCampos();
                btnNuevo.Visible = true;
                btnActualizar.Visible = btnCancelar.Visible = false;
                btnVistaPrevia.Visible = false;
                grdInfo.SelectedIndex = -1;
                CargarGridInfoData();
                registerToastrMsg(MessageType.Success, "Registro actualizado con éxito.");
            }
            txtBuscar.Text = "";
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonActualizar Ends");
            }
        }

        /// <summary>
        /// Dismisses oeration.
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void OnButtonCancelar(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonCancelar Starts");
            }
            LimpiarCampos();
            btnNuevo.Visible = true;
            btnActualizar.Visible = btnCancelar.Visible = false;
            btnVistaPrevia.Visible = false;
            grdInfo.SelectedIndex = -1;
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonCancelar Ends");
            }
        }

        /// <summary>
        /// Uploads an image to server to be used as a poster.
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void OnButtonUploadClick(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonUploadClick Starts");
            }
            if (imgUpload.HasFile) {
                string imgFolder = Settings.ImageFolderSave;
                string imgName = imgFolder + @"\" + imgUpload.FileName;

                if (log.IsDebugEnabled) {
                    log.Debug("image named saved " +  imgName + "]");
                }
                imgUpload.SaveAs(imgName);

                Uri u = Request.Url;
                string imageUrl = u.Scheme + "://" + u.Authority + "/" + Settings.ImageFolder + "/" + imgUpload.FileName;

                imgPoster.ImageUrl = imageUrl.Replace(@"\", "/");
                imgPoster.Width = 221;
                imgPoster.Height = 309;
                imgPoster.Visible = true;

                imgPoster.ImageUrl = imageUrl;
                posterImageName.Value = imgUpload.FileName;
            }
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonUploadClick Ends");
            }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Index()
            : base() {
        }
    }
}