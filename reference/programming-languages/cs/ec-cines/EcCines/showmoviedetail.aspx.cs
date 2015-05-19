/*==========================================================================*/
/* Source File:   SHOWMOVIEDETAIL.ASPX.CS                                   */
/* Description:   Visualize a movie with its schedule                       */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/*                Christian Zapata  Vásquez (CZV)                           */
/* Date:          Feb.11/2015                                               */
/* Last Modified: Mar.11/2015                                               */
/* Version:       1.8                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
============================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using ELCOLOMBIANO.EcCines.Business;
using ELCOLOMBIANO.EcCines.Common;
using ELCOLOMBIANO.EcCines.Entities.Dtos;
using ELCOLOMBIANO.EcCines.Entities.Dtos.Movie;

namespace EcCines {
    public partial class showmoviedetail : System.Web.UI.Page {
        private List<Movie> movieList = null;
        private Movie movieToShow = null;
        private MovieCatalog mc = null;
        private string m = ""; // Movie Id, gathered in Page_Load
        private string f = ""; // Format Id, gathered in Page_Load
        private string t = ""; // Theater Id, gathered in Page_Load

        /// <summary>
        /// Fills the list of theaters that exhibit this movie.
        /// It is filtered by format when selected.
        /// </summary>
        private void FillMovieTheaterList() {
            int formatId = Convert.ToInt32(f);
            List<KeyValue> l = new List<KeyValue>();
            l.Add(new KeyValue() { key = "-1", value = "- SELECCIONE -" });

            if (movieToShow != null) {
                movieToShow.locations.ForEach(loc => {
                    // Use filter by format if it is not -1
                    if (f == "-1") {
                        l.Add(new KeyValue() { key = loc.id.ToString(), value = loc.nameFull });
                    }
                    else {
                        var formatExist = loc.formats.Where(x => x.id == formatId).FirstOrDefault<MovieFormat>();
                        if (formatExist != null)
                            l.Add(new KeyValue() { key = loc.id.ToString(), value = loc.nameFull });
                    }
                });
            }
            movieTheater.DataSource = l.OrderBy(x => x.value).ToList();
            movieTheater.DataTextField = "value";
            movieTheater.DataValueField = "key";
            try {
                movieTheater.SelectedValue = t;
                movieTheater.DataBind();
            } catch (Exception) {
                // Reset format list combo to default.
                f = "-1";
            }
        }

        /// <summary>
        /// Fills the list of formats that exhibit this movie.
        /// It is filtered by theater when selected.
        /// </summary>
        private void FillMovieFormatList() {
            int movieId = Convert.ToInt32(m);
            List<KeyValue> l = new List<KeyValue>();
            l.Add(new KeyValue() { key = "-1", value = "- SELECCIONE -" });

            if (t == "-1") {
                // Sets all formats for movie (taken from Catalog).
                var mv = mc.movies.Where(x => x.id == movieId).FirstOrDefault<MovieShortFormat>();
                if (mv != null) {
                    mv.formats.ForEach(f => l.Add(new KeyValue() { key = f.id.ToString(), value = f.name }));
                }
            }
            else {
                int theaterId = Convert.ToInt32(t);
                var movieTheaterSelected = movieToShow.locations.Where(x => x.id == theaterId).FirstOrDefault<MovieLocation>();
                movieTheaterSelected.formats.ForEach(f => l.Add(new KeyValue() { key = f.id.ToString(), value = f.name }));
            }

            movieFormat.DataSource = l.OrderBy(x => x.value).ToList<KeyValue>();
            movieFormat.DataTextField = "value";
            movieFormat.DataValueField = "key";
            try {
                movieFormat.SelectedValue = f;
                movieFormat.DataBind();
            } catch (Exception) {
                // Reset theater list combo to default.
                t = "-1";
            }
        }

        private void LoadData() {
            noscheduleAtAll.Text = "";

            if (movieToShow != null && movieToShow.name != "") {
                movieName.Text = movieToShow.name;
            }
            else {
                noscheduleAtAll.Text = "<div class='select-mc clearfix'>Esta película no está programada en los cines de la ciudad</div>";
            }
            FillMovieTheaterList();
            FillMovieFormatList();
        }

        private void LoadDetailedData() {
            string s = "";
            if (movieToShow != null) {
                DateTime now = MapToLocalTimeColombia();
                DateTime nowOnlyDate = new DateTime(now.Year, now.Month, now.Day);
                int movieTheaterId = Convert.ToInt32(t);
                int movieFormatId = Convert.ToInt32(f);
                List<MovieLocation> theaterFilterList = movieToShow.locations;
                if (movieTheaterId > 0)
                    theaterFilterList = movieToShow.locations.Where(x => x.id == movieTheaterId).ToList<MovieLocation>();
                if (movieFormatId > 0)
                    theaterFilterList.ForEach(x => x.formats = x.formats.Where(y => y.id == movieFormatId).ToList());
                foreach (var movieTheater in theaterFilterList) {
                    foreach (var movieFormat in movieTheater.formats) {
                        s += "<div class='address-mc'>";
                        s += "<p>" + movieTheater.nameFull + "</p>";
                        s += "<p>" + movieTheater.address + "</p>";
                        s += "<p>" + movieFormat.name + "</p>";

                        // Filter dates.
                        movieFormat.shows = movieFormat.shows.Where(x => x.dt >= nowOnlyDate).OrderBy(x => x.dt).ToList();
                        if (movieFormat.shows.Count == 0) {
                            s += "<p>No hay programación disponible en este teatro</p>";
                        }
                        s += "</div>";

                        // Now schedule for this movieTheater.
                        foreach (var sc in movieFormat.shows) {
                            s += "<div class='list-days'>";
                            s += "<p>" + Util.getNombreDiaEspañol(sc.dt.DayOfWeek.ToString()) + "<span>" + sc.dt.Day.ToString().PadLeft(2, '0') + " de " + MonthToName(sc.dt.Month) + " de " + sc.dt.Year + "</span></p>";
                            s += "<ul class='clearfix'>";

                            if (sc.hours.Count == 1) {                                
                                var h = sc.hours[0];
                                DateTime dtProgrammed = new DateTime(sc.dt.Year, sc.dt.Month, sc.dt.Day, h.timeHour, h.timeMinute, 0);
                                if (DateTime.Compare(dtProgrammed, now) < 0)
                                    s += "<li class='last old'>";
                                else
                                    s += "<li class='last'>";
                                s += h.timeFull + "</li>";
                            }
                            else {
                                DateTime dtProgrammed;
                                for (int i = 0; i < sc.hours.Count; i++) {
                                    var h = sc.hours[i];
                                    dtProgrammed = new DateTime(sc.dt.Year, sc.dt.Month, sc.dt.Day, h.timeHour, h.timeMinute, 0);
                                    if (DateTime.Compare(dtProgrammed, now) < 0) {
                                        if (i == 0)//First
                                            s += "<li class='first old'>";
                                        else if (i == sc.hours.Count - 1)//ultimo
                                            s += "<li class='last old'>";
                                        else
                                            s += "<li class='old'>";
                                    }
                                    else {
                                        if (i == 0)
                                            s += "<li class='first'>";
                                        else if (i == sc.hours.Count - 1)
                                            s += "<li class='last'>";
                                        else
                                            s += "<li>";
                                    }
                                    s += h.timeFull + "</li>";
                                }
                            }
                            s += "</ul>";
                            s += "</div>";
                        }
                        s += "<div class='bg-footer'></div>";
                    }
                }
            }
            movieAllData.Text = s;
        }

        private string MonthToName(int p) {
            string s = "";
            switch (p) {
                case 1:
                    s = "enero";
                    break;
                case 2:
                    s = "febrero";
                    break;
                case 3:
                    s = "marzo";
                    break;
                case 4:
                    s = "abril";
                    break;
                case 5:
                    s = "mayo";
                    break;
                case 6:
                    s = "junio";
                    break;
                case 7:
                    s = "julio";
                    break;
                case 8:
                    s = "agosto";
                    break;
                case 9:
                    s = "septiembre";
                    break;
                case 10:
                    s = "octubre";
                    break;
                case 11:
                    s = "noviembre";
                    break;
                case 12:
                    s = "diciembre";
                    break;
                default:
                    break;
            }
            return s;
        }

        // Parameter in URL are
        // m: Movie id.
        // f: Movie Format id.
        // t: Theater id.
        protected void Page_Load(object sender, EventArgs e) {
            // Load parameters.
            m = Request.QueryString["m"];
            t = Request.QueryString["t"];
            f = Request.QueryString["f"];

            // Let's compute parameters as true integers.
            int tst = 0;

            if (m == null) {
                m = "0";
            }
            else {
                try {
                    tst = Convert.ToInt32(m);
                } catch (Exception) {
                    m = "0";
                }
            }
            if (t == null) {
                t = "-1";
            }
            else {
                try {
                    tst = Convert.ToInt32(t);
                    if (tst == 0) {
                        t = "-1";
                    }
                } catch (Exception) {
                    t = "-1";
                }
            }
            if (f == null) {
                f = "-1";
            }
            else {
                try {
                    tst = Convert.ToInt32(f);
                    if (tst == 0) {
                        f = "-1";
                    }
                } catch (Exception) {
                    f = "-1";
                }
            }

            ManageMovieCatalog mmc = new ManageMovieCatalog(Settings.JSONFolder + @"\" + Settings.FileMovieCatalog, Settings.JSONFolder + @"\" + Settings.FileMovies, "");
            int movieId = Convert.ToInt32(m);
            movieList = mmc.RetrieveMovieList();
            mc = mmc.RetriveMovieCatalog();

            movieToShow = movieList.Where(x => x.id == movieId).FirstOrDefault<Movie>();
            movieID.Value = m;
            if (!IsPostBack) {
                LoadData();
                LoadDetailedData();
            }
        }

        private DateTime MapToLocalTimeColombia() {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            DateTime time = DateTime.UtcNow;
            DateTime convertedTime = time;

            if (time.Kind == DateTimeKind.Local && !timeZone.Equals(TimeZoneInfo.Local))
                convertedTime = TimeZoneInfo.ConvertTime(time, TimeZoneInfo.Local, timeZone);
            else if (time.Kind == DateTimeKind.Utc && !timeZone.Equals(TimeZoneInfo.Utc))
                convertedTime = TimeZoneInfo.ConvertTime(time, TimeZoneInfo.Utc, timeZone);
            return convertedTime;
        }

        protected void OnMovieFormatChanged(object sender, EventArgs e) {
            m = "0";
            f = "-1";
            t = "-1";

            m = movieID.Value;
            f = movieFormat.SelectedValue;
            t = movieTheater.SelectedValue;
            LoadData();
            LoadDetailedData();
        }

        protected void OnMovieTheaterChanged(object sender, EventArgs e) {
            m = "0";
            f = "-1";
            t = "-1";

            m = movieID.Value;
            f = movieFormat.SelectedValue;
            t = movieTheater.SelectedValue;
            LoadData();
            LoadDetailedData();
        }
    }
}