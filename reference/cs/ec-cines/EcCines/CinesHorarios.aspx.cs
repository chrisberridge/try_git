using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using ELCOLOMBIANO.EcCines.Entities;
using ELCOLOMBIANO.EcCines.Entities.Dtos;

namespace EcCines {
    public partial class CinesHorarios : System.Web.UI.Page {

        List<PeliculaDto> lstPeliculasPorTeatroMemoria = null;
        int teatroSeleccionado;

        protected void Page_Load(object sender, EventArgs e) {
            if (Session["autenticado"] == null || !(bool)Session["autenticado"])
                Response.Redirect("Default.aspx");
            if (IsPostBack)
                return;
            poblarTeatros();
            poblarPeliculas();
            ListBox2.SelectedIndex = 0;
            seleccionarPeliculas();
        }

        protected void submitGuardar(object sender, EventArgs e) {
            int cnt = 0;

            foreach (ListItem t in checkBoxPeliculas.Items) {
                if (t.Selected) {
                    cnt++;
                }
            }
            if (cnt > 0) {
                Response.Redirect("~/Horario.aspx");
            }
            //Msg.Text = "Debe seleccionar al menos una película";
        }

        void poblarTeatros() {
            Dictionary<string, string> list = new Dictionary<string, string>();
            Cine cineDao = new Cine();
            List<TeatroDto> listaTeatros = null;
            listaTeatros = cineDao.getTeatros();
            foreach (var item in listaTeatros) {
                list.Add(item.idTeatro.ToString(), item.nombreTeatro);
            }
            ListBox2.DataSource = list;
            ListBox2.DataTextField = "Value";
            ListBox2.DataValueField = "Key";
            ListBox2.DataBind();
        }

        void poblarPeliculas() {
            Dictionary<string, string> list = new Dictionary<string, string>();
            Pelicula peliculaDao = new Pelicula();
            PeliculaDto peliculaParametro = new PeliculaDto();
            List<DetallePeliculaDto> listaPeliculas = null;
            listaPeliculas = peliculaDao.getPeliculas(peliculaParametro);
            foreach (var item in listaPeliculas) {
                list.Add(item.idPelicula.ToString(), item.nombrePelicula);
            }

            checkBoxPeliculas.DataSource = list;
            checkBoxPeliculas.DataTextField = "Value";
            checkBoxPeliculas.DataValueField = "Key";
            checkBoxPeliculas.DataBind();
        }

        void cargarPeliculasPorTeatroEnMemoria(int teatro) {
            Pelicula peliculaDao = new Pelicula();
            lstPeliculasPorTeatroMemoria = peliculaDao.getPeliculasPorTeatro(teatro);
            Session["lstPeliculasPorTeatroMemoria"] = lstPeliculasPorTeatroMemoria;
        }

        protected void ListBox2_SelectedIndexChanged(object sender, EventArgs e) {
            seleccionarPeliculas();
        }

        void seleccionarPeliculas() {
            teatroSeleccionado = Convert.ToInt32(ListBox2.SelectedValue);
            Session["teatroSeleccionado"] = teatroSeleccionado;
            Session["nombreTeatroSeleccionado"] = ListBox2.SelectedItem;
            limpiarCheckPeliculas();
            cargarPeliculasPorTeatroEnMemoria(teatroSeleccionado);
            checkearPeliculas();
        }

        protected void checkBoxPeliculas_SelectedIndexChanged(object sender, EventArgs e) {
            string peliculas = "";
            foreach (ListItem pelicula in checkBoxPeliculas.Items) {
                if (pelicula.Selected) {
                    if (peliculas == "") {
                        peliculas = pelicula.Value.ToString();
                    }
                    else {
                        peliculas = peliculas + "," + pelicula.Value.ToString();
                    }
                }
            }
            teatroSeleccionado = Convert.ToInt32(ListBox2.SelectedValue);
            Pelicula peliculaDao = new Pelicula();
            peliculaDao.crearPeliculaTeatro(teatroSeleccionado, peliculas);
        }

        void checkearPeliculas() {
            teatroSeleccionado = Convert.ToInt32(Session["teatroSeleccionado"]);
            List<PeliculaDto> lstPeliculasPorTeatroMemoria = Session["lstPeliculasPorTeatroMemoria"] as List<PeliculaDto>;

            foreach (var item in lstPeliculasPorTeatroMemoria) {
                int i = 0;
                foreach (ListItem pelicula in checkBoxPeliculas.Items) {
                    if (pelicula.Value.Equals(item.idPelicula.ToString())) {
                        pelicula.Selected = true;
                    }
                    i = i++;
                }
            }
        }

        public void limpiarCheckPeliculas() {
            int i = 0;
            foreach (ListItem pelicula in checkBoxPeliculas.Items) {
                checkBoxPeliculas.Items[i].Selected = false;
                i = i + 1;
            }
        }

    }
}