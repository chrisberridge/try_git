/*==========================================================================*/
/* Source File:   SHOWMOVIEDETAIL.ASPX.CS                                   */
/* Description:   Visualize a movie with its schedule                       */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/*                Christian Zapata  Vásquez (CZV)                           */
/* Date:          Feb.11/2015                                               */
/* Last Modified: May.05/2015                                               */
/* Version:       1.13                                                      */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
Apr.16/2015 COQ BE EXTREMELY CAREFUL AS THIS PAGE IS ALSO PRESENT IN 
                ECCINES.ADMIN
============================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;

using ELCOLOMBIANO.EcCines.Business;
using ELCOLOMBIANO.EcCines.Common;
using ELCOLOMBIANO.EcCines.Entities.Dtos;
using ELCOLOMBIANO.EcCines.Entities.Dtos.Movie;
using ELCOLOMBIANO.EcCines.Web;

namespace EcCines {
    /// <summary>
    /// Visualize a movie with its schedule
    /// NOTE: Must synchronize with same file in EcCines.
    /// </summary>
    public partial class showmoviedetail : WebPageBase {
        private ManageMovieCatalog mmc = null;
        private Movie movieToShow = null;
        private string m = ""; // Movie Id, gathered in Page_Load
        private string f = ""; // Format Id, gathered in Page_Load
        private string t = ""; // Theater Id, gathered in Page_Load

        /// <summary>
        /// Fills the list of theaters that exhibit this movie.
        /// It is filtered by format when selected.
        /// </summary>
        private void FillMovieTheaterList() {
            if (log.IsDebugEnabled) {
                log.Debug("FillMovieTheaterList Starts");
            }
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
            if (log.IsDebugEnabled) {
                log.Debug("FillMovieTheaterList Ends");
            }
        }

        /// <summary>
        /// Fills the list of formats that exhibit this movie.
        /// It is filtered by theater when selected.
        /// </summary>
        private void FillMovieFormatList() {
            if (log.IsDebugEnabled) {
                log.Debug("FillMovieFormatList Starts");
            }
            int movieId = Convert.ToInt32(m);
            List<KeyValue> l = new List<KeyValue>();
            l.Add(new KeyValue() { key = "-1", value = "- SELECCIONE -" });

            if (t == "-1") {
                if (movieToShow != null) {
                    movieToShow.locations.ForEach(loc => {
                        loc.formats.ForEach(f => {
                            var exist = l.Where(x => x.key == f.id.ToString()).FirstOrDefault<KeyValue>();
                            if (exist == null) {
                                l.Add(new KeyValue() { key = f.id.ToString(), value = f.name });
                            }
                        });
                    });
                }
            }
            else {
                int theaterId = Convert.ToInt32(t);
                var movieTheaterSelected = movieToShow.locations.Where(x => x.id == theaterId).FirstOrDefault<MovieLocation>();
                if (movieTheaterSelected != null) {
                    movieTheaterSelected.formats.ForEach(f => l.Add(new KeyValue() { key = f.id.ToString(), value = f.name }));
                }
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
            if (log.IsDebugEnabled) {
                log.Debug("FillMovieFormatList Ends");
            }
        }

        /// <summary>
        /// Load main data.
        /// </summary>
        private void LoadData() {
            if (log.IsDebugEnabled) {
                log.Debug("LoadData Starts");
            }
            noscheduleAtAll.Text = "";

            if (movieToShow != null && movieToShow.name != "") {
                movieName.Text = movieToShow.name;
            }
            else {
                noscheduleAtAll.Text = "<div class='select-mc clearfix'>Esta película no está programada en los cines de la ciudad</div>";
            }
            FillMovieTheaterList();
            FillMovieFormatList();
            if (log.IsDebugEnabled) {
                log.Debug("LoadData Ends");
            }
        }

        /// <summary>
        /// Load detail data.
        /// </summary>
        private void LoadDetailedData() {
            if (log.IsDebugEnabled) {
                log.Debug("LoadDetailedData Starts");
            }

            string s = "";
            if (movieToShow != null) {
                int movieTheaterId = Convert.ToInt32(t);
                int movieFormatId = Convert.ToInt32(f);
                s = mmc.ShowMovieData(movieToShow, movieTheaterId, movieFormatId);
                if (s == "") {
                    noscheduleAtAll.Text = "<div class='select-mc clearfix'>Esta película no está programada en los cines de la ciudad</div>";
                }
            }
            movieAllData.Text = s;
            if (log.IsDebugEnabled) {
                log.Debug("LoadDetailedData Ends");
            }
        }

        
        /// <summary>
        /// Page Load Event
        /// Parameter in URL are
        /// m: Movie id.
        /// f: Movie Format id.
        /// t: Theater id.
        /// </summary>
        /// <param name="sender">Object which fires event</param>
        /// <param name="e">Event argument</param>
        protected void Page_Load(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("Page_Load Starts");
            }

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
            if (log.IsDebugEnabled) {
                log.Debug("Parameters received");
                log.Debug(" m: Movie id. =[" + m + "]");
                log.Debug(" f: Movie Format id. =[" + f + "]");
                log.Debug(" t: Theater id. =[" + t + "]");
            }

            int movieId = Convert.ToInt32(m);
            mmc = new ManageMovieCatalog(Settings.JSONFolder + @"\" + Settings.FileMovieCatalog, Settings.JSONFolder + @"\" + Settings.FileMovies, "");
            movieToShow = mmc.SearchToShowDetail(movieId);
            movieID.Value = m;
            if (!IsPostBack) {
                LoadData();
                LoadDetailedData();
            }
            if (log.IsDebugEnabled) {
                log.Debug("Page_Load Starts");
            }
        }

        /// <summary>
        /// common code for OnChanged() event.
        /// </summary>
        private void DoChanged() {
            if (log.IsDebugEnabled) {
                log.Debug("DoChanged Starts");
            }
            m = "0";
            f = "-1";
            t = "-1";

            m = movieID.Value;
            f = movieFormat.SelectedValue;
            t = movieTheater.SelectedValue;
            if (log.IsDebugEnabled) {
                log.Debug("Parameters to use");
                log.Debug(" m: Movie id. =[" + m + "]");
                log.Debug(" f: Movie Format id. =[" + f + "]");
                log.Debug(" t: Theater id. =[" + t + "]");
            }
            LoadData();
            LoadDetailedData();
            if (log.IsDebugEnabled) {
                log.Debug("DoChanged Ends");
            }
        }
        /// <summary>
        /// When combo list for format changes this event is fired.
        /// </summary>
        /// <param name="sender">Object which fires the event</param>
        /// <param name="e">Event Arguments</param>
        protected void OnMovieFormatChanged(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnMovieFormatChanged Starts");
            }
            DoChanged();
            if (log.IsDebugEnabled) {
                log.Debug("OnMovieFormatChanged Ends");
            }
        }

        /// <summary>
        /// When combo list for theater changes this event is fired.
        /// </summary>
        /// <param name="sender">Object which fires the event</param>
        /// <param name="e">Event Arguments</param>
        protected void OnMovieTheaterChanged(object sender, EventArgs e) {
            if (log.IsDebugEnabled) {
                log.Debug("OnMovieTheaterChanged Starts");
            }
            DoChanged();
            if (log.IsDebugEnabled) {
                log.Debug("OnMovieTheaterChanged Ends");
            }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public showmoviedetail()
            : base() {
        }
    }
}