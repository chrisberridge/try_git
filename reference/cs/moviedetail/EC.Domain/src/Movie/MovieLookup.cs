using System.Collections.Generic;

namespace EC.Domain.Movie
{
    public class MovieLookup
    {
        public int id { get; set; }        
        public string name { get; set; }
        public int idFormat { get; set; }
        public string format { get; set; }
        public string nameFull { get; set; }
        public string img { get; set; }
        public string url { get; set; }
        public string active { get; set; }
        public int idGenre { get; set; }
        public string genre { get; set; }
        public List<MovieLookupLocation> locations { get; set; }
    }
}