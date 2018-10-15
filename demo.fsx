#r "./bin/GenbankTypeProvider/netstandard2.0/GenbankTypeProvider.dll"
open GenbankTypeProvider

// `time fsharpi demo.fsx`
// without cache 28.221 seconds
// with cache 3.042 seconds

let p = printfn "%s"
p Genbank.Provider.Other.Genomes.``marine_microorganism_AG-341-P21``.``Annotation Hashes``.``Assembly accession``;
p Genbank.Provider.Other.Genomes.``marine_microorganism_AG-341-P21``.``Annotation Hashes``.``Descriptors last changed``;
p Genbank.Provider.Other.Genomes.Saccharomyces_cerevisiae_synthetic_construct.``Annotation Hashes``.``Protein names hash``;

Genbank.Provider.Archaea.Genomes.Acidianus_sulfidivorans.``Annotation Hashes``.``Assembly accession``;;