/*==========================================================================*/
/* Source File:   STRINGUTILS.CS                                            */
/* Description:   Helper class to assist with string operations.            */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Jul.08/2014                                               */
/* Last Modified: Sep.25/2014                                               */
/* Version:       1.5                                                       */
/* Copyright (c), 2014 Aleriant, El Colombiano                              */
/*==========================================================================*/

/*===========================================================================
History
Jul.08/2014 COQ File created.
============================================================================*/
using System;

namespace EC.Utils {
    /// <summary>
    /// Helper class to assist with string operations.
    /// </summary>
    public static class StringUtils {

        /// <summary>
        /// Given string example 'se fue la libertad' it is changed to 'Se fue la libertad'.
        /// </summary>
        /// <param name="s">Input data</param>
        /// <returns>Converts first Letter of string to Uppercase</returns>
        public static string UppercaseFirst(string s) {
            // Check for empty string.
            if (string.IsNullOrEmpty(s)) {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        /// <summary>
        /// Inspects string 's' to determine if there are non-ascii characte4rs and chenges them accordingly. 
        /// That is, if 'á' or 'é'  characters, for instance, are present in string 's', they are changed for one of their
        /// respective ascii character. ASCII Table: http://www.ascii-code.com/. NOTE: the ''' char is replaced by '_' among other
        /// characters. 
        /// WARNING: '.' is not replaced or tourched here as it is a file name delimiter.
        /// </summary>
        /// <param name="s">the string to analyze, and changed accordingly as well.</param>
        /// <returns>True if at least one non-ascii character is found.</returns>
        public static bool IsStrWithNonAsciiChars(ref string s) {
            bool changed = false;
            // Here we must determine if fileName has non-ascii characters and change accordingly.
            var ach = s.ToCharArray();
            for (int i = 0; i < ach.Length; i++) {
                var ch = ach[i];
                if (ch > 127) {
                    switch (ch) {
                        case '€':
                        case '‚':
                        case 'ƒ':
                        case '„':
                        case '…':
                        case '†':
                        case '‡':
                        case 'ˆ':
                        case '‰':
                        case 'Š':
                        case '‹':
                        case 'Œ':
                        case 'Ž':
                        case '‘':
                        case '’':
                        case '“':
                        case '”':
                        case '•':
                        case '–':
                        case '—':
                        case '˜':
                        case '™':
                        case 'š':
                        case '›':
                        case 'œ':
                        case 'ž':
                        case 'Ÿ':
                        case '¡':
                        case '¢':
                        case '£':
                        case '¤':
                        case '¥':
                        case '¦':
                        case '§':
                        case '¨':
                        case '©':
                        case 'ª':
                        case '«':
                        case '¬':
                        case '®':
                        case '¯':
                        case '°':
                        case '±':
                        case '²':
                        case '³':
                        case '´':
                        case 'µ':
                        case '¶':
                        case '·':
                        case '¸':
                        case '¹':
                        case 'º':
                        case '»':
                        case '¼':
                        case '½':
                        case '¾':
                        case 'Æ':
                        case '×':
                        case '÷':
                            changed = true;
                            ch = '_';
                            break;
                        case 'À':
                        case 'Á':
                        case 'Â':
                        case 'Ã':
                        case 'Ä':
                        case 'Å':
                            changed = true;
                            ch = 'A';
                            break;
                        case 'à':
                        case 'á':
                        case 'â':
                        case 'ã':
                        case 'ä':
                        case 'å':
                        case 'æ':
                            changed = true;
                            ch = 'a';
                            break;
                        case 'È':
                        case 'É':
                        case 'Ê':
                        case 'Ë':
                        case 'Ð':
                            changed = true;
                            ch = 'E';
                            break;
                        case 'è':
                        case 'é':
                        case 'ê':
                        case 'ë':
                        case 'ð':
                            changed = true;
                            ch = 'e';
                            break;
                        case 'Ì':
                        case 'Í':
                        case 'Î':
                        case 'Ï':
                            changed = true;
                            ch = 'I';
                            break;
                        case 'ì':
                        case 'í':
                        case 'î':
                        case 'ï':
                            changed = true;
                            ch = 'i';
                            break;
                        case 'Ò':
                        case 'Ó':
                        case 'Ô':
                        case 'Õ':
                        case 'Ö':
                        case 'Ø':
                            changed = true;
                            ch = 'O';
                            break;
                        case 'ò':
                        case 'ó':
                        case 'ô':
                        case 'õ':
                        case 'ö':
                        case 'ø':
                            changed = true;
                            ch = 'o';
                            break;
                        case 'Ù':
                        case 'Ú':
                        case 'Û':
                        case 'Ü':
                            changed = true;
                            ch = 'U';
                            break;
                        case 'ù':
                        case 'ú':
                        case 'û':
                        case 'ü':
                            changed = true;
                            ch = 'u';
                            break;
                        case 'Ç':
                            changed = true;
                            ch = 'C';
                            break;
                        case 'ç':
                            changed = true;
                            ch = 'c';
                            break;
                        case 'Ñ':
                            changed = true;
                            ch = 'N';
                            break;
                        case 'ñ':
                            changed = true;
                            ch = 'n';
                            break;
                        case 'Ý':
                            changed = true;
                            ch = 'Y';
                            break;
                        case 'ý':
                            changed = true;
                            ch = 'y';
                            break;
                        case 'Þ':
                        case 'þ':
                            changed = true;
                            ch = 'P';
                            break;
                        case 'ß':
                            changed = true;
                            ch = 's';
                            break;
                    }
                }
                else {
                    switch (ch) {
                        case '\'':
                        case ' ':
                        case '!':
                        case '"':
                        case '#':
                        case '$':
                        case '%':
                        case '&':
                        case '*':
                        case '+':
                        case ',':                        
                        case '/':
                        case ':':
                        case ';':
                        case '<':
                        case '=':
                        case '>':
                        case '?':
                        case '@':
                        case '[':
                        case '\\':
                        case ']':
                        case '^':
                        case '`':
                        case '|':
                        case '~':
                            changed = true;
                            ch = '_';
                            break;
                    }
                }
                ach[i] = ch;
            }
            s = new string(ach);
            return changed;
        }
    }
}