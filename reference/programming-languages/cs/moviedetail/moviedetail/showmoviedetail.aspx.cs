using EC.Business;
using EC.Domain;
using EC.Domain.Movie;
using EC.Utils.Constants;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace moviedetail
{
    public partial class showmoviedetail : System.Web.UI.Page
    {
        private List<MovieLookup> movieList = null;
        private MovieCatalog mc = null;

        private void FillMovieTheaterList(string m, ref string f)
        {
            List<String> theaterNameList = new List<String>();
            List<KeyValue> l = new List<KeyValue>();
            int movieId = Convert.ToInt32(m);
            var movieListFiltered = (from it in movieList
                                     orderby it.nameFull
                                     where it.id == movieId
                                     select it
                                    ).ToList<MovieLookup>();

            l.Add(new KeyValue() { key = "-1", value = "- SELECCIONE -" });
            foreach (var mv in movieListFiltered)
            {
                // Locations
                foreach (var loc in mv.locations)
                {
                    if (!theaterNameList.Contains(loc.nameFull))
                    {
                        KeyValue kv = new KeyValue() { key = loc.id.ToString(), value = loc.nameFull };
                        l.Add(kv);
                    }
                }
            }
            var movieTheaterSorted = (from it in l
                                      orderby it.value
                                      select it).ToList<KeyValue>();
            movieTheater.DataSource = movieTheaterSorted;
            movieTheater.DataTextField = "value";
            movieTheater.DataValueField = "key";
            try
            {
                movieTheater.SelectedValue = f;
                movieTheater.DataBind();
            }
            catch (Exception)
            {
                f = "-1";
            }
        }

        private void FillMovieFormatList(string m,ref string t)
        {
            List<String> formatNameList = new List<String>();
            List<KeyValue> l = new List<KeyValue>();
            int movieId = Convert.ToInt32(m);
            var movieListFiltered = (from it in movieList
                                     orderby it.nameFull
                                     where it.id == movieId
                                     select it
                                    ).ToList<MovieLookup>();

            l.Add(new KeyValue() { key = "-1", value = "- SELECCIONE -" });
            foreach (var mv in movieListFiltered)
            {
                if (!formatNameList.Contains(mv.format))
                {
                    KeyValue kv = new KeyValue() { key = mv.idFormat.ToString(), value = mv.format };
                    l.Add(kv);
                }
            }
            var movieFormatSorted = (from it in l
                                     orderby it.value
                                     select it).ToList<KeyValue>();
            movieFormat.DataSource = movieFormatSorted;
            movieFormat.DataTextField = "value";
            movieFormat.DataValueField = "key";
            try
            {
                movieFormat.SelectedValue = t;
                movieFormat.DataBind();
            }
            catch (Exception)
            {
                t = "-1";
            }                        
        }

        private void LoadData(string m, ref string f, ref string t)
        {
            int movieId = Convert.ToInt32(m);
            movieID.Value = m;
            MovieShortInfo movieData = (from it in mc.movies
                                        where it.id == movieId
                                        select it).FirstOrDefault<MovieShortInfo>();
            if (movieData != null)
            {
                movieName.Text = movieData.name;

                FillMovieTheaterList(m, ref t);
                FillMovieFormatList(m, ref f);
            }
        }

        private void LoadDetailedData(string m, ref string f, ref string t)
        {
            DateTime now = DateTime.Now;
            string s = "";
            int movieId = Convert.ToInt32(m);
            int movieTheaterId = Convert.ToInt32(t);
            int movieFormatId = Convert.ToInt32(f);

            // Always in variable movieListFiltered we compute the records to show.
            List<MovieLookup> movieListFiltered = null;

            if (movieId != 0 && movieTheaterId == -1 && movieFormatId == -1)
            {
                // Select only movies
                movieListFiltered = (from it in movieList where it.id == movieId select it).ToList();
            }
            else
            {
                // Select only movies and formats
                if (movieId != 0 && movieFormatId != -1 && movieTheaterId != -1)
                {
                    // At this point all fiters are selected, that is, 
                    // movieId <> 0, movieFormatId <> -1 and movieTheaterId <> -1
                    var movieFormatListFiltered = (from it in movieList where it.id == movieId && it.idFormat == movieFormatId select it).ToList<MovieLookup>();
                    movieListFiltered = new List<MovieLookup>();
                    foreach (var mv in movieFormatListFiltered)
                    {
                        foreach (var loc in mv.locations)
                        {
                            if (loc.id == movieTheaterId)
                            {
                                // We must only add the location being looked up.
                                MovieLookup mvFiltered = new MovieLookup();
                                mvFiltered.id = mv.id;
                                mvFiltered.name = mv.name;
                                mvFiltered.idFormat = mv.idFormat;
                                mvFiltered.format = mv.format;
                                mvFiltered.nameFull = mv.name;
                                mvFiltered.img = mv.img;
                                mvFiltered.url = mv.url;
                                mvFiltered.active = mv.active;
                                mvFiltered.idGenre = mv.idGenre;
                                mvFiltered.genre = mv.genre;
                                mvFiltered.locations = new List<MovieLookupLocation>();
                                mvFiltered.locations.Add(loc);
                                movieListFiltered.Add(mvFiltered);
                            }
                        }
                    }
                }
                else
                {
                    if (movieId != 0 && movieTheaterId != -1)
                    {
                        // Select only movies and the theater desired
                        var movieAllList = (from it in movieList where it.id == movieId select it).ToList();                        
                        movieListFiltered = new List<MovieLookup>();
                        foreach (var mv in movieAllList)
                        {
                            foreach (var loc in mv.locations)
                            {
                                if (loc.id == movieTheaterId)
                                {
                                    // We must only add the location being looked up.
                                    MovieLookup mvFiltered = new MovieLookup();
                                    mvFiltered.id = mv.id;
                                    mvFiltered.name = mv.name;
                                    mvFiltered.idFormat = mv.idFormat;
                                    mvFiltered.format = mv.format;
                                    mvFiltered.nameFull = mv.name;
                                    mvFiltered.img = mv.img;
                                    mvFiltered.url = mv.url;
                                    mvFiltered.active = mv.active;
                                    mvFiltered.idGenre = mv.idGenre;
                                    mvFiltered.genre = mv.genre;
                                    mvFiltered.locations = new List<MovieLookupLocation>();
                                    mvFiltered.locations.Add(loc);
                                    movieListFiltered.Add(mvFiltered);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (movieId != 0 && movieFormatId != -1)
                        {
                            movieListFiltered = (from it in movieList where it.id == movieId && it.idFormat == movieFormatId select it).ToList();
                        }
                    }
                }
            }

            if (movieListFiltered != null)
            {
                foreach (var it in movieListFiltered)
                {
                    foreach (var loc in it.locations)
                    {
                        //s += "<div class='address-mc'><p>" + it.format + " - " + loc.nameFull + " - " + loc.address + "</p></div>";
                        s += "<div class='address-mc'>";
                        s += "<p>" + loc.nameFull + "</p>";
                        s += "<p>" + loc.address + "</p>";
                        if (movieTheaterId == -1)
                        {
                            s += "<p>" + it.format + "</p>";
                        }
                        s += "</div>";

                        // Now schedule for this theater.
                        foreach (var sc in loc.schedule)
                        {
                            DateTime dt = new DateTime(sc.year, sc.month, sc.day);
                            //if (dt < now)
                            //{
                            //    s += "<div class='list-days old'>";
                            //}
                            //else
                            //{
                                s += "<div class='list-days'>";
                            //}
                            s += "<p>" + sc.name + "<span>" + sc.day.ToString().PadLeft(2, '0') + " de " + MonthToName(sc.month) + " de " + sc.year + "</span></p>";
                            s += "<ul class='clearfix'>";

                            if (sc.hours.Count == 1)
                            {
                                int i = 0;
                                var h = sc.hours[i];                                
                                DateTime dtProgrammed = new DateTime(sc.year, sc.month, sc.day, h.timeHour, h.timeMinute, 0);                                
                                if (DateTime.Compare(dtProgrammed, now) < 0)
                                {
                                    if (i == 0)
                                    {
                                        s += "<li class='last old'>";
                                    }
                                    else
                                    {
                                        s += "<li class='old'>";
                                    }
                                }
                                else
                                {
                                    if (i == 0)
                                    {
                                        s += "<li class='last'>";
                                    }
                                    else
                                    {
                                        s += "<li>";
                                    }
                                }
                                s += h.timeFull + "</li>";
                            }
                            else
                            {
                                DateTime dtProgrammed;
                                for (int i = 0; i < sc.hours.Count; i++)
                                {
                                    var h = sc.hours[i];                                    
                                    dtProgrammed = new DateTime(sc.year, sc.month, sc.day, h.timeHour, h.timeMinute, 0);
                                    if (DateTime.Compare(dtProgrammed, now) < 0)
                                    {
                                        if (i == 0)
                                        {
                                            s += "<li class='first old'>";
                                        }
                                        else if (i == sc.hours.Count - 1)
                                        {
                                            s += "<li class='last old'>";
                                        }
                                        else
                                        {
                                            s += "<li class='old'>";
                                        }
                                    }
                                    else
                                    {
                                        if (i == 0)
                                        {
                                            s += "<li class='first'>";
                                        }
                                        else if (i == sc.hours.Count - 1)
                                        {
                                            s += "<li class='last'>";
                                        }
                                        else
                                        {
                                            s += "<li>";
                                        }
                                    }
                                    s += h.timeFull + "</li>";
                                }
                            }

                            s += "</ul>";
                            s += "</div>";
                        }
                    }
                    s += "<div class='bg-footer'></div>";
                }
            }
            movieAllData.Text = s;
        }

        private string MonthToName(int p)
        {
            string s = "";
            switch (p)
            {
                case 1: s = "enero";
                    break;
                case 2: s = "febrero"; break;
                case 3: s = "marzo"; break;
                case 4: s = "abril"; break;
                case 5: s = "mayo"; break;
                case 6: s = "junio"; break;
                case 7: s = "julio"; break;
                case 8: s = "agosto"; break;
                case 9: s = "septiembre"; break;
                case 10: s = "octubre"; break;
                case 11: s = "noviembre"; break;
                case 12: s = "diciembre"; break;
                default:
                    break;
            }
            return s;
        }

        // Parameter in URL are
        // m: Movie id.
        // f: Movie Format id.
        // t: Theater id.
        protected void Page_Load(object sender, EventArgs e)
        {
            ManageMovieCatalog mmc = new ManageMovieCatalog();
            mmc.catalogNameFileName = @ConfigurationManager.AppSettings[GlobalConstants.FileMoviesCatalogKey];
            mmc.moviesFileName = @ConfigurationManager.AppSettings[GlobalConstants.FileMoviesKey];
            mmc.dbConnection = @ConfigurationManager.AppSettings[GlobalConstants.ConnectionKey];

            movieList = mmc.RetrieveMovieList();
            mc = mmc.RetriveMovieCatalog();

            string m = Request.QueryString["m"];
            string t = Request.QueryString["t"];
            string f = Request.QueryString["f"];

            if (m == null)
            {
                m = "0";
            }
            if (t == null)
            {
                t = "-1";
            }
            if (f == null)
            {
                f = "-1";
            }

            if (!IsPostBack)
            {
                LoadData(m, ref f, ref t);
                LoadDetailedData(m, ref f, ref t);
            }
        }

        protected void OnMovieFormatChanged(object sender, EventArgs e)
        {
            string m = "0";
            string f = "-1";
            string t = "-1";

            m = movieID.Value;
            f = movieFormat.SelectedValue;
            t = movieTheater.SelectedValue;
            LoadDetailedData(m, ref f, ref t);
        }

        protected void OnMovieTheaterChanged(object sender, EventArgs e)
        {
            string m = "0";
            string f = "-1";
            string t = "-1";

            m = movieID.Value;
            f = movieFormat.SelectedValue;
            t = movieTheater.SelectedValue;
            LoadDetailedData(m, ref f, ref t);
        }
    }
}