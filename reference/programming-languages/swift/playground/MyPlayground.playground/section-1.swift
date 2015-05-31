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
let anotherArray = [Int]()
println(anotherArray)

let immutableArray = [42,25]
println(immutableArray)

var myArray = [1,2,3]
myArray.append(4)
myArray.insert(5, atIndex: 0)
myArray.insert(7, atIndex: 3)
myArray.insert(10, atIndex: 0)

myArray.removeAtIndex(0)
myArray.reverse()

myArray.count

// Dictionaries
var crew = [
    "Captain": "Jean-Luc Picard",
    "First Officer": "William Riker",
    "Second Officer": "Data"
];

crew["Captain"]

// Setting a new value
crew["Intern"] = "Wesley Crusher"
println(crew)

// This dictionary uses integers for both keys and values
var aNumberDictionary = [1:2]
aNumberDictionary[21] = 23
println(aNumberDictionary)

// empty dictionary
var aEmptyDictionary = [:]
var aMixedDictionary = ["Key": 1, 1:2]

if 1+1 == 2 {
    println("The math checks out, no need for parenthesis around the condition in the if statement")
}

if (2 != 30) {
    println("Not equal and parenthesis use")
}

if 1+1 == 2 {println("Cool!")}

println("Done")


for index in 1...5 {
    println("\(index) times 5 is \(index * 5)")
}

let base = 3
let power = 10
var answer = 1
for _ in 1...power {
    answer *= base
}
println("\(base) to the power of \(power) is \(answer)")
// prints "3 to the power of 10 is 59049"


let numberOfLegs = ["spider": 8, "ant": 6, "cat": 4]
for (animalName, legCount) in numberOfLegs {
    println("\(animalName)s have \(legCount) legs")
}
// ants have 6 legs
// cats have 4 legs
// spiders have 8 legs

var firstCounter = 0
for index in 1..<10{    firstCounter++}

