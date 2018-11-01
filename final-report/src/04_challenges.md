# Challenges

## Distribution

Due to the problems with dependency resolution, a major challenge is
distributing the library in a way where users can consume it in the simplest
way possible. In order to truly solve this problem, a 'one-touch install'
process should be achieved, whereby the user is required to only install one
package and have Intellisense in their editor of choice.

## Debugging

Debugging has been possibly the biggest issue that has made it difficult to
develop a type provider. Debugging is crucial to working with any new piece of
technology, and especially when the output is more-or-less invisible. Techniques
that may work to debug one issue could completely fail the next. Even when a
consistent solution is found, in general it is very slow work.

The general work-flow involves using F# Interactive and Forest for the most
part. When more in-depth debugging is required, Visual Studio under Windows is
capable of attaching to the executing F# Interactive process, and breakpoints
can be added to the provider. Any changes require stopping the debugger, closing
F# Interactive, recompiling, opening F# Interactive, re-attaching the debugger,
and running the same set of commands to get the provider back into the same
state it was in previously.

If a test of Intellisense is required, a similar process must be used. Except
instead of opening the relatively light-weight instance of F# Interactive, an
entire new Visual Studio instance must be opened and the debugger attached to
it. Often times when debugging in this manner, symbols would fail to load, so no
breakpoints would be hit. This required restarting the whole process. Each
incremental change required closing the IDE, running the compiler (it can't run
while the DLLs are being used to provide types), opening the IDE, attaching the
debugger, and finally opening a project that uses the provider.

### Wasted Network Requests
The provider makes a lot of redundant API calls and does a lot of unused data
processing if the types used are already explicitly defined. This makes sense
from the point-of-view of a type provider, because the compiler has to assert
that the types actually exist in order to verify correctness of the program. 
It is currently unknown whether a type can be 'verified' rather than
'generated'. That is to say, given a complete type definition like 

```fsharp
Genbank.Provider.Other.Genomes.``synthetic_Mycoplasma``.``Annotation Hashes``.``Assembly accession``
```

is it possible to verify that just that type is correct using the provider, or if every layer will have to be generated again. Given a type, the URL to lookup to verify if a type is valid can be determined by simply reversing the steps used to generate the type.

### Linking of Genomes
A feature of Genbank files is that they contain a list of related genes.
Ideally, these genes should link, through the types, to a list of related
genomes. Establishing how to re-use types in this manner, and providing a
`siblings` or similar type on any given genome would greatly improve the usage
of the tool for the purposes of exploration.
