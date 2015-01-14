using System.Web;
using System.IO;

namespace ElColombiano.Planepoly
{
    public abstract class ReadJson
    {
        /// <summary>
        /// The purpose of this page is to service JSON Files. The files are already generated 
        /// elsewhere and are loaded from file system.
        /// 
        /// A query string parameter is sent to this page. Given name of 'm' and has values of '1:Get movies' file,
        /// '2:Get Movies Catalog' and so on.
        /// </summary>
        public static void Read(HttpRequest Request, HttpResponse Response)
        {
            string fileNameMovies = @"D:\SitiosWeb\Sitio\EC100A_Servicios\EC100A_PlanepolyWidget\planepoly-movies.json";
            string fileNameMoviesCatalog = @"D:\SitiosWeb\Sitio\EC100A_Servicios\EC100A_PlanepolyWidget\planepoly-movies-catalog.json";
            string fileName = "";
            string m = Request.QueryString["m"];

            if (m == null || (m != "1" && m != "2"))
            {
                Response.Write("Invalid");
                return;
            }
            switch (m)
            {
                case "1":
                    fileName = fileNameMovies;
                    break;
                case "2":
                    fileName = fileNameMoviesCatalog;
                    break;
                default:
                    break;
            }
            string s;
            using (StreamReader reader = new StreamReader(fileName))
            {
                s = reader.ReadToEnd();
            }
            Response.Write(s);
            Response.AddHeader("Access-Control-Allow-Origin", "*");
        }
    }
}
