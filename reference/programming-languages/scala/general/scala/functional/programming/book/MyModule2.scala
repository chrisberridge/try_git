package scala.functional.programming.book

object MyModule2 {
  /**
   * Monomorphic function example.
   */
  def findFirst(ss: Array[String], key: String): Int = {
    @annotation.tailrec
    def loop(n: Int): Int =
      if (n >= ss.length) -1
      else if (ss(n) == key) n
      else loop(n + 1)
    loop(0)
  }

  /**
   * Polymorphic function example
   */
  def findFirst[A](as: Array[A], p: A => Boolean): Int = {
    @annotation.tailrec
    def loop(n: Int): Int =
      if (n >= as.length) -1
      else if (p(as(n))) n
      else loop(n + 1)
    loop(0)
  }

  def main(args: Array[String]): Unit = {
    println(findFirst(Array(7, 9, 13), (x: Int) => x == 9))    
  }
}