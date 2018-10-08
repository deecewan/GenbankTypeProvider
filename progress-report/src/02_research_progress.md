# Existing Project

As of the last report, there was a type provider with limited functionality. The
implementation was able to download listings of taxa and genomes from the
Genbank service, and provide them as types. Included was the FTP urls where the
given data could be found, and a rudimentary file parser for the accession data.
This acted as a proof of concept of what was to come. It enabled a basic type
provider to be built and for a better scope of what was required in the future.

# Updates to the Project

There have been few, if any, user facing changes to the project since the last
report. Most of the changes have been to the internals of the tool itself.

### Enhanced Logging Capabilities
In the previous version of the project, there was a logger added. However, it often did not work, was awkward to use, and was not used widely in the application. This made it difficult to debug issues in the already notoriously difficult conditions of working with Type Providers. The following changes were complemented by far more complete instrumentation throughout the library.

#### Colorized Output
A fairly minor addition, but an immeasurably useful one when confronted by a wall of text at a command prompt. A library, `Colorful.Console`, was used in order to print coloured text to the prompt. This included colouring `success`, `info`, `warn` and `error` logs in different colours for easy identification. The other feature was providing each logger with a new colour to print its name in. This allowed for easy identification of which module was producing which output.

A downside to adding this functionality, however, was that the distribution mechanism for the library seemed unable to pick up, or instrument, the dependency correctly when bundling for release. The net-result of this is that including the library in any other project resulted in a compiler error due to the missing dependency and the project unable to compile. This is discussed in further detail in [NuGet Distribution](#nuget-distribution).

#### Ergonomic API
The previous API was not easy to use. The logger accepted only strings, which meant that there were a lot of log lines that looked as follows:

```fsharp
logger.Info(sprintf("The result of the previous call was: %s")(result))
```
With the new implementation, the `sprintf` functionality has been incorporated using `Printf.ksprintf` to enable a far more user friendly API. The same call from above now looks as follows:

```fsharp
logger.Info("The result of the previous call was: %s") result
```
This new API still allows for the same amazing functionality of F#'s type-checked `printf` statements, but reduces the verbosity of the logging statements.

#### Functional file-level log writing
A part of the previous logging library that never quite worked correctly was the `FileWriter` - it would cause the application to crash, or would not result in a log being created. This has been fixed to allow for the log-lines to be appended to a file, which can then be watched with `tail` as new log entries are added for a live execution summary.
This makes it much easier to follow along with, as well as debug, the library when it is running in another project. Importantly, however, writing to a file causes a slowdown, as there is a non-zero cost to appending to a file, which is exacerbated by the sheer volume of logging. This could be fixed long-term by adding an in-memory buffer, and flushing it after a predetermined time. This approach would provide a 'best of both worlds' result, as long as the time between buffer writes was fast enough that it did not hinder a user watching the logs.

### NuGet Distribution

A big part of the original intention for the project was making it easy to consume by others. The primary way of consuming third-party libraries in the .NET ecosystem is using the NuGet package repository. In order to following the conventions of the community, a NuGet package was created an published. This was initially done as a proof-of-concept to ensure that it would work when required in the future.

Building the package initially proved straightforward, using a `paket.template` and the `paket pack` commands.

The [first version](https://www.nuget.org/packages/GenbankTypeProvider/1.0.0)
was published and tested in a [very small demo
project](https://github.com/deecewan/GenbankDemoProject). As soon as the newly
published package was added to the example project, the compilation broke and
complained of an error with `Colorful.Console`.

![The type provider errored out whenever the Colorful.Console library was
used.](src/images/colorful_console_error.png)

This meant that, not only were the provided types not being added during compilation, but there was no compilation possible. No concrete cause for the error has been found as of yet. The current work-around is to remove the `Colorful.Console` usage before packaging the application.

A fixed version was released as [Version
1.0.1](https://www.nuget.org/packages/GenbankTypeProvider/1.0.1), and using that
dependency results in the current implementation of the type provider being
usable in external project. This makes consumption of the library relatively
trivial, which should help aid adoption. Hurdles still to cover include macOS
and Linux support to provide a truly cross-platform user experience.

### .NET Bio
This project centres around bioinformatic data, so the obvious choice for a dependency is .NET Bio, as it includes parsers for common genomic data formats, including Genbank. This dependency was added, and a proof-of-concept was created to parse a Genbank file based on a provided type. This successfully retrieved the file, parsed it, and made the internal metadata accessible programmatically.

This was done by retrieving the ftp url of an item via the existing provided type, and then finding the correct Genbank-formatted file. Once the location of the file is determined, an FTP file download is initiated. This downloaded file is compressed using GZip, so the built-in `System.IO.Compression.GZipStream` is used by piping the FTP response stream directly into it. The `Bio.IO.GenBank.GenBankParser` from .NET Bio also accepts a stream, so the resulting output of the GZip stream is passed directly to the parser.

At present, nothing constructive is being done with this functionality. However, it was crucial to start this work and ensure it was feasible as the rest of the the project depends on the information. Further consultation is required to determine what metadata is useful, and which types to attach attributes to.  This will be a large part of the future of the project. Adding the metadata to the types will make the tool useful, and allow the data to be processed.

#### Problems Encountered
Adding the dependency took significantly longer than expected, due to problems with the .NET Bio library. The library has not been updated for over 7 months, which in and of itself is not a problem. The problem stems from the fact that the most recent version, `3.0.0` is still in `alpha`. This version of the project must be used in order to compile for modern F#. It required changes to the build pipeline that significantly reduced understanding of the process.

Online REPLs for F#, such as [.NET Fiddle](https://dotnetfiddle.net) support adding dependencies, which would make them a great way to quickly evaluate the project's suitability and feature set. However, .NET Fiddle, from testing, does not support `alpha`-level packages (or rather, any that deviate from the standard numbering convention).

The highest-impact change, however, is that the library no longer builds on platforms other than Windows. Reduced understanding of the build pipeline has thus far made fixing this issue impossible. To combat this, a transition to the standard F# ['Project Scaffold'](https://fsprojects.github.io/ProjectScaffold/) is planned for the future. This implements a build pipeline with [FAKE (F# Make)](https://fake.build/), which, from testing, results in more standardised, less platform-dependent builds. Additionally, the build configuration is written in an F#-based DSL, instead of the verbose, complex and unsightly MSBuild XML Syntax.

### Caching

One of the bigger pieces of work to be undertaken on the project is the development of a caching solution. The idea behind the caching was to reduce the time taken for requests and processing to generate the types.

The strategy was implemented at the network layer by replacing all FTP request with requests passing through the cache. As is standard, if the URL had been previously requested, the cached result was returned. If it had not been requested, a request was made, and the result was stored in the cache for any subsequent requests.

This was implemented as an in-memory cache, as maintaining a file system cache seemed significantly more difficult, and it was thought in-memory would be sufficient for the use-case. The implementation was successful, and the cache was populated as requests were made.

However, it was quickly noticed (thanks to the logging functionality) that, while the cache was being populated, it was never being hit subsequently. It was later established that the type provider maintained its *own* cache in-memory of types that had been generated. The net result of this is that once a particular type has been used, it is never required to be generated again, so never re-calls the HTTP endpoint.

This shows that the concept of using an in-memory cache was not suitable, and a file system cache should instead be used. The file system cache will have the benefit of drastically improving subsequent boot times. On the first invocation of the compiler, the cache warm-up penalty will still apply. However, any subsequent usage will result in cached value retrieval rather than FTP requests. This also allows a smarter system around cache invalidation to be implemented. With an in-memory cache, as currently understood, as soon as the compiling program (whether the IDE or the actual compiler) is closed, the cache of types is lost. The Genbank data store is only updated every three months. It is unlikely that a user of an IDE would maintain their instance running for so long, so there would have been many wasted HTTP calls with the in-memory method. By using a file system cache, a timestamp of the last change to Genbank can be maintained along side and checked during initial start-up, effectively only ever invalidating the cache when there are actual changes to the content on Genbank. This could possibly use something like the HTTP `ETAG` to only re-download files that have actually changed, rather than invalidating the cache wholesale.

The file system cache doesn't solve every use-case, however. Currently, the type provider loads *all* children at a given layer. For example, entering `Genbank.Provider`, the entire list of available taxa are downloaded. When a taxon is selected, and the genomes queried, as with `Genbank.Provider.Archara.Genomes`, the entire list of genomes is downloaded, processed and added as children. This goes on as far down as the user continues.

This means that, in an application that uses the provider as a verification/data collection tool, and not an exploration tool, many redundant API requests will be made, and a lot of unused data will be processed. The file system cache will not help reduce the amount of data that is being processed, even if it does save on a lot of the download of data.  This can likely be mitigated to a certain extent by being less 'generic' about how the cache works. At present, the cache has only got FTP level calls, such as `loadFile` or `loadDirectory`. If the cache had knowledge about how to process the data, it could store the data post-processing, and cut down on the amount of processing that would need to be done on every boot.

The other major problem that this caching strategy doesn't solve is that of an
ephemeral compile time environment, such as a continuous integration service.
This could be solved by the end user by adopting a caching solution put in place
by the continuous integration provider. This use-case also seems to be beyond
the scope of this project.
