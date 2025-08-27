```yaml
---
title: How to Create Command Line Console Applications in .NET
source: https://antondevtips.com/blog/how-to-create-command-line-console-applications-in-dotnet
date_published: 2024-11-26T08:55:21.852Z
date_captured: 2025-08-06T17:30:06.386Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [.NET, Cocona, ASP.NET Core, HtmlAgilityPack, AngleSharp, Microsoft.Extensions]
programming_languages: [C#, Bash]
tags: [command-line, console-app, dotnet, cli, cocona, html-processing, utility, argument-parsing, dependency-injection]
key_concepts: [command-line-interface, argument-parsing, dependency-injection, minimal-api-style, subcommands, help-generation, command-line-options]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces Cocona, a micro-framework designed to simplify the creation of command-line console applications in .NET. It explains how Cocona streamlines argument parsing, command definition, and option handling, drawing parallels to ASP.NET Core's Minimal API. The post demonstrates building a practical `HtmlTool` application with minify, beautify, and validate functionalities, showcasing Cocona's key features like automatic help generation, asynchronous commands, subcommands, and dependency injection. The article highlights Cocona's advantages in reducing boilerplate code and enabling rapid development of feature-rich CLI tools.
---
```

# How to Create Command Line Console Applications in .NET

![A dark blue banner with abstract purple shapes. On the left, a white square icon with a `</>` symbol and the text "dev tips" below it. On the right, large white text reads "HOW TO CREATE COMMAND LINE CONSOLE APPLICATIONS IN .NET".](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fcsharp%2Fcover_csharp_command_line_apps.png&w=3840&q=100)

# How to Create Command Line Console Applications in .NET

Nov 26, 2024

[Download source code](/source-code/how-to-create-command-line-console-applications-in-dotnet)

6 min read

### Newsletter Sponsors

[Master The Clean Architecture](https://www.courses.milanjovanovic.tech/a/aff_q2snzxhl/external?affcode=1486372_j2vpyytw). This comprehensive course will teach you how to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture. Join 3,150+ students to accelerate your growth as a software architect.

[Master The Modular Monolith Architecture](https://www.courses.milanjovanovic.tech/a/aff_l825rpg0/external?affcode=1486372_j2vpyytw). This in-depth course will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario. Join 1,050+ students to accelerate your growth as a software architect.

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

Throughout my career, I have built a ton of console command-line applications. From simple to really complex ones with a lot of commands.

In this blog post, I want to introduce you to a **Cocona** Nuget package that helped me create a new command-line tool this year.

## What is Cocona?

Creating console applications in .NET often involves parsing command-line arguments, handling options, and providing help messages. All these options require a lot of boilerplate-heavy and error-prone code.

[Cocona](https://github.com/mayuki/Cocona) is a micro-framework that streamlines this process, allowing you to focus on your application's core functionality. This library uses ASP.NET Core-like Minimal API style for handling commands.

Features supported by **Cocona**:

*   Command-line option semantics like UNIX tools standard (can handle both `-rf /` and `-r -f /`)
*   Support single command and multiple commands style:
    *   `myapp --foo --bar -n arg0 "arg1"` (e.g. dir, cp, ls ...)
    *   `myapp server -m "Hello world!"` (e.g. dotnet, git, kubectl ...)
*   Built-in help documentation support (see a help message by typing `-h` or `--help`)
*   Built-in similar commands suggestion
*   Highly modulable/customizable CLI framework (Cocona built on top of `Microsoft.Extensions.*` framework. Cocona natively supports Logging, DI, Configuration and ConsoleLifetime)

In this blog post, we'll use **Cocona** to build a command-line tool that performs the following operations on HTML files:

*   **Minify:** Compresses the HTML by removing unnecessary whitespace and comments.
*   **Beautify:** Formats the HTML to make it more readable.
*   **Validate:** Checks the HTML for syntax errors.

## Brief Overview of Command-Line Arguments

In traditional .NET console applications, handling command-line arguments requires parsing the `string[] args` array in the Main method. Here is how you can access `args` in the Program.cs:

```csharp
class Program
{
    static void Main(string[] args)
    {
        var name = args[0];
        Console.WriteLine($"Hello, {name}");
    }
}
```

In Program.cs with a top-level statements `args` is just magically available:

```csharp
var name = args[0];
Console.WriteLine($"Hello, {name}");
```

Handling command-line arguments involves the following:

*   Manually splitting arguments.
*   Validating input.
*   Providing help messages.

**Cocona** abstracts away this complexity, allowing you to define commands and options using attributes and method parameters.

Here is how this code will look like in **Cocona**:

```csharp
CoconaApp.Run((string name) =>
{
    Console.WriteLine($"Hello {name}");
})
```

It looks like a Minimal API endpoint with one argument that is coming from a command-line argument.

## Understanding Commands, Arguments, and Options

When building command-line applications, it's essential to understand the components that make up the command-line interface (CLI). These components allow users to interact with your application.

### Commands

A **command** is a specific action or operation that your application can perform. In the context of CLI applications, commands are typically the first word(s) after the application's name in the command line.

```bash
git commit -m "Commit message"
```

Here, **commit** is a command that tells git to create a new commit.

In Cocona, commands are represented as methods provided into `AddCommand`:

```csharp
var app = CoconaApp.Create();
app.AddCommand("commit", (string message) => { });
```

### Arguments

An **argument** (or positional argument) is a value that a command requires to perform its operation. Arguments are typically specified after the command and are not prefixed by any indicator (like - or --):

```bash
cp source.txt destination.txt
```

Here, `source.txt` and `destination.txt` are arguments to the **cp** (copy) command.

In Cocona, arguments are represented as method parameters decorated with the `[Argument]` attribute or inferred from the parameter position:

```csharp
app.AddCommand("copy", (string sourceFile, string destinationFile) => { });
app.AddCommand("copy", ([Argument] string sourceFile, [Argument] string destinationFile) => { });
```

### Options

An **option** (or flag) modifies the behavior of a command. Options are typically prefixed with one or two dashes (e.g., -o or --output) and can be followed by a value or act as a boolean flag.

```bash
ls -l --color
```

Here, `-l` and `--color` are options that change how the `ls` command behaves.

In Cocona, options are represented as method parameters decorated with the `[Option]` attribute:

```csharp
app.AddCommand("ls", ([Option('l')] bool longFormat, [Option("color")] bool useColor) => { });
```

### Putting It All Together

When a user runs your application from the command line, they might provide a combination of commands, arguments, and options:

```bash
appname command argument1 argument2 --option1 value1 -o value2
```

*   appname: The name of your application.
*   command: The action to perform.
*   argument1, argument2: Positional arguments required by the command.
*   `--option1 value1`, `-o value2`: Options that modify the command's behavior.

## Building the HTML Tool with Cocona

We'll build a command-line application named HtmlTool that accepts commands to minify, beautify, and validate HTML files.

We'll define the following commands:

*   Minify: app.AddCommand("minify", ...)
*   Beautify: app.AddCommand("beautify", ...)
*   Validate: app.AddCommand("validate", ...)

Here is an initial template for our application:

```csharp
var app = CoconaApp.Create();

app.AddCommand("minify", (string inputFile, string outputFile) => { })
    .WithDescription("Minify an HTML file by removing unnecessary whitespace and comments");

app.AddCommand("beautify", (string inputFile, string outputFile) => { })
    .WithDescription("Beautify an HTML file for better readability");

app.AddCommand("validate", (string inputFile) => { })
    .WithDescription("Minify an HTML file by removing unnecessary whitespace and comments");

app.Run();
```

First, we create a Cocona application by calling `CoconaApp.Create` method. Next, we can register commands with an `AddCommand` method.

### Implementing Minify Command

```csharp
app.AddCommand("minify", (
    [Option('i', Description = "The input file path")] string inputFile,
    [Option('o', Description = "The output file path")] string? outputFile = null
) =>
{
    var htmlContent = File.ReadAllText(inputFile);
    var minifiedHtml = MinifyHtml(htmlContent);

    var outPath = outputFile ?? GetOutputFilePath(inputFile, "min");

    File.WriteAllText(outPath, minifiedHtml);

    Console.WriteLine($"Minified HTML saved to {outPath}");
}).WithDescription("Minify an HTML file by removing unnecessary whitespace and comments");
```

Command parameters:

*   `inputFile`: Positional argument for the input HTML file.
*   `outputFile`: Optional parameter (`-o` or `--output`) for specifying the output file path.

Here is the code for the helper methods:

```csharp
string MinifyHtml(string html)
{
    // Remove comments
    var noComments = Regex.Replace(html, @"<!--(.*?)-->", "", RegexOptions.Singleline);

    // Remove unnecessary whitespace including tabs
    var noWhitespace = Regex.Replace(noComments, @"\s+", " ");

    // Remove spaces between tags
    var minified = Regex.Replace(noWhitespace, @">\s+<", "><");

    return minified.Trim();
}

string GetOutputFilePath(string inputFile, string suffix)
{
    var directory = Path.GetDirectoryName(inputFile);
    var filename = Path.GetFileNameWithoutExtension(inputFile);
    var extension = Path.GetExtension(inputFile);

    return Path.Combine(directory, $"{filename}_{suffix}{extension}");
}
```

### Implementing Beautify Command

```csharp
app.AddCommand("beautify", (
    [Option('i', Description = "The input file path")] string inputFile,
    [Option('o', Description = "The output file path")] string? outputFile = null
) =>
{
    var htmlContent = File.ReadAllText(inputFile);

    var beautifiedHtml = BeautifyHtml(htmlContent);

    var outPath = outputFile ?? GetOutputFilePath(inputFile, "beautify");

    File.WriteAllText(outPath, beautifiedHtml);

    Console.WriteLine($"Beautified HTML saved to {outPath}");
}).WithDescription("Beautify an HTML file for better readability");
```

Command Parameters:

*   `inputFile`: Positional argument for the input HTML file.
*   `outputFile`: Optional parameter for the output file path.

> You can download the full source code for this application at the end of the blog post

### Implementing Validate Command

To implement a Validate command, we need to download the following Nuget packages that allow parsing and formatting the HTML:

```bash
dotnet add package HtmlAgilityPack
dotnet add package AngleSharp
```
```csharp
app.AddCommand("validate", (
    [Option('i', Description = "The input file path")] string inputFile
) =>
{
    var htmlContent = File.ReadAllText(inputFile);

    var isValid = ValidateHtml(htmlContent);

    if (!isValid)
    {
        Console.WriteLine("The HTML is invalid.");
        return;
    }
    
    Console.WriteLine("The HTML is valid.");
}).WithDescription("Validate an HTML file for syntax errors");

bool ValidateHtml(string html)
{
    var htmlDoc = new HtmlDocument();
    htmlDoc.LoadHtml(html);

    return !htmlDoc.ParseErrors.Any();
}
```

Command Parameters:

*   `inputFile`: Positional argument for the input HTML file.

## Using the Application

I recommend publishing the HtmlTool app as a SingleFile, for example, for Windows:

```bash
dotnet publish -r win-x64 -p:PublishSingleFile=true --self-contained false
```

Let's see how to use our HtmlTool application.

### Displaying Help Information

Cocona automatically generates help messages. Run:

```bash
HtmlTool --help
```
```text
Usage: HtmlTool [command] [arguments] [options]

Commands:
  minify      Minify an HTML file by removing unnecessary whitespace and comments
  beautify    Beautify an HTML file for better readability
  validate    Validate an HTML file for syntax errors

Options:
  -h, --help    Show help message
  --version     Show version
```

### Minifying an HTML File

```bash
HtmlTool minify -i sample.html -o minified.html
```

This will create a `minified.html` file with the minified content.

### Beautifying an HTML File

```bash
HtmlTool beautify -i minified.html -o beautified.html
```

This will create a `beautified.html` file with beautified content using 1 tab for indentation.

### Validating an HTML File

```bash
HtmlTool validate -i sample.html
```

This will output whether the HTML is valid or not.

## Additional Features of Cocona

### Asynchronous Commands

Define async commands by returning Task.

```csharp
app.AddCommand("minify", async (string inputFile, string outputFile) =>
{
    // Async code here
});
```

### Subcommands

Organize commands using subcommands.

```csharp
var htmlCommands = new CommandCollection();
htmlCommands.AddCommand("minify", ...);
htmlCommands.AddCommand("beautify", ...);

app.AddSubCommand("html", htmlCommands, "Commands for HTML processing");
```

### Dependency Injection

If you need dependency injection, you can configure services when creating the app.

```csharp
var app = CoconaApp.CreateBuilder()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IHtmlProcessor, HtmlProcessor>();
    })
    .Build();

// Use the service in your commands
app.AddCommand("minify", ([FromService] IHtmlProcessor htmlProcessor,
    string inputFile, string outputFile) =>
{
    // Use htmlProcessor
});
```

For more information, you can read an official Cocona documentation on their [GitHub page](https://github.com/mayuki/Cocona).

## Summary

Cocona simplifies the development of .NET console applications by reducing boilerplate code and providing powerful features out of the box.

Cocona has the following advantages:

*   **Simplicity:** Define commands, arguments, and options directly when adding commands without the need to write boilerplate code.
*   **Automatic Help Generation:** Cocona provides help messages without additional code.
*   **Flexible Command and SubCommand Registration:** Use either method-based commands or inline command definitions.
*   **Asynchronous and DI Support:** You can define async commands easily and use services from the DI container.

With Cocona you can quickly build feature-rich tools with minimal effort.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/how-to-create-command-line-console-applications-in-dotnet)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-create-command-line-console-applications-in-dotnet&title=How%20to%20Create%20Command%20Line%20Console%20Applications%20in%20.NET)[X](https://twitter.com/intent/tweet?text=How%20to%20Create%20Command%20Line%20Console%20Applications%20in%20.NET&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-create-command-line-console-applications-in-dotnet)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-create-command-line-console-applications-in-dotnet)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.