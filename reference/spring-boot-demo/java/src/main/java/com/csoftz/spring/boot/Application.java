/*----------------------------------------------------------------------------*/
/* Source File:   APPLICATION.JAVA                                            */
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
package com.csoftz.spring.boot.config;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.EnableAutoConfiguration;
import org.springframework.context.annotation.ComponentScan;
import org.springframework.context.annotation.Configuration;

/**
 * Configures Spring boot to be launched as an standalone java application be it
 * a web server or a console app.
 * 
 * @since 1.7(JDK), Aug.18/2014
 * @author Carlos Adolfo Ortiz Quirós (COQ)
 * @version 1.1, Aug.21/2014
 */
@Configuration
@ComponentScan
@EnableAutoConfiguration
public class Application {

	/**
	 * As entry execution point for an Spring Boot application which can run in
	 * command line using java -jar [pathtojar].
	 * 
	 * @param args
	 *            Command line arguments.
	 */
	public static void main(String[] args) {
		/*
		 * If you are to deploy the applicatin to HEROKU, the port is passed in
		 * a system environment as 'server.port'.
		 */
		String webPort = System.getenv("PORT");
		if (webPort == null || webPort.isEmpty()) {
			webPort = "8080";
		}
		System.setProperty("server.port", webPort);

		/*
		 * Configure Spring application.
		 */
		SpringApplication.run(Application.class, args);
	}
}
