```yaml
---
title: Singleton in C# / Design Patterns
source: https://refactoring.guru/design-patterns/singleton/csharp/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T17:57:08.366Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [.NET, System.Threading]
programming_languages: [C#]
tags: [singleton, design-patterns, creational-patterns, csharp, concurrency, thread-safety, software-design, object-oriented-programming]
key_concepts: [singleton-pattern, creational-design-patterns, thread-safety, lazy-initialization, double-checked-locking, anti-pattern, unit-testing]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explains the Singleton design pattern, a creational pattern ensuring only one instance of a class exists while providing a global access point. It discusses the pattern's advantages and disadvantages, noting its potential as an anti-pattern due to breaking modularity and complicating unit testing. The content provides C# code examples for both a basic, non-thread-safe implementation and a more robust, thread-safe version utilizing double-checked locking. The examples demonstrate how to correctly implement the pattern and verify its behavior in single-threaded and multi-threaded environments.
---
```

# Singleton in C# / Design Patterns

![An illustration of a podium with three number '1's, representing the unique, single instance characteristic of the Singleton design pattern.](/images/patterns/cards/singleton-mini.png?id=914e1565dfdf15f240e766163bd303ec)

# **Singleton** in C#

**Singleton** is a creational design pattern, which ensures that only one object of its kind exists and provides a single point of access to it for any other code.

Singleton has almost the same pros and cons as global variables. Although they’re super-handy, they break the modularity of your code.

You can’t just use a class that depends on a Singleton in some other context, without carrying over the Singleton to the other context. Most of the time, this limitation comes up during the creation of unit tests.

## Naïve Singleton

It’s pretty easy to implement a sloppy Singleton. You just need to hide the constructor and implement a static creation method.

The same class behaves incorrectly in a multithreaded environment. Multiple threads can call the creation method simultaneously and get several instances of Singleton class.

#### **Program.cs:** Conceptual example

```csharp
using System;

namespace RefactoringGuru.DesignPatterns.Singleton.Conceptual.NonThreadSafe
{
    // The Singleton class defines the `GetInstance` method that serves as an
    // alternative to constructor and lets clients access the same instance of
    // this class over and over.

    // EN : The Singleton should always be a 'sealed' class to prevent class
    // inheritance through external classes and also through nested classes.
    public sealed class Singleton
    {
        // The Singleton's constructor should always be private to prevent
        // direct construction calls with the `new` operator.
        private Singleton() { }

        // The Singleton's instance is stored in a static field. There there are
        // multiple ways to initialize this field, all of them have various pros
        // and cons. In this example we'll show the simplest of these ways,
        // which, however, doesn't work really well in multithreaded program.
        private static Singleton _instance;

        // This is the static method that controls the access to the singleton
        // instance. On the first run, it creates a singleton object and places
        // it into the static field. On subsequent runs, it returns the client
        // existing object stored in the static field.
        public static Singleton GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Singleton();
            }
            return _instance;
        }

        // Finally, any singleton should define some business logic, which can
        // be executed on its instance.
        public void someBusinessLogic()
        {
            // ...
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // The client code.
            Singleton s1 = Singleton.GetInstance();
            Singleton s2 = Singleton.GetInstance();

            if (s1 == s2)
            {
                Console.WriteLine("Singleton works, both variables contain the same instance.");
            }
            else
            {
                Console.WriteLine("Singleton failed, variables contain different instances.");
            }
        }
    }
}
```

#### **Output.txt:** Execution result

```
Singleton works, both variables contain the same instance.
```

## Thread-safe Singleton

To fix the problem, you have to synchronize threads during the first creation of the Singleton object.

#### **Program.cs:** Conceptual example

```csharp
using System;
using System.Threading;

namespace Singleton
{
    // This Singleton implementation is called "double check lock". It is safe
    // in multithreaded environment and provides lazy initialization for the
    // Singleton object.
    class Singleton
    {
        private Singleton() { }

        private static Singleton _instance;

        // We now have a lock object that will be used to synchronize threads
        // during first access to the Singleton.
        private static readonly object _lock = new object();

        public static Singleton GetInstance(string value)
        {
            // This conditional is needed to prevent threads stumbling over the
            // lock once the instance is ready.
            if (_instance == null)
            {
                // Now, imagine that the program has just been launched. Since
                // there's no Singleton instance yet, multiple threads can
                // simultaneously pass the previous conditional and reach this
                // point almost at the same time. The first of them will acquire
                // lock and will proceed further, while the rest will wait here.
                lock (_lock)
                {
                    // The first thread to acquire the lock, reaches this
                    // conditional, goes inside and creates the Singleton
                    // instance. Once it leaves the lock block, a thread that
                    // might have been waiting for the lock release may then
                    // enter this section. But since the Singleton field is
                    // already initialized, the thread won't create a new
                    // object.
                    if (_instance == null)
                    {
                        _instance = new Singleton();
                        _instance.Value = value;
                    }
                }
            }
            return _instance;
        }

        // We'll use this property to prove that our Singleton really works.
        public string Value { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // The client code.
            
            Console.WriteLine(
                "{0}\n{1}\n\n{2}\n",
                "If you see the same value, then singleton was reused (yay!)",
                "If you see different values, then 2 singletons were created (booo!!)",
                "RESULT:"
            );
            
            Thread process1 = new Thread(() =>
            {
                TestSingleton("FOO");
            });
            Thread process2 = new Thread(() =>
            {
                TestSingleton("BAR");
            });
            
            process1.Start();
            process2.Start();
            
            process1.Join();
            process2.Join();
        }
        
        public static void TestSingleton(string value)
        {
            Singleton singleton = Singleton.GetInstance(value);
            Console.WriteLine(singleton.Value);
        } 
    }
}
```

#### **Output.txt:** Execution result

```
FOO
FOO
```

![An intricate illustration depicting a central desktop monitor, a tablet, and a smartphone, surrounded by various software development elements like code snippets, data charts, tools (wrench, hammer), and UI components, symbolizing a comprehensive software development environment or ecosystem.](https://refactoring.guru/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)