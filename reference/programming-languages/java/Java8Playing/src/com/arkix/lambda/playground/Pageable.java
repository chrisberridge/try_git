package com.arkix.lambda.playground;

import java.util.stream.IntStream;

public class Pageable {
	public static void main(String[] args) {
		/*
		 * for (int i = 0; i < 10; i++) { int z = (i + 1) * 10;
		 * IntStream.iterate(0, x -> x + 1).skip(i * 10).limit(z).forEach(val ->
		 * { System.out.print(val + " "); }); System.out.println(); }
		 */

		int numPage = 1;
		int numRecords = 10;
		IntStream.iterate(0, x -> x + 1).skip((numPage - 1) * numRecords).limit(numRecords)
				.forEach(System.out::println);
		System.out.println("-------");
		numPage++;
		IntStream.iterate(0, x -> x + 1).skip((numPage - 1) * numRecords).limit(numRecords)
		.forEach(System.out::println);
	}
}
