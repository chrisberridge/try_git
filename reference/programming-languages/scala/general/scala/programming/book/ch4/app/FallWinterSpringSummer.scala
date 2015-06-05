package scala.programming.book.ch4.app

import scala.programming.book.ch4.ChecksumAccumulator.calculate

// Adds trait App to execute full application
object FallWinterSpringSummer extends App {
  for (season <- List("fall", "winter", "spring"))
    println(season + ": " + calculate(season))
}