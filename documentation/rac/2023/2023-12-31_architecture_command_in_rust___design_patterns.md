```yaml
---
title: Command in Rust / Design Patterns
source: https://refactoring.guru/design-patterns/command/rust/example#example-0
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:28:48.160Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Rust, cursive]
programming_languages: [Rust]
tags: [design-patterns, command-pattern, behavioral-pattern, rust, undo-redo, text-editor, tui, software-architecture]
key_concepts: [command-design-pattern, behavioral-patterns, undo-redo-functionality, application-context, rust-traits, mutable-references]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Command behavioral design pattern, explaining how it encapsulates requests or operations into objects for deferred or remote execution and history tracking. It provides a practical implementation in Rust, demonstrating how to build a simple text editor with copy, cut, paste, and undo functionalities. The example highlights Rust-specific considerations, such as passing application context as a mutable parameter rather than holding permanent global references. Detailed Rust code snippets for the Command trait and concrete command implementations (Copy, Cut, Paste) are included, alongside the client code for the text editor's main application logic.]
---
```

# Command in Rust / Design Patterns

[Design Patterns](/design-patterns) / [Command](/design-patterns/command) / [Rust](/design-patterns/rust)

![Command Pattern Icon](images/patterns/cards/command-mini.png?id=b149eda017c0583c1e92343b83cfb1eb "Icon representing the Command design pattern with various operations encapsulated as objects.")

# Command in Rust

**Command** is a behavioral design pattern that converts requests or simple operations into objects.

The conversion allows deferred or remote execution of commands, storing command history, etc.

[Learn more about Command](/design-patterns/command)

### Navigation

*   [Intro](#)
*   [Text Editor: Commands and Undo](#text-editor-commands-and-undo)
*   [command.rs](#commandrs-command-inteface)
*   [command/copy.rs](#commandcopyrs-copy-command)
*   [command/cut.rs](#commandcutrs-cut-command)
*   [command/paste.rs](#commandpasters-paste-command)
*   [main.rs](#mainrs-client-code)

In Rust, a command instance should *NOT hold a permanent reference to global context*, instead the latter should be passed *from top to down as a mutable parameter* of the “`execute`” method:

```rust
fn execute(&mut self, app: &mut cursive::Cursive) -> bool;
```

## Text Editor: Commands and Undo

Key points:

*   Each button runs a separate command.
*   Because a command is represented as an object, it can be pushed into a `history` array in order to be undone later.
*   TUI is created with `cursive` crate.

### command.rs: Command Interface

```rust
mod copy;
mod cut;
mod paste;

pub use copy::CopyCommand;
pub use cut::CutCommand;
pub use paste::PasteCommand;

/// Declares a method for executing (and undoing) a command.
///
/// Each command receives an application context to access
/// visual components (e.g. edit view) and a clipboard.
pub trait Command {
    fn execute(&mut self, app: &mut cursive::Cursive) -> bool;
    fn undo(&mut self, app: &mut cursive::Cursive);
}
```

### command/copy.rs: Copy Command

```rust
use cursive::{views::EditView, Cursive};

use super::Command;
use crate::AppContext;

#[derive(Default)]
pub struct CopyCommand;

impl Command for CopyCommand {
    fn execute(&mut self, app: &mut Cursive) -> bool {
        let editor = app.find_name::<EditView>("Editor").unwrap();
        let mut context = app.take_user_data::<AppContext>().unwrap();

        context.clipboard = editor.get_content().to_string();

        app.set_user_data(context);
        false
    }

    fn undo(&mut self, _: &mut Cursive) {}
}
```

### command/cut.rs: Cut Command

```rust
use cursive::{views::EditView, Cursive};

use super::Command;
use crate::AppContext;

#[derive(Default)]
pub struct CutCommand {
    backup: String,
}

impl Command for CutCommand {
    fn execute(&mut self, app: &mut Cursive) -> bool {
        let mut editor = app.find_name::<EditView>("Editor").unwrap();

        app.with_user_data(|context: &mut AppContext| {
            self.backup = editor.get_content().to_string();
            context.clipboard = self.backup.clone();
            editor.set_content("".to_string());
        });

        true
    }

    fn undo(&mut self, app: &mut Cursive) {
        let mut editor = app.find_name::<EditView>("Editor").unwrap();
        editor.set_content(&self.backup);
    }
}
```

### command/paste.rs: Paste Command

```rust
use cursive::{views::EditView, Cursive};

use super::Command;
use crate::AppContext;

#[derive(Default)]
pub struct PasteCommand {
    backup: String,
}

impl Command for PasteCommand {
    fn execute(&mut self, app: &mut Cursive) -> bool {
        let mut editor = app.find_name::<EditView>("Editor").unwrap();

        app.with_user_data(|context: &mut AppContext| {
            self.backup = editor.get_content().to_string();
            editor.set_content(context.clipboard.clone());
        });

        true
    }

    fn undo(&mut self, app: &mut Cursive) {
        let mut editor = app.find_name::<EditView>("Editor").unwrap();
        editor.set_content(&self.backup);
    }
}
```

### main.rs: Client code

```rust
mod command;

use cursive::{
    traits::Nameable,
    views::{Dialog, EditView},
    Cursive,
};

use command::{Command, CopyCommand, CutCommand, PasteCommand};

/// An application context to be passed into visual component callbacks.
/// It contains a clipboard and a history of commands to be undone.
#[derive(Default)]
struct AppContext {
    clipboard: String,
    history: Vec<Box<dyn Command>>,
}

fn main() {
    let mut app = cursive::default();

    app.set_user_data(AppContext::default());
    app.add_layer(
        Dialog::around(EditView::default().with_name("Editor"))
            .title("Type and use buttons")
            .button("Copy", |s| execute(s, CopyCommand))
            .button("Cut", |s| execute(s, CutCommand::default()))
            .button("Paste", |s| execute(s, PasteCommand::default()))
            .button("Undo", undo)
            .button("Quit", |s| s.quit()),
    );

    app.run();
}

/// Executes a command and then pushes it to a history array.
fn execute(app: &mut Cursive, mut command: impl Command + 'static) {
    if command.execute(app) {
        app.with_user_data(|context: &mut AppContext| {
            context.history.push(Box::new(command));
        });
    }
}

/// Pops the last command and executes an undo action.
fn undo(app: &mut Cursive) {
    let mut context = app.take_user_data::<AppContext>().unwrap();
    if let Some(mut command) = context.history.pop() {
        command.undo(app)
    }
    app.set_user_data(context);
}
```

## Output

![Text Editor screenshot](data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAA8AAAAJKCAYAAADjiiyRAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAFiUAABYlAUlSJPAAADfASURBVPhe7d0PnFV1nfj/N4qDKaLyR/lnBKICuWCoiKZUK2Wh9QP3m9KWYClsKm3+qU10NzVL3FotE7QVNdG2oD9i35T0F9QKVghCQCojKIjyV4Y/4kjMSPi9M3MGZoaZYWZghsHP8/l4XDnnzJnx3M+9c+e+7jn33BYz7i56NwAAAOA97qDsXwAAAHhPE8AAAAAkQQADAACQBAEMAABAEgQwAAAASRDAAAAAJEEAAwAAkAQBDAAAQBIEMAAAAEkQwAAAACRBAAMAAJAEAQwAAEASBDAAAABJEMAAAAAkQQADAACQBAEMAABAEgQwAAAASRDAAAAAJEEAAwAAkAQBDAAAQBIEMAAAAEkQwAAAACRBAAMAAJAEAQwAAEASBDAAAABJEMAAAAAkQQADAACQBAEMAABAEgQwAAAASRDAAAAAJEEAAwAAkAQBDAAAQBIEMAAAAEkQwAAAACRBAAMAAJAEAQwAAEASBDAAAABJEMAAAAAkQQADAACQBAEMAABAEgQwAAAASRDAAAAAJEEAAwAAkAQBDAAAQBIEMAAAAEkQwAAAACRBAAMAAJAEAQwAAEASBDAAAABJEMAAAAAkQQADAACQBAEMAABAEgQwAAAASRDAAAAAJEEAAwAAkAQBDAAAQBIEMAAAAEkQwAAAACRBAAMAAJAEAQwAAEASBDAAAABJEMAAAAAkQQADAACQBAEMAABAEgQwAAAASRDAAAAAJEEAAwAAkAQBDAAAQBIEMAAAAEkQwAAAACRBAAMAAJAEAQwAAEASBDAAAABJEMAAAAAkQQADAACQBAEMAABAEgQwAAAASRDAAAAAJEEAAwAAkAQBDAAAQBIEMAAAAEkQwAAAACRBAAMAAJAEAQwAAEASBDAAAABJEMAAAAAkQQADAACQBAEMAABAEgQwAAAASRDAAAAAJEEAAwAAkAQBDAAAQBIEMAAAAEkQwAAAACRBAAMAAJAEAQwAAEASBDAAAABJEMAAAAAkQQADAACQBAEMAABAEgQwAAAASRDAAAAAJEEAAwAAkAQBDAAAQBIEMAAAAEkQwAAAACRBAAMAAJAEAQwAAEASBDAAAABJEMAAAAAkQQADAACQBAEMAABAEgQwAAAASRDAAAAAJEEAAwAAkAQBDAAAQBIEMAAAAEkQwAAAACRBAAMAAJAEAQwAAEASBDAAAABJEMAAAAAkQQADAACQBAEMAABAEgQwAAAASRDAAAAAJEEAAwAAkAQBDAAAQBIEMAAAAEkQwAAAACRBAAMAAJAEAQwAAEASBDAAAABJEMAAAAAkQQADAACQBAEMAABAEgQwAAAASRDAAAAAJEEAAwAAkAQBDAAAQBIEMAAAAEkQwAAAACRBAAMAAJAEAQwAAEASBDAAAABJEMAAAAAkQQADAACQBAEMAABAEgQwAAAASRDAAAAAJEEAAwAAkAQBDAAAQBIEMAAAAEkQwAAAACRBAAMAAJAEAQwAAEASBDAAAABJEMAAAAAkQQADAACQBAEMAABAEgQwAAAASRDAAAAAJEEAAwAAkAQBDAAAQBIEMAAAAEkQwAAAACRBAAMAAJAEAQwAAEASBDAAAABJEMAAAAAkQQADAACQBAEMAABAEgQwAAAASRDAAAAAJEEAAwAAkAQBDAAAQBIEMAAAAEkQwAAAACRBAAMAAJAEAQwAAEASBDAAAABJEMAAAAAkQQADAACQBAEMAABAEgQwAAAASRDAAAAAJEEAAwAAkAQBDAAAQBIEMAAAAEkQwAAAACRBAAMAAJAEAQwAAEASBDAAAABJEMAAAAAkQQADAACQBAEMAABAEgQwAAAASRDAAAAAJEEAAwAAkAQBDAAAQBIEMAAAAEkQwAAAACRBAAMAAJAEAQwAAEASBDAAAABJEMAAAAAkQQADAACQBAEMAABAEgQwAAAASRDAAAAAJEEAAwAAkAQBDAAAQBIEMAAAAEkQwAAAACRBAAMAAJAEAQwAAEASBDAAAABJEMAAAAAkQQADAACQBAEMAABAEgQwAAAASRDAAAAAJEEAAwAAkAQBDAAAQBIEMAAAAEkQwAAAACRBAAMAAJAEAQwAAEASBDAAAABJEMAAAAAkQQADAACQBAEMAABAEgQwAAAASRDAAAAAJEEAAwAAkAQBDAAAQBIEMAAAAEkQwAAAACRBAAMAAJAEAQwAAEASBDAAAABJEMAAAAAkQQADAACQBAEMAABAEgQwAAAASRDAAAAAJEEAAwAAkAQBDAAAQBIEMAAAAEkQwAAAACRBAAMAAJAEAQwAAEASBDAAAABJEMAAAAAkQQADAACQgIj/Bwiv3SlPndWAAAAAAElFTkSuQmCC "Screenshot of the Rust TUI text editor application, showing an input field with 'Hello, World!' and buttons for Copy, Cut, Paste, Undo, and Quit.")

#### Read next

[Iterator in Rust](/design-patterns/iterator/rust/example)

#### Return

[Chain of Responsibility in Rust](/design-patterns/chain-of-responsibility/rust/example)

## Command in Other Languages

[![Command in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58 "C# language icon")](/design-patterns/command/csharp/example "Command in C#") [![Command in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858 "C++ language icon")](/design-patterns/command/cpp/example "Command in C++") [![Command in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca "Go language icon")](/design-patterns/command/go/example "Command in Go") [![Command in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e "Java language icon")](/design-patterns/command/java/example "Command in Java") [![Command in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618 "PHP language icon")](/design-patterns/command/php/example "Command in PHP") [![Command in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f "Python language icon")](/design-patterns/command/python/example "Command in Python") [![Command in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb "Ruby language icon")](/design-patterns/command/ruby/example "Command in Ruby") [![Command in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d "Swift language icon")](/design-patterns/command/swift/example "Command in Swift") [![Command in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7 "TypeScript language icon")](/design-patterns/command/typescript/example "Command in TypeScript")

![Examples in IDE](images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc "Banner promoting an ebook with design pattern examples that can be opened in an IDE.")

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)