```yaml
---
title: "oThe Secret to FREE PDF Reporting in .NET — Handlebars + PuppeteerSharp | by Michael Maurice | Aug, 2025 | Medium"
source: https://medium.com/@michaelmaurice410/the-secret-to-free-pdf-reporting-in-net-handlebars-puppeteersharp-081c5f963b0b
date_published: 2025-08-09T17:02:05.890Z
date_captured: 2025-08-22T11:02:01.594Z
domain: medium.com
author: Michael Maurice
category: general
technologies: [Handlebars.NET, PuppeteerSharp, .NET, ASP.NET Core, Chromium, Docker, Azure App Service, Chart.js, IronPDF, Google Fonts, QR Server API]
programming_languages: [C#, HTML, CSS, JavaScript, Bash]
tags: [pdf-generation, dotnet, reporting, handlebars, puppeteersharp, web-technologies, html-to-pdf, cross-platform, template-engine, headless-browser]
key_concepts: [html-templating, headless-chrome, dependency-injection, performance-optimization, error-handling, containerization, print-media-queries, custom-helpers, browser-instance-management]
code_examples: false
difficulty_level: intermediate
summary: |
  This comprehensive guide introduces a free and powerful solution for PDF reporting in .NET, combining Handlebars.NET for HTML templating with PuppeteerSharp for headless Chrome rendering. It details the setup process, explains the core components, and provides extensive C#, HTML, CSS, and JavaScript code examples for generating dynamic and professional documents like invoices and multi-page reports. The article also covers advanced topics such as performance optimization, robust error handling, and deployment considerations for Docker and Azure App Service. This approach offers significant cost savings and design flexibility, leveraging modern web standards to produce enterprise-quality PDFs without licensing fees.
---
```

# oThe Secret to FREE PDF Reporting in .NET — Handlebars + PuppeteerSharp | by Michael Maurice | Aug, 2025 | Medium

# The Secret to FREE PDF Reporting in .NET — Handlebars + PuppeteerSharp

![Diagram illustrating the PDF reporting process in .NET. A '.NET Application' (represented by a code file icon) feeds into a 'Handlebars.NET Template Engine' (represented by a document with a database and pie chart icon). This then feeds into 'PuppeteerSharp Headless Chrome' (represented by the Chrome logo), which finally outputs a 'Generated PDF Report' (represented by a PDF document icon with a bar chart).](https://miro.medium.com/v2/resize:fit:700/1*5YTudGN0oOLmvDZdHNSveQ.png)

**If you want the full source code, download it from this link:** [https://www.elitesolutions.shop/](https://www.elitesolutions.shop/)

PDF generation has long been a costly necessity for .NET developers. Commercial libraries like IronPDF deliver excellent results but come with licensing fees that can strain project budgets. Enter the game-changing combination of Handlebars.NET and PuppeteerSharp — a completely free, powerful solution that rivals expensive commercial alternatives. This comprehensive guide reveals how to create professional, dynamic PDF reports using HTML templating and headless Chrome technology, delivering enterprise-quality results without the enterprise price tag.

# Why This Combination is Revolutionary

# The Problem with Traditional PDF Generation

Most .NET developers have experienced the frustration of PDF generation:

*   **Expensive Commercial Libraries:** Solutions like IronPDF, while excellent, can cost thousands in licensing fees
*   **Limited Styling Control:** Native PDF libraries require complex positioning and styling APIs
*   **Poor HTML Support:** Many free libraries struggle with modern CSS and JavaScript
*   **Platform Dependencies:** Some solutions are Windows-only or require specific runtime environments

# The Handlebars + PuppeteerSharp Solution

This powerful combination offers:

*   **Complete Creative Freedom:** Design PDFs using familiar HTML, CSS, and JavaScript
*   **Zero Licensing Costs:** Both libraries are completely free for commercial use
*   **Professional Quality:** Pixel-perfect rendering using Chrome’s proven engine
*   **Modern Web Standards:** Full support for CSS Grid, Flexbox, and modern web features
*   **Cross-Platform:** Runs on Windows, Linux, and macOS

# Understanding the Components

# Handlebars.NET: The Template Engine

Handlebars.NET is a powerful templating engine that compiles templates into optimized .NET code. Unlike Razor views, it doesn’t require ASP.NET Core, making it perfect for background services, desktop applications, and microservices.

Key Features:

*   Logic-less templates with conditional rendering and loops
*   Custom helper functions for complex data formatting
*   Partial template support for reusable components
*   Blazing fast performance through IL bytecode compilation

# PuppeteerSharp: The PDF Engine

PuppeteerSharp is a .NET port of Google’s Puppeteer library, providing programmatic control over headless Chrome. This means your PDFs are rendered using the same engine that powers billions of web pages daily.

Key Advantages:

*   Accurate HTML rendering identical to what users see in Chrome
*   JavaScript execution for dynamic content and charts
*   Complete CSS support including print media queries
*   Image and font handling with automatic resource loading

# Project Setup and Dependencies

# Installing Required Packages

```bash
# Create new console application
dotnet new console -n PdfReportingDemo
cd PdfReportingDemo
# Install required packages
dotnet add package Handlebars.Net --version 2.1.6
dotnet add package PuppeteerSharp --version 19.0.2
# Optional: For web applications
dotnet add package Microsoft.AspNetCore.App
```

# Project Structure

```
PdfReportingDemo/
├── Templates/
│   ├── invoice.hbs
│   ├── report.hbs
│   └── Partials/
│       ├── header.hbs
│       └── footer.hbs
├── Services/
│   ├── PdfService.cs
│   └── TemplateService.cs
├── Models/
│   └── InvoiceModel.cs
└── Program.cs
```

# Basic Service Configuration

```csharp
using PuppeteerSharp;
using HandlebarsDotNet;

public class PdfGenerationService
{
    private readonly IHandlebars _handlebars;
    private IBrowser? _browser;

    public PdfGenerationService()
    {
        _handlebars = Handlebars.Create();
        RegisterHelpers();
    }

    private void RegisterHelpers()
    {
        // Register custom helpers for formatting
        _handlebars.RegisterHelper("formatCurrency", (writer, context, parameters) =>
        {
            if (parameters.Length > 0 && decimal.TryParse(parameters[0].ToString(), out var amount))
            {
                writer.WriteSafeString(amount.ToString("C"));
            }
        });
        _handlebars.RegisterHelper("formatDate", (writer, context, parameters) =>
        {
            if (parameters.Length > 0 && DateTime.TryParse(parameters[0].ToString(), out var date))
            {
                writer.WriteSafeString(date.ToString("MMM dd, yyyy"));
            }
        });
        _handlebars.RegisterHelper("multiply", (writer, context, parameters) =>
        {
            if (parameters.Length >= 2
                && decimal.TryParse(parameters[0].ToString(), out var a)
                && decimal.TryParse(parameters[1].ToString(), out var b))
            {
                writer.WriteSafeString((a * b).ToString("F2"));
            }
        });
    }

    public async Task InitializeBrowserAsync()
    {
        if (_browser == null)
        {
            // Download Chromium if not present
            using var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            // Launch headless browser
            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[]
                {
                    "--no-sandbox",
                    "--disable-setuid-sandbox",
                    "--disable-dev-shm-usage",
                    "--disable-web-security",
                    "--allow-running-insecure-content"
                }
            });
        }
    }

    public async Task<byte[]> GeneratePdfAsync(string templatePath, object data, PdfOptions? options = null)
    {
        await InitializeBrowserAsync();
        // Compile template
        var templateSource = await File.ReadAllTextAsync(templatePath);
        var template = _handlebars.Compile(templateSource);
        var html = template(data);
        // Generate PDF using PuppeteerSharp
        using var page = await _browser!.NewPageAsync();
          
        await page.SetContentAsync(html, new NavigationOptions
        {
            WaitUntil = new[] { WaitUntilNavigation.NetworkIdle0 }
        });
        var pdfOptions = options ?? new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true,
            MarginOptions = new MarginOptions
            {
                Top = "20px",
                Right = "20px",
                Bottom = "20px",
                Left = "20px"
            }
        };
        return await page.PdfAsync(pdfOptions);
    }

    public async Task DisposeAsync()
    {
        if (_browser != null)
        {
            await _browser.CloseAsync();
            _browser.Dispose();
        }
    }
}
```

# Creating Professional Invoice Templates

# Basic Invoice Template Structure

Create a professional invoice template (`Templates/invoice.hbs`):

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Invoice #{{invoiceNumber}}</title>
    <style>        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            font-size: 12px;
            line-height: 1.6;
            color: #333;
            background: white;
        }
        .container {
            max-width: 800px;
            margin: 0 auto;
            padding: 40px;
        }
        .header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            margin-bottom: 40px;
            border-bottom: 2px solid #e0e0e0;
            padding-bottom: 20px;
        }
        .company-info h1 {
            font-size: 28px;
            color: #2c3e50;
            margin-bottom: 10px;
        }
        .company-info p {
            color: #7f8c8d;
            margin-bottom: 5px;
        }
        .invoice-details {
            text-align: right;
        }
        .invoice-details h2 {
            font-size: 24px;
            color: #e74c3c;
            margin-bottom: 10px;
        }
        .invoice-meta {
            background: #f8f9fa;
            padding: 20px;
            margin-bottom: 30px;
            border-radius: 8px;
        }
        .billing-info {
            display: flex;
            justify-content: space-between;
            margin-bottom: 30px;
        }
        .bill-to, .ship-to {
            flex: 1;
            margin-right: 20px;
        }
        .bill-to h3, .ship-to h3 {
            color: #2c3e50;
            margin-bottom: 10px;
            font-size: 16px;
        }
        .items-table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 30px;
        }
        .items-table th {
            background: #34495e;
            color: white;
            padding: 15px 10px;
            text-align: left;
            font-weight: 600;
        }
        .items-table td {
            padding: 12px 10px;
            border-bottom: 1px solid #e0e0e0;
        }
        .items-table tbody tr:nth-child(even) {
            background: #f8f9fa;
        }
        .text-right {
            text-align: right;
        }
        .totals {
            float: right;
            width: 300px;
            margin-top: 20px;
        }
        .totals table {
            width: 100%;
            border-collapse: collapse;
        }
        .totals td {
            padding: 8px 10px;
            border-bottom: 1px solid #e0e0e0;
        }
        .total-row {
            font-weight: bold;
            background: #e8f5e8;
        }
        .total-row td {
            border-top: 2px solid #27ae60;
            font-size: 16px;
        }
        .footer {
            clear: both;
            margin-top: 60px;
            padding-top: 20px;
            border-top: 1px solid #e0e0e0;
            text-align: center;
            color: #7f8c8d;
        }
        .payment-terms {
            background: #fff3cd;
            border: 1px solid #ffeaa7;
            border-radius: 5px;
            padding: 15px;
            margin-top: 20px;
        }
        .payment-terms h4 {
            color: #d68910;
            margin-bottom: 10px;
        }
        @media print {
            .container {
                padding: 20px;
            }
              
            body {
                print-color-adjust: exact;
                -webkit-print-color-adjust: exact;
            }
        }    </style>
</head>
<body>
    <div class="container">
        <!-- Header Section -->
        <div class="header">
            <div class="company-info">
                <h1>{{companyName}}</h1>
                <p>{{companyAddress}}</p>
                <p>{{companyCity}}, {{companyState}} {{companyZip}}</p>
                <p>Phone: {{companyPhone}}</p>
                <p>Email: {{companyEmail}}</p>
            </div>
            <div class="invoice-details">
                <h2>INVOICE</h2>
                <p><strong>Invoice #:</strong> {{invoiceNumber}}</p>
                <p><strong>Date:</strong> {{formatDate invoiceDate}}</p>
                <p><strong>Due Date:</strong> {{formatDate dueDate}}</p>
            </div>
        </div>
        <!-- Invoice Meta Information -->
        <div class="invoice-meta">
            <div class="billing-info">
                <div class="bill-to">
                    <h3>Bill To:</h3>
                    <p><strong>{{billTo.name}}</strong></p>
                    <p>{{billTo.company}}</p>
                    <p>{{billTo.address}}</p>
                    <p>{{billTo.city}}, {{billTo.state}} {{billTo.zip}}</p>
                    {{#if billTo.email}}
                    <p>{{billTo.email}}</p>
                    {{/if}}
                </div>
                {{#if shipTo}}
                <div class="ship-to">
                    <h3>Ship To:</h3>
                    <p><strong>{{shipTo.name}}</strong></p>
                    <p>{{shipTo.company}}</p>
                    <p>{{shipTo.address}}</p>
                    <p>{{shipTo.city}}, {{shipTo.state}} {{shipTo.zip}}</p>
                </div>
                {{/if}}
            </div>
        </div>
        <!-- Items Table -->
        <table class="items-table">
            <thead>
                <tr>
                    <th>Description</th>
                    <th class="text-right">Qty</th>
                    <th class="text-right">Rate</th>
                    <th class="text-right">Amount</th>
                </tr>
            </thead>
            <tbody>
                {{#each items}}
                <tr>
                    <td>
                        <strong>{{description}}</strong>
                        {{#if details}}
                        <br><small style="color: #7f8c8d;">{{details}}</small>
                        {{/if}}
                    </td>
                    <td class="text-right">{{quantity}}</td>
                    <td class="text-right">{{formatCurrency rate}}</td>
                    <td class="text-right">{{formatCurrency (multiply quantity rate)}}</td>
                </tr>
                {{/each}}
            </tbody>
        </table>
        <!-- Totals Section -->
        <div class="totals">
            <table>
                <tr>
                    <td>Subtotal:</td>
                    <td class="text-right">{{formatCurrency subtotal}}</td>
                </tr>
                {{#if discountAmount}}
                <tr>
                    <td>Discount ({{discountPercent}}%):</td>
                    <td class="text-right">-{{formatCurrency discountAmount}}</td>
                </tr>
                {{/if}}
                {{#if taxAmount}}
                <tr>
                    <td>Tax ({{taxPercent}}%):</td>
                    <td class="text-right">{{formatCurrency taxAmount}}</td>
                </tr>
                {{/if}}
                <tr class="total-row">
                    <td><strong>Total:</strong></td>
                    <td class="text-right"><strong>{{formatCurrency total}}</strong></td>
                </tr>
            </table>
        </div>
        {{#if paymentTerms}}
        <div class="payment-terms">
            <h4>Payment Terms</h4>
            <p>{{paymentTerms}}</p>
        </div>
        {{/if}}
        {{#if notes}}
        <div style="margin-top: 20px;">
            <h4>Notes:</h4>
            <p>{{notes}}</p>
        </div>
        {{/if}}
        <div class="footer">
            <p>Thank you for your business!</p>
        </div>
    </div>
</body>
</html>
```

# Advanced Template with Images and QR Codes

For more sophisticated invoices with logos and QR codes:

```html
{{! Enhanced invoice template with logo and QR code }}
<!DOCTYPE html>
<html>
<head>
    <title>Invoice #{{invoiceNumber}}</title>
    <style>        /* ... base styles ... */
          
        .logo-container {
            text-align: center;
            margin-bottom: 30px;
        }.company-logo {
            max-width: 200px;
            max-height: 100px;
        }
        .qr-code {
            text-align: center;
            margin-top: 30px;
        }
        .qr-code img {
            width: 120px;
            height: 120px;
        }
        .status-badge {
            position: absolute;
            top: 50px;
            right: 50px;
            padding: 10px 20px;
            border-radius: 5px;
            font-weight: bold;
            font-size: 18px;
            transform: rotate(-15deg);
        }
        .status-paid {
            background: #d4edda;
            color: #155724;
            border: 2px solid #c3e6cb;
        }
        .status-overdue {
            background: #f8d7da;
            color: #721c24;
            border: 2px solid #f5c6cb;
        }    </style>
</head>
<body>
    <div class="container">
        {{#if status}}
        <div class="status-badge status-{{status}}">
            {{#eq status "paid"}}PAID{{/eq}}
            {{#eq status "overdue"}}OVERDUE{{/eq}}
        </div>
        {{/if}}
        {{#if logoBase64}}
        <div class="logo-container">
            <img src="data:image/png;base64,{{logoBase64}}" alt="Company Logo" class="company-logo">
        </div>
        {{/if}}
        <!-- Rest of template content -->
        <!-- ... existing header, billing, items sections ... -->
        {{#if qrCodeData}}
        <div class="qr-code">
            <h4>Payment QR Code</h4>
            <img src="https://api.qrserver.com/v1/create-qr-code/?size=120x120&data={{qrCodeData}}" alt="Payment QR Code">
            <p><small>Scan to pay online</small></p>
        </div>
        {{/if}}
    </div>
</body>
</html>
```

# Advanced Data Models and Helpers

# Comprehensive Invoice Model

```csharp
public class InvoiceModel
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "pending"; // paid, overdue, pending
// Company Information
    public CompanyInfo Company { get; set; } = new();
    // Client Information
    public ClientInfo BillTo { get; set; } = new();
    public ClientInfo? ShipTo { get; set; }
    // Invoice Items
    public List<InvoiceItem> Items { get; set; } = new();
    // Financial Information
    public decimal Subtotal => Items.Sum(i => i.Quantity * i.Rate);
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount => Subtotal * (DiscountPercent / 100);
    public decimal TaxPercent { get; set; }
    public decimal TaxAmount => (Subtotal - DiscountAmount) * (TaxPercent / 100);
    public decimal Total => Subtotal - DiscountAmount + TaxAmount;
    // Additional Information
    public string? PaymentTerms { get; set; }
    public string? Notes { get; set; }
    public string? LogoBase64 { get; set; }
    public string? QrCodeData { get; set; }
}
public class CompanyInfo
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
}
public class ClientInfo
{
    public string Name { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
public class InvoiceItem
{
    public string Description { get; set; } = string.Empty;
    public string? Details { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount => Quantity * Rate;
}
```

# Advanced Handlebars Helpers

```csharp
public static class HandlebarsHelpers
{
    public static void RegisterAllHelpers(IHandlebars handlebars)
    {
        // Equality helper for conditionals
        handlebars.RegisterHelper("eq", (writer, context, parameters) =>
        {
            if (parameters.Length >= 2)
            {
                var isEqual = parameters[0]?.ToString() == parameters[1]?.ToString();
                if (isEqual)
                {
                    // Render the block content if equal
                    var options = parameters[parameters.Length - 1] as HelperOptions;
                    options?.Template(writer, context);
                }
                else
                {
                    // Render else block if not equal
                    var options = parameters[parameters.Length - 1] as HelperOptions;
                    options?.Inverse(writer, context);
                }
            }
        });
// Number formatting with custom culture
        handlebars.RegisterHelper("formatNumber", (writer, context, parameters) =>
        {
            if (parameters.Length >= 1 && decimal.TryParse(parameters[0]?.ToString(), out var number))
            {
                var format = parameters.Length > 1 ? parameters[1].ToString() : "N2";
                var culture = parameters.Length > 2 ? parameters[2].ToString() : "en-US";
                  
                var cultureInfo = new CultureInfo(culture);
                writer.WriteSafeString(number.ToString(format, cultureInfo));
            }
        });
        // Date formatting with timezone support
        handlebars.RegisterHelper("formatDateTz", (writer, context, parameters) =>
        {
            if (parameters.Length >= 1 && DateTime.TryParse(parameters[0]?.ToString(), out var date))
            {
                var format = parameters.Length > 1 ? parameters[1].ToString() : "MMM dd, yyyy";
                var timezone = parameters.Length > 2 ? parameters[2].ToString() : "UTC";
                if (timezone != "UTC")
                {
                    var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
                    date = TimeZoneInfo.ConvertTimeFromUtc(date, timeZoneInfo);
                }
                writer.WriteSafeString(date.ToString(format));
            }
        });
        // Conditional formatting based on value
        handlebars.RegisterHelper("conditionalClass", (writer, context, parameters) =>
        {
            if (parameters.Length >= 3)
            {
                var value = parameters[0]?.ToString();
                var condition = parameters[1]?.ToString();
                var className = parameters[2]?.ToString();
                if (value == condition)
                {
                    writer.WriteSafeString(className);
                }
            }
        });
        // Calculate percentage
        handlebars.RegisterHelper("percentage", (writer, context, parameters) =>
        {
            if (parameters.Length >= 2
                && decimal.TryParse(parameters[0]?.ToString(), out var part)
                && decimal.TryParse(parameters[1]?.ToString(), out var total)
                && total != 0)
            {
                var percentage = (part / total) * 100;
                writer.WriteSafeString($"{percentage:F1}%");
            }
        });
        // Sum helper for arrays
        handlebars.RegisterHelper("sum", (writer, context, parameters) =>
        {
            if (parameters.Length >= 1 && parameters[0] is IEnumerable<object> collection)
            {
                var sum = collection
                    .Select(item =>
                    {
                        var type = item.GetType();
                        var propertyName = parameters.Length > 1 ? parameters[1].ToString() : "Amount";
                        var property = type.GetProperty(propertyName);
                        return property?.GetValue(item);
                    })
                    .Where(value => value != null && decimal.TryParse(value.ToString(), out _))
                    .Sum(value => decimal.Parse(value.ToString()));
                writer.WriteSafeString(sum.ToString("C"));
            }
        });
    }
}
```

# Integration with ASP.NET Core

# PDF Generation Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly PdfGenerationService _pdfService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(PdfGenerationService pdfService, ILogger<ReportsController> logger)
    {
        _pdfService = pdfService;
        _logger = logger;
    }

    [HttpPost("invoice")]
    public async Task<IActionResult> GenerateInvoice([FromBody] InvoiceModel invoice)
    {
        try
        {
            var templatePath = Path.Combine("Templates", "invoice.hbs");
            var pdfBytes = await _pdfService.GeneratePdfAsync(templatePath, invoice);
            var fileName = $"invoice_{invoice.InvoiceNumber}_{DateTime.Now:yyyyMMdd}.pdf";
              
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice PDF for {InvoiceNumber}", invoice.InvoiceNumber);
            return StatusCode(500, "Error generating PDF");
        }
    }

    [HttpPost("invoice/preview")]
    public async Task<IActionResult> PreviewInvoice([FromBody] InvoiceModel invoice)
    {
        try
        {
            var templatePath = Path.Combine("Templates", "invoice.hbs");
              
            // Generate HTML instead of PDF for preview
            var templateSource = await System.IO.File.ReadAllTextAsync(templatePath);
            var handlebars = Handlebars.Create();
            HandlebarsHelpers.RegisterAllHelpers(handlebars);
            var template = handlebars.Compile(templateSource);
            var html = template(invoice);
            return Content(html, "text/html");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice preview for {InvoiceNumber}", invoice.InvoiceNumber);
            return StatusCode(500, "Error generating preview");
        }
    }

    [HttpGet("invoice/{id}")]
    public async Task<IActionResult> GetInvoicePdf(int id)
    {
        try
        {
            // Fetch invoice data from database
            var invoice = await GetInvoiceFromDatabase(id);
            if (invoice == null)
            {
                return NotFound();
            }
            var templatePath = Path.Combine("Templates", "invoice.hbs");
            var pdfBytes = await _pdfService.GeneratePdfAsync(templatePath, invoice);
            var fileName = $"invoice_{invoice.InvoiceNumber}.