```yaml
---
title: How to Move Beyond a Monolithic Data Lake to a Distributed Data Mesh
source: https://martinfowler.com/articles/data-monolith-to-mesh.html
date_published: unknown
date_captured: 2025-08-21T10:55:42.777Z
domain: martinfowler.com
author: Zhamak Dehghani
category: general
technologies: [Data Lake, Data Warehouse, Kappa Architecture, Apache Beam, Kafka, Amazon S3, Parquet, CloudEvents, Google Cloud Dataflow, Google Cloud Data Catalog, Google Cloud Storage, ETL, API, Graph Database, Enterprise Identity Management System, Role Based Access Control]
programming_languages: []
tags: [data-mesh, data-lake, data-platform, distributed-systems, domain-driven-design, data-governance, product-thinking, self-service, enterprise-architecture, data-analytics]
key_concepts: [data-mesh, domain-driven-design, product-thinking, self-service-platform, distributed-architecture, data-governance, data-product, data-lake-anti-pattern]
code_examples: false
difficulty_level: intermediate
summary: |
  This article critiques the common failure modes of centralized, monolithic data platforms like data lakes, which struggle with scalability, agility, and clear ownership. It proposes a paradigm shift to a "data mesh" architecture, advocating for a decentralized approach. This new model is built on four core principles: domain-oriented data ownership, treating data as a product, self-serve data infrastructure, and federated computational governance. The data mesh aims to enable ubiquitous data access, foster cross-functional data teams, and improve data discoverability, trustworthiness, and interoperability by distributing data responsibility to business domains. It redefines traditional data lakes and warehouses as mere nodes within this distributed mesh rather than central hubs.
---
```

# How to Move Beyond a Monolithic Data Lake to a Distributed Data Mesh

# How to Move Beyond a Monolithic Data Lake to a Distributed Data Mesh

_Many enterprises are investing in their next generation data lake, with the hope of democratizing data at scale to provide business insights and ultimately make automated intelligent decisions. Data platforms based on the data lake architecture have common failure modes that lead to unfulfilled promises at scale. To address these failure modes we need to shift from the centralized paradigm of a lake, or its predecessor data warehouse. We need to shift to a paradigm that draws from modern distributed architecture: considering domains as the first class concern, applying platform thinking to create self-serve data infrastructure, and treating data as a product._

20 May 2019

---

[![Photo of Zhamak Dehghani](data-monolith-to-mesh/zhamak.jpg)](https://twitter.com/zhamakd)

[Zhamak Dehghani](https://twitter.com/zhamakd)

Zhamak is a principal technology consultant at Thoughtworks with a focus on distributed systems architecture and digital platform strategy at Enterprise. She is a member of Thoughtworks Technology Advisory Board and contributes to the creation of Thoughtworks Technology Radar.

[enterprise architecture](/tags/enterprise%20architecture.html)

[data analytics](/tags/data%20analytics.html)

[domain driven design](/tags/domain%20driven%20design.html)

[data mesh](/tags/data%20mesh.html)

## Contents

*   [The current enterprise data platform architecture](#TheCurrentEnterpriseDataPlatformArchitecture)
    *   [Architectural failure modes](#ArchitecturalFailureModes)
        *   [Centralized and monolithic](#CentralizedAndMonolithic)
        *   [Coupled pipeline decomposition](#CoupledPipelineDecomposition)
        *   [Siloed and hyper-specialized ownership](#SiloedAndHyper-specializedOwnership)
*   [The next enterprise data platform architecture](#TheNextEnterpriseDataPlatformArchitecture)
    *   [Data and distributed domain driven architecture convergence](#DataAndDistributedDomainDrivenArchitectureConvergence)
        *   [Domain oriented data decomposition and ownership](#DomainOrientedDataDecompositionAndOwnership)
        *   [Source oriented domain data](#SourceOrientedDomainData)
        *   [Consumer oriented and shared domain data](#ConsumerOrientedAndSharedDomainData)
        *   [Distributed pipelines as domain internal implementation](#DistributedPipelinesAsDomainInternalImplementation)
    *   [Data and product thinking convergence](#DataAndProductThinkingConvergence)
        *   [Domain data as a product](#DomainDataAsAProduct)
            *   [Discoverable](#Discoverable)
            *   [Addressable](#Addressable)
            *   [Trustworthy and truthful](#TrustworthyAndTruthful)
            *   [Self-describing semantics and syntax](#Self-describingSemanticsAndSyntax)
            *   [Inter-operable and governed by global standards](#Inter-operableAndGovernedByGlobalStandards)
            *   [Secure and governed by a global access control](#SecureAndGovernedByAGlobalAccessControl)
        *   [Domain data cross-functional teams](#DomainDataCross-functionalTeams)
    *   [Data and self-serve platform design convergence](#DataAndSelf-servePlatformDesignConvergence)
*   [The paradigm shift towards a data mesh](#TheParadigmShiftTowardsADataMesh)

---

Becoming a data-driven organization remains one of the top strategic goals of many companies I work with. My clients are well aware of the benefits of becoming [intelligently empowered](https://www.thoughtworks.com/insights/blog/what-intelligent-empowerment): providing the best customer experience based on data and hyper-personalization; reducing operational costs and time through data-driven optimizations; and giving employees super powers with trend analysis and business intelligence. They have been investing heavily in building enablers such as data and intelligence platforms. Despite [increasing effort and investment in building such enabling platforms](http://newvantage.com/wp-content/uploads/2018/12/Big-Data-Executive-Survey-2019-Findings-Updated-010219-1.pdf), the organizations find the results middling.

[![](/articles/data-mesh-principles/data-mesh-book.jpg)](https://www.amazon.com/gp/product/1492092398/ref=as_li_tl?ie=UTF8&camp=1789&creative=9325&creativeASIN=1492092398&linkCode=as2&tag=martinfowlerc-20)
*   **Image Description**: O'REILLY book cover titled "Data Mesh: Delivering Data-Driven Value at Scale" by Zhamak Dehghani, featuring a brown bird illustration.

For more on Data Mesh, Zhamak went on to write a full book that covers more details on strategy, implementation, and organizational design.

I agree that organizations face a multi-faceted complexity in transforming to become data-driven; migrating from decades of legacy systems, resistance of legacy culture to rely on data, and ever competing business priorities. However what I would like to share with you is an architectural perspective that underpins the failure of many data platform initiatives. I demonstrate how we can adapt and apply the learnings of the past decade in building distributed architectures at scale, to the domain of data; and I will introduce a new enterprise data architecture that I call **data mesh**.

My ask before reading on is to momentarily suspend the deep assumptions and biases that the current paradigm of traditional data platform architecture has established; Be open to the possibility of moving beyond the monolithic and centralized data lakes to an intentionally distributed data mesh architecture; Embrace the reality of _ever present_, _ubiquitous_ and _distributed_ nature of data.

## The current enterprise data platform architecture

It is _centralized_, _monolithic_ and _domain agnostic_ aka _data lake_.

Almost every client I work with is either planning or building their 3rd generation data and intelligence platform, while admitting the failures of the past generations:

*   **The first generation**: _proprietary [enterprise data warehouse](https://www.thoughtworks.com/radar/platforms/enterprise-data-warehouse) and business intelligence_ platforms; solutions with large price tags that have left companies with equally large amounts of technical debt; Technical debt in thousands of unmaintainable ETL jobs, tables and reports that only a small group of specialized people understand, resulting in an under-realized positive impact on the business.
*   **The second generation**: _big data ecosystem with a [data lake](https://martinfowler.com/bliki/DataLake.html) as a silver bullet_; complex big data ecosystem and long running batch jobs operated by a central team of hyper-specialized data engineers have created [data lake monsters](https://www.thoughtworks.com/insights/blog/curse-data-lake-monster) that at best has enabled pockets of R&D analytics; over promised and under realized.

The **third and current generation data platforms** are more or less similar to the previous generation, with a modern twist towards (a) streaming for real-time data availability with architectures such as [Kappa](http://milinda.pathirage.org/kappa-architecture.com), (b) unifying the batch and stream processing for data transformation with frameworks such as [Apache Beam](https://www.thoughtworks.com/radar/languages-and-frameworks/apache-beam), as well as (c) fully embracing [cloud based managed services](https://cloud.google.com/solutions/big-data/#products-and-solutions) for storage, data pipeline execution engines and machine learning platforms. It is evident that the third generation data platform is addressing some of the gaps of the previous generations such as _real-time data analytics_, as well as _reducing the cost of managing big data infrastructure_. However it suffers from many of the underlying characteristics that led to the failures of the previous generations.

### Architectural failure modes

To unpack the underlying limitations that all generations of data platforms carry, let's look at their architecture and their characteristics. In this writeup I use the domain of internet media streaming business such as Spotify, SoundCloud, Apple iTunes, etc. as the example to clarify some of the concepts.

#### Centralized and monolithic

At 30,000 feet the data platform architecture looks like Figure 1 below; a centralized piece of architecture whose goal is to:

*   _Ingest data_ from all corners of the enterprise, ranging from operational and transactional systems and domains that run the business, or external data providers that augment the knowledge of the enterprise. For example in a media streaming business, data platform is responsible for ingesting large variety of data: the 'media players performance', how their 'users interact with the players', 'songs they play', 'artists they follow', as well as 'labels and artists' that the business has onboarded, the 'financial transactions' with the artists, and external market research data such as 'customer demographic' information.
*   _Cleanse, enrich, and transform_ the source data into trustworthy data that can address the needs of a diverse set of consumers. In our example, one of the transformations turns the click streams of user interaction to meaningful sessions enriched with details of the user. This attempts to reconstruct the journey and behavior of the user into aggregate views.
*   _Serve_ the datasets to a variety of consumers with a diverse set of needs. This ranges from analytical consumption to exploring the data looking for insights, machine learning based decision making, to business intelligence reports that summarize the performance of the business. In our media streaming example, the platform can serve near real-time error and quality information about the media players around the globe through distributed log interfaces such as Kafka or serve the static aggregate views of a particular artist's records being played to drive financial payments calculation to the artists and labels.

![](data-monolith-to-mesh/big-data-platform.png)
*   **Image Description**: Figure 1: The 30,000 ft view of the monolithic data platform. A diagram illustrating a centralized "BIG DATA PLATFORM" ingesting data from various "SOURCES" (represented by orange squares) and serving it to different "CONSUMERS" (represented by purple geometric shapes).

It's an accepted convention that the monolithic data platform hosts and owns the data that logically belong to different domains, e.g. 'play events', 'sales KPIs', 'artists', 'albums', 'labels', 'audio', 'podcasts', 'music events', etc.; data from a large number of disparate domains.

While over the last decade we have successfully applied [domain driven design and bounded context](https://martinfowler.com/bliki/BoundedContext.html) to our operational systems, we have largely disregarded the domain concepts in a data platform. We have moved away from _domain oriented data ownership_ to a centralized _domain agnostic data ownership_. We pride ourselves on creating the biggest monolith of them all, the big data platform.

![](data-monolith-to-mesh/no-domain-data-platform.png)
*   **Image Description**: Figure 2: Centralized data platform with no clear data domain boundaries and ownership of domain oriented data. A diagram showing the centralized "BIG DATA PLATFORM" containing various data domains (e.g., Artists, Player events, User sessions) as amorphous blobs within its boundary, still ingesting from sources and serving consumers, but highlighting the lack of external domain boundaries.

While this centralized model can work for organizations that have a simpler domain with smaller number of diverse consumption cases, it fails for enterprises with rich domains, a large number of sources and a diverse set of consumers.

There are two pressure points on the architecture and the organizational structure of a centralized data platform that often lead to its failure:

*   **Ubiquitous data and source proliferation**: As more data becomes ubiquitously available, the ability to consume it all and harmonize it in one place under the control of one platform diminishes. Imagine just in the domain of 'customer information', there are an increasing number of sources inside and outside of the boundaries of the organization that provide information about the existing and potential customers. The assumption that we need to ingest and store the data in one place to get value from a diverse set of sources is going to constrain our ability to respond to proliferation of data sources. I recognize the need for data users such as data scientists and analysts to process a diverse set of datasets with low overhead, as well as the need to separate the operational systems data usage from the data that is consumed for analytical purposes. But I propose that the existing centralized solution is not the optimal answer for large enterprises with rich domains and continuously added new sources.
*   **Organizations' innovation agenda and consumer proliferation**: Organizations' need for rapid experimentation introduces a larger number of use cases for consumption of the data from the platform. This implies an ever growing number of transformations on the data - aggregates, projections and slices that can satisfy the [test and learn cycle of innovation](https://www.thoughtworks.com/insights/blog/how-implement-hypothesis-driven-development). The long response time to satisfy the data consumer needs has historically been a point of organizational friction and remains to be so in the modern data platform architecture.

While I don't want to give my solution away just yet, I need to clarify that I'm not advocating for a fragmented, siloed domain-oriented data often hidden in the bowels of operational systems; siloed domain data that is hard to discover, make sense of and consume. I am not advocating for multiple fragmented data warehouses that are the results of years of accumulated tech debt. This is a concern that [leaders in the industry have voiced](https://medium.all-turtles.com/4-characteristics-of-an-ai-company-3bef59d29f53). But I argue that the response to these accidental silos of unreachable data is not creating a centralized data platform, with a centralized team who owns and curates the data from all domains. It does not organizationally scale as we have learned and demonstrated above.

#### Coupled pipeline decomposition

The second failure mode of a traditional data platform architecture is related to how we decompose the architecture. At 10,000 feet zooming into the centralized data platform, what we find is an architectural decomposition around the mechanical functions of _ingestion_, _cleansing_, _aggregation_, _serving_, etc. Architects and technical leaders in organizations decompose an architecture in response to the growth of the platform. As described in the previous section, the need for on-boarding new sources, or responding to new consumers requires the platform to grow. Architects need to find a way to scale the system by breaking it down to its **architectural quanta**. An architectural quantum, as described in [Building Evolutionary Architectures](http://evolutionaryarchitecture.com/), is an independently deployable component with high functional cohesion, which includes all the structural elements required for the system to function properly. The motivation behind breaking a system down into its architectural quantum is to create independent teams who can each build and operate an architectural quantum. Parallelize work across these teams to reach higher operational scalability and velocity.

Given the influence of previous generations of data platforms' architecture, architects decompose the data platform to a _pipeline of data processing stages_. A pipeline that at a very high level implements a functional cohesion around the technical implementation of processing data; i.e. capabilities of _ingestion_, _preparation_, _aggregation_, _serving_, etc.

![](data-monolith-to-mesh/functional-decomposition.png)
*   **Image Description**: Figure 3: Architectural decomposition of data platform. A diagram depicting the functional decomposition of a data platform into sequential stages: "INGEST", "PROCESS", and "SERVE", with examples of technologies/processes for each stage (e.g., Event Streams, ETLs for Ingest; Cleanse, Enrich, Aggregate for Process; Data marts, APIs for Serve).

Though this model provides some level of scale, by assigning teams to different stages of the pipeline, it has an inherent limitation that slows the delivery of features. It has high coupling between the stages of the pipeline to deliver an independent feature or value. It's decomposed _orthogonally to the axis of change_.

Let's look at our media streaming example. Internet media streaming platforms have a strong domain construct around the type of media that they offer. They often start their services with 'songs' and 'albums', and then extend to 'music events', 'podcasts', 'radio shows', 'movies', etc. Enabling a single new feature, such as visibility to the 'podcasts play rate', requires a change in all components of the pipeline. Teams must introduce new ingestion services, new cleansing and preparation as well as aggregates for viewing podcast play rates. This requires synchronization across implementation of different components and release management across teams. Many data platforms provide generic and configuration-based ingestion services that can cope with extensions such as adding new sources easily or modifying the existing sources to minimize the overhead of introducing new sources. However this does not remove an end to end dependency management of introducing new datasets from the consumer point of view. Though on paper, the pipeline architecture might appear as if we have achieved an architectural quantum of a pipeline stage, in practice the whole pipeline i.e. the monolithic platform, is the smallest unit that must change to cater for a new functionality: unlocking a new dataset and making it available for new or existing consumption. This limits our ability to achieve higher velocity and scale in response to new consumers or sources of the data.

![](data-monolith-to-mesh/axis-of-change.png)
*   **Image Description**: Figure 4: Architecture decomposition is **orthogonal to the axis of change** when introducing or enhancing features, leading to coupling and slower delivery. A diagram showing how changes (represented by orange, red, and yellow lines labeled "Features & Capabilities" like "Podcasts, Video streams") cut across all stages of a functionally decomposed pipeline (Ingest, Process, Serve), leading to high coupling and slower delivery, indicated by frustrated human figures below each stage.

#### Siloed and hyper-specialized ownership

The third failure mode of today's data platforms is related to how we structure the teams who build and own the platform. When we zoom close enough to observe the life of the people who build and operate a data platform, what we find is a group of hyper-specialized data engineers siloed from the operational units of the organization; where the data originates or where it is used and put into actions and decision making. The data platform engineers are not only siloed organizationally but also separated and grouped into a team based on their technical expertise of big data tooling, often absent of business and domain knowledge.

![](data-monolith-to-mesh/siloed-teams.png)
*   **Image Description**: Figure 5: Siloed hyper-specialized data platform team. A diagram showing a centralized "DATA PLATFORM TEAM" acting as a silo between "SOURCE TEAMS" and "CONSUMER TEAMS", highlighting the organizational disconnect and lack of direct interaction.

I personally don't envy the life of a data platform engineer. They need to consume data from teams who have no incentive in providing meaningful, truthful and correct data. They have very little understanding of the source domains that generate the data and lack the domain expertise in their teams. They need to provide data for a diverse set of needs, operational or analytical, without a clear understanding of the application of the data and access to the consuming domain's experts.

In the media streaming domain, for example, on the source end we have cross-functional 'media player' teams that provide signals around how users interact with a particular feature they provide e.g. 'play song events', 'purchase events', 'play audio quality', etc.; and on the other end sit the consumer cross-functional teams such as 'song recommendation' team, 'sales team' reporting sales KPIs, 'artists payment team' who calculate and pay artists based on play events, and so on. Sadly, in the middle sits the data platform team that through sheer effort provides suitable data for all sources and consumptions.

In reality what we find are disconnected source teams, frustrated consumers fighting for a spot on top of the data platform team backlog and an over stretched data platform team.

We have created an architecture and organization structure that does not scale and does not deliver the promised value of creating a data-driven organization.

## The next enterprise data platform architecture

It embraces the _ubiquitous data_ with a _distributed **Data Mesh**_.

So what is the answer to the failure modes and characteristics we discussed above? In my opinion a **paradigm shift** is necessary. A paradigm shift at the intersection of techniques that have been instrumental in building modern distributed architecture at scale; Techniques that the tech industry at large has adopted at an accelerated rate and that have created successful outcomes.

I suggest that the next enterprise data platform architecture is in the convergence of _Distributed Domain Driven Architecture_, _Self-serve Platform Design_, and _Product Thinking_ with _Data_.

![](data-monolith-to-mesh/convergence.png)
*   **Image Description**: Figure 6: Convergence: the paradigm shift for building the next data platforms. A diagram illustrating the convergence of "Distributed Domain Driven Architecture", "Self-serve Platform Design", and "Product Thinking" with "Data" at the center, forming the foundation for the next generation of data platforms.

Though this might sound like a lot of buzzwords in one sentence, each of these techniques have had a specific and incredibly positive impact in modernizing the technical foundations of operational systems. Lets deep dive into how we can apply each of these disciplines to the world of Data to escape the current paradigm, carried over from years of legacy data warehousing architecture.

### Data and distributed domain driven architecture convergence

#### Domain oriented data decomposition and ownership

Eric Evans's book [Domain-Driven Design](https://domainlanguage.com/ddd/) has deeply influenced modern architectural thinking, and consequently the organizational modeling. It has influenced the microservices architecture by decomposing the systems into distributed services built around business domain capabilities. It has fundamentally changed how the teams form, so that a team can independently and autonomously own a domain capability.

Though we have adopted domain oriented decomposition and ownership when implementing operational capabilities, curiously we have disregarded the notion of business domains when it comes to data. The closest application of DDD in data platform architecture is for source operational systems to emit their business [Domain Events](https://martinfowler.com/eaaDev/DomainEvent.html) and for the monolithic data platform to ingest them. However beyond the point of ingestion the concept of domains and the ownership of the domain data by different teams is lost.

Domain Bounded Context is a wonderfully powerful tool to design the ownership of the datasets. Ben Stopford's [Data Dichotomy](https://www.confluent.io/blog/data-dichotomy-rethinking-the-way-we-treat-data-and-services/) article unpacks the concept of sharing of domain datasets through streams.

In order to decentralize the monolithic data platform, we need to reverse how we think about data, its locality and ownership. Instead of _flowing_ the data from domains into a centrally owned data lake or platform, domains need to _host and serve_ their domain datasets in an easily consumable way.

In our example, instead of imagining data flowing from media players into some sort of centralized place for a centralized team to receive, why not imagine a player domain owning and serving their datasets for access by any team for any purpose downstream. The physical location where the datasets actually reside and how they flow, is a technical implementation of the 'player domain'. The physical storage could certainly be a centralized infrastructure such as Amazon S3 buckets but player datasets content and ownership remains with the domain generating them. Similarly in our example, the 'recommendations' domain creates datasets in a format that is suitable for its application, such as a graph database, while consuming the player datasets. If there are other domains such as 'new artist discovery domain' which find the 'recommendation domain' graph dataset useful, they can choose to pull and access that.

This implies that we may duplicate data in different domains as we transform them into a shape that is suitable for that particular domain, e.g. a time series play event to related artists graph.

This requires shifting our thinking from a _push and ingest_, traditionally through ETLs and more recently through event streams, to a _serving and pull_ model across all domains.

The _architectural quantum_ in a domain oriented data platform, is a _domain_ and not the pipeline stage.

![](data-monolith-to-mesh/data-domains.png)
*   **Image Description**: Figure 7: Decomposing the architecture and teams owning the data based on domains - source, consumer, and newly created shared domains. A diagram showing a decentralized approach where data ownership is distributed to "SOURCE DOMAINS" (e.g., Player Events, Artists) and "CONSUMER DOMAINS" (e.g., Recommendations, Notifications), with data flowing between them, contrasting with the centralized monolithic model.

#### Source oriented domain data

Some domains naturally align with the source, where the data originates. The _source domain datasets_ represent the _facts and reality of the business_. The _source domain datasets_ capture the data that is mapped very closely to what the operational systems of their origin, _systems of reality_, generate. In our example facts of the business such as 'how the users are interacting with the services', or 'the process of onboarding labels' lead to creation of domain datasets such as 'user click streams', 'audio play quality stream' and 'onboarded labels'. These facts are best known and generated by the operational systems that sit at the point of origin. For example the media player system knows best about the 'user click streams'.

In a mature and ideal situation, an operational system and its team or organizational unit, are not only responsible for providing business capabilities but also responsible for providing the _truths of their business domain_ as source domain datasets. At enterprise scale there is never a one to one mapping between a domain concept and a source system. There are often many systems that can serve parts of the data that belongs to a domain, some legacy and some easy to change. Hence there might be many _source aligned datasets_ aka _reality datasets_ that ultimately need to be aggregated to a cohesive domain aligned dataset.

The business facts are best presented as business [Domain Events](https://martinfowler.com/eaaDev/DomainEvent.html), can be stored and served as distributed logs of time-stamped events for any authorized consumer to access.

In addition to timed events, source data domains should also provide easily consumable historical snapshots of the source domain datasets, aggregated over a time interval that closely reflects the interval of change for their domain. For example in an 'onboarded labels' source domain, which shows the labels of the artists that provide music to the streaming business, aggregating the onboarded labels on a monthly basis is a reasonable view to provide in addition to the events generated through the process of onboarding labels.

Note that the source aligned domain datasets must be separated from the internal source systems' datasets. The nature of the domain datasets is very different from the internal data that the operational systems use to do their job. They have a much larger volume, represent immutable timed facts, and change less frequently than their systems. For this reason the actual underlying storage must be suitable for big data, and separate from the existing operational databases. Section [Data and self-serve platform design convergence](data-monolith-to-mesh.html#DataAndSelf-servePlatformDesignConvergence) describes how to create big data storage and serving infrastructure.

Source