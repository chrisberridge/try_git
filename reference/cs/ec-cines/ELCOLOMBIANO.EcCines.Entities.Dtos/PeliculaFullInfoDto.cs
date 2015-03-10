
namespace ELCOLOMBIANO.EcCines.Entities.Dtos
{
    public class PeliculaFullInfoDto
    {
        public int idFormato { get; set; }
        public string nombreFormato { get; set; }
        public int idPelicula { get; set; }
        public int idHorarioPelicula { get; set; }
        public string annoHorarioPelicula { get; set; }
        public string mesHorarioPelicula { get; set; }
        public string diaHorarioPelicula { get; set; }
        public string nombreDiaSemanaHorarioPelicula { get; set; }
        public int idTeatro { get; set; }
        public string nombreTeatro { get; set; }
        public int frecuencia { get; set; }
        public string horaPelicula { get; set; }
        public string minutoPelicula { get; set; }
        public int sala { get; set; }
    }
}
