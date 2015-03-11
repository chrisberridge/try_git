using System;
using System.Collections.Generic;
using System.Linq;
using ELCOLOMBIANO.EcCines.Business;
using ELCOLOMBIANO.EcCines.Common;
using ELCOLOMBIANO.EcCines.Entities;
using ELCOLOMBIANO.EcCines.Entities.Dtos;
using Newtonsoft.Json;

namespace EcCines {
    public partial class Horario : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {
            if (Session["autenticado"] == null || !(bool)Session["autenticado"])
                Response.Redirect("Default.aspx");
            if (!IsPostBack) {
                String teatroSeleccionado = Session["teatroSeleccionado"].ToString();
                if (string.IsNullOrEmpty(teatroSeleccionado))
                    Response.Redirect("~/CinesHorarios.aspx", true);
                String nombreTeatroSeleccionado = Session["nombreTeatroSeleccionado"].ToString();
                poblarPeliculas(Convert.ToInt32(teatroSeleccionado));
                Label2.Text = nombreTeatroSeleccionado;
                ListBoxPeliculas.SelectedIndex = 0;
                pintarProgramacion();
            }
        }

        private void poblarPeliculas(int teatro) {
            Dictionary<string, string> list = new Dictionary<string, string>();
            Pelicula peliculaDao = new Pelicula();
            List<PeliculaDto> listaPeliculas = null;
            listaPeliculas = peliculaDao.getPeliculasPorTeatro(teatro);
            PeliculaDto pelParametro = null;
            foreach (var item in listaPeliculas) {
                pelParametro = new PeliculaDto();
                pelParametro.idPelicula = item.idPelicula;
                List<DetallePeliculaDto> pelicula = peliculaDao.getPelicula(pelParametro);
                foreach (var item2 in pelicula) {
                    list.Add(item2.idPelicula.ToString(), item2.nombrePelicula);
                }
            }
            ListBoxPeliculas.DataSource = list;
            ListBoxPeliculas.DataTextField = "Value";
            ListBoxPeliculas.DataValueField = "Key";
            ListBoxPeliculas.DataBind();
        }

        protected void ObtenerProgramacion(object sender, EventArgs e) {
            pintarProgramacion();
        }
        protected void pintarProgramacion() {
            if (Session["Formatos"] == null) {
                var en = new Entidad();
                Session["Formatos"] = en.obtenerValoresEntidad("SISTEMA_FORMATO_PELICULA");
            }
            List<EntidadDto> formatos = (List<EntidadDto>)Session["Formatos"];
            int idPelicula = Convert.ToInt32(ListBoxPeliculas.SelectedValue);
            int idTeatro = Convert.ToInt32(Session["teatroSeleccionado"].ToString());
            List<PeliculaFullInfoDto> listaProgramacion = obtenerProgramacion(idPelicula, idTeatro);
            List<DateTime> fechas = listaProgramacion.Select(x => new DateTime(int.Parse(x.annoHorarioPelicula), int.Parse(x.mesHorarioPelicula), int.Parse(x.diaHorarioPelicula))).Distinct().OrderByDescending(x => x).ToList();
            ltFechas.Text = "";
            int cont = 1;
            foreach (var item in fechas) {
                ltFechas.Text += string.Format("<div class='fechas'>");

                List<PeliculaFullInfoDto> info_x_fecha = listaProgramacion.Where(x => (new DateTime(int.Parse(x.annoHorarioPelicula), int.Parse(x.mesHorarioPelicula), int.Parse(x.diaHorarioPelicula))) == item).ToList();
                ltFechas.Text += string.Format("<div class='fecha' ><input class='txt_fecha' type='text' value='{0}'/><span class='mas'><img id='imgExpand' title='Ver Elementos' class='imgFechas' src='images/{2}.png'/></span><span class='clon'><img class='imgFechas' title='Clonar Fecha' src='images/Clonar.png'/></span></div><div class='horas{1}'>", String.Format("{0:dddd, dd/MM/yyyy}", item), cont == 1 ? " expanded" : "", cont == 1 ? "FlechaAba" : "FlechaDer");
                foreach (var formato in formatos) {
                    var horarioPelicula = info_x_fecha.Where(x => x.idFormato == formato.idEntidad).ToList();
                    var idHorarioPelicula = horarioPelicula.Select(x => x.idHorarioPelicula).Distinct().FirstOrDefault();
                    ltFechas.Text += string.Format("<div class='formatos' idFormato='{0}' idHorarioPelicula='{2}'><span class='nombre_formato'>Formato - {1}  </span><span class='masHora'><img class='imgFechas'  title='Adicionar Hora' src='images/MasHora.png'/></span><br>", formato.idEntidad, formato.valorEntidad, idHorarioPelicula);
                    foreach (var hora in horarioPelicula) {
                        if (!string.IsNullOrEmpty(hora.horaPelicula) && !string.IsNullOrEmpty(hora.minutoPelicula))
                            ltFechas.Text += string.Format("<input class='txt_hora' type='text' value='{0}:{1}'/>", int.Parse(hora.horaPelicula) < 10 ? "0" + hora.horaPelicula : hora.horaPelicula, int.Parse(hora.minutoPelicula) < 10 ? "0" + hora.minutoPelicula : hora.minutoPelicula);
                    }
                    ltFechas.Text += "</div><hr>";  //Cierra div formato_{0}
                }
                ltFechas.Text += "</div></div>";//Cierra div horas  
                cont++;
            }
            //Llenar plantilla vacia para adicionar fecha
            foreach (var formato in formatos)
                codNuevaFecha.Value += string.Format("<div class='formatos' idFormato='{0}' idHorarioPelicula='{2}'><span class='nombre_formato'>Formato - {1}</span><span class='masHora_nuevo'><img class='imgFechas'  title='Adicionar Hora' src='images/MasHora.png'/></span><br></div><hr>", formato.idEntidad, formato.valorEntidad, 0);
            codNuevaFecha.Value = string.Format("<div class='fechas'><div class='fecha'><input class='txt_fecha' type='text' value=''/><span class='mas_nuevo'><img id='imgExpand' title='Ver Elementos' class='imgFechas' src='images/FlechaAba.png'/></span><span class='clon_nuevo'><img class='imgFechas' title='Clonar Fecha' src='images/Clonar.png'/></span></div><div class='horas expanded'>{0}</div></div>", codNuevaFecha.Value);

        }

        public List<PeliculaFullInfoDto> obtenerProgramacion(int idPelicula, int idTeatro) {
            DetalleProgramacion programacionObj;
            try {
                programacionObj = JsonConvert.DeserializeObject<DetalleProgramacion>(infoProgramacion.Value);
            } catch (Exception) {
                programacionObj = null;
            }
            infoProgramacion.Value = ListBoxPeliculas.SelectedValue;
            List<PeliculaFullInfoDto> listResultado = new List<PeliculaFullInfoDto>();
            Pelicula peliculaDao = new Pelicula();
            listResultado = peliculaDao.obtenerProgramacionPelicula(idPelicula, idTeatro);
            if (programacionObj != null)
                guardarProgramacion(programacionObj);
            return listResultado;
        }


        public void guardarProgramacion(DetalleProgramacion datosProgramacion) {
            Pelicula peliculaObj = new Pelicula();
            ProgramacionPeliculaDto programacionDto;
            if (datosProgramacion == null)
                return;
            foreach (var itemFechas in datosProgramacion.fs) {
                foreach (var itemFormatos in itemFechas.fms) {
                    programacionDto = new ProgramacionPeliculaDto();
                    programacionDto.horaMinutoPelicula = itemFormatos.h;
                    programacionDto.idHorarioPelicula = itemFormatos.idh;
                    if (!String.IsNullOrEmpty(programacionDto.horaMinutoPelicula) || programacionDto.idHorarioPelicula != 0) {
                        if (string.IsNullOrEmpty(itemFechas.f))
                            continue;
                        var s = itemFechas.f.Split('/');
                        DateTime fecha = new DateTime(int.Parse(s[2]), int.Parse(s[1]), int.Parse(s[0]));
                        programacionDto.idFormato = itemFormatos.idf;
                        programacionDto.idPelicula = datosProgramacion.id;
                        programacionDto.idTeatro = Convert.ToInt32(Session["teatroSeleccionado"].ToString());
                        programacionDto.mesHorarioPelicula = fecha.Month;
                        programacionDto.annoHorarioPelicula = fecha.Year;
                        programacionDto.diaHorarioPelicula = fecha.Day;
                        programacionDto.nombreDiaSemanaHorarioPelicula = Util.getNombreDiaEspañol(fecha.DayOfWeek.ToString());
                        programacionDto.frecuencia = Util.getNumeroDia(fecha.DayOfWeek.ToString());
                        peliculaObj.crearActualizarProgramacionPelicula(programacionDto);
                        programacionDto = null;
                    }
                }
            }
        }

        protected void OnButtonGuardar(object sender, EventArgs e) {
            int idPelicula = Convert.ToInt32(ListBoxPeliculas.SelectedValue);
            int idTeatro = Convert.ToInt32(Session["teatroSeleccionado"].ToString());
            List<PeliculaFullInfoDto> listaProgramacion = obtenerProgramacion(idPelicula, idTeatro);
            // pintarProgramacion();
            string imgPathUrl = Request.Url.Scheme + "://" + Request.Url.Authority + "/" + Settings.ImageFolder + "/";
            ManageMovieCatalog mmc = new ManageMovieCatalog(Settings.JSONFolder + @"\" + Settings.FileMovieCatalog, Settings.JSONFolder + @"\" + Settings.FileMovies, imgPathUrl.Replace(@"\", "/"));
            mmc.CompileAllMoviesSchedule();

            Response.Redirect("~/CinesHorarios.aspx");
        }
    }
}