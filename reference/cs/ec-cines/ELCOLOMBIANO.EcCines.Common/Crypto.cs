/*==========================================================================*/
/* Source File:   CRYPTO.CS                                                 */
/* Description:   Static class to suply Triple DES encrypt/decrypt services */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/*                Juan Cuartas (JC)                                         */
/* Date:          Feb.19/2015                                               */
/* Last Modified: Feb.19/2015                                               */
/* Version:       1.1                                                       */
/* Copyright (c), 2015 Arkix, El Colombiano                                 */
/*==========================================================================*/

/*===========================================================================
History
Feb.19/2015 COQ File created.
============================================================================*/

using System;
using System.Security.Cryptography;
using System.Text;

namespace ELCOLOMBIANO.EcCines.Business
{
    /// <summary>
    /// Static class to suply Triple DES encrypt/decrypt services
    /// </summary>
    public class Crypto
    {
        /// <summary>
        /// Encrypts the received string using TripleDES algorithm.
        /// </summary>
        /// <param name="str">Data to encrypt.</param>
        /// <param name="key">Encription key.</param>
        /// <returns>Encrypted string or NULL if it could not be encrypted.
        /// </returns>
        public static string Encrypt(string str, string key)
        {
            try
            {
                byte[] textArray = UTF8Encoding.UTF8.GetBytes(str);

                MD5CryptoServiceProvider hashmd5
                    = new MD5CryptoServiceProvider();

                byte[] keyArray = hashmd5.ComputeHash
                (
                    UTF8Encoding.UTF8.GetBytes(key)
                );

                hashmd5.Clear();

                TripleDESCryptoServiceProvider tdes
                    = new TripleDESCryptoServiceProvider();

                tdes.Key = keyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform cTrans = tdes.CreateEncryptor())
                {
                    byte[] resultArray = cTrans.TransformFinalBlock
                    (
                        textArray, 0, textArray.Length
                    );

                    tdes.Clear();

                    return Convert.ToBase64String
                    (
                        resultArray, 0, resultArray.Length
                    );
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Decrypts the received string using the TripleDES algorithm.
        /// </summary>
        /// <param name="str">Decrypt string.</param>
        /// <param name="key">Encription key.</param>
        /// <returns>Encrypted string or NULL if it could not be encrypted.
        public static string Decrypt(string str, string key)
        {
            try
            {
                byte[] textArray = Convert.FromBase64String(str);

                MD5CryptoServiceProvider hashmd5
                    = new MD5CryptoServiceProvider();

                byte[] keyArray = hashmd5.ComputeHash
                (
                    UTF8Encoding.UTF8.GetBytes(key)
                );

                hashmd5.Clear();

                TripleDESCryptoServiceProvider tdes
                    = new TripleDESCryptoServiceProvider();

                tdes.Key = keyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform cTrans = tdes.CreateDecryptor())
                {
                    byte[] resultArray = cTrans.TransformFinalBlock
                    (
                        textArray, 0, textArray.Length
                    );

                    tdes.Clear();

                    return UTF8Encoding.UTF8.GetString
                    (
                        resultArray, 0, resultArray.Length
                    );
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the hash MD5 for received string
        /// </summary>
        /// <param name="str">Data to use</param>
        /// <returns>The Hash MD5 for supplied string.</returns>
        public static string GetMD5Hash(string str)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                byte[] textArray = Encoding.UTF8.GetBytes(str);

                MD5CryptoServiceProvider hashmd5
                    = new MD5CryptoServiceProvider();

                byte[] keyArray = hashmd5.ComputeHash(textArray);

                foreach (byte b in keyArray)
                {
                    sb.Append(b.ToString("x2").ToLower());
                }

                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }
    }
}
