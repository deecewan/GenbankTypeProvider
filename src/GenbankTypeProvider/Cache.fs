module GenbankTypeProvider.Cache

open System.Net
open System.IO

// This is the generic interface for a cache
// If more caches need to be defined, they should conform to this same interface
type Cache =
  abstract member LoadFile: string -> string
  abstract member LoadDirectory: string -> string
  abstract member Purge: unit -> unit

let logger = Logger.createChild(Logger.logger)("Cache")

let downloadFile (url) =
  logger.Log("Downloading file from %s") url
  let req = WebRequest.Create(url)
  req.Method <- WebRequestMethods.Ftp.DownloadFile
  use res = req.GetResponse() :?> FtpWebResponse
  use stream = res.GetResponseStream()
  use reader = new StreamReader(stream)
  let data = reader.ReadToEnd()
  data

let downloadDir(url) =
    logger.Log("Downloading directory from %s") url
    // Inspiration from https://github.com/dsyme/FtpTypeProviderExample
    let req = WebRequest.Create(url)
    req.Method <- WebRequestMethods.Ftp.ListDirectoryDetails
    use res = req.GetResponse() :?> FtpWebResponse
    use stream = res.GetResponseStream()
    use reader = new StreamReader(stream)
    let data = reader.ReadToEnd()
    data

// The strategy here is pretty straight forward
// We store the URL as the cache key, and then the cache body is a record with a time and
// the data. we expire data after 2 hours (? - this can probably be much longer)
type MemoryRecord = { time: int; data: string }
type Memory() =
  let mutable storage = Map.empty;
  let timediff = System.TimeSpan.FromSeconds(10.)
  let memoryLogger = Logger.createChild(logger)("MemoryCache")
  member private this.downloadFile(url) =
    let data = downloadFile(url)
    this.persist(url, data)
    data
  member private this.downloadDir(url) =
    let data = downloadDir(url)
    this.persist(url, data)
    data
  member private this.persist(key, value) =
    let time = System.DateTime.Now.Millisecond
    storage <- storage.Add(key, { time = time; data = value })
  member private this.isValid(url) =
    memoryLogger.Log("Checking timestamp...")
    let time = storage.[url].time
    let currentTicks = System.DateTime.Now.Subtract(timediff)
    time < currentTicks.Millisecond
  member this.downloadIfRequired url downloader =
    if storage.ContainsKey(url) then
      if this.isValid(url) then
        memoryLogger.Log("Cache is invalid - reloading!")
        storage <- storage.Remove(url)
        // i only extracted out this functionality because I couldn't figure out how to
        // recursively call `this.LoadFile` after removing the key from the cache
        downloader(url)
      else
        memoryLogger.Log("Loading data from cache!")
        storage.[url].data
    else
      memoryLogger.Log("Loading data from the internet!")
      downloader(url)
  interface Cache with
    member this.LoadFile (url) =
      memoryLogger.Log("Making file request to %s") url
      this.downloadIfRequired(url)(this.downloadFile)
    member this.LoadDirectory (url) =
      memoryLogger.Log("Making directory request to %s") url
      this.downloadIfRequired(url)(this.downloadDir)
    member this.Purge () =
      printfn("Purging!")
      storage <- Map.empty

type FileSystem() =
  let storagePath = Path.Combine(Path.GetTempPath(), "GenbankTypeProvider")
  let fileLogger = Logger.createChild(logger)("FileCache")

  do()
    Directory.CreateDirectory(storagePath) |> ignore
    fileLogger.Log("Saving cache to %s") storagePath

  member private this.getPathForUrl (url: string) =
    let url = url |> String.map(fun c -> if Seq.exists((=)c)(":/") then '-' else c)
    String.concat(".")([Path.Combine(storagePath, url); "txt"])
  member private this.loadCacheFile(url) =
    let path = this.getPathForUrl(url);
    fileLogger.Log("Loading cache from path %s") path
    if File.Exists(path) then Some(File.ReadAllText(path)) else None
  member private this.saveCacheFile url data=
    let path = this.getPathForUrl(url);
    fileLogger.Log("Saving cache to path %s") path
    File.WriteAllText(path, data);
    data
  interface Cache with
    member this.LoadFile (url) =
      match this.loadCacheFile(url) with
      | Some(data) ->
        fileLogger.Log("Cache hit for file %s") url
        data
      | None ->
        fileLogger.Log("Cache miss for file %s") url
        downloadFile(url) |> this.saveCacheFile(url)
    member this.LoadDirectory (url) =
      match this.loadCacheFile(url) with
      | Some(data) -> 
        fileLogger.Log("Cache hit for directory %s") url
        data
      | None ->
        fileLogger.Log("Cache miss for directory %s") url
        downloadDir(url) |> this.saveCacheFile(url)
    member this.Purge() =
      fileLogger.Log("Purging cache")
      this.saveCacheFile("") |> ignore

let cache: Cache = FileSystem() :> Cache
