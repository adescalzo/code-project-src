```yaml
---
title: Bridge
source: https://refactoring.guru/design-patterns/bridge
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:21:17.533Z
domain: refactoring.guru
author: Unknown
category: general
technologies: [Windows, Linux, macOS, GUI, API]
programming_languages: [C#, C++, Go, Java, PHP, Python, Ruby, Rust, Swift, TypeScript, Pseudocode]
tags: [design-pattern, structural-pattern, object-oriented-programming, abstraction, implementation, code-organization, software-architecture, flexibility, inheritance, composition]
key_concepts: [Bridge Pattern, Abstraction, Implementation, Object Composition, Inheritance vs. Composition, Open/Closed Principle, Single Responsibility Principle, Gang of Four]
code_examples: false
difficulty_level: intermediate
summary: |
  [The Bridge pattern is a structural design pattern that separates an abstraction from its implementation, allowing them to vary independently. It solves the problem of exponential class growth when extending a class in multiple orthogonal dimensions, such as shapes and colors, by switching from inheritance to object composition. The pattern defines two separate hierarchies: Abstraction (high-level control) and Implementation (low-level platform-specific details), connected by a reference. This approach promotes flexibility, adheres to the Open/Closed and Single Responsibility Principles, and simplifies code maintenance by enabling independent development of each hierarchy. The article provides a detailed explanation with examples, pseudocode, and discusses its applicability and relations to other design patterns.]
---
```

# Bridge

[](/)/ [Design Patterns](/design-patterns) / [Structural Patterns](/design-patterns/structural-patterns)

# Bridge

// Shorten examples titles for users. var h1 = document.getElementsByTagName("H1")\[0\]; if (h1.offsetHeight > 160) { h1.className += ' smaller'; } // Small beautification for pattern examples. var title = h1.innerHTML; title = title.replace(/^(Java|C\\+\\+|C#|PHP|Python|Ruby|Delphi): (.\*)$/, '<strong>$1:</strong> $2'); h1.innerHTML = title;

## Intent

**Bridge** is a structural design pattern that lets you split a large class or a set of closely related classes into two separate hierarchies—abstraction and implementation—which can be developed independently of each other.

![Conceptual diagram of the Bridge design pattern, showing a bridge connecting two sides: one with various display devices (TVs, monitors) and the other with different remote controls. The bridge itself is composed of binary code, symbolizing the connection between implementation (devices) and abstraction (remotes).](/images/patterns/content/bridge/bridge.png?id=bd543d4fb32e11647767301581a5ad54)

## Problem

_Abstraction?_ _Implementation?_ Sound scary? Stay calm and let’s consider a simple example.

Say you have a geometric `Shape` class with a pair of subclasses: `Circle` and `Square`. You want to extend this class hierarchy to incorporate colors, so you plan to create `Red` and `Blue` shape subclasses. However, since you already have two subclasses, you’ll need to create four class combinations such as `BlueCircle` and `RedSquare`.

![Diagram illustrating the problem addressed by the Bridge pattern. It shows a `Shape` class, which, when combined with `Circle` and `Square` shapes and `Red` and `Blue` colors, leads to an explosion of specific subclasses like `RedCircle`, `RedSquare`, `BlueCircle`, and `BlueSquare`.](/images/patterns/diagrams/bridge/problem-en.png?id=81f8ed6e6f5d673e15203b22a7a3c502)

Number of class combinations grows in geometric progression.

Adding new shape types and colors to the hierarchy will grow it exponentially. For example, to add a triangle shape you’d need to introduce two subclasses, one for each color. And after that, adding a new color would require creating three subclasses, one for each shape type. The further we go, the worse it becomes.

## Solution

This problem occurs because we’re trying to extend the shape classes in two independent dimensions: by form and by color. That’s a very common issue with class inheritance.

The Bridge pattern attempts to solve this problem by switching from inheritance to the object composition. What this means is that you extract one of the dimensions into a separate class hierarchy, so that the original classes will reference an object of the new hierarchy, instead of having all of its state and behaviors within one class.

![Diagram showing the solution suggested by the Bridge pattern. Instead of a single exploding hierarchy, it shows two separate hierarchies: `Shape` (with `Circle` and `Square` subclasses) and `Color` (with `Red` and `Blue` subclasses). The `Shape` class now "contains" a `Color` object, establishing a composition relationship.](/images/patterns/diagrams/bridge/solution-en.png?id=b72caae18c400d6088072f2f3adda7cd)

You can prevent the explosion of a class hierarchy by transforming it into several related hierarchies.

Following this approach, we can extract the color-related code into its own class with two subclasses: `Red` and `Blue`. The `Shape` class then gets a reference field pointing to one of the color objects. Now the shape can delegate any color-related work to the linked color object. That reference will act as a bridge between the `Shape` and `Color` classes. From now on, adding new colors won’t require changing the shape hierarchy, and vice versa.

#### Abstraction and Implementation

The GoF book “Gang of Four” is a nickname given to the four authors of the original book about design patterns: _Design Patterns: Elements of Reusable Object-Oriented Software_ [https://refactoring.guru/gof-book](https://refactoring.guru/gof-book). introduces the terms _Abstraction_ and _Implementation_ as part of the Bridge definition. In my opinion, the terms sound too academic and make the pattern seem more complicated than it really is. Having read the simple example with shapes and colors, let’s decipher the meaning behind the GoF book’s scary words.

_Abstraction_ (also called _interface_) is a high-level control layer for some entity. This layer isn’t supposed to do any real work on its own. It should delegate the work to the _implementation_ layer (also called _platform_).

Note that we’re not talking about _interfaces_ or _abstract classes_ from your programming language. These aren’t the same things.

When talking about real applications, the abstraction can be represented by a graphical user interface (GUI), and the implementation could be the underlying operating system code (API) which the GUI layer calls in response to user interactions.

Generally speaking, you can extend such an app in two independent directions:

*   Have several different GUIs (for instance, tailored for regular customers or admins).
*   Support several different APIs (for example, to be able to launch the app under Windows, Linux, and macOS).

In a worst-case scenario, this app might look like a giant spaghetti bowl, where hundreds of conditionals connect different types of GUI with various APIs all over the code.

![Two-panel illustration comparing monolithic vs. modular code. The left panel shows a large, solid block labeled "CHANGES" with many arrows pointing to it from all directions, symbolizing how difficult it is to manage changes in a tightly coupled, monolithic codebase. The right panel shows two separate towers, "ABSTRACTION" and "IMPLEMENTATION", connected by a "BRIDGE". Changes flow into each tower independently, illustrating how modular code with a Bridge pattern makes managing changes much easier.](/images/patterns/content/bridge/bridge-3-en.png?id=15b8262114938f7bef6602af33f0a62e)

Making even a simple change to a monolithic codebase is pretty hard because you must understand the _entire thing_ very well. Making changes to smaller, well-defined modules is much easier.

You can bring order to this chaos by extracting the code related to specific interface-platform combinations into separate classes. However, soon you’ll discover that there are _lots_ of these classes. The class hierarchy will grow exponentially because adding a new GUI or supporting a different API would require creating more and more classes.

Let’s try to solve this issue with the Bridge pattern. It suggests that we divide the classes into two hierarchies:

*   Abstraction: the GUI layer of the app.
*   Implementation: the operating systems’ APIs.

![Diagram illustrating a cross-platform application architecture using the Bridge pattern. "YOUR APP" (representing the GUI/Abstraction) is connected via a "BRIDGE" to a "MULTI-PLATFORM FRAMEWORK" (representing the Implementation). This framework then interacts with different operating systems like macOS, Windows, and Linux, showing how the GUI can work across various platforms.](/images/patterns/content/bridge/bridge-2-en.png?id=5c5aef57ca6aa8c3c97fd8922fc8bb58)

One of the ways to structure a cross-platform application.

The abstraction object controls the appearance of the app, delegating the actual work to the linked implementation object. Different implementations are interchangeable as long as they follow a common interface, enabling the same GUI to work under Windows and Linux.

As a result, you can change the GUI classes without touching the API-related classes. Moreover, adding support for another operating system only requires creating a subclass in the implementation hierarchy.

## Structure

![UML-like diagram illustrating the structure of the Bridge design pattern. It shows an `Abstraction` class with a reference to an `Implementation` interface. `RefinedAbstraction` extends `Abstraction`. `ConcreteImplementationA` and `ConcreteImplementationB` implement the `Implementation` interface. A `Client` interacts with the `Abstraction`.](/images/patterns/diagrams/bridge/structure-en.png?id=827afa4b40008dc29d26fe0f4d41b9cc)![UML-like diagram illustrating the structure of the Bridge design pattern with numbered labels. It shows an `Abstraction` class with a reference to an `Implementation` interface. `RefinedAbstraction` extends `Abstraction`. `ConcreteImplementationA` and `ConcreteImplementationB` implement the `Implementation` interface. A `Client` interacts with the `Abstraction`. The numbers correspond to the descriptions below the image.](/images/patterns/diagrams/bridge/structure-en-indexed.png?id=0461ee029a15b02e03e9735f2ca576d4)

1.  The **Abstraction** provides high-level control logic. It relies on the implementation object to do the actual low-level work.
    
2.  The **Implementation** declares the interface that’s common for all concrete implementations. An abstraction can only communicate with an implementation object via methods that are declared here.
    
    The abstraction may list the same methods as the implementation, but usually the abstraction declares some complex behaviors that rely on a wide variety of primitive operations declared by the implementation.
    
3.  **Concrete Implementations** contain platform-specific code.
    
4.  **Refined Abstractions** provide variants of control logic. Like their parent, they work with different implementations via the general implementation interface.
    
5.  Usually, the **Client** is only interested in working with the abstraction. However, it’s the client’s job to link the abstraction object with one of the implementation objects.
    

@media (min-width: 1200px) { .structure { margin: 0; width: auto; height: 620px; } .struct-image1 { left: 60px; top: 130px; } .struct-li1 { left: 0px; top: 90px; width: 170px; } .struct-li2 { left: 640px; top: 50px; width: 190px; } .struct-li3 { left: 420px; top: 480px; width: 220px; } .struct-li4 { left: 110px; top: 520px; width: 280px; } .struct-li5 { left: 200px; top: 30px; width: 390px; } }

## Pseudocode

This example illustrates how the **Bridge** pattern can help divide the monolithic code of an app that manages devices and their remote controls. The `Device` classes act as the implementation, whereas the `Remote`s act as the abstraction.

![UML-like diagram illustrating the structure of the Bridge pattern example. It shows `RemoteControl` (Abstraction) and `AdvancedRemoteControl` (Refined Abstraction) classes. `RemoteControl` has a protected field `device` of type `Device`. `Device` is an interface (Implementation) which is implemented by `Tv` and `Radio` (Concrete Implementations). This visually represents how remotes control devices through a common interface.](/images/patterns/diagrams/bridge/example-en.png?id=89c406a189c45885004d7fa094f616b1)

The original class hierarchy is divided into two parts: devices and remote controls.

The base remote control class declares a reference field that links it with a device object. All remotes work with the devices via the general device interface, which lets the same remote support multiple device types.

You can develop the remote control classes independently from the device classes. All that’s needed is to create a new remote subclass. For example, a basic remote control might only have two buttons, but you could extend it with additional features, such as an extra battery or a touchscreen.

The client code links the desired type of remote control with a specific device object via the remote’s constructor.

// The "abstraction" defines the interface for the "control"
// part of the two class hierarchies. It maintains a reference
// to an object of the "implementation" hierarchy and delegates
// all of the real work to this object.
class RemoteControl is
    protected field device: Device
    constructor RemoteControl(device: Device) is
        this.device = device
    method togglePower() is
        if (device.isEnabled()) then
            device.disable()
        else
            device.enable()
    method volumeDown() is
        device.setVolume(device.getVolume() - 10)
    method volumeUp() is
        device.setVolume(device.getVolume() + 10)
    method channelDown() is
        device.setChannel(device.getChannel() - 1)
    method channelUp() is
        device.setChannel(device.getChannel() + 1)


// You can extend classes from the abstraction hierarchy
// independently from device classes.
class AdvancedRemoteControl extends RemoteControl is
    method mute() is
        device.setVolume(0)


// The "implementation" interface declares methods common to all
// concrete implementation classes. It doesn't have to match the
// abstraction's interface. In fact, the two interfaces can be
// entirely different. Typically the implementation interface
// provides only primitive operations, while the abstraction
// defines higher-level operations based on those primitives.
interface Device is
    method isEnabled()
    method enable()
    method disable()
    method getVolume()
    method setVolume(percent)
    method getChannel()
    method setChannel(channel)


// All devices follow the same interface.
class Tv implements Device is
    // ...

class Radio implements Device is
    // ...


// Somewhere in client code.
tv = new Tv()
remote = new RemoteControl(tv)
remote.togglePower()

radio = new Radio()
remote = new AdvancedRemoteControl(radio)

## Applicability

Use the Bridge pattern when you want to divide and organize a monolithic class that has several variants of some functionality (for example, if the class can work with various database servers).

The bigger a class becomes, the harder it is to figure out how it works, and the longer it takes to make a change. The changes made to one of the variations of functionality may require making changes across the whole class, which often results in making errors or not addressing some critical side effects.

The Bridge pattern lets you split the monolithic class into several class hierarchies. After this, you can change the classes in each hierarchy independently of the classes in the others. This approach simplifies code maintenance and minimizes the risk of breaking existing code.

Use the pattern when you need to extend a class in several orthogonal (independent) dimensions.

The Bridge suggests that you extract a separate class hierarchy for each of the dimensions. The original class delegates the related work to the objects belonging to those hierarchies instead of doing everything on its own.

Use the Bridge if you need to be able to switch implementations at runtime.

Although it’s optional, the Bridge pattern lets you replace the implementation object inside the abstraction. It’s as easy as assigning a new value to a field.

By the way, this last item is the main reason why so many people confuse the Bridge with the [Strategy](/design-patterns/strategy) pattern. Remember that a pattern is more than just a certain way to structure your classes. It may also communicate intent and a problem being addressed.

## How to Implement

1.  Identify the orthogonal dimensions in your classes. These independent concepts could be: abstraction/platform, domain/infrastructure, front-end/back-end, or interface/implementation.
    
2.  See what operations the client needs and define them in the base abstraction class.
    
3.  Determine the operations available on all platforms. Declare the ones that the abstraction needs in the general implementation interface.
    
4.  For all platforms in your domain create concrete implementation classes, but make sure they all follow the implementation interface.
    
5.  Inside the abstraction class, add a reference field for the implementation type. The abstraction delegates most of the work to the implementation object that’s referenced in that field.
    
6.  If you have several variants of high-level logic, create refined abstractions for each variant by extending the base abstraction class.
    
7.  The client code should pass an implementation object to the abstraction’s constructor to associate one with the other. After that, the client can forget about the implementation and work only with the abstraction object.
    

## Pros and Cons

*   You can create platform-independent classes and apps.
*   The client code works with high-level abstractions. It isn’t exposed to the platform details.
*   _Open/Closed Principle_. You can introduce new abstractions and implementations independently from each other.
*   _Single Responsibility Principle_. You can focus on high-level logic in the abstraction and on platform details in the implementation.

*   You might make the code more complicated by applying the pattern to a highly cohesive class.

## Relations with Other Patterns

*   [Bridge](/design-patterns/bridge) is usually designed up-front, letting you develop parts of an application independently of each other. On the other hand, [Adapter](/design-patterns/adapter) is commonly used with an existing app to make some otherwise-incompatible classes work together nicely.
    
*   [Bridge](/design-patterns/bridge), [State](/design-patterns/state), [Strategy](/design-patterns/strategy) (and to some degree [Adapter](/design-patterns/adapter)) have very similar structures. Indeed, all of these patterns are based on composition, which is delegating work to other objects. However, they all solve different problems. A pattern isn’t just a recipe for structuring your code in a specific way. It can also communicate to other developers the problem the pattern solves.
    
*   You can use [Abstract Factory](/design-patterns/abstract-factory) along with [Bridge](/design-patterns/bridge). This pairing is useful when some abstractions defined by _Bridge_ can only work with specific implementations. In this case, _Abstract Factory_ can encapsulate these relations and hide the complexity from the client code.
    
*   You can combine [Builder](/design-patterns/builder) with [Bridge](/design-patterns/bridge): the director class plays the role of the abstraction, while different builders act as implementations.
    

## Code Examples

[![Bridge in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58)](/design-patterns/bridge/csharp/example "Bridge in C#") [![Bridge in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/bridge/cpp/example "Bridge in C++") [![Bridge in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/bridge/go/example "Bridge in Go") [![Bridge in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/bridge/java/example "Bridge in Java") [![Bridge in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/bridge/php/example "Bridge in PHP") [![Bridge in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f)](/design-patterns/bridge/python/example "Bridge in Python") [![Bridge in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/bridge/ruby/example "Bridge in Ruby") [![Bridge in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87)](/design-patterns/bridge/rust/example "Bridge in Rust") [![Bridge in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/bridge/swift/example "Bridge in Swift") [![Bridge in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/bridge/typescript/example "Bridge in TypeScript")

[![](/images/patterns/banners/patterns-book-banner-3.png?id=7d445df13c80287beaab234b4f3b698c)](/design-patterns/book)

### Support our free website and own the eBook!

*   22 design patterns and 8 principles explained in depth.
*   409 well-structured, easy to read, jargon-free pages.
*   225 clear and helpful illustrations and diagrams.
*   An archive with code examples in 11 languages.
*   All devices supported: PDF/EPUB/MOBI/KFX formats.

[Learn more…](/design-patterns/book)

#### Read next

[Composite](/design-patterns/composite) 

#### Return

 [Adapter](/design-patterns/adapter)

[![](/images/patterns/book/web-cover-en.png?id=328861769fd11617674e3b8a7e2dd9e7)](/design-patterns/book)

This article is a part of our eBook  
**Dive Into Design Patterns**.

[Learn more…](/design-patterns/book)