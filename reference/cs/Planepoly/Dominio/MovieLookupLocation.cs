using System.Collections.Generic;

namespace ElColombiano.Planepoly
{
    public class MovieLookupLocation
    {
        public string name { get; set; }
        public string address { get; set; }
        public List<MovieLookupShow> schedule { get; set; }
    }
}