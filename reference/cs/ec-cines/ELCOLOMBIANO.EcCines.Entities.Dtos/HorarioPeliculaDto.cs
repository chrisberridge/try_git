
namespace ELCOLOMBIANO.EcCines.Entities.Dtos
{
    public class HorarioPeliculaDto
    {
        public int idHorarioPelicula { get; set; }
        public int idFormato { get; set; }
        public int idPelicula { get; set; }
        public int idTeatro { get; set; }
        public int annoHorarioPelicula { get; set; }
        public int mesHorarioPelicula { get; set; }
        public int diaHorarioPelicula { get; set; }
        public string nombreDiaSemanaPelicula { get; set; }
        public int frecuencia { get; set; }
    }
}