using System;
using System.Collections.Generic;

namespace EC.Domain.Movie
{
    public class MovieCatalog
    {
        public List<MovieTheaterInfo> theaters { get; set; }
        public List<MovieFormatInfo> formats { get; set; }
        public List<MovieGenreInfo> genres { get; set; }
        public List<MovieShortInfo> movies { get; set; }
        public Dictionary<string, List<MovieShortInfo>> theaterMovies { get; set; }
    }
}