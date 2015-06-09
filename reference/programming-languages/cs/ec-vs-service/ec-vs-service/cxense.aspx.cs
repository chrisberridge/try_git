/*==========================================================================*/
/* Source File:   CXENSE.ASPX.CS                                            */
/* Description:   Using this service page where the EL COLOMBIANO search    */
/*                functinality is nurtured.                                 */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          May.14/2013                                               */
/* Last Modified: Oct.02/2013                                               */
/* Version:       1.3                                                       */
/* Copyright (c), 2013 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
May.14/2013 COQ File created.
============================================================================*/
using System;
using System.IO;
using System.Net;
using System.Web;

namespace ElColombiano.Service
{
    /// <summary>
    /// Using this service page where the EL COLOMBIANO search
    /// functinality is nurtured.
    /// </summary>
    public partial class CXEnse : System.Web.UI.Page
    {
        /// <summary>
        /// In fact this is the core search for EL COLOMBIANO search. This search uses the cXense database, hence this ASPX name.
        /// It uses three parameters to guide the search result.
        /// Parameter 'r' indicates the term or phrase to be searched.
        /// Parameter 'c' controls the number of records per page.
        /// Parameter 'p' indicates the page to be retrieved from cXense database (pages start from 0 for first page, 1 for second page
        /// and so on).
        /// Parameter 's' indicates a sort method, it receives the following values: 1=By relevance, 2=By Creation Date Ascending,
        /// 3=By Creation Date Descending, if not received then it is not used.
        /// </summary>
        /// <param name="sender">Sender object which fired the event</param>
        /// <param name="e">Parameters sent from the event manager.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            WebClient client = new WebClient();
            string r = Request.QueryString["r"];
            string c = Request.QueryString["c"];
            string p = Request.QueryString["p"];
            string s = Request.QueryString["s"];
            int startIn = 0;
            int recsPerPage = 0;
            int pageNum = 10;
            int sortMode = 0;

            if (s != null)
            {
                sortMode = Convert.ToInt32(s);
            }
            if (r == null)
            {
                r = "*";
            }
            if (c != null)
            {
                recsPerPage = Convert.ToInt32(c);
            }
            if (p != null)
            {
                pageNum = Convert.ToInt32(p);
            }
            if (pageNum != 0)
            {
                startIn = recsPerPage * (pageNum - 1);
            }
            r = r.Replace("\"", "\\\"");
            r = HttpUtility.UrlEncode(r);
            string urlPart = "http://sitesearch.cxense.com/api/search/9222263900732340974?";
            string url = urlPart;

            switch (sortMode)
            {
                case 1: // By Relevance
                    url += "p_aq=query(heading^5,body^2:\"" + r + "\", token-op=and)";
                    url += "&p_rcs=rank=_score;t=time()-doc['recs-publishtime'].value;if(t<86400000) {rank=rank*3.00;}else if(t<604800000){rank=rank*2.5;}else if(t<2419200000){rank=rank*1.5;}else {rank=rank*0.5;};rank;";
                    break;
                case 2: // By Creation Date Ascending
                    url += "p_aq=query(heading^5,body^2:\"" + r + "\", token-op=and)";
                    url += "&p_rcs=rank=_score;t=time()-doc['recs-publishtime'].value;if(t<86400000) {rank=rank*3.00;}else if(t<604800000){rank=rank*2.5;}else if(t<2419200000){rank=rank*1.5;}else {rank=rank*0.5;};rank;";
                    url += "&p_sm=[{recs-publishtime:asc},{_score:asc}]";
                    break;
                case 3: // By Creation Date Descending
                    url += "p_aq=query(heading^5,body^2:\"" + r + "\", token-op=and)";
                    url += "&p_rcs=rank=_score;t=time()-doc['recs-publishtime'].value;if(t<86400000) {rank=rank*3.00;}else if(t<604800000){rank=rank*2.5;}else if(t<2419200000){rank=rank*1.5;}else {rank=rank*0.5;};rank;";
                    url += "&p_sm=[{recs-publishtime:desc},{_score:desc}]";                  
                    break;
            }
            url += "&p_c=[recsPerPage]";
            url += "&p_s=[startIn]";
            url += "&media=json";
            url = url.Replace("[recsPerPage]", Convert.ToString(recsPerPage));
            url = url.Replace("[startIn]", Convert.ToString(startIn));

            Stream data = null;
            StreamReader reader = null;
            try
            {
                data = client.OpenRead(url);
                reader = new StreamReader(data);
                string urlReadInfo = reader.ReadToEnd();
                Response.Write(urlReadInfo);
            }
            catch (Exception)
            {
                Response.Write("");
            }
            finally
            {
                if (data != null)
                {
                    data.Close();    
                }
                if (reader != null)
                {
                    reader.Close();
                }
            }
            Response.AddHeader("Access-Control-Allow-Origin", "*");
        }
    }
}