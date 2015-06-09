/*==========================================================================*/
/* Source File:   PLANEPOLYMOVIES.ASPX.CS                                   */
/* Description:   Using this service page to gather all of the information  */
/*                about events defined in the Planepoly site and            */
/*                manipulated by means of JSON format.                      */
/*                In fact this is the Planepoly to EL Colombiano JSON       */
/*                structure mapping.                                        */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          May.14/2013                                               */
/* Last Modified: Jun.18/2013                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
May.14/2013 COQ File created.
============================================================================*/
using ElColombiano.Service.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace ElColombiano.Service
{
    /// <summary>
    ///  Using this service page to gather all of the information
    ///  about events defined in the Planepoly site and          
    ///  manipulated by means of JSON format.                    
    ///  In fact this is the Planepoly to EL Colombiano JSON     
    ///  structure mapping.                                      
    /// </summary>
    public partial class PlanepolyMovies : System.Web.UI.Page
    {
        /// <summary>
        /// Generates the internal JSON representation to be used internally in EL COLOMBIANO web pages. The topic is the movies category.
        /// </summary>
        /// <param name="sender">Sender object which fired the event</param>
        /// <param name="e">Parameters sent from the event manager.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            WebClient client = new WebClient();
            string url = "https://www.planepoly.com:8181/PlanepolyCoreWeb/OSConcierge?lat=6.210506&long=-75.57096&idTipos=201&consulta=eventos&bypass=626262";
            Stream data = client.OpenRead(url);
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();
            data.Close();
            reader.Close();
            Response.AddHeader("Access-Control-Allow-Origin", "*");

            var movieList = JsonConvert.DeserializeObject<Movie>(s);
            var movieServiceList = movieList.servicios;
            foreach (var it in movieServiceList)
            {
                it.nombre = it.nombre.Trim();
            }
            var movieListOrdered = movieServiceList.OrderByDescending(x => x.estr).OrderBy(x => x.nombre).ToList();
            movieList.servicios = movieListOrdered;

            List<string> theaterNameList = new List<string>();
            movieList.servicios.ForEach(x => x.ptos.Select(y => y.nombre).ToList().ForEach(z => theaterNameList.Add(z)));
            var theatersList = theaterNameList.Distinct().OrderBy(y => y).ToList();
            var genresList = (from service in movieServiceList
                              orderby service.genero
                              select service.genero).Distinct().ToList<String>();
            var allMovieNameList = (from service in movieServiceList
                                 orderby service.nombre
                                 select service.nombre).Distinct().ToList<string>();
            MovieCatalog movieCatalog = new MovieCatalog();
            movieCatalog.theaters = theatersList;
            movieCatalog.genres = genresList;
            movieCatalog.movies = allMovieNameList;

            Dictionary<string, List<string>> theaterMoviesList = new Dictionary<string, List<string>>();
            foreach (var item in movieCatalog.theaters)
            {
                List<string> movieNameList = new List<string>();
                theaterMoviesList.Add(item, movieNameList);
            }

            // Creates now a list of all movies mapped to our structure, given
            // the Planepoly JSON structure.
            List<MovieLookup> movieLookupList = new List<MovieLookup>();
            foreach (var service in movieList.servicios)
            {
                MovieLookup movieLookup = new MovieLookup();
                movieLookup.name = service.nombre;
                movieLookup.img = service.img;
                movieLookup.url = service.url;
                movieLookup.premiere = service.estr;
                movieLookup.genre = service.genero;
                movieLookup.locations = new List<MovieLookupLocation>();
                foreach (var location in service.ptos)
                {
                    if (theaterMoviesList.ContainsKey(location.nombre))
                    {
                        var tempList = theaterMoviesList[location.nombre];
                        tempList.Add(service.nombre);
                    }

                    MovieLookupLocation mlLocation = new MovieLookupLocation();
                    mlLocation.name = location.nombre;
                    mlLocation.address = location.direccion;


                    List<MovieLookupShow> mlShowList = new List<MovieLookupShow>();
                    Dictionary<int, string> shows = new Dictionary<int, string>();
                    shows.Add(0, "");
                    shows.Add(1, "");
                    shows.Add(2, "");
                    shows.Add(3, "");
                    shows.Add(4, "");
                    shows.Add(5, "");
                    shows.Add(6, "");
                    shows.Add(7, "");
                    foreach (var show in location.funcs)
                    {
                        if (shows.ContainsKey(show.dia))
                        {
                            var valDay = shows[show.dia];
                            valDay += show.hora + " ";
                            shows[show.dia] = valDay;
                        }
                    }
                    foreach (var it in shows)
                    {
                        if (it.Value.Trim() != "")
                        {
                            MovieLookupShow mls = new MovieLookupShow();
                            mls.frequency = it.Key;
                            mls.hours = it.Value.Trim();
                            switch (mls.frequency)
                            {
                                case 0:
                                    mls.name = "Diario";
                                    break;
                                case 1:
                                    mls.name = "Lunes";
                                    break;
                                case 2:
                                    mls.name = "Martes";
                                    break;
                                case 3:
                                    mls.name = "Miércoles";
                                    break;
                                case 4:
                                    mls.name = "Jueves";
                                    break;
                                case 5:
                                    mls.name = "Viernes";
                                    break;
                                case 6:
                                    mls.name = "Sábado";
                                    break;
                                case 7:
                                    mls.name = "Domingo";
                                    break;
                                default:
                                    break;
                            }
                            mlShowList.Add(mls);
                        }
                    }
                    mlLocation.schedule = mlShowList;
                    movieLookup.locations.Add(mlLocation);

                }
                movieLookupList.Add(movieLookup);
            }
            foreach (var it in theaterMoviesList)
            {
                it.Value.Sort();
            }
            movieCatalog.theaterMovies = theaterMoviesList;

            var movieLookupListOrdered = (from item in movieLookupList
                                          orderby item.premiere descending, item.name
                                          select item).ToList();
            string movieCatalogJSON = JsonConvert.SerializeObject(movieCatalog);
            string movieLookupJSON = JsonConvert.SerializeObject(movieLookupListOrdered);
            
            // Now that we have just  gathered all the information, create static JSON versions
            // Now there are two files to consume the feed

            // Full movie (mapped from origin).
            string fileName = @"D:\SitiosWeb\Sitio\EC100A_Servicios\EC100A_PlanepolyWidget\planepoly-movies.json";
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.Write(movieLookupJSON);
            }

            // Full movie catalog (mapped from origin)
            fileName = @"D:\SitiosWeb\Sitio\EC100A_Servicios\EC100A_PlanepolyWidget\planepoly-movies-catalog.json";
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.Write(movieCatalogJSON);
            }

            // This is the page result.
            Response.Write(movieLookupJSON);
        }
    }
}