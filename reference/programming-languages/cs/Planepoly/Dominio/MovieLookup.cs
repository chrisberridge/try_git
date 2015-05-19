using System.Collections.Generic;

namespace ElColombiano.Planepoly
{
    public class MovieLookup
    {
        public string name { get; set; }
        public string img { get; set; }
        public string url { get; set; }
        public int premiere { get; set; }
        public string genre { get; set; }
        public List<MovieLookupLocation> locations { get; set; }
    }
}