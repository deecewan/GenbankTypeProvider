module GenbankTypeProvider.TypeGenerators

open ProviderImplementation.ProvidedTypes
open GenbankTypeProvider
open Microsoft.FSharp.Quotations
open GenbankTypeProvider.Helpers
open System.Reflection
open System.Globalization
open System.IO
open Bio.IO.GenBank

type PropertyType = {
  name: string
  value: string
}

let logger = Logger.createChild(Logger.logger) "TypeGenerators"

let ftpUrlType (location: string) =
  let t = ProvidedProperty("FTPUrl", typeof<string>, isStatic = true, getterCode = (fun _ -> Expr.Value(location)));
  t.AddXmlDoc("The URL where this item was retrieved from.")
  t

let provideSimpleValue (name: string, value: string) =
  ProvidedProperty(name, typeof<string>, isStatic = true, getterCode = fun _ -> Expr.Value(value))

let loadGenbankFile (location: string) callback =
  logger.Log("Location of Genbank File: %s") location
  use file = Helpers.downloadFileFromFTP(location)
  use stream = file |> fun c -> new Compression.GZipStream(c, Compression.CompressionMode.Decompress)
  let s = Bio.IO.GenBank.GenBankParser().Parse(stream) |> Seq.cast<Bio.ISequence>
  callback s

let createTypeForAssembly (file: Helpers.FTPFileItem) =
  logger.Log("Creating types for assembly: %A") file
  loadGenbankFile(file.location)(fun genbankFile ->
    genbankFile |> Seq.map(fun item ->
      let gb = item.Metadata.Item("GenBank") :?> GenBankMetadata
      let t = ProvidedTypeDefinition(gb.Locus.Name, Some typeof<obj>)
      t.AddMembers([
        provideSimpleValue("LocusName", gb.Locus.Name);
        provideSimpleValue("AccessionPrimary", gb.Accession.Primary);
        provideSimpleValue("BaseCount", gb.BaseCount);
        provideSimpleValue("Origin", gb.Origin);
        provideSimpleValue("SourceCommonName", gb.Source.CommonName);
        provideSimpleValue("Version", gb.Version.Version);
        provideSimpleValue("Contig", gb.Contig);
        provideSimpleValue("DbSource", gb.DbSource);
      ])
      t
    ) |> Seq.toList
  )

let createDocumentationForAssembly (file: Helpers.FTPFileItem) =
  logger.Log("Creating types for assembly: %A") file
  loadGenbankFile(file.location)(fun genbankFile ->
    genbankFile |> Seq.map(fun item ->
      let gb = item.Metadata.Item("GenBank") :?> GenBankMetadata
      gb.Definition
    ) |> String.concat("\n")
  )

let createGenomeExplorer (genome: FTPFileItem) =
  logger.Log("exploring %A") genome
  let latestAssemblies = Helpers.getLatestAssembliesFor(genome)
  latestAssemblies |> List.map(fun a ->
    let t = ProvidedTypeDefinition(a.name, Some typeof<obj>)
    let genbankFile = a.files.Item(Helpers.GenbankData)
    t.AddXmlDocDelayed(fun _ -> createDocumentationForAssembly(genbankFile))
    t.AddMembersDelayed(fun _ -> createTypeForAssembly(genbankFile))
    t
  )

let createGenomesTypes (variant: FTPFileItem) =
  logger.Log("Creating genome types for %A") variant
  // load the files for this variant
  Helpers.loadGenomesForVariant(variant)
  |> List.map(fun genome ->
    logger.Log("Loaded for genome %A") genome
    let genomeType = ProvidedTypeDefinition(genome.name, Some typeof<obj>)
    genomeType.AddMember(ftpUrlType(genome.location))
    genomeType.AddMembersDelayed(fun _ -> createGenomeExplorer(genome));

    genomeType
  )

let textInfo = CultureInfo("en-US").TextInfo

let createGenomeVariantType (asm : Assembly, ns: string) (root: FTPFileItem) =
  logger.Log("Creating variant %A") root
  let name = textInfo.ToTitleCase(root.name.Replace("_", ", "));
  let t = ProvidedTypeDefinition(asm, ns, name, Some typeof<obj>)
  let sub = ProvidedTypeDefinition("Genomes", Some typeof<obj>)
  sub.AddMembersDelayed(fun _ -> createGenomesTypes(root))
  t.AddMember(sub)
  t.AddMember(ftpUrlType(root.location))
  logger.Log("Finished with variant %A") root
  t