module GenbankTypeProvider.TypeGenerators

open ProviderImplementation.ProvidedTypes
open GenbankTypeProvider
open Microsoft.FSharp.Quotations
open System
open System
open GenbankTypeProvider.Helpers
open System.Reflection
open System.Globalization
open GenbankTypeProvider

type PropertyType = {
  name: string
  value: string
}

let ftpUrlType (location: string) =
  ProvidedProperty("FTPUrl", typeof<string>, isStatic = true, getterCode = (fun _ -> Expr.Value(location)));

let provideSimpleValue (name: string, value: string) =
  ProvidedProperty(name, typeof<string>, isStatic = true, getterCode = fun _ -> Expr.Value(value))

let parseAnnotationHashes (location: string) =
  let file = Helpers.downloadFileFromFTP(location)
  let split =
    file.Split([|'\n'|], StringSplitOptions.RemoveEmptyEntries)
    |> Array.map(fun line -> line.Split([|'\t'|], StringSplitOptions.RemoveEmptyEntries))
  
  Array.map2(fun (column: string) ->
    fun (value:string) ->
      provideSimpleValue(column, value))(split.[0])(split.[1])
    |> Array.toList

let provideAnnotationHashes (file: Helpers.FTPFileItem) =
  let t = ProvidedTypeDefinition("Annotation Hashes", Some typeof<obj>)
  t.AddMembersDelayed(fun _ -> parseAnnotationHashes(file.location))
  t

let findInAssemblyDirectory (a: List<Helpers.FTPFileItem>) (s: string) =
  a |> List.find(fun c -> c.name.EndsWith(s))

let createDelayExploreGenome (genome: FTPFileItem) () =
  Console.WriteLine("exploring {0}", genome)
  let assembly = Helpers.getLatestAssemblyFor(genome)
  let find = findInAssemblyDirectory(assembly)
  let annotationHashesFile = find("annotation_hashes.txt")
  let providedAnnotationHashes = provideAnnotationHashes(annotationHashesFile)

  [providedAnnotationHashes]

let createGenomesTypes (variant: FTPFileItem) =
  Console.WriteLine("Creating genome types for {0}", variant)
  // load the files for this variant
  Helpers.loadGenomesForVariant(variant)
  |> List.map(fun genome ->
    Console.WriteLine("Loaded for genome {0}", genome)
    let genomeType = ProvidedTypeDefinition(genome.name, Some typeof<obj>)
    genomeType.AddMember(ftpUrlType(genome.location))
    genomeType.AddMembersDelayed(createDelayExploreGenome(genome));

    genomeType
  )

let textInfo = CultureInfo("en-US").TextInfo

let createGenomeVariantType (asm : Assembly, ns: string) (root: FTPFileItem) =
  System.Console.WriteLine("Creating variant {0}", root.name)
  let name = textInfo.ToTitleCase(root.name.Replace("_", ", "));
  let t = ProvidedTypeDefinition(asm, ns, name, Some typeof<obj>)
  let sub = ProvidedTypeDefinition("Genomes", Some typeof<obj>)
  sub.AddMembersDelayed(fun _ -> createGenomesTypes(root))
  t.AddMember(sub)
  t.AddMember(ftpUrlType(root.location))
  Console.WriteLine("Finished with variant {0}", root.name)
  t