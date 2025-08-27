```yaml
---
title: Singleton in Python / Design Patterns
source: https://refactoring.guru/design-patterns/singleton/python/example#example-1
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:56:47.737Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Python, threading, Lock, Thread]
programming_languages: [Python]
tags: [design-pattern, singleton, python, creational-pattern, thread-safety, concurrency, metaclass, software-design]
key_concepts: [singleton-pattern, creational-design-patterns, thread-safety, metaclasses, global-variables, unit-testing, synchronization]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article details the Singleton design pattern in Python, a creational pattern that guarantees a class has only one instance and provides a global point of access to it. It discusses the pattern's advantages and disadvantages, noting its similarity to global variables and potential issues with code modularity and unit testing. The content presents two Python implementations: a basic "Naïve Singleton" using a metaclass, and a more robust "Thread-safe Singleton" that employs `threading.Lock` to prevent multiple instances in concurrent environments. Detailed code examples for both versions are provided, along with their execution results, illustrating how to achieve and verify thread-safe singleton behavior.]
---
```

# Singleton in Python / Design Patterns

[](/)/ [Design Patterns](/design-patterns) / [Singleton](/design-patterns/singleton) / [Python](/design-patterns/python)

![Singleton](/images/patterns/cards/singleton-mini.png?id=914e1565dfdf15f240e766163bd303ec)

# **Singleton** in Python

**Singleton** is a creational design pattern, which ensures that only one object of its kind exists and provides a single point of access to it for any other code.

Singleton has almost the same pros and cons as global variables. Although they’re super-handy, they break the modularity of your code.

You can’t just use a class that depends on a Singleton in some other context, without carrying over the Singleton to the other context. Most of the time, this limitation comes up during the creation of unit tests.

[Learn more about Singleton](/design-patterns/singleton)

Navigation

 [Intro](#)

 [Naïve Singleton](#example-0)

 [main](#example-0--main-py)

 [Output](#example-0--Output-txt)

 [Thread-safe Singleton](#example-1)

 [main](#example-1--main-py)

 [Output](#example-1--Output-txt)

**Usage examples:** A lot of developers consider the Singleton pattern an antipattern. That’s why its usage is on the decline in Python code.

**Identification:** Singleton can be recognized by a static creation method, which returns the same cached object.

[Naïve Singleton](#example-0) [Thread-safe Singleton](#example-1)

## Naïve Singleton

It’s pretty easy to implement a sloppy Singleton. You just need to hide the constructor and implement a static creation method.

The same class behaves incorrectly in a multithreaded environment. Multiple threads can call the creation method simultaneously and get several instances of Singleton class.

#### **main.py:** Conceptual example

```python
class SingletonMeta(type):
    """
    The Singleton class can be implemented in different ways in Python. Some
    possible methods include: base class, decorator, metaclass. We will use the
    metaclass because it is best suited for this purpose.
    """

    _instances = {}

    def __call__(cls, *args, **kwargs):
        """
        Possible changes to the value of the `__init__` argument do not affect
        the returned instance.
        """
        if cls not in cls._instances:
            instance = super().__call__(*args, **kwargs)
            cls._instances[cls] = instance
        return cls._instances[cls]

class Singleton(metaclass=SingletonMeta):
    def some_business_logic(self):
        """
        Finally, any singleton should define some business logic, which can be
        executed on its instance.
        """

        # ...

if __name__ == "__main__":
    # The client code.

    s1 = Singleton()
    s2 = Singleton()

    if id(s1) == id(s2):
        print("Singleton works, both variables contain the same instance.")
    else:
        print("Singleton failed, variables contain different instances.")
```

#### **Output.txt:** Execution result

```
Singleton works, both variables contain the same instance.
```

## Thread-safe Singleton

To fix the problem, you have to synchronize threads during the first creation of the Singleton object.

#### **main.py:** Conceptual example

```python
from threading import Lock, Thread

class SingletonMeta(type):
    """
    This is a thread-safe implementation of Singleton.
    """

    _instances = {}

    _lock: Lock = Lock()
    """
    We now have a lock object that will be used to synchronize threads during
    first access to the Singleton.
    """

    def __call__(cls, *args, **kwargs):
        """
        Possible changes to the value of the `__init__` argument do not affect
        the returned instance.
        """
        # Now, imagine that the program has just been launched. Since there's no
        # Singleton instance yet, multiple threads can simultaneously pass the
        # previous conditional and reach this point almost at the same time. The
        # first of them will acquire lock and will proceed further, while the
        # rest will wait here.
        with cls._lock:
            # The first thread to acquire the lock, reaches this conditional,
            # goes inside and creates the Singleton instance. Once it leaves the
            # lock block, a thread that might have been waiting for the lock
            # release may then enter this section. But since the Singleton field
            # is already initialized, the thread won't create a new object.
            if cls not in cls._instances:
                instance = super().__call__(*args, **kwargs)
                cls._instances[cls] = instance
        return cls._instances[cls]

class Singleton(metaclass=SingletonMeta):
    value: str = None
    """
    We'll use this property to prove that our Singleton really works.
    """

    def __init__(self, value: str) -> None:
        self.value = value

    def some_business_logic(self):
        """
        Finally, any singleton should define some business logic, which can be
        executed on its instance.
        """

def test_singleton(value: str) -> None:
    singleton = Singleton(value)
    print(singleton.value)

if __name__ == "__main__":
    # The client code.

    print("If you see the same value, then singleton was reused (yay!)\n"
          "If you see different values, "
          "then 2 singletons were created (booo!!)\n\n"
          "RESULT:\n")

    process1 = Thread(target=test_singleton, args=("FOO",))
    process2 = Thread(target=test_singleton, args=("BAR",))
    process1.start()
    process2.start()
```

#### **Output.txt:** Execution result

```
If you see the same value, then singleton was reused (yay!)
If you see different values, then 2 singletons were created (booo!!)

RESULT:

FOO
FOO
```

[Naïve Singleton](#example-0) [Thread-safe Singleton](#example-1)

#### Read next

[Adapter in Python](/design-patterns/adapter/python/example)

#### Return

[Prototype in Python](/design-patterns/prototype/python/example)

## **Singleton** in Other Languages

[![Singleton in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58)](/design-patterns/singleton/csharp/example "Singleton in C#") [![Singleton in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/singleton/cpp/example "Singleton in C++") [![Singleton in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/singleton/go/example "Singleton in Go") [![Singleton in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/singleton/java/example "Singleton in Java") [![Singleton in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/singleton/php/example "Singleton in PHP") [![Singleton in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/singleton/ruby/example "Singleton in Ruby") [![Singleton in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87)](/design-patterns/singleton/rust/example "Singleton in Rust") [![Singleton in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/singleton/swift/example "Singleton in Swift") [![Singleton in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/singleton/typescript/example "Singleton in TypeScript")

[![](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)](/design-patterns/book)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

---

**Image Analysis:**

The provided image is an abstract, illustrative representation of software development and data processing. It features a central tablet or monitor displaying various user interface elements, charts, and code snippets. Surrounding this central device are other digital tools and concepts, including a smartphone, smaller screens, abstract data visualizations, and stylized icons resembling development tools like a hammer and wrench. The overall composition suggests a comprehensive view of software creation, data analysis, and interconnected digital environments.