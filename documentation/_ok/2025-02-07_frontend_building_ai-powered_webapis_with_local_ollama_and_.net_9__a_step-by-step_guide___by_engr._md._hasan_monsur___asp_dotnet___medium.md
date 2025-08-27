```yaml
---
title: "Building AI-Powered WebAPIs with Local Ollama and .NET 9: A Step-by-Step Guide | by Engr. Md. Hasan Monsur | ASP DOTNET | Medium"
source: https://medium.com/asp-dotnet/building-ai-powered-webapis-with-local-ollama-and-net-9-a-step-by-step-guide-cc0c99a9a4a6
date_published: 2025-02-07T14:27:36.650Z
date_captured: 2025-08-27T10:54:15.036Z
domain: medium.com
author: Engr. Md. Hasan Monsur
category: frontend
technologies: [Ollama, .NET 9, ASP.NET Core Web API, NuGet, Microsoft.Extensions.AI, Microsoft.Extensions.AI.Ollama, HttpClient, HttpClientFactory, OpenAPI, HTTP, Large Language Models (LLMs), llama3.2, GitHub, Postman]
programming_languages: [C#]
tags: [ai, web-api, dotnet, ollama, llm, local-ai, csharp, api-integration, backend, development]
key_concepts: [AI-powered WebAPIs, Local LLM deployment, Dependency Injection, API integration, Large Language Models, HTTP client, Web API development, Request/Response handling]
code_examples: false
difficulty_level: intermediate
summary: |
  [The article provides a step-by-step guide on building AI-powered WebAPIs by integrating local Ollama with a .NET 9 application. It details the process from setting up the .NET 9 Web API project and installing required NuGet packages to configuring the Ollama client in `Program.cs`. The guide further explains how to create an `OllamaService` for interacting with the local LLM and implement API endpoints to handle chat requests. This tutorial enables developers to build scalable, AI-driven endpoints using local large language models. It concludes with instructions on running the application and testing the AI endpoint.]
---
```

# Building AI-Powered WebAPIs with Local Ollama and .NET 9: A Step-by-Step Guide | by Engr. Md. Hasan Monsur | ASP DOTNET | Medium

# **Building AI-Powered WebAPIs with Local Ollama and .NET 9: A Step-by-Step Guide**

Discover how to build AI-powered WebAPIs using local Ollama and .NET 9 in this comprehensive guide. Learn to set up a .NET 9 WebAPI that interacts with a local Ollama AI instance, enabling efficient and scalable AI solutions. The article provides step-by-step instructions, from configuring the development environment to implementing and testing the API, ensuring a seamless integration of AI capabilities into your applications.

![Building AI-Powered WebAPIs with Local Ollama and .NET 9: A Step-by-Step Guide banner image](https://miro.medium.com/v2/resize:fit:700/1*y5r9AftBHaFICecZbdGG5Q.png)

> In this guide, you’ll learn how to create intelligent WebAPIs by integrating **Local Ollama** with **.NET 9**. Discover how to set up Ollama for running AI models locally, connect it to your .NET 9 WebAPI project, and build scalable, AI-driven endpoints. Whether you’re developing chatbots, recommendation systems, or natural language processing tools, this step-by-step tutorial will help you unlock the power of AI in your applications. Perfect for developers looking to combine the flexibility of .NET 9 with the cutting-edge capabilities of Ollama!

To create an AI-powered Web API using .NET 9 and a local Ollama instance, follow these steps:

**1. Prerequisites:**

*   **.NET 9 SDK:** Ensure it’s installed on your system.
*   **Ollama:** A platform for running large language models (LLMs) locally. Download and install it from ollama.com.

**2. Set Up the .NET 9 Web API Project:**

Open your terminal or command prompt.

Create a new Web API project:

```bash
dotnet new webapi -n OllamaAIWebAPI
cd OllamaAIWebAPI
```

**3. Install Necessary NuGet Packages:**

Add the following packages to your project:

```bash
dotnet add package Microsoft.Extensions.AI --prerelease
dotnet add package Microsoft.Extensions.AI.Ollama --prerelease
```

*Note:* The `--prerelease` flag is used because these packages might be in preview.

**4. Configure the Application:**

In `Program.cs`, set up the Ollama client:

```csharp
using Microsoft.Extensions.DependencyInjection;
using OllamaAIWebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Configure Ollama settings

// Configure Ollama settings
// Register a named HttpClient in Program.cs or Startup.cs
builder.Services.AddHttpClient("OllamaClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:11434");
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});


builder.Services.AddTransient<OllamaService>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
// Use Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

Ensure that the Ollama server is running locally and accessible at `http://localhost:11434`

![Screenshot showing "Ollama is running" in a web browser at localhost:11434](https://miro.medium.com/v2/resize:fit:398/1*4jbOZ0VM0IGDfTWT8yr2Rg.png)

**5. Create OllamaService:**

```csharp
namespace OllamaAIWebAPI.Services
{
    public class OllamaService
    {
        private readonly HttpClient _httpClient;

        public OllamaService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("OllamaClient");
        }

        public async Task<string> GenerateResponseAsync(string prompt)
        {
            var requestBody = new
            {
                model = "llama3.2:latest",
                prompt = prompt,
                Stream=false
            };

            var response = await _httpClient.PostAsJsonAsync("/api/generate", requestBody);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
```

**6. Implement API Endpoints:**

Create controllers and endpoints to handle client requests and interact with the Ollama client.

```csharp
using Microsoft.AspNetCore.Mvc;
using OllamaAIWebAPI.Services;

namespace OllamaAIWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly OllamaService _ollmaServie;

        public AIController(OllamaService ollmaServie)
        {
            _ollmaServie = ollmaServie;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] string userInput)
        {
            var response = await _ollmaServie.GenerateResponseAsync(userInput);
            return Ok(response);
        }
    }
}
```

**7. Run the Application:**

Start the Ollama server:

```bash
ollama serve
```

Then run your .NET application:

```bash
dotnet run
```

![API client (Postman/Insomnia) showing a POST request to /api/ai/chat with "Hello" input and a JSON response from llama3.2:latest](https://miro.medium.com/v2/resize:fit:700/1*HEbnyQSf_win2YYDAk8IMg.png)

### Download Project — [Building-AI-Powered-WebAPIs](https://github.com/hasanmonsur/Building-AI-Powered-WebAPIs)

## Conclusion:

Thank you for providing more context! Based on your code snippet, it seems you’re trying to configure an **Ollama client** in an ASP.NET Core application. Ollama is a tool for running and managing large language models (LLMs) locally, and your code suggests you’re trying to integrate it into your application.