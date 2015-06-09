/*==========================================================================*/
/* Source File:   DEFAULT.ASPX.CS                                           */
/* Description:   Login window                                              */
/* Author:        Leonardino Lima (LLIMA)                                   */
/*                Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: May.05/2015                                               */
/* Version:       1.11                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 LLIMA File created.
Mar.26/2015 COQ   Init collaboration on web form.
============================================================================*/

using System;
using ELCOLOMBIANO.EcCines.Common;
using ELCOLOMBIANO.EcCines.Web;

namespace EcCines.Admin {
    /// <summary>
    /// Login window
    /// </summary>
    public partial class Default : WebPageBase {

        /// <summary>
        /// Takes user credentials and if user match then a record is stored in session variable.        
        /// </summary>
        /// <param name="sender">Objet which sends event</param>
        /// <param name="e">event parameteres</param>
        protected void AutenticarUsuario(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("AutenticarUsuario Starts");
            }
            if (string.IsNullOrEmpty(txtUsuario.Text) && string.IsNullOrEmpty(txtContrasena.Text)) {
                registerToastrMsg(MessageType.Error, "Ingrese el usuario y la contraseña.");
                txtUsuario.Focus();
                if (log.IsDebugEnabled) {
                    log.Debug("No User/password credentials supplied");
                    log.Debug("AutenticarUsuario Starts");
                }
                return;
            }
            if (string.IsNullOrEmpty(txtUsuario.Text)) {
                registerToastrMsg(MessageType.Error, "Ingrese el usuario.");
                txtUsuario.Focus();
                if (log.IsDebugEnabled) {
                    log.Debug("Invalid User credentials");
                    log.Debug("AutenticarUsuario Starts");
                }
                return;
            }
            if (string.IsNullOrEmpty(txtContrasena.Text)) {
                registerToastrMsg(MessageType.Error, "Ingrese la contraseña.");
                txtContrasena.Focus();
                if (log.IsDebugEnabled) {
                    log.Debug("Invalid Password credentials");
                    log.Debug("AutenticarUsuario Starts");
                }
                return;
            }
            var b = txtUsuario.Text == Settings.User && txtContrasena.Text == Settings.Password;
            if (!b) {
                registerToastrMsg(MessageType.Error, "Usuario y/o contrasena no válidos. Intente nuevamente.");
                if (log.IsDebugEnabled) {
                    log.Debug("Invalid User/Pwd credentials");
                    log.Debug("AutenticarUsuario Starts");
                }
                return;
            }

            Session["autenticado"] = b;
            if ((bool)Session["autenticado"]) {
                Session.Timeout = 60;
                
                if (log.IsDebugEnabled) {
                    log.Debug("Flag for authenticate user stored in Session");                    
                }
                if (log.IsDebugEnabled) {
                    log.Debug("AutenticarUsuario Ends");
                }
                Response.Redirect("Index.aspx");
            }            
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Default()
            : base() {
        }
    }
}