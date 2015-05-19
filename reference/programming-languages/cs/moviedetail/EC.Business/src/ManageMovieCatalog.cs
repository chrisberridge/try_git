/*==========================================================================*/
/* Source File:   MANAGEMOVIECATALOG.CS                                     */
/* Description:   Project constants                                         */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: Feb.18/2015                                               */
/* Version:       1.2                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
============================================================================*/

using EC.Domain.Movie;
using EC.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace EC.Business
{
    /// <summary>
    /// The purpose for this class is to be a helper to manipulate movie catalog. Dasource comes from MS Server 2012 database.
    /// Records from database are serialized in a defined folder as JSON files for easy processing.
    /// </summary>
    public class ManageMovieCatalog
    {
        public string catalogNameFileName { get; set; }
        public string moviesFileName { get; set; }
        public string dbConnection { get; set; }

        /// <summary>
        /// Queries database to locate hours for a time period of time for a movie.
        /// </summary>
        /// <param name="id">Record to locate</param>
        /// <returns>List of hours for movie.</returns>
        private List<MovieLookupShowHours> LoadHoursFor(int id)
        {
            string sql = "select * from tbl_hora where idHorarioPelicula = @id order by horaPelicula, minutoPelicula ";
            List<MovieLookupShowHours> hours = new List<MovieLookupShowHours>();
            HandleDatabase hdb = new HandleDatabase(dbConnection);
            hdb.Open();
            SqlTransaction transaction = hdb.BeginTransaction("LoadHoursFor");

            SqlParameter param1 = new SqlParameter();
            param1.ParameterName = "@id";
            param1.Value = id;
            param1.SqlDbType = SqlDbType.BigInt;

            SqlDataReader rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql, param1);
            while (rdr.Read())
            {
                MovieLookupShowHours msh = new MovieLookupShowHours()
                {
                    id = Convert.ToInt32(rdr["idHora"]),
                    timeHour = Convert.ToInt32(rdr["horaPelicula"]),
                    timeMinute = Convert.ToInt32(rdr["minutoPelicula"]),
                    room = rdr["sala"].ToString()
                };
                hours.Add(msh);
            }

            // Now that we have all of the hours set, we need now to compute a time format on timeHour 
            // and timeMinute fields.
            foreach (var item in hours)
            {
                string ampm = "am";
                int hour = item.timeHour;
                if (hour > 12)
                {
                    ampm = "pm";
                    hour -= 12;
                }
                if (hour == 12)
                {
                    ampm = "pm";
                }
                item.timeFull = hour + ":" + item.timeMinute.ToString().PadLeft(2, '0') + " " + ampm;
            }
            return hours;
        }

        /// <summary>
        /// Main method to compute DB records to JSON files. All JSON files are stored in a
        /// configurable folder.
        /// </summary>
        public void CompileAllMoviesSchedule()
        {
            string sql = "";
            MovieCatalog mc = new MovieCatalog();
            mc.theaters = new List<MovieTheaterInfo>();
            mc.formats = new List<MovieFormatInfo>();
            mc.movies = new List<MovieShortInfo>();
            mc.theaterMovies = new Dictionary<string, List<MovieShortInfo>>();

            sql += "select * from vw_datospelicula ";
            sql += "where activo = 'S' ";
            sql += "order by idPelicula, idTeatro, frecuencia, idformato ";
            List<MovieLookup> movieList = new List<MovieLookup>();
            HandleDatabase hdb = new HandleDatabase(dbConnection);
            hdb.Open();
            SqlTransaction transaction = hdb.BeginTransaction("CompileAllMoviesSchedule");
            SqlDataReader rdr = hdb.ExecSelectSQLStmtAsReader(transaction, sql);
            int oldMovie = -1;
            int newMovie = 0;
            int oldTheater = -1;
            int newTheater = 0;
            int oldFormat = -1;
            int newFormat = 0;
            bool doMovieRecord = false;
            MovieLookup m = null;
            MovieLookupLocation movieLocation = null;
            while (rdr.Read())
            {
                newMovie = Convert.ToInt32(rdr["idPelicula"]);
                newFormat = Convert.ToInt32(rdr["idFormato"]);
                if (oldMovie != newMovie)
                {
                    // Create Movie detail when movie break control is met.
                    oldMovie = newMovie;                    
                    oldFormat = newFormat;
                    doMovieRecord = true;
                }
                else
                {
                    if (oldFormat != newFormat)
                    {
                        oldFormat = newFormat;
                        doMovieRecord = true;
                    }
                }
                if (doMovieRecord)
                {
                    doMovieRecord = false;
                    oldTheater = -1;
                    m = new MovieLookup();
                    m.id = newMovie;
                    m.name = rdr["nombrePelicula"].ToString();
                    m.idFormat = newFormat;
                    m.format = rdr["nombreFormato"].ToString();
                    m.nameFull = m.name + " " + m.format;
                    m.img = rdr["imagenCartelera"].ToString();
                    m.url = rdr["urlArticuloEC"].ToString();
                    m.active = rdr["activo"].ToString();
                    m.idGenre = Convert.ToInt32(rdr["idGeneroPelicula"]);
                    m.genre = rdr["nombreGenero"].ToString();
                    movieList.Add(m);
                    m.locations = new List<MovieLookupLocation>();
                }
                newTheater = Convert.ToInt32(rdr["idTeatro"]);
                if (oldTheater != newTheater)
                {
                    oldTheater = newTheater;
                    movieLocation = new MovieLookupLocation();
                    movieLocation.schedule = new List<MovieLookupShow>();
                    movieLocation.id = newTheater;
                    movieLocation.name = rdr["nombreCine"].ToString();
                    movieLocation.branchName = rdr["nombreTeatro"].ToString();
                    movieLocation.nameFull = movieLocation.name + " " + movieLocation.branchName;
                    movieLocation.address = rdr["direccionTeatro"].ToString();
                    m.locations.Add(movieLocation);
                }

                // Now Load Location Hours
                MovieLookupShow mls = new MovieLookupShow();
                mls.id = Convert.ToInt32(rdr["idHorarioPelicula"]);
                mls.frequency = Convert.ToInt32(rdr["frecuencia"]);
                mls.name = rdr["nombreDiaSemanaHorarioPelicula"].ToString();
                mls.year = Convert.ToInt32(rdr["annoHorarioPelicula"]);
                mls.month = Convert.ToInt32(rdr["mesHorarioPelicula"]);
                mls.day = Convert.ToInt32(rdr["diaHorarioPelicula"]);
                mls.hours = LoadHoursFor(mls.id);
                movieLocation.schedule.Add(mls);
            }
            rdr.Close();
            transaction.Commit();
            hdb.Close();

            // Up to this point we have all movies gathered, now we build the movie Catalog in order
            // to ease search manipulation when required and to speed data processing.
            // 1. Load Movie Name list
            var movieNameList = (from it in movieList
                                 orderby it.active descending, it.nameFull
                                 select new MovieShortInfo()
                                 {
                                     id = it.id,
                                     idFormat = it.idFormat,
                                     name = it.name,
                                     nameFull = it.nameFull
                                 }).ToList<MovieShortInfo>();
            mc.movies = movieNameList;

            // 2. Movie Formats Name list
            var movieFormatNameList = (from it in movieList
                                       orderby it.format
                                       select new MovieFormatInfo()
                                       {
                                           id = it.idFormat,
                                           name = it.format
                                       }
                                      ).ToList<MovieFormatInfo>();

            List<MovieFormatInfo> formatListFiltered = new List<MovieFormatInfo>();
            oldFormat = -1;
            newFormat = 0;
            foreach (var item in movieFormatNameList)
            {
                newFormat = item.id;
                if (oldFormat != newFormat)
                {
                    oldFormat = newFormat;
                    formatListFiltered.Add(new MovieFormatInfo() { id = item.id, name = item.name });
                }
            }
            mc.formats = formatListFiltered;

            // 3. Movie Genre Name list
            var movieGenreNameList = (from it in movieList
                                      orderby it.genre
                                      select new MovieGenreInfo
                                      {
                                          id = it.idGenre,
                                          name = it.genre
                                      }).ToList<MovieGenreInfo>();
            List<MovieGenreInfo> genreListFiltered = new List<MovieGenreInfo>();
            int oldGenre = -1;
            int newGenre = 0;
            foreach (var item in movieGenreNameList)
            {
                newGenre = item.id;
                if (oldGenre != newGenre)
                {
                    oldGenre = newGenre;
                    genreListFiltered.Add(new MovieGenreInfo() { id = item.id, name = item.name });
                }
            }
            mc.genres = genreListFiltered;

            // 4. Load all theaters referenced in the schedule.
            foreach (var movieItem in movieList)
            {
                foreach (var loc in movieItem.locations)
                {
                    MovieTheaterInfo mti = new MovieTheaterInfo();
                    mti.id = loc.id;
                    mti.name = loc.name;
                    mti.branchName = loc.branchName;
                    mti.nameFull = loc.nameFull;
                    mc.theaters.Add(mti);
                }
            }

            // Lets order by id
            mc.theaters = (from t in mc.theaters
                           orderby t.id
                           select t).ToList<MovieTheaterInfo>();

            // We now have theater names, but not sorted and not distinct. Let's sort by name excluding repeated.
            List<MovieTheaterInfo> allTheatersDistinctList = new List<MovieTheaterInfo>();
            oldTheater = -1;
            newTheater = 0;
            foreach (var t in mc.theaters)
            {
                newTheater = t.id;
                if (oldTheater != newTheater)
                {
                    MovieTheaterInfo mti = new MovieTheaterInfo();
                    mti.id = t.id;
                    mti.name = t.name;
                    mti.branchName = t.branchName;
                    mti.nameFull = t.nameFull;
                    allTheatersDistinctList.Add(mti);
                    oldTheater = newTheater;
                }
            }
            // Now list has all distinct IDs, now let's sort by name
            mc.theaters = (from t in allTheatersDistinctList
                           orderby t.nameFull
                           select t).Distinct().ToList<MovieTheaterInfo>();

            // 5. Load Movie Name List and its associated theaters where each movie is being shown.
            Dictionary<string, List<MovieShortInfo>> theaterMovies = new Dictionary<string, List<MovieShortInfo>>();

            // Initialize theaterMovies for computation.
            mc.theaters.ForEach(t => theaterMovies.Add(t.nameFull, new List<MovieShortInfo>()));
            foreach (var movieItem in movieList)
            {
                int currentMovieId = movieItem.id;
                foreach (var loc in movieItem.locations)
                {
                    var theaterItem = theaterMovies[loc.nameFull];
                    bool movieExists = false;
                    foreach (var movieShortInfo in theaterItem)
                    {
                        if (movieShortInfo.id == currentMovieId)
                        {
                            movieExists = true;
                            break;
                        }
                    }
                    if (!movieExists)
                    {
                        theaterItem.Add(new MovieShortInfo()
                        {
                            id = currentMovieId,
                            idFormat = movieItem.idFormat,
                            name = movieItem.name,
                            nameFull = movieItem.nameFull
                        });
                    }
                }
            }
            mc.theaterMovies = theaterMovies;

            // Serialize
            string movieLookupJSON = JsonConvert.SerializeObject(movieList);
            string movieCatalogJSON = JsonConvert.SerializeObject(mc);

            // Now that we have just  gathered all the information, create static JSON versions
            // Now there are two files to consume the feed

            // Full movie catalog (mapped from origin).
            string fileName = moviesFileName;
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.Write(movieLookupJSON);
            }

            fileName = catalogNameFileName;
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.Write(movieCatalogJSON);
            }
        }

        /// <summary>
        /// Helper method to load a JSON file and deserialized to proper structure. NOTE: This is the movie list catalog.
        /// </summary>
        /// <returns>A list of MovieLookup objects</returns>
        public List<MovieLookup> RetrieveMovieList()
        {
            List<MovieLookup> movieList = null;
            string s;
            using (StreamReader reader = new StreamReader(moviesFileName))
            {
                s = reader.ReadToEnd();
            }
            movieList = JsonConvert.DeserializeObject<List<MovieLookup>>(s);
            return movieList;
        }

        /// <summary>
        /// Helper method to load a JSON file and deserialized to proper structure. NOTE: This is the movie catalog.
        /// </summary>
        /// <returns>An object of type MovieCatalog that contains helper information when consumed by third-party.</returns>
        public MovieCatalog RetriveMovieCatalog()
        {
            MovieCatalog mc = null;
            string s;
            using (StreamReader reader = new StreamReader(catalogNameFileName))
            {
                s = reader.ReadToEnd();
            }
            mc = JsonConvert.DeserializeObject<MovieCatalog>(s);
            return mc;
        }

        /// <summary>
        /// The purpose of this page is to service JSON Files. The files are already generated 
        /// elsewhere and are loaded from file system.
        /// 
        /// A query string parameter is sent to this page. Given name of 'm' and has values of '1:Get movies' file,
        /// '2:Get Movies Catalog' and so on.
        /// </summary>
        /// <param name="m">Which file to get. 1:Movies, 2: Movie Catalog</param>
        /// <returns></returns>
        public string RetrieveJSonContentsForModule(string m)
        {
            string rslt = "";

            MovieCatalog mc = RetriveMovieCatalog();

            switch (m)
            {
                case "1":
                    rslt = JsonConvert.SerializeObject(RetrieveMovieList());
                    break;
                case "2":
                    rslt = JsonConvert.SerializeObject(RetriveMovieCatalog());
                    break;
                default:
                    break;
            }
            return rslt;
        }

        /// <summary>
        /// Given parameters 't:for theater id', 'm:Movie Id', and 'g:Gender id', computes a lookup in the catalog
        /// matched records.
        /// </summary>
        /// <param name="t">Theater Id. Possible value are '-1', or '2'</param>
        /// <param name="m">Movie Id. Possible value are '-1', or '1|29' this means movieId=1 and movieIdFormat=29</param>
        /// <param name="g">Gender Id. Possible value are '-1', or '2'</param>
        /// <returns>A list of 'MovieLookup'  objects serialized as JSON. If list is empty, serialized should be empty.</returns>
        public string Search(string t, string m, string g)
        {
            int theaterId = Convert.ToInt32(t);
            int movieId = 0;
            int movieIdFormat = 0; // Only used where movieId is not set to -1
            int genreId = Convert.ToInt32(g);
            string rslt = "";

            // We must split 'm' parameter to its constituent values.
            // If m = -1", no need to split
            if (m == "-1")
            {
                movieId = Convert.ToInt32(m);
            }
            else
            {
                var movieValuesArray = m.Split('|');
                movieId = Convert.ToInt32(movieValuesArray[0]);
                movieIdFormat = Convert.ToInt32(movieValuesArray[1]);
            }

            List<MovieLookup> movieList = RetrieveMovieList();
            if (theaterId == -1 && movieId == -1 && genreId == -1)
            {
                // Retrieves all records.
                var movieListOrdered = (from it in movieList
                                        orderby it.nameFull
                                        select it).ToList();
                int oldMovie = -1;
                int newMovie = 0;
                List<MovieLookup> movieListSearch = new List<MovieLookup>();
                foreach (var mv in movieListOrdered)
                {
                    newMovie = mv.id;
                    if (oldMovie != newMovie)
                    {
                        oldMovie = newMovie;
                        movieListSearch.Add(mv);
                    }
                }
                movieListSearch = (from it in movieListSearch orderby it.nameFull select it).ToList();
                rslt = JsonConvert.SerializeObject(movieListSearch.ToList());
            }
            else
            {
                if (theaterId == -1 && movieId == -1 && genreId != -1)
                {
                    // Search only by Genre
                    var movieListOrdered = (from it in movieList
                                            orderby it.nameFull
                                            where it.idGenre == genreId
                                            select it).ToList();
                    int oldMovie = -1;
                    int newMovie = 0;
                    List<MovieLookup> movieListSearch = new List<MovieLookup>();
                    foreach (var mv in movieListOrdered)
                    {
                        newMovie = mv.id;
                        if (oldMovie != newMovie)
                        {
                            oldMovie = newMovie;
                            movieListSearch.Add(mv);
                        }
                    }
                    movieListSearch = (from it in movieListSearch orderby it.nameFull select it).ToList();
                    rslt = JsonConvert.SerializeObject(movieListSearch.ToList());
                }
                else
                {
                    if (theaterId != -1 && movieId != -1 && genreId == -1)
                    {
                        // Search a specific movie by theater. Any Genre.
                        // No idFormat is required herein.

                        // 1. Select movies
                        var movieListOrdered = (from it in movieList
                                                orderby it.nameFull
                                                where it.id == movieId
                                                select it).ToList();

                        // 2. Select theater
                        bool theaterFound = false;
                        MovieLookup mvSelected = null;
                        foreach (var mv in movieListOrdered)
                        {
                            var loc = (from l in mv.locations
                                       where l.id == theaterId
                                       select l).FirstOrDefault();
                            if (loc != null)
                            {
                                theaterFound = true;
                                mvSelected = mv;
                                break;
                            }
                        }
                        List<MovieLookup> movieListSearch = new List<MovieLookup>();
                        if (theaterFound)
                        {
                            movieListSearch.Add(mvSelected);
                        }
                        movieListSearch = (from it in movieListSearch orderby it.nameFull select it).ToList();
                        rslt = JsonConvert.SerializeObject(movieListSearch.ToList());
                    }
                    else
                    {
                        if (theaterId == -1 && movieId != -1 && genreId == -1)
                        {
                            // Only one movie must be selected.
                            // Movie record must match criteria for movieId and movieIdFormat as well
                            var mvSelected = (from it in movieList
                                              orderby it.nameFull
                                              where it.id == movieId && it.idFormat == movieIdFormat
                                              select it).FirstOrDefault();
                            List<MovieLookup> movieListSearch = new List<MovieLookup>();
                            movieListSearch.Add(mvSelected);
                            movieListSearch = (from it in movieListSearch orderby it.nameFull select it).ToList();
                            rslt = JsonConvert.SerializeObject(movieListSearch.ToList());
                        }
                        else
                        {
                            if (theaterId != -1 && movieId == -1 && genreId == -1)
                            {
                                // Find all movies by given theaterId
                                // Remove duplicates in the process.
                                List<MovieLookup> movieListSearch = new List<MovieLookup>();
                                foreach (var mv in movieList)
                                {
                                    foreach (var loc in mv.locations)
                                    {
                                        if (loc.id == theaterId)
                                        {
                                            bool movieExists = false;
                                            int curMovieId = mv.id;
                                            foreach (var ml in movieListSearch)
                                            {
                                                if (curMovieId == ml.id)
                                                {
                                                    movieExists = true;
                                                }
                                            }
                                            if (!movieExists)
                                            {
                                                movieListSearch.Add(mv);
                                            }
                                        }
                                    }
                                }
                                // Order by nameFull field.
                                movieListSearch = (from it in movieListSearch orderby it.nameFull select it).ToList();
                                rslt = JsonConvert.SerializeObject(movieListSearch.ToList());
                            }
                        }
                    }
                }
            }
            return rslt;
        }
    }
}
