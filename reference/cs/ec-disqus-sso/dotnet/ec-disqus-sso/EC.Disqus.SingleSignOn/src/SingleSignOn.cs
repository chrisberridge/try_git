/*----------------------------------------------------------------------------*/
/* Source File:   SINGLESIGNON.CS                                             */
/* Description:   Generates the payload we need to authenticate users remotely*/
/*                through Disqus (test)                                       */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                            */
/* Date:          Mar.31/2014                                                 */
/* Last Modified: Apr.21/2014                                                 */
/* Version:       1.2                                                         */
/* Copyright (c), 2014 El Colombiano, Aleriant                                */
/*----------------------------------------------------------------------------*/
/*-----------------------------------------------------------------------------
 History
 Mar.31/2014 COQ File created.
 -----------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

namespace EC.Disqus
{
    /// <summary>
    /// This class generates the payload we need to authenticate users remotely through Disqus
    /// This requires the Disqus SSO package and to have set up your application/remote domain properly
    /// See here for more: http://help.disqus.com/customer/portal/articles/236206-integrating-single-sign-on
    ///
    /// Usage:
    /// After inputting user data, a final payload will be generated which you use for the javascript variable 'remote_auth_s3'
    /// </summary>
    public class SingleSignOn
    {
        /// Disqus API secret key can be obtained here: http://disqus.com/api/applications/
        /// This will only work if that key is associated with your SSO remote domain

        private const string _apiSecret = "cxVu9uZLqObojz7P7ctQueTY48XYAXPkiDdHsMrVxt9Myszk2Os0mrc0PyxaFNoF";

        /// <summary>
        /// Convert a bytes array to a string representation in HEX notation.
        /// </summary>
        /// <param name="buff">Data to use</param>
        /// <returns>String with HEX notation.</returns>
        private string ByteToString(byte[] buff)
        {
            string sbinary = "";

            for (int i = 0; i < buff.Length; i++)
            {
                sbinary += buff[i].ToString("x2"); // hex format
            }
            return (sbinary);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializedUserData"></param>
        /// <returns></returns>
        private string CompilePayload(string serializedUserData)
        {
            byte[] userDataAsBytes = Encoding.ASCII.GetBytes(serializedUserData);

            // Base64 Encode the message
            string Message = System.Convert.ToBase64String(userDataAsBytes);

            // Get the proper timestamp
            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            string Timestamp = Convert.ToInt32(ts.TotalSeconds).ToString();

            // Convert the message + timestamp to bytes
            byte[] messageAndTimestampBytes = Encoding.ASCII.GetBytes(Message + " " + Timestamp);

            // Convert Disqus API key to HMAC-SHA1 signature
            byte[] apiBytes = Encoding.ASCII.GetBytes(_apiSecret);
            HMACSHA1 hmac = new HMACSHA1(apiBytes);
            byte[] hashedMessage = hmac.ComputeHash(messageAndTimestampBytes);

            // Put it all together into the final payload
            return Message + " " + ByteToString(hashedMessage) + " " + Timestamp;
        }

        /// <summary>
        /// Gets the Disqus SSO payload to authenticate users
        /// </summary>
        /// <param name="user_id">The unique ID to associate with the user</param>
        /// <param name="user_name">Non-unique name shown next to comments.</param>
        /// <param name="user_email">User's email address, defined by RFC 5322</param>
        /// <param name="avatar_url">URL of the avatar image</param>
        /// <param name="website_url">Website, blog or custom profile URL for the user, defined by RFC 3986</param>
        /// <returns>A string containing the signed payload</returns>
        public string GetPayload(string user_id, string user_name, string user_email, string avatar_url = "", string website_url = "")
        {
            var userdata = new
            {
                id = user_id,
                username = user_name,
                email = user_email
                //avatar = avatar_url,
                //url = website_url
            };

            string serializedUserData = new JavaScriptSerializer().Serialize(userdata);
            return CompilePayload(serializedUserData);
        }

        /// <summary>
        /// Conforms to Disqus authentication requirements by passing an encrypted
        /// Json user data, a signature and a timestamp, all encoded.
        /// </summary>
        /// <param name="pms"></param>
        /// <returns></returns>
        public string GeneratePayLoad(Dictionary<String, String> pms)
        {            
            string serializedUserData = new JavaScriptSerializer().Serialize(pms);
            return CompilePayload(serializedUserData);
        }

        /// <summary>
        /// Method to log out a user from SSO
        /// </summary>
        /// <returns>A signed, empty payload string</returns>
        public string LogoutUser()
        {
            Dictionary<String, String> d = new Dictionary<string,string>();
            return GeneratePayLoad(d);
        }
    }
}
