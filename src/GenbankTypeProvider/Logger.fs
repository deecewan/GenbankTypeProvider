module GenbankTypeProvider.Logger

open System.IO
open System

type Level = | Log | Warn | Error

type Message = { logName: string; level: Level; message: string } with
  static member Create(logName: string, level: Level, message: string) =
    { logName = logName; level = level; message = message }

type ITarget =
  abstract member Write: string -> Level -> string -> unit

type Logger = { name: string; mutable targets: ITarget list } with
  member private this.Write (level: Level, message) =
    let m = Message.Create(this.name, level, message)
    this.targets |> List.map(fun t -> t.Write(this.name)(level)(message)) |> ignore
  member this.Log message =
    Printf.ksprintf(fun res -> this.Write(Log, res)) message
  member this.Warn message =
    Printf.ksprintf(fun res -> this.Write(Warn, res)) message
  member this.Error message =
    Printf.ksprintf(fun res -> this.Write(Error, res)) message
  member this.AddTarget (target: ITarget) =
    this.targets <- target :: this.targets

let create (name: string, targets: ITarget list) : Logger =
  { name = name; targets = targets }

let createChild (logger:Logger) (name:string) : Logger =
  create(logger.name + ":" + name, logger.targets)

type FileWriter(directory: string, combinedLog: bool) =
  // TODO: Is it better to just keep 1 stream open always and write to it?
  do()
    Directory.CreateDirectory(directory) |> ignore
  let getFileName level =
    Path.Combine(directory, level.ToString().ToLower()) + ".log"
  // separate these so we don't have to do the if check on every log message
  // It'd be awesome if F# supports short-hand destructuring like JS
  let writeStandard logName level message =
    File.AppendAllLines(getFileName(level), [|sprintf "%A -- [%s] {%s}" DateTime.Now logName message|])
  let writeCombined logName level message =
    writeStandard(logName)(level)(message) // write to the standard log, too
    File.AppendAllLines(
      getFileName("combined"),
      [|sprintf("%A -- [%s] %s: %s")(DateTime.Now)(level.ToString().ToUpper())(logName)(message)|]
    )

  let writer = if combinedLog then writeCombined else writeStandard
  interface ITarget with
    member this.Write a b c = writer(a)(b)(c)
  new(directory: string) = FileWriter(directory, true)

let fw = FileWriter("/Users/david/Logs/GenbankTypeProvider")
let logger = create("GenbankTypeProvider", [fw])
