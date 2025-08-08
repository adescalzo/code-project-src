## Summary
This article explores the refactoring journey from using Transaction Scripts to Domain Models in software development, illustrated with a fitness tracking app example. Initially, Transaction Scripts offer simplicity for basic features, but they lead to bloated, hard-to-maintain code as business logic grows. The solution involves adopting a Domain Model, which encapsulates behavior and data within domain objects, improving code expressiveness, reusability, and testability. The author recommends a pragmatic approach: start with Transaction Scripts for simple applications and refactor to a Domain Model when complexity increases.

---

# From Transaction Scripts to Domain Models: A Refactoring Journey

```markdown
**Source:** https://www.milanjovanovic.tech/blog/from-transaction-scripts-to-domain-models-a-refactoring-journey?utm_source=newsletter&utm_medium=email&utm_campaign=tnw94
**Date Captured:** 2025-07-28T19:41:12.153Z
**Domain:** www.milanjovanovic.tech
**Author:** Milan Jovanović
**Category:** ai_ml
```

---

![Blog cover image for 'From Transaction Scripts to Domain Models: A Refactoring Journey' showing a blue background with geometric shapes and the text 'DOMAIN MODELS AND TRANSACTION SCRIPTS' and 'MJ Tech' logo.](/blog-covers/mnw_094.png)

5 min read · June 15, 2024

Supercharge your [**API development with AI**](https://blog.postman.com/announcing-postman-v11/). Postman v11 introduces many new features for collaboration, AI, and security. Try out the new features like the VS Code extension, Partner Workspaces, and Postman Vault. [**Learn more here**](https://blog.postman.com/announcing-postman-v11/).

Introducing [**Shesha**](https://shesha.io?utm_source=milan&utm_medium=email&utm_campaign=20240615), a brand new, open-source, low-code framework for .NET developers. Create business applications faster and with >80% less code! [**Learn more here.**](https://shesha.io?utm_source=milan&utm_medium=email&utm_campaign=20240615)

[Sponsor this newsletter](/sponsor-the-newsletter)

I once led the development of a fitness tracking app. We started with Transaction Scripts to handle features like workout creation and exercise logging. It was a simple and effective approach for the app's early stages.

However, as we added more complex features, our business logic became bloated. New business rules intertwined with existing logic, making the code difficult to maintain. Each change could introduce unintended consequences.

We solved our problem by introducing a Domain Model. The domain model shifts the focus from procedures to domain objects. Our code became more expressive, easier to reason about, and less prone to errors.

This experience taught me a valuable lesson, that I want to share in this newsletter.

## Transaction Script

At its core, business applications operate through distinct interactions (transactions) with their users. These transactions can range from simple data retrieval to complex operations involving multiple validations, calculations, and updates to the system's database.

The Transaction Script pattern provides a simple way to encapsulate the logic behind each transaction. It organizes all the necessary steps, from data access to business rules, into a single, self-contained procedure.

> Organizes business logic by procedures where each procedure handles a single request from the presentation.
>
> — [Transaction Script](https://martinfowler.com/eaaCatalog/transactionScript.html), Patterns of Enterprise Application Architecture

Here's an example of adding exercises to a workout:

```csharp
internal sealed class AddExercisesCommandHandler(
    IWorkoutRepository workoutRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddExercisesCommand>
{
    public async Task<Result> Handle(
        AddExercisesCommand request,
        CancellationToken cancellationToken)
    {
        Workout? workout = await workoutRepository.GetByIdAsync(
            request.WorkoutId,
            cancellationToken);

        if (workout is null)
        {
            return Result.Failure(WorkoutErrors.NotFound(request.WorkoutId));
        }

        List<Error> errors = [];
        foreach (ExerciseRequest exerciseDto in request.Exercises)
        {
            if (exerciseDto.TargetType == TargetType.Distance &&
                exerciseDto.DistanceInMeters is null)
            {
                errors.Add(ExerciseErrors.MissingDistance);

                continue;
            }

            if (exerciseDto.TargetType == TargetType.Time &&
                exerciseDto.DurationInSeconds is null)
            {
                errors.Add(ExerciseErrors.MissingDuration);

                continue;
            }

            var exercise = new Exercise(
                Guid.NewGuid(),
                workout.Id,
                exerciseDto.ExerciseType,
                exerciseDto.TargetType,
                exerciseDto.DistanceInMeters,
                exerciseDto.DurationInSeconds);

            workouts.Exercises.Add(exercise);
        }

        if (errors.Count != 0)
        {
            return Result.Failure(new ValidationError(errors.ToArray()));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
```

There isn't much logic here. We're just checking whether the workout exists and whether the exercises are valid.

What happens when we start to add more logic?

Let's add another business rule. We must enforce a limit on the number of exercises allowed in a single workout (e.g., no more than 10 exercises).

```csharp
internal sealed class AddExercisesCommandHandler(
    IWorkoutRepository workoutRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddExercisesCommand>
{
    public async Task<Result> Handle(
        AddExercisesCommand request,
        CancellationToken cancellationToken)
    {
        Workout? workout = await workoutRepository.GetByIdAsync(
            request.WorkoutId,
            cancellationToken);

        if (workout is null)
        {
            return Result.Failure(WorkoutErrors.NotFound(request.WorkoutId));
        }

        List<Error> errors = [];
        foreach (ExerciseRequest exerciseDto in request.Exercises)
        {
            if (exerciseDto.TargetType == TargetType.Distance &&
                exerciseDto.DistanceInMeters is null)
            {
                errors.Add(ExerciseErrors.MissingDistance);

                continue;
            }

            if (exerciseDto.TargetType == TargetType.Time &&
                exerciseDto.DurationInSeconds is null)
            {
                errors.Add(ExerciseErrors.MissingDuration);

                continue;
            }

            var exercise = new Exercise(
                Guid.NewGuid(),
                workout.Id,
                exerciseDto.ExerciseType,
                exerciseDto.TargetType,
                exerciseDto.DistanceInMeters,
                exerciseDto.DurationInSeconds);

            workouts.Exercises.Add(exercise);

            if (workouts.Exercise.Count > 10)
            {
                return Result.Failure(
                    WorkoutErrors.MaxExercisesReached(workout.Id));
            }
        }

        if (errors.Count != 0)
        {
            return Result.Failure(new ValidationError(errors.ToArray()));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
```

We can continue adding more business logic to the transaction script. For example, we can introduce exercise type restrictions or enforce a specific exercise order. You can imagine how the complexity will keep increasing over time.

Another concern is code duplication between transaction scripts. This could happen if we need similar logic in multiple transaction scripts. You may be tempted to solve this by calling one transaction script from the other, but this will introduce a different set of problems.

So, how can we solve these problems?

## Refactoring to Domain Model

What is a domain model?

> An object model of the domain that incorporates both behavior and data.
>
> — [Domain Model](https://martinfowler.com/eaaCatalog/domainModel.html), Patterns of Enterprise Application Architecture

A domain model lets you encapsulate domain logic (behavior) and state changes (data) inside an object. In Domain-Driven Design terminology, we would call this an aggregate.

An aggregate in DDD is a cluster of objects treated as a single unit for data changes. The aggregate represents a consistency boundary. It helps maintain consistency by ensuring that certain invariants always hold true for the entire aggregate. In our workout example, the `Workout` class can be treated as an aggregate root that encompasses all the exercises within it.

What does this have to do with a transaction script?

We can move the domain logic and state changes from the transaction script into the aggregate. This is often called "pushing logic down" into the domain.

Here's what the domain model will look like when we extract the domain logic:

```csharp
public sealed class Workout
{
    private readonly List<Exercise> _exercises = [];

    // Omitting the constructor and other propreties for brevity.

    public Result AddExercises(ExerciseModel[] exercises)
    {
        List<Error> errors = [];
        foreach (var exerciseModel in exercises)
        {
            if (exerciseModel.TargetType == TargetType.Distance &&
                exerciseModel.DistanceInMeters is null)
            {
                errors.Add(ExerciseErrors.MissingDistance);

                continue;
            }

            if (exerciseModel.TargetType == TargetType.Time &&
                exerciseModel.DurationInSeconds is null)
            {
                errors.Add(ExerciseErrors.MissingDuration);

                continue;
            }

            var exercise = new Exercise(
                Guid.NewGuid(),
                workout.Id,
                exerciseDto.ExerciseType,
                exerciseDto.TargetType,
                exerciseDto.DistanceInMeters,
                exerciseDto.DurationInSeconds);

            workouts.Exercises.Add(exercise);

            if (workouts.Exercise.Count > 10)
            {
                return Result.Failure(
                    WorkoutErrors.MaxExercisesReached(workout.Id));
            }
        }

        if (errors.Count != 0)
        {
            return Result.Failure(new ValidationError(errors.ToArray()));
        }

        return Result.Success();
    }
}
```

With the domain logic inside of the domain model, we can easily share it between transaction scripts. Testing the domain model is simpler than testing the transaction script. With a transaction script, we must provide any dependencies (possibly as mocks) for testing. However, we can test the domain model in isolation.

The updated transaction script becomes much more straightforward and focused on its primary task:

```csharp
internal sealed class AddExercisesCommandHandler(
    IWorkoutRepository workoutRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddExercisesCommand>
{
    public async Task<Result> Handle(
        AddExercisesCommand request,
        CancellationToken cancellationToken)
    {
        Workout? workout = await workoutRepository.GetByIdAsync(
            request.WorkoutId,
            cancellationToken);

        if (workout is null)
        {
            return Result.Failure(WorkoutErrors.NotFound(request.WorkoutId));
        }

        var exercises = request.Exercises.Select(e => e.ToModel()).ToArray();

        var result = workout.AddExercises(exercises);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
```

## Takeaway

Transaction Scripts are a practical starting point for simple applications. They offer a straightforward approach to implementing use cases. Transaction scripts are the recommended approach to start building [vertical slices](vertical-slice-architecture-structuring-vertical-slices). However, transaction scripts can become difficult to maintain as the application grows.

[Refactoring toward a Domain Model](refactoring-from-an-anemic-domain-model-to-a-rich-domain-model) allows you to encapsulate business logic in domain objects. This promotes code reusability and makes your application more adaptable to changes. Pushing logic down also improves testability and maintainability.

Should you use a Transaction Script or a Domain Model?

Here's a pragmatic approach you should consider. Start with a transaction script, but pay attention to growing complexity. When you notice a transaction script has too many concerns, consider adding a domain model. Remember, the domain model should encapsulate some of the complexity of the domain logic.

Thanks for reading, and I'll see you next week!

---

Whenever you're ready, there are 4 ways I can help you:

1.  [**Pragmatic Clean Architecture:**](/pragmatic-clean-architecture) Join 4,000+ students in this comprehensive course that will teach you the system I use to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture.
    ![Cover image for 'Pragmatic Clean Architecture' course, showing a blue book with a layered architecture diagram and Milan Jovanović's photo.](/_next/static/media/cover.27333f2f.png)
2.  [**Modular Monolith Architecture:**](/modular-monolith-architecture) Join 2,000+ engineers in this in-depth course that will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario.
    ![Cover image for 'Modular Monolith Architecture' course, showing a blue book with a modular architecture diagram.](/_next/static/media/cover.31e11f05.png)
3.  [(NEW) **Pragmatic REST APIs:**](/pragmatic-rest-apis) Join 1,100+ students in this course that will teach you how to build production-ready REST APIs using the latest ASP.NET Core features and best practices. It includes a fully functional UI application that we'll integrate with the REST API.
    ![Cover image for 'Pragmatic REST APIs' course, showing a blue book with an API diagram and 'NEW' badge.](/_next/static/media/cover_1.fc0deb78.png)
4.  [**Patreon Community:**](https://www.patreon.com/milanjovanovic) Join a community of 1,000+ engineers and software architects. You will also unlock access to the source code I use in my YouTube videos, early access to future videos, and exclusive discounts for my courses.

Become a Better .NET Software Engineer

Join 68,000+ engineers who are improving their skills every Saturday morning.

Join 68K+ Engineers