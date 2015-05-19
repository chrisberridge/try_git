package com.csoftz.playlambdas.play.intr;

public interface IPerson {
	long getId();

	default String getName() {
		return "John Q. Public";
	}
}
