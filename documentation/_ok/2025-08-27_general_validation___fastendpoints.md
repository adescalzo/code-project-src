```yaml
---
title: "Validation | FastEndpoints"
source: https://fast-endpoints.com/docs/validation#request-dto-validation
date_published: unknown
date_captured: 2025-08-27T14:57:35.425Z
domain: fast-endpoints.com
author: Unknown
category: general
technologies: [FluentValidation, FastEndpoints, .NET, ASP.NET Core]
programming_languages: [C#]
tags: [validation, fluentvalidation, fastendpoints, dotnet, web-api, data-transfer-object, error-handling, dependency-injection, model-validation, business-logic]
key_concepts: [request-validation, model-validation, business-logic-validation, dependency-injection, data-transfer-object, singleton-pattern, error-handling, custom-validation]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article details various aspects of request and application logic validation within the FastEndpoints framework, primarily leveraging FluentValidation. It covers how to define validation rules for Data Transfer Objects (DTOs) using `Validator<TRequest>` and how FastEndpoints automatically handles error responses. The content also explains how to disable automatic failure responses for more granular control over validation outcomes. Furthermore, it delves into implementing business logic validations within endpoint handlers and managing errors from any part of the application using `ValidationContext`. Finally, it touches upon advanced topics like abstract validators, duplicate validator handling, and the integration of DataAnnotations.]
---
```

# Validation | FastEndpoints

# Validation

## Request DTO Validation

Request validation is done using [FluentValidation](https://fluentvalidation.net/) rules. Please refer to the FluentValidation website if you haven't used it before. Just make sure to import it first (or add a `global using FluentValidation;` in **Program.cs**) before writing any validators.

> You don't need to install the FluentValidation package as it's automatically brought in by FastEndpoints.

Simply write your validators by inheriting the **Validator<TRequest>** base class like below.

You don't need to register your validators with the DI container. That is automatically taken care of by FastEndpoints.

Request.cs

```csharp
public class CreateUserRequest
{
    public string FullName { get; set; }
    public int Age { get; set; }
}
```

MyValidator.cs

```csharp
public class MyValidator : Validator<CreateUserRequest>
{
    public MyValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("your name is required!")
            .MinimumLength(5)
            .WithMessage("your name is too short!");

        RuleFor(x => x.Age)
            .NotEmpty()
            .WithMessage("we need your age!")
            .GreaterThan(18)
            .WithMessage("you are not legal yet!");
    }
}
```

If a request is received that doesn't meet the above model validation criteria, a 400 bad request response will be sent to the client automatically with the following json body describing the error details:

json

```json
{
  "StatusCode": 400,
  "Message": "One or more errors occured!",
  "Errors": {
    "FullName": [
      "your name is required!",
      "your name is too short!"
    ],
    "Age": [
      "we need your age!",
      "you are not legal yet!"
    ]
  }
}
```

The format of the error response is customizable as described in the [configuration settings](configuration-settings#customizing-error-responses) page.

## Disable Automatic Failure Response

In cases where you need more control of the validations, you can turn off the default behavior by calling the **DontThrowIfValidationFails()** method in the endpoint configuration like so:

CreateUserEndpoint.cs

```csharp
public class CreateUserEndpoint : Endpoint<CreateUserRequest>
{
    public override void Configure()
    {
        Post("/api/user/create");
        DontThrowIfValidationFails();
    }
}
```

Doing so will not send an automatic error response to the client and your handler will be executed. You can check the validation status by looking at the **ValidationFailures** property of the handler like so:

```csharp
public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
{
    if (ValidationFailed)
    {
        foreach (ValidationFailure failure in ValidationFailures)
        {
            var propertyName = failure.PropertyName;
            var errorMessage = failure.ErrorMessage;
        }
    }
    await Send.OkAsync();
}
```

## Application Logic Validation

In cases where there are app/business logic validation failures during the processing of a request in the handler, you can send an error response to the client like so:

CreateUserEndpoint.cs

```csharp
public class CreateUserEndpoint : Endpoint<CreateUserRequest, CreateUserResponse>
{
    public override void Configure() => Post("/api/user/create");

    public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
    {
        bool userExists = await userRepo.UserAlreadyExists(req.EmailAddress);
        if (userExists)
            AddError(r => r.EmailAddress, "this email is already in use!");

        var maxAge = await userRepo.GetMaxAllowedAge();
        if (req.Age >= maxAge)
            AddError(r => r.Age, "you are not eligible for insurance!");

        ThrowIfAnyErrors(); // If there are errors, execution shouldn't go beyond this point

        var userID = await userRepo.CreateNew(req);
        if (userID is null)
            ThrowError("creating a user did not go so well!"); // Error response sent here

        await Send.OkAsync(new()
        {
            UserID = userID,
            FullName = req.FullName
        });
    }
}
```

**AddError()** - This method adds a validation failure to the **ValidationFailures** property of the handler.

**ThrowIfAnyErrors()** - This method will cause the handler execution to be halted when called and an error response will be sent to the client **IF** there are any validation failures in the **ValidationFailures** list. If there's none, execution will proceed to the next line.

**ThrowError()** - This method will abort execution immediately and send an error response to the client.

### Throwing/Adding Errors From Anywhere

You can have the same error adding/throwing functionality from anywhere in your application by obtaining an instance of the **ValidationContext** as shown below. Manipulating the endpoint error state like this could be helpful when you need to add/throw errors from deep within your domain layers instead of passing down the **ValidationFailures** list of the endpoint.

```csharp
//typed validation context
var validationCtx = ValidationContext<Request>.Instance;
validationCtx.AddError(r => r.Id, "Bad identity!");
validationCtx.ThrowError(r => r.Id, "Whatever...");

//untyped validation context
var valCtx = ValidationContext.Instance;
valCtx.AddError("SomeOtherProp", "Blah Blah!");
valCtx.ThrowIfAnyErrors();
```

[See here](https://gist.github.com/dj-nitehawk/a3e673479c8f3fb3660cb837f9032031) for an example of this.

## Abstract Validator Classes

If for some reason you'd like to auto register validators inheriting **FluentValidation.AbstractValidator<T>**, you must instruct FastEndpoints at startup like so:

```csharp
bld.Services.AddFastEndpoints(o => o.IncludeAbstractValidators = true);
```

Doing so will include any validator implementing the **FluentValidation.IValidator** interface in the registration.

### Duplicate Validators

If there are duplicate validators discovered for the exact same request DTO in your solution, an exception will be thrown during app startup. When that happens, you need to instruct FastEndpoints which exact validator you want to use by specifying it in the endpoint configuration like so:

```csharp
public override void Configure()
{
    Get("test");
    Validator<MyValidator>();
}
```

> You can specify the validator in the endpoint config like above even if there are no duplicates in your solution and you'd just like to be explicit about which validator is being used by the endpoint.

---

## Dependency Injection

Validators are used as singletons for [performance reasons](/benchmarks). I.e. there will only ever be one instance of a validator type. All requests to an endpoint will use that single instance for validating the incoming request. You should not maintain state in your validators. If you need to resolve scoped dependencies in your validators, you may do so as shown [here](dependency-injection#validator-dependencies).

---

> For simple input validation, you can use **DataAnnotations** (for ex: **[Required]**, **[StringLength]**, etc.) on request DTOs instead of creating a FluentValidation validator. However, only one validation strategy is valid per endpoint. If both are present, only the fluent validator runs. Annotation support must be enabled like so:
>
> ```csharp
> app.UseFastEndpoints(c => c.Validation.EnableDataAnnotationsSupport = true)
> ```