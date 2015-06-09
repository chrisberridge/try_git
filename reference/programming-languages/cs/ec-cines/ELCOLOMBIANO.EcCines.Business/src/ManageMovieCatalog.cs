/*==========================================================================*/
/* Source File:   MANAGEMOVIECATALOG.CS                                     */
/* Description:   Project constants                                         */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: May.04/2015                                               */
/* Version:       1.15                                                      */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
Apr.17/2015 COQ Refactoring and new methdos.
============================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ELCOLOMBIANO.EcCines.Common;
using ELCOLOMBIANO.EcCines.Common.Extensions;
using ELCOLOMBIANO.EcCines.Entities;
using ELCOLOMBIANO.EcCines.Entities.Dtos.Movie;
using log4net;
using Newtonsoft.Json;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace ELCOLOMBIANO.EcCines.Business {
    /// <summary>
    /// The purpose for this class is to be a helper to manipulate movie catalog. Dasource comes from MS Server 2012 database.
    /// Records from database are serialized in a defined folder as JSON files for easy processing.
    /// </summary>
    public class ManageMovieCatalog {
        protected readonly ILog log = null;
        private string catalogNameFileName { get; set; }
        private string moviesFileName { get; set; }
        private string imgPathUrl { get; set; }

        /// <summary>
        /// Used in search operations to fill model data.
        /// </summary>
        /// <param name="searchMovieList">The list to act upon.</param>
        /// <returns>The action to use in Lambda expressions.</returns>
        private Action<Movie> FillMovieDataWithoutLocationSchedule(List<Movie> searchMovieList) {
            if (log.IsDebugEnabled) {
                log.Debug("FillMovieDataWithoutLocationSchedule Lambda Action Starts/Ends");
            }
            return movie => {
                Movie mv = new Movie() {
                    id = movie.id,
                    name = movie.name,
                    img = movie.img,
                    url = movie.url,
                    active = movie.active,
                    idGenre = movie.idGenre,
                    genre = movie.genre,
                    premiere = movie.premiere,
                    createDate = movie.createDate
                };
                searchMovieList.Add(mv);
            };
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ManageMovieCatalog() {
            this.log = LogManager.GetLogger(this.GetType());
        }

        /// <summary>
        /// Initialize internal properties.
        /// </summary>
        /// <param name="catalogNameFileName">File name for Catalog JSON</param>
        /// <param name="moviesFileName">File name for Movie JSON</param>
        /// <param name="imgPathUrl">Used internally to prefix URL domain to images to properly locate
        /// images. NOTE: It is  optional, with a default of empty string.</param>
        public ManageMovieCatalog(string catalogNameFileName, string moviesFileName, string imgPathUrl = "")
            : this() {
            if (log.IsDebugEnabled) {
                log.Debug("ManageMovieCatalog Starts");
                log.Debug("catalogNameFileName=[" + catalogNameFileName + "]");
                log.Debug("moviesFileName=[" + moviesFileName + "]");
                log.Debug("imgPathUrl (optional)=[" + imgPathUrl + "]");
            }
            this.catalogNameFileName = catalogNameFileName;
            this.moviesFileName = moviesFileName;
            this.imgPathUrl = imgPathUrl;
            if (log.IsDebugEnabled) {
                log.Debug("ManageMovieCatalog Ends");
            }
        }

        /// <summary>
        /// Search in the movie dictionary for the parameter supplier. And prepare the object for
        /// showing its details filtered by today date/time.
        /// </summary>
        /// <param name="movieId">Id of movie to search</param>
        /// <returns>A copy of the compiled movie filtered with information about today date/time. NULL if not faound.</returns>
        public Movie SearchToShowDetail(int movieId) {
            if (log.IsDebugEnabled) {
                log.Debug("SearchToShowDetail Starts");
                log.Debug("movieId=[" + movieId + "]");
            }
            List<Movie> movieList = RetrieveMovieList();
            Movie searchMovie = movieList.Where(x => x.id == movieId).FirstOrDefault<Movie>();
            if (searchMovie != null) {
                // Exclude all information that is past in time for today.
                DateTime nowOnlyDate = DateTime.Now.MapToLocalTimeColombiaTakeDatePart();
                searchMovie.locations.ForEach(loc => {
                    loc.formats.ForEach(f => {
                        f.shows = f.shows.Where(x => x.dt >= nowOnlyDate).OrderBy(x => x.dt).ToList();
                    });
                    loc.formats = loc.formats.Where(f => f.shows.Count != 0).ToList();
                });

                // Supress locations where no shows per format exist.
                searchMovie.locations = searchMovie.locations.Where(loc => loc.formats.Count != 0).OrderBy(loc => loc.nameFull).ToList<MovieLocation>();
            }
            if (log.IsDebugEnabled) {
                log.Debug("SearchToShowDetail Ends");
                if (searchMovie == null) {
                    log.Debug("Movie not found");
                }
                else {
                    log.Debug("Movie=[" + searchMovie.ToString() + "]");
                }
            }            
            return searchMovie;
        }

        /// <summary>
        /// Formats an HTML version for the information contained in the object at hand.
        /// </summary>
        /// <param name="movieToShow">Object to show.</param>
        /// <returns>Empty string if no information to show.</returns>
        public String ShowMovieData(Movie movieToShow, int movieTheaterId, int movieFormatId) {
            if (log.IsDebugEnabled) {
                log.Debug("ShowMovieData Starts");
                log.Debug("movieToShow=[" + movieToShow.ToString() + "]");
                log.Debug("movieTheaterId=[" + movieTheaterId + "]");
                log.Debug("movieFormatId=[" + movieFormatId + "]");
            }
            String s = "";
            DateTime now = DateTime.Now.MapToLocalTimeColombia();
            DateTime nowOnlyDate = DateTime.Now.MapToLocalTimeColombiaTakeDatePart();
            if (movieTheaterId > 0) {
                movieToShow.locations = movieToShow.locations.Where(x => x.id == movieTheaterId).ToList<MovieLocation>();
            }
            if (movieFormatId > 0) {
                movieToShow.locations.ForEach(x => x.formats = x.formats.Where(y => y.id == movieFormatId).ToList());
            }
            movieToShow.locations.ForEach(loc => {
                loc.formats.ForEach(f => {
                    s += "<div class='address-mc'>";
                    s += "<p>" + loc.nameFull + "</p>";
                    s += "<p>" + loc.address + "</p>";
                    s += "<p>" + f.name + "</p>";
                    s += "</div>";

                    // Now schedule for this location and format.
                    f.shows.ForEach(show => {
                        s += "<div class='list-days'>";
                        s += "<p>" + Utils.getDayNameSpanish(show.dt.DayOfWeek.ToString()) + "<span>" + show.dt.Day.ToString().PadLeft(2, '0') + " de " + Utils.monthToName(show.dt.Month) + " de " + show.dt.Year + "</span></p>";
                        s += "<ul class='clearfix'>";

                        if (show.hours.Count == 1) {
                            var h = show.hours[0];
                            DateTime dtProgrammed = new DateTime(show.dt.Year, show.dt.Month, show.dt.Day, h.timeHour, h.timeMinute, 0);
                            if (DateTime.Compare(dtProgrammed, now) < 0)
                                s += "<li class='last old'>";
                            else
                                s += "<li class='last'>";
                            s += h.timeFull + "</li>";
                        }
                        else {
                            DateTime dtProgrammed;
                            for (int i = 0; i < show.hours.Count; i++) {
                                var h = show.hours[i];
                                dtProgrammed = new DateTime(show.dt.Year, show.dt.Month, show.dt.Day, h.timeHour, h.timeMinute, 0);
                                if (DateTime.Compare(dtProgrammed, now) < 0) {
                                    if (i == 0)//First
                                        s += "<li class='first old'>";
                                    else if (i == show.hours.Count - 1)//Last
                                        s += "<li class='last old'>";
                                    else
                                        s += "<li class='old'>";
                                }
                                else {
                                    if (i == 0)
                                        s += "<li class='first'>";
                                    else if (i == show.hours.Count - 1)
                                        s += "<li class='last'>";
                                    else
                                        s += "<li>";
                                }
                                s += h.timeFull + "</li>";
                            }
                        }
                        s += "</ul>";
                        s += "</div>";
                    });
                    s += "<div class='bg-footer'></div>";
                });
            });
            if (log.IsDebugEnabled) {
                log.Debug("HTML set=[" + s + "]");
                log.Debug("ShowMovieData Edns");
            }
            return s;
        }

        /// <summary>
        /// Main method to compute DB records to JSON files. All JSON files are stored in a
        /// configurable folder.
        /// </summary>
        public void CompileAllMoviesSchedule() {
            if (log.IsDebugEnabled) {
                log.Debug("CompileAllMoviesSchedule Starts");
                log.Debug("Core");
            }
            Pelicula movieDao = new Pelicula();
            List<MovieFullInfo> movieFullList = movieDao.updateBillboardAndGetMovieFullInfo();

            // Normalize image to conform to URI.
            movieFullList.ForEach(m => {
                if (!m.img.Contains("http")) {
                    m.img = imgPathUrl + m.img;
                }
            });

            // Create List of movies
            List<Movie> movieList = new List<Movie>();
            Movie movieInfo = null;
            MovieLocation movieLocationInfo = null;
            MovieFormat movieFormatInfo = null;
            MovieShow movieShowInfo = null;
            movieFullList.ForEach(mfInfo => {
                // Let's first find movie existence
                var movieExist = movieList.Where(x => x.id == mfInfo.id).FirstOrDefault<Movie>();
                if (movieExist != null) {
                    movieInfo = movieExist;
                }
                else {
                    movieInfo = new Movie() {
                        id = mfInfo.id,
                        name = mfInfo.name,
                        img = mfInfo.img,
                        url = mfInfo.url,
                        active = mfInfo.active,
                        idGenre = mfInfo.idGenre,
                        genre = mfInfo.genre,
                        premiere = mfInfo.premiere,
                        createDate = mfInfo.createDate,
                        locations = new List<MovieLocation>()
                    };
                    movieList.Add(movieInfo);
                }

                // Fill remaining data for movieInfo record
                // 1. Fill Location
                var movieLocationExist = movieInfo.locations.Where(x => x.id == mfInfo.idLocation).FirstOrDefault<MovieLocation>();
                if (movieLocationExist != null) {
                    movieLocationInfo = movieLocationExist;
                }
                else {
                    movieLocationInfo = new MovieLocation() {
                        id = mfInfo.idLocation,
                        name = mfInfo.nameLocation,
                        branchName = mfInfo.branchName,
                        nameFull = mfInfo.nameFullLocation,
                        address = mfInfo.address,
                        formats = new List<MovieFormat>()
                    };
                    movieInfo.locations.Add(movieLocationInfo);
                }

                // 2. Fill Format
                var movieFormatExist = movieLocationInfo.formats.Where(x => x.id == mfInfo.idFormat).FirstOrDefault<MovieFormat>();
                if (movieFormatExist != null) {
                    movieFormatInfo = movieFormatExist;
                }
                else {
                    movieFormatInfo = new MovieFormat() {
                        id = mfInfo.idFormat,
                        name = mfInfo.nameFormat,
                        shows = new List<MovieShow>()
                    };
                    movieLocationInfo.formats.Add(movieFormatInfo);
                }

                // 3. Fill Show
                var movieShowExist = movieFormatInfo.shows.Where(x => x.id == mfInfo.idShow).FirstOrDefault<MovieShow>();
                if (movieShowExist != null) {
                    movieShowInfo = movieShowExist;
                }
                else {
                    movieShowInfo = new MovieShow() {
                        id = mfInfo.idShow,
                        dt = mfInfo.dt,
                        hours = movieDao.getMovieShowHoursFor(mfInfo.idShow)
                    };
                    movieFormatInfo.shows.Add(movieShowInfo);
                }
                movieFormatInfo.shows = movieFormatInfo.shows.OrderBy(x => x.dt).ToList<MovieShow>();
            });
            movieList = movieList.OrderByDescending(x => x.premiere).ThenBy(x => x.name).ToList<Movie>();

            // Up to this point we have all movies gathered, now we build the movie Catalog in order
            // to ease search manipulation when required and to speed data processing.
            // The following steps must be fullfilled in order to gather all of the movie Catalog.
            // 1. Load Movie Name list
            // 2. Movie Formats Name list
            // 3. Movie Genre Name list
            // 4. Load all theaters referenced in the schedule.
            // 5. Load All Theater Name List and its associated movies.
            // 6. Load Movie Name List and its associated theaters where each movie is being shown.

            // Let's start.
            MovieCatalog mc = new MovieCatalog();
            mc.theaters = new List<MovieLocationShort>();
            mc.formats = new List<MovieFormatShort>();
            mc.movies = new List<MovieShortFormat>();
            mc.genres = new List<MovieGenreShort>();

            // 1. Load Movie Name list
            movieList.ForEach(movie => {
                MovieShortFormat movieShort = new MovieShortFormat() {
                    id = movie.id,
                    name = movie.name,
                    premiere = movie.premiere,
                    formats = new List<MovieFormatShort>()
                };

                // Now load formats 
                movie.locations.ForEach(movieLocation =>
                movieLocation.formats.ForEach(
                    movieFormat => {
                        var formatExist = movieShort.formats.Where(x => x.id == movieFormat.id).FirstOrDefault<MovieFormatShort>();
                        if (formatExist == null) {
                            movieShort.formats.Add(new MovieFormatShort() { id = movieFormat.id, name = movieFormat.name });
                        }
                    }
                ));
                movieShort.formats = movieShort.formats.OrderBy(x => x.name).ToList<MovieFormatShort>();
                mc.movies.Add(movieShort);
            });
            mc.movies = mc.movies.OrderBy(x => x.name).ToList<MovieShortFormat>();
            // End 1.Load Movie Name List

            // 2. Movie Formats Name list and 3. Movie Genre Name list and 4. Load all theaters referenced in the schedule.
            movieFullList.ForEach(movie => {
                var movieFormatExist = mc.formats.Where(x => x.id == movie.idFormat).FirstOrDefault<MovieFormatShort>();
                if (movieFormatExist == null) {
                    mc.formats.Add(new MovieFormatShort() { id = movie.idFormat, name = movie.nameFormat });
                }

                var movieGenreExist = mc.genres.Where(x => x.id == movie.idGenre).FirstOrDefault<MovieGenreShort>();
                if (movieGenreExist == null) {
                    mc.genres.Add(new MovieGenreShort() { id = movie.idGenre, name = movie.genre });
                }

                var movieTheaterExist = mc.theaters.Where(x => x.id == movie.idLocation).FirstOrDefault<MovieLocationShort>();
                if (movieTheaterExist == null) {
                    mc.theaters.Add(new MovieLocationShort() { id = movie.idLocation, name = movie.nameLocation, branchName = movie.branchName, nameFull = movie.nameFullLocation });
                }
            });
            mc.formats = mc.formats.OrderBy(x => x.name).ToList<MovieFormatShort>();
            mc.genres = mc.genres.OrderBy(x => x.name).ToList<MovieGenreShort>();
            mc.theaters = mc.theaters.OrderBy(x => x.nameFull).ToList<MovieLocationShort>();
            // End 2. Movie Formats Name list and 3. Movie Genre Name List and 4. Load all theaters referenced in the schedule.

            // 5. Load All Theater Name List and its associated movies.

            // Initialize 
            Dictionary<int, List<MovieLocationShort>> movieInTheaters = new Dictionary<int, List<MovieLocationShort>>();
            movieList.ForEach(movie => movieInTheaters.Add(movie.id, new List<MovieLocationShort>()));

            // Now fill locations per movie.
            movieList.ForEach(movie => {
                var movieTheaterSelected = movieInTheaters[movie.id];
                movie.locations.ForEach(loc => movieTheaterSelected.Add(new MovieLocationShort() { id = loc.id, name = loc.name, branchName = loc.branchName, nameFull = loc.nameFull }));
                movieTheaterSelected = movieTheaterSelected.OrderBy(x => x.nameFull).ToList<MovieLocationShort>();
            });

            // Now store in catalog.
            mc.movieInTheaters = movieInTheaters;
            // End 5. Load All Theater Name List and its associated movies.

            // 6. Load Movie Name List and its associated theaters where each movie is being shown.

            // Initialize theaterMovies for computation.
            // Here we use theaterMoviesDictionary temporary variable so in the process it can be build orderd both
            // its keys and the values it contains.
            Dictionary<string, List<MovieShortFormat>> theaterMoviesDictionary = new Dictionary<string, List<MovieShortFormat>>();

            // Initialize theaterMoviesDictionary with keys ordered by name.
            List<string> theaterNameList = new List<string>();

            mc.theaters.ForEach(t => theaterNameList.Add(t.nameFull));
            theaterNameList = theaterNameList.OrderBy(t => t).ToList<string>();

            // Now that we have a theaterNameList  and ordered, we can now start building the Dictionary values.
            // Initialize theaterMoviesDictionary dictionary for further processing.
            theaterNameList.ForEach(t => theaterMoviesDictionary.Add(t, new List<MovieShortFormat>()));

            // Let's add values to theaterMoviesDictionary
            movieList.ForEach(movie => {
                movie.locations.ForEach(location => {
                    // Retrieve theater item from dictionary
                    var theaterItem = theaterMoviesDictionary[location.nameFull];
                    var existMovie = theaterItem.Where(x => x.id == movie.id).FirstOrDefault<MovieShortFormat>();
                    if (existMovie == null) {
                        // WARNING!: At this point and supported by mc.movies (which must be already compiled at this point.
                        var movieFormats = mc.movies.Where(x => x.id == movie.id).FirstOrDefault();
                        theaterItem.Add(new MovieShortFormat() { id = movie.id, premiere = movie.premiere, name = movie.name, formats = movieFormats.formats });
                    }
                });
            });

            // Now it is time to copy contents from theaterMoviesDictionary to mc.theaterMovies
            // When traversing its contents the list is sorted as well by name.
            mc.theaterMovies = new Dictionary<string, List<MovieShortFormat>>();
            foreach (var pair in theaterMoviesDictionary) {
                mc.theaterMovies.Add(pair.Key, pair.Value.OrderBy(m => m.name).ToList<MovieShortFormat>());
            }

            // End 6. Load Movie Name List and its associated theaters where each movie is being shown.

            // Serialize
            string movieLookupJSON = JsonConvert.SerializeObject(movieList);
            string movieCatalogJSON = JsonConvert.SerializeObject(mc);

            // Now that we have just  gathered all the information, create static JSON versions
            // Now there are two files to consume the feed

            // Full movie catalog (mapped from origin).
            string fileName = moviesFileName;
            using (StreamWriter writer = new StreamWriter(fileName)) {
                writer.Write(movieLookupJSON);
            }

            fileName = catalogNameFileName;
            using (StreamWriter writer = new StreamWriter(fileName)) {
                writer.Write(movieCatalogJSON);
            }
            if (log.IsDebugEnabled) {
                log.Debug("Info compiled");
                log.Debug("movieLookupJSON=[" + movieLookupJSON + "]");
                log.Debug("movieCatalogJSON=[" + movieCatalogJSON + "]");
                log.Debug("CompileAllMoviesSchedule Ends");
            }
        }

        /// <summary>
        /// Given parameters 't:for theater id', 'm:Movie Id', and 'g:Gender id', computes a lookup in the catalog
        /// matched records. All results must be guaranteed to be sorted.
        /// </summary>
        /// <param name="t">Theater Id. Possible value are '-1', or '2'</param>
        /// <param name="m">Movie Id. Possible value are '-1', or '1|29' this means movieId=1 and movieIdFormat=29</param>
        /// <param name="g">Gender Id. Possible value are '-1', or '2'</param>
        /// <returns>A list of 'Movie'  objects serialized as JSON. If list is empty, serialized should be empty.</returns>
        public string Search(string t, string m, string g) {
            if (log.IsDebugEnabled) {
                log.Debug("Search Starts");
                log.Debug("t=[" + t + "]");
                log.Debug("m=[" + m + "]");
                log.Debug("g=[" + g + "]");
            }
            int theaterId = Convert.ToInt32(t);
            int movieId = 0;
            int movieIdFormat = 0; // Only used where movieId is not set to -1
            int genreId = Convert.ToInt32(g);
            string rslt = "";

            // We must split 'm' parameter to its constituent values.
            // If m = -1", no need to split
            if (m == "-1") {
                movieId = Convert.ToInt32(m);
            }
            else {
                var movieValuesArray = m.Split('|');
                if (movieValuesArray.Length == 2) {
                    try {
                        movieId = Convert.ToInt32(movieValuesArray[0]);
                    } catch (Exception) {
                        movieId = -1;
                    }
                    movieIdFormat = Convert.ToInt32(movieValuesArray[1]);
                }
                else {
                    try {
                        movieId = Convert.ToInt32(movieValuesArray[0]);
                    } catch (Exception) {
                        movieId = -1;
                    }
                    movieIdFormat = -1;
                }
            }

            List<Movie> movieList = RetrieveMovieList();
            MovieCatalog mc = RetriveMovieCatalog();
            if (theaterId == -1 && movieId == -1 && genreId == -1) {
                // Retrieves all records.
                // For the purpose, we need to return Movie objects with its location but not its formats and schedule                
                List<Movie> searchMovieList = new List<Movie>();
                movieList.ForEach(FillMovieDataWithoutLocationSchedule(searchMovieList));
                searchMovieList = searchMovieList.OrderByDescending(x => x.premiere).ThenBy(x => x.name).ToList<Movie>();
                rslt = JsonConvert.SerializeObject(searchMovieList);
            }
            else {
                if (theaterId == -1 && movieId == -1 && genreId != -1) {
                    // Search only by Genre
                    List<Movie> searchMovieList = new List<Movie>();
                    var movieByGenreList = movieList.Where(x => x.idGenre == genreId).ToList<Movie>();
                    movieByGenreList.ForEach(FillMovieDataWithoutLocationSchedule(searchMovieList));
                    searchMovieList = searchMovieList.OrderByDescending(x => x.premiere).ThenBy(x => x.name).ToList<Movie>();
                    rslt = JsonConvert.SerializeObject(searchMovieList);
                }
                else {
                    if (theaterId != -1 && movieId != -1 && genreId == -1) {
                        // Search a specific movie by theater. Any Genre.
                        // No idFormat is required herein.

                        // Select movie and theater.
                        var movieSelected = movieList.Where(x => x.id == movieId).FirstOrDefault<Movie>();
                        var movieTheaterSelected = movieSelected.locations.Where(x => x.id == theaterId).FirstOrDefault<MovieLocation>();

                        // Build return movie object
                        Movie movieReturn = new Movie() {
                            id = movieSelected.id,
                            name = movieSelected.name,
                            img = movieSelected.img,
                            url = movieSelected.url,
                            active = movieSelected.active,
                            idGenre = movieSelected.idGenre,
                            genre = movieSelected.genre,
                            premiere = movieSelected.premiere,
                            createDate = movieSelected.createDate
                        };
                        rslt = JsonConvert.SerializeObject(movieReturn);
                    }
                    else {
                        if (theaterId == -1 && movieId != -1 && genreId == -1) {
                            // Search a movie and give me all the locations for it.
                            // Only one movie must be selected.
                            // Movie record must match criteria for movieId
                            var movieSelected = movieList.Where(x => x.id == movieId).FirstOrDefault<Movie>();
                            Movie movieReturn = new Movie() {
                                id = movieSelected.id,
                                name = movieSelected.name,
                                img = movieSelected.img,
                                url = movieSelected.url,
                                active = movieSelected.active,
                                idGenre = movieSelected.idGenre,
                                genre = movieSelected.genre,
                                premiere = movieSelected.premiere,
                                createDate = movieSelected.createDate
                            };

                            // Add the location but without its schedule
                            rslt = JsonConvert.SerializeObject(movieReturn);
                        }
                        else {
                            if (theaterId != -1 && movieId == -1 && genreId == -1) {
                                // Find all movies by given theaterId
                                // Remove duplicates in the process.
                                // This search is best suited by using the MovieCatalog object, as required
                                // objects are compiled to do so.
                                List<Movie> searchMovieList = new List<Movie>();
                                var theaterInfo = mc.theaters.Where(x => x.id == theaterId).FirstOrDefault<MovieLocationShort>();
                                if (theaterInfo == null) {
                                    // Return nothing as no record is found.
                                    rslt = JsonConvert.SerializeObject(searchMovieList);
                                }
                                else {
                                    // Retrieve Movie List by theater from theaterMovies
                                    var moviesByTheaterList = mc.theaterMovies[theaterInfo.nameFull];

                                    // Compile required information.
                                    moviesByTheaterList.ForEach(movie => {
                                        // Before creating the movie in 'searchMovieList', it must
                                        // be retrived from 'movieList' as 'movie' item is a short version of what
                                        // we require.
                                        var movieComplete = movieList.Where(x => x.id == movie.id).FirstOrDefault<Movie>();
                                        Movie mv = new Movie() {
                                            id = movieComplete.id,
                                            name = movieComplete.name,
                                            img = movieComplete.img,
                                            url = movieComplete.url,
                                            active = movieComplete.active,
                                            idGenre = movieComplete.idGenre,
                                            genre = movieComplete.genre,
                                            premiere = movieComplete.premiere,
                                            createDate = movieComplete.createDate
                                        };
                                        searchMovieList.Add(mv);
                                    });
                                }
                                searchMovieList = searchMovieList.OrderByDescending(x => x.premiere).ThenBy(x => x.name).ToList<Movie>();
                                rslt = JsonConvert.SerializeObject(searchMovieList);
                            }
                        }
                    }
                }
            }
            if (log.IsDebugEnabled) {
                log.Debug("Rslt=[" + rslt + "]");
                log.Debug("Search Ends");
            }
            return rslt;
        }

        /// <summary>
        /// Helper method to load a JSON file and deserialized to proper structure. NOTE: This is the movie list catalog.
        /// </summary>
        /// <returns>A list of Movie objects</returns>
        public List<Movie> RetrieveMovieList() {
            if (log.IsDebugEnabled) {
                log.Debug("RetrieveMovieList Starts");
            }
            string s;
            using (StreamReader reader = new StreamReader(moviesFileName)) {
                s = reader.ReadToEnd();
            }
            if (log.IsDebugEnabled) {
                log.Debug("Retrieved Contents=[" + s + "]");
                log.Debug("RetrieveMovieList Ends");
            }
            return JsonConvert.DeserializeObject<List<Movie>>(s);
        }

        /// <summary>
        /// Helper method to load a JSON file and deserialized to proper structure. NOTE: This is the movie catalog.
        /// </summary>
        /// <returns>An object of type MovieCatalog that contains helper information when consumed by third-party.</returns>
        public MovieCatalog RetriveMovieCatalog() {
            if (log.IsDebugEnabled) {
                log.Debug("RetriveMovieCatalog Starts");
            }
            MovieCatalog mc = null;
            string s;
            using (StreamReader reader = new StreamReader(catalogNameFileName)) {
                s = reader.ReadToEnd();
            }
            mc = JsonConvert.DeserializeObject<MovieCatalog>(s);
            if (log.IsDebugEnabled) {
                log.Debug("Retrieved Contents=[" + s + "]");
                log.Debug("RetriveMovieCatalog Ends");
            }
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
        public string RetrieveJSonContentsForModule(string m) {
            if (log.IsDebugEnabled) {
                log.Debug("RetrieveJSonContentsForModule Starts");
                log.Debug("Requested module m=[" + m + "]");
            }
            string rslt = "";
            switch (m) {
                case "1":
                    rslt = JsonConvert.SerializeObject(RetrieveMovieList());
                    break;
                case "2":
                    rslt = JsonConvert.SerializeObject(RetriveMovieCatalog());
                    break;
                default:
                    break;
            }
            if (log.IsDebugEnabled) {
                log.Debug("RetrieveJSonContentsForModule Ends");
            }
            return rslt;
        }
    }
}
