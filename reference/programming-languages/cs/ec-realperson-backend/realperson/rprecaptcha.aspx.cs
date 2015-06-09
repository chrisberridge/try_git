/*==========================================================================*/
/* Source File:   RPRECAPTCHA.ASPX.CS                                       */
/* Description:   Web form to validate through Jquery POST Ajax if user is  */
/*                a real person using Google Recaptcha.                     */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Mar.11/2014                                               */
/* Last Modified: Mar.17/2014                                               */
/* Version:       1.2                                                       */
/* Copyright (c), 2014 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Mar.11/2014 COQ File created.
============================================================================*/

using EC.Utils.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace EC.Web
{
    /// <summary>
    /// Web form to validate through Jquery POST Ajax if user is  a real person using Google Recaptcha (http://www.google.com/recaptcha)
    /// </summary>
    public partial class RPRecaptcha : System.Web.UI.Page
    {
        protected string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;

            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }

        protected bool DoValidateGoogleRecaptcha(string challenge, string response)
        {
            if (challenge == "" || response == "")
            {
                return false;
            }

            bool rslt = false;
            string remoteIP = GetIPAddress();
            NameValueCollection nv = new NameValueCollection() {
                { "privatekey", "6LcTFvASAAAAAIIGq0E0ZLnH81oAjt22Z8EWhTZ4"},
                { "remoteip", remoteIP },
                { "challenge", challenge },
                { "response", response }
            };

            string URI = "http://www.google.com/recaptcha/api/verify";
            string s = "";

            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                byte[] responsebytes = wc.UploadValues(URI, "POST", nv);
                string responsebody = Encoding.UTF8.GetString(responsebytes);
                s = responsebody;
            }
            
            if (s != "")
            {
                string[] lines = Regex.Split(s, "\n");
                if (lines[0] == "true")
                {
                    rslt = true;
                }                
            }
            return rslt;
        }

        /// <summary>
        /// Two values are of interest to this page. If same hash is obtained, then a real person
        /// submitted the form.
        /// </summary>
        /// <param name="sender">Sender object which fired the event</param>
        /// <param name="e">Parameters sent from the event manager.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            string recaptchaChallengeField = "";
            string recaptchaResponseField = "";
            RealPersonResponse rpr = new RealPersonResponse();
            if (Request.Form["recaptcha_challenge_field"] == null)
            {
                if (Request.QueryString["recaptcha_challenge_field"] == null)
                {
                    recaptchaChallengeField = "";
                } else {
                    recaptchaChallengeField  = Request.QueryString["recaptcha_challenge_field"];
                }
            } else {
                recaptchaChallengeField = Request.Form["recaptcha_challenge_field"];
            }
            if (Request.Form["recaptcha_response_field"] == null)
            {
                if (Request.QueryString["recaptcha_response_field"] == null)
                {
                    recaptchaResponseField = "";
                } else {
                    recaptchaResponseField  = Request.QueryString["recaptcha_response_field"];
                }
            } else {
                recaptchaResponseField = Request.Form["recaptcha_response_field"];
            }
            
            Boolean b = DoValidateGoogleRecaptcha(recaptchaChallengeField, recaptchaResponseField);
            if (b)
            {
                rpr.msg = "Valid";
                rpr.val = 1;
            }
            else
            {
                rpr.msg = "Not Valid";
                rpr.val = 0;
            }
            string s = JsonConvert.SerializeObject(rpr);
            Response.Write(s);
            Response.AddHeader("Access-Control-Allow-Origin", "*");
        }        
    }

    public static class Http
    {
        public static byte[] Post(string uri, NameValueCollection pairs)
        {
            byte[] response = null;
            using (WebClient client = new WebClient())
            {
                response = client.UploadValues(uri, pairs);
            }
            return response;
        }
    }
}