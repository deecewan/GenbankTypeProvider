module GenbankTypeProvider.Helpers

open System
open System.IO
open System.Net
open GenbankTypeProvider

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

let BaseFile = {
  name = "";
  variant = Directory;
  location = "ftp://ftp.ncbi.nlm.nih.gov/genomes/genbank/";
}

let filenamesFromDirectories (parent: FTPFileItem) (items: List<string []>) =
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
  let req = WebRequest.Create(url)
  req.Method <- WebRequestMethods.Ftp.DownloadFile
  use res = req.GetResponse() :?> FtpWebResponse
  use stream = res.GetResponseStream()
  use reader = new StreamReader(stream)
  reader.ReadToEnd()

let loadDirectoryFromFTP (item: FTPFileItem) =
  logger.Log(sprintf "Making request to %s" item.location)
  // Inspiration from https://github.com/dsyme/FtpTypeProviderExample
  let req = WebRequest.Create(item.location)
  req.Method <- WebRequestMethods.Ftp.ListDirectoryDetails
  use res = req.GetResponse() :?> FtpWebResponse
  use stream = res.GetResponseStream()
  use reader = new StreamReader(stream)
  let results = [
    while not reader.EndOfStream do
      yield reader.ReadLine().Split([| ' '; '\t' |], StringSplitOptions.RemoveEmptyEntries)
  ]
  filenamesFromDirectories(item)(results)

let getLatestAssemblyFor (genome: FTPFileItem) =
  let latestItem = genome.childDirectory("latest_assembly_versions")
  // in the latest assembly location, there should only ever be one item
  let items = latestItem |> loadDirectoryFromFTP
  let length = List.length(items)
  if length = 0 then
    (sprintf "Couldn't get latest assembly for %A at %s" genome latestItem.location) |> failwith
  if length > 1 then
    logger.Warn(sprintf "Got multiple items in latest assembly. Count: %d" length)
    Seq.map(fun (c: FTPFileItem) -> logger.Log(sprintf "%A" c))(items) |> ignore
  let item = Seq.item(0)(items)

  // these are the only files we're actually interested in here
  [
    item.childFile("annotation_hashes.txt")
    item.childFile(item.name + "_genomic_gbff.gz")
  ]

let getChildDirectories (item: FTPFileItem) =
  logger.Log(sprintf "Loading from URL: %s" item.location)
  loadDirectoryFromFTP(item)
  |> List.filter(fun f -> match f.variant with
                          | Directory -> true
                          | _ -> false
  )

let loadGenomesForVariant (variant: FTPFileItem) =
  variant |> getChildDirectories

let loadGenomeVariants () =
  getChildDirectories(BaseFile)
  // [{ name = "other"; location = "ftp://ftp.ncbi.nlm.nih.gov/genomes/genbank/other"; variant = Directory }]
