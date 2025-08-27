```yaml
---
title: Observer
source: https://refactoring.guru/design-patterns/observer
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:16:21.738Z
domain: refactoring.guru
author: Unknown
category: backend
technologies: []
programming_languages: [Pseudocode, C#, C++, Go, Java, PHP, Python, Ruby, Rust, Swift, TypeScript]
tags: [design-pattern, behavioral-pattern, observer, publisher-subscriber, event-driven, loose-coupling, notification, software-design, object-oriented-programming, event-handling]
key_concepts: [Observer Pattern, Publisher-Subscriber Model, Event-driven architecture, Loose Coupling, Open/Closed Principle, Notification Mechanism, Subscription Mechanism, Event Dispatcher]
code_examples: false
difficulty_level: intermediate
summary: |
  The Observer design pattern is a behavioral pattern that establishes a one-to-many dependency between objects, allowing multiple "subscriber" objects to be notified automatically when the state of a "publisher" object changes. It solves the problem of tight coupling or inefficient polling by introducing a subscription mechanism where subscribers can dynamically register or unregister with a publisher. The publisher maintains a list of subscribers and notifies them via a common interface when an event occurs. This pattern promotes loose coupling, allowing new subscriber types to be added without modifying the publisher's code, and enables runtime relationships between objects.
---
```

# Observer

[](/)/ [Design Patterns](/design-patterns) / [Behavioral Patterns](/design-patterns/behavioral-patterns)

# Observer

Also known as: Event-Subscriber, Listener

## Intent

**Observer** is a behavioral design pattern that lets you define a subscription mechanism to notify multiple objects about any events that happen to the object they’re observing.

![Observer Design Pattern](/images/patterns/content/observer/observer.png?id=6088e31e1b0d4a417506a66614dcf065 "A central object (publisher) is shown with multiple lines extending to other objects (subscribers), illustrating a one-to-many dependency where the publisher notifies the subscribers.")

## Problem

Imagine that you have two types of objects: a `Customer` and a `Store`. The customer is very interested in a particular brand of product (say, it’s a new model of the iPhone) which should become available in the store very soon.

The customer could visit the store every day and check product availability. But while the product is still en route, most of these trips would be pointless.

![Visiting store vs. sending spam](/images/patterns/content/observer/observer-comic-1-en.png?id=1ec8571b22ea8fd2ed537f06cc763152 "A two-panel comic illustrating the problem. The left panel shows a person repeatedly walking from a house to a store. The right panel shows a store sending out many emails to various houses, some of which appear annoyed, while one is happy.")

Visiting the store vs. sending spam

On the other hand, the store could send tons of emails (which might be considered spam) to all customers each time a new product becomes available. This would save some customers from endless trips to the store. At the same time, it’d upset other customers who aren’t interested in new products.

It looks like we’ve got a conflict. Either the customer wastes time checking product availability or the store wastes resources notifying the wrong customers.

## Solution

The object that has some interesting state is often called _subject_, but since it’s also going to notify other objects about the changes to its state, we’ll call it _publisher_. All other objects that want to track changes to the publisher’s state are called _subscribers_.

The Observer pattern suggests that you add a subscription mechanism to the publisher class so individual objects can subscribe to or unsubscribe from a stream of events coming from that publisher. Fear not! Everything isn’t as complicated as it sounds. In reality, this mechanism consists of 1) an array field for storing a list of references to subscriber objects and 2) several public methods which allow adding subscribers to and removing them from that list.

![Subscription mechanism](/images/patterns/diagrams/observer/solution1-en.png?id=60fb9a2822649dec1c68b78733479c57 "A UML-like diagram showing two 'Subscriber' objects sending messages ('Hey, sign me up, please!' and 'Me too!') to a 'Publisher' object. The Publisher has a private 'subscribers[]' field and public 'addSubscriber(subscriber)' and 'removeSubscriber(subscriber)' methods.")

A subscription mechanism lets individual objects subscribe to event notifications.

Now, whenever an important event happens to the publisher, it goes over its subscribers and calls the specific notification method on their objects.

Real apps might have dozens of different subscriber classes that are interested in tracking events of the same publisher class. You wouldn’t want to couple the publisher to all of those classes. Besides, you might not even know about some of them beforehand if your publisher class is supposed to be used by other people.

That’s why it’s crucial that all subscribers implement the same interface and that the publisher communicates with them only via that interface. This interface should declare the notification method along with a set of parameters that the publisher can use to pass some contextual data along with the notification.

![Notification methods](/images/patterns/diagrams/observer/solution2-en.png?id=fcea7791ac77b6ecb6fea2c2b4128d4a "A UML-like diagram showing a 'Publisher' object with a private 'subscribers[]' field and a 'notifySubscribers()' method. Arrows extend from the Publisher to multiple 'Subscriber' objects, each with an 'update()' method. The Publisher says, 'Guys, I just want to let you know that something has just happened to me.'")

Publisher notifies subscribers by calling the specific notification method on their objects.

If your app has several different types of publishers and you want to make your subscribers compatible with all of them, you can go even further and make all publishers follow the same interface. This interface would only need to describe a few subscription methods. The interface would allow subscribers to observe publishers’ states without coupling to their concrete classes.

## Real-World Analogy

![Magazine and newspaper subscriptions](/images/patterns/content/observer/observer-comic-2-en.png?id=a9be31ab5f90e47b0f250fe9821c34c5 "A two-panel comic illustrating the real-world analogy. The left panel shows a laptop displaying a 'The New York Times' website with a subscription form and a 'THANKS! YOU ARE SUBSCRIBED!' message. The right panel shows a person on a bicycle delivering newspapers to houses.")

Magazine and newspaper subscriptions.

If you subscribe to a newspaper or magazine, you no longer need to go to the store to check if the next issue is available. Instead, the publisher sends new issues directly to your mailbox right after publication or even in advance.

The publisher maintains a list of subscribers and knows which magazines they’re interested in. Subscribers can leave the list at any time when they wish to stop the publisher sending new magazine issues to them.

## Structure

![Structure of the Observer design pattern](/images/patterns/diagrams/observer/structure.png?id=365b7e2b8fbecc8948f34b9f8f16f33c "A UML diagram illustrating the Observer design pattern. It shows a 'Publisher' class, a 'Subscriber' interface, 'Concrete Subscriber' classes implementing the interface, and a 'Client' that connects publishers and subscribers. The Publisher maintains a list of Subscriber references and notifies them via the interface.")

1.  The **Publisher** issues events of interest to other objects. These events occur when the publisher changes its state or executes some behaviors. Publishers contain a subscription infrastructure that lets new subscribers join and current subscribers leave the list.

2.  When a new event happens, the publisher goes over the subscription list and calls the notification method declared in the subscriber interface on each subscriber object.

3.  The **Subscriber** interface declares the notification interface. In most cases, it consists of a single `update` method. The method may have several parameters that let the publisher pass some event details along with the update.

4.  **Concrete Subscribers** perform some actions in response to notifications issued by the publisher. All of these classes must implement the same interface so the publisher isn’t coupled to concrete classes.

5.  Usually, subscribers need some contextual information to handle the update correctly. For this reason, publishers often pass some context data as arguments of the notification method. The publisher can pass itself as an argument, letting subscriber fetch any required data directly.

6.  The **Client** creates publisher and subscriber objects separately and then registers subscribers for publisher updates.

## Pseudocode

In this example, the **Observer** pattern lets the text editor object notify other service objects about changes in its state.

![Structure of the Observer pattern example](/images/patterns/diagrams/observer/example.png?id=6d0603ab5a00e4463b81d9639cd746a0 "A UML-like diagram showing an 'Editor' class (Concrete Publisher) that uses an 'EventManager' (Publisher). The EventManager notifies 'LoggingListener' and 'EmailAlertsListener' (Concrete Subscribers) via the 'EventListener' interface.")

Notifying objects about events that happen to other objects.

The list of subscribers is compiled dynamically: objects can start or stop listening to notifications at runtime, depending on the desired behavior of your app.

In this implementation, the editor class doesn’t maintain the subscription list by itself. It delegates this job to the special helper object devoted to just that. You could upgrade that object to serve as a centralized event dispatcher, letting any object act as a publisher.

Adding new subscribers to the program doesn’t require changes to existing publisher classes, as long as they work with all subscribers through the same interface.

```
// The base publisher class includes subscription management
// code and notification methods.
class EventManager is
    private field listeners: hash map of event types and listeners

    method subscribe(eventType, listener) is
        listeners.add(eventType, listener)

    method unsubscribe(eventType, listener) is
        listeners.remove(eventType, listener)

    method notify(eventType, data) is
        foreach (listener in listeners.of(eventType)) do
            listener.update(data)

// The concrete publisher contains real business logic that's
// interesting for some subscribers. We could derive this class
// from the base publisher, but that isn't always possible in
// real life because the concrete publisher might already be a
// subclass. In this case, you can patch the subscription logic
// in with composition, as we did here.
class Editor is
    public field events: EventManager
    private field file: File

    constructor Editor() is
        events = new EventManager()

    // Methods of business logic can notify subscribers about
    // changes.
    method openFile(path) is
        this.file = new File(path)
        events.notify("open", file.name)

    method saveFile() is
        file.write()
        events.notify("save", file.name)

    // ...


// Here's the subscriber interface. If your programming language
// supports functional types, you can replace the whole
// subscriber hierarchy with a set of functions.
interface EventListener is
    method update(filename)

// Concrete subscribers react to updates issued by the publisher
// they are attached to.
class LoggingListener implements EventListener is
    private field log: File
    private field message: string

    constructor LoggingListener(log_filename, message) is
        this.log = new File(log_filename)
        this.message = message

    method update(filename) is
        log.write(replace('%s',filename,message))

class EmailAlertsListener implements EventListener is
    private field email: string
    private field message: string

    constructor EmailAlertsListener(email, message) is
        this.email = email
        this.message = message

    method update(filename) is
        system.email(email, replace('%s',filename,message))


// An application can configure publishers and subscribers at
// runtime.
class Application is
    method config() is
        editor = new Editor()

        logger = new LoggingListener(
            "/path/to/log.txt",
            "Someone has opened the file: %s")
        editor.events.subscribe("open", logger)

        emailAlerts = new EmailAlertsListener(
            "admin@example.com",
            "Someone has changed the file: %s")
        editor.events.subscribe("save", emailAlerts)
```

## Applicability

Use the Observer pattern when changes to the state of one object may require changing other objects, and the actual set of objects is unknown beforehand or changes dynamically.

You can often experience this problem when working with classes of the graphical user interface. For example, you created custom button classes, and you want to let the clients hook some custom code to your buttons so that it fires whenever a user presses a button.

The Observer pattern lets any object that implements the subscriber interface subscribe for event notifications in publisher objects. You can add the subscription mechanism to your buttons, letting the clients hook up their custom code via custom subscriber classes.

Use the pattern when some objects in your app must observe others, but only for a limited time or in specific cases.

The subscription list is dynamic, so subscribers can join or leave the list whenever they need to.

## How to Implement

1.  Look over your business logic and try to break it down into two parts: the core functionality, independent from other code, will act as the publisher; the rest will turn into a set of subscriber classes.

2.  Declare the subscriber interface. At a bare minimum, it should declare a single `update` method.

3.  Declare the publisher interface and describe a pair of methods for adding a subscriber object to and removing it from the list. Remember that publishers must work with subscribers only via the subscriber interface.

4.  Decide where to put the actual subscription list and the implementation of subscription methods. Usually, this code looks the same for all types of publishers, so the obvious place to put it is in an abstract class derived directly from the publisher interface. Concrete publishers extend that class, inheriting the subscription behavior.

    However, if you’re applying the pattern to an existing class hierarchy, consider an approach based on composition: put the subscription logic into a separate object, and make all real publishers use it.

5.  Create concrete publisher classes. Each time something important happens inside a publisher, it must notify all its subscribers.

6.  Implement the update notification methods in concrete subscriber classes. Most subscribers would need some context data about the event. It can be passed as an argument of the notification method.

    But there’s another option. Upon receiving a notification, the subscriber can fetch any data directly from the notification. In this case, the publisher must pass itself via the update method. The less flexible option is to link a publisher to the subscriber permanently via the constructor.

7.  The client must create all necessary subscribers and register them with proper publishers.

## Pros and Cons

*   _Open/Closed Principle_. You can introduce new subscriber classes without having to change the publisher’s code (and vice versa if there’s a publisher interface).
*   You can establish relations between objects at runtime.

*   Subscribers are notified in random order.

## Relations with Other Patterns

*   [Chain of Responsibility](/design-patterns/chain-of-responsibility), [Command](/design-patterns/command), [Mediator](/design-patterns/mediator) and [Observer](/design-patterns/observer) address various ways of connecting senders and receivers of requests:

    *   _Chain of Responsibility_ passes a request sequentially along a dynamic chain of potential receivers until one of them handles it.
    *   _Command_ establishes unidirectional connections between senders and receivers.
    *   _Mediator_ eliminates direct connections between senders and receivers, forcing them to communicate indirectly via a mediator object.
    *   _Observer_ lets receivers dynamically subscribe to and unsubscribe from receiving requests.
*   The difference between [Mediator](/design-patterns/mediator) and [Observer](/design-patterns/observer) is often elusive. In most cases, you can implement either of these patterns; but sometimes you can apply both simultaneously. Let’s see how we can do that.

    The primary goal of _Mediator_ is to eliminate mutual dependencies among a set of system components. Instead, these components become dependent on a single mediator object. The goal of _Observer_ is to establish dynamic one-way connections between objects, where some objects act as subordinates of others.

    There’s a popular implementation of the _Mediator_ pattern that relies on _Observer_. The mediator object plays the role of publisher, and the components act as subscribers which subscribe to and unsubscribe from the mediator’s events. When _Mediator_ is implemented this way, it may look very similar to _Observer_.

    When you’re confused, remember that you can implement the Mediator pattern in other ways. For example, you can permanently link all the components to the same mediator object. This implementation won’t resemble _Observer_ but will still be an instance of the Mediator pattern.

    Now imagine a program where all components have become publishers, allowing dynamic connections between each other. There won’t be a centralized mediator object, only a distributed set of observers.

## Code Examples

[![Observer in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58 "C# language icon")](/design-patterns/observer/csharp/example "Observer in C#") [![Observer in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858 "C++ language icon")](/design-patterns/observer/cpp/example "Observer in C++") [![Observer in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca "Go language icon")](/design-patterns/observer/go/example "Observer in Go") [![Observer in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e "Java language icon")](/design-patterns/observer/java/example "Observer in Java") [![Observer in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618 "PHP language icon")](/design-patterns/observer/php/example "Observer in PHP") [![Observer in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f "Python language icon")](/design-patterns/observer/python/example "Observer in Python") [![Observer in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb "Ruby language icon")](/design-patterns/observer/ruby/example "Observer in Ruby") [![Observer in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87 "Rust language icon")](/design-patterns/observer/rust/example "Observer in Rust") [![Observer in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d "Swift language icon")](/design-patterns/observer/swift/example "Observer in Swift") [![Observer in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7 "TypeScript language icon")](/design-patterns/observer/typescript/example "Observer in TypeScript")

[![](/images/patterns/banners/patterns-book-banner-3.png?id=7d445df13c80287beaab234b4f3b698c "A promotional banner for an e-book titled 'Dive Into Design Patterns' with a list of features like 22 design patterns, 409 pages, and code examples in 11 languages.")](/design-patterns/book)

### Support our free website and own the eBook!

*   22 design patterns and 8 principles explained in depth.
*   409 well-structured, easy to read, jargon-free pages.
*   225 clear and helpful illustrations and diagrams.
*   An archive with code examples in 11 languages.
*   All devices supported: PDF/EPUB/MOBI/KFX formats.

[Learn more…](/design-patterns/book)

#### Read next

[State](/design-patterns/state)

#### Return

[Memento](/design-patterns/memento)

[![](/images/patterns/book/web-cover-en.png?id=328861769fd11617674e3b8a7e2dd9e7 "The cover of the 'Dive Into Design Patterns' e-book, showing a stylized illustration of a book.")](/design-patterns/book)

This article is a part of our eBook
**Dive Into Design Patterns**.

[Learn more…](/design-patterns/book)