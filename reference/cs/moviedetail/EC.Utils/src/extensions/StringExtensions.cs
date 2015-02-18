/*==========================================================================*/
/* Source File:   STRINGEXTENSIONS.CS                                       */
/* Description:   A collection of extensions for String Class.              */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Aug.13/2013                                               */
/* Last Modified: Sep.23/2014                                               */
/* Version:       1.12                                                      */
/* Copyright (c), 2013, 2014 Arkix, El Colombiano                           */
/*==========================================================================*/

/*===========================================================================
History
Aug.13/2013 COQ File created.
============================================================================*/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EC.Utils.Extensions {
    /// <summary>
    /// A collection of extensions for String Class.
    /// </summary>
    public static class StringExtensions {
        /// <summary>
        /// Computes the number of words present in a string.
        /// </summary>
        /// <param name="str">The string object to extend</param>
        /// <returns></returns>
        public static int WordCount(this String str) {
            return str.Split(new char[] { ' ', '.', '?' },
                             StringSplitOptions.RemoveEmptyEntries).Length;
        }

        /// <summary>
        /// Given a string replaces all spaces within them with '-' and lowers the result.
        /// Ex: Given 'The Big Brown Fox jumps the fence' is converted 'the-big-brown-fox-jumps-the-fence';
        /// and it also appends the 'id' value. The net result for the latter example is 'the-big-brown-fox-jumps-the-fence-125';
        /// with id=125.
        /// </summary>
        /// <param name="str">The string object to extend</param>
        /// <param name="id">Appends this number to result</param>
        /// <returns>A modified version of original string.</returns>
        public static String HyphenLower(this String str, long id) {
            string s = str.Trim().ToLower().Replace(' ', '-') + "-" + id;
            return s;
        }

        /// <summary>
        /// Given a string replaces all spaces within them with '-' and lowers the result.
        /// Ex: Given 'The Big Brown Fox jumps the fence' is converted 'the-big-brown-fox-jumps-the-fence'.
        /// </summary>
        /// <param name="str">The string object to extend</param>        
        /// <returns>A modified version of original string.</returns>
        public static String HyphenLower(this String str) {
            string s = str.Trim().ToLower().Replace(' ', '-');
            return s;
        }

        /// <summary>
        /// Given a string, it removes all occurrences of a character. If this 
        /// character appears at the beginning of string then the whole string
        /// is trimmed.
        /// </summary>
        /// <param name="str">The string object to extend</param>
        /// <param name="ch">The character to match for removal</param>
        /// <returns>A modified version of string with character removed if present</returns>
        public static String Supress(this String str, char ch) {
            string s = str;
            s = s.Replace(ch, ' ').Trim();
            return s;
        }

        /// <summary>
        /// Given a string, it removes all occurrences of a substring. If this 
        /// string appears at the beginning of string then the whole string
        /// is trimmed.
        /// </summary>
        /// <param name="str">The string object to extend</param>
        /// <param name="substr">The character to match for removal</param>
        /// <returns>A modified version of string with substring removed if present</returns>
        public static String Supress(this String str, String substr) {
            string s = str;
            s = s.Replace(substr, " ").Trim();
            return s;
        }

        /// <summary>
        /// Rmoves the 'refBase' (case insensitive) parameter from 'str'.
        /// the end result is trimmed and case is preserved.
        /// </summary>
        /// <param name="str">The string object to extend</param>
        /// <param name="refBase"></param>
        /// <returns>A new version of source string</returns>
        public static String SupressInsensitive(this String str, string refBase) {
            string s = Regex.Replace(str, refBase, "", RegexOptions.IgnoreCase);
            return s;
        }

        /// <summary>
        /// If we have for example llano.jpg, then puts 'inc' inside this name, yielding 'llano_g.jpg' with 'inc' set to '_g'.
        /// </summary>
        /// <param name="str">The string object to extend</param>
        /// <param name="inc">The text to include inside.</param>
        /// <param name="imgExtensions">A list of valid image extensions</param>
        /// <returns>The source string with included string inside it.</returns>
        public static String IncludeInsideToImageName(this String str, String inc, String[] imgExtensions) {
            string s = str;
            for (int i = 0; i < imgExtensions.Length; i++) {
                var suffix = inc + imgExtensions[i];
                s = s.Replace(imgExtensions[i], suffix);
            }
            return s;
        }

        /// <summary>
        /// Cut string by the 'len' characters.
        /// </summary>
        /// <param name="str">The string object to extend</param>
        /// <param name="len">How many characters to limit</param>
        /// <returns>A new version of source string</returns>
        public static String LimitTo(this String str, int len) {
            string s = str;
            if (s.Length > len) {
                s = s.Substring(0, len);
            }
            return s;
        }

        /// <summary>
        /// If string contains an image name like 'carlos-mario-oquendo-mariana-pajon-miguel-calixto-640x280-07072014-Cortesia.jpg'
        /// then some characters are remove before '.' and concatenates to get a 'len' parameter string length.
        /// </summary>
        /// <param name="str">Data to act upon</param>
        /// <param name="len">Desired final length of string</param>
        /// <param name="ext">Image extension to preserve. E.g., '.jpg'</param>
        /// <returns>If data is empty then the same is returned, otherwise the up to 'len' value string length is returned.</returns>
        public static String LimitImageNameTo(this String str, int len, String ext) {
            string extUpper = ext.ToUpper();
            string rslt = "";
            if (str == "") {
                return str;
            }
            else {
                String sTmp = str;

                if (sTmp.EndsWith(extUpper)) {
                    sTmp = sTmp.ToLower();
                }
                if (sTmp.EndsWith(ext)) {
                    sTmp = sTmp.Substring(0, sTmp.IndexOf(ext));
                    while (sTmp.Length + ext.Length >= len) {
                        sTmp = sTmp.Substring(0, sTmp.Length - 1);
                    }
                    rslt = sTmp + ext;
                }
                else {
                    return str;
                }
            }
            return rslt;
        }

        /// <summary>
        /// Given the domain name contained in urlToUse it is. 
        /// Besides it must contain 'BancoConocimiento' named in the URL.
        /// </summary>
        /// <param name="urlToUse">The contents to use</param>
        /// <param name="knownDomainList">Valid domain names to use</param>        
        /// <returns>True if URL host domain is containd in 'knowDomainList'</returns>
        public static bool ValidateUrlDomain(this string urlToUse, List<String> knownDomainList) {
            Uri checkedUrl = new Uri(urlToUse);
            bool found = false;
            string checkedUrlHost = checkedUrl.Host;
            checkedUrlHost = checkedUrlHost.Replace("http://", "");

            for (int i = 0; i < knownDomainList.Count; i++) {
                var s = knownDomainList[i];
                if (s == checkedUrlHost) {
                    found = true;
                    break;
                }
            }
            if (found) {
                var s = "BANCOCONOCIMIENTO";
                if (urlToUse.ToUpper().Contains(s)) {
                    return true;
                }
                else {
                    return false;
                }
            }
            return found;
        }

        /// <summary>
        /// Changes the replace domain item in urlToUse if and only if it is not empty.
        /// </summary>
        /// <param name="urlToUse">URL name to use</param>
        /// <param name="knownDomainList">Valid domains to use</param>
        /// <param name="replaceKnownDomainList">Valid replace domain to use. If one is to be picked, it is the same index found in 'knownDomainList'</param>       
        /// <returns>Changed domain name in url if replace item is not empty</returns>
        public static String ReconfigureHostNameFrom(this string urlToUse, List<string> knownDomainList, List<String> replaceKnownDomainList) {
            int i = 0;
            string rslt = "";
            Uri checkedUrl = new Uri(urlToUse);
            bool found = false;
            string checkedUrlHost = checkedUrl.Host;
            checkedUrlHost = checkedUrlHost.Replace("http://", "");

            for (i = 0; i < knownDomainList.Count; i++) {
                var s = knownDomainList[i];
                if (s == checkedUrlHost) {
                    found = true;
                    break;
                }
            }

            if (found) {
                if (replaceKnownDomainList[i] != "") {
                    rslt = urlToUse.Replace(knownDomainList[i], replaceKnownDomainList[i]);
                }
                else {
                    rslt = urlToUse;
                }
            }
            else {
                rslt = urlToUse;
            }
            return rslt;
        }

        /// <summary>
        /// Given string is used to extract a substring delimited by the 'chStart', 'chEnd' parameters.
        /// </summary>
        /// <param name="s">Reference to string to use</param>
        /// <param name="chStart">First character to find to start extracting characters, not itself included.</param>
        /// <param name="chEnd">Last character to find to end extracting characters, not itself included.</param>
        public static String ExtractCharactersUsingDelimiters(this string s, char chStart, char chEnd) {
            string rslt = "";
            int cnt = s.Length;

            // Find in string the first 'chStart' delimiter
            int i = 0;
            while (i < cnt) {
                if (s[i++] == chStart) {
                    break;
                }
            }
            if (i < cnt) {
                // While trying to find the 'chEnd' delimiter
                // lets retrive characters along the way
                while (i < cnt) {
                    if (s[i] != chEnd) {
                        rslt += s[i];
                    }
                    else {
                        break;
                    }
                    i++;
                }
            }
            return rslt;
        }

        /// <summary>
        /// Suppose you have the HTML code that includes some TAG you no longer want.
        /// Removes from referenced string the HTML tag supplied. An example is as follows:
        /// Suppose you have the HTML as &lt;b&gt;&lt;img &gt;&lt;/img&gt;&lt;/b&gt; and you want to remove image tag
        /// the call it as follows 's.RemoveHTMLTag("&lt;img")', but of course, there are two ways of closing an HTML Tag, 
        /// one that puts the '/' in the same TAG, the second that takes  the &gt;/TAGNAME&gt; form.
        /// Another way is not have a closing at all.
        /// </summary>
        /// <param name="s">Reference to string to work on</param>
        /// <param name="htmlStartTag">Text of tag to remove</param>
        /// <returns>Referenced string without the HTML Tag removed if found.</returns>
        public static String RemoveHTMLTag(this String s, String htmlStartTag) {
            // NOT TESTED
            string rslt = "";
            int inxPos = s.IndexOf(htmlStartTag);
            int inxStart = 0;
            int len = s.Length;
            int cnt = 0;

            inxStart = inxPos = s.IndexOf(htmlStartTag);
            if (inxPos != -1) {
                inxPos++;
                while (inxPos < len) {
                    if (s[inxPos] == '>') {
                        break;
                    }
                    cnt++;
                    inxPos++;
                }
                // Let's try to figure out what kind of tag closing it is.
                // That is, it is of the form </TAGNAME> or it is inside it or none at all 
                if (inxPos > len && s[inxPos - 1] == '/') {
                    // We are done
                    rslt = s.Remove(inxStart, cnt);
                }
            }
            return rslt;
        }

        /// <summary>
        /// Given parameter in the form '[DOMAIN]/BancoConocimiento/A/an_article/an_article.asp' 
        /// it will return 'an_article' to be used as the Url Title.
        /// </summary>
        /// <param name="s">URL Path</param>
        /// <returns>Url Title based in URL Path</returns>
        public static String ExtractUrlTitle(this String s) {
            String info = s;
            var splitInfo = s.Split('/');
            if (splitInfo.Length != 0) {
                info = splitInfo[splitInfo.Length - 1];
                info = info.SupressInsensitive(".asp");
            }
            else {
                info = s;
            }
            return info;
        }

        /// <summary>
        /// Given 'url' validates that has ASCII characters only.
        /// </summary>
        /// <param name="url">Data to act upon</param>
        /// <returns>True if all characters are ASCII</returns>
        public static bool IsUrlWithValidChars(this String url) {
            var rslt = true;
            var ach = url.ToCharArray();
            for (int i = 0; i < ach.Length; i++) {
                var ch = ach[i];
                if (ch > 127) {
                    rslt = false;
                    break;
                }
            }
            return rslt;
        }
    }
}
