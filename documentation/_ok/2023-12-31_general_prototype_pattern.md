```yaml
---
title: Prototype
source: https://refactoring.guru/design-patterns/prototype
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:21:15.320Z
domain: refactoring.guru
author: Unknown
category: general
technologies: [C#, C++, Go, Java, PHP, Python, Ruby, Rust, Swift, TypeScript]
programming_languages: [C#, C++, Go, Java, PHP, Python, Ruby, Rust, Swift, TypeScript, Pseudocode]
tags: [design-patterns, creational-patterns, object-cloning, software-design, polymorphism, inheritance, object-oriented-programming, prototype-pattern, object-creation, class-hierarchy]
key_concepts: [prototype-pattern, object-cloning, creational-design-patterns, polymorphism, class-hierarchy, prototype-registry, deep-copy, shallow-copy, alternative-to-inheritance]
code_examples: false
difficulty_level: intermediate
summary: |
  [The Prototype pattern is a creational design pattern that allows you to copy existing objects without making your code dependent on their concrete classes. It works by delegating the cloning process to the objects themselves, typically through a common `clone` method defined in a prototype interface. This pattern is particularly useful when you need to create copies of objects whose concrete types are unknown at compile time or when you want to reduce the number of subclasses used for object configuration. It offers an alternative to inheritance for managing complex object presets by enabling the use of pre-built, configurable prototypes. However, cloning complex objects with circular references can pose significant challenges.]
---
```

# Prototype

[](/)/ [Design Patterns](/design-patterns) / [Creational Patterns](/design-patterns/creational-patterns)

# Prototype

// Shorten examples titles for users. var h1 = document.getElementsByTagName("H1")\[0\]; if (h1.offsetHeight > 160) { h1.className += ' smaller'; } // Small beautification for pattern examples. var title = h1.innerHTML; title = title.replace(/^(Java|C\\+\\+|C#|PHP|Python|Ruby|Delphi): (.\*)$/, '<strong>$1:</strong> $2'); h1.innerHTML = title;

Also known as: Clone

## Intent

**Prototype** is a creational design pattern that lets you copy existing objects without making your code dependent on their classes.

![An illustration depicting a robot assembling another robot from parts, while several identical robots stand in a line, suggesting mass production or copying.](/images/patterns/content/prototype/prototype.png?id=e912b1ada20bbf7b2ffc09e93b9fab20)

## Problem

Say you have an object, and you want to create an exact copy of it. How would you do it? First, you have to create a new object of the same class. Then you have to go through all the fields of the original object and copy their values over to the new object.

Nice! But there’s a catch. Not all objects can be copied that way because some of the object’s fields may be private and not visible from outside of the object itself.

![A comic strip showing two primitive figures attempting to copy an airplane. In the first panel, they look at the plane, one asking "Do you think we'd be able to copy it?". In the second panel, "One month later", they stand next to a crudely drawn, messy copy of the plane, one exclaiming "PERFECT!". This humorously illustrates the difficulty of copying objects "from the outside" without proper internal access.](/images/patterns/content/prototype/prototype-comic-1-en.png?id=4cc45ae42e26cc9533a6ac540713d1fa)

Copying an object “from the outside” [isn’t](/cargo-cult) always possible.

There’s one more problem with the direct approach. Since you have to know the object’s class to create a duplicate, your code becomes dependent on that class. If the extra dependency doesn’t scare you, there’s another catch. Sometimes you only know the interface that the object follows, but not its concrete class, when, for example, a parameter in a method accepts any objects that follow some interface.

## Solution

The Prototype pattern delegates the cloning process to the actual objects that are being cloned. The pattern declares a common interface for all objects that support cloning. This interface lets you clone an object without coupling your code to the class of that object. Usually, such an interface contains just a single `clone` method.

The implementation of the `clone` method is very similar in all classes. The method creates an object of the current class and carries over all of the field values of the old object into the new one. You can even copy private fields because most programming languages let objects access private fields of other objects that belong to the same class.

An object that supports cloning is called a _prototype_. When your objects have dozens of fields and hundreds of possible configurations, cloning them might serve as an alternative to subclassing.

![A comic strip showing a small, wavy-lined character thinking "Okay, let's make some clones." while looking at three large cylindrical tanks labeled "CONFIG A", "CONFIG B", "CONFIG C", each containing a wavy-lined character. This illustrates the concept of pre-built, configured prototypes ready for cloning, as an alternative to subclassing.](/images/patterns/content/prototype/prototype-comic-2-en.png?id=e1df2dc39404c5eb2d485b7ae7c9914f)

Pre-built prototypes can be an alternative to subclassing.

Here’s how it works: you create a set of objects, configured in various ways. When you need an object like the one you’ve configured, you just clone a prototype instead of constructing a new object from scratch.

## Real-World Analogy

In real life, prototypes are used for performing various tests before starting mass production of a product. However, in this case, prototypes don’t participate in any actual production, playing a passive role instead.

![A four-panel comic strip illustrating cell division (mitosis). It shows a single cell dividing into two identical cells, with the final panel showing the two new cells greeting each other ("HEY, BRO!", "HELLO, BRO!"). This serves as a real-world analogy for the Prototype pattern, where the original cell actively participates in creating its copy.](/images/patterns/content/prototype/prototype-comic-3-en.png?id=ff16dedbd0c10944d27bf87d2adcf8a6)

The division of a cell.

Since industrial prototypes don’t really copy themselves, a much closer analogy to the pattern is the process of mitotic cell division (biology, remember?). After mitotic division, a pair of identical cells is formed. The original cell acts as a prototype and takes an active role in creating the copy.

## Structure

#### Basic implementation

![A UML-like diagram showing the basic structure of the Prototype design pattern. It depicts a `Client` interacting with a `Prototype` interface (which has a `clone()` method), and `ConcretePrototype` classes implementing this interface. Arrows indicate relationships and method calls for cloning.](/images/patterns/diagrams/prototype/structure.png?id=088102c5e9785ff45debbbce86f4df81)![A UML-like diagram showing the basic structure of the Prototype design pattern with indexed elements.](/images/patterns/diagrams/prototype/structure-indexed.png?id=0e1c809842f5c43aca0541a2eba1f844)

1.  The **Prototype** interface declares the cloning methods. In most cases, it’s a single `clone` method.
    
2.  The **Concrete Prototype** class implements the cloning method. In addition to copying the original object’s data to the clone, this method may also handle some edge cases of the cloning process related to cloning linked objects, untangling recursive dependencies, etc.
    
3.  The **Client** can produce a copy of any object that follows the prototype interface.
    

#### Prototype registry implementation

![A UML-like diagram illustrating the Prototype Registry implementation. It shows a `Client` interacting with a `PrototypeRegistry` (or `PrototypeCache`) which manages and provides access to multiple `ConcretePrototype` instances, allowing the client to request and clone pre-configured prototypes.](/images/patterns/diagrams/prototype/structure-prototype-cache.png?id=609c2af5d14ed55dcbb218a00f98e7d5)![A UML-like diagram illustrating the Prototype Registry implementation with indexed elements.](/images/patterns/diagrams/prototype/structure-prototype-cache-indexed.png?id=10a4a84a1a318f59dbc2b806fc936d04)

1.  The **Prototype Registry** provides an easy way to access frequently-used prototypes. It stores a set of pre-built objects that are ready to be copied. The simplest prototype registry is a `name → prototype` hash map. However, if you need better search criteria than a simple name, you can build a much more robust version of the registry.
    

@media (min-width: 1200px) { .structure { margin: 0; width: auto; height: 1190px; } .prototype-normal { height: 490px; } .prototype-pool { height: 500px; } .struct-image1 { left: 0px; top: 170px; } .struct-li1 { left: 300px; top: 70px; width: 260px; } .struct-li2 { left: 510px; top: 310px; width: 280px; } .struct-li3 { left: 0px; top: 70px; width: 240px; } .struct-image2 { left: 100px; top: 670px; } .struct-li4 { left: 0px; top: 1020px; width: 390px; } }

## Pseudocode

In this example, the **Prototype** pattern lets you produce exact copies of geometric objects, without coupling the code to their classes.

![A UML-like diagram illustrating the example of cloning geometric shapes (Shape, Rectangle, Circle). It shows a base `Shape` abstract class with a `clone()` method, and `Rectangle` and `Circle` concrete classes inheriting from `Shape` and implementing their own `clone()` methods, demonstrating how a class hierarchy can support cloning.](/images/patterns/diagrams/prototype/example.png?id=47bc6c1058cb100b81e675b5ca6bda6c)

Cloning a set of objects that belong to a class hierarchy.

All shape classes follow the same interface, which provides a cloning method. A subclass may call the parent’s cloning method before copying its own field values to the resulting object.

```
// Base prototype.
abstract class Shape is
    field X: int
    field Y: int
    field color: string

    // A regular constructor.
    constructor Shape() is
        // ...

    // The prototype constructor. A fresh object is initialized
    // with values from the existing object.
    constructor Shape(source: Shape) is
        this()
        this.X = source.X
        this.Y = source.Y
        this.color = source.color

    // The clone operation returns one of the Shape subclasses.
    abstract method clone():Shape


// Concrete prototype. The cloning method creates a new object
// in one go by calling the constructor of the current class and
// passing the current object as the constructor's argument.
// Performing all the actual copying in the constructor helps to
// keep the result consistent: the constructor will not return a
// result until the new object is fully built; thus, no object
// can have a reference to a partially-built clone.
class Rectangle extends Shape is
    field width: int
    field height: int

    constructor Rectangle(source: Rectangle) is
        // A parent constructor call is needed to copy private
        // fields defined in the parent class.
        super(source)
        this.width = source.width
        this.height = source.height

    method clone():Shape is
        return new Rectangle(this)


class Circle extends Shape is
    field radius: int

    constructor Circle(source: Circle) is
        super(source)
        this.radius = source.radius

    method clone():Shape is
        return new Circle(this)


// Somewhere in the client code.
class Application is
    field shapes: array of Shape

    constructor Application() is
        Circle circle = new Circle()
        circle.X = 10
        circle.Y = 10
        circle.radius = 20
        shapes.add(circle)

        Circle anotherCircle = circle.clone()
        shapes.add(anotherCircle)
        // The `anotherCircle` variable contains an exact copy
        // of the `circle` object.

        Rectangle rectangle = new Rectangle()
        rectangle.width = 10
        rectangle.height = 20
        shapes.add(rectangle)

    method businessLogic() is
        // Prototype rocks because it lets you produce a copy of
        // an object without knowing anything about its type.
        Array shapesCopy = new Array of Shapes.

        // For instance, we don't know the exact elements in the
        // shapes array. All we know is that they are all
        // shapes. But thanks to polymorphism, when we call the
        // `clone` method on a shape the program checks its real
        // class and runs the appropriate clone method defined
        // in that class. That's why we get proper clones
        // instead of a set of simple Shape objects.
        foreach (s in shapes) do
            shapesCopy.add(s.clone())

        // The `shapesCopy` array contains exact copies of the
        // `shape` array's children.
```

## Applicability

Use the Prototype pattern when your code shouldn’t depend on the concrete classes of objects that you need to copy.

This happens a lot when your code works with objects passed to you from 3rd-party code via some interface. The concrete classes of these objects are unknown, and you couldn’t depend on them even if you wanted to.

The Prototype pattern provides the client code with a general interface for working with all objects that support cloning. This interface makes the client code independent from the concrete classes of objects that it clones.

Use the pattern when you want to reduce the number of subclasses that only differ in the way they initialize their respective objects.

Suppose you have a complex class that requires a laborious configuration before it can be used. There are several common ways to configure this class, and this code is scattered through your app. To reduce the duplication, you create several subclasses and put every common configuration code into their constructors. You solved the duplication problem, but now you have lots of dummy subclasses.

The Prototype pattern lets you use a set of pre-built objects configured in various ways as prototypes. Instead of instantiating a subclass that matches some configuration, the client can simply look for an appropriate prototype and clone it.

## How to Implement

1.  Create the prototype interface and declare the `clone` method in it. Or just add the method to all classes of an existing class hierarchy, if you have one.
    
2.  A prototype class must define the alternative constructor that accepts an object of that class as an argument. The constructor must copy the values of all fields defined in the class from the passed object into the newly created instance. If you’re changing a subclass, you must call the parent constructor to let the superclass handle the cloning of its private fields.
    
    If your programming language doesn’t support method overloading, you won’t be able to create a separate “prototype” constructor. Thus, copying the object’s data into the newly created clone will have to be performed within the `clone` method. Still, having this code in a regular constructor is safer because the resulting object is returned fully configured right after you call the `new` operator.
    
3.  The cloning method usually consists of just one line: running a `new` operator with the prototypical version of the constructor. Note, that every class must explicitly override the cloning method and use its own class name along with the `new` operator. Otherwise, the cloning method may produce an object of a parent class.
    
4.  Optionally, create a centralized prototype registry to store a catalog of frequently used prototypes.
    
    You can implement the registry as a new factory class or put it in the base prototype class with a static method for fetching the prototype. This method should search for a prototype based on search criteria that the client code passes to the method. The criteria might either be a simple string tag or a complex set of search parameters. After the appropriate prototype is found, the registry should clone it and return the copy to the client.
    
    Finally, replace the direct calls to the subclasses’ constructors with calls to the factory method of the prototype registry.
    

## Pros and Cons

*   You can clone objects without coupling to their concrete classes.
*   You can get rid of repeated initialization code in favor of cloning pre-built prototypes.
*   You can produce complex objects more conveniently.
*   You get an alternative to inheritance when dealing with configuration presets for complex objects.

*   Cloning complex objects that have circular references might be very tricky.

## Relations with Other Patterns

*   Many designs start by using [Factory Method](/design-patterns/factory-method) (less complicated and more customizable via subclasses) and evolve toward [Abstract Factory](/design-patterns/abstract-factory), [Prototype](/design-patterns/prototype), or [Builder](/design-patterns/builder) (more flexible, but more complicated).
    
*   [Abstract Factory](/design-patterns/abstract-factory) classes are often based on a set of [Factory Methods](/design-patterns/factory-method), but you can also use [Prototype](/design-patterns/prototype) to compose the methods on these classes.
    
*   [Prototype](/design-patterns/prototype) can help when you need to save copies of [Commands](/design-patterns/command) into history.
    
*   Designs that make heavy use of [Composite](/design-patterns/composite) and [Decorator](/design-patterns/decorator) can often benefit from using [Prototype](/design-patterns/prototype). Applying the pattern lets you clone complex structures instead of re-constructing them from scratch.
    
*   [Prototype](/design-patterns/prototype) isn’t based on inheritance, so it doesn’t have its drawbacks. On the other hand, _Prototype_ requires a complicated initialization of the cloned object. [Factory Method](/design-patterns/factory-method) is based on inheritance but doesn’t require an initialization step.
    
*   Sometimes [Prototype](/design-patterns/prototype) can be a simpler alternative to [Memento](/design-patterns/memento). This works if the object, the state of which you want to store in the history, is fairly straightforward and doesn’t have links to external resources, or the links are easy to re-establish.
    
*   [Abstract Factories](/design-patterns/abstract-factory), [Builders](/design-patterns/builder) and [Prototypes](/design-patterns/prototype) can all be implemented as [Singletons](/design-patterns/singleton).
    

## Code Examples

[![Prototype in C# icon](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58)](/design-patterns/prototype/csharp/example "Prototype in C#") [![Prototype in C++ icon](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/prototype/cpp/example "Prototype in C++") [![Prototype in Go icon](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/prototype/go/example "Prototype in Go") [![Prototype in Java icon](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/prototype/java/example "Prototype in Java") [![Prototype in PHP icon](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/prototype/php/example "Prototype in PHP") [![Prototype in Python icon](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f)](/design-patterns/prototype/python/example "Prototype in Python") [![Prototype in Ruby icon](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/prototype/ruby/example "Prototype in Ruby") [![Prototype in Rust icon](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87)](/design-patterns/prototype/rust/example "Prototype in Rust") [![Prototype in Swift icon](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/prototype/swift/example "Prototype in Swift") [![Prototype in TypeScript icon](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/prototype/typescript/example "Prototype in TypeScript")

[![](/images/patterns/banners/patterns-book-banner-3.png?id=7d445df13c80287beaab234b4f3b698c)](/design-patterns/book)

### Support our free website and own the eBook!

*   22 design patterns and 8 principles explained in depth.
*   409 well-structured, easy to read, jargon-free pages.
*   225 clear and helpful illustrations and diagrams.
*   An archive with code examples in 11 languages.
*   All devices supported: PDF/EPUB/MOBI/KFX formats.

[Learn more…](/design-patterns/book)

#### Read next

[Singleton](/design-patterns/singleton) 

#### Return

 [Builder](/design-patterns/builder)

[![](/images/patterns/book/web-cover-en.png?id=328861769fd11617674e3b8a7e2dd9e7)](/design-patterns/book)

This article is a part of our eBook  
**Dive Into Design Patterns**.

[Learn more…](/design-patterns/book)