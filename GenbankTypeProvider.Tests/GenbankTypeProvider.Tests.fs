module GenbankTypeProvider.Tests

open Xunit

[<Fact>]
let ``Does something`` () =
  Assert.Equal("David Buchan-Swanson", Genbank.Provider.Meta.Author)

[<Fact>]
let ``Something`` () =
  Assert.Equal("", Genbank.Provider.Meta.Author)