```yaml
---
title: Wolverine as Mediator in .NET - by Muhammad Waseem
source: https://mwaseemzakir.substack.com/p/wolverine-as-mediator-in-net?utm_source=post-email-title&publication_id=1232416&post_id=162126274&utm_campaign=email-post-title&isFreemail=true&r=a97lu&triedRedirect=true
date_published: 2025-04-26T04:14:13.000Z
date_captured: 2025-08-08T12:35:43.925Z
domain: mwaseemzakir.substack.com
author: Muhammad Waseem
category: ai_ml
technologies: [Wolverine, .NET 8, ASP.NET Core Web API, MediatR, NuGet, FluentValidation]
programming_languages: [C#]
tags: [dotnet, mediator-pattern, message-bus, wolverine, web-api, nuget, validation, dependency-injection]
key_concepts: [Mediator Pattern, Message Bus, Dependency Injection, Request-Response Pattern, Validation, Naming Conventions, In-process Messaging]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces Wolverine as a free, next-generation .NET mediator and message bus, positioning it as an alternative to MediatR following its commercialization announcement. It guides developers through setting up a .NET 8 Web API project and integrating Wolverine. The post demonstrates Wolverine's convention-based approach for defining and handling messages using the `IMessageBus` interface. Additionally, it covers how to incorporate validation using Wolverine's built-in FluentValidation package, emphasizing the simplicity and reduced boilerplate. The article concludes by showing how to configure Wolverine to only include its core in-process mediator capabilities for a leaner application.
---
```

# Wolverine as Mediator in .NET - by Muhammad Waseem

# Wolverine as Mediator in .NET

With the recent news that MediatR will become commercial in its upcoming versions, developers might be considering alternatives. I respect Jimmy Bogard's decision and wish him the best; however, if you are an individual, not a company, and are wondering how to navigate this change, today’s article will be helpful for you.

We will explore how to use this awesome NuGet package, free to use as of the date I am writing this article.

Wolverine is a _Next Generation .NET Mediator and Message Bus_ available as a free nuget package on nuget package manager store.

Let’s start with a fresh .NET 8 Web API :

Name your project:

I am using .NET 8 and choosing the controllers option :

Install the wolverine package :

Register the dependency of this package :

Let’s keep this super simple:

We want to send a “greeting” message and get back a friendly hello. In Wolverine, you don’t implement any special interfaces; everything happens by naming conventions.

First, define a plain request object that carries the user’s name. Then, create a public handler class whose name ends in _Handler (or Consumer)_ and give it a single method that ends in _Handle (or Consume)_. Inside that method, you write your greeting logic.

To send a message from our controller, we use the `IMessageBus` interface provided by the package. We inject `IMessageBus` into the controller’s constructor and then invoke it like this:

When you call `bus.InvokeAsync` With your request, Wolverine scans its known assemblies for any public class named `SomethingHandler` (or `SomethingConsumer`) and looks for a matching `Handle(...)` (or `Consume(...)`) method that takes your request type. It then invokes that method and returns the result no interfaces, no manual registrations, no extra boilerplate.

If your request returns a response, then we must specify it next to `InvokeAsync<ResponseType>.` Let’s try our request :

Ok but this is not enough we might need to add the fluent validation as well, so we can use this nugget come from same library :

You don’t need to install `FluentValidation` separately this packages does it under the hood :

Time to add some validation :

Here we add and modify our dependencies by adding FluentValidator as an option :

And here our validator comes into action :

That looks ugly, but we can do that some day later to make it look better.

Wolverine is packed with powerful features, durable outbox, scheduled delivery, sagas, and more but if you only need its in-process mediator capabilities, you can trim the extras in your `Program.cs`. By registering just the core mediator services, you avoid pulling in unnecessary modules and keep your application lean and efficient.