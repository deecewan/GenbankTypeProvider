module GenbankTypeProvider.Tests

open Xunit

[<Fact>]
let ``Does something`` () =
  Assert.Equal("David Buchan-Swanson", Genbank.Provider.Meta.Author)

[<Fact>]
let ``Loads a bacterium from the FTP service`` () =
  Genbank.Provider.Bacteria.Abditibacterium_utsteinense.FTPUrl