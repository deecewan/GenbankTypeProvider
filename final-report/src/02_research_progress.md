# Research Progress

startcyancolor

Initially, the plan was to base the work on another type provider built by a
QUT student previously. The proposal highlighted that the first step was to
complete a review into that provider, and decide on its suitability to use as
a base. Research down that avenue showed that whilst the project would be a
great source of inspiration, it had focussed on achieving goals in a
different manner to what was intended for this research.

To that end, a new type provider was created. This facilitated gaining a far
deeper understanding of how type providers work that will accelerate development
in the future.

endcyancolor

The rationale was also that starting from scratch would result in a stable base
and in-depth knowledge of how the provider was build, resulting in greater
familiarity. Whilst that somewhat came to fruition, there were many challenges
met with project structure, and the project was re-written (albeit not
completely from the ground up) 3 more times during the course of the year.

A fresh start did allow for greater control over exactly what use-cases would be
met, as well as allowed for architecting in such a way to allow for the
long-term goal of expanding to more than one data source.

## Initial Implementation

The initial implementation of the type provider was able to query the Genbank
database for a very limited subset of information. All the taxa from the
database were downloaded and inserted as types inside the provider, as shown in
Figure. \ref{all_taxa}.

![The type provider shows all the taxa available on Genbank
\label{all_taxa}](src/images/all_taxa.png)

This implementation involved downloading all the data using a `WebRequest` to
[the Genbank FTP server](ftp://ftp.ncbi.nlm.nih.gov/genomes/genbank/) requesting
the full directory listing. The listing was filtered down to just the
directories, which represented the names of the taxa available. This list was
then mapped over to produce the types.

There was some light processing done on the names to make them more
human-friendly (switching out underscores for '\texttt{,\textvisiblespace}',
displaying the name in *TitleCase*, etc), however there were problems with
Intellisense not showing any options containing commas.

These taxon types were then expanded to add all their applicable genomes as
children. This was initially added under a subtype of `Genomes`, which delayed
the loading a little further in the vague hope this would improve performance
(it did - but the cost was paid later on instead).

Once loaded, the user could choose a taxon, and explore the genomes contained
within. This made use of the `AddDelayedMembers` API, provided in the Type
Provider SDK, which was a way to asynchronously request type information. This
method on a type meant that the request for a type's children would only fire
once it had been 'dotted in to' (accessed, and followed by a dot, which would
trigger Intellisense to show).

At this stage, it was identified that the user's experience would be
significantly better if Intellisense could be populated asynchronously, too.
This would mean that the user doesn't have to wait for *all* data to be loaded
and processed - each item could show in the list as it was ready. Unfortunately,
this was not (and is not) currently possible.

Genomes were loaded in much the same way as taxa, making a `WebRequest` to the
relevant FTP directory, and listing it's child directories (the child
directories are the assembly names for the genomes). The request retrieved the
`latest_assembly_versions` for a given genome, inside each of these would exist
any number of assemblies. Each of these assemblies was then queried for it's
children, which contained the URLs to the relevant files. 

startcyancolor

It was quickly discovered that making a minimum of 3 requests (at times up to
10, depending on how many `latest_assembly_versions` exist) took an exorbitant
amount of time. A pattern emerged in how the naming of these items was done
(based entirely on the accession number of the assembly version in question), so
only one request was actually required, to the `latest_assembly_versions`
directory. This was assumed to be present for every genome and accessed directly
as a child to the selected genomes FTP URL. The accession numbers for each
assembly version was gathered, and from there, the names and locations for the
relevant files could be determined by applying the same pattern noticed
previously.

The code-block below shows a snippet of how the annotation hashes file and the
Genbank data file locations were determined.

```fsharp
// `item' is an `FTPFileItem' which represent the name
// and location of an FTP file
AnnotationHashes: item.childFile("annotation_hashes.txt");
GenbankData: item.childFile(item.name + "_genomic_gbff.gz");
```

Removing these excess requests resulted in significant speed-up of the provider.

endcyancolor

The '`annotation_hashes`' file was chosen to use as a proof-of-concept, because
it was a very basic format (tab-separated values, first row headings, second row
content) and could be parsed without having to add any external dependencies.
This was used to develop the basis of the type provider - file access was
going to be a necessity, and there was definitely going to be file downloading.

Once the user had found the genome they were looking for, they were able to drill
into it, is shown in Figure. \ref{selected_genome}.

![A user is able to view the properties that live in the `annotation_hashes.txt`
file for every
genome\label{selected_genome}](src/images/selected_genome.png)

### Output

A type provider with minimal functionality was produced. It had the bare-bones
required to be used as a base for subsequent development.

### Challenges Identified

- Optimisation for loading. The data took a long time to gather, and resulted in
  too long a wait before Intellisense showed up, making it unreasonable for
  real-world usage
- Difficulties in development. There was no good strategy for developing type
  providers, and that resulted in significant time delays during development,
  involving completely exiting the IDE and restarting it.
- Inexperience. There was a lack of familiarity with type providers, F#, and the
  .NET ecosystem as a whole. This resulted in slow progress, and sections of
  code re-written many times over the course of development as knowledge of
  idiomatic patterns increased.

## Iterations

### Caching

The first major iteration was caching. The lack of caching was causing the
application to re-fetch the data every time the provider was used. Some files
were extremely large, and also required processing after being fetched. This
made the provider not fit for real-world use.  Adding a cache would reduce the
time taken for requests, and perhaps even allow the provider to amortise the
cost of parsing over many runs.

A first attempt was made by creating a proxy for the FTP requests that were
being sent. Now, instead of being passed directly to the network, the request
was passed to a middle-man, which would check for the presence of the result in
an *in-memory* cache. If the data was present, it would be returned. Otherwise,
the request was sent to the outside world, and the returned data stored for
subsequent calls.

In-memory caching was used initially because a file-system cache comes with a
plethora of problems, not least of which involves how to invalidate the cache.
An in-memory cache comes with a built-in invalidation mechanism - stop whatever
program is using it.

Logging showed that, while the cache was being populated by requests as they
were first made, it was never being hit subsequently. It was quickly established
that the type provider maintained its *own* in-memory cache of types that were
produced (in fact better than the implemented one - it stored the results
*after* processing, so that cost was never paid again). The net result of this
is that once a particular type has been used, it is never generated again, so
the cache is never hit.

After proving the concept of an in-memory cache was not suitable, it was decided
to implement a file system cache. Whilst the in-memory cache tried (and failed)
to improve load times during use, a file system cache would have the added
benefit of drastically improving initial load times, when an in-memory cache
would be cold. There would still be a cache warm-up penalty, but it would be
paid once overall, and not once per compiler invocation.

The in-memory cache implemented an interface that was written to generically
allow for caching of requests. This meant that switching to a file-system cache
required no changes to code outside of the cache.

At completion, the file-system cache provided great improvement to initial load
times, but included no method for invalidation, and cached only network requests
\- there was no was to cache results post-processing. This was a result of
making the cache too generic. This cache was not to be used in any project
outside of this one, so should have been made more specific to the project in
order to allow the storage of results in a more-easily machine-readable format.

### .NET Bio

The addition of .NET Bio allowed the project to parse and make use of
bioinformatic data. The .NET Bio library brought with it parsers for common
genomic data formats, including Genbank. A proof-of-concept was created by
adding the library and using it to manually process a file retrieved from the
FTP server.

The Genbank files on the FTP server were compressed using GZip, so the built-in
`System.IO.Compression.GZipStream` was used to generate an uncompressed stream
that could be piped into `Bio.IO.GenBank.GenBankParser`. With a small amount of
processing, the Genbank metadata was retrieved.

Nothing constructive was done with the data at that point. However, it was
crucial to get an understanding of how this process worked so that the rest of
the project could proceed with the knowledge that the data was available.

The data was used to create some types under the genome, which were fairly basic
and very limited. They would not allow the user to genuinely explore the data
stored in the file, as the types were a hard-coded subset of the properties
available. This served as a basis for expansion in the future.

### Nuget Distributions

In order to fulfil the goal of distributing the library to the community at
large, a distribution was created for release on Nuget. This was a fairly
straight-forward process which involved creating a `paket.template` file,
invoking the `paket pack` command, and then uploading to Nuget. At that point,
in theory at least, any other F# project should have been able to add the
library as a dependency, and Visual Studio would start generating Intellisense.

In practice, however, this did not work as expected. There was partial success,
but a lack of understanding of the dependency resolution system meant that many
problems were created by virtue of not having the necessary `DLL`s loaded into
the `REPL` when testing.

A version that had partial support was eventually released, but did not support
any platform aside from Windows. [GenbankTypeProvider
1.0.1](https://www.nuget.org/packages/GenbankTypeProvider/1.0.1) was made
available on Nuget for usage, which allowed a [very small demo
project](https://github.com/deecewan/GenbankDemoProject) to be built.

### Switch to FAKE

There was a community recommendation to switch to FAKE. FAKE is 'F# Make' and
allowed for deterministic builds across platforms, and automated packaging and
publishing of the resultant bundles.

The project structure was converted to the F# community standard, which included
FAKE support. This did not result in easier builds across platform (or on
Windows), and in fact made the build process more of a black-box, because the
boilerplate `build.fsx` (FAKE's configuration DSL) was very opaque and complex.

### First Principles Architecture

After the failure of the initial FAKE implementation, a 'from first principles'
approach was taken to the second major architecture change. To ensure better
cross platform support, this was all done on macOS with `mono`.

This effort involved no boilerplate or scaffolding. The entire project was
created by hand, using `dotnet new` commands to create a solution and the
children projects. The result of this was a build that worked easily (just
entering `dotnet build`) and a dependency manager in the form of Nuget (a
change from the previously used `paket`).

After this conversion, there was a `DLL` capable of being built and run on both
platforms. The problem was that transitive dependencies became quite painful to
manage. Dependencies of dependencies would either not be installed as expected
or not be loaded into the runtime as expected.

### Switch back to Paket

An incremental change was made to the base provided by the new `dotnet`
architecture, involving the (re-)addition of `paket` to the project. This,
again, was done 'from first principles', manually adding the necessary items,
and not adding any additional, unknown lines of code to the project.

`paket` allowed for better resolution of transitive dependencies, and the
minimal footprint of the project after the new architecture meant that paket was
easily added and understood.

### Implementation of Runtime + DesignTime

The next major iteration came in the form of a 'RunTime' file. This follows an
architecture similar to that of the `WorldBankProvider` by FSharp.Data, where
there is a type provider that eventually hands off to a runtime file that will
provide complex values for consumption.

A limitation of type providers is that complex values can't be inserted into the
underlying assembly. This precludes the usage of `GenBankMetadata` (the format
supplied by .NET Bio), and even `seq`, which are an F# built-in sequence class.

The runtime class went through various iterations whilst in the discovery
process, and eventually ended up loading the same data as the provider based on
an FTP URL (strings are able to be passed in from the type provider to the
assembly). The type provider would insert a new instantiation of the class, and
at runtime, the consuming program would be given, essentially, an instance of
the `GenBankMetadata` class.

Another benefit to this architecture was that the data could be passed to a
class that implemented an iterable interface, which means that a consumer could
use `Seq.map` to iterate over the collection. This is what gives rise to a far
richer experience than could be achieved without the type provider. This
interface provides users with a way to compare, contrast and collect large
swaths of data with well-defined programming constructs.

This architecture was an iterative improvement, but involved a heavy departure
from the previous incarnation - it involved dropping support for the 'Annotation
Hashes' file (which had served its purpose of being a placeholder for real
data), and restructuring the provided types.

The runtime bridge is where the most important gains were made, though they were
relatively small to the end user, and took the bulk of the time due to research
and discovery while implementing. This is also where the majority of future
development will take place.
