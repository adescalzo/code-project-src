```yaml
---
title: Abstract Factory in Rust / Design Patterns
source: https://refactoring.guru/design-patterns/abstract-factory/rust/example#example-0
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:41:54.515Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Rust]
programming_languages: [Rust]
tags: [abstract-factory, design-patterns, rust, creational-patterns, gui, static-dispatch, dynamic-dispatch, software-architecture, traits, generics]
key_concepts: [Abstract Factory pattern, creational design patterns, static dispatch, dynamic dispatch, Rust traits, Rust generics, product families, interface-based programming]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explains the Abstract Factory design pattern, a creational pattern that enables the creation of entire product families without specifying their concrete classes. It demonstrates the pattern's implementation in Rust through a practical GUI elements factory example. The content highlights two distinct approaches for implementing abstract factories in Rust: static dispatch using generics and dynamic dispatch using `Box` pointers. It also discusses the considerations and trade-offs between static and dynamic dispatch, offering guidance on when to use each in Rust applications and libraries.]
---
```

# Abstract Factory in Rust / Design Patterns

![Icon representing the Abstract Factory design pattern.](/images/patterns/cards/abstract-factory-mini.png?id=4c3927c446313a38ce77dfee38111e27)

# **Abstract Factory** in Rust

**Abstract Factory** is a creational design pattern, which solves the problem of creating entire product families without specifying their concrete classes.

Abstract Factory defines an interface for creating all distinct products but leaves the actual product creation to concrete factory classes. Each factory type corresponds to a certain product variety.

The client code calls the creation methods of a factory object instead of creating products directly with a constructor call (`new` operator). Since a factory corresponds to a single product variant, all its products will be compatible.

Client code works with factories and products only through their abstract interfaces. This lets the client code work with any product variants, created by the factory object. You just create a new concrete factory class and pass it to the client code.

> If you can’t figure out the difference between various factory patterns and concepts, then read our [Factory Comparison](/design-patterns/factory-comparison).

[Learn more about Abstract Factory](/design-patterns/abstract-factory)

Navigation

 [Intro](#)

 [GUI Elements Factory](#example-0)

 gui

  [lib](#example-0--gui-lib-rs)

 macos-gui

  [lib](#example-0--macos-gui-lib-rs)

 windows-gui

  [lib](#example-0--windows-gui-lib-rs)

 app

  [main](#example-0--app-main-rs)

  [render](#example-0--app-render-rs)

 app-dyn

  [main](#example-0--app-dyn-main-rs)

  [render](#example-0--app-dyn-render-rs)

## GUI Elements Factory

This example illustrates how a GUI framework can organize its classes into independent libraries:

1.  The `gui` library defines interfaces for all the components.  
    It has no external dependencies.
2.  The `windows-gui` library provides Windows implementation of the base GUI.  
    Depends on `gui`.
3.  The `macos-gui` library provides Mac OS implementation of the base GUI.  
    Depends on `gui`.

The `app` is a client application that can use several implementations of the GUI framework, depending on the current environment or configuration. However, most of the `app` code _doesn’t depend on specific types of GUI elements_. All the client code works with GUI elements through abstract interfaces (traits) defined by the `gui` lib.

There are two approaches to implementing abstract factories in Rust:

*   using generics (_static dispatch_)
*   using dynamic allocation (_dynamic dispatch_)

When you’re given a choice between static and dynamic dispatch, there is rarely a clear-cut correct answer. You’ll want to use static dispatch in your libraries and dynamic dispatch in your binaries. In a library, you want to allow your users to decide what kind of dispatch is best for them since you don’t know what their needs are. If you use dynamic dispatch, they’re forced to do the same, whereas if you use static dispatch, they can choose whether to use dynamic dispatch or not.

### **gui:** Abstract Factory and Abstract Products

#### **gui/lib.rs**

```rust
pub trait Button {
    fn press(&self);
}

pub trait Checkbox {
    fn switch(&self);
}

/// Abstract Factory defined using generics.
pub trait GuiFactory {
    type B: Button;
    type C: Checkbox;

    fn create_button(&self) -> Self::B;
    fn create_checkbox(&self) -> Self::C;
}

/// Abstract Factory defined using Box pointer.
pub trait GuiFactoryDynamic {
    fn create_button(&self) -> Box<dyn Button>;
    fn create_checkbox(&self) -> Box<dyn Checkbox>;
}
```

### **macos-gui:** One family of products

#### **macos-gui/lib.rs**

```rust
pub mod button;
pub mod checkbox;
pub mod factory;
```

### **windows-gui:** Another family of products

#### **windows-gui/lib.rs**

```rust
pub mod button;
pub mod checkbox;
pub mod factory;
```

#### Static dispatch

Here, the abstract factory is implemented via **generics** which lets the compiler create a code that does NOT require dynamic dispatch in runtime.

### **app:** Client code with static dispatch

#### **app/main.rs**

```rust
mod render;

use render::render;

use macos_gui::factory::MacFactory;
use windows_gui::factory::WindowsFactory;

fn main() {
    let windows = true;

    if windows {
        render(WindowsFactory);
    } else {
        render(MacFactory);
    }
}
```

#### **app/render.rs**

```rust
//! The code demonstrates that it doesn't depend on a concrete
//! factory implementation.

use gui::GuiFactory;

// Renders GUI. Factory object must be passed as a parameter to such the
// generic function with factory invocation to utilize static dispatch.
pub fn render(factory: impl GuiFactory) {
    let button1 = factory.create_button();
    let button2 = factory.create_button();
    let checkbox1 = factory.create_checkbox();
    let checkbox2 = factory.create_checkbox();

    use gui::{Button, Checkbox};

    button1.press();
    button2.press();
    checkbox1.switch();
    checkbox2.switch();
}
```

#### Dynamic dispatch

If a concrete type of abstract factory is not known at the compilation time, then is should be implemented using `Box` pointers.

### **app-dyn:** Client code with dynamic dispatch

#### **app-dyn/main.rs**

```rust
mod render;

use render::render;

use gui::GuiFactoryDynamic;
use macos_gui::factory::MacFactory;
use windows_gui::factory::WindowsFactory;

fn main() {
    let windows = false;

    // Allocate a factory object in runtime depending on unpredictable input.
    let factory: &dyn GuiFactoryDynamic = if windows {
        &WindowsFactory
    } else {
        &MacFactory
    };

    // Factory invocation can be inlined right here.
    let button = factory.create_button();
    button.press();

    // Factory object can be passed to a function as a parameter.
    render(factory);
}
```

#### **app-dyn/render.rs**

```rust
//! The code demonstrates that it doesn't depend on a concrete
//! factory implementation.

use gui::GuiFactoryDynamic;

/// Renders GUI.
pub fn render(factory: &dyn GuiFactoryDynamic) {
    let button1 = factory.create_button();
    let button2 = factory.create_button();
    let checkbox1 = factory.create_checkbox();
    let checkbox2 = factory.create_checkbox();

    button1.press();
    button2.press();
    checkbox1.switch();
    checkbox2.switch();
}
```

### Output

```
Windows button has pressed
Windows button has pressed
Windows checkbox has switched
Windows checkbox has switched
```

#### Read next

[Builder in Rust](/design-patterns/builder/rust/example) 

#### Return

 [Design Patterns in Rust](/design-patterns/rust)

## **Abstract Factory** in Other Languages

![Abstract Factory in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58 "Abstract Factory in C#") ![Abstract Factory in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858 "Abstract Factory in C++") ![Abstract Factory in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca "Abstract Factory in Go") ![Abstract Factory in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e "Abstract Factory in Java") ![Abstract Factory in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618 "Abstract Factory in PHP") ![Abstract Factory in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f "Abstract Factory in Python") ![Abstract Factory in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb "Abstract Factory in Ruby") ![Abstract Factory in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d "Abstract Factory in Swift") ![Abstract Factory in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7 "Abstract Factory in TypeScript")

![Banner promoting an eBook 'Dive Into Design Patterns' with access to code examples for various IDEs.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [GUI Elements Factory](#example-0)

 gui

  [lib](#example-0--gui-lib-rs)

 macos-gui

  [lib](#example-0--macos-gui-lib-rs)

 windows-gui

  [lib](#example-0--windows-gui-lib-rs)

 app

  [main](#example-0--app-main-rs)

  [render](#example-0--app-render-rs)

 app-dyn

  [main](#example-0--app-dyn-main-rs)

  [render](#example-0--app-dyn-render-rs)

![An illustration depicting various user interface elements (like a tablet, smartphone, and desktop screen showing GUI components) surrounded by development tools (like a hammer, wrench, code snippets, and gears), symbolizing software development, design, and system architecture.](image.png)