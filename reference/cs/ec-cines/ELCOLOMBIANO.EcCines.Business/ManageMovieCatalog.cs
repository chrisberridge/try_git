/*==========================================================================*/
/* Source File:   MANAGEMOVIECATALOG.CS                                     */
/* Description:   Project constants                                         */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Feb.11/2015                                               */
/* Last Modified: Mar.09/2015                                               */
/* Version:       1.9                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.11/2015 COQ File created.
============================================================================*/

using ELCOLOMBIANO.EcCines.Entities;
using ELCOLOMBIANO.EcCines.Entities.Dtos;
using ELCOLOMBIANO.EcCines.Entities.Dtos.Movie;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ELCOLOMBIANO.EcCines.Business
{
    /// <summary>
    /// The purpose for this class is to be a helper to manipulate movie catalog. Dasource comes from MS Server 2012 database.
    /// Records from database are serialized in a defined folder as JSON files for easy processing.
    /// </summary>
    public class ManageMovieCatalog
    {
        private string catalogNameFileName { get; set; }
        private string moviesFileName { get; set; }
        private string imgPathUrl { get; set; }

        /// <summary>
        /// Initialize internal properties.
        /// </summary>
        /// <param name="catalogNameFileName">File name for Catalog JSON</param>
        /// <param name="moviesFileName">File name for Movie JSON</param>
        /// <param name="imgPathUrl">Used internally to prefix URL domain to images to properly locate
        /// images. NOTE: It is  optional, with a default of empty string.</param>
        public ManageMovieCatalog(string catalogNameFileName, string moviesFileName, string imgPathUrl = "")
        {
            this.catalogNameFileName = catalogNameFileName;
            this.moviesFileName = moviesFileName;
            this.imgPathUrl = imgPathUrl;
        }

        /// <summary>
        /// Main method to compute DB records to JSON files. All JSON files are stored in a
        /// configurable folder.
        /// </summary>
        public void CompileAllMoviesSchedule()
        {
            Pelicula movieDao = new Pelicula();
            List<MovieFullInfo> movieFullList = movieDao.getMovieFullinfo();

            // Normalize image to conform to URI.
            movieFullList.ForEach(m =>
            {
                if (!m.img.Contains("http"))
                {
                    m.img = imgPathUrl + m.img;
                }
            });

            // Create List of movies
            List<Movie> movieList = new List<Movie>();
            Movie movieInfo = null;
            MovieLocation movieLocationInfo = null;
            MovieFormat movieFormatInfo = null;
            MovieShow movieShowInfo = null;
            movieFullList.ForEach(mfInfo =>
            {
                // Let's first find movie existence
                var movieExist = movieList.Where(x => x.id == mfInfo.id).FirstOrDefault<Movie>();
                if (movieExist != null)
                {
                    movieInfo = movieExist;
                }
                else
                {
                    movieInfo = new Movie()
                    {
                        id = mfInfo.id,
                        name = mfInfo.name,
                        img = mfInfo.img,
                        url = mfInfo.url,
                        active = mfInfo.active,
                        idGenre = mfInfo.idGenre,
                        genre = mfInfo.genre,
                        locations = new List<MovieLocation>()
                    };
                    movieList.Add(movieInfo);
                }

                // Fill remaining data for movieInfo record
                // 1. Fill Location
                var movieLocationExist = movieInfo.locations.Where(x => x.id == mfInfo.idLocation).FirstOrDefault<MovieLocation>();
                if (movieLocationExist != null)
                {
                    movieLocationInfo = movieLocationExist;
                }
                else
                {
                    movieLocationInfo = new MovieLocation()
                    {
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
                if (movieFormatExist != null)
                {
                    movieFormatInfo = movieFormatExist;
                }
                else
                {
                    movieFormatInfo = new MovieFormat()
                    {
                        id = mfInfo.idFormat,
                        name = mfInfo.nameFormat,
                        shows = new List<MovieShow>()
                    };
                    movieLocationInfo.formats.Add(movieFormatInfo);
                }

                // 3. Fill Show
                var movieShowExist = movieFormatInfo.shows.Where(x => x.id == mfInfo.idShow).FirstOrDefault<MovieShow>();
                if (movieShowExist != null)
                {
                    movieShowInfo = movieShowExist;
                }
                else
                {
                    movieShowInfo = new MovieShow()
                    {
                        id = mfInfo.idShow,
                        dt = mfInfo.dt,
                        hours = movieDao.getMovieShowHoursFor(mfInfo.idShow)
                    };
                    movieFormatInfo.shows.Add(movieShowInfo);
                }
                movieFormatInfo.shows = movieFormatInfo.shows.OrderBy(x => x.dt).ToList<MovieShow>();
            });
            movieList = movieList.OrderBy(x => x.name).ToList<Movie>();

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
            movieList.ForEach(movie =>
            {
                MovieShortFormat movieShort = new MovieShortFormat()
                {
                    id = movie.id,
                    name = movie.name,
                    formats = new List<MovieFormatShort>()
                };

                // Now load formats 
                movie.locations.ForEach(movieLocation =>
                movieLocation.formats.ForEach(
                    movieFormat =>
                    {
                        var formatExist = movieShort.formats.Where(x => x.id == movieFormat.id).FirstOrDefault<MovieFormatShort>();
                        if (formatExist == null)
                        {
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
            movieFullList.ForEach(movie =>
            {
                var movieFormatExist = mc.formats.Where(x => x.id == movie.idFormat).FirstOrDefault<MovieFormatShort>();
                if (movieFormatExist == null)
                {
                    mc.formats.Add(new MovieFormatShort() { id = movie.idFormat, name = movie.nameFormat });
                }

                var movieGenreExist = mc.genres.Where(x => x.id == movie.idGenre).FirstOrDefault<MovieGenreShort>();
                if (movieGenreExist == null)
                {
                    mc.genres.Add(new MovieGenreShort() { id = movie.idGenre, name = movie.genre });
                }

                var movieTheaterExist = mc.theaters.Where(x => x.id == movie.idLocation).FirstOrDefault<MovieLocationShort>();
                if (movieTheaterExist == null)
                {
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
                movie.locations.ForEach(loc => movieTheaterSelected.Add(new MovieLocationShort() {id=loc.id, name = loc.name, branchName = loc.branchName, nameFull = loc.nameFull}));
                movieTheaterSelected = movieTheaterSelected.OrderBy(x=> x.nameFull).ToList<MovieLocationShort>();
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
            movieList.ForEach(movie =>
            {
                movie.locations.ForEach(location =>
                {
                    // Retrieve theater item from dictionary
                    var theaterItem = theaterMoviesDictionary[location.nameFull];
                    var existMovie = theaterItem.Where(x => x.id == movie.id).FirstOrDefault<MovieShortFormat>();
                    if (existMovie == null)
                    {
                        // WARNING!: At this point and supported by mc.movies (which must be already compiled at this point.
                        var movieFormats = mc.movies.Where(x => x.id == movie.id).FirstOrDefault();
                        theaterItem.Add(new MovieShortFormat() { id = movie.id, name = movie.name, formats = movieFormats.formats });
                    }
                });
            });

            // Now it is time to copy contents from theaterMoviesDictionary to mc.theaterMovies
            // When traversing its contents the list is sorted as well by name.
            mc.theaterMovies = new Dictionary<string, List<MovieShortFormat>>();
            foreach (var pair in theaterMoviesDictionary)
            {
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
        /// Given parameters 't:for theater id', 'm:Movie Id', and 'g:Gender id', computes a lookup in the catalog
        /// matched records. All results must be guaranteed to be sorted.
        /// </summary>
        /// <param name="t">Theater Id. Possible value are '-1', or '2'</param>
        /// <param name="m">Movie Id. Possible value are '-1', or '1|29' this means movieId=1 and movieIdFormat=29</param>
        /// <param name="g">Gender Id. Possible value are '-1', or '2'</param>
        /// <returns>A list of 'Movie'  objects serialized as JSON. If list is empty, serialized should be empty.</returns>
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
                if (movieValuesArray.Length == 2)
                {
                    try
                    {
                        movieId = Convert.ToInt32(movieValuesArray[0]);
                    }
                    catch (Exception)
                    {
                        movieId = -1;
                    }                    
                    movieIdFormat = Convert.ToInt32(movieValuesArray[1]);
                }
                else
                {
                    try
                    {
                        movieId = Convert.ToInt32(movieValuesArray[0]);
                    }
                    catch (Exception)
                    {
                        movieId = -1;
                    }                    
                    movieIdFormat = -1;
                }
            }

            List<Movie> movieList = RetrieveMovieList();
            MovieCatalog mc = RetriveMovieCatalog();
            if (theaterId == -1 && movieId == -1 && genreId == -1)
            {
                // Retrieves all records.
                // For the purpose, we need to return Movie objects with its location but not its formats and schedule                
                List<Movie> searchMovieList = new List<Movie>();
                movieList.ForEach(FillMovieDataWithoutLocationSchedule(searchMovieList));
                searchMovieList = searchMovieList.OrderBy(x => x.name).ToList<Movie>();
                rslt = JsonConvert.SerializeObject(searchMovieList);
            }
            else
            {
                if (theaterId == -1 && movieId == -1 && genreId != -1)
                {
                    // Search only by Genre
                    List<Movie> searchMovieList = new List<Movie>();
                    var movieByGenreList = movieList.Where(x => x.idGenre == genreId).ToList<Movie>();
                    movieByGenreList.ForEach(FillMovieDataWithoutLocationSchedule(searchMovieList));
                    searchMovieList = searchMovieList.OrderBy(x => x.name).ToList<Movie>();
                    rslt = JsonConvert.SerializeObject(searchMovieList);
                }
                else
                {
                    if (theaterId != -1 && movieId != -1 && genreId == -1)
                    {
                        // Search a specific movie by theater. Any Genre.
                        // No idFormat is required herein.

                        // Select movie and theater.
                        var movieSelected = movieList.Where(x => x.id == movieId).FirstOrDefault<Movie>();
                        var movieTheaterSelected = movieSelected.locations.Where(x => x.id == theaterId).FirstOrDefault<MovieLocation>();

                        // Build return movie object
                        Movie movieReturn = new Movie()
                        {
                            id = movieSelected.id,
                            name = movieSelected.name,
                            img = movieSelected.img,
                            url = movieSelected.url,
                            active = movieSelected.active,
                            idGenre = movieSelected.idGenre,
                            genre = movieSelected.genre
                        };

                        //// Add the location but without its schedule
                        //movieReturn.locations.Add(new MovieLocation()
                        //{
                        //    id = movieTheaterSelected.id,
                        //    name = movieTheaterSelected.name,
                        //    branchName = movieTheaterSelected.branchName,
                        //    nameFull = movieTheaterSelected.nameFull,
                        //    address = movieTheaterSelected.address
                        //});
                        rslt = JsonConvert.SerializeObject(movieReturn);
                    }
                    else
                    {
                        if (theaterId == -1 && movieId != -1 && genreId == -1)
                        {
                            // Search a movie and give me all the locations for it.
                            // Only one movie must be selected.
                            // Movie record must match criteria for movieId
                            var movieSelected = movieList.Where(x => x.id == movieId).FirstOrDefault<Movie>();
                            Movie movieReturn = new Movie()
                            {
                                id = movieSelected.id,
                                name = movieSelected.name,
                                img = movieSelected.img,
                                url = movieSelected.url,
                                active = movieSelected.active,
                                idGenre = movieSelected.idGenre,
                                genre = movieSelected.genre
                            };                            

                            // Add the location but without its schedule
                            rslt = JsonConvert.SerializeObject(movieReturn);
                        }
                        else
                        {
                            if (theaterId != -1 && movieId == -1 && genreId == -1)
                            {
                                // Find all movies by given theaterId
                                // Remove duplicates in the process.
                                // This search is best suited by using the MovieCatalog object, as required
                                // objects are compiled to do so.
                                List<Movie> searchMovieList = new List<Movie>();
                                var theaterInfo = mc.theaters.Where(x => x.id == theaterId).FirstOrDefault<MovieLocationShort>();
                                if (theaterInfo == null)
                                {
                                    // Return nothing as no record is found.
                                    rslt = JsonConvert.SerializeObject(searchMovieList);
                                }
                                else
                                {
                                    // Retrieve Movie List by theater from theaterMovies
                                    var moviesByTheaterList = mc.theaterMovies[theaterInfo.nameFull];

                                    // Compile required information.
                                    moviesByTheaterList.ForEach(movie =>
                                    {
                                        // Before creating the movie in 'searchMovieList', it must
                                        // be retrived from 'movieList' as 'movie' item is a short version of what
                                        // we require.
                                        var movieComplete = movieList.Where(x => x.id == movie.id).FirstOrDefault<Movie>();
                                        Movie mv = new Movie()
                                        {
                                            id = movieComplete.id,
                                            name = movieComplete.name,
                                            img = movieComplete.img,
                                            url = movieComplete.url,
                                            active = movieComplete.active,
                                            idGenre = movieComplete.idGenre,
                                            genre = movieComplete.genre                                            
                                        };                                       
                                        searchMovieList.Add(mv);
                                    });
                                }
                                searchMovieList = searchMovieList.OrderBy(x => x.name).ToList<Movie>();
                                rslt = JsonConvert.SerializeObject(searchMovieList);
                            }
                        }
                    }
                }
            }
            return rslt;
        }

        /// <summary>
        /// Helper method to load a JSON file and deserialized to proper structure. NOTE: This is the movie list catalog.
        /// </summary>
        /// <returns>A list of Movie objects</returns>
        public List<Movie> RetrieveMovieList()
        {
            string s;
            using (StreamReader reader = new StreamReader(moviesFileName))
            {
                s = reader.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<List<Movie>>(s);
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
        /// Used in search operations to fill model data.
        /// </summary>
        /// <param name="searchMovieList">The list to act upon.</param>
        /// <returns>The action to use in Lambda expressions.</returns>
        private Action<Movie> FillMovieDataWithoutLocationSchedule(List<Movie> searchMovieList)
        {
            return movie =>
            {
                Movie mv = new Movie()
                {
                    id = movie.id,
                    name = movie.name,
                    img = movie.img,
                    url = movie.url,
                    active = movie.active,
                    idGenre = movie.idGenre,
                    genre = movie.genre
                };
                searchMovieList.Add(mv);
            };
        }

    }
}
