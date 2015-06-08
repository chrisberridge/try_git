struct MinMax {             // can create structures
  var min:Int               // min must be specified upon creation
  var max:Int               // max must be specified upon creation
}

func minmaxStruct(numbers:Int...) // function as before
 -> MinMax? {               // except returning a struct
  var minmax = MinMax(min:Int.max, max:Int.min) // init struct
  if(numbers.count == 0) {
    return nil              // nil as before for optionality
  } else {
    for number in numbers {
      if number < minmax.min {
        minmax.min = number // assign to struct member
      }
      if number > minmax.max {
        minmax.max = number // assign to struct member
      }
    }
    return minmax           // return struct as a whole
  }
}
minmaxStruct(1,2,3,4)