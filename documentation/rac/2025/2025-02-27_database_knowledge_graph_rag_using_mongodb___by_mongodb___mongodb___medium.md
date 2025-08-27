```yaml
---
title: "Knowledge Graph RAG Using MongoDB | by MongoDB | MongoDB | Medium"
source: https://medium.com/mongodb/knowledge-graph-rag-using-mongodb-1346e953064c
date_published: 2025-02-27T14:37:17.640Z
date_captured: 2025-08-12T13:37:31.468Z
domain: medium.com
author: MongoDB
category: database
technologies: [MongoDB, LangChain, Large Language Models (LLM), OpenAI, langchain-mongodb, GitHub]
programming_languages: [Python]
tags: [knowledge-graph, rag, mongodb, langchain, llm, graph-database, data-access, python, ai, prompt-engineering]
key_concepts: [Retrieval-Augmented Generation (RAG), Knowledge Graph, Entity Extraction, Graph Traversal, MongoDB Aggregation Pipeline, Prompt Engineering, Structured Data, Relationship Discovery]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces Knowledge Graph Retrieval-Augmented Generation (GraphRAG) as an alternative to traditional vector-based RAG, leveraging MongoDB as a graph database. It details how GraphRAG uses Large Language Models (LLMs) for entity extraction to build knowledge graphs composed of nodes and relationships. The implementation utilizes LangChain for LLM integration and MongoDB's `$graphLookup` operator for efficient graph traversal and search. The author highlights GraphRAG's advantages, such as handling complex, multi-step queries and overcoming vector-based RAG's chunking limitations, by focusing on structured data and relationships.]
---
```

# Knowledge Graph RAG Using MongoDB | by MongoDB | MongoDB | Medium

# Knowledge Graph RAG Using MongoDB

## We use MongoDB as a graph database to discover deep connections between disparate documents using an LLM’s inherent power to work with structured data.

![MongoDB Logo](https://miro.medium.com/v2/resize:fill:64:64/1*KsX3qkqj95ZPqoAdu9DOTA.png)

[MongoDB](/@MongoDB?source=post_page---byline--1346e953064c---------------------------------------)

Follow

7 min read

·

Feb 27, 2025

77

1

Listen

Share

More

_This article was written by_ [_Casey Clements_](http://casey.clements@mongodb.com)_._

Press enter or click to view image in full size

![Knowledge Graph RAG Using MongoDB - An abstract illustration depicting interconnected data points and structures, symbolizing a knowledge graph and its components, with the title 'Knowledge Graph RAG Using MongoDB'.](https://miro.medium.com/v2/resize:fit:700/1*NK838SRzVqzYjX4h82Z9tg.png)

However they are dressed up, all retrieval-augmented generation (RAG) implementations are, in the end, modifications to the prompt sent to an LLM. They simply augment the user prompt by providing pertinent additional context that is collected from documents that the LLM was not trained on.

At a high level, GraphRAG (search based on a knowledge graph) is almost identical to RAG approaches based on vector-based, and to full-text search. They all can use the same LLM for the chat completion. The difference is in the model used to extract, represent, and retrieve relevant information from novel sources. In standard vector-based RAG, one uses an embedding model to create the representations (embeddings), while in GraphRAG, one uses an LLM to create the representation (a knowledge graph).

Just like vector-based RAG, GraphRAG makes two calls to an LLM: the first to convert the list of input documents into a form that we wish to store, and the second to convert the query into a form that we can use to find similar documents. Just like vector-based RAG, we retrieve the most relevant information and include this as context to the query that is then passed to the LLM.

In GraphRAG, one uses an entity extraction model in place of vector-based RAG’s embedding model. The entity extraction model converts documents into knowledge graphs composed of nodes that are entities and edges that are relationships. The graph representation of a connection is a list of triplets: [source entity (node), relationship (edge), target entity (node)]. This is a common representation, and LLMs have no trouble interpreting it.

Search for relevant information is done by graph traversal, finding entities connected to the query. The results are then included with the query passed to the LLM for chat completion. Notably, GraphRAG’s output is in a structured JSON format.

Being so similar, and still so new, it is easy to conflate GraphRAG with vector-based approaches, but they are distinct. GraphRAG need not have any embedding model involved! Though they can be combined into hybrid approaches, in our implementation described below, we provide a pure GraphRAG implementation.

# Advantages over standard vector-based RAG

A knowledge graph’s inherent focus on connections means it can answer questions that require multiple steps to arrive at an answer, such as “Which companies has ACME acquired?” or “Who are mutual friends of John Doe and Satnam Singh?” — that is, queries that demand an understanding of structured relationships rather than just textual proximity. Vector-based RAG excels at semantic similarity search but does best when relevant information exists in individual documents, or chunks of them. MongoDBGraphStore produces a structured document instead of a vector. This permits us to accumulate new attributes and relationships as new documents are added. Of course, MongoDB excels at updates of nested data structures! Even when we are not interested in complex connections, a rich description of an entity in one response is invaluable.

Chunking is the Achilles’ heel of vector-based RAG. The longer the document one embeds into a vector, the more information one adds but also the more noise as the document loses specificity. The shorter the document, the less context one includes. Graph RAG does not suffer this headache. It can disambiguate separate topics from larger documents, and even infer from context that Jane Doe, Jane, John Doe’s wife, and the pronouns “she” and “her” all refer to the same entity!

Chunking is also what makes combining vector- and graph-based approaches difficult because one entity’s attributes and relationships may come from many chunks.This is another reason that what we have chosen is a simple pure graph approach, which we describe below.

**Comparison of approaches, including MongoDB and LangChain components**

Press enter or click to view image in full size

![Comparison of approaches, including MongoDB and LangChain components - A table comparing three RAG approaches: Graph, Vector-based, and Full-text. It details LangChain classes, input types, extraction methods, storage formats, search mechanisms, output types, and LangChain LLM components for each approach.](https://miro.medium.com/v2/resize:fit:700/1*TLtM9wcr_W1aJ_PizeuGHg.png)

# MongoDBGraphStore

We now describe our implementation. We extract entities using an LLM using LangChain, storing them as documents in a MongoDB collection, which we use as a [graph database](https://www.mongodb.com/resources/basics/databases/mongodb-graph-database?utm_campaign=devrel&utm_source=third-party-content&utm_medium=cta&utm_content=knowledge-graph&utm_term=megan.grant) to find relevant context to provide to the chat completion model.

## Entities

A knowledge graph is stored in a single collection. Each MongoDB document represents a single entity (node), and its relationships (edges) are defined in a nested field named “relationships.” It can be represented by a JSON document. Both entities and relationships are described with types and attributes. Each can extend indefinitely and can also be constrained by the user to focus on specific topics. Relationships, and their types and attributes, are arrays of the same length. With this schema, the $graphLookup operator of an aggregation pipeline can follow arbitrary edges simultaneously!

An example will make things concrete. Suppose we were given the following input document.

_“OpenAI hosted its first developer conference, OpenAI DevDay, on November 6, 2023 in San Francisco. The opening keynote was delivered by OpenAI’s co-founder and CEO, Sam Altman. A notable guest speaker at the event was Satya Nadella, the CEO of Microsoft, who discussed the partnership between Microsoft and OpenAI.”_

The document added to the collection might look like the following.

```json
{  
  "entities": [  
    {  
      "_id": "2022 OpenAI DevDay",  
      "type": "Event",  
      "attributes": {  
        "date": ["2023-11-23"],  
        "location": ["San Francisco"]  
      },  
      "relationships": {  
        "targets": ["Sam Altman", "Satya Nadella"],  
        "types": ["Speaker", "Speaker"]  
      }  
    },  
    { "_id": "Satya Nadella", "type": "Person", "attributes": {"Microsoft CEO"}},  
    { "_id": "Sam Altman", "type": "Person", "attributes": {"OpenAI CEO"}}  
  ]  
}
```

To keep examples short, we have constrained entity and relationship types as follows. This is done in the constructor.

`allowed_entity_types=["Event", "Person"],`  
`allowed_relationship_types=["Speaker"]`

## Entity extraction

As so often in development, on top of large language models, much of the work is in designing the prompts. As we said at the beginning, all RAG produces ultimately is a better prompt. As you might imagine, it takes an involved prompt to produce a structured output adhering to the format above, with the flexibility required to adapt to your domain. We won’t include it, but you can find it on [GitHub](https://github.com/langchain-ai/langchain-mongodb/blob/main/libs/langchain-mongodb/langchain_mongodb/graphrag/prompts.py). The extraction itself in LangChain, however, is very simple. The prompt is passed to the LLM along with the user’s document and (optionally) prescribed types.

```python
# Combine the LLM with the prompt template to form a chain  
chain: RunnableSequence = self.entity_prompt | self.entity_extraction_model  
# Invoke on a document to extract entities and relationships  
response: str = chain.invoke(  
    dict(  
        input_document=raw_document,  
        entity_schema=self.entity_schema,  
        allowed_entity_types=self.allowed_entity_types,  
        allowed_relationship_types=self.allowed_relationship_types,  
    )  
).content
```

## Graph traversal/search

MongoDB is not specifically a graph database, but with its $graphLookup operator, one can use it as such. It is able to traverse an arbitrary number of edges of an arbitrary number of nodes — thus no additional loops or logic in Python! There are two parts to the query phase: Find the relevant entities in the original prompt, and traverse the graph to find related entities.

```python
def similarity_search(self, input_document: str) -> List[Entity]:  
    starting_ids: List[str] = self.extract_entity_names(input_document)  
    return self.related_entities(starting_ids)
```

This time, the prompt is decidedly simpler. All we need are entity names. The extraction code is the same — just a call to the LLM:

`starting_entities = (query_prompt | entity_extraction_model).invoke(**)`

The graph traversal is done in standard MongoDB Query API. What is shown below is the first two of the stages of the pipeline that traverses the graph. The first does a `$match` on the names of entities found in the query. The entities found all flow through to `$graphLookup` which connects from (the array of) relationships.target_ids to any entity with the same _id. This continues until the max_depth is hit or no more connections are found. Stages after this unwind the connections and combine with the original matches, remove duplicates, and tidy up so that what is returned is a flat list of entity documents!

```javascript
 pipeline = [  
       # Match starting entities  
       {"$match": {"_id": {"$in": starting_entities}}},  
       {  
           "$graphLookup": {  
               "from": self.collection.name,  
               "startWith": "$relationships.target_ids",  
               "connectFromField": "relationships.target_ids",  
               "connectToField": "_id",  # names  
               "as": "connections",  
               "maxDepth": max_depth or self.max_depth,  
           }  
       },  
       # Further stages: combine original with connections, remove duplicates,   
       ...
```

This list of documents, as should now be clear, is all JSON that an LLM can easily interpret. Therefore, the results can be prepended directly as context to the original human message (query). Voilà! GraphRAG!

Putting it all together…

```python
# Perform Retrieval on knowledge graph  
related_entities = self.similarity_search(query)  
# Combine the LLM with the prompt template to form a chain  
chain: RunnableSequence = prompt | chat_model  
# Invoke with query, retrieved info, and schema  
return chain.invoke(  
    dict(  
        query=query,  
        related_entities=related_entities,  
        entity_schema=entity_schema,  
    )  
)
```

# Conclusion

GraphRAG need not be complicated. Today’s LLMs can perform the complex task of finding and combining distinct named entities and relationships in a text document and reporting these in structured JSON. From text, a single call to an LLM can produce a knowledge graph! No embedding model need be involved. One can add many documents from different sources, and MongoDB flexibly combines them in the existing collection, enriching entities’ attributes and relationships. At query time, use MongoDB’s abilities as a graph database to find connected entities. Throughout, LangChain provides an intuitive and full-featured framework to plug into, handling the prompt construction and calls to the LLMs.

MongoDB and LangChain provide the “developer data platform” for building generative AI applications. Install the `[langchain-mongodb](https://github.com/langchain-ai/langchain-mongodb)` package now, and try out MongoDBGraphStore.