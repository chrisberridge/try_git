/*----------------------------------------------------------------------------*/
/* Source File:   SERVLETINITIALIZER.JAVA                                     */
/* Description:   Embedded Servlet Initializer.                               */
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
package com.csoftz.spring.boot.config;

import org.springframework.boot.builder.SpringApplicationBuilder;
import org.springframework.boot.context.web.SpringBootServletInitializer;

/**
 * Configures Spring boot to signal embedded web container, be it tomcat or
 * jetty to know how to start the web app (this way it allows that no WAR file
 * be necessary).
 * 
 * @since 1.7(JDK), Aug.18/2014
 * @author Carlos Adolfo Ortiz Quirós (COQ)
 * @version 1.1, Aug.21/2014
 */
public class ServletInitializer extends SpringBootServletInitializer {

	/**
	 * Automatic Java configuration for web servlet configuration. No WEB.XML
	 * required.
	 * 
	 * @see org.springframework.boot.context.web.SpringBootServletInitializer#configure(org.springframework.boot.builder.SpringApplicationBuilder)
	 */
	@Override
	protected SpringApplicationBuilder configure(
			SpringApplicationBuilder application) {
		return application.sources(Application.class);
	}

}
