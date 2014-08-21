package com.csoftz.spring.boot.web.controller;

import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestMapping;

@Controller
@RequestMapping("/greeting")
public class GreetingController {

	@RequestMapping("")
	public String greeting() {
		return "greeting";
	}

	@RequestMapping("/{name}")
	public String greetingName(
			@PathVariable String name,
			Model model) {
		model.addAttribute("name", name);
		return "greetingname";
	}

}