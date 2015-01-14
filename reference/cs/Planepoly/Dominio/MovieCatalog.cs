using System;
using System.Collections.Generic;

namespace ElColombiano.Planepoly
{
    public class MovieCatalog
    {
        public List<String> theaters { get; set; }
        public List<String> genres { get; set; }
        public List<String> movies { get; set; }
        public Dictionary<string, List<string>> theaterMovies { get; set; }
    }
}