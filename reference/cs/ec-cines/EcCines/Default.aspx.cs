using ELCOLOMBIANO.EcCines.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EcCines
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Valida las credenciales ingresadas por el usuario y crea la autenticacion en sesion para la validación posterior
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void AutenticarUsuario(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsuario.Text) && string.IsNullOrEmpty(txtContrasena.Text))
            {
                MostrarMsg(TipoMensaje.Error, "Ingrese el usuario y la contraseña");
                txtUsuario.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtUsuario.Text))
            {
                MostrarMsg(TipoMensaje.Error, "Ingrese el usuario");
                txtUsuario.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtContrasena.Text))
            {
                MostrarMsg(TipoMensaje.Error, "Ingrese la contraseña");
                txtContrasena.Focus();
                return;
            }
            Session["autenticado"] = txtUsuario.Text == Settings.Usuario && txtContrasena.Text == Settings.Contrasena;
            if ((bool)Session["autenticado"])
            {
                Session.Timeout = 60;
                Response.Redirect("Index.aspx");
            }
            MostrarMsg(TipoMensaje.Error, "Usuario o contraseña invalidos");
        }

        /// <summary>
        /// Metodo que muestra un mensaje del tipo que se seleccione en el parametro t
        /// </summary>
        /// <param name="t">TipoMensaje</param>
        /// <param name="mensaje">string</param>
        public void MostrarMsg(TipoMensaje t, string mensaje)
        {
            switch (t)
            {
                case TipoMensaje.Informacion:
                    var js1 = string.Format("toastr.info('{0}');", mensaje);
                    ScriptManager.RegisterStartupScript(this, typeof(Page), Guid.NewGuid().ToString(), js1, true);
                    break;
                case TipoMensaje.Error:
                    var js2 = string.Format("toastr.error('{0}', 'Error');", mensaje);
                    ScriptManager.RegisterStartupScript(this, typeof(Page), Guid.NewGuid().ToString(), js2, true);
                    break;
                case TipoMensaje.Advertencia:
                    var js3 = string.Format("toastr.warning('{0}');", mensaje);
                    ScriptManager.RegisterStartupScript(this, typeof(Page), Guid.NewGuid().ToString(), js3, true);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Define los tres tipos de mensajes que se pueden mostrar
        /// </summary>
        public enum TipoMensaje
        {
            Informacion,
            Error,
            Advertencia
        }
    }
}