```yaml
---
title: Strategy in Rust / Design Patterns
source: https://refactoring.guru/design-patterns/strategy/rust/example#example-0
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:26:55.850Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Rust]
programming_languages: [Rust]
tags: [design-patterns, strategy-pattern, rust, behavioral-patterns, traits, closures, functional-programming, software-design]
key_concepts: [strategy-pattern, behavioral-design-patterns, traits, polymorphism, dependency-injection, functional-programming, closures]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explains the Strategy design pattern, a behavioral pattern that encapsulates interchangeable behaviors into separate objects. It demonstrates how to implement the Strategy pattern in Rust through two distinct approaches. The first approach uses Rust traits to achieve polymorphism, allowing different routing strategies to be injected into a `Navigator` context. The second approach showcases a more functional implementation, leveraging Rust's functions and closures to inject behavior directly, simplifying the pattern's application. Both examples illustrate how the pattern enables flexible and extensible code by allowing the algorithm to vary independently from the client that uses it.
---
```

# Strategy in Rust / Design Patterns

# **Strategy** in Rust

**Strategy** is a behavioral design pattern that turns a set of behaviors into objects and makes them interchangeable inside original context object.

The original object, called context, holds a reference to a strategy object. The context delegates executing the behavior to the linked strategy object. In order to change the way the context performs its work, other objects may replace the currently linked strategy object with another one.

## Conceptual Example

A conceptual Strategy example via traits.

### conceptual.rs

```rust
/// Defines an injectable strategy for building routes.
trait RouteStrategy {
    fn build_route(&self, from: &str, to: &str);
}

struct WalkingStrategy;

impl RouteStrategy for WalkingStrategy {
    fn build_route(&self, from: &str, to: &str) {
        println!("Walking route from {} to {}: 4 km, 30 min", from, to);
    }
}

struct PublicTransportStrategy;

impl RouteStrategy for PublicTransportStrategy {
    fn build_route(&self, from: &str, to: &str) {
        println!(
            "Public transport route from {} to {}: 3 km, 5 min",
            from, to
        );
    }
}

struct Navigator<T: RouteStrategy> {
    route_strategy: T,
}

impl<T: RouteStrategy> Navigator<T> {
    pub fn new(route_strategy: T) -> Self {
        Self { route_strategy }
    }

    pub fn route(&self, from: &str, to: &str) {
        self.route_strategy.build_route(from, to);
    }
}

fn main() {
    let navigator = Navigator::new(WalkingStrategy);
    navigator.route("Home", "Club");
    navigator.route("Club", "Work");

    let navigator = Navigator::new(PublicTransportStrategy);
    navigator.route("Home", "Club");
    navigator.route("Club", "Work");
}
```

### Output

```
Walking route from Home to Club: 4 km, 30 min
Walking route from Club to Work: 4 km, 30 min
Public transport route from Home to Club: 3 km, 5 min
Public transport route from Club to Work: 3 km, 5 min
```

## Functional approach

Functions and closures simplify Strategy implementation as you can inject behavior right into the object without complex interface definition.

It seems that Strategy is often implicitly and widely used in the modern development with Rust, e.g. it’s just like iterators work:

```rust
let a = [0i32, 1, 2];

let mut iter = a.iter().filter(|x| x.is_positive());
```

### functional.rs

```rust
type RouteStrategy = fn(from: &str, to: &str);

fn walking_strategy(from: &str, to: &str) {
    println!("Walking route from {} to {}: 4 km, 30 min", from, to);
}

fn public_transport_strategy(from: &str, to: &str) {
    println!(
        "Public transport route from {} to {}: 3 km, 5 min",
        from, to
    );
}

struct Navigator {
    route_strategy: RouteStrategy,
}

impl Navigator {
    pub fn new(route_strategy: RouteStrategy) -> Self {
        Self { route_strategy }
    }

    pub fn route(&self, from: &str, to: &str) {
        (self.route_strategy)(from, to);
    }
}

fn main() {
    let navigator = Navigator::new(walking_strategy);
    navigator.route("Home", "Club");
    navigator.route("Club", "Work");

    let navigator = Navigator::new(public_transport_strategy);
    navigator.route("Home", "Club");
    navigator.route("Club", "Work");

    let navigator = Navigator::new(|from, to| println!("Specific route from {} to {}", from, to));
    navigator.route("Home", "Club");
    navigator.route("Club", "Work");
}
```

### Output

```
Walking route from Home to Club: 4 km, 30 min
Walking route from Club to Work: 4 km, 30 min
Public transport route from Home to Club: 3 km, 5 min
Public transport route from Club to Work: 3 km, 5 min
Specific route from Home to Club
Specific route from Club to Work
```

**Image Analysis:**

1.  **Image 1:** An icon representing the Strategy design pattern, showing a document with a highlighted section delegating to a hand icon, which then branches to different action icons like a sword and a shield, symbolizing interchangeable behaviors.
2.  **Image 2:** A banner image depicting various digital devices (desktop, tablet, smartphone) displaying code and development tools, likely promoting an eBook or a collection of code examples.