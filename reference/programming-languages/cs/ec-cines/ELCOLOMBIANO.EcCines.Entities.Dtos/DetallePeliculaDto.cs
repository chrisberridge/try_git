using System;

namespace ELCOLOMBIANO.EcCines.Entities.Dtos {
    public class DetallePeliculaDto {
        public int idDetallePelicula { get; set; }
        public int idPelicula { get; set; }
        public string nombrePelicula { get; set; }
        public int idUsuarioCreador { get; set; }
        public DateTime fechaCreacionPelicula { get; set; }
        public int idGeneroPelicula { get; set; }
        public string sinopsis { get; set; }
        public string imagenCartelera { get; set; }
        public string urlArticuloEc { get; set; }
        public string enCartelera { get; set; }

        public DetallePeliculaDto() {
            idDetallePelicula = 0;
            idPelicula = 0;
            nombrePelicula = "";
            idUsuarioCreador = 0;
            fechaCreacionPelicula = DateTime.Now;
            idGeneroPelicula = 0;
            sinopsis = "";
            imagenCartelera = "";


        }
    }
}