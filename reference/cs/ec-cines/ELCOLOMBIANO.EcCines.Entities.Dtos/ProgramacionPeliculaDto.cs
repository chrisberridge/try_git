
namespace ELCOLOMBIANO.EcCines.Entities.Dtos
{
    public class ProgramacionPeliculaDto
    {
        public int idFormato { get; set; }
        public int idPelicula { get; set; }
        public int idHorarioPelicula { get; set; }
        public int annoHorarioPelicula { get; set; }
        public int mesHorarioPelicula { get; set; }
        public int diaHorarioPelicula { get; set; }
        public string nombreDiaSemanaHorarioPelicula { get; set; }
        public int idTeatro { get; set; }
        public string horaMinutoPelicula { get; set; }
        public int sala { get; set; }
        public int frecuencia { get; set; }
    }
}
