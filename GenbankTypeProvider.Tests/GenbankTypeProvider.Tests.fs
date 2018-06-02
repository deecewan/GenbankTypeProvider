module GenbankTypeProvider.Tests

open Xunit
open System.Reflection

[<Fact>]
let ``Does something`` () =
  Assert.Equal("David Buchan-Swanson", Genbank.Provider.Meta.Author)

[<Fact>]
let ``Gets correct FTP URL for dynamic child`` () =
  Assert.Equal("ftp://ftp.ncbi.nlm.nih.gov/genomes/genbank/other/", Genbank.Provider.Other.FTPUrl)

[<Fact>]
let ``Gets correct accession number for arbitarty child`` () =
  Assert.Equal("GCA_000952055.2", Genbank.Provider.``Vertebrate, Mammalian``.Genomes.Aotus_nancymaae.``Annotation Hashes``.``Assembly accession``)
