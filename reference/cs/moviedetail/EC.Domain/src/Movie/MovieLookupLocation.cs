using System.Collections.Generic;

namespace EC.Domain.Movie
{
    public class MovieLookupLocation
    {
        public int id { get; set; }
        public string name { get; set; }
        public string branchName { get; set; }
        public string nameFull { get; set; }
        public string address { get; set; }
        public List<MovieLookupShow> schedule { get; set; }
    }
}