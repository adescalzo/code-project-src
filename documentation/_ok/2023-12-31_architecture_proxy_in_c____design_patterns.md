```yaml
---
title: Proxy in C# / Design Patterns
source: https://refactoring.guru/design-patterns/proxy/csharp/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T15:56:05.272Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: []
programming_languages: [C#]
tags: [design-pattern, structural-pattern, proxy, csharp, object-oriented-programming, software-design, code-example]
key_concepts: [proxy-pattern, structural-design-pattern, subject-interface, real-subject, lazy-loading, caching, access-control, logging]
code_examples: false
difficulty_level: intermediate
summary: |
  [The Proxy design pattern is a structural pattern that provides a substitute or placeholder for another object to control access to it. It allows adding extra behaviors like access control, caching, or logging before or after forwarding requests to the real service object. The proxy object shares the same interface as the real service, making it interchangeable from the client's perspective. This article illustrates the pattern's structure with a conceptual C# example, demonstrating how a proxy can intercept and manage requests to a `RealSubject` without altering the client code.]
---
```

# Proxy in C# / Design Patterns

![Diagram illustrating the Proxy pattern, showing a client interacting with a proxy (represented by a red 'C' shape wrapping a square) which then interacts with the real subject (a simple square). An arrow points from the client to the proxy, and another from the proxy to the real subject.](/images/patterns/cards/proxy-mini.png?id=25890b11e7dc5af29625ccd0678b63a8)

# **Proxy** in C#

**Proxy** is a structural design pattern that provides an object that acts as a substitute for a real service object used by a client. A proxy receives client requests, does some work (access control, caching, etc.) and then passes the request to a service object.

The proxy object has the same interface as a service, which makes it interchangeable with a real object when passed to a client.

[Learn more about Proxy](/design-patterns/proxy)

**Usage examples:** While the Proxy pattern isn’t a frequent guest in most C# applications, it’s still very handy in some special cases. It’s irreplaceable when you want to add some additional behaviors to an object of some existing class without changing the client code.

**Identification:** Proxies delegate all of the real work to some other object. Each proxy method should, in the end, refer to a service object unless the proxy is a subclass of a service.

## Conceptual Example

This example illustrates the structure of the **Proxy** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

### Program.cs: Conceptual example

```csharp
using System;

namespace RefactoringGuru.DesignPatterns.Proxy.Conceptual
{
    // The Subject interface declares common operations for both RealSubject and
    // the Proxy. As long as the client works with RealSubject using this
    // interface, you'll be able to pass it a proxy instead of a real subject.
    public interface ISubject
    {
        void Request();
    }
    
    // The RealSubject contains some core business logic. Usually, RealSubjects
    // are capable of doing some useful work which may also be very slow or
    // sensitive - e.g. correcting input data. A Proxy can solve these issues
    // without any changes to the RealSubject's code.
    class RealSubject : ISubject
    {
        public void Request()
        {
            Console.WriteLine("RealSubject: Handling Request.");
        }
    }
    
    // The Proxy has an interface identical to the RealSubject.
    class Proxy : ISubject
    {
        private RealSubject _realSubject;
        
        public Proxy(RealSubject realSubject)
        {
            this._realSubject = realSubject;
        }
        
        // The most common applications of the Proxy pattern are lazy loading,
        // caching, controlling the access, logging, etc. A Proxy can perform
        // one of these things and then, depending on the result, pass the
        // execution to the same method in a linked RealSubject object.
        public void Request()
        {
            if (this.CheckAccess())
            {
                this._realSubject.Request();

                this.LogAccess();
            }
        }
		
        public bool CheckAccess()
        {
            // Some real checks should go here.
            Console.WriteLine("Proxy: Checking access prior to firing a real request.");

            return true;
        }
		
        public void LogAccess()
        {
            Console.WriteLine("Proxy: Logging the time of request.");
        }
    }
    
    public class Client
    {
        // The client code is supposed to work with all objects (both subjects
        // and proxies) via the Subject interface in order to support both real
        // subjects and proxies. In real life, however, clients mostly work with
        // their real subjects directly. In this case, to implement the pattern
        // more easily, you can extend your proxy from the real subject's class.
        public void ClientCode(ISubject subject)
        {
            // ...
            
            subject.Request();
            
            // ...
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client();
            
            Console.WriteLine("Client: Executing the client code with a real subject:");
            RealSubject realSubject = new RealSubject();
            client.ClientCode(realSubject);

            Console.WriteLine();

            Console.WriteLine("Client: Executing the same client code with a proxy:");
            Proxy proxy = new Proxy(realSubject);
            client.ClientCode(proxy);
        }
    }
}
```

### Output: Execution result

```text
Client: Executing the client code with a real subject:
RealSubject: Handling Request.

Client: Executing the same client code with a proxy:
Proxy: Checking access prior to firing a real request.
RealSubject: Handling Request.
Proxy: Logging the time of request.
```