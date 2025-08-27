```yaml
---
title: "Repository pattern (the real deal) | by Bananica Bananica | Aug, 2025 | Medium"
source: https://medium.com/@bananicabananica/repository-pattern-the-real-deal-94218155c0ba
date_published: 2025-08-20T14:15:58.415Z
date_captured: 2025-08-22T10:49:00.749Z
domain: medium.com
author: Bananica Bananica
category: general
technologies: [Entity Framework, SQLite, ADO.NET, .NET]
programming_languages: [C#, SQL]
tags: [repository-pattern, unit-of-work, data-access, sqlite, ado.net, csharp, database, architecture, design-patterns, orm-alternatives]
key_concepts: [repository-pattern, unit-of-work, abstraction, dependency-injection, connection-pooling, transactional-behavior, low-level-data-access, native-aot]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article critically examines the Repository and Unit of Work patterns, arguing against their superficial implementation over ORMs like Entity Framework. It demonstrates a "real deal" implementation from scratch, showcasing how to abstract data access without an ORM. The author provides a comprehensive C# example using SQLite and ADO.NET, covering interfaces for repositories and UoW, a custom "DbContext" equivalent, connection pooling, and transactional accessor logic. The piece highlights the complexity of building such patterns natively and suggests their renewed relevance with technologies like Native AOT.]
---
```

# Repository pattern (the real deal) | by Bananica Bananica | Aug, 2025 | Medium

# Repository pattern (the real deal)

![A laptop with its screen replaced by a wooden filing cabinet, one drawer pulled out revealing yellow folders, symbolizing data storage and retrieval.](https://miro.medium.com/v2/resize:fit:700/1*ulFt54y3Hjnr0DCOdia3RA.png)

In one of my previous articles, I’ve [written](/@bananicabananica/repository-over-entityframework-is-dumb-32a8d2ca58d0) why repository pattern over EntityFramework (EF) is a dumb thing to do. While there are minor exceptions (covered in the article), most online examples do exactly the same thing, usually followed by some dumb checklist of pros and cons.

Not here. Have you ever tried to implement repository pattern without EF or at least find such an example? There aren’t many and it’s gonna look wildly different. I’d like to spare you the trouble.

Since most C# devs are familiar with EF, I will present an example that resembles it. So Repository + Unit of Work (UoW). No generic repositories because… Well… You’ll see.

# A quick recap

EF does a lot of things for us. It has repositories `DbSet<T>`, UoW `DbContext`, change tracking and even handling relations. Change tracking and relation handling are out of scope as well. These aren’t trivial problems that can fit into one article, and I’m not an expert on the subject. Hell, I’m not an expert on this subject either, which should convince you not to blindly believe in anything you read.

I’m providing the simplest possible solution, not a solution _already implemented by EF masked behind an interface_.

Repositories abstract data store, whatever it may be. UoW adds transactional behavior, so for this example,