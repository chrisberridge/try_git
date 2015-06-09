/*==========================================================================*/
/* Source File:   CINE.ASPX.CS                                              */
/* Description:   CRUD for TBL_CINE                                         */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.18/2015                                               */
/* Last Modified: May.05/2015                                               */
/* Version:       1.7                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.18/2015 COQ File created.
============================================================================*/

using System;
using ELCOLOMBIANO.EcCines.Common;
using ELCOLOMBIANO.EcCines.Entities;
using ELCOLOMBIANO.EcCines.Entities.Dtos;
using ELCOLOMBIANO.EcCines.Web;

namespace EcCines.Admin {
    /// <summary>
    /// CRUD for TBL_CINE
    /// </summary>
    public partial class cine : WebPageBase {
        /// <summary>
        /// Validates fields.
        /// </summary>
        /// <returns>True if required fields exist</returns>
        private bool ValidarCampos() {
            if (log.IsDebugEnabled) {
                log.Debug("ValidarCampos Starts");
            }
            bool rslt = true;
            if (txtNombre.Text == "") {
                rslt = false;
            }
            if (txtNit.Text == "") {
                rslt = false;
            }
            if (log.IsDebugEnabled) {
                log.Debug("Call result [" + rslt + "]");
                log.Debug("ValidarCampos Ends");
            }
            return rslt;
        }

        /// <summary>
        /// Loads the CINE table data into a grid.
        /// </summary>
        private void CargarGridInfoData() {
            if (log.IsDebugEnabled) {
                log.Debug("CargarGridInfoData Starts");
            }
            var movieList = new Cine().getCines();
            if (movieList.Count > 0) {
                btnActualizar.Visible = btnEliminar.Visible = false;
            }
            grdInfo.DataSource = movieList;
            grdInfo.SelectedIndex = -1;
            grdInfo.DataBind();
            if (log.IsDebugEnabled) {
                log.Debug("CargarGridInfoData Ends");
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
                btnNuevo.Visible = true;
                btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
                CargarGridInfoData();
            }
            if (log.IsDebugEnabled) {
                log.Debug("Page_Load Starts");
            }
        }

        /// <summary>
        /// When grid selected index is changed, the event is fired
        /// </summary>
        /// <param name="sender">object which fires the event</param>
        /// <param name="e">Event arguments</param>
        protected void OnGridInfoSelectedIndexChanged(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnGridInfoSelectedIndexChanged Starts");
            }
            Cine daoMovie = new Cine();
            var idToLocate = Convert.ToInt32(grdInfo.DataKeys[grdInfo.SelectedIndex].Value);
            CineDto r = daoMovie.getCine(idToLocate);
            if (r != null) {
                txtNombre.Text = r.nombreCine;
                txtNit.Text = r.nit;
                btnNuevo.Visible = false;
                btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = true;
            }
            if (log.IsDebugEnabled) {
                log.Debug("OnGridInfoSelectedIndexChanged Ends");
            }
        }

        /// <summary>
        /// When grid Page index is changed, the event is fired
        /// </summary>
        /// <param name="sender">object which fires the event</param>
        /// <param name="e">Event arguments</param>
        protected void OnGridInfoPageIndexChanging(object sender, System.Web.UI.WebControls.GridViewPageEventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnGridInfoPageIndexChanging Starts");
            }
            grdInfo.DataSource = new Cine().getCines();
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
                registerToastrMsg(MessageType.Error, "No ha ingresado datos para crear.");
                if (log.IsDebugEnabled) {
                    log.Debug("No data input");
                }
            }
            else {
                Cine daoMovie = new Cine();
                CineDto movieInfo = new CineDto() { nombreCine = txtNombre.Text, nit = txtNit.Text, fechaCreacionCine = DateTime.Now };
                if (log.IsDebugEnabled) {
                    log.Debug("New Record data [" + movieInfo.ToString() + "]");
                }
                daoMovie.createCine(movieInfo, 1);
                txtNombre.Text = txtNit.Text = "";
                btnEliminar.Visible = btnActualizar.Visible = false;
                registerToastrMsg(MessageType.Success, "Nuevo registro realizado con éxito.");
                CargarGridInfoData();
                if (log.IsDebugEnabled) {
                    log.Debug("Record created with data=[" + movieInfo.ToString() + "]");
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonNuevo End");
            }
        }

        /// <summary>
        /// Event fired to remove a new record
        /// </summary>
        /// <param name="sender">object which fires the event</param>
        /// <param name="e">Event arguments</param>
        protected void OnButtonEliminar(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonEliminar Starts");
            }
            if (grdInfo.SelectedIndex == -1) {
                registerToastrMsg(MessageType.Error, "No ha seleccionado un registro para eliminar.");
                if (log.IsDebugEnabled) {
                    log.Debug("No input data to remove");
                }
            }
            else {
                Cine daoMovie = new Cine();
                var idToLocate = Convert.ToInt32(grdInfo.DataKeys[grdInfo.SelectedIndex].Value);
                if (log.IsDebugEnabled) {
                    log.Debug("id for record to delete is [" + idToLocate + "]");
                }
                CineDto r = daoMovie.getCine(idToLocate);
                if (r == null) {
                    if (log.IsDebugEnabled) {
                        log.Debug("Record to remove not found");
                    }
                }
                if (r != null) {
                    if (log.IsDebugEnabled) {
                        log.Debug("Record data to remove [" + r.ToString() + "]");
                    }
                    try {
                        var rslt = daoMovie.createCine(r, 3);
                        if (log.IsDebugEnabled) {
                            log.Debug("Record to remove =[" + r.ToString() + "]");
                        }
                        if (rslt == -1) {
                            registerToastrMsg(MessageType.Error, "El registro de cine a eliminar no se puede eliminar ya que tiene referencias en el sistema.");
                            if (log.IsDebugEnabled) {
                                log.Debug("Record has referenced data, cannot be removed");
                            }
                        }
                        else {
                            registerToastrMsg(MessageType.Success, "Registro eliminado con éxito.");
                            if (log.IsDebugEnabled) {
                                log.Debug("Record removed");
                            }
                        }
                    } catch (Exception) {
                        registerToastrMsg(MessageType.Error, "El registro de cine a eliminar no se puede eliminar ya que tiene referencias en el sistema.");
                        if (log.IsDebugEnabled) {
                            log.Debug("Record has referenced data, cannot be removed");
                        }
                    }
                    CargarGridInfoData();
                    txtNombre.Text = txtNit.Text = "";
                    btnNuevo.Visible = true;
                    btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonEliminar Ends");
            }
        }

        /// <summary>
        /// Event fired to update a new record
        /// </summary>
        /// <param name="sender">object which fires the event</param>
        /// <param name="e">Event arguments</param>
        protected void OnButtonActualizar(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonActualizar Ends");
            }
            if (!ValidarCampos()) {
                registerToastrMsg(MessageType.Error, "No ha ingresado datos para actualizar.");

            }
            else {
                Cine daoMovie = new Cine();
                var idToLocate = Convert.ToInt32(grdInfo.DataKeys[grdInfo.SelectedIndex].Value);
                CineDto r = daoMovie.getCine(idToLocate);
                if (r == null) {
                    if (log.IsDebugEnabled) {
                        log.Debug("Record not found  for id [" + idToLocate+ "]");
                    }
                }
                if (r != null) {
                    r.nombreCine = txtNombre.Text;
                    r.nit = txtNit.Text;
                    if (log.IsDebugEnabled) {
                        log.Debug("Update Record data [" + r.ToString() + "]");
                    }
                    daoMovie.createCine(r, 2);                    
                    CargarGridInfoData();
                    txtNombre.Text = txtNit.Text = "";
                    btnNuevo.Visible = true;
                    btnEliminar.Visible = btnActualizar.Visible = btnCancelar.Visible = false;
                    registerToastrMsg(MessageType.Success, "Actualización realizada con éxito.");
                    if (log.IsDebugEnabled) {
                        log.Debug("Update sucessfull");
                    }
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonActualizar Ends");
            }
        }

        /// <summary>
        /// Event fired to cancel operation
        /// </summary>
        /// <param name="sender">object which fires the event</param>
        /// <param name="e">Event arguments</param>
        protected void OnButtonCancelar(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnButtonCancelar Starts");
            }
            txtNombre.Text = txtNit.Text = "";
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
        public cine() : base() { 
        }
    }
}