spring-boot-demo README
=======================
V1.0.10.10-Aug.21/2014

In this folder you will find a demo application built using the PoC (Proof of concept) 
for [Spring Boot Framework](http://projects.spring.io/spring-boot/ "Spring Boot Framework") which by the 
way uses the stellar [Spring Framework](http://projects.spring.io/spring-framework/ "Spring Framework").

Concepts
--------
The application shown is bare-bones. It only shows how to configure the application as a getting started but also shows one web page by 
pointing browser to http://localhost:8080/greeting or http://localhost:8080/greeting/hello and it is useful for learning.
Anyway, you have to be careful not to make the same mistakes I suffered by not reading documentation.

First, code samples are based on 1.1.5.RELEASE for spring boot (and tend to be upgraded from time to time to reflect latest versions, so 
stay tuned) and they use [Apache Maven](http://maven.apache.org/ "Apache Maven").

<b>NOTE:</b> You can use [Spring Starter Project](http://start.spring.io/ "Spring Starter Project") to initialize project settings with current
Spring Boot Best practices.

Notice that one version is written entirely in Java (contained in the Java folder while the other version is written part in Groovy and part in Java (contained in Groovy Folder).

<b>WARNING:</b> When writing the Spring-boot configuration, as Spring Java Config is used with the help of annotations, you must base these
configuration files in a root package of its own. Next is the structure that reflects this idea from [src] folder like so: `src/main/com/csoftz/spring/boot` as the 
@EnableAutoConfiguration would start from this folder to scan for all componentes as well and it is also recommended in documentation.

Heroku
------
Files in directory named 'Procfile' and 'system.properties' are used by [Heroku](https://www.heroku.com/) cloud platform.