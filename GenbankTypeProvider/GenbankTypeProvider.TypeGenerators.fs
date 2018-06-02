module GenbankTypeProvider.TypeGenerators

open ProviderImplementation.ProvidedTypes
open GenbankTypeProvider
open Microsoft.FSharp.Quotations
open System
open System

type PropertyType = {
  name: string
  value: string
}

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

let createDelayExploreBacterium (bacterium: string) () =
  let assembly = Helpers.getLatestAssemblyFor(bacterium)
  let find = findInAssemblyDirectory(assembly)
  let annotationHashesFile = find("annotation_hashes.txt")
  let providedAnnotationHashes = provideAnnotationHashes(annotationHashesFile)

  [providedAnnotationHashes]

let createBacteriumType (bacterium: string) =
  // create a base type here
  // attach the url to it
  // attach a delayed member generator to load the extra pieces
  let bacteriumType = ProvidedTypeDefinition(bacterium, Some typeof<obj>)
  bacteriumType.AddMember(provideSimpleValue("FTPUrl", Helpers.urlForBacteria(bacterium)))
  bacteriumType.AddMember(
    ProvidedProperty("FTPUrl", typeof<string>, isStatic = true, getterCode = (fun _ -> Expr.Value(Helpers.urlForBacteria bacterium)));
  )
  bacteriumType.AddMembersDelayed(createDelayExploreBacterium(bacterium))
  bacteriumType

let createBacteriaTypes =
  // we would usually load the bacteria
  // from FTP, but I don't have a good solution to do that yet
  // because it takes a long, long time to download them all
  (* let bacteria = Helpers.loadBacteriaFromFTP *)
  // we'll use these bacteria as test cases
  let bacteria = ["Abditibacterium_utsteinense"; "Abiotrophia_defectiva"; "Abiotrophia_sp._HMSC24B09";]

  bacteria |> List.map(createBacteriumType)
