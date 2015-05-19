using System.Collections.Generic;

namespace ElColombiano.Planepoly
{
    public class MovieLocation
    {
        public string nombre { get; set; }
        public int id { get; set; }
        public string direccion { get; set; }
        public List<MovieShow> funcs { get; set; }
        public double calif { get; set; }
        public int califres { get; set; }
        public double @long { get; set; }
        public double lat { get; set; }

        
    }    
}