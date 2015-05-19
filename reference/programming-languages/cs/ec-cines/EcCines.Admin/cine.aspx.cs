/*==========================================================================*/
/* Source File:   CINE.ASPX.CS                                              */
/* Description:   CRUD for TBL_CINE                                         */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.18/2015                                               */
/* Last Modified: Mar.11/2015                                               */
/* Version:       1.4                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.18/2015 COQ File created.
============================================================================*/

using System;
using ELCOLOMBIANO.EcCines.Entities;
using ELCOLOMBIANO.EcCines.Entities.Dtos;

namespace EcCines {
    /// <summary>
    /// CRUD for TBL_CINE
    /// </summary>
    public partial class cine : System.Web.UI.Page {
        private bool ValidarCampos() {
            bool rslt = true;
            if (txtNombre.Text == "") {
                rslt = false;
            }
            if (txtNit.Text == "") {
                rslt = false;
            }
            return rslt;
        }

        private void CargarGridInfoData() {
            var movieList = new Cine().getCines();
            if (movieList.Count > 0) {
                btnActualizar.Visible = btnEliminar.Visible = false;
            }
            grdInfo.DataSource = movieList;
            grdInfo.SelectedIndex = -1;
            grdInfo.DataBind();
        }

        protected void Page_Load(object sender, EventArgs e) {
            if (Session["autenticado"] == null || !(bool)Session["autenticado"])
                Response.Redirect("Default.aspx");

            if (!IsPostBack) {
                btnNuevo.Visible = true;
                btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
                CargarGridInfoData();
            }
            lblMsg.Text = lblError.Text = "";
        }

        protected void OnGridInfoSelectedIndexChanged(object sender, EventArgs e) {
            Cine daoMovie = new Cine();
            var idToLocate = Convert.ToInt32(grdInfo.DataKeys[grdInfo.SelectedIndex].Value);
            CineDto r = daoMovie.getCine(idToLocate);
            if (r != null) {
                txtNombre.Text = r.nombreCine;
                txtNit.Text = r.nit;
                btnNuevo.Visible = false;
                btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = true;
            }
        }

        protected void OnGridInfoPageIndexChanging(object sender, System.Web.UI.WebControls.GridViewPageEventArgs e) {
            grdInfo.DataSource = new Cine().getCines();
            grdInfo.PageIndex = e.NewPageIndex;
            grdInfo.DataBind();
        }

        protected void OnButtonNuevo(object sender, EventArgs e) {
            lblMsg.Text = lblError.Text = "";
            if (!ValidarCampos()) {
                lblError.Text = "No ha ingresado datos para crear.";
            }
            else {
                Cine daoMovie = new Cine();
                CineDto movieInfo = new CineDto() { nombreCine = txtNombre.Text, nit = txtNit.Text, fechaCreacionCine = DateTime.Now };
                daoMovie.crearCine(movieInfo, 1);
                txtNombre.Text = txtNit.Text = "";
                btnEliminar.Visible = btnActualizar.Visible = false;
                lblMsg.Text = "Nuevo registro realizado con éxito.";
                CargarGridInfoData();
            }
        }

        protected void OnButtonEliminar(object sender, EventArgs e) {
            lblMsg.Text = lblError.Text = "";
            if (grdInfo.SelectedIndex == -1) {
                lblError.Text = "No ha seleccionado un registro para eliminar.";
            }
            else {
                Cine daoMovie = new Cine();
                var idToLocate = Convert.ToInt32(grdInfo.DataKeys[grdInfo.SelectedIndex].Value);
                CineDto r = daoMovie.getCine(idToLocate);
                if (r != null) {
                    try {
                        var rslt = daoMovie.crearCine(r, 3);
                        if (rslt == -1) {
                            lblError.Text = "El registro de cine a eliminar no se puede eliminar ya que tiene referencias en el sistema.";
                        }
                        else {
                            lblMsg.Text = "Registro eliminado con éxito.";
                        }
                    } catch (Exception) {
                        lblError.Text = "El registro de cine a eliminar no se puede eliminar ya que tiene referencias en el sistema.";
                    }
                    CargarGridInfoData();
                    txtNombre.Text = txtNit.Text = "";
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
                Cine daoMovie = new Cine();
                var idToLocate = Convert.ToInt32(grdInfo.DataKeys[grdInfo.SelectedIndex].Value);
                CineDto r = daoMovie.getCine(idToLocate);
                if (r != null) {
                    r.nombreCine = txtNombre.Text;
                    r.nit = txtNit.Text;
                    daoMovie.crearCine(r, 2);
                    CargarGridInfoData();
                    txtNombre.Text = txtNit.Text = "";
                    btnNuevo.Visible = true;
                    btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
                    lblMsg.Text = "Actualización realizada con éxito.";
                }
            }
        }

        protected void OnButtonCancelar(object sender, EventArgs e) {
            lblMsg.Text = lblError.Text = "";
            txtNombre.Text = txtNit.Text = "";
            btnNuevo.Visible = true;
            btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
            grdInfo.SelectedIndex = -1;
        }
    }
}