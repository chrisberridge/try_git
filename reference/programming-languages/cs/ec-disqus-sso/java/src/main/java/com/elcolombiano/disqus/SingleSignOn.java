/*----------------------------------------------------------------------------*/
/* Source File:   SINGLESIGNON.JAVA                                           */
/* Description:   Generates the payload we need to authenticate users remotely*/
/*                through Disqus                                              */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                            */
/* Date:          Mar.31/2014                                                 */
/* Last Modified: Apr.02/2014                                                 */
/* Version:       1.1                                                         */
/* Copyright (c), 2014 El Colombiano, Aleriant                                */
/*----------------------------------------------------------------------------*/
/*-----------------------------------------------------------------------------
 History
 Mar.31/2014 COQ File created.
 -----------------------------------------------------------------------------*/
package com.elcolombiano.disqus;

import java.security.InvalidKeyException;
import java.security.NoSuchAlgorithmException;
import java.security.SignatureException;
import java.util.Formatter;
import java.util.HashMap;

import javax.crypto.Mac;
import javax.crypto.spec.SecretKeySpec;

import org.apache.commons.codec.binary.Base64;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;

/**
 * This class generates the payload we need to authenticate users remotely
 * through Disqus<br>
 * <br>
 * This requires the Disqus SSO package and to have set up your
 * application/remote domain properly<br>
 * <br>
 * See here for more:
 * <code>http://help.disqus.com/customer
 * /portal/articles/236206-integrating-single-sign-on</code>
 * <br><br>
 * Usage: After inputting user data, a final payload will be generated which you
 * use for the javascript variable 'remote_auth_s3'<br><br> 
 * Disqus API secret key can be obtained here: <code>http://disqus.com/api/applications/</code><br>
 * This will only work if that key is associated with your SSO remote domain
 *
 * @since 1.5(JDK), Mar.31/2014
 * @author Carlos Adolfo Ortiz Quirós (COQ)
 * @version 1.1, Apr.02/2014
 */
public class SingleSignOn {

	// Your Disqus secret key from http://disqus.com/api/applications/
	private String DISQUS_SECRET_KEY = "cxVu9uZLqObojz7P7ctQueTY48XYAXPkiDdHsMrVxt9Myszk2Os0mrc0PyxaFNoF";

	/**
	 * Given array converts to a Hex string representation.
	 * 
	 * @param bytes
	 *            Data to convert
	 * @return A hex string for given 'bytes' array.
	 */
	private String toHexString(byte[] bytes) {
		Formatter formatter = new Formatter();
		for (byte b : bytes) {
			formatter.format("%02x", b);
		}
		String rslt = formatter.toString();
		formatter.close();

		return rslt;
	}

	/**
	 * Encrypts data using key.
	 * 
	 * @param data
	 *            Supplied information
	 * @param key
	 *            Encryption key.
	 * @return A full string in HEX format with the desired information.
	 * @throws SignatureException
	 * @throws NoSuchAlgorithmException
	 * @throws InvalidKeyException
	 */
	private String calculateRFC2104HMAC(String data, String key)
			throws SignatureException, NoSuchAlgorithmException,
			InvalidKeyException {
		final String HMAC_SHA1_ALGORITHM = "HmacSHA1";
		SecretKeySpec signingKey = new SecretKeySpec(key.getBytes(),
				HMAC_SHA1_ALGORITHM);
		Mac mac = Mac.getInstance(HMAC_SHA1_ALGORITHM);
		mac.init(signingKey);
		return toHexString(mac.doFinal(data.getBytes()));
	}

	/**
	 * Conforms to Disqus authentication requirements by passing an encrypted
	 * Json user data, a signature and a timestamp, all encoded.
	 * 
	 * @param params
	 *            User data to encode.
	 * @return String with encoded data in Json format with user data, signature
	 *         and timestamp.
	 * @throws JsonProcessingException
	 * @throws SignatureException
	 * @throws NoSuchAlgorithmException
	 * @throws InvalidKeyException
	 */
	public String generatePayload(HashMap<String, String> params)
			throws JsonProcessingException, SignatureException,
			NoSuchAlgorithmException, InvalidKeyException {
		// Encode user data
		ObjectMapper mapper = new ObjectMapper();
		String jsonMessage = mapper.writeValueAsString(params);

		String base64EncodedStr = new String(Base64.encodeBase64(jsonMessage
				.getBytes()));

		// Get the timestamp
		long timestamp = System.currentTimeMillis() / 1000;

		// Assemble the HMAC-SHA1 signature
		String signature = new SingleSignOn().calculateRFC2104HMAC(
				base64EncodedStr + " " + timestamp, DISQUS_SECRET_KEY);

		return base64EncodedStr + " " + signature + " " + timestamp;
	}

	/**
	 * If it is required to logout current session, if needed.
	 * 
	 * @return String with encoded data in Json format with user data, signature
	 *         and timestamp.
	 * @return String with encoded data in Json format with user data, signature
	 *         and timestamp.
	 * @throws JsonProcessingException
	 * @throws SignatureException
	 * @throws NoSuchAlgorithmException
	 * @throws InvalidKeyException
	 */
	public String logoutUser() throws JsonProcessingException, SignatureException,
			NoSuchAlgorithmException, InvalidKeyException {
		HashMap<String, String> userData = new HashMap<String, String>();
		return generatePayload(userData);
	}

	public static void main(String[] args) throws InvalidKeyException,
			JsonProcessingException, SignatureException,
			NoSuchAlgorithmException {

		// User data, replace values with authenticated user data
		HashMap<String, String> userData = new HashMap<String, String>();

		userData.put("id", "uniqueId_123456789");
		userData.put("username", "Charlie Chaplin");
		userData.put("email", "charlie.chaplin@example.com");
		userData.put("avatar", "");

		/*
		 * message.put("id", "idnew_20140401_1"); message.put("username",
		 * "Frank Borland"); message.put("email", "frank.borland@info.com");
		 */

		/*
		 * userData.put("id", "idnew_20140401_2"); userData.put("username",
		 * "Maesse the OWLO"); userData.put("email", "theowlo@gmail.com");
		 * userData.put("avatar",
		 * "https://s.gravatar.com/avatar/56859799e061b7c87c15d481251af410?s=80"
		 * );
		 */

		SingleSignOn sso = new SingleSignOn();
		// Output string to use in remote_auth_s3 variable
		System.out.println(sso.generatePayload(userData));
		System.out.println();

		System.out.println(sso.logoutUser());
	}
}
