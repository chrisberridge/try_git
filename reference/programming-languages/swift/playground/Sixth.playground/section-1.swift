// Playground - noun: a place where people can play

import UIKit

var str = "Hello, playground"

// A protocol is a contract that a class must
// adhere to. (In ohter languages such as Java
// C# it is named as Interface
protocol Blinking {
    // This property must be (at least gettable)
    var isBlinking : Bool { get }
    
    // This property mut be gettable and settable
    var blinkSpeed : Double { get set }
    
    // This Function must exist, but what it does is up to the implemetator
    func startBlinking(blinkSpeed: Double) -> Void
}

// Using a potocol
class Light : Blinking {
    var isBlinking : Bool = false
    
    var blinkSpeed : Double = 0.0
    
    func startBlinking(blinkSpeed: Double) {
        println("I am now blinking")
        isBlinking = true
        
        // We say self.blinkSpeed here to help the compiller
        // tell the difference between the parameter 'blinkSpeed' and the property
        self.blinkSpeed = blinkSpeed
        
    }
}

// can be ANY object that has the Blinking protocol
var aBlinkingThing : Blinking?

aBlinkingThing = Light()

// Using ? after the variable name checks to see
// if aBlinkingThing has a value before trying to work with it
aBlinkingThing?.startBlinking(4.0)
aBlinkingThing?.blinkSpeed = 0.0

// Extensions
extension Int {
    var doubled : Int {
        return self * 2
    }
    func multiplyWith(anotherNumber : Int) -> Int {
       return self * anotherNumber
    }
}

2.doubled
4.multiplyWith(32)


extension Int : Blinking {
    var isBlinking : Bool {
        return false
    }
    var blinkSpeed : Double {
        get {
            return 0.0
        }
        set {
            // do nothing
        }
    }
    func startBlinking(blinkSpeed: Double) {
        println("I am the integer \(self). I do not blink")
    }
}

2.isBlinking
2.startBlinking(2.0)
10.doubled


var emptyString = String()
emptyString.isEmpty

emptyString = "Hello"
emptyString.isEmpty
emptyString.capitalizedString

var composingAString = "Hello"
composingAString += ", World!"

var reversedString = ""
for character in composingAString {
    reversedString = String(character) + reversedString
}
reversedString


countElements(composingAString)

let string1 : String = "Hello"
let string2 : String = "Hel" + "lo";

if string1 == string2 {
    println("The strings are equal")
}

if string1 as AnyObject === string2 as AnyObject {
    println("The strings are the same object")
}

string1.uppercaseString
string2.lowercaseString

if string1.hasPrefix("H") {
    println("String begins with an H")
}
if string1.hasSuffix("llo") {
    println("String ends in 'llo'")
}

let stringToConvert = "Hello, Swift"
let data = stringToConvert.dataUsingEncoding(NSUTF8StringEncoding)
data

// use of delegates
protocol HouseSecurityDelegate {
    // We don't define the function here, but rather
    // indicate that any class that is a HouseSecurityDelegate
    // is required to have a handleIntruder() function
    func handleIntruder()
}

class House {
    // The delegate can be any object that conforms to the HouseSecurityDelegate
    // protocol
    var delegate : HouseSecurityDelegate?
    
    func burglarDetected() {
        // Check to see if the delegate is there, then call it
        delegate?.handleIntruder()
    }
}

class GuardDog : HouseSecurityDelegate {
    func  handleIntruder() {
        println("Releasing the hounds!")
    }
}

let myHouse = House()
myHouse.burglarDetected() // does nothing

let theHounds = GuardDog()
myHouse.delegate = theHounds
myHouse.burglarDetected()

// resourcePath is now a string containing the absolute path
// reference to SomeFile.txt, or nil (as it is in this playground
// because it does not point to any bundle or package.
let resourcePath = NSBundle.mainBundle().pathForResource("SomeFile", ofType: "txt")

let resourceURl = NSBundle.mainBundle().URLForResource("SomeFile", withExtension: "txt")

