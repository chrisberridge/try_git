/*==========================================================================*/
/* Source File:   JSONPLAY.ASPX.CS                                          */
/* Description:   A test web page that serves as proof of concept when      */
/*                dealing with JSON APIs.                                   */
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
using System.IO;
using System.Linq;
using System.Net;

namespace ElColombiano.Service
{
    /// <summary>
    ///  A test web page that serves as proof of concept when 
    ///  dealing with JSON APIs.
    ///  </summary>
    public partial class jsonplay : System.Web.UI.Page
    {
        /// <summary>
        /// Using list manipulation by LINQ Library and JSON.NET library, we are able to process
        /// fluidly any JSON structure.
        /// Reference
        /// ---------
        /// json2csharp: http://json2csharp.com/
        /// json parser online: http://json.parser.online.fr/
        /// json Editor online: http://jsoneditoronline.org/
        /// json: http://json.org/
        /// json http://jsonlint.com/
        /// 
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
                        
            var ev2 = JsonConvert.DeserializeObject<Movie>(s);
            var l = ev2.servicios.OrderBy(x => x.nombre).OrderByDescending(x => x.estr).ToList();
            ev2.servicios = l;
            Response.Write(JsonConvert.SerializeObject(ev2));
            Response.Write(ev2.ToString());

            //string oneService = "{\"servicios\":[{\"inicio\":\"17/06/2013\",\"salaForo\":\"\",\"ptos\":[{\"nombre\":\"Teatro Metropolitano Jos\u00e9 Gutierrez G\u00f3mez\",\"id\":524,\"direccion\":\"Calle\u00a041  Nro. 57-30\",\"funcs\":[{\"hora\":\"20:00\",\"dia\":1}],\"calif\":4,\"califres\":3,\"long\":-75.576737523079,\"lat\":6.241350978501}],\"img\":\"http://planepoly.com/PlanepolyImgWeb/Get?id=311&res=HR&tipo=600\",\"fin\":\"17/06/2013\",\"estr\":0,\"urlVideo\":\"\",\"urlWww\":\"\",\"url\":\"http://www.planepoly.com/Accion?Servicio=311\",\"id\":311,\"nombre\":\"Middlesex\",\"genero\":\"Sin Clasificar\",\"sinopsis\":\"Hora: 8:00 p.m.\nLugar: Teatro Metropolitano\nDescuento con Intelecto: 20 por ciento para 150 boletas disponibles. El descuento no incluye en Ticket Service\nInversi\u00f3n: 60.000 - 40.000 pesos.\nVenta de boleter\u00eda: Taquilla del Teatro Metropolitano y Tu Boleta.\nInformes: 232 28 58 - 232 45 97\",\"tipo\":209,\"calif\":0,\"califres\":0}, {\"inicio\":\"07/06/2013\",\"salaForo\":\"\",\"ptos\":[{\"nombre\":\"Teatro Universidad de Medell\u00edn\",\"id\":530,\"direccion\":\"Carrera 87 Nro. 30 - 65\",\"funcs\":[{\"hora\":\"20:00\",\"dia\":5}],\"calif\":0,\"califres\":0,\"long\":-75.610582,\"lat\":6.232728}],\"img\":\"http://planepoly.com/PlanepolyImgWeb/Get?id=254&res=HR&tipo=600\",\"fin\":\"07/06/2013\",\"estr\":0,\"urlVideo\":\"\",\"urlWww\":\"\",\"url\":\"http://www.planepoly.com/Accion?Servicio=254\",\"id\":254,\"nombre\":\"Tap Factory\",\"genero\":\"Danza\",\"sinopsis\":\"Hora: 8:00pm\nLugar: Teatro Universidad de Medell\u00edn\nInversi\u00f3n: $170,000, $150.000, $130,000 y $82,000.\nVenta de boleter\u00eda: taquilla Del Teatro Universidad de Medell\u00edn. \nInformes: 340 52 02.\",\"tipo\":209,\"calif\":0,\"califres\":0}, {\"inicio\":\"06/06/2013\",\"salaForo\":\"\",\"ptos\":[{\"nombre\":\"El Teatrico\",\"id\":1044,\"direccion\":\"Avenida Nutibara Transversal 39B #C2 -46\",\"funcs\":[{\"hora\":\"20:00\",\"dia\":4}, {\"hora\":\"20:00\",\"dia\":5}, {\"hora\":\"20:00\",\"dia\":6}],\"calif\":4.67,\"califres\":3,\"long\":-75.59078,\"lat\":6.2432437}],\"img\":\"http://planepoly.com/PlanepolyImgWeb/Get?id=305&res=HR&tipo=600\",\"fin\":\"29/06/2013\",\"estr\":0,\"urlVideo\":\"\",\"urlWww\":\"\",\"url\":\"http://www.planepoly.com/Accion?Servicio=305\",\"id\":305,\"nombre\":\"Humor Inmarcesible\",\"genero\":\"Teatro\",\"sinopsis\":\"\u00a1Un j\u00fabilo inmortal! Humor colombiano, m\u00e1s colombiano que tirar piscina en calzoncillos. Reservas: 411 8878.  \",\"tipo\":209,\"calif\":0,\"califres\":0}]}";
            url = "https://www.planepoly.com:8181/PlanepolyCoreWeb/OSConcierge?lat=6.210506&long=-75.57096&idTipos=202&consulta=eventos&bypass=626262";
            data = client.OpenRead(url);
            reader = new StreamReader(data);
            s = reader.ReadToEnd();
            var ev3 = JsonConvert.DeserializeObject<Event>(s);
            Response.Write("<br><br><hr><br>");
            Response.Write(JsonConvert.SerializeObject(ev3));
            Response.Write(ev3.ToString());
        }
    }
}

