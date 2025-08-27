```yaml
---
title: "Create a Beautiful API Documentation with Scalar in ASP .NET Core | by Juldhais Hengkyawan | Medium"
source: https://juldhais.net/create-a-beautiful-api-documentation-with-scalar-in-asp-net-core-d3d4d17570a6
date_published: 2024-12-20T12:16:50.342Z
date_captured: 2025-08-06T17:46:10.264Z
domain: juldhais.net
author: Juldhais Hengkyawan
category: backend
technologies: [ASP.NET Core, Scalar, Swagger, NuGet, OpenApi, JWT]
programming_languages: [C#, SQL]
tags: [api-documentation, aspnet-core, web-api, swagger, scalar, dotnet, ui-ux, authentication, jwt]
key_concepts: [api-documentation, swagger-ui-replacement, nuget-package-management, middleware-configuration, jwt-authentication, launch-settings]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces Scalar as a modern and visually appealing alternative to the default Swagger UI for generating API documentation in ASP.NET Core Web API projects. It provides a step-by-step guide on how to install the `Scalar.AspNetCore` NuGet package and configure it within the `Program.cs` file to replace the existing Swagger setup. The guide also demonstrates how to add Bearer authentication support to the Scalar documentation and modify the `launchSettings.json` file for automatic browser launch. The author highlights Scalar's clean design, intuitive navigation, and built-in API testing capabilities, which enhance the developer experience.]
---
```

# Create a Beautiful API Documentation with Scalar in ASP .NET Core | by Juldhais Hengkyawan | Medium

# Create a Beautiful API Documentation with Scalar in ASP .NET Core

![Author's profile picture, Juldhais Hengkyawan](https://miro.medium.com/v2/resize:fill:64:64/1*-P6OXwDeCtDsxAdu2ACrJw.png)

[Juldhais Hengkyawan](/?source=post_page---byline--d3d4d17570a6---------------------------------------)

Follow

3 min read

·

Dec 20, 2024

249

6

Listen

Share

More

![Screenshot of the Scalar API documentation UI, showing a dark theme with API endpoints listed on the left and details for a selected endpoint on the right.](https://miro.medium.com/v2/resize:fit:700/1*IEZYBbS7px9EjMJbLGCzuA.png)

Scalar is a tool that helps us create beautiful documentation for our APIs. Unlike the default Swagger documentation, which can feel a bit outdated, Scalar offers a fresh and modern UI for API documentation. Its clean design makes it easy for developers to find the APIs they need for testing.

In this article, we will explore how to use Scalar for API documentation in ASP.NET Core Web API to replace the default Swagger UI.

# Install Scalar

Open the NuGet Package Manager, search and install the _Scalar.AspNetCore_ package:

![Screenshot of the NuGet Package Manager in Visual Studio, with 'Scalar.AspNetCore' searched and selected, showing package details and download count.](https://miro.medium.com/v2/resize:fit:597/1*eBIzBwfZe-Ysvf5AK7SWkA.png)

# Configure Scalar

Open the _Program.cs_ file and replace the default swagger configuration:

**From this:**

```csharp
if (app.Environment.IsDevelopment())  
{  
    app.UseSwagger();  
    app.UseSwaggerUI();  
}
```

**To this:**

```csharp
app.UseSwagger(opt =>  
{  
    opt.RouteTemplate = "openapi/{documentName}.json";  
});  
app.MapScalarApiReference(opt =>  
{  
    opt.Title = "Scalar Example";  
    opt.Theme = ScalarTheme.Mars;  
    opt.DefaultHttpClient = new(ScalarTarget.Http, ScalarClient.Http11);  
});
```

## Bearer Authentication

We can also add a Bearer authentication header to the Scalar documentation. Just replace the `builder.Services.AddSwaggerGen();` configuration with this:

```csharp
builder.Services.AddSwaggerGen(opt =>   
{  
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme  
    {  
        BearerFormat = "JWT",  
        Description = "JWT Authorization header using the Bearer scheme.",  
        Name = "Authorization",  
        In = ParameterLocation.Header,  
        Type = SecuritySchemeType.Http,  
        Scheme = "Bearer"  
    });  
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement  
            {  
                {  
                    new OpenApiSecurityScheme  
                    {  
                        Reference = new OpenApiReference  
                        {  
                            Id = "Bearer",  
                            Type = ReferenceType.SecurityScheme  
                        }  
                    },  
                    Array.Empty<string>()  
                }  
            });  
});
```

This is complete _Program.cs_ file:

```csharp
using Microsoft.OpenApi.Models;  
using Scalar.AspNetCore;  
  
var builder = WebApplication.CreateBuilder(args);  
builder.Services.AddControllers();  
builder.Services.AddEndpointsApiExplorer();  
  
builder.Services.AddSwaggerGen(opt =>   
{  
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme  
    {  
        BearerFormat = "JWT",  
        Description = "JWT Authorization header using the Bearer scheme.",  
        Name = "Authorization",  
        In = ParameterLocation.Header,  
        Type = SecuritySchemeType.Http,  
        Scheme = "Bearer"  
    });  
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement  
            {  
                {  
                    new OpenApiSecurityScheme  
                    {  
                        Reference = new OpenApiReference  
                        {  
                            Id = "Bearer",  
                            Type = ReferenceType.SecurityScheme  
                        }  
                    },  
                    Array.Empty<string>()  
                }  
            });  
});  
  
var app = builder.Build();  
  
app.UseSwagger(opt =>  
{  
    opt.RouteTemplate = "openapi/{documentName}.json";  
});  
app.MapScalarApiReference(opt =>  
{  
    opt.Title = "Scalar Example";  
    opt.Theme = ScalarTheme.Mars;  
    opt.DefaultHttpClient = new(ScalarTarget.Http, ScalarClient.Http11);  
});  
  
app.UseHttpsRedirection();  
  
app.UseAuthorization();  
  
app.MapControllers();  
  
app.Run();
```

# Launch Settings

To have Scalar open automatically when we run the application, we need to tweak the _LaunchSettings.json_ file. We can find this file in the _Properties_ folder:

![Screenshot of the Solution Explorer in Visual Studio, highlighting the 'Properties' folder and the 'launchSettings.json' file within an ASP.NET Core project.](https://miro.medium.com/v2/resize:fit:285/1*QnyP6Ulrb0wo-7fj53YjRw.png)

Open the _LaunchSettings.json_ file and change the `"launchUrl": "swagger"` configuration to `"launchUrl": "scalar/v1"`:

```json
{  
  "$schema": "http://json.schemastore.org/launchsettings.json",  
  "iisSettings": {  
    "windowsAuthentication": false,  
    "anonymousAuthentication": true,  
    "iisExpress": {  
      "applicationUrl": "http://localhost:39471",  
      "sslPort": 44375  
    }  
  },  
  "profiles": {  
    "http": {  
      "commandName": "Project",  
      "dotnetRunMessages": true,  
      "launchBrowser": true,  
      "launchUrl": "scalar/v1",  
      "applicationUrl": "http://localhost:5290",  
      "environmentVariables": {  
        "ASPNETCORE_ENVIRONMENT": "Development"  
      }  
    },  
    "https": {  
      "commandName": "Project",  
      "dotnetRunMessages": true,  
      "launchBrowser": true,  
      "launchUrl": "scalar/v1",  
      "applicationUrl": "https://localhost:7242;http://localhost:5290",  
      "environmentVariables": {  
        "ASPNETCORE_ENVIRONMENT": "Development"  
      }  
    },  
    "IIS Express": {  
      "commandName": "IISExpress",  
      "launchBrowser": true,  
      "launchUrl": "scalar/v1",  
      "environmentVariables": {  
        "ASPNETCORE_ENVIRONMENT": "Development"  
      }  
    }  
  }  
}
```

# Run the Application

Now let’s run the application:

![Screenshot of the Scalar API documentation UI after running the application, displaying the 'WeatherForecast' endpoint with its details and a 'Test Request' button.](https://miro.medium.com/v2/resize:fit:700/1*rKcURZdrjjgrYWGyG-1WSw.png)

**Test Request**:

![Screenshot of the Scalar API documentation 'Test Request' feature, showing the request details, headers, and the JSON response body for the WeatherForecast API endpoint.](https://miro.medium.com/v2/resize:fit:700/1*1_co78RweoE73hg0I2IMoA.png)

Our API documentation now looks amazing!

Using Scalar for API documentation has many benefits that simplify the developer experience. The design is beautiful. The side menu navigation allows us to quickly find sections without scrolling, and the _Test Request_ feature lets us test API endpoints directly from the documentation.

The complete source code can be found here:

[
## GitHub - juldhais/ScalarExample: Create a Beautiful API Documentation with Scalar in ASP .NET Core

### Create a Beautiful API Documentation with Scalar in ASP .NET Core - juldhais/ScalarExample

github.com

](https://github.com/juldhais/ScalarExample?source=post_page-----d3d4d17570a6---------------------------------------)

Thank you for reading 👍