```yaml
---
title: Observer in Rust / Design Patterns
source: https://refactoring.guru/design-patterns/observer/rust/example#example-0
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:27:33.502Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Rust, "std::collections::HashMap"]
programming_languages: [Rust]
tags: [design-patterns, observer-pattern, behavioral-patterns, rust, event-driven, publish-subscribe, software-architecture]
key_concepts: [observer-pattern, publish-subscribe, event-handling, event-notification, callbacks, lambda-functions, design-patterns]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explains the Observer behavioral design pattern, focusing on its implementation in Rust. It details how objects can notify other subscribed objects about state changes through a publish-subscribe mechanism. The conceptual example demonstrates an `Editor` (publisher) notifying `Subscriber` functions (listeners) about `Load` and `Save` events. Subscribers can be either lambda functions or explicit functions, showcasing how to subscribe, unsubscribe, and notify listeners. The provided Rust code illustrates the `Publisher` and `Editor` structures, along with event handling logic.]
---
```

# Observer in Rust / Design Patterns

[](/)/ [Design Patterns](/design-patterns) / [Observer](/design-patterns/observer) / [Rust](/design-patterns/rust)

![A pair of binoculars, symbolizing observation or looking out for changes, representing the Observer design pattern.](/images/patterns/cards/observer-mini.png?id=fd2081ab1cff29c60b499bcf6a62786a)

# **Observer** in Rust

**Observer** is a behavioral design pattern that allows some objects to notify other objects about changes in their state.

The Observer pattern provides a way to subscribe and unsubscribe to and from these events for any object that implements a subscriber interface.

[Learn more about Observer](/design-patterns/observer)

Navigation

 [Intro](#)

 [Conceptual example](#example-0)

 [editor](#example-0--editor-rs)

 [observer](#example-0--observer-rs)

 [main](#example-0--main-rs)

## Conceptual example

In Rust, a convenient way to define a subscriber is to have a function as a callable object with complex logic passing it to a event publisher.

In this Observer example, Subscribers are either _a lambda function_ or _an explicit function_ subscribed to the event. Explicit function objects could be also unsubscribed (although, there could be limitations for some function types).

#### [](#example-0--editor-rs)**editor.rs**

use crate::observer::{Event, Publisher};

/// Editor has its own logic and it utilizes a publisher
/// to operate with subscribers and events.
#\[derive(Default)\]
pub struct Editor {
    publisher: Publisher,
    file\_path: String,
}

impl Editor {
    pub fn events(&mut self) \-> &mut Publisher {
        &mut self.publisher
    }

    pub fn load(&mut self, path: String) {
        self.file\_path \= path.clone();
        self.publisher.notify(Event::Load, path);
    }

    pub fn save(&self) {
        self.publisher.notify(Event::Save, self.file\_path.clone());
    }
}

#### [](#example-0--observer-rs)**observer.rs**

use std::collections::HashMap;

/// An event type.
#\[derive(PartialEq, Eq, Hash, Clone)\]
pub enum Event {
    Load,
    Save,
}

/// A subscriber (listener) has type of a callable function.
pub type Subscriber \= fn(file\_path: String);

/// Publisher sends events to subscribers (listeners).
#\[derive(Default)\]
pub struct Publisher {
    events: HashMap<Event, Vec<Subscriber\>>,
}

impl Publisher {
    pub fn subscribe(&mut self, event\_type: Event, listener: Subscriber) {
        self.events.entry(event\_type.clone()).or\_default();
        self.events.get\_mut(&event\_type).unwrap().push(listener);
    }

    pub fn unsubscribe(&mut self, event\_type: Event, listener: Subscriber) {
        self.events
            .get\_mut(&event\_type)
            .unwrap()
            .retain(|&x| x != listener);
    }

    pub fn notify(&self, event\_type: Event, file\_path: String) {
        let listeners \= self.events.get(&event\_type).unwrap();
        for listener in listeners {
            listener(file\_path.clone());
        }
    }
}

#### [](#example-0--main-rs)**main.rs**

use editor::Editor;
use observer::Event;

mod editor;
mod observer;

fn main() {
    let mut editor \= Editor::default();

    editor.events().subscribe(Event::Load, |file\_path| {
        let log \= "/path/to/log/file.txt".to\_string();
        println!("Save log to {}: Load file {}", log, file\_path);
    });

    editor.events().subscribe(Event::Save, save\_listener);

    editor.load("test1.txt".into());
    editor.load("test2.txt".into());
    editor.save();

    editor.events().unsubscribe(Event::Save, save\_listener);
    editor.save();
}

fn save\_listener(file\_path: String) {
    let email \= "admin@example.com".to\_string();
    println!("Email to {}: Save file {}", email, file\_path);
}

### Output

Save log to /path/to/log/file.txt: Load file test1.txt
Save log to /path/to/log/file.txt: Load file test2.txt
Email to admin@example.com: Save file test2.txt

#### Read next

[State in Rust](/design-patterns/state/rust/example) 

#### Return

 [Memento in Rust](/design-patterns/memento/rust/example)

## **Observer** in Other Languages

[![Observer in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58)](/design-patterns/observer/csharp/example "Observer in C#") [![Observer in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/observer/cpp/example "Observer in C++") [![Observer in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/observer/go/example "Observer in Go") [![Observer in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/observer/java/example "Observer in Java") [![Observer in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/observer/php/example "Observer in PHP") [![Observer in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f)](/design-patterns/observer/python/example "Observer in Python") [![Observer in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/observer/ruby/example "Observer in Ruby") [![Observer in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/observer/swift/example "Observer in Swift") [![Observer in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/observer/typescript/example "Observer in TypeScript")

[![An illustration depicting various software development elements like a desktop monitor showing code, a mobile phone, gears, tools, and charts, suggesting a comprehensive set of examples or a development environment.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)](/design-patterns/book)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Conceptual example](#example-0)

 [editor](#example-0--editor-rs)

 [observer](#example-0--observer-rs)

 [main](#example-0--main-rs)