// Playground - noun: a place where people can play

import UIKit

var str = "Hello, playground"
var notice = 10
notice += 10

func sumNumber(numbers: Int...) -> Int {
    var total = 0
    for number in numbers {
        total += number
    }
    return total
}
let sum = sumNumber(2,3,4,5)
println(sum)

let myConstantVariable = 123
var myVariable = 123
myVariable += 5

// Optional variables
var anOptionalInteger : Int? = nil
anOptionalInteger = 42

if anOptionalInteger != nil {
    println("It has a value!")
} else {
    println("It has no value!")
}

1 + anOptionalInteger!
let aString = String(anOptionalInteger!)
println(aString)

let anotherString = String(myVariable)

let aTuple = (1, "Changes")
let theNumber = aTuple.0
let theString = aTuple.1

// using labels for tuples
let anotherTuple = (aNumber: 1, aString: "Yes")
let theOtherNumber = anotherTuple.aNumber
let theOtherString = anotherTuple.aString


// Arrays
let arrayOfIntegers : [Int] = [1,2,3]
let implicitArrayOfIntegers = [1,2,3]


// You can also create an empty array, but you must provide the type
let anotherArray =[Int]()
println(anotherArray)

let immutableArray = [42,25]
println(immutableArray)