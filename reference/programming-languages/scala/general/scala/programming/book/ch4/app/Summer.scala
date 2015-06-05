package scala.programming.book.ch4.app

import scala.programming.book.ch4.ChecksumAccumulator.calculate

// Standalone object. This way you can define a Scala Application.
object Summer {
  def main(args: Array[String]) {
    for (arg <- args)
      println(arg + ": " + calculate(arg))
  }
}