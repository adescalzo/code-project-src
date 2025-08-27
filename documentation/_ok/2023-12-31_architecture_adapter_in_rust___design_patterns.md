```yaml
---
title: Adapter in Rust / Design Patterns
source: https://refactoring.guru/design-patterns/adapter/rust/example#example-0
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:41:39.552Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Rust]
programming_languages: [Rust]
tags: [design-patterns, adapter-pattern, structural-patterns, rust, software-architecture, interface-adaptation, code-example, object-composition]
key_concepts: [Adapter design pattern, structural design patterns, interface incompatibility, trait, object composition, wrapper pattern]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces the Adapter structural design pattern, which facilitates collaboration between incompatible objects by acting as a wrapper. It provides a practical demonstration of the Adapter pattern implemented in Rust, illustrating how to bridge the gap between a `SpecificTarget` and a `Target` trait. The content includes detailed Rust code examples for the adapter, adaptee, target, and client logic, showing how the adapter transforms requests. The article concludes by displaying the output of the example code and offering links to Adapter pattern implementations in other programming languages.
---
```

# Adapter in Rust / Design Patterns

[](/)/ [Design Patterns](/design-patterns) / [Adapter](/design-patterns/adapter) / [Rust](/design-patterns/rust)

![Icon representing the Adapter design pattern, showing two incompatible components connected by a central adapter piece.](/images/patterns/cards/adapter-mini.png?id=b2ee4f681fb589be5a0685b94692aebb)

# **Adapter** in Rust

**Adapter** is a structural design pattern, which allows incompatible objects to collaborate.

The Adapter acts as a wrapper between two objects. It catches calls for one object and transforms them to format and interface recognizable by the second object.

[Learn more about Adapter](/design-patterns/adapter)

Navigation

 [Intro](#)

 [Adapter in Rust](#example-0)

 [adapter](#example-0--adapter-rs)

 [adaptee](#example-0--adaptee-rs)

 [target](#example-0--target-rs)

 [main](#example-0--main-rs)

## Adapter in Rust

In this example, the `trait SpecificTarget` is incompatible with a `call` function which accepts `trait Target` only.

```rust
fn call(target: impl Target);
```

The adapter helps to pass the incompatible interface to the `call` function.

```rust
let target = TargetAdapter::new(specific_target);
call(target);
```

#### [](#example-0--adapter-rs)**adapter.rs**

```rust
use crate::{adaptee::SpecificTarget, Target};

/// Converts adaptee's specific interface to a compatible `Target` output.
pub struct TargetAdapter {
    adaptee: SpecificTarget,
}

impl TargetAdapter {
    pub fn new(adaptee: SpecificTarget) -> Self {
        Self { adaptee }
    }
}

impl Target for TargetAdapter {
    fn request(&self) -> String {
        // Here's the "adaptation" of a specific output to a compatible output.
        self.adaptee.specific_request().chars().rev().collect()
    }
}
```

#### [](#example-0--adaptee-rs)**adaptee.rs**

```rust
pub struct SpecificTarget;

impl SpecificTarget {
    pub fn specific_request(&self) -> String {
        ".tseuqer cificepS".into()
    }
}
```

#### [](#example-0--target-rs)**target.rs**

```rust
pub trait Target {
    fn request(&self) -> String;
}

pub struct OrdinaryTarget;

impl Target for OrdinaryTarget {
    fn request(&self) -> String {
        "Ordinary request.".into()
    }
}
```

#### [](#example-0--main-rs)**main.rs**

```rust
mod adaptee;
mod adapter;
mod target;

use adaptee::SpecificTarget;
use adapter::TargetAdapter;
use target::{OrdinaryTarget, Target};

/// Calls any object of a `Target` trait.
///
/// To understand the Adapter pattern better, imagine that this is
/// a client code, which can operate over a specific interface only
/// (`Target` trait only). It means that an incompatible interface cannot be
/// passed here without an adapter.
fn call(target: impl Target) {
    println!("'{}'", target.request());
}

fn main() {
    let target = OrdinaryTarget;

    print!("A compatible target can be directly called: ");
    call(target);

    let adaptee = SpecificTarget;

    println!(
        "Adaptee is incompatible with client: '{}'",
        adaptee.specific_request()
    );

    let adapter = TargetAdapter::new(adaptee);

    print!("But with adapter client can call its method: ");
    call(adapter);
}
```

### Output

```
A compatible target can be directly called: 'Ordinary request.'
Adaptee is incompatible with client: '.tseuqer cificepS'
But with adapter client can call its method: 'Specific request.'
```

#### Read next

[Bridge in Rust](/design-patterns/bridge/rust/example) 

#### Return

 [Singleton in Rust](/design-patterns/singleton/rust/example)

## **Adapter** in Other Languages

[![Adapter in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58)](/design-patterns/adapter/csharp/example "Adapter in C#") [![Adapter in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/adapter/cpp/example "Adapter in C++") [![Adapter in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/adapter/go/example "Adapter in Go") [![Adapter in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/adapter/java/example "Adapter in Java") [![Adapter in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/adapter/php/example "Adapter in PHP") [![Adapter in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f)](/design-patterns/adapter/python/example "Adapter in Python") [![Adapter in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/adapter/ruby/example "Adapter in Ruby") [![Adapter in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/adapter/swift/example "Adapter in Swift") [![Adapter in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/adapter/typescript/example "Adapter in TypeScript")

[![Banner promoting an eBook with examples that can be opened in an IDE, showing various development tools and screens.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)](/design-patterns/book)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Adapter in Rust](#example-0)

 [adapter](#example-0--adapter-rs)

 [adaptee](#example-0--adaptee-rs)

 [target](#example-0--target-rs)

 [main](#example-0--main-rs)