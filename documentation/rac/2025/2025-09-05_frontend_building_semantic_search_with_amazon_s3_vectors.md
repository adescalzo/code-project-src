```yaml
---
title: Building Semantic Search with Amazon S3 Vectors
source: https://www.milanjovanovic.tech/blog/building-semantic-search-with-amazon-s3-vectors?utm_source=newsletter&utm_medium=email&utm_campaign=tnw158
date_published: 2025-09-06T00:00:00.000Z
date_captured: 2025-09-06T16:00:32.719Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: frontend
technologies: [Amazon S3, Amazon S3 Vectors, Amazon Bedrock, Semantic Kernel, NuGet, AWSSDK.BedrockRuntime, Lunr.js, Pinecone, Weaviate, Qdrant, Amazon Titan embedding model, AWS SDK, ASP.NET Core]
programming_languages: [C#, PowerShell, JavaScript]
tags: [semantic-search, vector-database, aws, s3, embeddings, dotnet, ai, search, cloud-storage, serverless]
key_concepts: [semantic-search, vector-embeddings, vector-database, embedding-model, cosine-similarity, full-text-search, metadata-filtering, serverless-architecture]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explores how to implement semantic search using the new Amazon S3 Vectors feature, offering a cost-effective and serverless alternative to traditional vector databases. It begins by contrasting semantic search, which understands meaning, with conventional full-text search. The author demonstrates the process of generating vector embeddings from text content using Microsoft's Semantic Kernel and Amazon Bedrock's Titan embedding model. The guide then details how to create and configure an S3 Vector Bucket, including setting up a vector index with a chosen distance metric like cosine similarity. Finally, it provides C# code examples for storing vectors with metadata and querying the vector index to retrieve semantically relevant results, highlighting the ease of integration and potential for enhanced search experiences.]
---
```

# Building Semantic Search with Amazon S3 Vectors

![Building Semantic Search with Amazon S3 Vectors](/blog-covers/mnw_158.png?imwidth=3840)

# Building Semantic Search with Amazon S3 Vectors

6 min read · September 06, 2025

I implemented [**full-text search**](how-i-implemented-full-text-search-on-my-website) on my static website using Lunr.js. It works great for exact matches or phrases, but it doesn't understand meaning. Someone searches for ["modular monolith"](search?q=modular%20monolith) and finds posts that contain these phrases. But when they search for "database performance issues," they might miss my articles about query optimization and index tuning, even though that's exactly what they need.

[**Amazon S3 is now also a vector database**](https://aws.amazon.com/blogs/aws/introducing-amazon-s3-vectors-first-cloud-storage-with-native-vector-support-at-scale/), and it's 90% cheaper (according to the announcement) than the alternatives.

For those of us running static sites or simple web apps, it means we can finally add semantic search without the operational overhead of running a vector database like [Pinecone](https://www.pinecone.io/), [Weaviate](https://weaviate.io/), or [Qdrant](https://qdrant.tech/).

Instead, we can just use S3 buckets that understand vectors.

I'm adding it alongside my existing full-text search implementation, and the whole thing took an afternoon to build.

## How Semantic Search Works

The entire [semantic search](https://cloud.google.com/discover/what-is-semantic-search) flow is straightforward:

![Diagram illustrating the semantic search flow, starting with data, generating vector embeddings using an embedding model, storing vector data in S3 Vectors, creating a vector index, and finally allowing users to perform semantic searches.](blogs/mnw_158/semantic_search_flow.png?imwidth=3840)

You have to start with some data. In my case, this data comes from the articles I've written over the years.

Then we need an embedding model. Amazon Bedrock offers many to choose from. You use an embedding model to convert text into vectors. Vectors are numerical representations of your data that capture semantic meaning.

Finally, we store these vectors in an S3 Vector Bucket, a new type of S3 bucket designed specifically for vector storage and search. When someone searches, we convert their query into a vector and find the closest matches in the bucket.

If you want to understand the fundamentals, I wrote an article explaining what [**vector search**](what-is-vector-search-a-concise-guide) is.

## Generating Embeddings with Semantic Kernel

Microsoft's [Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/overview/) makes working with Bedrock surprisingly clean.

We'll need to install a few NuGet packages:

```powershell
# Semantic kernel packages
Install-Package Microsoft.SemanticKernel
Install-Package Microsoft.SemanticKernel.Connectors.Amazon

# AWS SDK for Bedrock
Install-Package AWSSDK.BedrockRuntime
```

We have to configure the embedding generator in our application. Here we can specify which model to use for generating embeddings. I'll use the Amazon Titan embedding model (`amazon.titan-embed-text-v2:0`) which produces 1024-dimensional vectors.

```csharp
builder.Services.AddBedrockEmbeddingGenerator("amazon.titan-embed-text-v2:0");

builder.Services.AddTransient(sp =>
{
    return new Kernel(sp);
});
```

And then we can use the `IEmbeddingGenerator` abstraction to generate embeddings:

```csharp
var kernel = app.Services.GetRequiredService<Kernel>();

var embeddingGenerator = kernel.Services
    .GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

var articleContent = await blogService.GetBlogContentAsync(articleUrl);

Embedding<float> embedding = await embeddingGenerator.GenerateAsync(articleContent);

embeddings.Add((articleUrl, embedding.Vector.ToArray()));
```

The number of dimensions varies depending on the embedding model you choose. You can find more information about that in the documentation for your specific model.

## Creating Your S3 Vector Bucket

S3 Vectors uses an entirely new bucket type, not a feature you enable on existing buckets. They're a fundamentally different storage system optimized for vector operations. Creating one feels familiar if you've used S3 before:

![Screenshot showing the AWS console interface for creating an S3 Vectors bucket, highlighting the option to select "S3 Vectors" as the bucket type.](blogs/mnw_158/s3_vectors_bucket.png?imwidth=3840)

Pick a unique name for your vector bucket:

![Screenshot of the AWS console showing the step to name a new S3 Vectors bucket.](blogs/mnw_158/s3_vectors_bucket_create.png?imwidth=3840)

And finally, you can create a **vector index** for your bucket. You get to choose the number of dimensions in each vector. This is dictated by the embedding model you use. All vectors within a vector index should use the same embedding model. Otherwise, you won't get the correct results when searching. You also have to choose the [distance metric](https://en.wikipedia.org/wiki/Metric_space) (e.g., cosine similarity, Euclidean distance) for your vector index. I went with [cosine similarity](https://en.wikipedia.org/wiki/Cosine_similarity), which is a common choice for text embeddings. The additional settings let you configure non-filterable metadata. By default, all metadata is filterable.

![Screenshot of the AWS console showing the configuration options for creating a vector index within an S3 Vectors bucket, including setting dimensions and distance metric.](blogs/mnw_158/s3_vectors_index_create.png?imwidth=3840)

The UI is not polished at all, and you can't do much else. I expect this will improve over time, but for now, everything else is done via the SDK or CLI. There are certain limitations to be aware of, you can check out the docs [here](https://docs.aws.amazon.com/AmazonS3/latest/userguide/s3-vectors-limitations.html).

## Storing Vectors with Metadata

Now that we have our vector index, we can start storing vector embeddings with metadata. Metadata is interesting because it enables filtered searches. Want to search only recent posts? Posts in a specific category? The metadata makes it possible.

Here's how you can store vectors:

```csharp
public async Task IndexBlogPost(BlogPost post)
{
    List<float> embedding = await GenerateEmbedding(post.Content);

    await s3VectorsClient.PutVectorsAsync(new PutVectorsRequest
    {
        VectorBucketName = "mjtech-articles-semantic-search",
        IndexName = "mjtech-article-content",
        Vectors = new List<Vector>
        {
            new PutInputVector
            {
                Key = post.Slug,
                Data = new VectorData
                {
                    Float32 = embedding
                },
                Metadata = new Document(new Dictionary<string, Document>
                {
                    ["title"] = post.Title,
                    ["date"] = post.PublishedDate.ToString("yyyy-MM-dd"),
                    ["category"] = post.Category,
                    ["url"] = $"/posts/{post.Slug}"
                })
            }
        }
    });
}
```

But if you're indexing your entire blog archive, doing it one post at a time is costly. You can batch together multiple posts and index them in a single API call. The SDK makes it easy to do this by accepting a list of vectors.

## Querying Vector Indexes

When someone searches your site, you convert their query into a vector and find the closest matches.

Here's how you can implement semantic search:

```csharp
public async Task<List<SearchResult>> SemanticSearch(
    string query,
    int topK = 10)
{
    // Convert search query to vector (use same model as vectors!)
    List<float> queryEmbedding = await GenerateEmbedding(query);

    var request = new QueryVectorsRequest
    {
        VectorBucketName = "mjtech-articles-semantic-search",
        IndexName = "mjtech-article-content",
        QueryVector = new VectorData
        {
            Float32 = queryEmbedding
        },
        TopK = topK,
        ReturnMetadata = true,
        ReturnDistance = true
    };

    QueryVectorsResponse response = await s3VectorsClient.QueryVectorsAsync(request);

    return response.Vectors.Select(v => new SearchResult
    {
        Distance = v.Distance,
        Title = v.Metadata.AsDictionary()["title"].ToString(),
        Url = v.Metadata.AsDictionary()["url"].ToString(),
        Category = v.Metadata.AsDictionary()["category"].ToString()
    }).ToList();
}
```

I omitted metadata filtering here, but you can explore the [documentation](https://docs.aws.amazon.com/AmazonS3/latest/userguide/s3-vectors-metadata-filtering.html) for more details.

## Next Steps

Would I migrate from an existing vector database? Probably not if everything's working. The operational overhead would have to justify the switch.

But if you're adding semantic search for the first time, or your vector database bills are getting uncomfortable, S3 Vectors is a viable choice. The setup takes an afternoon, the ongoing maintenance is zero, and your users get search that actually understands what they're looking for.

Don't forget that S3 Vectors is still in preview, so we can expect some changes before general availability.

Here's what I'm planning to do next:

1.  Automatically update the vector index when I publish a new post. I can do this in my CI/CD pipeline, where I can detect new posts and trigger a re-indexing.
2.  Expose a search endpoint that uses the new semantic search capabilities. Combine the results with the full-text search results.
3.  Make sure everything is performant and cost-effective. Acceptable latency is under 500ms.
4.  Share details about the implementation and any challenges faced. Especially around the costs of using an embedding model and vector storage.

That's all for today.

See you next week.