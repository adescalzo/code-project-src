```yaml
---
title: Iterator in Rust / Design Patterns
source: https://refactoring.guru/design-patterns/iterator/rust/example#example-0
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:28:35.256Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Rust Standard Library]
programming_languages: [Rust]
tags: [design-patterns, iterator-pattern, rust, behavioral-pattern, data-structures, traversal, idiomatic-rust]
key_concepts: [Iterator design pattern, behavioral design pattern, sequential traversal, Rust Iterator trait, custom iterators, next method]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explains the Iterator behavioral design pattern, which enables sequential traversal through complex data structures without exposing their internal details. It demonstrates how to use iterators in idiomatic Rust, covering both the standard `Iterator` trait for built-in collections and the implementation of custom iterators for user-defined types. The content highlights the importance of the `next` method and how implementing the `Iterator` trait provides access to a wide range of standard methods like `for_each`, `map`, and `fold`. Code examples illustrate both standard and custom iterator usage in Rust.
---
```

# Iterator in Rust / Design Patterns

![Iterator Pattern Icon](images/patterns/cards/iterator-mini.png "A red outline icon depicting a person with a backpack and walking stick, traversing a path of linked square nodes, symbolizing sequential traversal.")

# **Iterator** in Rust

**Iterator** is a behavioral design pattern that allows sequential traversal through a complex data structure without exposing its internal details.

Thanks to the Iterator, clients can go over elements of different collections in a similar fashion using a single iterator interface.

[Learn more about Iterator](/design-patterns/iterator)

Navigation

 [Intro](#)

 [Standard Iterator](#example-0)

 [Custom Iterator](#example-1)

 [users](#example-1--users-rs)

 [main](#example-1--main-rs)

[Standard Iterator](#example-0) [Custom Iterator](#example-1)

## Standard Iterator

Iterators are heavily used in idiomatic Rust code. This is how to use iterators over a standard array collection.

```rust
let array = &[1, 2, 3];
let iterator = array.iter();

// Traversal over each element of the vector.
iterator.for_each(|e| print!("{}, ", e));
```

## Custom Iterator

In Rust, the recommended way to define your _custom_ iterator is to use a standard `Iterator` trait. The example doesn’t contain a synthetic iterator interface, because it is really recommended to use the idiomatic Rust way.

```rust
let users = UserCollection::new();
let mut iterator = users.iter();

iterator.next();
```

A `next` method is the only `Iterator` trait method which is mandatory to be implemented. It makes accessible a huge range of standard methods, e.g. `fold`, `map`, `for_each`.

```rust
impl Iterator for UserIterator<'_> {
    fn next(&mut self) -> Option<Self::Item>;
}
```

#### **users.rs:** Collection & Iterator

```rust
pub struct UserCollection {
    users: [&'static str; 3],
}

/// A custom collection contains an arbitrary user array under the hood.
impl UserCollection {
    /// Returns a custom user collection.
    pub fn new() -> Self {
        Self {
            users: ["Alice", "Bob", "Carl"],
        }
    }

    /// Returns an iterator over a user collection.
    ///
    /// The method name may be different, however, `iter` is used as a de facto
    /// standard in a Rust naming convention.
    pub fn iter(&self) -> UserIterator {
        UserIterator {
            index: 0,
            user_collection: self,
        }
    }
}

/// UserIterator allows sequential traversal through a complex user collection
/// without exposing its internal details.
pub struct UserIterator<'a> {
    index: usize,
    user_collection: &'a UserCollection,
}

/// `Iterator` is a standard interface for dealing with iterators
/// from the Rust standard library.
impl Iterator for UserIterator<'_> {
    type Item = &'static str;

    /// A `next` method is the only `Iterator` trait method which is mandatory to be
    /// implemented. It makes accessible a huge range of standard methods,
    /// e.g. `fold`, `map`, `for_each`.
    fn next(&mut self) -> Option<Self::Item> {
        if self.index < self.user_collection.users.len() {
            let user = Some(self.user_collection.users[self.index]);
            self.index += 1;
            return user;
        }

        None
    }
}
```

#### **main.rs:** Client code

```rust
use crate::users::UserCollection;

mod users;

fn main() {
    print!("Iterators are widely used in the standard library: ");

    let array = &[1, 2, 3];
    let iterator = array.iter();

    // Traversal over each element of the array.
    iterator.for_each(|e| print!("{} ", e));

    println!("\n\nLet's test our own iterator.\n");

    let users = UserCollection::new();
    let mut iterator = users.iter();

    println!("1nd element: {:?}", iterator.next());
    println!("2nd element: {:?}", iterator.next());
    println!("3rd element: {:?}", iterator.next());
    println!("4th element: {:?}", iterator.next());

    print!("\nAll elements in user collection: ");
    users.iter().for_each(|e| print!("{} ", e));

    println!();
}
```

### Output

```
Iterators are widely used in the standard library: 1 2 3

Let's test our own iterator.

1nd element: Some("Alice")
2nd element: Some("Bob")
3rd element: Some("Carl")
4th element: None


All elements in user collection: Alice Bob Carl
```

[Standard Iterator](#example-0) [Custom Iterator](#example-1)

#### Read next

[Mediator in Rust](/design-patterns/mediator/rust/example) 

#### Return

 [Command in Rust](/design-patterns/command/rust/example)

## **Iterator** in Other Languages

[![Iterator in C#](/images/patterns/icons/csharp.svg "Iterator in C#")](/design-patterns/iterator/csharp/example "Iterator in C#") [![Iterator in C++](/images/patterns/icons/cpp.svg "Iterator in C++")](/design-patterns/iterator/cpp/example "Iterator in C++") [![Iterator in Go](/images/patterns/icons/go.svg "Iterator in Go")](/design-patterns/iterator/go/example "Iterator in Go") [![Iterator in Java](/images/patterns/icons/java.svg "Iterator in Java")](/design-patterns/iterator/java/example "Iterator in Java") [![Iterator in PHP](/images/patterns/icons/php.svg "Iterator in PHP")](/design-patterns/iterator/php/example "Iterator in PHP") [![Iterator in Python](/images/patterns/icons/python.svg "Iterator in Python")](/design-patterns/iterator/python/example "Iterator in Python") [![Iterator in Ruby](/images/patterns/icons/ruby.svg "Iterator in Ruby")](/design-patterns/iterator/ruby/example "Iterator in Ruby") [![Iterator in Swift](/images/patterns/icons/swift.svg "Iterator in Swift")](/design-patterns/iterator/swift/example "Iterator in Swift") [![Iterator in TypeScript](/images/patterns/icons/typescript.svg "Iterator in TypeScript")](/design-patterns/iterator/typescript/example "Iterator in TypeScript")

![Examples IDE Banner](images/patterns/banners/examples-ide.png "A stylized illustration of various programming tools and UI elements around a central tablet or monitor, promoting an eBook with code examples.")

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Standard Iterator](#example-0)

 [Custom Iterator](#example-1)

 [users](#example-1--users-rs)

 [main](#example-1--main-rs)