```yaml
---
title: A custom 404 page in Blazor Web Apps
source: https://steven-giesel.com/blogPost/38a4f1dc-420f-4489-9179-77371a79b9a9?utm_source=dotnetnews.beehiiv.com&utm_medium=newsletter&utm_campaign=the-net-news-daily-issue-183
date_published: 2025-04-14T07:44:00.000Z
date_captured: 2025-08-08T12:30:55.088Z
domain: steven-giesel.com
author: Unknown
category: ai_ml
technologies: [Blazor, .NET 8, Blazor Web Apps, Blazor Server, Blazor Client]
programming_languages: [C#, HTML]
tags: [blazor, dotnet, web-development, error-handling, 404-page, routing, web-app]
key_concepts: [custom-404-page, blazor-routing, blazor-web-app-model, server-side-rendering, client-side-rendering, route-specificity]
code_examples: false
difficulty_level: intermediate
summary: |
  This article addresses the challenge of creating a custom 404 page in Blazor Web Apps, specifically with .NET 8, where the traditional `<NotFound>` tag within the `Router` component no longer functions as expected. It first explains the previous approach used in Blazor Server and Client applications. The core of the article then provides a solution for the new Blazor Web App model by demonstrating how to create a "fall-through" page using a specific route pattern (`/{*route:nonfile}`). This method leverages Blazor's routing specificity to ensure the custom page is displayed when no other route matches, effectively serving as a custom 404 error page.
---
```

# A custom 404 page in Blazor Web Apps

# A custom 404 page in Blazor Web Apps

Sometimes you want to have a custom 404 page - and since .NET 8 and "Blazor WebApps" the `<NotFound>` tag of the `Router` doesn't work anymore, so let's create a custom page for that.

## Before Blazor Web App (aka Blazor Server and Client)

In "earlier" times you could do the following inside your `Router` component:

```csharp
<Router AppAssembly="@typeof(Program).Assembly">
	<Found Context="routeData">
		<AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)"/>
	</Found>
	<NotFound>
		<LayoutView Layout="@typeof(MainLayout)">
			<p>Here your custom component or code that is shown when the navigation results in a 404</p>
		</LayoutView>
	</NotFound>
</Router>
```

That still works in the "old" approach where you either the client-side or server-side approach (aka something like `<script src="_framework/blazor.server.js"></script>`). But in the new **Web App** model that doesn't work. Still, you are able to pass in a `NotFound` child to the `Router` but it doesn't get picked up. The underlying `Router` type is different in the Web App (where you have `<script src="_framework/blazor.web.js"></script>` somewhere in your `App.razor` and have `AddInteractiveServerComponents` in your sevice container).

## Adding a "fall trough" page

But we can easily define a web-page that is displayed with a lower specificity so that it has the least priority. Let's call that `NotFoundPage.razor`:

```html
@page "/{*route:nonfile}"

<p>Here your custom component or code that is shown when the navigation results in a 404</p>
@code {
  [Parameter]
  public string? Route { get; set; }
}
```

The `Route` isn't used but mandatory - otherwise Blazor will throw an exception that it can not set `route` to a property. On my companies website [https://bitspire.ch](https://bitspire.ch) we did exactly that. Head over to: [https://bitspire.ch/not-found](https://bitspire.ch/not-found) (or add any useless subpage that probably doesn't exist) and you are greeted with a custom 404 page.