```yaml
---
title: Observer in C# / Design Patterns
source: https://refactoring.guru/design-patterns/observer/csharp/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T14:00:47.634Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [.NET]
programming_languages: [C#]
tags: [design-patterns, observer-pattern, behavioral-patterns, csharp, software-design, event-driven, object-oriented-programming, notification, loose-coupling]
key_concepts: [observer-pattern, behavioral-design-pattern, subject-observer-relationship, subscription-mechanism, loose-coupling, interfaces, event-notification]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive overview of the Observer design pattern, a behavioral pattern that enables objects to notify other objects about changes in their state. It details how the pattern facilitates subscription and unsubscription to events, promoting loose coupling between components. A practical C# conceptual example demonstrates the roles of `ISubject`, `IObserver`, `Subject`, and `ConcreteObserver` classes, illustrating the notification mechanism. The content highlights the pattern's common application in GUI components and offers clear criteria for its identification in codebases.
---
```

# Observer in C# / Design Patterns

[](/)/ [Design Patterns](/design-patterns) / [Observer](/design-patterns/observer) / [C#](/design-patterns/csharp)

![A stylized illustration of a pair of binoculars, symbolizing observation or monitoring.](/images/patterns/cards/observer-mini.png?id=fd2081ab1cff29c60b499bcf6a62786a)

# **Observer** in C#

**Observer** is a behavioral design pattern that allows some objects to notify other objects about changes in their state.

The Observer pattern provides a way to subscribe and unsubscribe to and from these events for any object that implements a subscriber interface.

[Learn more about Observer](/design-patterns/observer)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)

**Complexity:**

**Popularity:**

**Usage examples:** The Observer pattern is pretty common in C# code, especially in the GUI components. It provides a way to react to events happening in other objects without coupling to their classes.

**Identification:** The pattern can be recognized by subscription methods, that store objects in a list and by calls to the update method issued to objects in that list.

## Conceptual Example

This example illustrates the structure of the **Observer** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### [](#example-0--Program-cs)**Program.cs:** Conceptual example

```csharp
using System;
using System.Collections.Generic;
using System.Threading;

namespace RefactoringGuru.DesignPatterns.Observer.Conceptual
{
    public interface IObserver
    {
        // Receive update from subject
        void Update(ISubject subject);
    }

    public interface ISubject
    {
        // Attach an observer to the subject.
        void Attach(IObserver observer);

        // Detach an observer from the subject.
        void Detach(IObserver observer);

        // Notify all observers about an event.
        void Notify();
    }

    // The Subject owns some important state and notifies observers when the
    // state changes.
    public class Subject : ISubject
    {
        // For the sake of simplicity, the Subject's state, essential to all
        // subscribers, is stored in this variable.
        public int State { get; set; } = -0;

        // List of subscribers. In real life, the list of subscribers can be
        // stored more comprehensively (categorized by event type, etc.).
        private List<IObserver> _observers = new List<IObserver>();

        // The subscription management methods.
        public void Attach(IObserver observer)
        {
            Console.WriteLine("Subject: Attached an observer.");
            this._observers.Add(observer);
        }

        public void Detach(IObserver observer)
        {
            this._observers.Remove(observer);
            Console.WriteLine("Subject: Detached an observer.");
        }

        // Trigger an update in each subscriber.
        public void Notify()
        {
            Console.WriteLine("Subject: Notifying observers...");

            foreach (var observer in _observers)
            {
                observer.Update(this);
            }
        }

        // Usually, the subscription logic is only a fraction of what a Subject
        // can really do. Subjects commonly hold some important business logic,
        // that triggers a notification method whenever something important is
        // about to happen (or after it).
        public void SomeBusinessLogic()
        {
            Console.WriteLine("\nSubject: I'm doing something important.");
            this.State = new Random().Next(0, 10);

            Thread.Sleep(15);

            Console.WriteLine("Subject: My state has just changed to: " + this.State);
            this.Notify();
        }
    }

    // Concrete Observers react to the updates issued by the Subject they had
    // been attached to.
    class ConcreteObserverA : IObserver
    {
        public void Update(ISubject subject)
        {            
            if ((subject as Subject).State < 3)
            {
                Console.WriteLine("ConcreteObserverA: Reacted to the event.");
            }
        }
    }

    class ConcreteObserverB : IObserver
    {
        public void Update(ISubject subject)
        {
            if ((subject as Subject).State == 0 || (subject as Subject).State >= 2)
            {
                Console.WriteLine("ConcreteObserverB: Reacted to the event.");
            }
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            // The client code.
            var subject = new Subject();
            var observerA = new ConcreteObserverA();
            subject.Attach(observerA);

            var observerB = new ConcreteObserverB();
            subject.Attach(observerB);

            subject.SomeBusinessLogic();
            subject.SomeBusinessLogic();

            subject.Detach(observerB);

            subject.SomeBusinessLogic();
        }
    }
}
```

#### [](#example-0--Output-txt)**Output.txt:** Execution result

```
Subject: Attached an observer.
Subject: Attached an observer.

Subject: I'm doing something important.
Subject: My state has just changed to: 2
Subject: Notifying observers...
ConcreteObserverA: Reacted to the event.
ConcreteObserverB: Reacted to the event.

Subject: I'm doing something important.
Subject: My state has just changed to: 1
Subject: Notifying observers...
ConcreteObserverA: Reacted to the event.
Subject: Detached an observer.

Subject: I'm doing something important.
Subject: My state has just changed to: 5
Subject: Notifying observers...
```

#### Read next

[State in C#](/design-patterns/state/csharp/example) 

#### Return

 [Memento in C#](/design-patterns/memento/csharp/example)

## **Observer** in Other Languages

[![Observer in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858 "Observer in C++")](/design-patterns/observer/cpp/example) [![Observer in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca "Observer in Go")](/design-patterns/observer/go/example) [![Observer in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e "Observer in Java")](/design-patterns/observer/java/example) [![Observer in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618 "Observer in PHP")](/design-patterns/observer/php/example) [![Observer in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f "Observer in Python")](/design-patterns/observer/python/example) [![Observer in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb "Observer in Ruby")](/design-patterns/observer/ruby/example) [![Observer in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87 "Observer in Rust")](/design-patterns/observer/rust/example) [![Observer in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d "Observer in Swift")](/design-patterns/observer/swift/example) [![Observer in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7 "Observer in TypeScript")](/design-patterns/observer/typescript/example)

![A complex illustration depicting a desktop computer, tablet, and smartphone displaying various UI elements, code snippets, and charts, surrounded by development tools and icons, suggesting a comprehensive collection of code examples or a development environment.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)