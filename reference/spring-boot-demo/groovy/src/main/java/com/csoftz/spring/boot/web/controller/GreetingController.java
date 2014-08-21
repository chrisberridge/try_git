/*----------------------------------------------------------------------------*/
/* Source File:   GREETINGCONTROLLER.JAVA                                     */
/* Description:   Web controller for /greeting URI part.                      */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                            */
/* Date:          Aug.18/2014                                                 */
/* Last Modified: Aug.20/2014                                                 */
/* Version:       1.1                                                         */
/* Copyright (c), 2014 CSoftZ                                                 */
/*----------------------------------------------------------------------------*/
/*-----------------------------------------------------------------------------
 History
 Aug.18/2014 COQ  File created.
 -----------------------------------------------------------------------------*/
package com.csoftz.spring.boot.web.controller;

import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestMapping;

/**
 * Web controller for /greeting URI part
 *
 * @since 1.7(JDK), Aug.18/2014
 * @author Carlos Adolfo Ortiz Quirós (COQ)
 * @version 1.1, Aug.20/2014
 */
@Controller
@RequestMapping("/greeting")
public class GreetingController {

	/**
	 * Uses Spring MVC to match URL like [ServerApp]/greeting (aka index page).
	 * 
	 * @return View template page named greeting.html in thymeleaf engine.
	 */
	@RequestMapping("")
	public String greeting() {
		return "greeting";
	}

	/**
	 * Uses Spring MVC to match URL like [ServerApp]/greeting/{name} (aka index
	 * page).
	 * 
	 * @param name
	 *            Parameter given in URL
	 * @param model
	 *            Data to be sent from Controller to view.
	 * @return View template page named greetingname.html in thymeleaf engine.
	 */
	@RequestMapping("/{name}")
	public String greetingName(@PathVariable String name, Model model) {
		model.addAttribute("name", name);
		return "greetingname";
	}
}