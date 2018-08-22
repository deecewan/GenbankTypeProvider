module GenbankTypeProvider.Cache

open Bio.Util

// This is the generic interface for a cache
// If more caches need to be defined, they should conform to this same interface
type Cache =
  abstract member LoadFile: string -> string
  abstract member LoadDirectory: string -> unit
  abstract member Purge: unit -> unit

type Memory() =
  let mutable storage = Map.empty;
  interface Cache with
    member this.LoadFile (url) =
      if storage.ContainsKey(url) then
        printfn("Loading file from cache: %s!") url
        storage.[url]
      else
        printfn("Loading new file!") |> ignore
        let res = sprintf "%d" System.DateTime.Now.Ticks
        storage <- storage.Add(url, res)
        res
    member this.LoadDirectory (url) =
      printfn("Loading Directory!") |> ignore
    member this.Purge () =
      printfn("Purging!") |> ignore

let cache: Cache = Memory() :> Cache