using System;

namespace ELCOLOMBIANO.EcCines.Entities.Dtos {
    public class CineDto {
        public int idCine { get; set; }
        public String nit { get; set; }
        public DateTime fechaCreacionCine { get; set; }
        public String nombreCine { get; set; }
    }
}