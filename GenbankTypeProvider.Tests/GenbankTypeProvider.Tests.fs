(**
  * Welcome to some of the most brittle tests in history.
  * These will routinely break every three months, as a new Genbank release is
  * made.
  *)
module GenbankTypeProvider.Tests

open Xunit

[<Fact>]
let ``Has a Meta property with Author details`` () =
  Assert.Equal("David Buchan-Swanson", Genbank.Provider.Meta.Author)

[<Fact>]
let ``Gets correct FTP URL for dynamic child`` () =
  Assert.Equal("ftp://ftp.ncbi.nlm.nih.gov/genomes/genbank/other/", Genbank.Provider.Other.FTPUrl)

[<Fact>]
let ``Gets correct accession number for arbitarty child`` () =
  Assert.Equal("GCA_000952055.2", Genbank.Provider.``Vertebrate, Mammalian``.Genomes.Aotus_nancymaae.``Annotation Hashes``.``Assembly accession``)

[<Fact>]
let ``if there is only one assembly, attach directly to genome`` () =
  Assert.IsType(
    typeof<string>, 
    Genbank.Provider.Other.Genomes.``synthetic_bacterium_JCVI-Syn3.0``.``Annotation Hashes``.``Features hash``
  )

[<Fact>]
let ``if there is more than on assembly, attach under assembly name`` () =
  Assert.IsType(
    typeof<string>,
    Genbank.Provider.Other.Genomes.enrichment_culture.``GCA_900230615.1_oilsands_bin_006``.``Annotation Hashes``.``Features hash``
  )