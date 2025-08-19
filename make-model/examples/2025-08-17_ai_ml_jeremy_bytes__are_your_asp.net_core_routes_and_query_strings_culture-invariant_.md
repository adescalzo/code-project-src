```yaml
---
title: "Jeremy Bytes: Are Your ASP.NET Core Routes and Query Strings Culture-Invariant?"
source: https://jeremybytes.blogspot.com/2024/12/are-your-aspnet-core-routes-and-query.html?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=invoking-async-power&_bhlid=3d02d42e4acc633b5cccf2740dc14a6908571b36
date_published: unknown
date_captured: 2025-08-17T21:43:35.490Z
domain: jeremybytes.blogspot.com
author: Jeremy
category: ai_ml
technologies: [ASP.NET Core, .NET, GitHub, Windows 11, Microsoft Learn]
programming_languages: [C#]
tags: [aspnet-core, globalization, localization, model-binding, web-api, routing, query-strings, csharp, dotnet, culture-invariant]
key_concepts: [Globalization, Model Binding, Culture-Invariant Data, Culture-Sensitive Data, Route Parameters, Query String Parameters, IValueProviderFactory, Minimal APIs, Controller APIs]
code_examples: false
difficulty_level: intermediate
summary: |
  This article addresses a common issue in ASP.NET Core where route and query string model binding is culture-invariant, leading to errors with non-US English regional formats. The author demonstrates how numerical values with culture-specific decimal separators (e.g., commas) are incorrectly parsed, causing API failures. A primary solution involves explicitly formatting URL parameters using `CultureInfo.InvariantCulture` when constructing API calls. The post also briefly mentions an alternative approach using a custom `IValueProviderFactory` to achieve culture-sensitive APIs, which will be detailed in a follow-up article. The goal is to highlight the importance of handling globalization for robust web applications.
---
```

# Jeremy Bytes: Are Your ASP.NET Core Routes and Query Strings Culture-Invariant?

# Jeremy Bytes

byte-sized chunks of .NET

## Sunday, December 8, 2024

### Are Your ASP.NET Core Routes and Query Strings Culture-Invariant?

In a recent workshop, I ran into an interesting issue with ASP.NET Core and globalization. One of the attendees was from Iceland, and she was getting errors when running an application hitting a local API. Later, I was able to hunt down the globalization issue, and it has to do with the way model binding works.

> **Model binding in ASP.NET Core for route and query string data is culture-invariant.**

This caught me a bit off guard. I expected that the globalization settings on the computer would be used throughout the code. But because of the culture-invariant behavior of route and query string data in ASP.NET Core, things do not work that way.

*Note: This specifically refers to model binding for route and query string data (i.e., data that is part of a URL). Model binding for form data* is *culture sensitive.*

If you are biased toward United States English (as too many of us are), this issue does not come up locally. But if your API calls are made from non-US culture machines, you may run into issues if the routes or query strings are not specifically created as culture-invariant.

In this article, we'll take a look at making sure that our API calls are culture-invariant. In a future article, we'll look at how we can add a custom value provider that respects CultureInfo settings. (Update: Article is now available: [Make an ASP.NET Core Controller API Culture-Sensitive with IValueProviderFactory](https://jeremybytes.blogspot.com/2024/12/make-aspnet-core-controller-api-culture.html).)

Source code for this article is available on GitHub: [https://github.com/jeremybytes/aspnetcore-api-globalization](https://github.com/jeremybytes/aspnetcore-api-globalization).

The workshop code that brought this issue to light is available here: [https://github.com/jeremybytes/vslive2024-orlando-workshop-labs](https://github.com/jeremybytes/vslive2024-orlando-workshop-labs).

## Happy Path for US English

We'll start with the happy path. The scenario is that I need to get sunset information for my home automation software. The application calls a (local) service that provides sunset time based on latitude, longitude, and date. (For purposes of the sample code in the workshop, the service is run on the local machine. In the original implementation, this was a third-party service.)

### Application Output

Running the application produces the following output.

```
(From Controller Service) Sunset Tomorrow: 12/7/2024 4:26:56 PM
  (From Minimal API Service) Sunset Tomorrow: 12/7/2024 4:26:56 PM
```

This shows "tomorrow" sunset time as 4:26 p.m. (this is where I live in Vancouver, WA, USA). And since I am in the US, the date represents December 7, 2024.

### API Call

Here is the API call that produces the sunset data:

Here is the URL:
```
http://localhost:8973/SolarCalculator/45.6382/-122.7013/2024-12-07
```

And the JSON result:

```json
{"results":{"sunrise":"7:37:52 AM","sunset":"4:26:56
  PM","solar_noon":"12:02:24 PM","day_length":"08:49:03.8810000"},"status":"OK"}
```

The route in the URL has the latitude (45.6382), the longitude (-122.7013), and the date (2024-12-07).

The latitude and longitude values are culturally significant.

The URL / route is created in the code:

```csharp
string endpoint =
      $"SolarCalculator/{latitude:F4}/{longitude:F4}/{date:yyyy-MM-dd}";
  HttpResponseMessage response =
      client.GetAsync(endpoint).Result;
```

The formatting on the latitude and longitude parameters specify that they should include 4 decimal places.

## Changing Culture to Iceland

To duplicate the issue, I changed my computer's "Regional format" to Icelandic in the "Language & Region" settings (I'm using Windows 11).

This changes the number format to use a comma (",") for the decimal separator and a dot (".") for the thousands separator.

Re-running the application throws an error at this point. We can trace through the application to generate the URL and run it manually:

Here is the URL and the JSON result:

```
http://localhost:8973/SolarCalculator/45,6382/-122,7013/2024-12-07
```
```json
{"results":null,"status":"ERROR"}
```

This shows the latitude value as "45,6382" and longitude as "-122,7013". These are the proper formats for the Icelandic regional format setting.

However, we see that the API now returns an error. For some reason, it does not work with these route parameters.

### API Route Model Binding

It took a bit of debugging to find what was happening. Ultimately, I ended up in the HTTPGet code of the API controller.

*Note: Minimal APIs exhibit this same behavior. This is not specific to Controller APIs.*

Here is the endpoint:

```csharp
[HttpGet("{lat}/{lng}/{date}")]
    public SolarData GetWithRoute(double lat, double lng, DateTime date)
    {
        var result = SolarCalculatorProvider.GetSolarTimes(date, lat, lng);
        return result;
    }
```

When debugging into this method, I found that the "lat" and "lng" parameters did not have the values I expected:

```
lat = 456382
  lng = -1227013
  date = (7.12.2024 00:00:00)
```

This is where I hit the unexpected. The decimal separator (",") from the route was not being used.

### Route & Query Strings are Culture-Invariant

After much digging, I found a relevant section on the Microsoft Learn site ([https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-9.0#globalization-behavior-of-model-binding-route-data-and-query-strings](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-9.0#globalization-behavior-of-model-binding-route-data-and-query-strings)):

> `Globalization behavior of model binding route data and query strings   ``The ASP.NET Core route value provider and query string value provider:   ``o Treat values as invariant culture.   ``o Expect that URLs are culture-invariant.   ``In contrast, values coming from form data undergo a culture-sensitive conversion. This is by design so that URLs are shareable across locales.`

**Culture-invariant means US English** (because the world revolves around the US </sarcasm>). So, the comma is not recognized as a decimal separator here and ends up being ignored.

The result in this code is that the values overflow latitude and longitude values (which range from -180 to +180). So the API returns an error result.

The sample code also has an HTTPGet that uses a query string rather than a route. When I was investigating the problem, I tried this endpoint and had the same issue as the endpoint that used route parameters. As noted in the documentation above, both route parameters and query string parameters are mapped the same way -- with invariant culture.

### Creating a Culture-Invariant API Call

Based on what we know now, one solution is to make sure that the parameters of our API calls are culture-invariant.

Here is the revised calling code:

```csharp
string endpoint = string.Format(
                      CultureInfo.InvariantCulture,
                      "SolarCalculator/{0:F4}/{1:F4}/{2:yyyy-MM-dd}",
                      latitude, longitude, date);
```

Note that we cannot use the default string interpolation like we had previously. There are ways to create a string that respects culture while using string interpolation, but I find it easier to fall back to String.Format. This is probably out of habit, but it's an approach that works. You can use whatever method you like to create a culture-invariant string.

### Working Application with Icelandic Regional Format

Now that we have an invariant culture specified on the string, the URL will be formatted just like the original US English version that we started with (using a dot as the decimal separator).

Here is the resulting application output:

```
(From Controller Service) Sunset Tomorrow: 7.12.2024 16:26:56
  (From Minimal API Service) Sunset Tomorrow: 7.12.2024 16:26:56
```

This output also shows the Icelandic general date format: day dot month dot year.

## Another Alternative

Instead of using a culture-invariant route / query string, we can also update the service so that it is culture sensitive. How to do this is described in the same Microsoft Learn article mentioned above ([https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-9.0#globalization-behavior-of-model-binding-route-data-and-query-strings](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-9.0#globalization-behavior-of-model-binding-route-data-and-query-strings)).

I have done this as well. The general idea is that you create a custom IValueProviderFactory that parses the parameters based on the current culture (or other desired culture). You do this for both query string values and route values. Then in the "AddControllers" part of the API builder process, you replace the default route and query string value providers with the custom value providers. (Update: Article is now available: [Make an ASP.NET Core Controller API Culture-Sensitive with IValueProviderFactory](https://jeremybytes.blogspot.com/2024/12/make-aspnet-core-controller-api-culture.html).)

This solution only works with Controller APIs. I have been unable to find a similar option for Minimal APIs.

The sample code has an implementation of this, and I'll go through it in a future article.

## The Important Question

So the important question is "**Are your ASP.NET Core route and query strings culture-invariant?**" If not, you may need to take a look at them. You may not need to worry about it if you are using an unambiguous date format (like the year/month/day format used here). But if your parameters have decimals or thousands separators, then you will want to make sure that those parameters are represented in a culture-invariant way.

## Wrap Up

Localization and globalization are important topics. I haven't touched on localization here (where we make sure that the text in our applications is available in other languages). In my particular situation (coding workshops), the workshops are conducted in English, and so the application text can also be in English.

But when it comes to globalization, folks use different number and date formats. I'm often conscious of date formats when I present outside of the US (since we have the worst of all "standard" date formats). But this is the first time I have run into an issue with globalization around a decimal separator.

I understand the desire to have URLs culture-invariant. But it also grates on me a bit. The idea of making the API culture sensitive seems like a better solution for my scenario. Stay tuned for the next article, and you can help me decide by telling me which option you prefer.

Happy Coding!

Posted by [Jeremy](https://www.blogger.com/profile/06749690234470413216 "author profile") at [6:00 AM](https://jeremybytes.blogspot.com/2024/12/are-your-aspnet-core-routes-and-query.html "permanent link")

Labels: [API](https://jeremybytes.blogspot.com/search/label/API), [ASP.NET Core](https://jeremybytes.blogspot.com/search/label/ASP.NET%20Core), [Globalization](https://jeremybytes.blogspot.com/search/label/Globalization)