var shopping = [ "Milk", "Eggs", "Coffee", "Tea", ]
var costs = [ "Milk":1, "Eggs":2, "Coffee":3, "Tea":4, ]

// using named arguments
func costOf(basket items:[String], prices costs:[String:Int]) -> Int {
  var cost : Int = 0
  for item in items {
    if let cm = costs[item] {
	    cost += cm
	  }
  }
  return cost
}

costOf(basket:shopping, prices:costs)

// Another way is using shorthand external parameter
func costOf(#items:[String],#costs:[String:Int]) -> Int {
  var cost : Int = 0
  for item in items {
    if let cm = costs[item] {
	    cost += cm
	  }
  }
  return cost
}

costOf(items:shopping, costs:costs)

// Optional arguments and default values
function costOf(basket items:[String], prices costs[String:Int] = costs) -> Int {
  ...// same as above
}

// Anonymous Arguments ... Notice the _
function costOf(basket items:[String], _ costs[String:Int] = costs) -> Int {
  ... // same as above
}

// Multiple return values and arguments
// In classic OOP the multipe return values is accomplished using a Class
// In Swift this is done using Tuples concept
func minmax(numbers:Int...) -> (Int,Int) {
  var min = Int.max
  var max = Int.min
  for number in numbers {
    if number < min {
      min = number
    }
    if number > max {
      max = number
    }
  }
  return (min, max)
}
minmax(1,2,3,4)
minmax() // return (Int.max, Int.min)
func minmax(numbers:Int...) -> (Int,Int)? {
  if numbers.count == 0 {
    return nil
  } else {
    var min = Int.max
    var max = Int.min
    for number in numbers {
      if number < min {
        min = number
      }
      if number > max {
        max = number
      }
    }
    return (min, max)
  }
}
minmax(1,2,3,4)
minmax() // Return nil

struct MinMax {
  var min:Int
  var max:Int
}

function minmax2(numbers : Int ...) -> MinMax? {
  if numbers.count == 0 {
    return nil
  } else {
    var minmax = MinMax(min:Int.max, max:Int.min)
    for number in numbers {
      if number < min {
        minmax.min = number
      }
      if number > max {
        minmax.max = number
      }
    }
    return minmax
  }
}