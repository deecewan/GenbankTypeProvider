# Future Work

There is much work remaining on this project, and it will be a challenge to
manage time effectively enough to complete it. As such, the following list has a
semblance of priority.

Obviously, building out support for the Genbank file format is of highest
priority. The remaining features rely on it, and it will give a better
understanding of the minimum loading times required for the project.

With that in mind, optimizing for initial load and subsequent builds. This will
be done in a number of ways. Firstly, delaying requests as far as possible, to
ensure that downloads (and therefore slow-downs) only happen when it can be
almost guaranteed that the user needs the data. With this, being able to add
types as they arrive in the `Stream` from the `WebRequest` would significantly
improve the usability of the provider when combined with Intellisense. However,
this will not significantly improve compile times, as all the data in the chain
will need to be loaded when compiling.

To make that tolerable, before attempting to optimise prematurely, a cache
should be added. It is currently unknown if an in-memory cache will suffice, or
if a file system cache is required. Either way, no network requests will
significantly improve speeds, which will lead to improved speeds for all but the
first compile.  Implementation will likely be through an adapter which will
replace direct web request calls. If the requested URL exists in the cache, the
cached version will be returned. Otherwise, the request will be made, result
stored, and returned. This makes caching an implementation detail that doesn't
affect the code of the provider itself, so multiple strategies can be attempted
to optimize for the best times.

For extra functionality, making use of the parameterization capability of type
providers can lead to some useful outcomes. For instance, being able to search
for a genome, taxon or other relevant metadata directly instead of moving
through the entire FTP system will no only improve times, but rationalise the
long-term storage of these files to the Genbank servers, where they are
consistently updated.  Parameterization could also be used to allow for loading
Genbank format files directly to make use of the providers forthcoming analysis
tooling, or to access a remote server other than NCBI Genbank's.  Below is an
example of what a parameterization API may look like.

```fsharp
type NameSearch = Genbank.Search.Name<"Acanthamoeba*">
type LoadByAccession = Genbank.Load.Accession<"CP016816">
type LoadFromFile = Genbank.Load.File<"./ASM170832v2_genomic.gbff">
type AlternativeRemove = Genbank.Remote<"ftp://my-remote.com/genbank">
```

Creating an easy installation process is paramount. This will reduce the barrier
to entry for new users, and drive adoption of the project. Distributing via
NuGet or a similar package manager for one-line install would be ideal. More
research is required to determine the viability of this approach.

Finally, taking on board community feedback for some 'smart operations' the
provider could perform on the data. Examples may include comparing two genomes
to identify particular similarities or differences. This functionality seems
like it may be slight beyond the scope of a traditional type provider, but if it
proves useful, would be a great addition.
