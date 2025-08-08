```yaml
---
title: "Implement a Geo-distance search using .NET Aspire, Elasticsearch and ASP.NET Core | Software Engineering"
source: https://damienbod.com/2024/09/23/implement-a-geo-distance-search-using-net-aspire-elasticsearch-and-asp-net-core/?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=implementing-blocked-floyd-warshall-algorithm&_bhlid=f6f43ff25fb1e8557d4e4f9463eaf3e82f3877cf
date_published: 2024-09-23T08:56:43.000Z
date_captured: 2025-08-08T13:47:07.529Z
domain: damienbod.com
author: damienbod
category: ai_ml
technologies: [.NET Aspire, Elasticsearch, ASP.NET Core, LeafletJs, Docker, Elastic.Clients.Elasticsearch, MVC, HTTPS, GitHub]
programming_languages: [C#, JavaScript]
tags: [geo-distance-search, elasticsearch, aspnet-core, .net-aspire, docker, web-development, location-based-services, csharp, frontend, mapping]
key_concepts: [geo-distance-query, geo-point-mapping, containerization, service-orchestration, client-side-mapping, api-key-authentication, dependency-injection, configuration-management]
code_examples: true
difficulty_level: intermediate
summary: |
  This article demonstrates how to implement a geo-location search feature within an ASP.NET Core application. It utilizes Elasticsearch for performing efficient geo-distance queries and LeafletJs for visualizing the results on an interactive map. The development setup is streamlined using .NET Aspire, which orchestrates both the Elasticsearch container and the ASP.NET Core UI application for local testing. The guide provides C# code examples for configuring the Elasticsearch client, defining geo-point mappings, and executing proximity searches, showcasing a practical approach to building location-aware web solutions.
---
```

# Implement a Geo-distance search using .NET Aspire, Elasticsearch and ASP.NET Core | Software Engineering

# Implement a Geo-distance search using .NET Aspire, Elasticsearch and ASP.NET Core

This article shows how to implement a geo location search in an ASP.NET Core application using a LeafletJs map. The selected location can be used to find the nearest location with an Elasticsearch Geo-distance query. The Elasticsearch container and the ASP.NET Core UI application are setup for development using .NET Aspire.

**Code**: <https://github.com/damienbod/WebGeoElasticsearch>

## Setup

For local development, .NET Aspire is used to setup the two services and the HTTPS connections between the services. The services are configured in the Aspire **AppHost** project.

![Diagram showing .NET Aspire orchestrating an Elasticsearch Docker container and an ASP.NET Core application, with the ASP.NET Core app querying Elasticsearch via Geo Distance.](https://damienbod.com/wp-content/uploads/2024/09/aspire-elastic-3.png?w=581)

The Elasticsearch client is setup as a singleton and requires the connection configuration. This can be changed, if for example an API key is used instead. The connection URL is read from the configuration as well as the secrets.

```csharp
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace WebGeoElasticsearch.ElasticsearchApi;

public class ElasticClientProvider
{
    private readonly ElasticsearchClient? _client = null;
    public ElasticClientProvider(IConfiguration configuration)
    {
        if (_client == null)
        {
            var settings = new ElasticsearchClientSettings(new Uri(configuration["ElasticsearchUrl"]!))
                .Authentication(new BasicAuthentication(configuration["ElasticsearchUserName"]!,
                    configuration["ElasticsearchPassword"]!));
            _client = new ElasticsearchClient(settings);
        }
    }
    public ElasticsearchClient GetClient()
    {
        if (_client != null)
        {
            return _client;
        }
        throw new Exception("Elasticsearch client not initialized");
    }
}
```

## Create Index with mapping

The index cannot be created by adding a document because the mapping is created incorrectly using the default settings. The mapping can be created for the defined index using the **Mappings** extension from the **Elastic.Clients.Elasticsearch** Nuget package. This was added to the client project in the **Aspire.Elastic.Clients.Elasticsearch** package. The mapping is really simple and probably not complete for a production index, some keyword optimizations are required. The **detailsCoordinates** field is defined as a **GeoPointProperty**.

```csharp
var mapping = await _client.Indices.CreateAsync<MapDetail>(IndexName, c => c
    .Mappings(map => map
        .Properties(
            new Properties<MapDetail>()
            {
                { "details", new TextProperty() },
                { "detailsCoordinates", new GeoPointProperty() },
                { "detailsType", new TextProperty() },
                { "id", new TextProperty() },
                { "information", new TextProperty() },
                { "name", new TextProperty() }
            }
        )
    )
);
```

The created mapping can be validated using the “IndexName”/\_mapping GET request. This returns the definitions as a Json response.

<https://localhost:9200/mapdetails/_mapping>

Documents can be added to the Elasticsearch index using the IndexAsync method.

```csharp
response = await _client.IndexAsync(dotNetGroup, IndexName, "1");
```

## Search Query

A Geo-distance query is used to find the distance from the selected location to the different Geo points in the index. This using latitude and longitude coordinates.

```csharp
public async Task<List<MapDetail>> SearchForClosestAsync(
    uint maxDistanceInMeter,
    double centerLatitude,
    double centerLongitude)
{
    // Bern Lat 46.94792, Long 7.44461
    if (maxDistanceInMeter == 0)
    {
        maxDistanceInMeter = 1000000;
    }
    var searchRequest = new SearchRequest(IndexName)
    {
        Query = new GeoDistanceQuery
        {
            DistanceType = GeoDistanceType.Plane,
            Field = "detailsCoordinates",
            Distance = $"{maxDistanceInMeter}m",
            Location = GeoLocation.LatitudeLongitude(
                new LatLonGeoLocation
                {
                    Lat = centerLatitude,
                    Lon = centerLongitude
                })
        },
        Sort = BuildGeoDistanceSort(centerLatitude, centerLongitude)
    };
    searchRequest.ErrorTrace = true;
    _logger.LogInformation("SearchForClosestAsync: {SearchBody}",
        searchRequest);
    var searchResponse = await _client
        .SearchAsync<MapDetail>(searchRequest);
    return searchResponse.Documents.ToList();
}
```

The found results are returned sorted using the Geo-distance sort. This puts the location with the smallest distance first. This is used for the map display.

```csharp
private static List<SortOptions> BuildGeoDistanceSort(
    double centerLatitude,
    double centerLongitude)
{
    var sorts = new List<SortOptions>();
    var sort = SortOptions.GeoDistance(
        new GeoDistanceSort
        {
            Field = new Field("detailsCoordinates"),
            Location = new List<GeoLocation>
            {
                GeoLocation.LatitudeLongitude(
                    new LatLonGeoLocation
                    {
                        Lat = centerLatitude,
                        Lon = centerLongitude
                    })
            },
            Order = SortOrder.Asc,
            Unit = DistanceUnit.Meters
        }
    );
    sorts.Add(sort);
    return sorts;
}
```

## Display using Leaflet.js

The ASP.NET Core displays the locations and the results of the search in a Leafletjs map component. The location closest to the center location is displayed differently. You can click around the map and test the different searches. The data used for this display is powered using the Geo-distance query.

![Leaflet.js map displaying multiple geo-located points and a search radius, with a highlighted closest location.](https://damienbod.com/wp-content/uploads/2024/09/elastic_geo_search_01.png?w=1024)

## Testing

The applications can be started using the .NET Aspire host project. One is run as a container, the other is a project. The docker container requires a Desktop docker installation on the host operating system. When the applications started, the containers need to boot up first. An optimization would remove this boot up.

![Screenshot of the .NET Aspire dashboard showing the running Elasticsearch container and webgeoelastic search project as resources.](https://damienbod.com/wp-content/uploads/2024/09/elastic_geo_search_02.png?w=1024)

## Notes

Using Elasticsearch, it is very simple to create fairly complex search requests for your web applications. With a bit of experience complex reports, queries can be implemented as well. You can also use Elasticsearch aggregations to group and organize results for data analysis tools, reports. .NET Aspire makes it easy to develop locally and use HTTPS everywhere.

## Links

*   <https://www.elastic.co/guide/en/elasticsearch/reference/current/geo-point.html>
*   <https://www.elastic.co/guide/en/elasticsearch/reference/current/query-dsl-geo-distance-query.html>
*   <https://leafletjs.com/>
*   <https://www.elastic.co/guide/en/elasticsearch/reference/current/explicit-mapping.html>
*   [Using Elasticsearch with .NET Aspire](https://damienbod.com/2024/09/16/using-elasticsearch-with-net-aspire/)