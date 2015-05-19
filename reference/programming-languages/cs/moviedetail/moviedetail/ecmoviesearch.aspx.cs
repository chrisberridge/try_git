using EC.Business;
using EC.Utils.Constants;
using System;
using System.Configuration;
namespace moviedetail
{
    public partial class ecmoviesearch : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ManageMovieCatalog mmc = new ManageMovieCatalog();
            mmc.catalogNameFileName = @ConfigurationManager.AppSettings[GlobalConstants.FileMoviesCatalogKey];
            mmc.moviesFileName = @ConfigurationManager.AppSettings[GlobalConstants.FileMoviesKey];
            mmc.dbConnection = @ConfigurationManager.AppSettings[GlobalConstants.ConnectionKey];

            string t = Request.QueryString["t"];
            string m = Request.QueryString["m"];
            string g = Request.QueryString["g"];

            if (t == null)
            {
                t = "-1";
            }
            if (m == null)
            {
                m = "-1";
            }
            if (g == null)
            {
                g = "-1";
            }
            Response.Write(mmc.Search(t, m, g));
            Response.AddHeader("Access-Control-Allow-Origin", "*");
        }
    }
}