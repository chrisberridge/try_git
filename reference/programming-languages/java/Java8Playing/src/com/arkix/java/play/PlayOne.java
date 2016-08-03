package com.arkix.java.play;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.stream.Collectors;

public class PlayOne {
	public static void main(String[] args) {
		/*
		 * Sample taken from URL:
		 * [https://zeroturnaround.com/rebellabs/streams-the-real-powerhouse-in-
		 * java-8-by-venkat-subramaniam/]
		 */
		List<String> names = new ArrayList<String>();
		List<Person> people = new ArrayList<>();
		people.stream().filter(p -> p.getGender() == "Female").map(Person::getName).map(String::toUpperCase)
				.forEach(name -> names.add(name));

		names.forEach(System.out::println);

		// Better do this instead;
		List<String> namesCollectors = people.stream().filter(p -> p.getGender() == "Female").map(Person::getName)
				.map(String::toUpperCase).collect(Collectors.toList());
		namesCollectors.forEach(System.out::println);
		
		Map<String, Person> namesMap = people.stream()
				  .collect(Collectors.toMap(p -> p.getName(), p -> p)); 
		
		// filter, map,  are intermediate. No computation.
		// Operations like forEach, collect, reduce are terminal. Computation takes place here.
		
	}
}
