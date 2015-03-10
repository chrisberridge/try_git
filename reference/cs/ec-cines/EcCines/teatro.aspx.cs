/*==========================================================================*/
/* Source File:   TEATRO.ASPX.CS                                            */
/* Description:   CRUD for TBL_TEATRO                                       */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.21/2015                                               */
/* Last Modified: Mar.04/2015                                               */
/* Version:       1.5                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.21/2015 COQ File created.
============================================================================*/

using ELCOLOMBIANO.EcCines.Business;
using ELCOLOMBIANO.EcCines.Entities;
using ELCOLOMBIANO.EcCines.Entities.Dtos;
using System;
using System.Collections.Generic;

namespace EcCines
{
    /// <summary>
    /// CRUD for TBL_CINE
    /// </summary>
    public partial class teatro : System.Web.UI.Page
    {
        private void LimpiarControles()
        {
            listaCines.SelectedIndex = -1;
            txtNombre.Text = "";
            txtTelefono1.Text = txtTelefono2.Text = txtTelefono3.Text = "";
            listaMunicipios.SelectedIndex = -1;
            listaDepartamentos.SelectedIndex = -1;
            txtDireccion.Text = "";
        }

        private bool ValidarCampos()
        {
            bool rslt = true;
            if (listaCines.SelectedValue == "-1")
            {
                rslt = false;
            }
            if (txtNombre.Text == "")
            {
                return rslt;
            }
            if (listaMunicipios.SelectedValue == "-1")
            {
                rslt = false;
            }
            if (listaDepartamentos.SelectedValue == "-1")
            {
                rslt = false;
            }
            if (txtDireccion.Text == "")
            {
                rslt = false;
            }
            return rslt;
        }

        private void CargarGridInfoData()
        {
            var theaterList = new Teatro().getTeatrosEx();
            if (theaterList.Count > 0)
            {
                btnActualizar.Visible = btnEliminar.Visible = false;
            }
            grdInfo.DataSource = theaterList;
            grdInfo.SelectedIndex = -1;
            grdInfo.DataBind();
        }

        private void PoblarCines()
        {
            Cine daoMovie = new Cine();
            List<CineDto> movieList = daoMovie.getCines();
            List<KeyValue> l = new List<KeyValue>();
            l.Add(new KeyValue() { key = "-1", value = "-- SELECCIONE --" });
            movieList.ForEach(t => l.Add(new KeyValue() { key = t.idCine.ToString(), value = t.nombreCine }));
            listaCines.DataSource = l;
            listaCines.DataTextField = "value";
            listaCines.DataValueField = "key";
            listaCines.DataBind();
        }

        private void PoblarMunicipios()
        {
            Entidad daoEntity = new Entidad();
            List<EntidadDto> countyList = daoEntity.obtenerValoresEntidad(Settings.SysParamCounty);
            List<KeyValue> l = new List<KeyValue>();
            l.Add(new KeyValue() { key = "-1", value = "-- SELECCIONE --" });
            countyList.ForEach(t => l.Add(new KeyValue { key = t.idEntidad.ToString(), value = t.valorEntidad }));
            listaMunicipios.DataSource = l;
            listaMunicipios.DataTextField = "value";
            listaMunicipios.DataValueField = "key";
            listaMunicipios.DataBind();
        }

        private void PoblarDepartamentos()
        {
            Entidad daoEntity = new Entidad();
            List<EntidadDto> countyList = daoEntity.obtenerValoresEntidad(Settings.SysParamEstate);
            List<KeyValue> l = new List<KeyValue>();
            l.Add(new KeyValue() { key = "-1", value = "-- SELECCIONE --" });
            countyList.ForEach(t => l.Add(new KeyValue { key = t.idEntidad.ToString(), value = t.valorEntidad }));
            listaDepartamentos.DataSource = l;
            listaDepartamentos.DataTextField = "value";
            listaDepartamentos.DataValueField = "key";
            listaDepartamentos.DataBind();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["autenticado"] == null || !(bool)Session["autenticado"]) Response.Redirect("Default.aspx");
            if (!IsPostBack)
            {
                PoblarCines();
                PoblarMunicipios();
                PoblarDepartamentos();
                btnNuevo.Visible = true;
                btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
                CargarGridInfoData();
            }
            lblMsg.Text = lblError.Text = "";
        }

        protected void OnGridInfoSelectedIndexChanged(object sender, EventArgs e)
        {
            Teatro daoTheater = new Teatro();
            var idToLocate = Convert.ToInt32(grdInfo.DataKeys[grdInfo.SelectedIndex].Value);
            TeatroDto r = daoTheater.getTeatro(idToLocate);
            if (r != null)
            {
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
        }

        protected void OnGridInfoPageIndexChanging(object sender, System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            grdInfo.DataSource = new Teatro().getTeatrosEx();
            grdInfo.PageIndex = e.NewPageIndex;
            grdInfo.DataBind();
        }

        protected void OnButtonNuevo(object sender, EventArgs e)
        {
            lblMsg.Text = lblError.Text = "";
            if (!ValidarCampos())
            {
                lblError.Text = "No ha ingresado datos para crear.";
            }
            else
            {
                Teatro daoTheter = new Teatro();
                TeatroDto theaterInfo = new TeatroDto()
                {
                    idCine = Convert.ToInt32(listaCines.SelectedValue),
                    nombreTeatro = txtNombre.Text,
                    telefono1Teatro = txtTelefono1.Text,
                    telefono2Teatro = txtTelefono2.Text,
                    telefono3Teatro = txtTelefono3.Text,
                    idMunicipioTeatro = Convert.ToInt32(listaMunicipios.SelectedValue),
                    idDepeartamentoTeatro = Convert.ToInt32(listaDepartamentos.SelectedValue),
                    direccionTeatro = txtDireccion.Text
                };
                daoTheter.crearTeatro(theaterInfo, 1);
                LimpiarControles();
                btnEliminar.Visible = btnActualizar.Visible = false;
                lblMsg.Text = "Nuevo registro realizado con éxito.";
                CargarGridInfoData();
            }
        }

        protected void OnButtonEliminar(object sender, EventArgs e)
        {
            lblMsg.Text = lblError.Text = "";
            if (grdInfo.SelectedIndex == -1)
            {
                lblError.Text = "No ha seleccionado un registro para eliminar.";
            }
            else
            {
                Teatro daoTheater = new Teatro();
                var idToLocate = Convert.ToInt32(grdInfo.DataKeys[grdInfo.SelectedIndex].Value);
                TeatroDto r = daoTheater.getTeatro(idToLocate);
                if (r != null)
                {
                    try
                    {
                        var rslt = daoTheater.crearTeatro(r, 3);
                        if (rslt == -1)
                        {
                            lblError.Text = "El registro de teatro a eliminar no se puede eliminar ya que tiene referencias en el sistema.";
                        }
                        else
                        {
                            lblMsg.Text = "Registro eliminado con éxito.";
                        }
                    }
                    catch (Exception)
                    {
                        lblError.Text = "El registro de teatro a eliminar no se puede eliminar ya que tiene referencias en el sistema.";
                    }
                    CargarGridInfoData();
                    LimpiarControles();
                    btnNuevo.Visible = true;
                    btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
                }
            }
        }

        protected void OnButtonActualizar(object sender, EventArgs e)
        {
            lblMsg.Text = lblError.Text = "";
            if (!ValidarCampos())
            {
                lblError.Text = "No ha ingresado datos para actualizar.";
            }
            else
            {
                Teatro daoTheater = new Teatro();
                var idToLocate = Convert.ToInt32(grdInfo.DataKeys[grdInfo.SelectedIndex].Value);
                TeatroDto r = daoTheater.getTeatro(idToLocate);
                if (r != null)
                {
                    r.idCine = Convert.ToInt32(listaCines.SelectedValue);
                    r.idMunicipioTeatro = Convert.ToInt32(listaMunicipios.SelectedValue);
                    r.idDepeartamentoTeatro = Convert.ToInt32(listaDepartamentos.SelectedValue);
                    r.nombreTeatro = txtNombre.Text;
                    r.telefono1Teatro = txtTelefono1.Text;
                    r.telefono2Teatro = txtTelefono2.Text;
                    r.telefono3Teatro = txtTelefono3.Text;
                    r.direccionTeatro = txtDireccion.Text;
                    daoTheater.crearTeatro(r, 2);
                    CargarGridInfoData();
                    LimpiarControles();
                    btnNuevo.Visible = true;
                    btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
                    lblMsg.Text = "Actualización realizada con éxito.";
                }
            }
        }

        protected void OnButtonCancelar(object sender, EventArgs e)
        {
            lblMsg.Text = lblError.Text = "";
            LimpiarControles();
            btnNuevo.Visible = true;
            btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
            grdInfo.SelectedIndex = -1;
        }
    }
}