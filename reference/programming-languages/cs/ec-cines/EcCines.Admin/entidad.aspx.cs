/*==========================================================================*/
/* Source File:   ENTIDAD.ASPX.CS                                           */
/* Description:   CRUD for TBL_ENTIDAD                                      */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.18/2015                                               */
/* Last Modified: May.05/2015                                               */
/* Version:       1.6                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.18/2015 COQ File created.
============================================================================*/

using System;
using System.Collections.Generic;
using ELCOLOMBIANO.EcCines.Common;
using ELCOLOMBIANO.EcCines.Entities;
using ELCOLOMBIANO.EcCines.Entities.Dtos;
using ELCOLOMBIANO.EcCines.Web;

namespace EcCines.Admin {
    /// <summary>
    /// CRUD for TBL_ENTIDAD
    /// </summary>
    public partial class entidad : WebPageBase {
        /// <summary>
        /// Loads data into grid view
        /// </summary>
        private void CargarGridInfoData() {
            if (log.IsDebugEnabled) {
                log.Debug("CargarGridInfoData Starts");
            }
            var entList = new Entidad().getValoresEntidad(listaEntidades.SelectedValue);
            if (entList.Count > 0) {
                btnActualizar.Visible = btnEliminar.Visible = false;
            }
            grdInfo.DataSource = entList;
            grdInfo.SelectedIndex = -1;
            grdInfo.DataBind();
            if (log.IsDebugEnabled) {
                log.Debug("CargarGridInfoData Ends");
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
            if (txtEntidad.Text == "") {
                rslt = false;
            }
            if (txtDescEntidad.Text == "") {
                rslt = false;
            }
            if (log.IsDebugEnabled) {
                log.Debug("ValidarCampos Ends");
                log.Debug("Result of computation is [" + rslt + "]");
            }
            return rslt;
        }

        /// <summary>
        /// An entity list is populated to combo box indicating which
        /// kind of data it needs to CRUD.
        /// </summary>
        private void PoblarListaEntidades() {
            if (log.IsDebugEnabled) {
                log.Debug("PoblarListaEntidades Starts");
            }
            ParametroSistema daoPs = new ParametroSistema();
            List<ParametroSistemaDto> sysParamList = daoPs.getValoresParametroSistema();
            List<KeyValue> l = new List<KeyValue>();
            l.Add(new KeyValue() { key = "-1", value = "-- SELECCIONE --" });
            sysParamList.ForEach(sp => l.Add(new KeyValue() { key = sp.nombreParametro, value = sp.descValorParametro }));
            listaEntidades.DataSource = l;
            listaEntidades.DataTextField = "value";
            listaEntidades.DataValueField = "key";
            listaEntidades.DataBind();
            if (log.IsDebugEnabled) {
                log.Debug("PoblarListaEntidades Ends");
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
                PoblarListaEntidades();
                btnNuevo.Visible = btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
            }
            if (log.IsDebugEnabled) {
                log.Debug("Page_Load Starts");
            }
        }

        /// <summary>
        /// Wnen Entities list changes in grid.
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void OnListaEntidadesIndexChanged(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnListaEntidadesIndexChanged Starts");
            }
            CargarGridInfoData();
            btnNuevo.Visible = (listaEntidades.SelectedValue != "-1");
            if (log.IsDebugEnabled) {
                log.Debug("OnListaEntidadesIndexChanged Ends");
            }
        }

        /// <summary>
        /// When a select index changes.
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void OnGridInfoSelectedIndexChanged(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnGridInfoSelectedIndexChanged Starts");
            }
            Entidad entDao = new Entidad();
            var idToLocate = Convert.ToInt32(grdInfo.DataKeys[grdInfo.SelectedIndex].Value);
            EntidadDto r = entDao.getValorEntidad(idToLocate);
            if (r != null) {
                txtEntidad.Text = r.valorEntidad;
                txtDescEntidad.Text = r.descripcionEntidad;
                btnNuevo.Visible = false;
                btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = true;
            }
            if (log.IsDebugEnabled) {
                log.Debug("OnGridInfoSelectedIndexChanged Ends");
            }
        }

        /// <summary>
        /// When grid changes pages.
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void OnGridInfoPageIndexChanging(object sender, System.Web.UI.WebControls.GridViewPageEventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnGridInfoPageIndexChanging Starts");
            }
            grdInfo.DataSource = new Entidad().getValoresEntidad(listaEntidades.SelectedValue);
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
                if (listaEntidades.SelectedValue == "-1") {
                    registerToastrMsg(MessageType.Warning, "Debe seleccionar un valor de entidad para crear un nuevo registro.");
                    if (log.IsDebugEnabled) {
                        log.Debug("Must an entity value to work on");
                    }
                }
                else {
                    registerToastrMsg(MessageType.Error, "No ha ingresado datos para crear.");
                    if (log.IsDebugEnabled) {
                        log.Debug("No data input");
                    }
                }
            }
            else {
                ParametroSistema daoPs = new ParametroSistema();
                ParametroSistemaDto ps = daoPs.getValorParametroSistema(listaEntidades.SelectedValue.ToString());

                if (log.IsDebugEnabled) {
                    log.Debug("Record data to work on [" + listaEntidades.SelectedValue.ToString() + "]");
                }
                if (ps == null) {
                    if (log.IsDebugEnabled) {
                        log.Debug("Record data not found");
                    }
                }
                if (ps != null) {                    
                    Entidad daoEnt = new Entidad();
                    EntidadDto entInfo = new EntidadDto() { idEntidad = 0, codEntidad = Convert.ToInt32(ps.valorParametro), nombreEntidad = ps.descValorParametro, valorEntidad = txtEntidad.Text, descripcionEntidad = txtDescEntidad.Text };
                    if (log.IsDebugEnabled) {
                        log.Debug("Record data [" + entInfo.ToString() + "]");
                    }
                    daoEnt.createEntidad(entInfo, 1);
                    CargarGridInfoData();
                    txtDescEntidad.Text = txtEntidad.Text = "";
                    btnNuevo.Visible = true;
                    btnEliminar.Visible = btnActualizar.Visible = false;
                    registerToastrMsg(MessageType.Success, "Nuevo registro realizado con éxito.");
                    if (log.IsDebugEnabled) {
                        log.Debug("New record created");
                    }
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonNuevo Ends");
            }
        }

        /// <summary>
        /// Record to remove.
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void OnButtonEliminar(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonEliminar Starts");
            }
            if (grdInfo.SelectedIndex == -1) {
                if (log.IsDebugEnabled) {
                    log.Debug("No input data supplied");
                }
                registerToastrMsg(MessageType.Error, "No ha seleccionado un registro para eliminar.");
            }
            else {
                Entidad daoEnt = new Entidad();
                var idToLocate = Convert.ToInt32(grdInfo.DataKeys[grdInfo.SelectedIndex].Value);
                if (log.IsDebugEnabled) {
                    log.Debug("Record data id to remove [" + idToLocate + "]");
                }
                EntidadDto r = daoEnt.getValorEntidad(idToLocate);
                if (r == null) {
                    if (log.IsDebugEnabled) {
                        log.Debug("Record data to remove not found");
                    }
                }                
                if (r != null) {
                    if (log.IsDebugEnabled) {
                        log.Debug("Record to remove [" + r.ToString() + "]");
                    }
                    try {
                        var rslt = daoEnt.createEntidad(r, 3);
                        if (rslt == -1) {
                            if (log.IsDebugEnabled) {
                                log.Debug("Record cannot be removed as relationships would break");
                            }
                            registerToastrMsg(MessageType.Error, "El registro de entidad a eliminar no se puede eliminar ya que tiene referencias en el sistema.");
                        }
                        else {
                            if (log.IsDebugEnabled) {
                                log.Debug("Record removed");
                            }
                            registerToastrMsg(MessageType.Success, "Registro eliminado con éxito.");
                        }
                    } catch (Exception) {
                        if (log.IsDebugEnabled) {
                            log.Debug("Record cannot be removed as relationships would break");
                        }
                        registerToastrMsg(MessageType.Error, "El registro de entidad a eliminar no se puede eliminar ya que tiene referencias en el sistema.");
                    }
                    CargarGridInfoData();
                    txtDescEntidad.Text = txtEntidad.Text = "";
                    btnNuevo.Visible = true;
                    btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonEliminar Ends");
            }
        }

        /// <summary>
        /// Update a record
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void OnButtonActualizar(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonActualizar Starts");
            }
            if (!ValidarCampos()) {
                if (log.IsDebugEnabled) {
                    log.Debug("No input data supplied");
                }
                registerToastrMsg(MessageType.Error, "No ha ingresado datos para actualizar.");
            }
            else {
                Entidad daoEnt = new Entidad();
                var idToLocate = Convert.ToInt32(grdInfo.DataKeys[grdInfo.SelectedIndex].Value);
                if (log.IsDebugEnabled) {
                    log.Debug("Record data id to update [" + idToLocate + "]");
                }
                EntidadDto r = daoEnt.getValorEntidad(idToLocate);
                if (r == null) {
                    if (log.IsDebugEnabled) {
                        log.Debug("Record data to update not found");
                    }
                }
                if (r != null) {                   
                    r.valorEntidad = txtEntidad.Text;
                    r.descripcionEntidad = txtDescEntidad.Text;
                    if (log.IsDebugEnabled) {
                        log.Debug("Record data id to update [" + r.ToString() + "]");
                    }
                    daoEnt.createEntidad(r, 2);
                    CargarGridInfoData();
                    txtDescEntidad.Text = txtEntidad.Text = "";
                    btnNuevo.Visible = true;
                    btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
                    registerToastrMsg(MessageType.Success, "Actualización realizada con éxito.");
                    if (log.IsDebugEnabled) {
                        log.Debug("Record updated");
                    }
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonActualizar Ends");
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
            txtDescEntidad.Text = txtEntidad.Text = "";
            btnNuevo.Visible = true;
            btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
            grdInfo.SelectedIndex = -1;
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonCancelar Ends");
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public entidad()
            : base() {
        }
    }
}