This is a sample web application that contains JSR 168 portlets developed
using the Spring Portlet MVC framework.

BUILDING

You will need a fully functional Java 5 (or later) JDK and Maven 2 environment
(see http://maven.apache.org/) in order to build this project from source.

To build a deployable portlet application war file, simply run the command:

	mvn clean package

The resulting war file will be in the "target" directory.

The build process will automatically preconfigure the war file to run in a
Pluto 1.1 portlet container.  If you are deploying to a different environment,
you will want to strip out the 'maven-pluto-plugin' entry in the plugins
section of the pom.xml file, and follow the instructions for your target
portlet container.

DEPLOYING

Maven is capable of automatically deploying this webapp to a target
application server by using the 'cargo' plugin.  Please look at the
'cargo-maven2-plugin' entry in the plugins section of the pom.xml file for
the configuration.

Once properly configured, the webapp can be completely built and deployed
using the command:

	mvn clean package cargo:redeploy

The target application server does need to be up and running for this work
properly.

COMPATIBILITY

This sample application uses Java 5 features and is intended for a container
that supports Servlet 2.4 and JSP 2.0, such as Tomcat 5.5.  It has not been
tested extensively with any other servlet containers.

There are a number of required libraries that get included in the WEB-INF/lib
directory of this webapp.  Be aware that having duplicate libraries in any of
the other class-loaders can create serious problems.  Depending on your portal,
you may well already have some of these libraries in a shared class-loader.  Be
sure to move/delete any duplicate libraries out of this webapp or bad things
may happen.

ECLIPSE

This project contains project metadata for use in Eclipse.  Be sure to add
the project into Eclipse using "Import - Existing Projects into Workspace".

We recommend the Eclipse IDE Europa 3.3 for Java EE Developers distribution
available for download at: http://www.eclipse.org/downloads/

To properly edit and build this project in Eclipse, you need to install the
following plugins:

	M2Eclipse (Maven Integration)
	Web Site: http://m2eclipse.codehaus.org/
	Eclipse Update Site: http://m2eclipse.sonatype.org/update/

	SpringIDE (Spring Framework Integtration)
	Web Site: http://springide.org/
	Eclipse Update Site: http://springide.org/updatesite/

CONTACT

If you have any questions or problems with this sample, please post your issues
in the Spring Framework Support Forums (http://forum.springframework.org/),
specifically in the 'Web' Forum.

