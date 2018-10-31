# Research Progress

startcyancolor

Initially, the plan was to base the work on another type provider built by a
QUT student previously. The proposal highlighted that the first step was to
complete a review into that provider, and decide on its suitability to use as
a base. Research down that avenue showed that whilst the project would be a
great source of in- spiration, it had focussed on achieving goals in a
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

These Taxon types were then expanded to add all their applicable genomes as
children. This was initially added under a subtype of `Genomes`, which delayed
the loading a little further in the vague hope this would improve performance
(it did - but the cost was payed later on instead).

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

- allowed me to process the genbank files
- initial implementation of genbank types
  - in a non-scalable, pretty non-useful way

### Nuget Distributions

### Switch to FAKE

- on recommendation from the community
- didn't work cross-platform easily
- was hard to understand the generic boiler plate involved

### Switch to Nuget

- much better cross-platform support (built on macOS first)
- 'from first principles'
- no boilerplate at all - everything made from scratch
- was able to get the provider booting on Windows and macOS some of the time

### Switch back to Paket

- much better dependency management
- minimal footprint of the nuget base meant that paket was easily understood

### Implementation of Runtime + DesignTime

- allowed me to hand off to the runtime where required
- would hopefully allow for iteration over the types at some point
- involved a restructure that dropped support for the annotation hashes file