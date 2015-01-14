using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using Newtonsoft.Json;

namespace ElColombiano.Planepoly
{
    public abstract class PlanepolyEvent
    {
        static MovieCatalog eventCatalog = new MovieCatalog();   
        public static string CallService(string Tipos)
        {
            WebClient client = new WebClient();
            string url = "https://www.planepoly.com:8181/PlanepolyCoreWeb/OSConcierge?lat=6.210506&long=-75.57096&idTipos=" + Tipos + "&consulta=eventos&bypass=626262";
            Stream data = client.OpenRead(url);
            StreamReader reader = new StreamReader(data);
            string Rpta = reader.ReadToEnd();
            data.Close();
            reader.Close();
            return Rpta;
        }

        private static string ReadHtmlPageContent(string url)
        {
            WebClient client = new WebClient();
            Stream data = client.OpenRead(url);
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();
            data.Close();
            reader.Close();
            return (s);
        }

        /// <summary>
        /// Gets the description for parameter 'type' given.
        /// </summary>
        /// <param name="type">integer value to map</param>
        /// <returns>Empty if no match, else description for given parameter.</returns>
        private static string GetTypeName(int type)
        {
            string s = "";
            switch (type)
            {
                case 202:
                    s = "Obras de Teatro";
                    break;
                case 203:
                    s = "Conciertos";
                    break;
                case 209:
                    s = "Culturales";
                    break;
            }
            return s;
        }

        /// <summary>
        /// Transform the JSON structure given by Planepoly (using EventService class) to our own JSON structure.
        /// </summary>
        /// <param name="eventPlanepolyServiceList">List of records to map.</param>
        /// <returns>A list of our own JSON structure (using EventLookup) class)</returns>
        private static List<EventLookup> TransformEventServiceToEventLookup(List<EventService> eventPlanepolyServiceList)
        {
            // Creates now a list of all events mapped to our structure, given
            // the Planepoly JSON structure.
            List<EventLookup> eventLookupList = new List<EventLookup>();
            foreach (var service in eventPlanepolyServiceList)
            {
                EventLookup eventLookup = new EventLookup();
                eventLookup.name = service.nombre.Trim();
                eventLookup.img = service.img;
                eventLookup.url = service.url;
                eventLookup.premiere = service.estr;
                eventLookup.genre = service.genero;
                eventLookup.type = service.tipo;
                eventLookup.typeName = GetTypeName(eventLookup.type);
                eventLookup.locations = new List<EventLookupLocation>();
                foreach (var location in service.ptos)
                {
                    EventLookupLocation elLocation = new EventLookupLocation();
                    elLocation.name = location.nombre;
                    elLocation.address = location.direccion;

                    List<EventLookupShow> elShowList = new List<EventLookupShow>();
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
                            EventLookupShow mls = new EventLookupShow();
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
                            elShowList.Add(mls);
                        }
                    }
                    elLocation.schedule = elShowList;
                    eventLookup.locations.Add(elLocation);

                }
                eventLookupList.Add(eventLookup);
            }
            return eventLookupList;
        }

        public static void ProccesPlanepolyEvent(HttpRequest Request, HttpResponse Response)
        {
            List<EventService> eventPlanepolyServiceList = new List<EventService>();                        
            string[] urls = {"https://www.planepoly.com:8181/PlanepolyCoreWeb/OSConcierge?lat=6.210506&long=-75.57096&idTipos=202&consulta=eventos&bypass=626262" /* Refers to 'Obras de Teatro' at Planepoly.com */,
                            "https://www.planepoly.com:8181/PlanepolyCoreWeb/OSConcierge?lat=6.210506&long=-75.57096&idTipos=203&consulta=eventos&bypass=626262"  /* Refers to 'Conciertos' at Planepoly.com */,
                            "https://www.planepoly.com:8181/PlanepolyCoreWeb/OSConcierge?lat=6.210506&long=-75.57096&idTipos=209&consulta=eventos&bypass=626262"  /* Refers to 'Culturales' at Planepoly.com */ };
            //string[] urls = { "https://www.planepoly.com:8181/PlanepolyCoreWeb/OSConcierge?lat=6.210506&long=-75.57096&idTipos=203&consulta=eventos&bypass=626262" /* Refers to 'Obras de Teatro' at Planepoly.com */};
            string s = "";
            try
            {


                for (int i = 0; i < urls.Length; i++)
                {
                    s = ReadHtmlPageContent(urls[i]); 
                    Event o = JsonConvert.DeserializeObject<Event>(s);
                    foreach (var item in o.servicios)
                    {
                        eventPlanepolyServiceList.Add(item);
                    }
                }
            }
            catch (Exception e)
            {
                Response.Write("{0} Exception caught." + e.ToString());
            }
            
            Response.AddHeader("Access-Control-Allow-Origin", "*");            
            // Get data to be stored in file                        
            List<EventLookup> eventLookupList = TransformEventServiceToEventLookup(eventPlanepolyServiceList);
            List<string> theaterNameList = new List<string>();
            eventPlanepolyServiceList.ForEach(x => x.ptos.Select(y => y.nombre).ToList().ForEach(z => theaterNameList.Add(z)));
            var theatersList = theaterNameList.Distinct().OrderBy(y => y).ToList();
            var genresList = (from service in eventPlanepolyServiceList
                              orderby service.genero
                              select service.genero).Distinct().ToList<String>();
            var allMovieNameList = (from service in eventPlanepolyServiceList
                                    orderby service.nombre
                                    select service.nombre).Distinct().ToList<string>();
            MovieCatalog eventCatalog = new MovieCatalog();
            eventCatalog.theaters = theatersList;
            eventCatalog.genres = genresList;
            eventCatalog.movies = allMovieNameList;

            Dictionary<string, List<string>> theaterMoviesList = new Dictionary<string, List<string>>();
            foreach (var item in eventCatalog.theaters)
            {
                List<string> movieNameList = new List<string>();
                theaterMoviesList.Add(item, movieNameList);
            }
            eventCatalog.theaterMovies = theaterMoviesList;
            foreach (var service in eventPlanepolyServiceList)
            {
                EventLookup eventLookup = new EventLookup();
                eventLookup.name = service.nombre;
                eventLookup.img = service.img;
                eventLookup.url = service.url;
                eventLookup.premiere = service.estr;
                eventLookup.genre = service.genero;
                eventLookup.locations = new List<EventLookupLocation>();
                foreach (var location in service.ptos)
                {
                    if (theaterMoviesList.ContainsKey(location.nombre))
                    {
                        var tempList = theaterMoviesList[location.nombre];
                        tempList.Add(service.nombre);
                    }
                }
            }
            eventLookupList = (from item in eventLookupList
                               orderby item.premiere descending, item.typeName, item.name
                               select item).Distinct().ToList();

            string eventLookupJSON = JsonConvert.SerializeObject(eventLookupList);
            string eventCatalogJSON = JsonConvert.SerializeObject(eventCatalog);

            // Full movie (mapped from origin)
            string fileName = @"D:\SitiosWeb\Sitio\EC100A_Servicios\EC100A_PlanepolyWidget\planepoly-events.json";
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.Write(eventLookupJSON);
            }

            fileName = @"D:\SitiosWeb\Sitio\EC100A_Servicios\EC100A_PlanepolyWidget\planepoly-events-catalog.json";
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.Write(eventCatalogJSON);
            }
            // This is the page result.            
            //Response.Write(eventLookupJSON);
            Response.Write(eventCatalogJSON);
        }

        public static void ProccesPlanepolyEventNew(HttpRequest Request, HttpResponse Response)
        {
            /*WebClient client = new WebClient();
            string url = "https://www.planepoly.com:8181/PlanepolyCoreWeb/OSConcierge?lat=6.210506&long=-75.57096&idTipos=201&consulta=eventos&bypass=626262";
            Stream data = client.OpenRead(url);
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();
            data.Close();
            reader.Close();*/
            
            string s = CallService("202");
            Response.AddHeader("Access-Control-Allow-Origin", "*");
            s = s + CallService("203");
            Response.AddHeader("Access-Control-Allow-Origin", "*");
            s = s + CallService("209");
            Response.AddHeader("Access-Control-Allow-Origin", "*");
            Response.Write("S: " + s);
            //return;
            try
            {
                var eventList = JsonConvert.DeserializeObject<Event>(s);
                var eventServiceList = eventList.servicios;
                foreach (var it in eventServiceList)
                {
                    it.nombre = it.nombre.Trim();
                }
                var eventListOrdered = eventServiceList.OrderByDescending(x => x.estr).OrderBy(x => x.nombre).ToList();
                eventList.servicios = eventListOrdered;

                List<string> theaterNameList = new List<string>();
                eventList.servicios.ForEach(x => x.ptos.Select(y => y.nombre).ToList().ForEach(z => theaterNameList.Add(z)));
                var theatersList = theaterNameList.Distinct().OrderBy(y => y).ToList();
                var genresList = (from service in eventServiceList
                                  orderby service.genero
                                  select service.genero).Distinct().ToList<String>();
                var allMovieNameList = (from service in eventServiceList
                                        orderby service.nombre
                                        select service.nombre).Distinct().ToList<string>();
                MovieCatalog eventCatalog = new MovieCatalog();
                eventCatalog.theaters = theatersList;
                eventCatalog.genres = genresList;
                eventCatalog.movies = allMovieNameList;

                Dictionary<string, List<string>> theaterMoviesList = new Dictionary<string, List<string>>();
                foreach (var item in eventCatalog.theaters)
                {
                    List<string> movieNameList = new List<string>();
                    theaterMoviesList.Add(item, movieNameList);
                }

                List<EventLookup> eventLookupList = new List<EventLookup>();
                foreach (var service in eventList.servicios)
                {
                    EventLookup eventLookup = new EventLookup();
                    eventLookup.name = service.nombre;
                    eventLookup.img = service.img;
                    eventLookup.url = service.url;
                    eventLookup.premiere = service.estr;
                    eventLookup.genre = service.genero;
                    eventLookup.locations = new List<EventLookupLocation>();
                    foreach (var location in service.ptos)
                    {
                        if (theaterMoviesList.ContainsKey(location.nombre))
                        {
                            var tempList = theaterMoviesList[location.nombre];
                            tempList.Add(service.nombre);
                        }

                        EventLookupLocation mlLocation = new EventLookupLocation();
                        mlLocation.name = location.nombre;
                        mlLocation.address = location.direccion;


                        List<EventLookupShow> mlShowList = new List<EventLookupShow>();
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
                                EventLookupShow mls = new EventLookupShow();
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
                        eventLookup.locations.Add(mlLocation);

                    }
                    eventLookupList.Add(eventLookup);
                }
                foreach (var it in theaterMoviesList)
                {
                    it.Value.Sort();
                }
                eventCatalog.theaterMovies = theaterMoviesList;

                var eventLookupListOrdered = (from item in eventLookupList
                                              orderby item.premiere descending, item.name
                                              select item).ToList();
                string eventCatalogJSON = JsonConvert.SerializeObject(eventCatalog);
                string eventLookupJSON = JsonConvert.SerializeObject(eventLookupListOrdered);
                //Response.Write(JsonConvert.SerializeObject(eventLookupListOrdered));
                Response.Write("Catalog: " + eventCatalogJSON);

                // Now that we have just  gathered all the information, create static JSON versions
                // Now there are two files to consume the feed

                // Full movie catalog (mapped from origin).
                string fileName = @"D:\SitiosWeb\Sitio\EC100A_Servicios\EC100A_PlanepolyWidget\planepoly-events.json";
                using (StreamWriter writer = new StreamWriter(fileName))
                {
                    writer.Write(eventLookupJSON);
                }

                fileName = @"D:\SitiosWeb\Sitio\EC100A_Servicios\EC100A_PlanepolyWidget\planepoly-events-catalog.json";
                using (StreamWriter writer = new StreamWriter(fileName))
                {
                    writer.Write(eventCatalogJSON);
                }
            }
            catch (Exception e)
            {
                Response.Write("{0} Exception caught." + e.ToString());
            }
        }
    }
}