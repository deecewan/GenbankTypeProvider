﻿module GenbankTypeProvider.TypeGenerators

open ProviderImplementation.ProvidedTypes
open GenbankTypeProvider
open Microsoft.FSharp.Quotations
open System
open GenbankTypeProvider.Helpers
open System.Reflection
open System.Globalization
open System.Collections.Generic
open System.Text
open System.IO
open Bio.IO.GenBank

type PropertyType = {
  name: string
  value: string
}

let logger = Logger.createChild(Logger.logger) "TypeGenerators"

let ftpUrlType (location: string) =
  ProvidedProperty("FTPUrl", typeof<string>, isStatic = true, getterCode = (fun _ -> Expr.Value(location)));

let provideSimpleValue (name: string, value: string) =
  ProvidedProperty(name, typeof<string>, isStatic = true, getterCode = fun _ -> Expr.Value(value))

let parseAnnotationHashes (location: string) =
  let file = Helpers.downloadFileFromFTP(location)
  let split =
    file
      .Substring(2) // there is a `# ` at the start of the file that I don't want
      .Split([|'\n'|], StringSplitOptions.RemoveEmptyEntries)
    |> Array.map(fun line -> line.Split([|'\t'|], StringSplitOptions.RemoveEmptyEntries))
  
  Array.map2(fun (column: string) ->
    fun (value:string) ->
      provideSimpleValue(column, value))(split.[0])(split.[1])
    |> Array.toList

let provideAnnotationHashes (file: Helpers.FTPFileItem) =
  let t = ProvidedTypeDefinition("Annotation Hashes", Some typeof<obj>)
  t.AddMembersDelayed(fun _ -> parseAnnotationHashes(file.location))
  t

let parseGenbankFile (location: string) =
  logger.Log("Location of Genbank File: %s") location
  let file = Helpers.downloadFileFromFTP(location)
  let stream =
    file
      |> Encoding.UTF8.GetBytes
      |> fun c -> new MemoryStream(c)
      |> fun c ->new Compression.GZipStream(c, Compression.CompressionMode.Decompress)
  logger.Log("Got stream: %A") stream
  Bio.IO.GenBank.GenBankParser().Parse(stream) |> Seq.cast<Bio.ISequence>
    |> Seq.map(fun item ->
      logger.Log("Parsed Genbank Data: %A") item
      let gb = item.Metadata.Item("Genbank") :?> GenBankMetadata

      logger.Log("Locus Name: %A") gb.Locus.Name

      provideSimpleValue("LocusName", gb.Locus.Name)
    )
    |> Seq.toList

let provideGenbankData (file: Helpers.FTPFileItem) =
  logger.Log("Printing Genbank File: %A") file
  parseGenbankFile(file.location)

let createTypeForAssembly (files: IDictionary<Helpers.AssemblyFile, FTPFileItem>) =
  files.Item(Helpers.GenbankData) |> provideGenbankData;

let createGenomeExplorer (genome: FTPFileItem) =
  logger.Log("exploring %A") genome
  Helpers.getLatestAssembliesFor(genome) |> List.map(fun a ->
    let t = ProvidedTypeDefinition(a.name, Some typeof<obj>) 
    t.AddMembersDelayed(fun _ -> createTypeForAssembly(a.files))
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