```yaml
---
title: Proxy in Python / Design Patterns
source: https://refactoring.guru/design-patterns/proxy/python/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:56:14.637Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Python]
programming_languages: [Python, C#, C++, Go, Java, PHP, Ruby, Rust, Swift, TypeScript]
tags: [design-patterns, proxy-pattern, python, structural-pattern, object-oriented-programming, software-design, code-example, abstraction, interface]
key_concepts: [proxy-pattern, structural-design-patterns, interface, real-subject, client-code, access-control, caching, lazy-loading, logging]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Proxy design pattern, a structural pattern in software design, with a specific focus on its implementation in Python. It defines Proxy as an object that acts as a substitute for a real service object, intercepting client requests to perform additional work such as access control, caching, or logging before delegating to the actual service. The content includes a detailed conceptual example in Python, showcasing the `Subject`, `RealSubject`, and `Proxy` classes and their interactions. It also highlights common usage scenarios for the pattern and mentions its applicability across various other programming languages.]
---
```

# Proxy in Python / Design Patterns

![Proxy Pattern Icon](images/patterns/cards/proxy-mini.png?id=25890b11e7dc5af29625ccd0678b63a8 "A simple diagram showing a square pointing to a larger red bracket enclosing another square, symbolizing a proxy wrapping a real object.")

# **Proxy** in Python

**Proxy** is a structural design pattern that provides an object that acts as a substitute for a real service object used by a client. A proxy receives client requests, does some work (access control, caching, etc.) and then passes the request to a service object.

The proxy object has the same interface as a service, which makes it interchangeable with a real object when passed to a client.

[Learn more about Proxy](/design-patterns/proxy)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [main](#example-0--main-py)

 [Output](#example-0--Output-txt)

**Complexity:**

**Popularity:**

**Usage examples:** While the Proxy pattern isn’t a frequent guest in most Python applications, it’s still very handy in some special cases. It’s irreplaceable when you want to add some additional behaviors to an object of some existing class without changing the client code.

**Identification:** Proxies delegate all of the real work to some other object. Each proxy method should, in the end, refer to a service object unless the proxy is a subclass of a service.

## Conceptual Example

This example illustrates the structure of the **Proxy** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### **main.py:** Conceptual example

```python
from abc import ABC, abstractmethod


class Subject(ABC):
    """
    The Subject interface declares common operations for both RealSubject and
    the Proxy. As long as the client works with RealSubject using this
    interface, you'll be able to pass it a proxy instead of a real subject.
    """

    @abstractmethod
    def request(self) -> None:
        pass


class RealSubject(Subject):
    """
    The RealSubject contains some core business logic. Usually, RealSubjects are
    capable of doing some useful work which may also be very slow or sensitive -
    e.g. correcting input data. A Proxy can solve these issues without any
    changes to the RealSubject's code.
    """

    def request(self) -> None:
        print("RealSubject: Handling request.")


class Proxy(Subject):
    """
    The Proxy has an interface identical to the RealSubject.
    """

    def __init__(self, real_subject: RealSubject) -> None:
        self._real_subject = real_subject

    def request(self) -> None:
        """
        The most common applications of the Proxy pattern are lazy loading,
        caching, controlling the access, logging, etc. A Proxy can perform one
        of these things and then, depending on the result, pass the execution to
        the same method in a linked RealSubject object.
        """

        if self.check_access():
            self._real_subject.request()
            self.log_access()

    def check_access(self) -> bool:
        print("Proxy: Checking access prior to firing a real request.")
        return True

    def log_access(self) -> None:
        print("Proxy: Logging the time of request.", end="")


def client_code(subject: Subject) -> None:
    """
    The client code is supposed to work with all objects (both subjects and
    proxies) via the Subject interface in order to support both real subjects
    and proxies. In real life, however, clients mostly work with their real
    subjects directly. In this case, to implement the pattern more easily, you
    can extend your proxy from the real subject's class.
    """

    # ...

    subject.request()

    # ...


if __name__ == "__main__":
    print("Client: Executing the client code with a real subject:")
    real_subject = RealSubject()
    client_code(real_subject)

    print("")

    print("Client: Executing the same client code with a proxy:")
    proxy = Proxy(real_subject)
    client_code(proxy)
```

#### **Output.txt:** Execution result

```text
Client: Executing the client code with a real subject:
RealSubject: Handling request.

Client: Executing the same client code with a proxy:
Proxy: Checking access prior to firing a real request.
RealSubject: Handling request.
Proxy: Logging the time of request.
```

#### Read next

[Chain of Responsibility in Python](/design-patterns/chain-of-responsibility/python/example) 

#### Return

 [Flyweight in Python](/design-patterns/flyweight/python/example)

## **Proxy** in Other Languages

[![Proxy in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58 "C# language icon")](/design-patterns/proxy/csharp/example "Proxy in C#") [![Proxy in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858 "C++ language icon")](/design-patterns/proxy/cpp/example "Proxy in C++") [![Proxy in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca "Go language icon")](/design-patterns/proxy/go/example "Proxy in Go") [![Proxy in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e "Java language icon")](/design-patterns/proxy/java/example "Proxy in Java") [![Proxy in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618 "PHP language icon")](/design-patterns/proxy/php/example "Proxy in PHP") [![Proxy in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb "Ruby language icon")](/design-patterns/proxy/ruby/example "Proxy in Ruby") [![Proxy in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87 "Rust language icon")](/design-patterns/proxy/rust/example "Proxy in Rust") [![Proxy in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d "Swift language icon")](/design-patterns/proxy/swift/example "Proxy in Swift") [![Proxy in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7 "TypeScript language icon")](/design-patterns/proxy/typescript/example "Proxy in TypeScript")

![Examples IDE Banner](images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc "An illustration depicting various digital devices (desktop, tablet, mobile) surrounded by coding tools, charts, and data, representing a development environment or a collection of code examples.")

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [main](#example-0--main-py)

 [Output](#example-0--Output-txt)