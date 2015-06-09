/*==========================================================================*/
/* Source File:   PLANEPOLYEVENTS.ASPX.CS                                   */
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
using ElColombiano.Service.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace ElColombiano.Service
{
    /// <summary>
    /// Using this service page to gather all of the information
    /// about events defined in the Planepoly site and          
    /// manipulated by means of JSON format.
    /// 
    /// In fact this is the Planepoly to EL Colombiano JSON
    /// structure mapping.                                 
    /// </summary>
    public partial class PlanepolyEvents : System.Web.UI.Page
    {
        /// <summary>
        /// Gets the description for parameter 'type' given.
        /// </summary>
        /// <param name="type">integer value to map</param>
        /// <returns>Empty if no match, else description for given parameter.</returns>
        private string GetTypeName(int type)
        {
            string s = "";
            switch (type)
            {
                case 202:
                    s = "Obras de Teatro";
                    break;
                case 203:
                    s = "Concietos";
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
        private List<EventLookup> TransformEventServiceToEventLookup(List<EventService> eventPlanepolyServiceList)
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

        /// <summary>
        /// Generates the internal JSON representation to be used internally in EL COLOMBIANO web pages. The topic is the events category.
        /// </summary>
        /// <param name="sender">Sender object which fired the event</param>
        /// <param name="e">Parameters sent from the event manager.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            List<EventService> eventPlanepolyServiceList = new List<EventService>();
            Utils utils = new Utils();
            WebClient client = new WebClient();
            string[] urls = {"https://www.planepoly.com:8181/PlanepolyCoreWeb/OSConcierge?lat=6.210506&long=-75.57096&idTipos=202&consulta=eventos&bypass=626262" /* Refers to 'Obras de Teatro' at Planepoly.com */,
                            "https://www.planepoly.com:8181/PlanepolyCoreWeb/OSConcierge?lat=6.210506&long=-75.57096&idTipos=203&consulta=eventos&bypass=626262"  /* Refers to 'Conciertos' at Planepoly.com */,
                            "https://www.planepoly.com:8181/PlanepolyCoreWeb/OSConcierge?lat=6.210506&long=-75.57096&idTipos=209&consulta=eventos&bypass=626262"  /* Refers to 'Culturales' at Planepoly.com */ };
            string s = "";

            for (int i = 0; i < urls.Length; i++)
            {
                s = utils.ReadHtmlPageContent(urls[i]);
                Event o = JsonConvert.DeserializeObject<Event>(s);                
                foreach (var item in o.servicios)
                {
                    eventPlanepolyServiceList.Add(item);
                }
            }
            Response.AddHeader("Access-Control-Allow-Origin", "*");

            // Get data to be stored in file
            List<EventLookup> eventLookupList = TransformEventServiceToEventLookup(eventPlanepolyServiceList);
            eventLookupList = (from item in eventLookupList
                               orderby item.premiere descending, item.typeName, item.name
                               select item).Distinct().ToList();
            
            string eventLookupJSON =  JsonConvert.SerializeObject(eventLookupList);

            // Full movie (mapped from origin)
            string fileName = @"D:\SitiosWeb\Sitio\EC100A_Servicios\EC100A_PlanepolyWidget\planepoly-events.json";
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.Write(eventLookupJSON);
            }

            // This is the page result.            
            Response.Write(eventLookupJSON);           
        }
    }
}