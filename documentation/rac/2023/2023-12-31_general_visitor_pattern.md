```yaml
---
title: Visitor
source: https://refactoring.guru/design-patterns/visitor
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:10:48.722Z
domain: refactoring.guru
author: Unknown
category: general
technologies: []
programming_languages: [C#, C++, Go, Java, PHP, Python, Ruby, Rust, Swift, TypeScript, Pseudocode]
tags: [design-pattern, behavioral-pattern, object-oriented, polymorphism, double-dispatch, code-extensibility, separation-of-concerns, software-design, refactoring, code-structure]
key_concepts: [Visitor pattern, behavioral design pattern, double dispatch, polymorphism, Open/Closed Principle, Single Responsibility Principle, object structure traversal, separation of concerns]
code_examples: false
difficulty_level: intermediate
summary: |
  The Visitor pattern is a behavioral design pattern that allows you to separate algorithms from the objects on which they operate. It addresses the challenge of adding new operations to existing class hierarchies without modifying the original classes, promoting the Open/Closed Principle. The pattern achieves this by introducing a separate "visitor" class that contains the new behavior, using a technique called double dispatch to ensure the correct method is executed for each object type. While requiring a minor initial change to existing classes, it enables future extensibility for new behaviors without further modifications to the core object structure. The article provides a problem scenario, a detailed solution, real-world analogy, structure, pseudocode, applicability, implementation steps, and discusses pros and cons.
---
```

# Visitor

[](/)/ [Design Patterns](/design-patterns) / [Behavioral Patterns](/design-patterns/behavioral-patterns)

# Visitor

// Shorten examples titles for users. var h1 = document.getElementsByTagName("H1")[0]; if (h1.offsetHeight > 160) { h1.className += ' smaller'; } // Small beautification for pattern examples. var title = h1.innerHTML; title = title.replace(/^(Java|C\+\+|C#|PHP|Python|Ruby|Delphi): (.*)$/, '<strong>$1:</strong> $2'); h1.innerHTML = title;

## Intent

**Visitor** is a behavioral design pattern that lets you separate algorithms from the objects on which they operate.

![Diagram illustrating the Visitor design pattern, showing a Visitor interacting with various Element types (A, B, C) to perform operations.](/images/patterns/content/visitor/visitor.png?id=f36d100188340db7a18854ef7916f972)

## Problem

Imagine that your team develops an app which works with geographic information structured as one colossal graph. Each node of the graph may represent a complex entity such as a city, but also more granular things like industries, sightseeing areas, etc. The nodes are connected with others if there’s a road between the real objects that they represent. Under the hood, each node type is represented by its own class, while each specific node is an object.

![A map showing interconnected geographic entities (house, bank, factory, construction site) with an arrow pointing to an XML document, representing the problem of exporting graph data to XML.](/images/patterns/diagrams/visitor/problem1.png?id=e7076532da1e936f3519c63270da8454)

Exporting the graph into XML.

At some point, you got a task to implement exporting the graph into XML format. At first, the job seemed pretty straightforward. You planned to add an export method to each node class and then leverage recursion to go over each node of the graph, executing the export method. The solution was simple and elegant: thanks to polymorphism, you weren’t coupling the code which called the export method to concrete classes of nodes.

Unfortunately, the system architect refused to allow you to alter existing node classes. He said that the code was already in production and he didn’t want to risk breaking it because of a potential bug in your changes.

![Diagram depicting existing application classes (Industrial, Residential, Commercial, Construction) and a cloud representing XML Export Implementation, with crossed-out lines indicating the difficulty of adding export logic directly to existing classes.](/images/patterns/diagrams/visitor/problem2-en.png?id=f53c592d755890f5d027d7950b9967fc)

The XML export method had to be added into all node classes, which bore the risk of breaking the whole application if any bugs slipped through along with the change.

Besides, he questioned whether it makes sense to have the XML export code within the node classes. The primary job of these classes was to work with geodata. The XML export behavior would look alien there.

There was another reason for the refusal. It was highly likely that after this feature was implemented, someone from the marketing department would ask you to provide the ability to export into a different format, or request some other weird stuff. This would force you to change those precious and fragile classes again.

## Solution

The Visitor pattern suggests that you place the new behavior into a separate class called _visitor_, instead of trying to integrate it into existing classes. The original object that had to perform the behavior is now passed to one of the visitor’s methods as an argument, providing the method access to all necessary data contained within the object.

Now, what if that behavior can be executed over objects of different classes? For example, in our case with XML export, the actual implementation will probably be a little bit different across various node classes. Thus, the visitor class may define not one, but a set of methods, each of which could take arguments of different types, like this:

class ExportVisitor implements Visitor is
    method doForCity(City c) { ... }
    method doForIndustry(Industry f) { ... }
    method doForSightSeeing(SightSeeing ss) { ... }
    // ...

But how exactly would we call these methods, especially when dealing with the whole graph? These methods have different signatures, so we can’t use polymorphism. To pick a proper visitor method that’s able to process a given object, we’d need to check its class. Doesn’t this sound like a nightmare?

foreach (Node node in graph)
    if (node instanceof City)
        exportVisitor.doForCity((City) node)
    if (node instanceof Industry)
        exportVisitor.doForIndustry((Industry) node)
    // ...
}

You might ask, why don’t we use method overloading? That’s when you give all methods the same name, even if they support different sets of parameters. Unfortunately, even assuming that our programming language supports it at all (as Java and C# do), it won’t help us. Since the exact class of a node object is unknown in advance, the overloading mechanism won’t be able to determine the correct method to execute. It’ll default to the method that takes an object of the base `Node` class.

However, the Visitor pattern addresses this problem. It uses a technique called [Double Dispatch](/design-patterns/visitor-double-dispatch), which helps to execute the proper method on an object without cumbersome conditionals. Instead of letting the client select a proper version of the method to call, how about we delegate this choice to objects we’re passing to the visitor as an argument? Since the objects know their own classes, they’ll be able to pick a proper method on the visitor less awkwardly. They “accept” a visitor and tell it what visiting method should be executed.

// Client code
foreach (Node node in graph)
    node.accept(exportVisitor)

// City
class City is
    method accept(Visitor v) is
        v.doForCity(this)
    // ...

// Industry
class Industry is
    method accept(Visitor v) is
        v.doForIndustry(this)
    // ...

I confess. We had to change the node classes after all. But at least the change is trivial and it lets us add further behaviors without altering the code once again.

Now, if we extract a common interface for all visitors, all existing nodes can work with any visitor you introduce into the app. If you find yourself introducing a new behavior related to nodes, all you have to do is implement a new visitor class.

## Real-World Analogy

![A comic strip illustrating the real-world analogy of an insurance agent visiting different types of organizations (residential, bank, coffee shop) and offering specialized policies.](/images/patterns/content/visitor/visitor-comic-1.png?id=7ee4fa8800f7c4df4e1aa3b1aca2b7f1)

A good insurance agent is always ready to offer different policies to various types of organizations.

Imagine a seasoned insurance agent who’s eager to get new customers. He can visit every building in a neighborhood, trying to sell insurance to everyone he meets. Depending on the type of organization that occupies the building, he can offer specialized insurance policies:

*   If it’s a residential building, he sells medical insurance.
*   If it’s a bank, he sells theft insurance.
*   If it’s a coffee shop, he sells fire and flood insurance.

## Structure

![UML-like class diagram showing the structure of the Visitor design pattern, including Visitor and Element interfaces, Concrete Visitors, Concrete Elements, and the Client.](/images/patterns/diagrams/visitor/structure-en.png?id=34126311c4e0d5c9fbb970595d2f1777)![UML-like class diagram showing the structure of the Visitor design pattern, including Visitor and Element interfaces, Concrete Visitors, Concrete Elements, and the Client.](/images/patterns/diagrams/visitor/structure-en-indexed.png?id=5896a26c1b13364872b585022f29f29c)

1.  The **Visitor** interface declares a set of visiting methods that can take concrete elements of an object structure as arguments. These methods may have the same names if the program is written in a language that supports overloading, but the type of their parameters must be different.

2.  Each **Concrete Visitor** implements several versions of the same behaviors, tailored for different concrete element classes.

3.  The **Element** interface declares a method for “accepting” visitors. This method should have one parameter declared with the type of the visitor interface.

4.  Each **Concrete Element** must implement the acceptance method. The purpose of this method is to redirect the call to the proper visitor’s method corresponding to the current element class. Be aware that even if a base element class implements this method, all subclasses must still override this method in their own classes and call the appropriate method on the visitor object.

5.  The **Client** usually represents a collection or some other complex object (for example, a [Composite](/design-patterns/composite) tree). Usually, clients aren’t aware of all the concrete element classes because they work with objects from that collection via some abstract interface.

@media (min-width: 1200px) { .structure { margin: 0; width: auto; height: 600px; } .struct-image1 { left: 180px; top: 50px; } .struct-li1 { left: 0px; top: 10px; width: 180px; } .struct-li2 { left: 0px; top: 320px; width: 180px; } .struct-li3 { left: 700px; top: 10px; width: 220px; } .struct-li4 { left: 700px; top: 180px; width: 220px; } .struct-li5 { left: 600px; top: 490px; width: 320px; } }

## Pseudocode

In this example, the **Visitor** pattern adds XML export support to the class hierarchy of geometric shapes.

![Diagram illustrating the Visitor pattern applied to geometric shapes, showing different shape objects (Dot, Circle, Rectangle, CompoundShape) being processed by an XMLExportVisitor to generate XML output.](/images/patterns/diagrams/visitor/example.png?id=d66acd1b9096c47db17ab3bb82b54a59)

Exporting various types of objects into XML format via a visitor object.

// The element interface declares an `accept` method that takes
// the base visitor interface as an argument.
interface Shape is
    method move(x, y)
    method draw()
    method accept(v: Visitor)

// Each concrete element class must implement the `accept`
// method in such a way that it calls the visitor's method that
// corresponds to the element's class.
class Dot implements Shape is
    // ...

    // Note that we're calling `visitDot`, which matches the
    // current class name. This way we let the visitor know the
    // class of the element it works with.
    method accept(v: Visitor) is
        v.visitDot(this)

class Circle implements Shape is
    // ...
    method accept(v: Visitor) is
        v.visitCircle(this)

class Rectangle implements Shape is
    // ...
    method accept(v: Visitor) is
        v.visitRectangle(this)

class CompoundShape implements Shape is
    // ...
    method accept(v: Visitor) is
        v.visitCompoundShape(this)

// The Visitor interface declares a set of visiting methods that
// correspond to element classes. The signature of a visiting
// method lets the visitor identify the exact class of the
// element that it's dealing with.
interface Visitor is
    method visitDot(d: Dot)
    method visitCircle(c: Circle)
    method visitRectangle(r: Rectangle)
    method visitCompoundShape(cs: CompoundShape)

// Concrete visitors implement several versions of the same
// algorithm, which can work with all concrete element classes.
//
// You can experience the biggest benefit of the Visitor pattern
// when using it with a complex object structure such as a
// Composite tree. In this case, it might be helpful to store
// some intermediate state of the algorithm while executing the
// visitor's methods over various objects of the structure.
class XMLExportVisitor implements Visitor is
    method visitDot(d: Dot) is
        // Export the dot's ID and center coordinates.

    method visitCircle(c: Circle) is
        // Export the circle's ID, center coordinates and
        // radius.

    method visitRectangle(r: Rectangle) is
        // Export the rectangle's ID, left-top coordinates,
        // width and height.

    method visitCompoundShape(cs: CompoundShape) is
        // Export the shape's ID as well as the list of its
        // children's IDs.

// The client code can run visitor operations over any set of
// elements without figuring out their concrete classes. The
// accept operation directs a call to the appropriate operation
// in the visitor object.
class Application is
    field allShapes: array of Shapes

    method export() is
        exportVisitor \= new XMLExportVisitor()

        foreach (shape in allShapes) do
            shape.accept(exportVisitor)

If you wonder why we need the `accept` method in this example, my article [Visitor and Double Dispatch](/design-patterns/visitor-double-dispatch) addresses this question in detail.

## Applicability

Use the Visitor when you need to perform an operation on all elements of a complex object structure (for example, an object tree).

The Visitor pattern lets you execute an operation over a set of objects with different classes by having a visitor object implement several variants of the same operation, which correspond to all target classes.

Use the Visitor to clean up the business logic of auxiliary behaviors.

The pattern lets you make the primary classes of your app more focused on their main jobs by extracting all other behaviors into a set of visitor classes.

Use the pattern when a behavior makes sense only in some classes of a class hierarchy, but not in others.

You can extract this behavior into a separate visitor class and implement only those visiting methods that accept objects of relevant classes, leaving the rest empty.

## How to Implement

1.  Declare the visitor interface with a set of “visiting” methods, one per each concrete element class that exists in the program.

2.  Declare the element interface. If you’re working with an existing element class hierarchy, add the abstract “acceptance” method to the base class of the hierarchy. This method should accept a visitor object as an argument.

3.  Implement the acceptance methods in all concrete element classes. These methods must simply redirect the call to a visiting method on the incoming visitor object which matches the class of the current element.

4.  The element classes should only work with visitors via the visitor interface. Visitors, however, must be aware of all concrete element classes, referenced as parameter types of the visiting methods.

5.  For each behavior that can’t be implemented inside the element hierarchy, create a new concrete visitor class and implement all of the visiting methods.

    You might encounter a situation where the visitor will need access to some private members of the element class. In this case, you can either make these fields or methods public, violating the element’s encapsulation, or nest the visitor class in the element class. The latter is only possible if you’re lucky to work with a programming language that supports nested classes.

6.  The client must create visitor objects and pass them into elements via “acceptance” methods.

## Pros and Cons

*   _Open/Closed Principle_. You can introduce a new behavior that can work with objects of different classes without changing these classes.
*   _Single Responsibility Principle_. You can move multiple versions of the same behavior into the same class.
*   A visitor object can accumulate some useful information while working with various objects. This might be handy when you want to traverse some complex object structure, such as an object tree, and apply the visitor to each object of this structure.

*   You need to update all visitors each time a class gets added to or removed from the element hierarchy.
*   Visitors might lack the necessary access to the private fields and methods of the elements that they’re supposed to work with.

## Relations with Other Patterns

*   You can treat [Visitor](/design-patterns/visitor) as a powerful version of the [Command](/design-patterns/command) pattern. Its objects can execute operations over various objects of different classes.

*   You can use [Visitor](/design-patterns/visitor) to execute an operation over an entire [Composite](/design-patterns/composite) tree.

*   You can use [Visitor](/design-patterns/visitor) along with [Iterator](/design-patterns/iterator) to traverse a complex data structure and execute some operation over its elements, even if they all have different classes.

## Code Examples

[![Visitor in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58)](/design-patterns/visitor/csharp/example "Visitor in C#") [![Visitor in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/visitor/cpp/example "Visitor in C++") [![Visitor in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/visitor/go/example "Visitor in Go") [![Visitor in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/visitor/java/example "Visitor in Java") [![Visitor in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/visitor/php/example "Visitor in PHP") [![Visitor in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f)](/design-patterns/visitor/python/example "Visitor in Python") [![Visitor in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/visitor/ruby/example "Visitor in Ruby") [![Visitor in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87)](/design-patterns/visitor/rust/example "Visitor in Rust") [![Visitor in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/visitor/swift/example "Visitor in Swift") [![Visitor in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/visitor/typescript/example "Visitor in TypeScript")

## Extra Content

*   Puzzled why we can’t simply replace the Visitor pattern with method overloading? Read my article [Visitor and Double Dispatch](/design-patterns/visitor-double-dispatch) to learn about the nasty details.

![Promotional banner for the "Dive Into Design Patterns" eBook.](/images/patterns/banners/patterns-book-banner-3.png?id=7d445df13c80287beaab234b4f3b698c)

### Support our free website and own the eBook!

*   22 design patterns and 8 principles explained in depth.
*   409 well-structured, easy to read, jargon-free pages.
*   225 clear and helpful illustrations and diagrams.
*   An archive with code examples in 11 languages.
*   All devices supported: PDF/EPUB/MOBI/KFX formats.

[Learn more…](/design-patterns/book)

#### Read next

[Visitor and Double Dispatch](/design-patterns/visitor-double-dispatch)

#### Return

[Template Method](/design-patterns/template-method)

![Cover image for the "Dive Into Design Patterns" eBook.](/images/patterns/book/web-cover-en.png?id=328861769fd11617674e3b8a7e2dd9e7)

This article is a part of our eBook
**Dive Into Design Patterns**.

[Learn more…](/design-patterns/book)