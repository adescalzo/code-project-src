```yaml
---
title: How to Create and Convert PDF Documents in ASP.NET Core
source: https://antondevtips.com/blog/how-to-create-and-convert-pdf-documents-in-aspnetcore
date_published: 2025-06-10T07:45:43.098Z
date_captured: 2025-08-06T16:35:45.002Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [ASP.NET Core, .NET, QuestPDF, IronPDF, Aspose.PDF, NuGet, Razor, TailwindCSS, Chromium, Razor.Templating.Core]
programming_languages: [C#, HTML, CSS, JavaScript]
tags: [pdf-generation, pdf-conversion, dotnet, aspnet-core, library-comparison, html-to-pdf, reporting, razor-views, document-automation]
key_concepts: [pdf-generation, html-to-pdf-conversion, razor-views, fluent-api, library-comparison, performance-optimization, document-automation, programmatic-pdf-creation]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide on creating and converting PDF documents within ASP.NET Core applications. It introduces and compares three prominent .NET PDF libraries: QuestPDF, IronPDF, and Aspose.PDF, illustrating their usage with practical C# code examples. A real-world scenario demonstrates generating a detailed monthly sales report using IronPDF in conjunction with Razor views and TailwindCSS for rich HTML content. The post concludes with a detailed comparison, highlighting IronPDF's advantages in performance, ease of use, and HTML to PDF conversion capabilities over its competitors, positioning it as a highly recommended solution for modern PDF handling in .NET.]
---
```

# How to Create and Convert PDF Documents in ASP.NET Core

![Cover image for the article titled "How to Create and Convert PDF Documents in ASP.NET Core" with a "dev tips" logo.](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Faspnetcore%2Fcover_asp_pdfs.png&w=3840&q=100)

# How to Create and Convert PDF Documents in ASP.NET Core

Jun 10, 2025

[Download source code](/source-code/how-to-create-and-convert-pdf-documents-in-aspnetcore)

8 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

Creating and managing PDF documents is a common and crucial requirement for many applications built with .NET. You may need to generate invoices, reports, agreements, or transform content from web pages and other formats.

To make sure you deliver professional documents without lags — you need a reliable PDF library.

In this post, we will explore the following topics:

*   Creating PDFs from scratch.
*   How to handle conversions between PDF and other popular formats.
*   Real-world scenarios on generating PDF documents in ASP.NET Core.
*   Comparison of the popular PDF libraries: performance, ease of use, developer experience, documentation, support and licensing.

We will explore the following PDF libraries for .NET:

*   QuestPDF
*   IronPDF
*   Aspose.PDF

By the end, you'll understand which library best fits your project's needs, saving you valuable time and ensuring your application's PDF handling meets high professional standards.

Let's dive in.

[](#creating-pdf-documents-with-questpdf)

## Creating PDF Documents with QuestPDF

[QuestPDF](https://www.questpdf.com/) is a popular open-source PDF generation library for .NET developers. Its primary goal is to simplify PDF creation through a fluent API.

Here is how you can create a PDF document with QuestPDF:

```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

var pdf = Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(2, Unit.Centimetre);
        page.PageColor(Colors.White);
        page.DefaultTextStyle(x => x.FontSize(14));

        page.Header()
            .Text("Monthly Sales Report")
            .SemiBold()
            .FontSize(20)
            .FontColor(Colors.Blue.Medium);

        page.Content()
            .Column(x =>
            {
                x.Spacing(10);
                x.Item().Text("Generated on: " + DateTime.Now.ToShortDateString());
                x.Item().Text("Total Sales: $15,450");
                x.Item().Text("Best-selling product: Wireless Headphones");
            });

        page.Footer()
            .AlignCenter()
            .Text(x =>
            {
                x.Span("Page ");
                x.CurrentPageNumber();
            });
    });
});

pdf.GeneratePdf("MonthlySalesReport.pdf");
```

QuestPDF allows you to use C# and fluent API to create PDF documents. It's easy to use and has good documentation.

However, it has the following limitations:

*   Doesn't support converting HTML to PDF: Unable to convert HTML content directly into PDFs, limiting versatility in web applications.
*   Limited Scope of Features: Advanced tasks, like complex document manipulation and interactive elements, are difficult to achieve.
*   A lot of code: even basic documents can require a lot of boilerplate.

QuestPDF is a solid choice for simple use cases but may fall short in enterprise-grade scenarios. Real-world projects often require more advanced capabilities, such as converting HTML pages directly to PDF, performing detailed image and text manipulation, or handling large documents efficiently. In these cases, developers usually turn to more powerful libraries like [IronPDF](https://ironpdf.com?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=june-2025) or [Aspose.PDF](https://products.aspose.com/pdf/).

> Note: QuestPDF offers a free license for smaller companies or development purposes, but commercial use for larger organizations requires obtaining a [commercial license](https://www.questpdf.com/license/).

At the end of the day, it's much simpler to create a rich HTML document with CSS styling and turn it into a PDF with a few lines of code. This way you can create a more complex PDF document much faster than doing it with C# manually.

That's how I was always creating my PDF documents in production.

When selecting a PDF library, the quality and flexibility of PDF creation capabilities are among the most critical factors. That's why my favourite PDF library is IronPDF. Let's explore it closely.

[](#creating-pdf-documents-with-ironpdf)

## Creating PDF Documents with IronPDF

[IronPDF](https://ironpdf.com?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=june-2025) is especially well-known for its intuitive and developer-friendly approach to generating PDFs from HTML content. Its primary strength lies in converting HTML and CSS into pixel-perfect PDFs quickly and easily.

Here is how you can create a PDF document with IronPDF:

```csharp
using IronPdf;

var htmlContent = @"
<html>
<head>
    <style>
        h1 { color: blue; }
        p { font-size: 16px; }
    </style>
</head>
<body>
    <h1>IronPDF Example</h1>
    <p>This PDF was created effortlessly using IronPDF from HTML content!</p>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(htmlContent);
pdf.SaveAs("IronPdfGenerated.pdf");
```

With IronPDF you can create a PDF document from HTML content in just three lines of code.

IronPDF offers the following features:

*   PDF Creation and Editing
*   HTML, DOCX, RTF, XML, MD to PDF Conversion
*   Image to PDF Conversion
*   PDF FROM URL
*   PDF to HTML
*   Sign and Secure PDFs

[](#realworld-example-of-creating-pdf-documents-in-aspnet-core)

## Real-World Example of Creating PDF Documents in ASP.NET Core

Let's build a "Monthly Sales Report" PDF document.

We have the following models:

```csharp
public class SalesReportRequest
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int DepartmentId { get; set; }
}

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class SalesEntry
{
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
    public DateTime SaleDate { get; set; }
}

public class MonthlySalesReport
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public Department Department { get; set; } = new();
    public List<SalesEntry> Sales { get; set; } = [];
    public decimal TotalSales => Sales.Sum(s => s.TotalPrice);
    public int TotalItems => Sales.Sum(s => s.Quantity);
    public int TotalUniqueProducts => Sales.Select(s => s.ProductName).Distinct().Count();
    public DateTime GeneratedOn { get; set; } = DateTime.Now;
}
```

As we already know, making PDF documents is pretty easy from the HTML content. But how we can quickly generate a rich professional-looking HTML document in ASP.NET Core?

For this purpose, we can use [Razor views](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/overview?view=aspnetcore-9.0). They are used in ASP.NET Core MVC, Razor Pages and Blazor applications.

Razor views have `.cshtml` extension and allow you top use C# inside the HTML markup.

Let's build a "Monthly Sales Report" PDF document using Razor views.

First, let's install the following NuGet packages:

```bash
dotnet add package Razor.Templating.Core
dotnet add package IronPdf
```

`Razor.Templating.Core` package allows you to use Razor views in any assembly, even a class library, so you don't need to create an ASP.NET Core MVC application.

I am a big fan of TailwindCSS, it simplifies the CSS styling and makes it easier to create responsive layouts.

Let's create a `MonthlySalesReport.cshtml` file in the `Views` folder:

```csharp
@model CreatingPDFs.Models.MonthlySalesReport

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Monthly Sales Report</title>
    <script src="https://cdn.tailwindcss.com"></script>
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }
        .page-break {
            page-break-after: always;
        }
    </style>
</head>
<body class="bg-gray-50">
    <div class="container mx-auto p-8 max-w-6xl bg-white shadow-lg">
        <!-- Header -->
        <div class="flex justify-between items-center border-b-2 border-gray-200 pb-4 mb-6">
            <div>
                <h1 class="text-3xl font-bold text-gray-800">Monthly Sales Report</h1>
                <p class="text-gray-600 mt-2">Department: <span class="font-semibold">@Model.Department.Name</span></p>
            </div>
            <div class="text-right">
                <p class="text-gray-600">Period: <span class="font-semibold">@Model.DateFrom.ToString("MMM d, yyyy") - @Model.DateTo.ToString("MMM d, yyyy")</span></p>
                <p class="text-gray-500 text-sm">Generated on: @Model.GeneratedOn.ToString("MMMM d, yyyy - HH:mm")</p>
            </div>
        </div>

        <!-- Summary Section -->
        <div class="mb-8 bg-gray-100 p-4 rounded-lg">
            <h2 class="text-xl font-bold text-gray-700 mb-3">Summary</h2>
            <div class="grid grid-cols-3 gap-4">
                <div class="bg-white p-4 rounded-md shadow">
                    <p class="text-gray-500 text-sm">Total Sales</p>
                    <p class="text-2xl font-bold text-green-600">$@Model.TotalSales.ToString("N2")</p>
                </div>
                <div class="bg-white p-4 rounded-md shadow">
                    <p class="text-gray-500 text-sm">Total Items Sold</p>
                    <p class="text-2xl font-bold text-blue-600">@Model.TotalItems.ToString("N0")</p>
                </div>
                <div class="bg-white p-4 rounded-md shadow">
                    <p class="text-gray-500 text-sm">Unique Products</p>
                    <p class="text-2xl font-bold text-purple-600">@Model.TotalUniqueProducts.ToString("N0")</p>
                </div>
            </div>
        </div>

        <!-- Sales Table -->
        <div>
            <h2 class="text-xl font-bold text-gray-700 mb-3">Sales Details</h2>
            <table class="min-w-full bg-white rounded-lg overflow-hidden border border-gray-200">
                <thead class="bg-gray-800 text-white">
                    <tr>
                        <th class="py-3 px-4 text-left">Date</th>
                        <th class="py-3 px-4 text-left">Product</th>
                        <th class="py-3 px-4 text-right">Quantity</th>
                        <th class="py-3 px-4 text-right">Unit Price</th>
                        <th class="py-3 px-4 text-right">Total</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-gray-200">
                    @foreach (var sale in Model.Sales)
                    {
                        <tr class="hover:bg-gray-50">
                            <td class="py-2 px-4 text-gray-700">@sale.SaleDate.ToString("MMM d, yyyy")</td>
                            <td class="py-2 px-4 text-gray-700">@sale.ProductName</td>
                            <td class="py-2 px-4 text-right text-gray-700">@sale.Quantity</td>
                            <td class="py-2 px-4 text-right text-gray-700">$@sale.UnitPrice.ToString("N2")</td>
                            <td class="py-2 px-4 text-right font-medium text-gray-900">$@sale.TotalPrice.ToString("N2")</td>
                        </tr>
                    }
                </tbody>
                <tfoot class="bg-gray-100">
                    <tr>
                        <td colspan="4" class="py-3 px-4 text-right font-bold text-gray-700">Grand Total:</td>
                        <td class="py-3 px-4 text-right font-bold text-green-600">$@Model.TotalSales.ToString("N2")</td>
                    </tr>
                </tfoot>
            </table>
        </div>

        <!-- Footer -->
        <div class="mt-8 pt-4 border-t border-gray-200 text-center text-gray-500 text-sm">
            <p>This is an automatically generated report. For questions, please contact the finance department.</p>
        </div>
    </div>
</body>
</html>
```

Then we call `RazorTemplateEngine.RenderAsync` to dynamically render the HTML content from a Razor view:

```csharp
public class PdfGenerator(SalesDataGenerator dataGenerator)
{
    public async Task<byte[]> GenerateMonthlySalesReport(
        DateTime dateFrom,
        DateTime dateTo,
        int departmentId)
    {
        // Get department info
        var department = dataGenerator.GetDepartment(departmentId);
        if (department is null)
        {
            throw new ArgumentException($"Department with ID {departmentId} not found");
        }

        // Generate sales data
        var salesData = dataGenerator.GenerateSalesData(dateFrom, dateTo, departmentId);

        // Create the report model
        var report = new MonthlySalesReport
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            Department = department,
            Sales = salesData,
            GeneratedOn = DateTime.Now
        };

        // Render the HTML using Razor template
        var html = await RazorTemplateEngine.RenderAsync("Views/MonthlySalesReport.cshtml", report);

        // Create PDF from HTML
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);

        return pdf.BinaryData;
    }
}
```

You can use the `ChromePdfRenderer` class from `IronPDF` to generate a PDF document:

```csharp
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
```

We can save the document or return the `BinaryData` to download the file from the web browser:

```csharp
public record SalesReportRequest(DateTime DateFrom, DateTime DateTo, int DepartmentId);

app.MapGet("/api/reports/sales", async (
    [AsParameters] SalesReportRequest request,
    PdfGenerator pdfGenerator) =>
{
    var pdfBytes = await pdfGenerator.GenerateMonthlySalesReport(
        request.DateFrom,
        request.DateTo,
        request.DepartmentId);

    var reportName = $"sales_report_{request.DepartmentId}_{request.DateFrom:yyyyMMdd}_{request.DateTo:yyyyMMdd}.pdf";
    return Results.File(pdfBytes, "application/pdf", reportName);
});
```

Here is what the downloaded report looks like:

![Screenshot of the top part of a generated "Monthly Sales Report" PDF, showing summary statistics for total sales, total items sold, and unique products.](https://antondevtips.com/media/code_screenshots/aspnetcore/generating-pdfs/img_1.png)

![Screenshot of the bottom part of a generated "Monthly Sales Report" PDF, displaying sales details in a table format with columns for Date, Product, Quantity, Unit Price, and Total, along with a grand total.](https://antondevtips.com/media/code_screenshots/aspnetcore/generating-pdfs/img_2.png)

Literally, you spend about 5 minutes to set up and write the code with IronPDF to create a PDF document.

IronPDF is a relatively new library that is gaining high popularity in the .NET community. But how does it compare to a well-known Aspose.PDF ?

Let's find out.

[](#creating-pdf-documents-with-asposepdf)

## Creating PDF Documents with Aspose.PDF

Aspose.PDF was the most popular PDF library until IronPDF was released.

Here is how you can create a PDF document with Aspose.PDF:

```csharp
using Aspose.Pdf;

Document doc = new Document();
Page page = doc.Pages.Add();

var HtmlStr = "<h1>Hello Aspose.PDF!</h1> <h2>Welcome to PDF File</h2>";

HtmlFragment HtmlFrg = new HtmlFragment(HtmlStr);
page.Paragraphs.Add(HtmlFrg);
doc.Save("Sample.pdf");
```

Aspose.PDF provides advanced capabilities for programmatic PDF creation. It gives developers extensive control over document elements such as paragraphs, tables, images, forms, and annotations. However, the approach to generating PDFs tends to be more verbose, involving detailed coding for document structure.

[](#comparing-ironpdf-and-asposepdf)

## Comparing IronPDF and Aspose.PDF

When choosing a PDF library for .NET, it's not just about what features they offer — it's also about how well they perform, how easy they are to work with, and whether they scale with your development and business needs.

[](#performance-and-efficiency)

### Performance and Efficiency

IronPDF is built on top of a real Chromium rendering engine (the same engine behind Google Chrome), which ensures high accuracy and fast rendering of web content to PDFs. It efficiently handles complex HTML, CSS, and even JavaScript-rendered content.

Aspose.PDF, while powerful, takes a more traditional approach. It constructs PDFs programmatically or parses and manipulates existing ones, but doesn't use Chromium (or another web engine) for rendering. As a result, while it offers manual control over each aspect, it's often slower and more resource-intensive when working with rich layouts or conversions.

IronPDF can give you up to 2-4x performance boost over Aspose.PDF and less memory usage.

[](#ease-of-use)

### Ease of Use

IronPDF is extremely easy to get started with — just install the NuGet package and you're ready to render HTML to PDF in a few lines of code. The fluent API and modern approach make it accessible even to junior developers.

Aspose.PDF, in contrast, has a more verbose API that requires developers to manually define document structure, elements, formatting, and styles — even for simple tasks. This can lead to steeper learning curves and slower development.

[](#html-to-pdf-capabilities)

### HTML to PDF Capabilities

This is where IronPDF truly shines. Built specifically for rendering HTML into PDFs, it supports:

*   Full CSS styling
*   JavaScript execution
*   Web fonts and media queries
*   Inline or external HTML content

In ASP.NET Core you can use Razor views, export invoices from web templates, and convert entire webpages into pixel-perfect PDFs.

Aspose.PDF supports limited HTML to PDF conversion but lacks a native Chromium engine. JavaScript is not supported, and complex layouts are much harder to implement. These all result in rendering limitations.

[](#developer-experience)

### Developer Experience

IronPDF provides a clean, well-documented API with hundreds of code samples and a searchable knowledge base. Their support team is known for fast response times, and the company actively publishes case studies and how-to articles.

Aspose.PDF also offers extensive documentation, but due to the API's complexity, developers often need to dig deep into docs or community forums to accomplish more complex tasks. While support is available, it takes more time to respond. And the overall onboarding experience is less developer-centric.

[](#licensing)

### Licensing

Both libraries offer a 30-day trial license for free.

[IronPDF](https://ironpdf.com/licensing?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=june-2025) offers a license at a much more affordable price and offers more benefits than [Aspose](https://purchase.aspose.com/pricing/pdf/net/), which is honestly very overpriced.

For me a clear winner is IronPDF.

It offers the following advantages:

*   It's easier to use
*   It's faster
*   It has a quicker learning curve, just 5 minutes from getting started to generating the first PDF
*   It's well-documented
*   It's actively maintained
*   It has a better support
*   It's much cheaper compared to Aspose.PDF

If you're more interested in a more in-depth side-by-side comparison, check out [this article](https://ironpdf.com/competitors/aspose-vs-ironpdf?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=june-2025) and [this](https://ironsoftware.com/customers/case-studies/jeff-fritz-pdf-library-comparison?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=june-2025).

[](#summary)

## Summary

PDF generation is a common task in many .NET applications: documents, invoices, reports, contracts, etc. Bad PDFs drive users away. Slow load times, broken pages, and clunky UX are a bad user experience. Your business success may rely on how good are your PDF documents.

The easiest and fastest way is to generate PDF documents from well-formatted HTML content. This is where IronPDF really shines. It's easy to use, has great documentation, and offers a 30-day trial license for free.

When compared to QuestPDF and Aspose.PDF, IronPDF offers the most features, the best performance, and developer experience. And when it comes to pricing — it has the most balanced value for the provided features.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/how-to-create-and-convert-pdf-documents-in-aspnetcore)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-create-and-convert-pdf-documents-in-aspnetcore&title=How%20to%20Create%20and%20Convert%20PDF%20Documents%20in%20ASP.NET%20Core)[X](https://twitter.com/intent/tweet?text=How%20to%20Create%20and%20Convert%20PDF%20Documents%20in%20ASP.NET%20Core&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-create%2Dand%2Dconvert%2Dpdf%2Ddocuments%2Din%2Daspnetcore)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-create-and-convert-pdf-documents-in-aspnetcore)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.