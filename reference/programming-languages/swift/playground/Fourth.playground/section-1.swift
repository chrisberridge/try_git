// Playground - noun: a place where people can play

import UIKit

var str = "Hello, playground"

var conditionalString : String? = "a string"if let theString = conditionalString? {
    println("The string is '\(theString)'")} else{    println("The string is nil")}

let myNumber = 3
let myString = "My number is \(myNumber)"
let myOtherString = "My number plus one is \(myNumber + 1)"

// Functions
func firstFunction() {
    println("Hello")
}
firstFunction()

func secondFunction() -> Int {
    return 123
}
secondFunction()

func thirdFunction(firstValue: Int, secondValue: Int) -> Int {
   return firstValue + secondValue
}
thirdFunction(1, 2)

func fourthFunction(firstValue: Int, secondValue: Int) -> (doubled: Int, quadrupled: Int) {
    return (firstValue * 2, secondValue * 4)
}
fourthFunction(2, 4)
fourthFunction(2, 4).0
fourthFunction(2, 4).1
fourthFunction(2, 4).doubled
fourthFunction(2, 4).quadrupled

// In the previous examples, no names were given to parameters.
// If you give names to parameters is very useful when it might not be immediately
// obvious what each parameter is ment to be used for.
func addNumbers(firstNumber num1 : Int, toSecondNumber num2 : Int) -> Int {
    return num1 + num2
}
addNumbers(firstNumber: 2, toSecondNumber: 4)

// in addNumbers firstNumber is the external name and num1 is the internal name,
// that is, the actual parameter to be used inside the function.
// If you need that the external name be the same as the internal name, use a pound sign (#)
// in front of the parameter name, as follows:
func multiplyNumbers(#firstNumber : Int, #multiplier : Int) ->Int {
    return firstNumber * multiplier
}
// this is how to invoke a function with
// named parameters
multiplyNumbers(firstNumber: 2, multiplier: 13)
    
// Function with default values for parameters. NOTE: If you define a default parameter its
// its given name becomes a named parameter and must be supplied when not using the default
// value as shown in the examples below.
func multiplyNumbers2(firstNumber: Int, multiplier: Int = 2) -> Int {
    return firstNumber * multiplier;
}
multiplyNumbers2(20)
multiplyNumbers2(10, multiplier: 50)


// Use of variable number of parameters (called variadic parameter).
// When using variadic parameters it must be last parameter to be defined.
func sumNumber2(startBase : Int = 0, numbers: Int...) -> Int {
    var total = 0
    
    if startBase != 0 {
        total = startBase
    }
    
    for number in numbers {
       total += number
    }
    return total
}
let sum = sumNumber2(startBase:200, 2,3,4,5)
println(sum)

// Parameters are always by Value, if you need them by Reference use the inout modifier
// as follows
func swapValues(inout firstValue: Int, inout secondValue : Int) {
    let tempValue = firstValue
    firstValue = secondValue
    secondValue = tempValue
}
var swap1 = 2
var swap2 = 3
swapValues(&swap1, &swap2)
    
// Using Functions as Variables
var numbersFunc : (Int, Int) ->Int

numbersFunc = addNumbers
numbersFunc(2, 3)

// Functions can also receive other functions as parameters, and use them.  
// This means that you can combine functions together.
func timesThree(number:Int) -> Int {
    return number * 3
}

func doSomethingToNumber(aNumber: Int, thingToDo : (Int)->Int) -> Int {
    // we´ve received some function as a parameter, which we refer
    // to as 'thingToDo' inside this function.
    
    // Call the function 'thingToDo' using 'aNumber', and return the result
    return thingToDo(aNumber)
}
// Give the 'timesThree' function to use as 'thingToDo'
doSomethingToNumber(4, timesThree)

// Functions can also return other functions.
func createAdder(numberToAdd : Int) -> (Int) -> Int {
    func adder(number : Int) -> Int {
        return number + numberToAdd
    }
    return adder
}
var addTwo = createAdder(2)
addTwo(2)
 
// function that captures a value.
// With this feature you can create functions that acts as
// generators -- functions that return different values each
// time they´re called.
func createIncrementor(incrementAmount : Int) -> () -> Int{
    var amount = 0 // Captured value
    func incrementor() -> Int {
        amount += incrementAmount
        return amount
    }
    return incrementor
}

var incrementByTen = createIncrementor(10)
incrementByTen()
incrementByTen()

var incrementByFifteen = createIncrementor(15)

incrementByFifteen()
incrementByTen()
incrementByFifteen()

// Closures
var sortingInline = [2,5,98,2,13]
sort(&sortingInline)
sortingInline

var numbers = [2, 1, 56, 32, 120,13]
var numbersSorted = sorted(numbers, { (n1: Int, n2 : Int) -> Bool in
    return n2 > n1
})

var numbersSortedReverse = sorted(numbers, {n1, n2 in
    return n1 > n2
})

var numbersSortedAgain = sorted(numbers) {$1 > $0}

// Closures stored in a variable
var comparator = {(a : Int, b : Int) in a < b}
comparator(1,2)

// Objects
class Vehicle {
    var color : String?
    var maxSpeed = 80
    
    func description() -> String {
        return "The  \(self.color!) Color vehicle"
    }
    
    func travel() {
        println("Traveling at \(self.maxSpeed) kph")
    }
}

class Car : Vehicle {
    // Inherited classes can override functions
    override func description() -> String {
        var description = super.description()
        return description + ", which is a car"
    }
}

var redVehicle = Vehicle()
redVehicle.color = "Red"
redVehicle.maxSpeed = 90
redVehicle.travel()
redVehicle.description()

var carVehicle = Car()
carVehicle.color = "Green"
carVehicle.maxSpeed = 60
carVehicle.travel()
carVehicle.description()

class InitAndDeinitExample {
    // Designated (i.e., main) initializer
    init() {
       println("I´ve been created!")
    }
    
    // Convenience initializer, required to call the
    // designated initializer (above)
    convenience init(text: String) {
        self.init() // This is mandatory
        println("I was called with the convenience initializer!")
    }
    
    // Deinitializer
    deinit {
        println("I´m going away")
    }
}

var example : InitAndDeinitExample?

// using the designated initializer
example = InitAndDeinitExample()
example = nil

// using the convenience initializer
example = InitAndDeinitExample(text: "Hello")

// Stored property
// Computed property
class Rectangle {
    var width : Double = 0.0 // Stored property
    var height : Double = 0.0 // Stored property
    // Computed property
    var area : Double {
       // computed getter
       get {
          return width * height
       }
    
       // Computed setter
       set {
          // Assume equal dimensions (i.e., a square)
          width = sqrt(newValue)
          height = sqrt(newValue)
       }
    }
}

var rect = Rectangle()
rect.width = 3.0
rect.height = 4.5
rect.area
rect.area = 9
rect.area

