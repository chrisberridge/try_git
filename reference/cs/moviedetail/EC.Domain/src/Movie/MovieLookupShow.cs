using System.Collections.Generic;

namespace EC.Domain.Movie
{
    public class MovieLookupShow
    {
        public int id { get; set; }
        public int frequency { get; set; }
        public string name { get; set; }
        public int year { get; set; }
        public int month { get; set; }
        public int day { get; set; }
        public List<MovieLookupShowHours> hours { get; set; }
    }
}