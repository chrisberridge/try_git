package com.csoftz.playlambdas.play;

import com.csoftz.playlambdas.play.intr.INamed;
import com.csoftz.playlambdas.play.intr.IPerson;

public class Student implements IPerson, INamed {

	@Override
	public long getId() {
		// TODO Auto-generated method stub
		return 0;
	}

	// Both IPerson and INamed has a default method, but the compiler does not
	// decide which one to take. It is up to you.
	public String getName() {
		return IPerson.super.getName(); // Here takes IPerson implementation.
		// return INamed.super.getName(); // Here takes INamed implementation.
		// If wished
	}
}
