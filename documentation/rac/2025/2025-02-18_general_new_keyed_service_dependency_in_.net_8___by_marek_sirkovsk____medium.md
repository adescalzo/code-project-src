```yaml
---
title: "New keyed service dependency in .NET 8 | by Marek Sirkovský | Medium"
source: https://mareks-082.medium.com/keyed-service-dependency-in-net-8-20a1c9d08e48
date_published: 2025-02-18T06:07:52.408Z
date_captured: 2025-08-06T17:48:32.294Z
domain: mareks-082.medium.com
author: Marek Sirkovský
category: general
technologies: [.NET 8, ASP.NET Core, Structuremap, Autofac, Entity Framework, Ninject]
programming_languages: [C#]
tags: [dependency-injection, dotnet, keyed-services, ab-testing, feature-toggles, configuration-management, performance, software-design, web-api, di-container]
key_concepts: [keyed services, dependency injection, A/B testing, feature toggles, configuration management, service lifetime, single responsibility principle, service locator]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces and explores the new "keyed service" dependency injection feature in .NET 8. It explains the concept of registering and resolving services by an additional key, providing practical use cases such as A/B testing, configuration management, handling different service lifetimes, and entity-driven resolution. The author also thoroughly discusses the potential downsides, including increased configuration complexity, runtime errors due to lack of type safety, performance overhead, and the risk of misuse leading to anti-patterns like Service Locator. The article concludes by recommending cautious adoption of keyed services, acknowledging their utility while emphasizing the added complexity they introduce.
---
```

# New keyed service dependency in .NET 8 | by Marek Sirkovský | Medium

Top highlight

# **New keyed service dependency in .NET 8**

[

![Marek Sirkovský](https://miro.medium.com/v2/resize:fill:64:64/1*sd6GN4VkST6HdJW7e5xMKQ.jpeg)

](/?source=post_page---byline--20a1c9d08e48---------------------------------------)

[Marek Sirkovský](/?source=post_page---byline--20a1c9d08e48---------------------------------------)

Following

8 min read

·

Nov 18, 2023

281

6

Listen

Share

More

Zoom image will be displayed

![A close-up, top-down view of multiple house keys arranged in a grid pattern on a dark, possibly black, surface. Some keys are silver, and others are white, creating a visual contrast. The keys are oriented similarly, with their heads facing upwards and teeth downwards.](https://miro.medium.com/v2/resize:fit:700/1*1xoavKDNoGQVRa196OCwKw.jpeg)

Photo by [rc.xyz NFT gallery](https://unsplash.com/@moneyphotos?utm_content=creditCopyText&utm_medium=referral&utm_source=unsplash) on [Unsplash](https://unsplash.com/photos/a-bunch-of-keys-sitting-on-top-of-a-table-q7h8LVeUgFU?utm_content=creditCopyText&utm_medium=referral&utm_source=unsplash)

The latest version of .NET has finally introduced the concept of a “keyed service” support for the dependency injection container. The built-in DI container in .NET 8 now includes what other DI containers like [Structuremap](https://structuremap.github.io/) and [Autofac](https://autofac.org/) have had for a while.

# What’s a keyed service

The “keyed” or “named” registration is a pattern where dependencies are not only registered by their type but also with an additional key. Take a look at the following example that illustrates the operation of keyed services in practice:

```csharp
public interface IService {}
  
public class ServiceA: IService {}
public class ServiceB: IService {}
  
container.Register<IService, ServiceA>("keyA");
container.Register<IService, ServiceB>("keyB");
  
// You need to use a key to get a correct implementation
var myServiceA = container.Resolve<IService>("keyA");
var myServiceB = container.Resolve<IService>("keyB");
```

That short introduction doesn't fully show the complexity of the new DI keyed functionality. For more information, read [Andrew Lock's article](https://andrewlock.net/exploring-the-dotnet-8-preview-keyed-services-dependency-injection-support/). In this post, I'd rather focus on the advantages, disadvantages, and hidden effects of the new DI pattern.

# Use cases for keyed service

So we will soon get a new shine tool. Do we really need it?

Actually, you don't. It's just a minor addition to the dependency registration and resolving mechanism. However, it might play nicely with a few standard use cases. Let me give you some scenarios where keyed dependency injection may come in handy.

## **A/B Testing or Feature Toggles**

A keyed service can manage [feature toggles](https://martinfowler.com/articles/feature-toggles.html) or [A/B testing](https://vwo.com/ab-testing/), providing different users or user groups with distinct feature sets.

In the following example, I implemented a simple random generator for A/B testing. There are two implementations, _BehaviorA_ and _BehaviorB._ I want to _use BehaviorA_ for 50 percent of controller calls and _BehaviorB_ for the other 50 percent.

```csharp
// the startup class:
builder.Services.AddKeyedTransient<IBehavior, BehaviorA>(0);
builder.Services.AddKeyedTransient<IBehavior, BehaviorB>(1);
  
builder.Services.AddTransient<IBehavior>(serviceProvider =>
{
    var number = new Random().Next(2);
    return serviceProvider.GetRequiredKeyedService<IBehavior>(number);
});
  
[ApiController]
[Route("[controller]")]
public class ABTestingController : ControllerBase
{
    private readonly IBehavior _behavior;
  
    public ABTestingController(IBehavior behavior)
    {
        _behavior = behavior;
    }
  
    [HttpGet]
    public string DoSomething()
    {
        return _behavior.DoSomething();
    }
}
  
public interface IBehavior
{
    string DoSomething();
}
  
public class BehaviorA : IBehavior
{
    public string DoSomething()
    {
        return "A";
    }
}
  
public class BehaviorB : IBehavior
{
    public string DoSomething()
    {
        return "B";
    }
}
```

The trick is that I registered _IBehavior_ three times. Two times as keyed service — for keys “0” and “1”. The third one is a standard transient registration using an implementation factory. This one is also used in the _ABTestingController_.

It’s handy because:

1) Dynamic values are not allowed in attributes. We can’t use this pattern:

```csharp
public ABTestingController(
[FromKeyedServices(new Random().Next(2))] IKeyedServiceProvider keyedServiceProvider)
{
   ...
```

2) Another reason is that after you finish testing, you can easily replace the factory:

```csharp
builder.Services.AddTransient<IBehavior>(serviceProvider =>
```

by class:

```csharp
builder.Services.AddTransient<IBehavior, BehaviorA>()
```

Or _BehaviorB_, it depends on the result of testing.

_Side note:_

**Single responsibility principle.**
You might have realized that the keyed services would help with the well-known (for many people’s notorious) pattern of the single responsibility principle.

Without keyed service, you need to write code similar to this:

```csharp
public class OverloadedBehavior : IBehavior
{
    public string DoSomething()
    {
       var number = new Random().Next(2);
       return number == 0 ? "A" : "B";
    }
}
```

I know this example is contrived, but imagine more complex logic in the method _DoSomething_. The keyed services seem perfectly suited for this particular use case.

## Configuration Management

A keyed service can manage configurations for different parts of an app and modules or environments like staging and production. The key is used to look up the relevant configuration. It’s like the A/B testing, but in this case, you utilize data from the environment variables.

```csharp
builder.Services
  .AddKeyedTransient<IEmailSender, SmtpEmailSender>("production");
  
builder.Services
  .AddKeyedTransient<IEmailSender, FakeEmailSender>("non-production");
  
builder.Services.AddTransient<IEmailSender>(serviceProvider =>
{
    var env = serviceProvider.GetRequiredService<IHostingEnvironment>();
    var key = env.IsDevelopment() ? "non-production" : "production";
    return serviceProvider.GetRequiredKeyedService<IEmailSender>(key);
});
  
public interface IEmailSender
{
    void SendEmail();
}
  
public class SmtpEmailSender : IEmailSender
{
    public void SendEmail()
    {
        /*send a regular email*/
    }
}
public class FakeEmailSender : IEmailSender
{
    public void SendEmail()
    {
        /*do nothing*/
   }
}
  
public EnvController(IEmailSender sender)
{
    _sender = sender;
}
```

The main logic lies in registering the _IEmailSender_. I used _IHostingEnvironment.IsDevelopment_ property instead of generating a random number.

## **Dealing with the lifetime**

Keyed services are handy when you need different lifetimes of the same dependency. The resolving of the [Entity Framework DbContext](https://learn.microsoft.com/en-us/dotnet/api/system.data.entity.dbcontext?view=entity-framework-6.2.0) is a great example. In complex applications, you might need DbContext with different lifetimes. Keyed service allows you to introduce the following pattern:

```csharp
Services.AddTransient<EntityContext>();
services.AddKeyedScoped<EntityContext>("scoped");
  
public Controller1([FromKeyedServices("scoped")] EntityContext dbContext)
{   
   // scoped dbContext
}
  
public Controller2(EntityContext dbContext)
{
  // transient dbContext
}
```

Without the support for keyed service, you’d have to introduce a _DBContextFactory_ with a similar method like the following:

```csharp
// DbContetxFactory has to be registered as scoped
public class DbContetxFactory
{
  public EntityContext CreateTransientDbContext()
  {
     // returns a new transient instance
     return new EntityContext // omitted for clarity
  }
     
  private EntityContext? _scopedDbContext;
  public EntityContext CreateScopedDbContext()
  {
    // omitted for clarity
    return _scopedDbContext ?? (_scopedDbContext = new ...)
  }
}
```

Again, it’s a very handy pattern.

## **Entity-driven resolving**

The craziest way to utilize keyed service is probably entity-driven resolving. Entity-driven resolving involves saving the key into the database table and using it for service resolving. In the following example, we have two payment processors, Stripe and Paypal.

```csharp
public class PayPalProcessor : IPaymentProcessor { /* … */ }
public class StripeProcessor : IPaymentProcessor { /* … */ }
  
  
builder.Services
  .AddKeyedTransient<IPaymentProcessor, PayPalProcessor>("PayPal");
builder.Services
  .AddKeyedTransient<IPaymentProcessor, StripeProcessor>("Stripe");
  
[ApiController]
[Route("[controller]")]
public class PaymentController : ControllerBase{
  private readonly IKeyedServiceProvider _keyedServiceProvider;
  public PaymentController(IKeyedServiceProvider keyedServiceProvider)
  {
    _keyedServiceProvider = keyedServiceProvider;
  }
  
[HttpGet]
public string ProcessPayment(int orderId){
  var order = FetchOrder(orderId);
  var payment= _keyedServiceProvider
    .GetRequiredKeyedService<IPaymentProcessor>(order.TypeOfPayment);
  var request= order.GetPaymentRequest();
  payment.Process(request);
  return "Payment processed";
}
```

The key to the type of payment processor is saved in the _Order_ table in the database and fetched in the method _FetchOrder_. The keys(Stripe and PayPal constants) are used when registering services. The main issue here is when the column in DB contains something different than Stripe or PayPal. Then, the app throws a runtime error.

While the idea may seem a bit risky and unconventional, there is also potential for it to be an extremely flexible way of resolving services.

# Is it all sunshine and rainbows?

Now, everyone may have an idea of how to utilize keyed services in your codebase. But before starting to add the keyed service everywhere, let’s examine its downsides.

## **Complex Configuration**

Developers new to the project may face a steep learning curve due to the potential complexity of the configuration, particularly in larger projects with multiple dependencies. Taming the dependency injection container proved to be quite a challenging task. The more ways you use the DI configuration, the more complex your app will be.

## **Runtime Errors**

Misconfigurations very often result in runtime errors that are considerably harder to troubleshoot. Such errors occur at runtime instead of during compilation if a key is misspelled or if a corresponding dependency for a key is not registered.

Let’s see an example. If you make a typo in registering or resolving, .NET 8 shows you the following error:

```csharp
// registration:
builder.Services.AddKeyedTransient<IPaymentProcessor, StripeProcessor>("Stripe");
  
// typo in a capital letter
keyedServiceProvider.GetKeyedService<INotificationService>("stripe");
  
// the error:
Unhandled exception. System.InvalidOperationException:   
No service for type 'IPaymentProcessor' has been registered.
```

The message is incomplete and provides misleading info on what is wrong. Service for type _IPaymentProcessor_ has been registered but under a different key.

## **Lack of Type safety**

Relying on keys, which could be strings or other basic types, usually compromises type safety and increases the likelihood of errors within the system. This is particularly true if a key is misspelled or if its corresponding dependency is not correctly registered, which could again lead to difficult-to-troubleshoot runtime errors. Example:

```csharp
var b = serviceProvider
  .GetRequiredKeyedService<IPaymentProcessor>("pay"+"pal");
```

Combination writing code in this manner with misleading error messages leads to a troubleshooting nightmare.

## **Overuse or Misuse**

As usual, it is tempting to rely too heavily on the dependency container, using it as a catch-all solution for dependency management. However, this approach can result in (anti)-patterns like [Service Locator](https://www.codeproject.com/Articles/5337102/Service-Locator-Pattern-in-Csharp), which ultimately lead to difficult-to-maintain code. Example of this potential misuse:

```csharp
interface IHandler {}
  
class StandardOrderProcessor : IHandler{}
  
class VatExludedOrderProcessor : IHandler{}
  
class SaveOrderHandler : IHandler{}
  
  
// Then you can call it like:
serviceProvider.GetKeyedService<IHandler>("VatExludedOrderProcessor");
serviceProvider.GetKeyedService<IHandler>("SaveOrderHandler");
```

_VatExludedOrderProcessor_, _SaveOrderHandler_, and _StandardOrderProcessor_ are completely different functionality, so I don’t think it’s fine to use the same interface(_IHandler_).

## **Managing keys**

Firstly, any .NET object can be used as a key, which can result in various issues. I usually prefer to use either classic strings or enum values.

Using string keys without abstracting them to constants spreads “**magic strings**” throughout the code. That can be challenging to maintain and prone to errors. Enum values look more appealing, but they also come with their own set of problems. Using enums raises questions like do you need one big enum or a couple of separate enums? If so, where should these separated enums live?

Moreover, this is only the ice of the iceberg. If you plan to use a lot of keys, you need to think about their validation and resolving duplicity. For example, can you guess what happens when you override registration, like in the following code:

```csharp
builder.Services
  .AddKeyedTransient<IPaymentProcessor, PayPalProcessor>("PayPal");
  
builder.Services
  .AddKeyedTransient<IPaymentProcessor, StripeProcessor>("Stripe");
  
// "PayPal" returns StripeProcessor.
builder.Services
  .AddKeyedTransient<IPaymentProcessor, StripeProcessor>("PayPal");
  
app.Services
  .GetKeyedService<IPaymentProcessor>("PayPal").Process(new Request());
```

.NET 8 When you call this code, you get the latter service (_StripeProcessor_). Unfortunately, there is no validation for duplicity built-in in the current version of .NET.

## **Performance Overhead**

Dependency resolution at runtime can introduce a performance overhead, especially in a keyed container with numerous dependencies. During my recent testing, I evaluated the potential performance impact of using keyed services. The code:

```csharp
public class OrderProcessor : IOrderProcessor
{
  public void Process()
  {
  }
}
  
public class StripeProcessor : IPaymentProcessor
{
  public void Process(IRequest request)
  {
  }
}
  
public class PerfTests
{
  private ServiceProvider _provider;
   
  [GlobalSetup]
  public void Setup()
  {
   var serviceCollection = new ServiceCollection();
   serviceCollection
     .AddKeyedTransient<IPaymentProcessor, StripeProcessor>("Stripe");
   serviceCollection
     .AddTransient<IOrderProcessor, OrderProcessor>();
  
   _provider = serviceCollection.BuildServiceProvider();
  }
  
[Benchmark]
 public object Keyed() => _provider
   .GetKeyedServices<IPaymentProcessor>("Stripe");
  
 [Benchmark]
 public object Normal() => _provider
   .GetServices<StripeProcessor>();
}
```

The results were:

| Method |   Mean    |   Error  |  StdDev  |
|--------|-----------|----------|----------|
| Keyed  | 101.83 ns | 1.951 ns | 1.825 ns |
| Normal |  11.15 ns | 0.264 ns | 0.247 ns |

The performance of keyed services on my machine is **nine times slower** than the standard resolving. On the other hand, the performance degradation is in nanoseconds, which is acceptable for most standard applications. Yet, if you are chasing milliseconds, you may need to be concerned.

## **Codebase Consistency**

Ensuring consistency across the codebase in how dependencies are registered and resolved can be challenging, especially in larger teams or projects. It’s a similar problem when language brings a new keyword. Do you recall when async/await was first introduced? It made our codebase legacy and created a need for the gradual adaption of this new pattern.

The software industry is young. If you want to innovate, you must pay the price of constant inconsistencies and tech debt tickets in your backlog.

## Is keyed services the final piece that we’ve been waiting for?

(Un)Fortunately, not at all. There are more complex DI containers out there. For instance, [Ninject](https://github.com/ninject/Ninject) offers not only keyed services but also other [constrained resolution mechanisms](https://github.com/ninject/ninject/wiki/Contextual-Binding#other-constrained-resolution-mechanisms). You can utilize attributes or target classes for even more complex service graphs. However, it may be good that Microsoft is conservative here and isn’t adding a new feature to their DI container every six months.

## **All in all**

Keyed services have multiple ideal use cases, such as A/B testing or life management, but they introduce additional complexity. So, I’d recommend you definitely try to use it, but be cautious while doing so. I still believe the more complex the resolving of dependencies, the more complex the application becomes.