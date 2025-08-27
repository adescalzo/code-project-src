```yaml
---
title: Singleton in Rust / Design Patterns
source: https://refactoring.guru/design-patterns/singleton/rust/example#example-0
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:29:54.915Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Rust, lazy_static, "std::sync::Mutex", log crate, env_logger crate]
programming_languages: [Rust, C#, C++, Go, Java, PHP, Python, Ruby, Swift, TypeScript]
tags: [singleton, design-patterns, rust, concurrency, global-state, unsafe, mutex, lazy-initialization, creational-patterns, software-design]
key_concepts: [singleton-pattern, creational-design-pattern, global-mutable-object, unsafe-block, lazy-initialization, mutex, static-mut, modularity]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explores the Singleton design pattern within the context of Rust, defining it as a creational pattern that ensures only one instance of a class exists. It highlights the challenges of implementing Singletons in Rust due to its strict safety rules, particularly concerning global mutable objects and the use of `unsafe` blocks. The content presents three distinct implementation strategies: a "safe" approach that avoids global variables, a "lazy" approach utilizing the `lazy_static!` crate for on-demand initialization, and a method employing `std::sync::Mutex` for thread-safe access. The discussion also touches upon the general drawbacks of Singletons, such as reduced code modularity and difficulties in unit testing, while providing practical code examples for each Rust implementation.]
---
```

# Singleton in Rust / Design Patterns

/ [Design Patterns](/design-patterns) / [Singleton](/design-patterns/singleton) / [Rust](/design-patterns/rust)

![This image shows a three-tiered podium, with the number "1" prominently displayed on each step, symbolizing the singular instance characteristic of the Singleton design pattern.](/images/patterns/cards/singleton-mini.png?id=914e1565dfdf15f240e766163bd303ec)

# **Singleton** in Rust

**Singleton** is a creational design pattern, which ensures that only one object of its kind exists and provides a single point of access to it for any other code.

Singleton has almost the same pros and cons as global variables. Although they’re super-handy, they break the modularity of your code.

You can’t just use a class that depends on a Singleton in some other context, without carrying over the Singleton to the other context. Most of the time, this limitation comes up during the creation of unit tests.

[Learn more about Singleton](/design-patterns/singleton)

Navigation

 [Intro](#)

 [Safe Singleton](#example-0)

 [local](#example-0--local-rs)

 [Lazy Singleton](#example-1)

 [lazy](#example-1--lazy-rs)

 [Singleton using _Mutex_](#example-2)

 [mutex](#example-2--mutex-rs)

### Rust specifics

By definition, Singleton is a _global mutable object_. In Rust this is a `static mut` item. Thus, to avoid all sorts of concurrency issues, the function or block that is either reading or writing to a mutable static variable should be marked as an [**`unsafe`** block](https://doc.rust-lang.org/reference/items/static-items.html#mutable-statics).

For this reason, the Singleton pattern can be percieved as unsafe. However, the pattern is still widely used in practice. A good read-world example of Singleton is a `log` crate that introduces `log!`, `debug!` and other logging macros, which you can use throughout your code after setting up a concrete logger instance, such as [env\_logger](https://crates.io/crates/env_logger). As we can see, `env_logger` uses [log::set\_boxed\_logger](https://docs.rs/log/latest/log/fn.set_boxed_logger.html) under the hood, which has an `unsafe` block to set up a global logger object.

*   In order to provide safe and usable access to a singleton object, introduce an API hiding `unsafe` blocks under the hood.
*   See the thread about a mutable Singleton on [Stackoverflow](https://stackoverflow.com/questions/27791532/how-do-i-create-a-global-mutable-singleton) for more information.

Starting with Rust 1.63, `Mutex::new` is `const`, you can use global static `Mutex` locks without needing lazy initialization. See the _Singleton using Mutex_ example below.

[Safe Singleton](#example-0) [Lazy Singleton](#example-1) [Singleton using _Mutex_](#example-2)

## Safe Singleton

A pure safe way to implement Singleton in Rust is using no global variables at all and passing everything around through function arguments. The oldest living variable is an object created at the start of the `main()`.

#### **local.rs**

```rust
//! A pure safe way to implement Singleton in Rust is using no static variables
//! and passing everything around through function arguments.
//! The oldest living variable is an object created at the start of the `main()`.

fn change(global_state: &mut u32) {
    *global_state += 1;
}

fn main() {
    let mut global_state = 0u32;

    change(&mut global_state);

    println!("Final state: {}", global_state);
}
```

### Output

```
Final state: 1
```

## Lazy Singleton

This is a singleton implementation via `lazy_static!`, which allows declaring a static variable with lazy initialization at first access. It is actually implemented via `unsafe` with `static mut` manipulation, however, it keeps your code clear of `unsafe` blocks.

#### **lazy.rs**

```rust
//! Taken from: https://stackoverflow.com/questions/27791532/how-do-i-create-a-global-mutable-singleton
//!
//! Rust doesn't really allow a singleton pattern without `unsafe` because it
//! doesn't have a safe mutable global state.
//!
//! `lazy-static` allows declaring a static variable with lazy initialization
//! at first access. It is actually implemented via `unsafe` with `static mut`
//! manipulation, however, it keeps your code clear of `unsafe` blocks.
//!
//! `Mutex` provides safe access to a single object.

use lazy_static::lazy_static;
use std::sync::Mutex;

lazy_static! {
    static ref ARRAY: Mutex<Vec<u8>> = Mutex::new(vec![]);
}

fn do_a_call() {
    ARRAY.lock().unwrap().push(1);
}

fn main() {
    do_a_call();
    do_a_call();
    do_a_call();

    println!("Called {}", ARRAY.lock().unwrap().len());
}
```

### Output

```
Called 3
```

## Singleton using _Mutex_

Starting with `Rust 1.63`, it can be easier to work with global mutable singletons, although it’s still preferable to avoid global variables in mostcases.

Now that `Mutex::new` is `const`, you can use global static `Mutex` locks without needing lazy initialization.

#### **mutex.rs**

```rust
//! ructc 1.63
//! https://stackoverflow.com/questions/27791532/how-do-i-create-a-global-mutable-singleton
//!
//! Starting with Rust 1.63, it can be easier to work with global mutable
//! singletons, although it's still preferable to avoid global variables in most
//! cases.
//!
//! Now that `Mutex::new` is `const`, you can use global static `Mutex` locks
//! without needing lazy initialization.

use std::sync::Mutex;

static ARRAY: Mutex<Vec<i32>> = Mutex::new(Vec::new());

fn do_a_call() {
    ARRAY.lock().unwrap().push(1);
}

fn main() {
    do_a_call();
    do_a_call();
    do_a_call();

    let array = ARRAY.lock().unwrap();
    println!("Called {} times: {:?}", array.len(), array);
    drop(array);

    *ARRAY.lock().unwrap() = vec![3, 4, 5];

    println!("New singleton object: {:?}", ARRAY.lock().unwrap());
}
```

### Output

```
Called 3 times
```

[Safe Singleton](#example-0) [Lazy Singleton](#example-1) [Singleton using _Mutex_](#example-2)

#### Read next

[Adapter in Rust](/design-patterns/adapter/rust/example) 

#### Return

 [Prototype in Rust](/design-patterns/prototype/rust/example)

## **Singleton** in Other Languages

[![Singleton in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58)](/design-patterns/singleton/csharp/example "Singleton in C#") [![Singleton in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/singleton/cpp/example "Singleton in C++") [![Singleton in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/singleton/go/example "Singleton in Go") [![Singleton in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/singleton/java/example "Singleton in Java") [![Singleton in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/singleton/php/example "Singleton in PHP") [![Singleton in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f)](/design-patterns/singleton/python/example "Singleton in Python") [![Singleton in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/singleton/ruby/example "Singleton in Ruby") [![Singleton in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/singleton/swift/example "Singleton in Swift") [![Singleton in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/singleton/typescript/example "Singleton in TypeScript")

[![This image is a banner depicting a stylized desktop environment with various development tools, code snippets, and UI elements, suggesting an Integrated Development Environment (IDE) or a comprehensive software development setup.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)](/design-patterns/book)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Safe Singleton](#example-0)

 [local](#example-0--local-rs)

 [Lazy Singleton](#example-1)

 [lazy](#example-1--lazy-rs)

 [Singleton using _Mutex_](#example-2)

 [mutex](#example-2--mutex-rs)