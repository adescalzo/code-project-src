```yaml
---
title: Proxy in Rust / Design Patterns
source: https://refactoring.guru/design-patterns/proxy/rust/example#example-0
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:29:13.047Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Nginx]
programming_languages: [Rust]
tags: [design-patterns, proxy-pattern, rust, web-server, nginx, rate-limiting, caching, structural-pattern, software-design]
key_concepts: [proxy-design-pattern, structural-design-pattern, access-control, caching, rate-limiting, service-object, interface, conceptual-example]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces the Proxy structural design pattern, defining it as an object that acts as a substitute for a real service object, handling client requests by performing tasks like access control or caching before forwarding them. It emphasizes that the proxy shares the same interface as the service, making it interchangeable. A conceptual example demonstrates Nginx acting as a proxy for an application server, illustrating features like rate limiting and controlled access. The article provides detailed Rust code examples for implementing the `Server` trait and `NginxServer` to showcase the pattern's practical application.
---
```

# Proxy in Rust / Design Patterns

[](/)/ [Design Patterns](/design-patterns) / [Proxy](/design-patterns/proxy) / [Rust](/design-patterns/rust)

![Proxy](/images/patterns/cards/proxy-mini.png?id=25890b11e7dc5af29625ccd0678b63a8)

# **Proxy** in Rust

**Proxy** is a structural design pattern that provides an object that acts as a substitute for a real service object used by a client. A proxy receives client requests, does some work (access control, caching, etc.) and then passes the request to a service object.

The proxy object has the same interface as a service, which makes it interchangeable with a real object when passed to a client.

[Learn more about Proxy](/design-patterns/proxy)

Navigation

 [Intro](#)

 [Conceptual Example: Nginx Proxy](#example-0)

 [server](#example-0--server-rs)

  [application](#example-0--server-application-rs)

  [nginx](#example-0--server-nginx-rs)

 [main](#example-0--main-rs)

## Conceptual Example: Nginx Proxy

A web server such as Nginx can act as a proxy for your application server:

*   It provides controlled access to your application server.
*   It can do rate limiting.
*   It can do request caching.

#### [](#example-0--server-rs)**server.rs**

```rust
mod application;
mod nginx;

pub use nginx::NginxServer;

pub trait Server {
    fn handle_request(&mut self, url: &str, method: &str) -> (u16, String);
}
```

#### [](#example-0--server-application-rs)**server/application.rs**

```rust
use super::Server;

pub struct Application;

impl Server for Application {
    fn handle_request(&mut self, url: &str, method: &str) -> (u16, String) {
        if url == "/app/status" && method == "GET" {
            return (200, "Ok".into());
        }

        if url == "/create/user" && method == "POST" {
            return (201, "User Created".into());
        }

        (404, "Not Ok".into())
    }
}
```

#### [](#example-0--server-nginx-rs)**server/nginx.rs**

```rust
use std::collections::HashMap;

use super::{application::Application, Server};

/// NGINX server is a proxy to an application server.
pub struct NginxServer {
    application: Application,
    max_allowed_requests: u32,
    rate_limiter: HashMap<String, u32>,
}

impl NginxServer {
    pub fn new() -> Self {
        Self {
            application: Application,
            max_allowed_requests: 2,
            rate_limiter: HashMap::default(),
        }
    }

    pub fn check_rate_limiting(&mut self, url: &str) -> bool {
        let rate = self.rate_limiter.entry(url.to_string()).or_insert(1);

        if *rate > self.max_allowed_requests {
            return false;
        }

        *rate += 1;
        true
    }
}

impl Server for NginxServer {
    fn handle_request(&mut self, url: &str, method: &str) -> (u16, String) {
        if !self.check_rate_limiting(url) {
            return (403, "Not Allowed".into());
        }

        self.application.handle_request(url, method)
    }
}
```

#### [](#example-0--main-rs)**main.rs**

```rust
mod server;

use crate::server::{NginxServer, Server};

fn main() {
    let app_status = &"/app/status".to_string();
    let create_user = &"/create/user".to_string();

    let mut nginx = NginxServer::new();

    let (code, body) = nginx.handle_request(app_status, "GET");
    println!("Url: {}\nHttpCode: {}\nBody: {}\n", app_status, code, body);

    let (code, body) = nginx.handle_request(app_status, "GET");
    println!("Url: {}\nHttpCode: {}\nBody: {}\n", app_status, code, body);

    let (code, body) = nginx.handle_request(app_status, "GET");
    println!("Url: {}\nHttpCode: {}\nBody: {}\n", app_status, code, body);

    let (code, body) = nginx.handle_request(create_user, "POST");
    println!("Url: {}\nHttpCode: {}\nBody: {}\n", create_user, code, body);

    let (code, body) = nginx.handle_request(create_user, "GET");
    println!("Url: {}\nHttpCode: {}\nBody: {}\n", create_user, code, body);
}
```

### Output

```
Url: /app/status
HttpCode: 200
Body: Ok

Url: /app/status
HttpCode: 200
Body: Ok

Url: /app/status
HttpCode: 403
Body: Not Allowed

Url: /create/user
HttpCode: 201
Body: User Created

Url: /create/user
HttpCode: 404
Body: Not Ok
```

#### Read next

[Chain of Responsibility in Rust](/design-patterns/chain-of-responsibility/rust/example) 

#### Return

 [Flyweight in Rust](/design-patterns/flyweight/rust/example)

## **Proxy** in Other Languages

[![Proxy in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58)](/design-patterns/proxy/csharp/example "Proxy in C#") [![Proxy in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/proxy/cpp/example "Proxy in C++") [![Proxy in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/proxy/go/example "Proxy in Go") [![Proxy in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/proxy/java/example "Proxy in Java") [![Proxy in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/proxy/php/example "Proxy in PHP") [![Proxy in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f)](/design-patterns/proxy/python/example "Proxy in Python") [![Proxy in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/proxy/ruby/example "Proxy in Ruby") [![Proxy in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/proxy/swift/example "Proxy in Swift") [![Proxy in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/proxy/typescript/example "Proxy in TypeScript")

[![](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)](/design-patterns/book)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Conceptual Example: Nginx Proxy](#example-0)

 [server](#example-0--server-rs)

  [application](#example-0--server-application-rs)

  [nginx](#example-0--server-nginx-rs)

 [main](#example-0--main-rs)

### Image Analysis:

1.  **Image 1 Description:** A minimalist diagram illustrating the Proxy design pattern. It shows a small grey square (representing a client or request) pointing with an arrow to a larger red 'C' shape that encloses another small grey square (representing the real service object). This visually conveys the concept of a proxy wrapping or controlling access to a service.
2.  **Image 2 Description:** A vibrant, abstract illustration depicting various elements related to software development and web services. It features a central tablet displaying code snippets and user interface elements, surrounded by a diverse collection of icons representing programming concepts, development tools, and digital devices (such as a mobile phone, gears, code brackets, a hammer, and data charts). The overall composition suggests a comprehensive development environment or a collection of practical code examples.