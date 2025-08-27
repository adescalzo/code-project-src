```yaml
---
title: Factory Method in Rust / Design Patterns
source: https://refactoring.guru/design-patterns/factory-method/rust/example#example-1
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:30:27.180Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Rust, HTML, Windows]
programming_languages: [Rust]
tags: [design-patterns, factory-method, creational-patterns, rust, dynamic-dispatch, static-dispatch, gui, game-development, software-design]
key_concepts: [Factory Method pattern, creational design patterns, dynamic dispatch, static dispatch, Rust traits, generics, GUI framework design]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explains the Factory Method design pattern, a creational pattern that allows object creation without specifying concrete classes. It demonstrates the pattern with two detailed Rust examples: a GUI framework using dynamic dispatch via trait objects, and a maze game utilizing static dispatch with generics. The examples illustrate how subclasses can override a factory method to produce different product types, showcasing the flexibility of the pattern. The content also highlights the differences between dynamic and static dispatch in Rust within the context of design patterns.]
---
```

# Factory Method in Rust / Design Patterns

[](/)/ [Design Patterns](/design-patterns) / [Factory Method](/design-patterns/factory-method) / [Rust](/design-patterns/rust)

![Factory Method Icon](images/patterns/cards/factory-method-mini.png?id=72619e9527893374b98a5913779ac167)
*Description: A graphical icon representing the Factory Method design pattern, typically used as a card or thumbnail.*

# **Factory Method** in Rust

**Factory method** is a creational design pattern which solves the problem of creating product objects without specifying their concrete classes.

The Factory Method defines a method, which should be used for creating objects instead of using a direct constructor call (`new` operator). Subclasses can override this method to change the class of objects that will be created.

> If you can’t figure out the difference between various factory patterns and concepts, then read our [Factory Comparison](/design-patterns/factory-comparison).

[Learn more about Factory Method](/design-patterns/factory-method)

Navigation

 [Intro](#)

 [Dialog Rendering](#example-0)

 [gui](#example-0--gui-rs)

 [html\_gui](#example-0--html_gui-rs)

 [windows\_gui](#example-0--windows_gui-rs)

 [init](#example-0--init-rs)

 [main](#example-0--main-rs)

 [Maze Game](#example-1)

 [game](#example-1--game-rs)

 [magic\_maze](#example-1--magic_maze-rs)

 [ordinary\_maze](#example-1--ordinary_maze-rs)

 [main](#example-1--main-rs)

[Dialog Rendering](#example-0) [Maze Game](#example-1)

## Dialog Rendering

This example illustrates how to organize a GUI framework into independent modules using **dynamic dispatch**:

1.  The `gui` module defines interfaces for all the components.  
    It has no external dependencies.
2.  The `html_gui` module provides HTML implementation of the base GUI.  
    Depends on `gui`.
3.  The `windows_gui` module provides Windows implementation of the base GUI.  
    Depends on `gui`.

The `app` is a client application that can use several implementations of the GUI framework, depending on the current environment or configuration. However, most of the app code doesn’t depend on specific types of GUI elements. All client code works with GUI elements through abstract interfaces defined by the `gui` module.

The [Abstract Factory example](/design-patterns/abstract-factory/rust/example) demonstrates an even greater separation of a factory interface and its implementations.

#### [](#example-0--gui-rs)**gui.rs:** Product & Creator

```rust
pub trait Button {
    fn render(&self);
    fn on_click(&self);
}

/// Dialog has a factory method `create_button`.
///
/// It creates different buttons depending on a factory implementation.
pub trait Dialog {
    /// The factory method. It must be overridden with a concrete implementation.
    fn create_button(&self) -> Box<dyn Button>;

    fn render(&self) {
        let button = self.create_button();
        button.render();
    }

    fn refresh(&self) {
        println!("Dialog - Refresh");
    }
}
```

#### [](#example-0--html_gui-rs)**html\_gui.rs:** Concrete creator

```rust
use crate::gui::{Button, Dialog};

pub struct HtmlButton;

impl Button for HtmlButton {
    fn render(&self) {
        println!("<button>Test Button</button>");
        self.on_click();
    }

    fn on_click(&self) {
        println!("Click! Button says - 'Hello World!'");
    }
}

pub struct HtmlDialog;

impl Dialog for HtmlDialog {
    /// Creates an HTML button.
    fn create_button(&self) -> Box<dyn Button> {
        Box::new(HtmlButton)
    }
}
```

#### [](#example-0--windows_gui-rs)**windows\_gui.rs:** Another concrete creator

```rust
use crate::gui::{Button, Dialog};

pub struct WindowsButton;

impl Button for WindowsButton {
    fn render(&self) {
        println!("Drawing a Windows button");
        self.on_click();
    }

    fn on_click(&self) {
        println!("Click! Hello, Windows!");
    }
}

pub struct WindowsDialog;

impl Dialog for WindowsDialog {
    /// Creates a Windows button.
    fn create_button(&self) -> Box<dyn Button> {
        Box::new(WindowsButton)
    }
}
```

#### [](#example-0--init-rs)**init.rs:** Initialization code

```rust
use crate::gui::Dialog;
use crate::html_gui::HtmlDialog;
use crate::windows_gui::WindowsDialog;

pub fn initialize() -> &'static dyn Dialog {
    // The dialog type is selected depending on the environment settings or configuration.
    if cfg!(windows) {
        println!("-- Windows detected, creating Windows GUI --");
        &WindowsDialog
    } else {
        println!("-- No OS detected, creating the HTML GUI --");
        &HtmlDialog
    }
}
```

#### [](#example-0--main-rs)**main.rs:** Client code

```rust
mod gui;
mod html_gui;
mod init;
mod windows_gui;

use init::initialize;

fn main() {
    // The rest of the code doesn't depend on specific dialog types, because
    // it works with all dialog objects via the abstract `Dialog` trait
    // which is defined in the `gui` module.
    let dialog = initialize();
    dialog.render();
    dialog.refresh();
}
```

### Output

```
<button>Test Button</button>
Click! Button says - 'Hello World!'
Dialog - Refresh
```

## Maze Game

This example illustrates how to implement the Factory Method pattern using **static dispatch** (generics).

_Inspired by the Factory Method [example from the GoF book](https://en.wikipedia.org/wiki/Factory_method_pattern)._

#### [](#example-1--game-rs)**game.rs**

```rust
/// Maze room that is going to be instantiated with a factory method.
pub trait Room {
    fn render(&self);
}

/// Maze game has a factory method producing different rooms.
pub trait MazeGame {
    type RoomImpl: Room;

    /// A factory method.
    fn rooms(&self) -> Vec<Self::RoomImpl>;

    fn play(&self) {
        for room in self.rooms() {
            room.render();
        }
    }
}

/// The client code initializes resources and does other preparations
/// then it uses a factory to construct and run the game.
pub fn run(maze_game: impl MazeGame) {
    println!("Loading resources...");
    println!("Starting the game...");

    maze_game.play();
}
```

#### [](#example-1--magic_maze-rs)**magic\_maze.rs**

```rust
use super::game::{MazeGame, Room};

#[derive(Clone)]
pub struct MagicRoom {
    title: String,
}

impl MagicRoom {
    pub fn new(title: String) -> Self {
        Self { title }
    }
}

impl Room for MagicRoom {
    fn render(&self) {
        println!("Magic Room: {}", self.title);
    }
}

pub struct MagicMaze {
    rooms: Vec<MagicRoom>,
}

impl MagicMaze {
    pub fn new() -> Self {
        Self {
            rooms: vec![
                MagicRoom::new("Infinite Room".into()),
                MagicRoom::new("Red Room".into()),
            ],
        }
    }
}

impl MazeGame for MagicMaze {
    type RoomImpl = MagicRoom;

    fn rooms(&self) -> Vec<Self::RoomImpl> {
        self.rooms.clone()
    }
}
```

#### [](#example-1--ordinary_maze-rs)**ordinary\_maze.rs**

```rust
use super::game::{MazeGame, Room};

#[derive(Clone)]
pub struct OrdinaryRoom {
    id: u32,
}

impl OrdinaryRoom {
    pub fn new(id: u32) -> Self {
        Self { id }
    }
}

impl Room for OrdinaryRoom {
    fn render(&self) {
        println!("Ordinary Room: #{}", self.id);
    }
}

pub struct OrdinaryMaze {
    rooms: Vec<OrdinaryRoom>,
}

impl OrdinaryMaze {
    pub fn new() -> Self {
        Self {
            rooms: vec![OrdinaryRoom::new(1), OrdinaryRoom::new(2)],
        }
    }
}

impl MazeGame for OrdinaryMaze {
    type RoomImpl = OrdinaryRoom;

    fn rooms(&self) -> Vec<Self::RoomImpl> {
        let mut rooms = self.rooms.clone();
        rooms.reverse();
        rooms
    }
}
```

#### [](#example-1--main-rs)**main.rs:** Client code

```rust
mod game;
mod magic_maze;
mod ordinary_maze;

use magic_maze::MagicMaze;
use ordinary_maze::OrdinaryMaze;

/// The game runs with different mazes depending on the concrete factory type:
/// it's either an ordinary maze or a magic maze.
///
/// For demonstration purposes, both mazes are used to construct the game.
fn main() {
    // Option 1: The game starts with an ordinary maze.
    let ordinary_maze = OrdinaryMaze::new();
    game::run(ordinary_maze);

    // Option 2: The game starts with a magic maze.
    let magic_maze = MagicMaze::new();
    game::run(magic_maze);
}
```

### Output

```
Loading resources...
Starting the game...
Magic Room: Infinite Room
Magic Room: Red Room
Loading resources...
Starting the game...
Ordinary Room: #2
Ordinary Room: #1
```

[Dialog Rendering](#example-0) [Maze Game](#example-1)

#### Read next

[Prototype in Rust](/design-patterns/prototype/rust/example) 

#### Return

 [Builder in Rust](/design-patterns/builder/rust/example)

## **Factory Method** in Other Languages

[![Factory Method in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58)](/design-patterns/factory-method/csharp/example "Factory Method in C#") [![Factory Method in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/factory-method/cpp/example "Factory Method in C++") [![Factory Method in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/factory-method/go/example "Factory Method in Go") [![Factory Method in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/factory-method/java/example "Factory Method in Java") [![Factory Method in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/factory-method/php/example "Factory Method in PHP") [![Factory Method in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f)](/design-patterns/factory-method/python/example "Factory Method in Python") [![Factory Method in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/factory-method/ruby/example "Factory Method in Ruby") [![Factory Method in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/factory-method/swift/example "Factory Method in Swift") [![Factory Method in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/factory-method/typescript/example "Factory Method in TypeScript")

![Promotional Banner for Design Patterns Ebook](images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)
*Description: A promotional banner image depicting various development tools and code snippets on a tablet, laptop, and mobile phone, advertising an ebook with design pattern examples.*

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Dialog Rendering](#example-0)

 [gui](#example-0--gui-rs)

 [html\_gui](#example-0--html_gui-rs)

 [windows\_gui](#example-0--windows_gui-rs)

 [init](#example-0--init-rs)

 [main](#example-0--main-rs)

 [Maze Game](#example-1)

 [game](#example-1--game-rs)

 [magic\_maze](#example-1--magic_maze-rs)

 [ordinary\_maze](#example-1--ordinary_maze-rs)

 [main](#example-1--main-rs)