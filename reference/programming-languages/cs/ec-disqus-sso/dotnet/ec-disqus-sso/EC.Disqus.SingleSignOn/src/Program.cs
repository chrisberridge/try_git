/*----------------------------------------------------------------------------*/
/* Source File:   PROGRAM.JAVA                                                */
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

namespace EC.Disqus
{
    public class Program
    {
        public static void Main()
        {
            Dictionary<String, String> userData = new Dictionary<string, string>();
            userData.Add("id", "uniqueId_123456789");
            userData.Add("username", "Charlie Chaplin");
            userData.Add("email", "charlie.chaplin@example.com");
            userData.Add("avatar", "http://pathtogif.com/gif?abc");

            SingleSignOn sso = new SingleSignOn();
            string s = sso.GeneratePayLoad(userData);
            Console.WriteLine(s);
            s = sso.LogoutUser();
            Console.WriteLine(s);
        }
    }
}
