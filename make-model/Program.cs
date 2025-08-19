using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelProcessor.Data;
using ModelProcessor.Models;
using ModelProcessor.Services;
using Spectre.Console;
using System.Diagnostics;

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

// Setup dependency injection
var services = new ServiceCollection();

// Add logging
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Add configuration
services.AddSingleton<IConfiguration>(configuration);

// Add database context with pgvector
var connectionString = configuration.GetConnectionString("PostgreSQL") 
    ?? throw new InvalidOperationException("PostgreSQL connection string not found");
services.AddVectorDatabase(connectionString);

// Add AI services
var openAiKey = configuration["OpenAI:ApiKey"] 
    ?? throw new InvalidOperationException("OpenAI API key not found");
services.AddEmbeddingServices(openAiKey, configuration["OpenAI:EmbeddingModel"]);
services.AddRagServices(openAiKey, configuration["OpenAI:ChatModel"]);

// Add application services
services.AddSingleton<MarkdownProcessor>();

// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Initialize database
using (var scope = serviceProvider.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<VectorDbContext>();
    await dbContext.InitializeDatabaseAsync();
}

// Main application
await RunInteractiveConsole(serviceProvider);

static async Task RunInteractiveConsole(IServiceProvider serviceProvider)
{
    AnsiConsole.Write(new FigletText("Model Processor")
        .Centered()
        .Color(Color.Cyan1));
    
    AnsiConsole.WriteLine();
    AnsiConsole.Write(new Rule("[yellow]AI-Powered Document Processing System[/]"));
    AnsiConsole.WriteLine();
    
    while (true)
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green]What would you like to do?[/]")
                .PageSize(10)
                .AddChoices([
                    "📄 Process markdown documents",
                    "🔍 Search documents",
                    "💬 Ask a question (RAG)",
                    "📊 View statistics",
                    "🗑️ Clear database",
                    "❌ Exit"
                ]));
        
        try
        {
            switch (choice)
            {
                case "📄 Process markdown documents":
                    await ProcessDocuments(serviceProvider);
                    break;
                    
                case "🔍 Search documents":
                    await SearchDocuments(serviceProvider);
                    break;
                    
                case "💬 Ask a question (RAG)":
                    await AskQuestion(serviceProvider);
                    break;
                    
                case "📊 View statistics":
                    await ViewStatistics(serviceProvider);
                    break;
                    
                case "🗑️ Clear database":
                    await ClearDatabase(serviceProvider);
                    break;
                    
                case "❌ Exit":
                    AnsiConsole.MarkupLine("[yellow]Goodbye![/]");
                    return;
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
        
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule());
        AnsiConsole.WriteLine();
    }
}

static async Task ProcessDocuments(IServiceProvider serviceProvider)
{
    var path = AnsiConsole.Ask<string>(
        "[green]Enter the path to markdown files[/] (or press Enter for 'examples' folder):");
    
    if (string.IsNullOrWhiteSpace(path))
        path = "examples";
    
    if (!Directory.Exists(path))
    {
        AnsiConsole.MarkupLine($"[red]Directory not found: {path}[/]");
        return;
    }
    
    await AnsiConsole.Progress()
        .AutoClear(false)
        .Columns([
            new TaskDescriptionColumn(),
            new ProgressBarColumn(),
            new PercentageColumn(),
            new SpinnerColumn(),
        ])
        .StartAsync(async ctx =>
        {
            var task = ctx.AddTask("[green]Processing documents[/]");
            
            var processor = serviceProvider.GetRequiredService<MarkdownProcessor>();
            var embeddingService = serviceProvider.GetRequiredService<IEmbeddingService>();
            
            // Process markdown files
            task.Description = "Parsing markdown files...";
            var progress = new Progress<string>(msg => task.Description = msg);
            var chunks = await processor.ProcessDirectoryAsync(path, progress);
            
            task.MaxValue = chunks.Length;
            
            // Generate and store embeddings
            task.Description = "Generating embeddings...";
            var processedCount = 0;
            
            // Process in batches
            const int batchSize = 10;
            for (int i = 0; i < chunks.Length; i += batchSize)
            {
                var batch = chunks.Skip(i).Take(batchSize);
                var count = await embeddingService.ProcessAndStoreChunksAsync(batch);
                processedCount += count;
                task.Value = Math.Min(i + batchSize, chunks.Length);
            }
            
            task.Description = $"✅ Processed {processedCount} chunks";
            task.Value = task.MaxValue;
        });
}

static async Task SearchDocuments(IServiceProvider serviceProvider)
{
    var query = AnsiConsole.Ask<string>("[green]Enter your search query:[/]");
    
    var filterByCategory = AnsiConsole.Confirm("Filter by category?", false);
    List<string>? categories = null;
    
    if (filterByCategory)
    {
        var category = AnsiConsole.Ask<string>("Enter category (e.g., general, ai_ml, backend):");
        categories = [category];
    }
    
    var filterByTags = AnsiConsole.Confirm("Filter by tags?", false);
    List<string>? tags = null;
    
    if (filterByTags)
    {
        var tagInput = AnsiConsole.Ask<string>("Enter tags (comma-separated):");
        tags = tagInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .ToList();
    }
    
    var searchRequest = new SearchRequest(query.Split(' '))
    {
        MaxResults = 5,
        MinSimilarity = 0.7f,
        Categories = categories,
        Tags = tags
    };
    
    var embeddingService = serviceProvider.GetRequiredService<IEmbeddingService>();
    
    await AnsiConsole.Status()
        .StartAsync("Searching...", async ctx =>
        {
            var results = await embeddingService.SearchAsync(query, searchRequest);
            
            if (!results.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No results found[/]");
                return;
            }
            
            var table = new Table();
            table.AddColumn("Score");
            table.AddColumn("Type");
            table.AddColumn("Title");
            table.AddColumn("Content Preview");
            
            foreach (var result in results.Take(5))
            {
                var metadata = result.Document.MetadataJson.Deserialize<DocumentMetadata>();
                table.AddRow(
                    $"{result.Score:P0}",
                    result.Document.ChunkType,
                    metadata?.Title ?? "Unknown",
                    result.HighlightedContent
                );
            }
            
            AnsiConsole.Write(table);
        });
}

static async Task AskQuestion(IServiceProvider serviceProvider)
{
    var question = AnsiConsole.Ask<string>("[green]Ask your question:[/]");
    
    var ragService = serviceProvider.GetRequiredService<IRagService>();
    
    await AnsiConsole.Status()
        .StartAsync("Thinking...", async ctx =>
        {
            var stopwatch = Stopwatch.StartNew();
            var response = await ragService.GetAugmentedResponseAsync(question);
            stopwatch.Stop();
            
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Panel(new Markup(response.Answer))
                .Header($"[cyan]Answer (via {response.Model})[/]")
                .Border(BoxBorder.Rounded));
            
            if (response.Sources.Any())
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[dim]Sources:[/]");
                foreach (var source in response.Sources.Take(3))
                {
                    var metadata = source.Document.MetadataJson.Deserialize<DocumentMetadata>();
                    AnsiConsole.MarkupLine($"  • [dim]{metadata?.Title} (Score: {source.Score:P0})[/]");
                }
            }
            
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[dim]Response time: {response.ResponseTime.TotalSeconds:F2}s[/]");
        });
}

static async Task ViewStatistics(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<VectorDbContext>();
    
    var totalCount = await dbContext.DocumentEmbeddings.CountAsync();
    var byType = await dbContext.DocumentEmbeddings
        .GroupBy(d => d.ChunkType)
        .Select(g => new { Type = g.Key, Count = g.Count() })
        .ToListAsync();
    
    var byPriority = await dbContext.DocumentEmbeddings
        .GroupBy(d => d.Priority)
        .Select(g => new { Priority = g.Key, Count = g.Count() })
        .ToListAsync();
    
    AnsiConsole.Write(new Rule("[yellow]Database Statistics[/]"));
    AnsiConsole.WriteLine();
    
    var grid = new Grid();
    grid.AddColumn();
    grid.AddColumn();
    grid.AddColumn();
    
    grid.AddRow(
        new Panel($"[cyan]Total Documents[/]\n[bold]{totalCount}[/]"),
        new Panel($"[cyan]Document Types[/]\n{string.Join("\n", byType.Select(t => $"{t.Type}: {t.Count}"))}"),
        new Panel($"[cyan]By Priority[/]\n{string.Join("\n", byPriority.OrderBy(p => p.Priority).Select(p => $"Priority {p.Priority}: {p.Count}"))}")
    );
    
    AnsiConsole.Write(grid);
}

static async Task ClearDatabase(IServiceProvider serviceProvider)
{
    if (!AnsiConsole.Confirm("[red]Are you sure you want to clear all data?[/]"))
        return;
    
    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<VectorDbContext>();
    
    await AnsiConsole.Status()
        .StartAsync("Clearing database...", async ctx =>
        {
            await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"DocumentEmbeddings\"");
            AnsiConsole.MarkupLine("[green]✅ Database cleared[/]");
        });
}