module GenbankTypeProvider.Helpers

open System
open System.IO
open System.Net


type FileType = Directory | File | Symlink
type FTPFileItem = {
  variant: FileType
  name: string
  location: string
} with
  member this.childFile child =
    { variant = File; name = child; location = this.location + child }
  member this.childSymlink child =
    { variant = Symlink; name = child; location = this.location + child }
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
  Console.WriteLine("Making request to {0}", item.location)
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

let getLatestAssemblyFor (item: FTPFileItem) =
  let latestItem = item.childDirectory("latest_assembly_versions")
  // in the latest assembly location, there should only ever be one item
  let items = latestItem |> loadDirectoryFromFTP
  let length = List.length(items)
  if length = 0 then
    failwith(String.Format("Couldn't get latest assembly for {0} at {1}", item, latestItem.location));
  if length > 1 then
    System.Console.WriteLine("Got multiple items in latest assembly. Count: {0}", length)
    Seq.map(fun (c: FTPFileItem) -> System.Console.WriteLine("{0}", c))(items) |> ignore
  let item = Seq.item(0)(items)

  let createFile name =
    { name = name; variant = File; location = item.location + "/" + name }

  // these are the only files we're actually interested in here
  [
    createFile("annotation_hashes.txt");
    createFile(item.name + "_genomic_gbff.gz");
  ]

let getChildDirectories (item: FTPFileItem) =
  Console.WriteLine("Loading from URL: {0}", item.location)
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
