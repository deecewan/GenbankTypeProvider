# Outputs

The resultant outputs of the project in its current form were the library
itself, as well as some other auxiliary libraries that were built and released
for consumption by `Genbank.Provider`.

## Genbank.Provider

The main output of the project. The result at submission was a provider that
still had minor problems with dependency management, but provided a small
benefit to the community. The functionality provided users with a way to query
an individual locus:

```fsharp
printfn "Locus Def: %s" (Genbank.Provider.Archaea.
  ``Acidianus - Brierleyi``.``GCA_003201835.1_ASM320183v1``.
  CP029289.Definition)
// output:
// "Locus Def: Acidianus brierleyi strain DSM 1651
// chromosome, complete genome."
```

Once the user has access to the locus directly (the `CP029289` property in the
above example), they are able to access any of the properties made available by
the `GenBankMetadata` class of `NetBio.Core`. If further community feedback were
to result in the necessity of other classes or data becoming required, it is
relatively trivial to add support for the extra information.

The more significant benefit comes from being able to map over a collection
of loci. The following example shows a trivial example of mapping over the
collection and outputting the name of each item. Again, object provided to
the `map` callback is the `GenBankMetadata` class of `NetBio.Core`, which
means that the user is free to access any information as required.

```fsharp
Genbank.Provider.Archaea.``Archaeoglobales - Archaeon``.
  ``GCA_003662925.1_ASM366292v1``.Loci
  |> Seq.length |> printfn "Number of Loci: %d"
Genbank.Provider.Archaea.``Archaeoglobales - Archaeon``.
  ``GCA_003662925.1_ASM366292v1``.Loci
  |> Seq.iter(fun x -> printfn("Value: %A") x.Locus.Name)

(*
output:
Number of Loci: 533
Value: "QMZB01000001"
Value: "QMZB01000002"
<...>
Value: "QMZB01000532"
Value: "QMZB01000533"
*)
```

At their most basic, the generated output provided functionality that would
simplify the access and use of bioinformatic data, as was the aim. There are,
however, a few significant disadvantages.

The primary downside was that, while the library was usable, and can be run in
programs using F# Interactive, there was still no intellisense in editors.
Tab-completion worked in F# Interactive, to an extent. As soon as a type with a
space was met, there was no more tab completion available. In practice, almost all
genomes had a space, so the provider was useful to find genomes but not loci or
further. 

The lack of intellisense was seemingly caused by the compiler not loading in the
correct assemblies as were required by the type provider (for dependencies, such
as the critically important `NetBio.Core`). When running in F# Interactive, the
assemblies could be loaded using the `paket`-generated load scripts, which
primed the `REPL` with the necessary `DLL`s, as shown in the following snippet:

```fsharp
#load ".paket/load/main.group.fsx"
#r "src/Genbank.RunTime/bin/Debug/netstandard2.0/Genbank.RunTime.dll"
#r "src/Genbank.DesignTime/bin/Debug/netstandard2.0/Genbank.DesignTime.dll"
```

This limitation essentially broke the promise of enabling exploration of the
data, but the full solution space was not explored, so there may still be a way
to load the necessary `DLL`s correctly.

Another limitation, as displayed in the iteration example, was that the
collection could only be used once - assigning the collection to a variable and
attempting to use it more than once would result in a runtime error. This was
caused by the streams in use - instead of the processed data being stored, the
stream itself was being accessed.

The size of some of the collections, and the size of some of the Genbank
formatted files resulted in the slow-down of Intellisense beyond what could be
considered reasonable. Taxa such as 'Bacteria' contained so many items that
processing the directory listing resulted in Visual Studio coming to a halt
while the types were generated, and and F# Interactive instance becoming
unresponsive.  Similar problems occurred with certain Genbank files, as their
size is in the hundreds of megabytes. Not only is there significant slow down,
but the memory usage of the provider can climb into the gigabytes.

## Forest

Forest is an output-agnostic logging application. It defines an interface that
outputs can implement, and allows applications to log data to any number of
outputs.  This was invaluable in the development of a type provider, as often
the outputs that are interesting cannot be seen. The log messages generated
using `printfn` or similar are swallowed by the compiler and not shown anywhere
inside of Visual Studio. The interface defined three levels: `Log`, `Warn` and
`Error`.

To get around this limitation, two output libraries were also released. These
were developed alongside `Forest` and lived in the same repository.
`Forest.FileWriter` allowed for logging to be made persistently to a file. A
program like `tail(1)` could then be used to watch the log lines as they were
generated by Visual Studio, and debugging could be done that way. Logs were
broken up per-level, as well as stored in one combined log file to see all
output in one place.

The second output library was `Forest.ConsoleWriter`. This output generated
colourful log lines using [`Texta`](#texta) depending on the level specified:
`Log` in blue, `Warn` in orange and `Error` in red. This made it easy to
distinguish between the log messages and genuine program output when running
the type provider in F# Interactive, as well as easily identify critical error
messages in amongst more verbose `Log` lines.

This library, and the two output libraries, have been released onto Nuget and
the source is available at [https://github.com/deecewan/Forest](https://github.com/deecewan/Forest). 

## Texta

Texta was a small utility library built to generate ANSI escape sequences for
colouring text. This was built due to `Colorful.Console` seemingly not working
on platforms other than windows.

The library is currently very limited, supporting only the base set of ANSI
colours, but support is planned for true-color in supported terminals.  It was
released onto Nuget, and the source is available at
[https://github.com/deecewan/Texta](https://github.com/deecewan/Texta). 