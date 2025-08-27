```yaml
---
title: Abstract Factory
source: https://refactoring.guru/design-patterns/abstract-factory
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:22:16.812Z
domain: refactoring.guru
author: Unknown
category: general
technologies: [Windows, macOS]
programming_languages: [C#, C++, Go, Java, PHP, Python, Ruby, Rust, Swift, TypeScript, Pseudocode, JavaScript]
tags: [design-patterns, creational-patterns, abstract-factory, software-design, object-oriented-programming, factory-pattern, extensibility, decoupling, cross-platform, ui]
key_concepts: [abstract-factory-pattern, creational-patterns, product-family, abstract-product, concrete-product, abstract-factory-interface, concrete-factory, client-decoupling]
code_examples: false
difficulty_level: intermediate
summary: |
  The Abstract Factory is a creational design pattern that enables the production of families of related objects without specifying their concrete classes. It addresses the problem of ensuring product compatibility within a family while allowing for easy addition of new product variants without modifying existing client code. The pattern achieves this by defining abstract interfaces for products and factories, with concrete factories responsible for creating specific product variants. This approach promotes loose coupling between the client code and concrete product implementations, adhering to principles like Single Responsibility and Open/Closed.
---
```

# Abstract Factory

[](/)/ [Design Patterns](/design-patterns) / [Creational Patterns](/design-patterns/creational-patterns)

# Abstract Factory

## Intent

**Abstract Factory** is a creational design pattern that lets you produce families of related objects without specifying their concrete classes.

![Abstract Factory pattern: An illustrative diagram showing a main "Abstract Factory" building producing goods that are then distributed to "Spheric Factory" and "Pyramidal Factory", which then produce specific shaped products. This visually represents the abstract factory producing different concrete factories.](/images/patterns/content/abstract-factory/abstract-factory-en.png?id=d0210ee255712a245fead94a3fafabe0)

## Problem

Imagine that you’re creating a furniture shop simulator. Your code consists of classes that represent:

1.  A family of related products, say: `Chair` + `Sofa` + `CoffeeTable`.
    
2.  Several variants of this family. For example, products `Chair` + `Sofa` + `CoffeeTable` are available in these variants: `Modern`, `Victorian`, `ArtDeco`.
    

![Product families and their variants: A grid showing "Product families and their variants". Rows are "Art Deco", "Victorian", "Modern". Columns are "Chair", "Sofa", "Coffee Table". Each cell shows an icon of the corresponding furniture style and type. This illustrates the problem of needing to create compatible product families.](/images/patterns/diagrams/abstract-factory/problem-en.png?id=e38c307511e684828be898de02d6c268)

Product families and their variants.

You need a way to create individual furniture objects so that they match other objects of the same family. Customers get quite mad when they receive non-matching furniture.

![A Modern-style sofa doesn’t match Victorian-style chairs: A comic strip. Left panel: A person on the phone, surrounded by Victorian-style chairs, says "Listen, I ordered some chairs last week, but I guess I need a sofa too...". Right panel: The person looks confused at a Modern-style sofa placed next to Victorian-style chairs, thinking "Hmm... Something does not look right." This humorously depicts the problem of non-matching product variants.](/images/patterns/content/abstract-factory/abstract-factory-comic-1-en.png?id=f4012920c5034122eedbb0c9fec0cdb3)

A Modern-style sofa doesn’t match Victorian-style chairs.

Also, you don’t want to change existing code when adding new products or families of products to the program. Furniture vendors update their catalogs very often, and you wouldn’t want to change the core code each time it happens.

## Solution

The first thing the Abstract Factory pattern suggests is to explicitly declare interfaces for each distinct product of the product family (e.g., chair, sofa or coffee table). Then you can make all variants of products follow those interfaces. For example, all chair variants can implement the `Chair` interface; all coffee table variants can implement the `CoffeeTable` interface, and so on.

![The Chairs class hierarchy: A UML-like class diagram showing an «interface» Chair at the top, with VictorianChair and ModernChair inheriting from it. Icons of a Victorian chair and a Modern chair are next to their respective classes. This illustrates how all variants of the same object must be moved to a single class hierarchy.](/images/patterns/diagrams/abstract-factory/solution1.png?id=71f2018d8bb443b9cce90d57110675e3)

All variants of the same object must be moved to a single class hierarchy.

The next move is to declare the _Abstract Factory_—an interface with a list of creation methods for all products that are part of the product family (for example, `createChair`, `createSofa` and `createCoffeeTable`). These methods must return **abstract** product types represented by the interfaces we extracted previously: `Chair`, `Sofa`, `CoffeeTable` and so on.

![The _Factories_ class hierarchy: A UML-like class diagram showing an «interface» FurnitureFactory at the top, with VictorianFurnitureFactory and ModernFurnitureFactory inheriting from it. Each concrete factory shows createChair(), createCoffeeTable(), createSofa() methods, returning abstract types. Icons of Victorian and Modern furniture sets are shown next to their respective factories. This illustrates the _Factories_ class hierarchy, where each concrete factory corresponds to a specific product variant.](/images/patterns/diagrams/abstract-factory/solution2.png?id=53975d6e4714c6f942633a879f7ac571)

Each concrete factory corresponds to a specific product variant.

Now, how about the product variants? For each variant of a product family, we create a separate factory class based on the `AbstractFactory` interface. A factory is a class that returns products of a particular kind. For example, the `ModernFurnitureFactory` can only create `ModernChair`, `ModernSofa` and `ModernCoffeeTable` objects.

The client code has to work with both factories and products via their respective abstract interfaces. This lets you change the type of a factory that you pass to the client code, as well as the product variant that the client code receives, without breaking the actual client code.

![The client shouldn’t care about the concrete class of the factory it works with: A comic strip. A person is holding a generic "Factory" object. The factory has a dial that can be set to "Modern" or "Victorian". The person says, "The client shouldn't care about the concrete class of the factory it works with." This illustrates the client's decoupling from concrete factory implementations.](/images/patterns/content/abstract-factory/abstract-factory-comic-2-en.png?id=fbce1a263acfefc76074fd20fae7b8c3)

The client shouldn’t care about the concrete class of the factory it works with.

Say the client wants a factory to produce a chair. The client doesn’t have to be aware of the factory’s class, nor does it matter what kind of chair it gets. Whether it’s a Modern model or a Victorian-style chair, the client must treat all chairs in the same manner, using the abstract `Chair` interface. With this approach, the only thing that the client knows about the chair is that it implements the `sitOn` method in some way. Also, whichever variant of the chair is returned, it’ll always match the type of sofa or coffee table produced by the same factory object.

There’s one more thing left to clarify: if the client is only exposed to the abstract interfaces, what creates the actual factory objects? Usually, the application creates a concrete factory object at the initialization stage. Just before that, the app must select the factory type depending on the configuration or the environment settings.

## Structure

![Abstract Factory design pattern structure: A UML-like class diagram showing the full structure of the Abstract Factory pattern with Abstract Product, Concrete Product, Abstract Factory, Concrete Factory, and Client components.](/images/patterns/diagrams/abstract-factory/structure.png?id=a3112cdd98765406af94595a3c5e7762)![Abstract Factory design pattern structure (indexed): A UML-like class diagram showing the full structure of the Abstract Factory pattern with Abstract Product, Concrete Product, Abstract Factory, Concrete Factory, and Client components. This version highlights each component with a number corresponding to a textual description.](/images/patterns/diagrams/abstract-factory/structure-indexed.png?id=6ae1c99cbd90cf58753c633624fb1a04)

1.  **Abstract Products** declare interfaces for a set of distinct but related products which make up a product family.
    
2.  **Concrete Products** are various implementations of abstract products, grouped by variants. Each abstract product (chair/sofa) must be implemented in all given variants (Victorian/Modern).
    
3.  The **Abstract Factory** interface declares a set of methods for creating each of the abstract products.
    
4.  **Concrete Factories** implement creation methods of the abstract factory. Each concrete factory corresponds to a specific variant of products and creates only those product variants.
    
5.  Although concrete factories instantiate concrete products, signatures of their creation methods must return corresponding _abstract_ products. This way the client code that uses a factory doesn’t get coupled to the specific variant of the product it gets from a factory. The **Client** can work with any concrete factory/product variant, as long as it communicates with their objects via abstract interfaces.
    

## Pseudocode

This example illustrates how the **Abstract Factory** pattern can be used for creating cross-platform UI elements without coupling the client code to concrete UI classes, while keeping all created elements consistent with a selected operating system.

![The class diagram for the Abstract Factory pattern example: A UML-like class diagram for the cross-platform UI example. It shows GUIFactory interface implemented by WinFactory and MacFactory. It also shows Button and Checkbox interfaces implemented by WinButton, MacButton, WinCheckbox, MacCheckbox. An Application class uses GUIFactory, Button, and Checkbox. This illustrates the cross-platform UI classes example.](/images/patterns/diagrams/abstract-factory/example.png?id=5928a61d18bf00b047463471c599100a)

The cross-platform UI classes example.

The same UI elements in a cross-platform application are expected to behave similarly, but look a little bit different under different operating systems. Moreover, it’s your job to make sure that the UI elements match the style of the current operating system. You wouldn’t want your program to render macOS controls when it’s executed in Windows.

The Abstract Factory interface declares a set of creation methods that the client code can use to produce different types of UI elements. Concrete factories correspond to specific operating systems and create the UI elements that match that particular OS.

It works like this: when an application launches, it checks the type of the current operating system. The app uses this information to create a factory object from a class that matches the operating system. The rest of the code uses this factory to create UI elements. This prevents the wrong elements from being created.

With this approach, the client code doesn’t depend on concrete classes of factories and UI elements as long as it works with these objects via their abstract interfaces. This also lets the client code support other factories or UI elements that you might add in the future.

As a result, you don’t need to modify the client code each time you add a new variation of UI elements to your app. You just have to create a new factory class that produces these elements and slightly modify the app’s initialization code so it selects that class when appropriate.

```pseudocode
// The abstract factory interface declares a set of methods that
// return different abstract products. These products are called
// a family and are related by a high-level theme or concept.
// Products of one family are usually able to collaborate among
// themselves. A family of products may have several variants,
// but the products of one variant are incompatible with the
// products of another variant.
interface GUIFactory is
    method createButton():Button
    method createCheckbox():Checkbox


// Concrete factories produce a family of products that belong
// to a single variant. The factory guarantees that the
// resulting products are compatible. Signatures of the concrete
// factory's methods return an abstract product, while inside
// the method a concrete product is instantiated.
class WinFactory implements GUIFactory is
    method createButton():Button is
        return new WinButton()
    method createCheckbox():Checkbox is
        return new WinCheckbox()

// Each concrete factory has a corresponding product variant.
class MacFactory implements GUIFactory is
    method createButton():Button is
        return new MacButton()
    method createCheckbox():Checkbox is
        return new MacCheckbox()


// Each distinct product of a product family should have a base
// interface. All variants of the product must implement this
// interface.
interface Button is
    method paint()

// Concrete products are created by corresponding concrete
// factories.
class WinButton implements Button is
    method paint() is
        // Render a button in Windows style.

class MacButton implements Button is
    method paint() is
        // Render a button in macOS style.

// Here's the base interface of another product. All products
// can interact with each other, but proper interaction is
// possible only between products of the same concrete variant.
interface Checkbox is
    method paint()

class WinCheckbox implements Checkbox is
    method paint() is
        // Render a checkbox in Windows style.

class MacCheckbox implements Checkbox is
    method paint() is
        // Render a checkbox in macOS style.


// The client code works with factories and products only
// through abstract types: GUIFactory, Button and Checkbox. This
// lets you pass any factory or product subclass to the client
// code without breaking it.
class Application is
    private field factory: GUIFactory
    private field button: Button
    constructor Application(factory: GUIFactory) is
        this.factory = factory
    method createUI() is
        this.button = factory.createButton()
    method paint() is
        button.paint()


// The application picks the factory type depending on the
// current configuration or environment settings and creates it
// at runtime (usually at the initialization stage).
class ApplicationConfigurator is
    method main() is
        config = readApplicationConfigFile()

        if (config.OS == "Windows") then
            factory = new WinFactory()
        else if (config.OS == "Mac") then
            factory = new MacFactory()
        else
            throw new Exception("Error! Unknown operating system.")

        Application app = new Application(factory)
```

## Applicability

Use the Abstract Factory when your code needs to work with various families of related products, but you don’t want it to depend on the concrete classes of those products—they might be unknown beforehand or you simply want to allow for future extensibility.

The Abstract Factory provides you with an interface for creating objects from each class of the product family. As long as your code creates objects via this interface, you don’t have to worry about creating the wrong variant of a product which doesn’t match the products already created by your app.

Consider implementing the Abstract Factory when you have a class with a set of [Factory Methods](/design-patterns/factory-method) that blur its primary responsibility.

In a well-designed program _each class is responsible only for one thing_. When a class deals with multiple product types, it may be worth extracting its factory methods into a stand-alone factory class or a full-blown Abstract Factory implementation.

## How to Implement

1.  Map out a matrix of distinct product types versus variants of these products.
    
2.  Declare abstract product interfaces for all product types. Then make all concrete product classes implement these interfaces.
    
3.  Declare the abstract factory interface with a set of creation methods for all abstract products.
    
4.  Implement a set of concrete factory classes, one for each product variant.
    
5.  Create factory initialization code somewhere in the app. It should instantiate one of the concrete factory classes, depending on the application configuration or the current environment. Pass this factory object to all classes that construct products.
    
6.  Scan through the code and find all direct calls to product constructors. Replace them with calls to the appropriate creation method on the factory object.
    

## Pros and Cons

*   You can be sure that the products you’re getting from a factory are compatible with each other.
*   You avoid tight coupling between concrete products and client code.
*   _Single Responsibility Principle_. You can extract the product creation code into one place, making the code easier to support.
*   _Open/Closed Principle_. You can introduce new variants of products without breaking existing client code.

*   The code may become more complicated than it should be, since a lot of new interfaces and classes are introduced along with the pattern.

## Relations with Other Patterns

*   Many designs start by using [Factory Method](/design-patterns/factory-method) (less complicated and more customizable via subclasses) and evolve toward [Abstract Factory](/design-patterns/abstract-factory), [Prototype](/design-patterns/prototype), or [Builder](/design-patterns/builder) (more flexible, but more complicated).
    
*   [Builder](/design-patterns/builder) focuses on constructing complex objects step by step. [Abstract Factory](/design-patterns/abstract-factory) specializes in creating families of related objects. _Abstract Factory_ returns the product immediately, whereas _Builder_ lets you run some additional construction steps before fetching the product.
    
*   [Abstract Factory](/design-patterns/abstract-factory) classes are often based on a set of [Factory Methods](/design-patterns/factory-method), but you can also use [Prototype](/design-patterns/prototype) to compose the methods on these classes.
    
*   [Abstract Factory](/design-patterns/abstract-factory) can serve as an alternative to [Facade](/design-patterns/facade) when you only want to hide the way the subsystem objects are created from the client code.
    
*   You can use [Abstract Factory](/design-patterns/abstract-factory) along with [Bridge](/design-patterns/bridge). This pairing is useful when some abstractions defined by _Bridge_ can only work with specific implementations. In this case, _Abstract Factory_ can encapsulate these relations and hide the complexity from the client code.
    
*   [Abstract Factories](/design-patterns/abstract-factory), [Builders](/design-patterns/builder) and [Prototypes](/design-patterns/prototype) can all be implemented as [Singletons](/design-patterns/singleton).
    

## Code Examples

[![Abstract Factory in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58)](/design-patterns/abstract-factory/csharp/example "Abstract Factory in C#") [![Abstract Factory in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/abstract-factory/cpp/example "Abstract Factory in C++") [![Abstract Factory in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/abstract-factory/go/example "Abstract Factory in Go") [![Abstract Factory in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/abstract-factory/java/example "Abstract Factory in Java") [![Abstract Factory in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/abstract-factory/php/example "Abstract Factory in PHP") [![Abstract Factory in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f)](/design-patterns/abstract-factory/python/example "Abstract Factory in Python") [![Abstract Factory in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/abstract-factory/ruby/example "Abstract Factory in Ruby") [![Abstract Factory in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87)](/design-patterns/abstract-factory/rust/example "Abstract Factory in Rust") [![Abstract Factory in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/abstract-factory/swift/example "Abstract Factory in Swift") [![Abstract Factory in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/abstract-factory/typescript/example "Abstract Factory in TypeScript")

## Extra Content

*   Read our [Factory Comparison](/design-patterns/factory-comparison) to learn more about the differences between various factory patterns and concepts.

[![](/images/patterns/banners/patterns-book-banner-3.png?id=7d445df13c80287beaab234b4f3b698c)](/design-patterns/book)

### Support our free website and own the eBook!

*   22 design patterns and 8 principles explained in depth.
*   409 well-structured, easy to read, jargon-free pages.
*   225 clear and helpful illustrations and diagrams.
*   An archive with code examples in 11 languages.
*   All devices supported: PDF/EPUB/MOBI/KFX formats.

[Learn more…](/design-patterns/book)

#### Read next

[Factory Comparison](/design-patterns/factory-comparison) 

#### Return

 [Factory Method](/design-patterns/factory-method)

[![](/images/patterns/book/web-cover-en.png?id=328861769fd11617674e3b8a7e2dd9e7)](/design-patterns/book)

This article is a part of our eBook  
**Dive Into Design Patterns**.

[Learn more…](/design-patterns/book)