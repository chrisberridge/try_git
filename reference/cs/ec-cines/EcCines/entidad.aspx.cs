/*==========================================================================*/
/* Source File:   ENTIDAD.ASPX.CS                                           */
/* Description:   CRUD for TBL_ENTIDAD                                      */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.18/2015                                               */
/* Last Modified: Mar.11/2015                                               */
/* Version:       1.3                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.18/2015 COQ File created.
============================================================================*/

using System;
using System.Collections.Generic;
using ELCOLOMBIANO.EcCines.Entities;
using ELCOLOMBIANO.EcCines.Entities.Dtos;

namespace EcCines {
    /// <summary>
    /// CRUD for TBL_ENTIDAD
    /// </summary>
    public partial class entidad : System.Web.UI.Page {
        private void CargarGridInfoData() {
            var entList = new Entidad().obtenerValoresEntidad(listaEntidades.SelectedValue);
            if (entList.Count > 0) {
                btnActualizar.Visible = btnEliminar.Visible = false;
            }
            grdInfo.DataSource = entList;
            grdInfo.SelectedIndex = -1;
            grdInfo.DataBind();
        }

        private bool ValidarCampos() {
            bool rslt = true;
            if (txtEntidad.Text == "") {
                rslt = false;
            }
            if (txtDescEntidad.Text == "") {
                rslt = false;
            }
            return rslt;
        }

        private void PoblarListaEntidades() {
            ParametroSistema daoPs = new ParametroSistema();
            List<ParametroSistemaDto> sysParamList = daoPs.ObtenerValoresParametroSistema();
            List<KeyValue> l = new List<KeyValue>();
            l.Add(new KeyValue() { key = "-1", value = "-- SELECCIONE --" });
            sysParamList.ForEach(sp => l.Add(new KeyValue() { key = sp.nombreParametro, value = sp.descValorParametro }));
            listaEntidades.DataSource = l;
            listaEntidades.DataTextField = "value";
            listaEntidades.DataValueField = "key";
            listaEntidades.DataBind();
        }

        protected void Page_Load(object sender, EventArgs e) {
            if (Session["autenticado"] == null || !(bool)Session["autenticado"])
                Response.Redirect("Default.aspx");
            if (!IsPostBack) {
                PoblarListaEntidades();
                btnNuevo.Visible = btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
            }
            lblMsg.Text = lblError.Text = "";
        }

        protected void OnListaEntidadesIndexChanged(object sender, EventArgs e) {
            CargarGridInfoData();
            btnNuevo.Visible = (listaEntidades.SelectedValue != "-1");
        }

        protected void OnGridInfoSelectedIndexChanged(object sender, EventArgs e) {
            Entidad entDao = new Entidad();
            var idToLocate = Convert.ToInt32(grdInfo.DataKeys[grdInfo.SelectedIndex].Value);
            EntidadDto r = entDao.obtenerValorEntidad(idToLocate);
            if (r != null) {
                txtEntidad.Text = r.valorEntidad;
                txtDescEntidad.Text = r.descripcionEntidad;
                btnNuevo.Visible = false;
                btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = true;
            }
        }

        protected void OnGridInfoPageIndexChanging(object sender, System.Web.UI.WebControls.GridViewPageEventArgs e) {
            grdInfo.DataSource = new Entidad().obtenerValoresEntidad(listaEntidades.SelectedValue);
            grdInfo.PageIndex = e.NewPageIndex;
            grdInfo.DataBind();
        }

        protected void OnButtonNuevo(object sender, EventArgs e) {
            lblMsg.Text = lblError.Text = "";
            if (!ValidarCampos()) {
                if (listaEntidades.SelectedValue == "-1") {
                    lblError.Text = "Debe seleccionar un valor de entidad para crear un nuevo registro.";
                }
                else {
                    lblError.Text = "No ha ingresado datos para crear.";
                }
            }
            else {
                ParametroSistema daoPs = new ParametroSistema();
                ParametroSistemaDto ps = daoPs.ObtenerValorParametroSistema(listaEntidades.SelectedValue.ToString());

                if (ps != null) {
                    Entidad daoEnt = new Entidad();
                    EntidadDto entInfo = new EntidadDto() { idEntidad = 0, codEntidad = Convert.ToInt32(ps.valorParametro), nombreEntidad = ps.descValorParametro, valorEntidad = txtEntidad.Text, descripcionEntidad = txtDescEntidad.Text };
                    daoEnt.crearEntidad(entInfo, 1);
                    CargarGridInfoData();
                    txtDescEntidad.Text = txtEntidad.Text = "";
                    btnNuevo.Visible = true;
                    btnEliminar.Visible = btnActualizar.Visible = false;
                    lblMsg.Text = "Nuevo registro realizado con éxito.";
                }
            }
        }

        protected void OnButtonEliminar(object sender, EventArgs e) {
            lblMsg.Text = lblError.Text = "";
            if (grdInfo.SelectedIndex == -1) {
                lblError.Text = "No ha seleccionado un registro para eliminar.";
            }
            else {
                Entidad daoEnt = new Entidad();
                var idToLocate = Convert.ToInt32(grdInfo.DataKeys[grdInfo.SelectedIndex].Value);
                EntidadDto r = daoEnt.obtenerValorEntidad(idToLocate);
                if (r != null) {
                    try {
                        var rslt = daoEnt.crearEntidad(r, 3);
                        if (rslt == -1) {
                            lblError.Text = "El registro de entidad a eliminar no se puede eliminar ya que tiene referencias en el sistema.";
                        }
                        else {
                            lblMsg.Text = "Registro eliminado con éxito.";
                        }
                    } catch (Exception) {
                        lblError.Text = "El registro de entidad a eliminar no se puede eliminar ya que tiene referencias en el sistema.";
                    }
                    CargarGridInfoData();
                    txtDescEntidad.Text = txtEntidad.Text = "";
                    btnNuevo.Visible = true;
                    btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
                }
            }
        }

        protected void OnButtonActualizar(object sender, EventArgs e) {
            lblMsg.Text = lblError.Text = "";
            if (!ValidarCampos()) {
                lblError.Text = "No ha ingresado datos para actualizar.";
            }
            else {
                Entidad daoEnt = new Entidad();
                var idToLocate = Convert.ToInt32(grdInfo.DataKeys[grdInfo.SelectedIndex].Value);
                EntidadDto r = daoEnt.obtenerValorEntidad(idToLocate);
                if (r != null) {
                    r.valorEntidad = txtEntidad.Text;
                    r.descripcionEntidad = txtDescEntidad.Text;
                    daoEnt.crearEntidad(r, 2);
                    CargarGridInfoData();
                    txtDescEntidad.Text = txtEntidad.Text = "";
                    btnNuevo.Visible = true;
                    btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
                    lblMsg.Text = "Actualización realizada con éxito.";
                }
            }
        }

        protected void OnButtonCancelar(object sender, EventArgs e) {
            lblMsg.Text = lblError.Text = "";
            txtDescEntidad.Text = txtEntidad.Text = "";
            btnNuevo.Visible = true;
            btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
            grdInfo.SelectedIndex = -1;
        }
    }
}