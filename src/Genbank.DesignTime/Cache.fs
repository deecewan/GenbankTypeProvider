module GenbankTypeProvider.Cache

open System.Net
open System.IO

// This is the generic interface for a cache
// If more caches need to be defined, they should conform to this same interface
type Cache =
  abstract member LoadFile: string -> Stream
  abstract member LoadDirectory: string -> Stream
  abstract member Purge: unit -> unit

let logger = Logger.createChild(Logger.logger)("Cache")

let downloadFile (url) =
  logger.Log("Downloading file from %s") url
  let req = WebRequest.Create(url)
  req.Method <- WebRequestMethods.Ftp.DownloadFile
  let res = req.GetResponse() :?> FtpWebResponse
  res.GetResponseStream()

let downloadDir(url) =
    logger.Log("Downloading directory from %s") url
    // Inspiration from https://github.com/dsyme/FtpTypeProviderExample
    let req = WebRequest.Create(url)
    req.Method <- WebRequestMethods.Ftp.ListDirectoryDetails
    let res = req.GetResponse() :?> FtpWebResponse
    res.GetResponseStream()

type FileSystem() =
  let storagePath = Path.Combine(Path.GetTempPath(), "GenbankTypeProvider")
  let fileLogger = Logger.createChild(logger)("FileCache")

  do()
    Directory.CreateDirectory(storagePath) |> ignore
    fileLogger.Log("Saving cache to %s") storagePath

  member private this.getPathForUrl (url: string) =
    let url = url |> String.map(fun c -> if Seq.exists((=)c)(":/") then '-' else c)
    Path.Combine(storagePath, url);

  member private this.loadCacheFile(url) =
    let path = this.getPathForUrl(url);
    fileLogger.Log("Loading cache from path %s") path
    if File.Exists(path) then Some(File.OpenRead(path)) else None

  member private this.saveCacheFile url (data: Stream) =
    let path = this.getPathForUrl(url);
    fileLogger.Log("Saving cache to path %s") path
    let output = File.OpenWrite(path)
    data.CopyTo(output)
    output.Close()
    data.Close()
    File.OpenRead(path)

  interface Cache with
    member this.LoadFile (url) =
      let result = match this.loadCacheFile(url) with
                   | Some(data) ->
                     fileLogger.Log("Cache hit for file %s") url
                     data
                   | None ->
                     fileLogger.Log("Cache miss for file %s") url
                     downloadFile(url) |> this.saveCacheFile(url)
      result :> Stream
    member this.LoadDirectory (url) =
      let result = match this.loadCacheFile(url) with
                   | Some(data) -> 
                     fileLogger.Log("Cache hit for directory %s") url
                     data
                   | None ->
                     fileLogger.Log("Cache miss for directory %s") url
                     downloadDir(url) |> this.saveCacheFile(url)
      result :> Stream
    member this.Purge() =
      fileLogger.Log("Purging cache")
      this.saveCacheFile("") |> ignore

let cache: Cache = FileSystem() :> Cache
