/*----------------------------------------------------------------------------*/
/* Source File:   DQRESPONSE.JAVA                                             */
/* Description:   Disqus Ajax Response                                        */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                            */
/* Date:          Apr.08/2014                                                 */
/* Last Modified: Apr.08/2014                                                 */
/* Version:       1.2                                                         */
/* Copyright (c), 2014 El Colombiano, Aleriant                                */
/*----------------------------------------------------------------------------*/
/*-----------------------------------------------------------------------------
 History
 Apr.08/2014 COQ File created.
 -----------------------------------------------------------------------------*/
namespace EC.Disqus.Domain
{
    /// <summary>
    /// Domain object for Ajax response.
    /// </summary>
    public class DQResponse
    {
        public string msg { get; set; }
        public int val { get; set; }
    }
}
