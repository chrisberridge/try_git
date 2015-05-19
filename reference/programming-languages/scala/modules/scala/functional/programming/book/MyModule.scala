package scala.functional.programming.book
// A Comment!
/* Another Comment */
/** Documentation Comment */
object MyModule {
  def abs(n: Int): Int = {
    if (n < 0) {
      -n
    } else {
      n
    }
  }

  def factorial(n: Int): Int = {
    def go(n: Int, acc: Int): Int = {
      if (n <= 0) acc
      else go(n - 1, n * acc)
    }
    go(n, 1)
  }

  /**
   * This is a Higher Order Function or (HOF)
   */
  def formatResult(name: String, n: Int, f: Int => Int) = {
    val msg = "The %s of %d is %d."
    msg.format(name, n, f(n))
  }
  
  private def formatAbs(x: Int) = {
    val msg = "The Absolute value of %d is %d";
    msg.format(x, abs(x));
  }

  private def formatFactorial(n: Int) = {
    val msg = "The factorial of %d is %d."
    msg.format(n, factorial(n))
  }

  /**
   * Thus this is considered an impure function.
   */
  def main(args: Array[String]): Unit = {
    println(formatAbs(-42))
    println(formatFactorial(7))
    println("Another Way")
    println(formatResult("absolute value", -42, abs))
    println(formatResult("factorial", 7, factorial))
  }

}