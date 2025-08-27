```yaml
---
title: Proxy
source: https://refactoring.guru/design-patterns/proxy
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:19:36.320Z
domain: refactoring.guru
author: Unknown
category: general
technologies: []
programming_languages: [C#, C++, Go, Java, PHP, Python, Ruby, Rust, Swift, TypeScript]
tags: [design-patterns, structural-patterns, proxy-pattern, software-design, object-oriented-programming, lazy-initialization, caching, access-control, software-architecture]
key_concepts: [proxy-pattern, structural-design-patterns, lazy-initialization, access-control, caching, remote-proxy, logging-proxy, delegation]
code_examples: false
difficulty_level: intermediate
summary: |
  The Proxy design pattern is a structural pattern that provides a substitute or placeholder for another object, controlling access to it. It allows for performing actions before or after a request reaches the original object without modifying the service class itself. Common applications include lazy initialization of heavyweight objects, implementing access control, caching results, logging requests, or handling remote service calls. The proxy implements the same interface as the original service, enabling transparent interchangeability for clients. This pattern helps to manage the service object's lifecycle and add functionality while adhering to the Open/Closed Principle.
---
```

# Proxy

# Proxy

## Intent

**Proxy** is a structural design pattern that lets you provide a substitute or placeholder for another object. A proxy controls access to the original object, allowing you to perform something either before or after the request gets through to the original object.

![A large, complex object (server/database) is shown behind a fence with barbed wire, while several smaller, simpler objects (clients) queue to access it through a single gate. Another large object is relaxing by a pool, symbolizing resource consumption and the problem of direct access to a resource-intensive object.](images/patterns/content/proxy/proxy.png)

## Problem

Why would you want to control access to an object? Here is an example: you have a massive object that consumes a vast amount of system resources. You need it from time to time, but not always.

![Multiple "Client" boxes send direct, wavy requests to a "DATABASE" cylinder, illustrating potentially slow or inefficient direct access.](images/patterns/diagrams/proxy/problem-en.png)

Database queries can be really slow.

You could implement lazy initialization: create this object only when it’s actually needed. All of the object’s clients would need to execute some deferred initialization code. Unfortunately, this would probably cause a lot of code duplication.

In an ideal world, we’d want to put this code directly into our object’s class, but that isn’t always possible. For instance, the class may be part of a closed 3rd-party library.

## Solution

The Proxy pattern suggests that you create a new proxy class with the same interface as an original service object. Then you update your app so that it passes the proxy object to all of the original object’s clients. Upon receiving a request from a client, the proxy creates a real service object and delegates all the work to it.

![Multiple "Client" boxes send requests to a "Proxy" cylinder. The Proxy then sends a single, wavy request to the "DATABASE" cylinder, indicating that the proxy mediates and potentially optimizes access.](images/patterns/diagrams/proxy/solution-en.png)

The proxy disguises itself as a database object. It can handle lazy initialization and result caching without the client or the real database object even knowing.

But what’s the benefit? If you need to execute something either before or after the primary logic of the class, the proxy lets you do this without changing that class. Since the proxy implements the same interface as the original class, it can be passed to any client that expects a real service object.

## Real-World Analogy

![A credit card and a bundle of cash are depicted, with an arrow from the credit card to the cash, and a "Payment" interface above them. This illustrates how a credit card acts as a proxy for cash, both fulfilling the "Payment" interface.](images/patterns/diagrams/proxy/live-example.png)

Credit cards can be used for payments just the same as cash.

A credit card is a proxy for a bank account, which is a proxy for a bundle of cash. Both implement the same interface: they can be used for making a payment. A consumer feels great because there’s no need to carry loads of cash around. A shop owner is also happy since the income from a transaction gets added electronically to the shop’s bank account without the risk of losing the deposit or getting robbed on the way to the bank.

## Structure

![UML-like class diagram showing the core components of the Proxy pattern: Client, ServiceInterface (interface implemented by both Proxy and Service), Service (the real object), and Proxy (which holds a reference to the Service and delegates calls).](images/patterns/diagrams/proxy/structure.png)
![UML-like class diagram showing the core components of the Proxy pattern with indexed labels for detailed explanation.](images/patterns/diagrams/proxy/structure-indexed.png)

1.  The **Service Interface** declares the interface of the Service. The proxy must follow this interface to be able to disguise itself as a service object.

2.  The **Service** is a class that provides some useful business logic.

3.  The **Proxy** class has a reference field that points to a service object. After the proxy finishes its processing (e.g., lazy initialization, logging, access control, caching, etc.), it passes the request to the service object.

    Usually, proxies manage the full lifecycle of their service objects.

4.  The **Client** should work with both services and proxies via the same interface. This way you can pass a proxy into any code that expects a service object.

## Pseudocode

This example illustrates how the **Proxy** pattern can help to introduce lazy initialization and caching to a 3rd-party YouTube integration library.

![UML-like diagram specific to the YouTube caching example, showing `YouTubeManager` interacting with `ThirdPartyYouTubeLib` interface, which is implemented by `ThirdPartyYouTubeClass` (the service) and `CachedYouTubeClass` (the proxy).](images/patterns/diagrams/proxy/example.png)

Caching results of a service with a proxy.

The library provides us with the video downloading class. However, it’s very inefficient. If the client application requests the same video multiple times, the library just downloads it over and over, instead of caching and reusing the first downloaded file.

The proxy class implements the same interface as the original downloader and delegates it all the work. However, it keeps track of the downloaded files and returns the cached result when the app requests the same video multiple times.

```
// The interface of a remote service.
interface ThirdPartyYouTubeLib is
    method listVideos()
    method getVideoInfo(id)
    method downloadVideo(id)

// The concrete implementation of a service connector. Methods
// of this class can request information from YouTube. The speed
// of the request depends on a user's internet connection as
// well as YouTube's. The application will slow down if a lot of
// requests are fired at the same time, even if they all request
// the same information.
class ThirdPartyYouTubeClass implements ThirdPartyYouTubeLib is
    method listVideos() is
        // Send an API request to YouTube.

    method getVideoInfo(id) is
        // Get metadata about some video.

    method downloadVideo(id) is
        // Download a video file from YouTube.

// To save some bandwidth, we can cache request results and keep
// them for some time. But it may be impossible to put such code
// directly into the service class. For example, it could have
// been provided as part of a third party library and/or defined
// as `final`. That's why we put the caching code into a new
// proxy class which implements the same interface as the
// service class. It delegates to the service object only when
// the real requests have to be sent.
class CachedYouTubeClass implements ThirdPartyYouTubeLib is
    private field service: ThirdPartyYouTubeLib
    private field listCache, videoCache
    field needReset

    constructor CachedYouTubeClass(service: ThirdPartyYouTubeLib) is
        this.service = service

    method listVideos() is
        if (listCache == null || needReset)
            listCache = service.listVideos()
        return listCache

    method getVideoInfo(id) is
        if (videoCache == null || needReset)
            videoCache = service.getVideoInfo(id)
        return videoCache

    method downloadVideo(id) is
        if (!downloadExists(id) || needReset)
            service.downloadVideo(id)

// The GUI class, which used to work directly with a service
// object, stays unchanged as long as it works with the service
// object through an interface. We can safely pass a proxy
// object instead of a real service object since they both
// implement the same interface.
class YouTubeManager is
    protected field service: ThirdPartyYouTubeLib

    constructor YouTubeManager(service: ThirdPartyYouTubeLib) is
        this.service = service

    method renderVideoPage(id) is
        info = service.getVideoInfo(id)
        // Render the video page.

    method renderListPanel() is
        list = service.listVideos()
        // Render the list of video thumbnails.

    method reactOnUserInput() is
        renderVideoPage()
        renderListPanel()

// The application can configure proxies on the fly.
class Application is
    method init() is
        aYouTubeService = new ThirdPartyYouTubeClass()
        aYouTubeProxy = new CachedYouTubeClass(aYouTubeService)
        manager = new YouTubeManager(aYouTubeProxy)
        manager.reactOnUserInput()
```

## Applicability

There are dozens of ways to utilize the Proxy pattern. Let’s go over the most popular uses.

Lazy initialization (virtual proxy). This is when you have a heavyweight service object that wastes system resources by being always up, even though you only need it from time to time.

Instead of creating the object when the app launches, you can delay the object’s initialization to a time when it’s really needed.

Access control (protection proxy). This is when you want only specific clients to be able to use the service object; for instance, when your objects are crucial parts of an operating system and clients are various launched applications (including malicious ones).

The proxy can pass the request to the service object only if the client’s credentials match some criteria.

Local execution of a remote service (remote proxy). This is when the service object is located on a remote server.

In this case, the proxy passes the client request over the network, handling all of the nasty details of working with the network.

Logging requests (logging proxy). This is when you want to keep a history of requests to the service object.

The proxy can log each request before passing it to the service.

Caching request results (caching proxy). This is when you need to cache results of client requests and manage the life cycle of this cache, especially if results are quite large.

The proxy can implement caching for recurring requests that always yield the same results. The proxy may use the parameters of requests as the cache keys.

Smart reference. This is when you need to be able to dismiss a heavyweight object once there are no clients that use it.

The proxy can keep track of clients that obtained a reference to the service object or its results. From time to time, the proxy may go over the clients and check whether they are still active. If the client list gets empty, the proxy might dismiss the service object and free the underlying system resources.

The proxy can also track whether the client had modified the service object. Then the unchanged objects may be reused by other clients.

## How to Implement

1.  If there’s no pre-existing service interface, create one to make proxy and service objects interchangeable. Extracting the interface from the service class isn’t always possible, because you’d need to change all of the service’s clients to use that interface. Plan B is to make the proxy a subclass of the service class, and this way it’ll inherit the interface of the service.

2.  Create the proxy class. It should have a field for storing a reference to the service. Usually, proxies create and manage the whole life cycle of their services. On rare occasions, a service is passed to the proxy via a constructor by the client.

3.  Implement the proxy methods according to their purposes. In most cases, after doing some work, the proxy should delegate the work to the service object.

4.  Consider introducing a creation method that decides whether the client gets a proxy or a real service. This can be a simple static method in the proxy class or a full-blown factory method.

5.  Consider implementing lazy initialization for the service object.

## Pros and Cons

*   You can control the service object without clients knowing about it.
*   You can manage the lifecycle of the service object when clients don’t care about it.
*   The proxy works even if the service object isn’t ready or is not available.
*   _Open/Closed Principle_. You can introduce new proxies without changing the service or clients.

*   The code may become more complicated since you need to introduce a lot of new classes.
*   The response from the service might get delayed.

## Relations with Other Patterns

*   With [Adapter](/design-patterns/adapter) you access an existing object via different interface. With [Proxy](/design-patterns/proxy), the interface stays the same. With [Decorator](/design-patterns/decorator) you access the object via an enhanced interface.

*   [Facade](/design-patterns/facade) is similar to [Proxy](/design-patterns/proxy) in that both buffer a complex entity and initialize it on its own. Unlike _Facade_, _Proxy_ has the same interface as its service object, which makes them interchangeable.

*   [Decorator](/design-patterns/decorator) and [Proxy](/design-patterns/proxy) have similar structures, but very different intents. Both patterns are built on the composition principle, where one object is supposed to delegate some of the work to another. The difference is that a _Proxy_ usually manages the life cycle of its service object on its own, whereas the composition of _Decorators_ is always controlled by the client.

## Code Examples

[![Proxy in C#](images/patterns/icons/csharp.svg)](/design-patterns/proxy/csharp/example "Proxy in C#") [![Proxy in C++](images/patterns/icons/cpp.svg)](/design-patterns/proxy/cpp/example "Proxy in C++") [![Proxy in Go](images/patterns/icons/go.svg)](/design-patterns/proxy/go/example "Proxy in Go") [![Proxy in Java](images/patterns/icons/java.svg)](/design-patterns/proxy/java/example "Proxy in Java") [![Proxy in PHP](images/patterns/icons/php.svg)](/design-patterns/proxy/php/example "Proxy in PHP") [![Proxy in Python](images/patterns/icons/python.svg)](/design-patterns/proxy/python/example "Proxy in Python") [![Proxy in Ruby](images/patterns/icons/ruby.svg)](/design-patterns/proxy/ruby/example "Proxy in Ruby") [![Proxy in Rust](images/patterns/icons/rust.svg)](/design-patterns/proxy/rust/example "Proxy in Rust") [![Proxy in Swift](images/patterns/icons/swift.svg)](/design-patterns/proxy/swift/example "Proxy in Swift") [![Proxy in TypeScript](images/patterns/icons/typescript.svg)](/design-patterns/proxy/typescript/example "Proxy in TypeScript")