# Future Work

## Intellisense

Given the primary function of this library was to enable exploration of data
using Intellisense, lacking this functionality is a major drawback. This should
be the highest priority for work going forward.

## Improvements

The most important piece of work to be completed in the future is the bundling
of the software in a way that can be easily consumed by an end user.

Some further work to all mapping over assemblies, genomes and even taxa would be
beneficial. This would allow more complex work-flows that can process more data
and hopefully help a user draw some conclusions about the data.

## Search and Scope

Some additional functionality around search, or limiting the scope of a query,
would allow the provider to significantly cut down on what it was processing.
Because most of the file names can be obtained deterministically, and the
functions to generate the types are split out at each level, it is conceivable
that a user could limit the scope to a particular taxon, or genome. This would
allow for a user to work with a genome from the Bacteria taxon without requiring
huge performance improvements for the entire provider.

An example API might look something like the following:

```fsharp
type Bacterium = Genbank.GenomeProvider<"Bacteria", "Abditibacterium, Utsteinense">
let name = Bacterium.Locus.Name
```

## Linking of Genomes

A feature of Genbank files is that they contain a list of related genes.
Ideally, these genes should link, through the types, to a list of related
genomes. Establishing how to re-use types in this manner, and providing a
`siblings` or similar type on any given genome would greatly improve the usage
of the tool for the purposes of exploration.

## Context-Aware Caching

Allowing the cache to store data post-processing will improve the usage times,
and reduce memory consumption of the provider. It will also mean less of a
reliance on streams.

Currently, there is no strategy surrounding this. As the data is all processed
differently depending on where it is used, an initial thought is to pass a
callback to the request that does the processing. The result of this callback
would be the processed data, and could be cached ready for the next run. A lack
of familiarity with .NET means a lack of understanding of the best concepts for
machine readable data.

## FAKE Builds

To come full circle with the project, builds should be completed using FAKE.
Extra experience with FAKE whilst building Forest and Texta, and starting from
first principles, resulted in a much stronger understanding of both FAKE and
its benefits.