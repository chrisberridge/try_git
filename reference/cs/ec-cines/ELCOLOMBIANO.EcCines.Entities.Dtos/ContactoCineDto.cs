using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ELCOLOMBIANO.EcCines.Entities.Dtos
{
    public class ContactoCineDto
    {
        public int idContacto { get ; set ; }
        public int idCine { get ; set ; }
        public string cedulaContacto { get; set; }
        public string nombreContacto { get; set; }
        public string cargoContacto { get; set; }
        public string telefono1Contacto { get; set; }
        public string telefono2Contacto { get; set; }
    }
}