module GenbankTypeProvider.Helpers

open System
open System.IO
open System.Net
open System
open System.Security.Policy

let GENOME_BASE_URL = "ftp://ftp.ncbi.nlm.nih.gov/genomes/genbank/bacteria";

let urlForBacteria name = String.concat("/")([GENOME_BASE_URL; name])

let latestAssemblyURL name = String.concat("/")([name |> urlForBacteria; "latest_assembly_versions"])

type FileType = Directory | File | Symlink
type FTPFileItem = {
  variant: FileType
  name: string
  location: string
}

let filenamesFromDirectories (url: string) (items: List<string []>) =
  [for i in items do
    if i.Length > 1 then
      let fileType: FileType =
        if i.[0].StartsWith("d") then 
          Directory
        elif i.[0].StartsWith("l") then
          Symlink
        else
          File
      let name = match fileType with
                 // get the symlink name, not it's location
                 | Symlink -> Seq.item(Seq.length(i) - 3)(i)
                 | File | Directory -> Seq.last(i)
      yield {
        variant = fileType;
        name = name;
        location = url + "/" + name;
      }
  ]

let downloadFileFromFTP (url: string) =
  let req = WebRequest.Create(url)
  req.Method <- WebRequestMethods.Ftp.DownloadFile
  use res = req.GetResponse() :?> FtpWebResponse
  use stream = res.GetResponseStream()
  use reader = new StreamReader(stream)
  reader.ReadToEnd()

let loadDirectoryFromFTP (url: string) =
  // Inspiration from https://github.com/dsyme/FtpTypeProviderExample
  let req = WebRequest.Create(url)
  req.Method <- WebRequestMethods.Ftp.ListDirectoryDetails
  use res = req.GetResponse() :?> FtpWebResponse
  use stream = res.GetResponseStream()
  use reader = new StreamReader(stream)
  let results = [
    while not reader.EndOfStream do
      yield reader.ReadLine().Split([| ' '; '\t' |], StringSplitOptions.RemoveEmptyEntries)
  ]
  filenamesFromDirectories(url)(results)

let getLatestAssemblyFor (bacterium: string) =
  let url = latestAssemblyURL(bacterium)
  // in the latest assembly location, there should only ever be one item
  let items = url |> loadDirectoryFromFTP
  let length = List.length(items)
  if length = 0 then
    failwith("Couldn't get latest assembly for " + bacterium)
  if length > 1 then
    System.Console.WriteLine("Got multiple items in latest assembly. Count: {0}", length)
    Seq.map(fun (c: FTPFileItem) -> System.Console.WriteLine("{0}", c))(items) |> ignore
  let item = Seq.item(0)(items)
  
  (url + "/" + item.name) |> loadDirectoryFromFTP

let loadBacteriaFromFTP () =
  loadDirectoryFromFTP(GENOME_BASE_URL)
  |> List.filter(fun f -> match f.variant with
                          | Directory -> true
                          | _ -> false
  )