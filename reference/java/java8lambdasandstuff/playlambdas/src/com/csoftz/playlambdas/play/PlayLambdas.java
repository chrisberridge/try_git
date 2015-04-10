package com.csoftz.playlambdas.play;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Comparator;
import java.util.List;
import java.util.Random;
import java.util.StringJoiner;
import java.util.function.BiFunction;
import java.util.function.Function;
import java.util.regex.Pattern;
import java.util.stream.Collectors;
import java.util.stream.IntStream;
import java.util.stream.Stream;

import javax.script.ScriptEngine;
import javax.script.ScriptEngineManager;

import com.csoftz.playlambdas.play.domain.Person;

public class PlayLambdas {
	public static void repeatMessage(String text, int count) {
		Runnable r = () -> {
			for (int i = 0; i < count; i++) {
				System.out.println(text);
				Thread.yield();
			}
		};
		new Thread(r).start();
	}

	public static void main(String[] args) {
		// http://www.drdobbs.com/jvm/lambda-expressions-in-java-8/240166764?pgno=1
		// and
		// https://leanpub.com/whatsnewinjava8/read
		List<String> lst = new ArrayList<String>();

		lst.add("mango");
		lst.add("pineapple");
		lst.add("orange");
		lst.add("apple");
		lst.add("lemon");
		lst.add("papaya");

		lst.forEach(x -> System.out.println(x)); // Can use System.out::println
													// instead see below.
		System.out.println("");

		lst.sort((first, second) -> first.compareTo(second));
		lst.forEach(System.out::println);

		Comparator<String> compStrings = (x, y) -> x.compareTo(y);
		lst.sort(compStrings);

		lst.add("guava");
		lst.sort(String::compareTo);// Use of method reference. It is the same
									// as (x,y)->x.compareTo(y)
		System.out.println("\r\nAdded new string\r\n");
		lst.forEach(System.out::println);
		repeatMessage("Print Hello 10 times", 10); // Prints Hello 1,000 times
													// in a separate thread
		// On Functional interfaces
		BiFunction<String, String, Integer> comp = (first, second) -> Integer
				.compare(first.length(), second.length());
		Function<String, String> atr = (name) -> {
			return "@" + name;
		};
		Function<String, Integer> leng = (name) -> name.length();
		Function<String, Integer> leng2 = String::length;

		String[] sample = { "Abc", "cuatro", "cinco" };

		for (String s : sample)
			System.out.println(leng2.apply(s));

		List<String> lst1 = Arrays.asList(sample);
		System.out.println("Again");
		lst1.forEach(s -> System.out.println(s + "-> " + leng2.apply(s)));

		// Create a list of Person Objects and then sort by last name and then
		// by first name.
		List<Person> personList = new ArrayList<Person>();
		personList.add(new Person("Carlos", "Ortiz"));
		personList.add(new Person("Luz Maritza", "Zuluaga"));
		personList.add(new Person("Luz Angelica", "Garcia"));
		personList.add(new Person("Joshua", "Ortiz"));
		personList.add(new Person("Joseph", "Ortiz"));

		System.out.println("Print Person List unordered");
		personList.forEach(s -> System.out.println(s.getLastName() + " "
				+ s.getFirstName()));
		personList.sort(Comparator.comparing(Person::getLastName)
				.thenComparing(Person::getFirstName));
		System.out.println("Print Person List ordered");
		personList.forEach(s -> System.out.println(s.getLastName() + " "
				+ s.getFirstName()));

		Pattern patt = Pattern.compile(",");
		patt.splitAsStream("a,b,c").forEach(System.out::println);

		IntStream.range(1, 11).forEach(System.out::println);

		Stream<Integer> s = Stream.of(5, 1, 2, 3);
		s.forEach(System.out::println);
		System.out.println("UnOrdered stream");
		s = Stream.of(5, 1, 2, 3);
		s.forEachOrdered(System.out::println);

		System.out.println("Ordered stream");
		s = Stream.of(5, 1, 2, 3);

		Random rnd = new Random();
		System.out.println("Unsorted random number list");
		rnd.ints().limit(10).forEach(System.out::println);

		System.out.println("Sorted random number list");
		rnd.ints().limit(10).sorted().forEach(System.out::println);

		String personNames = personList.stream()
				.map(p -> p.getFirstName() + " " + p.getLastName())
				.collect(Collectors.joining(","));
		System.out.println(personNames);

		// Use of String Join
		StringJoiner joiner = new StringJoiner(",");
		joiner.add("foo");
		joiner.add("bar");
		joiner.add("baz");
		String joined = joiner.toString(); // "foo,bar,baz"
		System.out.println(joined);

		// add() calls can be chained
		joined = new StringJoiner("-").add("foo").add("bar").add("baz")
				.toString(); // "foo-bar-baz"
		System.out.println(joined);

		// join(CharSequence delimiter, CharSequence... elements)
		joined = String.join("/", "2014", "10", "28"); // "2014/10/28"
		System.out.println(joined);

		// join(CharSequence delimiter, Iterable<? extends CharSequence>
		// elements)
		List<String> list = Arrays.asList("foo", "bar", "baz");
		joined = String.join(";", list); // "foo;bar;baz"
		System.out.println(joined);

		// nashorn run javascript in the Java runtime
		ScriptEngineManager engineManager = new ScriptEngineManager();
		ScriptEngine engine = engineManager.getEngineByName("nashorn");

		// http://winterbe.com/posts/2014/04/05/java8-nashorn-tutorial/
		// engine.eval("function p(s) { print(s) }");
		// engine.ev

		Function<Integer, String> f = Function.<Integer> identity()
				.andThen(i -> 2 * i).andThen(i -> "str" + i);
		
	}
}
