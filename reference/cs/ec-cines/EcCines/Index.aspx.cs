/*==========================================================================*/
/* Source File:   INDEX.ASPX.CS                                             */
/* Description:   CRUD for TBL_DETALLEPELICULA                              */
/* Author:        Leonardino Lima (LLIMA)                                   */
/*                Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: Mar.04/2015                                               */
/* Version:       1.6                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 LLIMA File created.
Feb.23/2015 COQ   Init collaboration on web form.
============================================================================*/

using ELCOLOMBIANO.EcCines.Business;
using ELCOLOMBIANO.EcCines.Entities;
using ELCOLOMBIANO.EcCines.Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EcCines
{
    public partial class Index : System.Web.UI.Page
    {
        protected void PoblarDropGenero()
        {
            Entidad daoEnt = new Entidad();
            List<EntidadDto> entityList = daoEnt.obtenerValoresEntidad(Settings.SysParamGender);
            List<KeyValue> l = new List<KeyValue>();
            l.Add(new KeyValue() { key = "-1", value = "-- SELECCIONE --" });
            entityList.ForEach(t => l.Add(new KeyValue() { key = t.idEntidad.ToString(), value = t.valorEntidad }));
            listGenero.DataSource = l;
            listGenero.DataTextField = "value";
            listGenero.DataValueField = "key";
            listGenero.DataBind();
        }

        private bool ValidarControles()
        {
            bool rslt = true;

            if (nombre.Text == "")
            {
                rslt = false;
            }
            if (listGenero.SelectedValue == "-1")
            {
                rslt = false;
            }
            return rslt;
        }

        private void LimpiarCampos()
        {
            nombre.Text = "";
            url.Text = "";
            sinopsis.Text = "";
            checkEnCartelera.Checked = false;
            listGenero.SelectedValue = "-1";
            imgPoster.Visible = false;
        }

        private void PoblarCamposPelicula(DetallePeliculaDto item)
        {
            nombre.Text = item.nombrePelicula;
            url.Text = item.urlArticuloEc;
            sinopsis.Text = item.sinopsis;
            listGenero.SelectedValue = item.idGeneroPelicula.ToString();

            if (item.imagenCartelera != "")
            {
                Uri u = Request.Url;
                string imageUrl = u.Scheme + "://" + u.Authority + "/" + Settings.ImageFolder + "/" + item.imagenCartelera;

                imgPoster.ImageUrl = imageUrl.Replace(@"\", "/");
                imgPoster.Width = 221;
                imgPoster.Height = 309;
                imgPoster.Visible = true;
            }
            else
            {
                imgPoster.Visible = false;
            }
            if (item.enCartelera.Equals("S"))
            {
                checkEnCartelera.Checked = true;
            }
            else
            {
                checkEnCartelera.Checked = false;
            }
        }

        private void CargarGridInfoData(string filtro = null, Object newPage = null)
        {
            var listaPeliculas = new Pelicula().getPeliculas(new PeliculaDto());
            if (!string.IsNullOrEmpty(filtro))
            {
                listaPeliculas = listaPeliculas.Where(x => x.nombrePelicula.ToLower().Contains(filtro.ToLower())).ToList();
            }
            if (listaPeliculas.Count > 0)
            {
                btnActualizar.Visible = false;
            }
            grdInfo.DataSource = listaPeliculas;
            if (newPage != null)
            {
                grdInfo.PageIndex = Convert.ToInt32(newPage);
            }
            grdInfo.DataBind();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["autenticado"] == null || !(bool)Session["autenticado"]) Response.Redirect("Default.aspx");
            if (!IsPostBack)
            {
                btnNuevo.Visible = true;
                btnVistaPrevia.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
                PoblarDropGenero();
                CargarGridInfoData();
            }
            lblMsg.Text = lblError.Text = "";
        }

        protected void OnGridInfoSelectedIndexChanged(object sender, EventArgs e)
        {
            var idToLocate = Convert.ToInt32(grdInfo.DataKeys[grdInfo.SelectedIndex].Value);
            DetallePeliculaDto r = (new Pelicula()).getPelicula(new PeliculaDto() { idPelicula = idToLocate }).FirstOrDefault<DetallePeliculaDto>();
            idPelicula.Value = idToLocate.ToString();
            if (r != null)
            {
                PoblarCamposPelicula(r);
                btnNuevo.Visible = false;
                btnActualizar.Visible = btnCancelar.Visible = true;
                btnVistaPrevia.Visible = true;
            }
            else
            {
                grdInfo.SelectedIndex = -1;
            }
        }

        protected void OnGridInfoPageIndexChanging(object sender, System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            CargarGridInfoData(txtBuscar.Text, e.NewPageIndex);
        }

        protected void OnButtonLimpiar(object sender, EventArgs e)
        {
            txtBuscar.Text = "";
            CargarGridInfoData();
            grdInfo.SelectedIndex = -1;
        }

        protected void OnButtonActualizarCartelera(object sender, EventArgs e)
        {
            string imgPathUrl = Request.Url.Scheme + "://" + Request.Url.Authority + "/" + Settings.ImageFolder + "/";
            ManageMovieCatalog mmc = new ManageMovieCatalog(Settings.JSONFolder + @"\" + Settings.FileMovieCatalog, Settings.JSONFolder + @"\" + Settings.FileMovies, imgPathUrl.Replace(@"\", "/"));
            mmc.CompileAllMoviesSchedule();
            lblMsg.Text = "Cartelera actualizada";
        }

        protected void OnButtonFiltrar(object sender, EventArgs e)
        {
            CargarGridInfoData(txtBuscar.Text);

        }

        protected void OnButtonNuevo(object sender, EventArgs e)
        {
            lblMsg.Text = lblError.Text = "";
            if (!ValidarControles())
            {
                lblError.Text = "No ha ingresado datos para crear.";
            }
            else
            {
                int r = CrearPelicula(1);
                LimpiarCampos();
                btnNuevo.Visible = true;
                btnActualizar.Visible = btnCancelar.Visible = false;
                btnVistaPrevia.Visible = false;
                grdInfo.SelectedIndex = -1;
                if (r == -1)
                    lblError.Text = "Registro no ingresado con éxito";
                else
                    lblMsg.Text = "Nuevo registro realizado con éxito.";
                CargarGridInfoData();
            }
        }

        protected void OnButtonActualizar(object sender, EventArgs e)
        {
            lblMsg.Text = lblError.Text = "";
            if (!ValidarControles())
            {
                lblError.Text = "No ha ingresado datos para crear.";
            }
            else
            {
                CrearPelicula(2);
                LimpiarCampos();
                btnNuevo.Visible = true;
                btnActualizar.Visible = btnCancelar.Visible = false;
                btnVistaPrevia.Visible = false;
                grdInfo.SelectedIndex = -1;
                lblMsg.Text = "Registro actualizado con éxito.";
                CargarGridInfoData();
            }
            txtBuscar.Text = "";
        }

        protected void OnButtonCancelar(object sender, EventArgs e)
        {
            lblMsg.Text = lblError.Text = "";
            LimpiarCampos();
            btnNuevo.Visible = true;
            btnActualizar.Visible = btnCancelar.Visible = false;
            btnVistaPrevia.Visible = false;
            grdInfo.SelectedIndex = -1;
        }

        protected int CrearPelicula(int operacion)
        {
            Pelicula peliculaDao = new Pelicula();
            DetallePeliculaDto datosPelicula = new DetallePeliculaDto();
            if (operacion == 2)
            {
                datosPelicula = peliculaDao.getPelicula(new PeliculaDto() { idPelicula = Convert.ToInt32(grdInfo.DataKeys[grdInfo.SelectedIndex].Value) }).FirstOrDefault();
            }
            if (imgUpload.HasFile)
            {
                string imgFolder = Settings.ImageFolderSave;
                string imgName = imgFolder + @"\" + imgUpload.FileName;

                imgUpload.SaveAs(imgName);
                datosPelicula.imagenCartelera = imgUpload.FileName;
            }

            datosPelicula.nombrePelicula = nombre.Text;
            datosPelicula.idUsuarioCreador = 1;
            datosPelicula.fechaCreacionPelicula = DateTime.Now;
            datosPelicula.idGeneroPelicula = Convert.ToInt32(listGenero.SelectedValue);
            datosPelicula.sinopsis = sinopsis.Text;
            datosPelicula.urlArticuloEc = url.Text;
            if (checkEnCartelera.Checked)
            {
                datosPelicula.enCartelera = "S";
            }
            else
            {
                datosPelicula.enCartelera = "N";
            }
            int resultado = peliculaDao.crearPelicula(datosPelicula, operacion);
            if (resultado == -1)
            {
                lblError.Text = "Registro no creado con éxito.";
            }
            txtBuscar.Text = "";
            return resultado;
        }
    }
}