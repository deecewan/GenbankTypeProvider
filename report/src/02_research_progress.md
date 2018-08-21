# Research Progress

Initially, the plan was to base the work on another type provider built
by a QUT student previously. The proposal highlighted that the first step was
to complete a review into that provider, and decide on its suitability to use as
a base. Research down that avenue showed that whilst the project would be a
great source of inspiration, it had focussed on achieving goals in a different
manner to what was intended for this research.

To that end, a new type provider was created. This facilitated gaining a far
deeper understanding of how type providers work that will accelerate development
in the future.

This ensured a stable base and strong knowledge of the infrastructure used to
build the provider, as well as greater control of the use cases to be met, and
has allowed the design to reflect the long-term goal of expanding past just the
Genbank data source.

Due to starting from a clean slate, a significant amount of time was spent
researching type providers and learning more about the way they interact with
the F# compiler.

## Current Progress

The new type provider that has been created is able to dynamically query the
Genbank database for a very limited subset of information. All the available
taxa are downloaded and inserted as types, as shown in Figure. \ref{all_taxa}.

![The type provider shows all the taxa available on
Genbank\label{all_taxa}](src/images/all_taxa.png)

These are downloaded by making a `WebRequest` to [the Genbank FTP
server](ftp://ftp.ncbi.nlm.nih.gov/genomes/genbank/) requesting the full
directory listing. This listing is filtered down to just children directories,
as these are the names of the taxa available, and the resultant list is mapped
over to create types.

The name is determined by calling `toTitleCase` on the directory title, and
replacing underscores with '\texttt{,\textvisiblespace}'. However, this seems to
have partially broken Intellisense, as these 'split' names do not appear in the
completions. They still type check as expected, but do not receive completion
assistance. This could likely be fixed by switching the split strategy for
something else.

Each type has 2 children added. Firstly, the FTP URL the node resolves to. This
is attached to every remote reference, to be used both for debugging,
downloading manually, and, as an implementation detail, to easily be able to
retrieve children of that file. The second child is entirely a misdirection
layer.

The second type attached is `Genomes`. The purpose of `Genomes` is to hold the
genomes that belong to the taxon. Initially, this was not included, and the
genomes were attached directly to the taxon type. This resulted in FTP requests
for every taxon's children as soon at the provided type was used. This caused a
massive slowdown, and crashes due to high memory consumption. The documentation
indicated that delayed members should not be requested 'until necessary'.
However, Intellisense and the compiler both deemed it necessary to make the
requests before a taxon was selected. The `Genomes` misdirection results in only
the genomes for the selected taxon to be downloaded.

Once all the taxa are loaded, a user can step into any taxon and then into the
`Genomes` type of that taxon. Figure. \ref{taxon_genomes} shows the Intellisense
completion after stepping into the 'Protozoa' taxon. Seen are a subset of the
child genomes of that taxon, attached to `Genomes` as nested types.

![A taxon and its nested `Genome` types. These are loaded by the compiler only
when necessary\label{taxon_genomes}](src/images/taxon_genomes.png)

It should be noted here that by using the `AddDelayedMembers` API in the type
provider, the fetching of the genomes only happens once a taxon has been
selected, and only the genomes for that taxon are loaded. This minimizes the
amount of data that needs to be retrieved and the length of time spent waiting
for the Intellisense to appear.

These genome types are created in a very similar way to the parent. A
`WebRequest` is made to the FTP URL for the taxon in question, and the directory
listing is retrieved. It is again filtered down to directories, as these are the
genomes. There are some items in the list that are *not* genomes (such as
metadata files) and will need to be filtered out in the future.

Ideally, progressively injecting types as the requests complete would be ideal.
However, from research into the way type providers work, and investigation of
the API, it seems that while the request for types can be made asynchronously,
all the types generated from the asynchronous request must be attached to the
parent at one time. This definitely needs further investigation, as this will
help significantly with the initial load problem.

Once the directory is parsed, a `Genome` type is created. This is a simple type
definition. It cannot be used as a value and exists solely to attach children
to. The FTP URL is attached and `AddDelayedMembers` is used to create the
'exploration' layer.

This exploration layer initially made a request to the child, looked for a
folder named `latest_assembly_versions`, made a subsequent request, looked for
any directories or symlinks (one genome may have many
`latest_assembly_versions`), and then requests through each of these for the
relevant files.

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

The `annotation_hashes.txt` file was chosen to use for a proof-of-concept as the
file exists for, as far as is known, every genome on the server. It is a simple
file, with just column names and values in a tab-separated format. File access
was always going to be a necessity, so this was built around an easy file format
instead of building out complex parsers for other files early in development.

Once the user has found the genome they are looking for, they are able to drill
into that as well. The 'Annotation Hashes' section, as determined from the file
mentioned above, is shown in Figure. \ref{selected_genome}.

![A user is able to view the properties that live in the `annotation_hashes.txt`
file for every
genome\label{selected_genome}](src/images/selected_genome.png)

If only one `latest_assembly_version` exists for a given genome, the additional
details gathered (such as `Annotation Hashes`) are attached directly to
the genome. If there are multiple versions available, each becomes a type named
after the Accession Number, and the additional details are attached below that.

These are not attached as 'delayed' members. Rather, the additional details
types have delayed members to retrieve the file in question, parse and attach
types.

