```yaml
---
title: "Modern .NET APIs: Minimal APIs, Clean Architecture, Aspire + Ollama | by Yaseer Arafat | Sep, 2025 | Medium"
source: https://blog.yaseerarafat.com/ship-production-ready-net-apis-with-aspire-ollamasharp-40e4a3dde90a
date_published: 2025-09-03T06:55:18.728Z
date_captured: 2025-09-09T11:15:43.307Z
domain: blog.yaseerarafat.com
author: Yaseer Arafat
category: architecture
technologies: [.NET Aspire, ASP.NET Core, Minimal APIs, .NET 9, Ollama, OllamaSharp, Microsoft.Extensions.AI, PostgreSQL, Redis, Entity Framework Core, OpenTelemetry, Garnet, OpenAPI, Microsoft.AspNetCore.OpenApi, StackExchange.Redis, Npgsql.EntityFrameworkCore.PostgreSQL]
programming_languages: [C#, SQL]
tags: [dotnet, aspnet-core, minimal-apis, clean-architecture, ai, llm, ollama, aspire, microservices, data-access]
key_concepts: [Clean Architecture, Minimal APIs, Distributed Application Orchestration, Local LLMs, Provider-agnostic AI, OpenAPI generation, Observability, Repository Pattern]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a blueprint for building modern .NET APIs using Minimal APIs, Clean Architecture, and .NET Aspire for local orchestration. It focuses on integrating local LLMs with Ollama and OllamaSharp, offering a private-by-default AI solution. The architecture emphasizes maintainability, testability, and flexibility, allowing for easy swapping of infrastructure components and AI providers using Microsoft.Extensions.AI. It also highlights built-in OpenAPI support in .NET 9 and robust observability with the Aspire Dashboard and OpenTelemetry. This approach aims to accelerate development while ensuring production readiness and operational safety for small platform teams.
---
```

# Modern .NET APIs: Minimal APIs, Clean Architecture, Aspire + Ollama | by Yaseer Arafat | Sep, 2025 | Medium

# Modern .NET APIs: Minimal APIs, Clean Architecture, Aspire + Ollama

![A diagram illustrating the components of a production-ready .NET API. On the left, it shows a .NET application interacting with Postgres, Redis, and OllamaSharp, representing data and AI integration. On the right, it depicts an API endpoint processing AI/LLM streaming data, with a focus on an "Aspire" component at the top, suggesting orchestration.](https://miro.medium.com/v2/resize:fit:700/1*MAmn2uIEKYycgXwr7vt-7A.png)

Stop wrestling with Kubernetes on day one. With **.NET Aspire**, you can spin up your services and infrastructure locally from a single app model — Postgres, Redis, Ollama, all ready to go. Keep your API **lean but maintainable**, organizing endpoints by file so it stays readable as it grows. Lock in **Clean Architecture** so your domain logic stays pure, while swapping out infrastructure is effortless. Generate **OpenAPI** right in **.NET 9**, no extra plugins needed, and run **LLMs on-prem** with **OllamaSharp** — no vendor lock-in, no data leaving your network.

This is the kind of blueprint I’d hand a small platform team: a way to ship fast, stay observable, and build something that doesn’t crumble the moment requirements change. It’s practical, it’s repeatable, and it’s built for real-world devs who want to move quickly without leaving maintainability behind.

## 🏆 Why This Approach Wins

**Single source of truth:** Aspire’s **AppHost** defines all services, containers, and wiring. Think of it as a single **orchestra conductor** for your distributed app — everything is composed declaratively, repeatable, and easy to reason about.

**Lean API surface:** Minimal APIs, organized into **file-scoped endpoint groups**, keep routing simple, discoverable, and maintainable. Less ceremony, more shipping features.

**Clean seams:** Following **N-Layer / Clean Architecture** principles, the **Domain** and **Application** layers remain isolated from Infrastructure. Swap databases, caches, or AI providers with minimal blast radius.

**Docs without plugins:** **.NET 9** includes built-in **Microsoft.AspNetCore.OpenApi** support. Generate OpenAPI docs out-of-the-box — no Swashbuckle needed — and enrich your endpoints with `Produces<T>()` and `Accepts<T>()` for high-fidelity client generation.

**AI that respects boundaries:** **OllamaSharp** interacts with local models with full API coverage, streaming, and minimal footprint. It also integrates cleanly with **Microsoft.Extensions.AI**, giving you a provider-agnostic abstraction layer for future-proof AI pipelines.

This approach balances **speed, maintainability, and operational safety**. You get a reproducible, observable, and extendable architecture that supports rapid iteration without sacrificing long-term flexibility.

## The architecture at a glance

**Solution layout**

```
SmartResumeAI/  
├─ src/  
│  ├─ Api/                       # ASP.NET Core Minimal API  
│  │  ├─ Endpoints/  
│  │  │  ├─ Candidates/  
│  │  │  │  ├─ GetCandidate.cs  
│  │  │  │  └─ CreateCandidate.cs  
│  │  │  ├─ Skills/  
│  │  │  │  ├─ SuggestSkills.cs  
│  │  │  │  └─ EvaluateSkills.cs  
│  │  │  └─ Health/HealthEndpoints.cs  
│  │  ├─ Modules/                # Endpoint grouping extensions  
│  │  ├─ Api.csproj  
│  │  └─ Program.cs  
│  ├─ Application/               # Use-cases, handlers, ports  
│  │  ├─ Abstractions/Repositories/  
│  │  ├─ Candidates/Commands/  
│  │  ├─ Candidates/Queries/  
│  │  ├─ Skills/Services/ISkillSuggester.cs  
│  │  └─ Application.csproj  
│  ├─ Domain/                    # Entities, value objects, logic  
│  │  ├─ Candidates/Candidate.cs  
│  │  ├─ Skills/Skill.cs  
│  │  ├─ Common/Result.cs  
│  │  └─ Domain.csproj  
│  ├─ Infrastructure/            # EF Core, OllamaSharp, Redis, etc.  
│  │  ├─ Data/AppDbContext.cs  
│  │  ├─ Repositories/  
│  │  ├─ Ai/OllamaSkillSuggester.cs  
│  │  ├─ Ai/ExtensionsAIBridge.cs  
│  │  └─ Infrastructure.csproj  
│  └─ Workers/                   # Background jobs/consumers (optional)  
│     └─ Workers.csproj  
├─ apphost/                      # .NET Aspire AppHost  
│  ├─ Program.cs  
│  └─ apphost.csproj  
└─ SmartResumeAI.sln
```

**Flow**

*   **Api** exposes minimal endpoints grouped by module.
*   **Application** orchestrates use cases and depends only on **Domain** + abstractions.
*   **Infrastructure** implements data and AI adapters (EF Core, Redis cache, **OllamaSharp** skill suggester, and optional **Microsoft.Extensions.AI** bridge).
*   **AppHost (Aspire)** spins the whole stack: **Postgres**, **Redis/Garnet**, **Ollama** container, and the **Api** project with wiring, diagnostics, and the Aspire dashboard.

![A detailed flow diagram of a Production-Ready .NET API. It starts with ".NET Aspire AppHost (Orchestrator)" at the top, connecting to Postgres, Redis/Garnet, Ollama Container, and the API. The API layer (Minimal APIs) handles endpoints like /candidates, /skills, /health, and generates OpenAPI JSON. Below, the Application Layer (Use Cases/Handlers) interacts with the Domain Layer (Entities/Value Objects), which then connects to the Infrastructure Layer (EF Core for Postgres, OllamaSharp/Extensions AI for Ollama). This diagram clearly illustrates the Clean Architecture layers and how Aspire orchestrates the distributed components.](https://miro.medium.com/v2/resize:fit:700/1*irE7LKe-C0I_VvaB7k_B-A.png)

## 🚀 Step 1 — Compose your distributed app with .NET Aspire

Add the **AppHost** project and wire containers/resources. Aspire’s builder exposes `AddProject`, `AddPostgres`, `AddGarnet`, and `**AddContainer**` for arbitrary images (that’s how we bring in Ollama).

`**apphost/Program.cs**`

```csharp
using Aspire.Hosting;  
  
var builder = DistributedApplication.CreateBuilder(args);  
  
// Database (Postgres)  
var postgres = builder.AddPostgres("postgres");  
var appDb = postgres.AddDatabase("app-db", databaseName: "smartresume");  
  
// Cache (Garnet/Redis-compatible)  
var cache = builder.AddGarnet("cache");  
  
// Ollama (local LLM server)  
var ollama = builder.AddContainer(  
        name: "ollama",  
        image: "ollama/ollama",  
        tag: "latest")  
    .WithEndpoint(port: 11434, targetPort: 11434, name: "http")  
    .WithHttpHealthCheck("/api/tags");  
  
// API Project  
var api = builder.AddProject<Projects.Api>("api")  
    .WithReference(appDb)  
    .WithReference(cache)  
    .WithReference(ollama);  
  
builder.Build().Run();
```

> **_Notes_**
>
> `AddContainer` is the idiomatic way to express custom images in Aspire; you can expose ports and health checks straight from the app model.
>
> Aspire’s **dashboard** lights up logs, traces, dependencies, and environment values during local runs. You’ll actually _see_ the Ollama container next to your API and DB.

## 🚀 Step 2 — Minimal APIs, but split by file (so they scale)

File-scoped endpoints give you **feature folders** without controllers. Each endpoint is a tiny, testable unit.

`**Api/Program.cs**`

```csharp
var builder = WebApplication.CreateBuilder(args);  
  
builder.Services.AddEndpointsApiExplorer();     // Adds endpoint discovery  
builder.Services.AddOpenApi();                  // .NET 9 built-in OpenAPI (no Swashbuckle needed)  
  
builder.Services.AddApiServices(builder.Configuration); // Extension: DI for app/infra  
  
var app = builder.Build();  
  
if (app.Environment.IsDevelopment())  
{  
    app.MapOpenApi();          // /openapi/v1.json  
    app.UseDeveloperExceptionPage();  
}  
  
// Grouped endpoints loaded from separate files  
app.MapCandidatesEndpoints();  
app.MapSkillsEndpoints();  
app.MapHealthEndpoints();  
  
app.Run();
```

**OpenAPI baked in**
The `Microsoft.AspNetCore.OpenApi` package generates the document and lets you tag/group minimal endpoints cleanly—**no controller attributes required**.

`**Api/Endpoints/Candidates/GetCandidate.cs**`

```csharp
public static class GetCandidate  
{  
    public static IEndpointRouteBuilder MapGetCandidate(this IEndpointRouteBuilder routes)  
    {  
        var group = routes.MapGroup("/api/candidates").WithTags("Candidates");  
  
        group.MapGet("{id:guid}", async (Guid id, ICandidateQueries queries, CancellationToken ct) =>  
        {  
            var result = await queries.GetByIdAsync(id, ct);  
            return result is null ? Results.NotFound() : Results.Ok(result);  
        })  
        .WithName("GetCandidate")  
        .Produces<CandidateDto>(StatusCodes.Status200OK)  
        .Produces(StatusCodes.Status404NotFound);  
  
        return routes;  
    }  
}  
  
public static class CandidatesEndpoints  
{  
    public static void MapCandidatesEndpoints(this IEndpointRouteBuilder app)  
    {  
        app.MapGetCandidate();  
        // app.MapCreateCandidate(); // lives in CreateCandidate.cs  
    }  
}
```

`**Api/Endpoints/Health/HealthEndpoints.cs**`

```csharp
public static class HealthEndpoints  
{  
    public static void MapHealthEndpoints(this IEndpointRouteBuilder app)    {  
        app.MapGet("/health", () => Results.Ok(new { status = "ok" }))  
           .WithTags("Health");  
    }  
}
```

> **_Why this scales_**_Minimal APIs are fast to read, easy to route, and play nicely with OpenAPI in .NET 9. You can still enforce response types with_ `_TypedResults_` _and keep the documentation on rails._

## 🚀 Step 3 — Clean Architecture: keep your core clean

**Domain** is your source of truth. **Application** defines use cases via ports. **Infrastructure** implements adapters (EF Core, cache, AI).

`**Domain/Candidates/Candidate.cs**`

```csharp
public sealed class Candidate  
{  
    public Guid Id { get; private set; } = Guid.NewGuid();  
    public string FullName { get; private set; }  
    public List<Skill> Skills { get; } = new();  
  
    public Candidate(string fullName)  
    {  
        FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));  
    }  
  
    public void AddSkill(string name, int score)  
    {  
        Skills.Add(new Skill(name, score));  
    }  
}  
  
public sealed class Skill  
{  
    public string Name { get; }  
    public int Score { get; }  
  
    public Skill(string name, int score)  
    {  
        Name = name;  
        Score = Math.Clamp(score, 0, 100);  
    }  
}
```

`**Application/Abstractions/Repositories/ICandidateRepository.cs**`

```csharp
public interface ICandidateRepository  
{  
    Task<Candidate?> GetAsync(Guid id, CancellationToken ct);  
    Task AddAsync(Candidate candidate, CancellationToken ct);  
    Task SaveChangesAsync(CancellationToken ct);  
}
```

`**Application/Candidates/Queries/GetCandidateById.cs**`

```csharp
public interface ICandidateQueries  
{  
    Task<CandidateDto?> GetByIdAsync(Guid id, CancellationToken ct);  
}  
  
public sealed class CandidateDto(Guid id, string fullName, IReadOnlyList<SkillDto> skills)  
{  
    public Guid Id { get; } = id;  
    public string FullName { get; } = fullName;  
    public IReadOnlyList<SkillDto> Skills { get; } = skills;  
}  
  
public sealed class SkillDto(string name, int score)  
{  
    public string Name { get; } = name;  
    public int Score { get; } = score;  
}
```

## 🚀 Step 4 — Infrastructure: EF Core + Postgres + caching

`**Infrastructure/Data/AppDbContext.cs**`

```csharp
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)  
{  
    public DbSet<Candidate> Candidates => Set<Candidate>();  
  
    protected override void OnModelCreating(ModelBuilder modelBuilder)  
    {  
        modelBuilder.Entity<Candidate>(b =>  
        {  
            b.HasKey(x => x.Id);  
            b.Property(x => x.FullName).HasMaxLength(150).IsRequired();  
            b.OwnsMany(x => x.Skills, sb =>  
            {  
                sb.ToTable("candidate_skills");  
                sb.WithOwner().HasForeignKey("CandidateId");  
                sb.Property<int>("Id");  
                sb.HasKey("Id");  
                sb.Property(s => s.Name).HasMaxLength(100).IsRequired();  
            });  
        });  
    }  
}
```

`**Infrastructure/Repositories/CandidateRepository.cs**`

```csharp
public sealed class CandidateRepository(AppDbContext db) : ICandidateRepository  
{  
    public Task<Candidate?> GetAsync(Guid id, CancellationToken ct) =>  
        db.Candidates.Include(c => c.Skills).SingleOrDefaultAsync(c => c.Id == id, ct);  
  
    public async Task AddAsync(Candidate candidate, CancellationToken ct) =>  
        await db.Candidates.AddAsync(candidate, ct);  
  
    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);  
}
```

`**Api/ServiceRegistration.cs**`

```csharp
public static class ServiceRegistration  
{  
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration config)  
    {  
        // Database  
        services.AddDbContext<AppDbContext>(opt =>  
            opt.UseNpgsql(config.GetConnectionString("app-db")));  
  
        // Caching (StackExchange.Redis compatible - Garnet works)  
        services.AddStackExchangeRedisCache(options =>  
        {  
            options.Configuration = config.GetConnectionString("cache");  
        });  
  
        // App abstraction wiring  
        services.AddScoped<ICandidateRepository, CandidateRepository>();  
        services.AddScoped<ICandidateQueries, CandidateQueries>();  
  
        // AI services added below in the AI section  
        return services;  
    }  
}
```

Aspire injects the connection strings at runtime; your app can simply read `**ConnectionStrings:app-db**` / `**cache**` via standard configuration.

## 🚀 Step 5 — Local LLMs with OllamaSharp (streaming, private by default)

If your security model prefers **local inference**, **Ollama** is a strong default. With Aspire you can run the Ollama container as a first-class resource; with **OllamaSharp** you integrate in a few lines. [Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/orchestrate-resources?utm_source=chatgpt.com)[GitHub](https://github.com/awaescher/OllamaSharp?utm_source=chatgpt.com)

**Install**

```bash
dotnet add src/Infrastructure package OllamaSharp
```

`**Infrastructure/Ai/OllamaSkillSuggester.cs**`

```csharp
using OllamaSharp;  
  
public interface ISkillSuggester  
{  
    IAsyncEnumerable<string> SuggestAsync(string jobDescription, CancellationToken ct);  
}  
  
public sealed class OllamaSkillSuggester(IConfiguration cfg, ILogger<OllamaSkillSuggester> logger) : ISkillSuggester  
{  
    public async IAsyncEnumerable<string> SuggestAsync(string jobDescription, [EnumeratorCancellation] CancellationToken ct)  
    {  
        var host = cfg.GetConnectionString("ollama") ?? "http://localhost:11434";  
        var model = cfg["Ai:Model"] ?? "llama3.1";  
        var client = new OllamaApiClient(new Uri(host));  
  
        var sb = new StringBuilder();  
  
        await foreach (var chunk in client.GenerateAsync(  
            model,  
            prompt: $"Suggest exactly 10 skills for this role. Output as a single comma-separated line, all lowercase, no commentary:\n\n{jobDescription}",  
            cancellationToken: ct))  
        {  
            logger.LogDebug("AI chunk: {Chunk}", chunk.Response);  
            sb.Append(chunk.Response);  
        }  
  
        foreach (var skill in sb.ToString().Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))  
        {  
            yield return skill;  
        }  
    }  
}
```

> **_Why OllamaSharp?_**_It covers_ **_all_** _Ollama endpoints (chats, embeddings, model mgmt) and supports_ **_streaming_** _— important for UX and throughput. It’s also used in the broader .NET AI ecosystem._

**Endpoint wired to the suggester**

```csharp
public static class SuggestSkills  
{  
    public static void MapSkillsEndpoints(this IEndpointRouteBuilder app)  
    {  
        var group = app.MapGroup("/api/skills").WithTags("Skills");  
  
        group.MapPost("/suggest", async (SuggestReq req, ISkillSuggester ai, HttpResponse response, CancellationToken ct) =>  
        {  
            response.ContentType = "text/plain; charset=utf-8";  
            response.Headers.CacheControl = "no-cache";  
            response.Headers.Add("X-Accel-Buffering", "no");  
  
            try  
            {  
                await foreach (var token in ai.SuggestAsync(req.JobDescription, ct))  
                {  
                    await response.WriteAsync(token, ct);  
                    await response.Body.FlushAsync(ct);  
                }  
            }  
            catch (Exception ex)  
            {  
                // Optional: send error marker to client  
                await response.WriteAsync($"\n[error: {ex.Message}]", ct);  
            }  
        })  
        .WithName("SuggestSkills")  
        .Accepts<SuggestReq>("application/json")  
        .Produces(StatusCodes.Status200OK);  
    }  
  
    public sealed record SuggestReq(string JobDescription);  
}
```

**Local model notes:** If you’ve never run Llama locally, start with an 8B parameter model for RAM/VRAM friendliness, then scale up.

## 🚀 Step 6 — Microsoft.Extensions.AI for Provider-Swap Flexibility

If you want **AI-powered .NET APIs** that stay maintainable, **Microsoft.Extensions.AI** is your friend. It provides **thin abstractions** like `IChatClient` and related interfaces, letting you swap AI providers — **OpenAI, Azure OpenAI, or local/open-source models** — without rewriting your application logic.

**Provider-agnostic design:** Build your endpoints and handlers against `IChatClient`. Later, switch from a cloud provider to **OllamaSharp** or any other supported backend seamlessly. Your business logic remains untouched.

**Observability out-of-the-box:** Add `UseLogging()` and `UseOpenTelemetry()` on your AI pipeline to capture **traces, metrics, and structured logs**. Combined with Aspire’s dashboard, you get full visibility into request journeys, including AI prompt and response metadata.

**Benefits:**

*   Flexible AI provider integration.
*   Cleaner, testable code.
*   Operational insights without extra wiring.

**Key takeaway:** Microsoft.Extensions.AI lets you **future-proof your AI layer**, decoupling provider specifics from your application while giving robust monitoring capabilities with minimal effort.

**Install**

```bash
dotnet add src/Infrastructure package Microsoft.Extensions.AI  
dotnet add src/Infrastructure package Microsoft.Extensions.AI.OpenAI
```

`**Infrastructure/Ai/ExtensionsAIBridge.cs**` (simple adapter that wraps your prompt into a `IChatClient` call; you can plug OpenAI, Azure OpenAI, or a community adapter for Ollama)

```csharp
using Microsoft.Extensions.AI;  
  
public sealed class ExtensionsAiSkillSuggester : ISkillSuggester  
{  
    private readonly IChatClient _chat;  
  
    public ExtensionsAiSkillSuggester(IChatClient chat) => _chat = chat;  
  
    public async IAsyncEnumerable<string> SuggestAsync(string jobDescription, [EnumeratorCancellation] CancellationToken ct)  
    {  
        var sys = ChatMessage.CreateSystemMessage("You are a concise recruiter assistant. Output CSV only, no commentary.");  
        var usr = ChatMessage.CreateUserMessage($"Suggest 10 skills for: {jobDescription}");  
  
        await foreach (var update in _chat.CompleteStreamingAsync([sys, usr], cancellationToken: ct))  
        {  
            if (update is { ContentUpdate.Count: >0 })  
                foreach (var part in update.ContentUpdate)  
                    yield return part.Text ?? string.Empty;  
        }  
    }  
}
```

**Register an OpenAI-compatible backend** (e.g., OpenAI or an OpenAI-compatible gateway you host):

```csharp
builder.Services.AddSingleton<IChatClient>(sp =>  
{  
    var apiKey = sp.GetRequiredService<IConfiguration>()["OpenAI:ApiKey"];  
    return new OpenAIChatClient(apiKey)  
        .UseLogging(sp.GetRequiredService<ILoggerFactory>())  
        .UseOpenTelemetry(sp.GetRequiredService<ILoggerFactory>());  
});
```

> **_Reality check:_** `_IChatClient_` _is_ **_thread-safe_** _and designed for concurrent requests. If you swap providers later, your app code remains stable._
>
> **_Local models with Extensions.AI?_**_You can point_ **_Extensions.AI_** _at any_ **_OpenAI-compatible_** _endpoint. Many teams place a small proxy in front of Ollama to speak the OpenAI dialect; others use direct_ **_OllamaSharp_** _for full coverage + speed. Choose based on operability and feature parity._

## 🚀 Step 7 — OpenAPI That’s First-Class (Grouped, Typed, and Discoverable)

Modern **.NET 9** makes OpenAPI a first-class citizen — no external packages like Swashbuckle required. With a single line, you can enable clean, typed, and discoverable API documentation:

```csharp
builder.Services.AddOpenApi();  
  
app.MapOpenApi(); // GET /openapi/v1.json  
  
// Tag example shown earlier: .WithTags("Skills")
```

You get an OpenAPI document without external packages, and you can decorate Minimal API handlers with metadata (`Produces<T>()`, `Accepts<T>()`, etc.) for high-fidelity docs and client generation.

## 🚀 Step 8 — A Pragmatic CI/CD Stance

Building and shipping modern **.NET APIs with AI integration** demands a workflow that balances **speed, repeatability, and production safety**. Aspire helps bridge that gap.

*   **Local development:** Let **Aspire orchestrate everything** — Postgres, distributed caches, Ollama containers, and background workers. You get a fully composed environment out-of-the-box. This allows fast iteration, quick testing of AI prompts, and safe experimentation without touching production infrastructure. Your team can focus on **business logic, Minimal API endpoints, and AI workflows** rather than deployment glue.
*   **Pre-production / production:** When it’s time to ship, containerize APIs and background workers. Deploy **databases and caches** using **managed cloud services** or **Helm charts** on Kubernetes. Aspire doesn’t replace production orchestration — it isn’t a full orchestrator like Kubernetes — but it **accelerates the path to production**. The composition remains code-first, repeatable, and testable, so what works locally translates cleanly to pre-prod and prod environments.
*   **CI/CD best practices:** Integrate **automated builds, container scans, and deployment pipelines**. Aspire’s declarative AppHost composition ensures that the same resource wiring — databases, caches, and AI models — can be provisioned consistently across environments, reducing drift and human error.
*   **Key takeaway:** Aspire’s strength is in **speed and predictability**. It gives teams a reproducible foundation locally and a clear blueprint for production, so you can iterate fast while maintaining operational confidence. [DEV Community](https://dev.to/rineshpk/simplifying-microservice-development-with-net-aspire-dapr-and-podman-3hp0)

## End-to-end sample: create + fetch candidate, suggest skills

`**Api/Endpoints/Candidates/CreateCandidate.cs**`

```csharp
public static class CreateCandidate  
{  
    public static IEndpointRouteBuilder MapCreateCandidate(this IEndpointRouteBuilder routes)  
    {  
        var group = routes.MapGroup("/api/candidates").WithTags("Candidates");  
  
        group.MapPost("", async (CreateCandidateReq req, ICandidateRepository repo, CancellationToken ct) =>  
        {  
            if (string.IsNullOrWhiteSpace(req.FullName))  
                return Results.BadRequest("FullName is required.");  
  
            var candidate = new Candidate(req.FullName);  
  
            foreach (var s in (req.SeedSkills ?? [])  
                .Where(s => !string.IsNullOrWhiteSpace(s))  
                .Distinct(StringComparer.OrdinalIgnoreCase))  
            {  
                candidate.AddSkill(s.Trim(), score: 50);  
            }  
  
            await repo.AddAsync(candidate, ct);  
            await repo.SaveChangesAsync(ct);  
  
            return Results.Created($"/api/candidates/{candidate.Id}", new { candidate.Id });  
        })  
        .WithName("CreateCandidate")  
        .Accepts<CreateCandidateReq>("application/json")  
        .Produces(StatusCodes.Status201Created)  
        .ProducesValidationProblem();  
  
        return routes;  
    }  
}  
  
public sealed record CreateCandidateReq(string FullName, List<string>? SeedSkills);
```

`**Api/Endpoints/Skills/EvaluateSkills.cs**` (AI step via OllamaSharp adapter)

```csharp
public static class EvaluateSkills  
{  
    public static void MapSkillsEndpoints(this IEndpointRouteBuilder app)  
    {  
        var group = app.MapGroup("/api/skills").WithTags("Skills");  
  
        group.MapPost("/evaluate", async (EvalReq req, ISkillSuggester ai, CancellationToken ct) =>  
        {  
            if (string.IsNullOrWhiteSpace(req.JobDescription))  
                return Results.BadRequest("Job description is required.");  
  
            var sb = new StringBuilder();  
            await foreach (var t in ai.SuggestAsync(req.JobDescription, ct))  
                sb.Append(t