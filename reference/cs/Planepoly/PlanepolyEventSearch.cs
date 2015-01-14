using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace ElColombiano.Planepoly
{
    public abstract class PlanepolyEventSearch
    {
        /// <summary>
        /// It makes a search for movies that meets the search criteria.
        /// Receives three parameters.
        /// t=Theater: Holds the theater to look for.
        /// m=Movie: Holds the movie to look for.
        /// g=Genre: HOlds the genre to look for.
        /// </summary>
        public static void Search(HttpRequest Request, HttpResponse Response)
        {
            string movieSearchListJSON = "";
            string fileNameMovies = @"D:\SitiosWeb\Sitio\EC100A_Servicios\EC100A_PlanepolyWidget\planepoly-events.json";
            string t = Request.QueryString["t"];
            string m = Request.QueryString["m"];
            string g = Request.QueryString["g"];
            string s;
            using (StreamReader reader = new StreamReader(fileNameMovies))
            {
                s = reader.ReadToEnd();
            }

            if ((t == null && m == null & g == null) || (t == "-1" && m == "-1" && g == "-1"))
            {
                // Returns all movies available.
                // All movies are expanded in order to give to each movie all of its locations.
                List<MovieLookup> movieLookupList = JsonConvert.DeserializeObject<List<MovieLookup>>(s);
                List<MovieLookup> movieLookupAllList = new List<MovieLookup>();

                foreach (var movieInfo in movieLookupList)
                {
                    MovieLookup ml = new MovieLookup();
                    ml.name = movieInfo.name;
                    ml.img = movieInfo.img;
                    ml.url = movieInfo.url;
                    ml.premiere = movieInfo.premiere;
                    ml.genre = movieInfo.genre;
                    movieLookupAllList.Add(ml);
                }
                movieLookupAllList = (from item in movieLookupAllList
                                      orderby item.premiere descending, item.name
                                      select item).Distinct().ToList();
                string allMoviesJSON = JsonConvert.SerializeObject(movieLookupAllList);
                movieSearchListJSON = allMoviesJSON;
            }
            else
            {
                List<MovieLookup> movieSearchList = new List<MovieLookup>();
                List<MovieLookup> movieLookupList = JsonConvert.DeserializeObject<List<MovieLookup>>(s);
                List<MovieLookup> movieByGenre = null;
                List<MovieLookup> movieByMovie = null;
                List<MovieLookup> movieByTheater = null;
                List<MovieLookup> movieByTheaterMovie = null;

                if (t == "-1" && m == "-1" && g != "-1")
                {
                    // Search only by genre
                    var movieGenreList = (from item in movieLookupList
                                          where item.genre == g
                                          select item).ToList();

                    // Now we have all movies by genre, now it is needed to expand by location.
                    movieByGenre = new List<MovieLookup>();
                    foreach (var ml in movieGenreList)
                    {
                        // Sort locations
                        var sortedLocations = (from l in ml.locations
                                               orderby l.name
                                               select l).ToList();
                        foreach (var l in sortedLocations)
                        {
                            MovieLookup movieLookupInfo = new MovieLookup();
                            movieLookupInfo.name = ml.name;
                            movieLookupInfo.img = ml.img;
                            movieLookupInfo.url = ml.url;
                            movieLookupInfo.premiere = ml.premiere;
                            movieLookupInfo.genre = ml.genre;

                            List<MovieLookupLocation> foundMovieLocationList = new List<MovieLookupLocation>();
                            MovieLookupLocation loc = new MovieLookupLocation();
                            loc.name = l.name;
                            loc.address = l.address;
                            loc.schedule = l.schedule;
                            foundMovieLocationList.Add(loc);
                            movieLookupInfo.locations = foundMovieLocationList;

                            movieByGenre.Add(movieLookupInfo);
                        }
                    }
                    movieByGenre = (from it in movieByGenre
                                    orderby it.premiere descending, it.name
                                    select it).Distinct().ToList();
                }
                else
                {
                    if (t != "-1" && m != "-1")
                    {
                        // Search both theater and movie in that theater                        
                        movieByTheaterMovie = new List<MovieLookup>();
                        foreach (var ml in movieLookupList)
                        {
                            var sortedLocations = (from loc in ml.locations
                                                   orderby loc.name
                                                   select loc).Distinct().ToList();
                            foreach (var l in sortedLocations)
                            {
                                if (l.name == t && ml.name == m)
                                {
                                    MovieLookup foundMovie = new MovieLookup();
                                    foundMovie.name = ml.name;
                                    foundMovie.img = ml.img;
                                    foundMovie.url = ml.url;
                                    foundMovie.premiere = ml.premiere;
                                    foundMovie.genre = ml.genre;

                                    List<MovieLookupLocation> foundMovieLocationList = new List<MovieLookupLocation>();
                                    foundMovieLocationList.Add(l);
                                    foundMovie.locations = foundMovieLocationList;
                                    movieByTheaterMovie.Add(foundMovie);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (t == "-1" && m != "-1")
                        {
                            // Search only by movie name (this gives many movies in different locations --aka theaters).
                            movieByMovie = new List<MovieLookup>();
                            var existingMoviesList = (from item in movieLookupList
                                                      where item.name == m
                                                      select item).ToList();

                            // The existing movie must be expanded one item by all of its locations.
                            foreach (var ml in existingMoviesList)
                            {
                                var sortedLocations = (from loc in ml.locations
                                                       orderby loc.name
                                                       select loc).Distinct().ToList();
                                foreach (var l in sortedLocations)
                                {
                                    MovieLookup expando = new MovieLookup();
                                    expando.name = ml.name;
                                    expando.img = ml.img;
                                    expando.url = ml.url;
                                    expando.premiere = ml.premiere;
                                    expando.genre = ml.genre;

                                    List<MovieLookupLocation> expandoLocationList = new List<MovieLookupLocation>();
                                    expandoLocationList.Add(l);
                                    expando.locations = expandoLocationList;
                                    movieByMovie.Add(expando);
                                }
                            }
                            movieByMovie = (from ml in movieByMovie
                                            orderby ml.premiere descending, ml.name
                                            select ml).Distinct().ToList();
                        }
                        else
                        {
                            // Search only by theater
                            movieByTheater = new List<MovieLookup>();
                            foreach (var ml in movieLookupList)
                            {
                                var sortedLocations = (from loc in ml.locations
                                                       orderby loc.name
                                                       select loc).Distinct().ToList();
                                foreach (var l in sortedLocations)
                                {
                                    if (l.name == t)
                                    {
                                        MovieLookup foundMovie = new MovieLookup();
                                        foundMovie.name = ml.name;
                                        foundMovie.img = ml.img;
                                        foundMovie.url = ml.url;
                                        foundMovie.premiere = ml.premiere;
                                        foundMovie.genre = ml.genre;

                                        List<MovieLookupLocation> foundMovieLocationList = new List<MovieLookupLocation>();
                                        foundMovieLocationList.Add(l);
                                        foundMovie.locations = foundMovieLocationList;
                                        movieByTheater.Add(foundMovie);
                                    }
                                }
                            }
                        }
                    }
                }
                if (movieByGenre != null)
                {
                    movieSearchList.AddRange(movieByGenre);
                }
                if (movieByTheater != null)
                {
                    movieSearchList.AddRange(movieByTheater);
                }
                if (movieByTheaterMovie != null)
                {
                    movieSearchList.AddRange(movieByTheaterMovie);
                }
                if (movieByMovie != null)
                {
                    movieSearchList.AddRange(movieByMovie);
                }
                movieSearchList = (from item in movieSearchList
                                   orderby item.premiere descending, item.name
                                   select item).Distinct().ToList();
                movieSearchListJSON = JsonConvert.SerializeObject(movieSearchList);
            }

            Response.Write(movieSearchListJSON);
            Response.AddHeader("Access-Control-Allow-Origin", "*");
        }
    }
}
