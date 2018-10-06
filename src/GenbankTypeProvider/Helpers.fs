module GenbankTypeProvider.Helpers

open System
open System.Net
open GenbankTypeProvider
open System.Collections.Generic
open System.IO.Compression

let logger = Logger.createChild(Logger.logger)("Helpers")

type FileType = Directory | File | Symlink
type FTPFileItem = {
  variant: FileType
  name: string
  location: string
} with
  member this.childFile child =
    { variant = File; name = child; location = this.location + child }
  member this.childSymlink child =
    { variant = Symlink; name = child; location = this.location + child + "/" }
  member this.childDirectory child =
    { variant = Directory; name = child; location = this.location + child + "/" }

type AssemblyFile = | AnnotationHashes | GenbankData

type AssemblyRecord = {
  name: string;
  files: IDictionary<AssemblyFile, FTPFileItem>;
}

let BaseFile = {
  name = "";
  variant = Directory;
  location = "ftp://ftp.ncbi.nlm.nih.gov/genomes/genbank/";
}

let filenamesFromDirectories (parent: FTPFileItem) (items: string [] list) =
  [for i in items do
    if i.Length > 1 then
      let fileType: FileType =
        if i.[0].StartsWith("d") then
          Directory
        elif i.[0].StartsWith("l") then
          Symlink
        else
          File
      yield match fileType with
            // get the symlink name, not it's location
            | Symlink -> parent.childSymlink(Seq.item(Seq.length(i) - 3)(i))
            | Directory -> parent.childDirectory(Seq.last(i))
            | File -> parent.childFile(Seq.last(i))
  ]

let downloadFileFromFTP (url: string) =
  Cache.cache.LoadFile(url)

let loadDirectoryFromFTP (item: FTPFileItem) =
  let res = Cache.cache.LoadDirectory(item.location)
  res.Split([| '\r'; '\n' |], StringSplitOptions.RemoveEmptyEntries)
  |> Seq.toList
  |> List.map(fun s -> s.Split([| ' '; '\t'; '\n' |], StringSplitOptions.RemoveEmptyEntries))
  |> filenamesFromDirectories(item)

let getAssemblyDetails (item: FTPFileItem) =
  // these are the only files we're actually interested in here
  {
    name = item.name;
    files =
      dict [
        AnnotationHashes, item.childFile("annotation_hashes.txt");
        GenbankData, item.childFile(item.name + "_genomic.gbff.gz");
      ]
  }

let getLatestAssembliesFor (genome: FTPFileItem) =
  let latestItem = genome.childDirectory("latest_assembly_versions")
  // in the latest assembly location, there should only ever be one item
  let items = latestItem |> loadDirectoryFromFTP
  let length = List.length(items)
  if length = 0 then
    let message = sprintf "Couldn't get latest assembly for %A at %s" genome latestItem.location
    logger.Error("%s") message
    message |> failwith

  items |> List.map getAssemblyDetails

let getChildDirectories (item: FTPFileItem) =
  logger.Log("Loading from URL: %s") item.location
  loadDirectoryFromFTP(item)
  |> List.filter(fun f -> match f.variant with
                          | Directory -> true
                          | _ -> false
  )

let loadGenomesForVariant (variant: FTPFileItem) =
  variant |> getChildDirectories

let loadGenomeVariants () =
  getChildDirectories(BaseFile)