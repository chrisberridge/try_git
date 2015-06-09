/*==========================================================================*/
/* Source File:   TEATRO.ASPX.CS                                            */
/* Description:   CRUD for TBL_TEATRO                                       */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.21/2015                                               */
/* Last Modified: May.05/2015                                               */
/* Version:       1.9                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.21/2015 COQ File created.
============================================================================*/

using System;
using System.Collections.Generic;
using ELCOLOMBIANO.EcCines.Common;
using ELCOLOMBIANO.EcCines.Entities;
using ELCOLOMBIANO.EcCines.Entities.Dtos;
using ELCOLOMBIANO.EcCines.Web;

namespace EcCines.Admin {
    /// <summary>
    /// CRUD for TBL_CINE
    /// </summary>
    public partial class teatro : WebPageBase {
        /// <summary>
        /// Clean control values.
        /// </summary>
        private void LimpiarControles() {
            if (log.IsDebugEnabled) {
                log.Debug("LimpiarControles Starts");
            }
            listaCines.SelectedIndex = -1;
            txtNombre.Text = "";
            txtTelefono1.Text = txtTelefono2.Text = txtTelefono3.Text = "";
            listaMunicipios.SelectedIndex = -1;
            listaDepartamentos.SelectedIndex = -1;
            txtDireccion.Text = "";
            if (log.IsDebugEnabled) {
                log.Debug("");
            }
        }

        /// <summary>
        /// Logic to validate all required input control values.
        /// </summary>
        /// <returns>TRUE if all required fields are input.</returns>
        private bool ValidarCampos() {
            if (log.IsDebugEnabled) {
                log.Debug("ValidarCampos Starts");
            }
            bool rslt = true;
            if (listaCines.SelectedValue == "-1") {
                rslt = false;
            }
            if (txtNombre.Text == "") {
                return rslt;
            }
            if (listaMunicipios.SelectedValue == "-1") {
                rslt = false;
            }
            if (listaDepartamentos.SelectedValue == "-1") {
                rslt = false;
            }
            if (txtDireccion.Text == "") {
                rslt = false;
            }
            if (log.IsDebugEnabled) {
                log.Debug("ValidarCampos Ends");
                log.Debug("Result of computation is [" + rslt + "]");
            }
            return rslt;
        }

        /// <summary>
        /// Loads records for TEATRO in grid view.
        /// </summary>
        private void CargarGridInfoData() {
            if (log.IsDebugEnabled) {
                log.Debug("CargarGridInfoData Starts");
            }
            var theaterList = new Teatro().getTeatrosEx();
            if (theaterList.Count > 0) {
                btnActualizar.Visible = btnEliminar.Visible = false;
            }
            grdInfo.DataSource = theaterList;
            grdInfo.SelectedIndex = -1;
            grdInfo.DataBind();
            if (log.IsDebugEnabled) {
                log.Debug("CargarGridInfoData Ends");
            }
        }

        /// <summary>
        /// Loads combo box with CINES data.
        /// </summary>
        private void PoblarCines() {
            if (log.IsDebugEnabled) {
                log.Debug("PoblarCines Starts");
            }
            Cine daoMovie = new Cine();
            List<CineDto> movieList = daoMovie.getCines();
            List<KeyValue> l = new List<KeyValue>();
            l.Add(new KeyValue() { key = "-1", value = "-- SELECCIONE --" });
            movieList.ForEach(t => l.Add(new KeyValue() { key = t.idCine.ToString(), value = t.nombreCine }));
            listaCines.DataSource = l;
            listaCines.DataTextField = "value";
            listaCines.DataValueField = "key";
            listaCines.DataBind();
            if (log.IsDebugEnabled) {
                log.Debug("PoblarCines Ends");
            }
        }

        /// <summary>
        /// Loads Municipios in combo box.
        /// </summary>
        private void PoblarMunicipios() {
            if (log.IsDebugEnabled) {
                log.Debug("PoblarMunicipios Starts ");
            }
            Entidad daoEntity = new Entidad();
            List<EntidadDto> countyList = daoEntity.getValoresEntidad(Settings.SysParamCounty);
            List<KeyValue> l = new List<KeyValue>();
            l.Add(new KeyValue() { key = "-1", value = "-- SELECCIONE --" });
            countyList.ForEach(t => l.Add(new KeyValue { key = t.idEntidad.ToString(), value = t.valorEntidad }));
            listaMunicipios.DataSource = l;
            listaMunicipios.DataTextField = "value";
            listaMunicipios.DataValueField = "key";
            listaMunicipios.DataBind();
            if (log.IsDebugEnabled) {
                log.Debug("PoblarMunicipios Ends ");
            }
        }

        /// <summary>
        /// Loads Departamentos in combo box.
        /// </summary>
        private void PoblarDepartamentos() {
            if (log.IsDebugEnabled) {
                log.Debug("PoblarDepartamentos Starts ");
            }
            Entidad daoEntity = new Entidad();
            List<EntidadDto> countyList = daoEntity.getValoresEntidad(Settings.SysParamEstate);
            List<KeyValue> l = new List<KeyValue>();
            l.Add(new KeyValue() { key = "-1", value = "-- SELECCIONE --" });
            countyList.ForEach(t => l.Add(new KeyValue { key = t.idEntidad.ToString(), value = t.valorEntidad }));
            listaDepartamentos.DataSource = l;
            listaDepartamentos.DataTextField = "value";
            listaDepartamentos.DataValueField = "key";
            listaDepartamentos.DataBind();
            if (log.IsDebugEnabled) {
                log.Debug("PoblarDepartamentos Ends ");
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
            if (!IsPostBack) {
                PoblarCines();
                PoblarMunicipios();
                PoblarDepartamentos();
                btnNuevo.Visible = true;
                btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
                CargarGridInfoData();
            }
            if (log.IsDebugEnabled) {
                log.Debug("Page_Load Starts");
            }
        }

        /// <summary>
        /// Loads a record for one selected grid row.
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void OnGridInfoSelectedIndexChanged(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnGridInfoSelectedIndexChanged Starts");
            }
            Teatro daoTheater = new Teatro();
            var idToLocate = Convert.ToInt32(grdInfo.DataKeys[grdInfo.SelectedIndex].Value);
            TeatroDto r = daoTheater.getTeatro(idToLocate);
            if (r != null) {
                listaCines.SelectedValue = r.idCine.ToString();
                listaMunicipios.SelectedValue = r.idMunicipioTeatro.ToString();
                listaDepartamentos.SelectedValue = r.idDepeartamentoTeatro.ToString();
                txtNombre.Text = r.nombreTeatro;
                txtTelefono1.Text = r.telefono1Teatro;
                txtTelefono2.Text = r.telefono2Teatro;
                txtTelefono3.Text = r.telefono3Teatro;
                txtDireccion.Text = r.direccionTeatro;
                btnNuevo.Visible = false;
                btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = true;
            }
            if (log.IsDebugEnabled) {
                log.Debug("OnGridInfoSelectedIndexChanged Ends");
            }
        }

        /// <summary>
        /// Fires when grid changes page.
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void OnGridInfoPageIndexChanging(object sender, System.Web.UI.WebControls.GridViewPageEventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnGridInfoPageIndexChanging Starts");
            }
            grdInfo.DataSource = new Teatro().getTeatrosEx();
            grdInfo.PageIndex = e.NewPageIndex;
            grdInfo.DataBind();
            if (log.IsDebugEnabled) {
                log.Debug("OnGridInfoPageIndexChanging Ends");
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
            if (!ValidarCampos()) {
                if (log.IsDebugEnabled) {
                    log.Debug("No data info supplied");
                }
                registerToastrMsg(MessageType.Error, "No ha ingresado datos para crear.");
            }
            else {
                Teatro daoTheter = new Teatro();
                TeatroDto theaterInfo = new TeatroDto() {
                    idCine = Convert.ToInt32(listaCines.SelectedValue),
                    nombreTeatro = txtNombre.Text,
                    telefono1Teatro = txtTelefono1.Text,
                    telefono2Teatro = txtTelefono2.Text,
                    telefono3Teatro = txtTelefono3.Text,
                    idMunicipioTeatro = Convert.ToInt32(listaMunicipios.SelectedValue),
                    idDepeartamentoTeatro = Convert.ToInt32(listaDepartamentos.SelectedValue),
                    direccionTeatro = txtDireccion.Text
                };
                if (log.IsDebugEnabled) {
                    log.Debug("Record data [" + theaterInfo.ToString() + "]");
                }
                daoTheter.crearTeatro(theaterInfo, 1);
                LimpiarControles();
                btnEliminar.Visible = btnActualizar.Visible = false;
                registerToastrMsg(MessageType.Success, "Nuevo registro realizado con éxito.");
                CargarGridInfoData();
                if (log.IsDebugEnabled) {
                    log.Debug("New record created");
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonNuevo Ends");
            }
        }

        /// <summary>
        /// Removes a record from database.
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void OnButtonEliminar(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonEliminar Starts");
            }
            if (grdInfo.SelectedIndex == -1) {
                if (log.IsDebugEnabled) {
                    log.Debug("No record selected to remove");
                }
                registerToastrMsg(MessageType.Error, "No ha seleccionado un registro para eliminar.");
            }
            else {
                Teatro daoTheater = new Teatro();
                var idToLocate = Convert.ToInt32(grdInfo.DataKeys[grdInfo.SelectedIndex].Value);
                if (log.IsDebugEnabled) {
                    log.Debug("idToLocate [" + idToLocate+ "]");
                }
                TeatroDto r = daoTheater.getTeatro(idToLocate);
                if (r == null) {
                    if (log.IsDebugEnabled) {
                        log.Debug("Record data to remove not found");
                    }
                }
                if (r != null) {
                    if (log.IsDebugEnabled) {
                        log.Debug("Record data to remove [" + r.ToString() + "]");
                    }
                    try {
                        var rslt = daoTheater.crearTeatro(r, 3);
                        if (rslt == -1) {
                            if (log.IsDebugEnabled) {
                                log.Debug("Record cannot be removed as relationships would be broken");
                            }
                            registerToastrMsg(MessageType.Error, "El registro de teatro a eliminar no se puede eliminar ya que tiene referencias en el sistema.");
                        }
                        else {
                            if (log.IsDebugEnabled) {
                                log.Debug("Record removed");
                            }
                            registerToastrMsg(MessageType.Success, "Registro eliminado con éxito.");
                        }
                    } catch (Exception) {
                        if (log.IsDebugEnabled) {
                            log.Debug("Record cannot be removed as relationships would be broken");
                        }
                        registerToastrMsg(MessageType.Error, "El registro de teatro a eliminar no se puede eliminar ya que tiene referencias en el sistema.");
                    }
                    CargarGridInfoData();
                    LimpiarControles();
                    btnNuevo.Visible = true;
                    btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonEliminar Ends");
            }
        }

        /// <summary>
        /// Updates a record to database
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void OnButtonActualizar(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonEliminar Starts");
            }
            if (!ValidarCampos()) {
                if (log.IsDebugEnabled) {
                    log.Debug("No record info to update");
                }
                registerToastrMsg(MessageType.Error, "No ha ingresado datos para actualizar.");
            }
            else {
                Teatro daoTheater = new Teatro();
                var idToLocate = Convert.ToInt32(grdInfo.DataKeys[grdInfo.SelectedIndex].Value);
                if (log.IsDebugEnabled) {
                    log.Debug("Record id to locate [" +  idToLocate + "]");
                }
                TeatroDto r = daoTheater.getTeatro(idToLocate);
                if (r == null) {
                    if (log.IsDebugEnabled) {
                        log.Debug("Record data not found to update");
                    }
                }
                if (r != null) {
                    r.idCine = Convert.ToInt32(listaCines.SelectedValue);
                    r.idMunicipioTeatro = Convert.ToInt32(listaMunicipios.SelectedValue);
                    r.idDepeartamentoTeatro = Convert.ToInt32(listaDepartamentos.SelectedValue);
                    r.nombreTeatro = txtNombre.Text;
                    r.telefono1Teatro = txtTelefono1.Text;
                    r.telefono2Teatro = txtTelefono2.Text;
                    r.telefono3Teatro = txtTelefono3.Text;
                    r.direccionTeatro = txtDireccion.Text;
                    if (log.IsDebugEnabled) {
                        log.Debug("Record data [" + r.ToString() + "]");
                    }
                    daoTheater.crearTeatro(r, 2);
                    CargarGridInfoData();
                    LimpiarControles();
                    btnNuevo.Visible = true;
                    btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
                    registerToastrMsg(MessageType.Success, "Actualización realizada con éxito.");
                    if (log.IsDebugEnabled) {
                        log.Debug("Record updated");
                    }
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonEliminar Ends");
            }
        }

        /// <summary>
        /// Dismiss operation.
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void OnButtonCancelar(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonCancelar Starts");
            }
            LimpiarControles();
            btnNuevo.Visible = true;
            btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
            grdInfo.SelectedIndex = -1;
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonCancelar Ends");
            }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public teatro() : base() {
        }
    }
}