package com.csoftz.playlambdas.play.intr;

public interface INamed {

	default String getName() {
		return getClass().getName() + "_" + hashCode();
	}

}
