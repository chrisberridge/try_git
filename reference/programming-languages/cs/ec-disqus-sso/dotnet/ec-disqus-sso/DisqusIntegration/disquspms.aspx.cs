/*----------------------------------------------------------------------------*/
/* Source File:   DISQUSPMS.ASPX.CS                                           */
/* Description:   Generates the payload we need to authenticate users remotely*/
/*                through Disqus (test)                                       */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                            */
/* Date:          Apr.02/2014                                                 */
/* Last Modified: Apr.02/2014                                                 */
/* Version:       1.1                                                         */
/* Copyright (c), 2014 El Colombiano, Aleriant                                */
/*----------------------------------------------------------------------------*/
/*-----------------------------------------------------------------------------
 History
 Apr.02/2014 COQ File created.
 -----------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using EC.Disqus;

namespace Ec.Disqus.Web.Integration
{
    public partial class disquspms : System.Web.UI.Page
    {
        /// <summary>
        /// Using request object, it is determined if a parameter is stored given 'paramName' name to search.
        /// </summary>
        /// <param name="paramName">Name of element to search </param>
        /// <returns>Empty string if parameter value not found.</returns>
        protected String RetrieveHTMLParam(string paramName)
        {
            string rslt = "";
            if (Request.Form[paramName] == null)
            {
                if (Request.QueryString[paramName] == null)
                {
                    rslt = "";
                }
                else
                {
                    rslt = Request.QueryString[paramName];
                }
            }
            else
            {
                rslt= Request.Form[paramName];
            }
            return rslt;
        }

        /// <summary>
        /// Loads page data.
        /// </summary>
        /// <param name="sender">Object responsible of event</param>
        /// <param name="e">Event arguments.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            string id, username, email, avatar;

            id = RetrieveHTMLParam("userId");
            username = RetrieveHTMLParam("username");
            email = RetrieveHTMLParam("email");
            avatar = RetrieveHTMLParam("avatar");

            Dictionary<String, String> userData = new Dictionary<string, string>();
            userData.Add("id", id);
            userData.Add("username", username);
            userData.Add("email", email);
            userData.Add("avatar", avatar);

            SingleSignOn sso = new SingleSignOn();
            string s = sso.GeneratePayLoad(userData);

            lblData.InnerText = s;            
        }
    }
}