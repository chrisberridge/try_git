/*----------------------------------------------------------------------------*/
/* Source File:   APPLICATIONTEST.JAVA                                        */
/* Description:   Command line application launcher.                          */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                            */
/* Date:          Aug.18/2014                                                 */
/* Last Modified: Aug.21/2014                                                 */
/* Version:       1.1                                                         */
/* Copyright (c), 2014 CSoftZ                                                 */
/*----------------------------------------------------------------------------*/
/*-----------------------------------------------------------------------------
 History
 Aug.18/2014 COQ  File created.
 -----------------------------------------------------------------------------*/
package com.csoftz.spring.boot.test;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.test.context.web.WebAppConfiguration;
import org.springframework.boot.test.SpringApplicationConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.csoftz.spring.boot.Application;

/**
 * Configures Spring boot to be launched as an standalone java application be it
 * a web server or a console app. This is a test case harness to validate app
 * packaging.
 * 
 * @since 1.7(JDK), Aug.18/2014
 * @author Carlos Adolfo Ortiz Quirós (COQ)
 * @version 1.1, Aug.21/2014
 */

@RunWith(SpringJUnit4ClassRunner.class)
@SpringApplicationConfiguration(classes = Application.class)
@WebAppConfiguration
public class ApplicationTests {

	@Test
	public void contextLoads() {
	}

}
