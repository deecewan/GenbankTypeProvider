module GenbankTypeProvider.Logger

open System.IO
open System
open Colorful
open System.Drawing
open Colorful

type Level = | Log | Warn | Error

type Message = { logName: string; level: Level; message: string } with
  static member create(logName: string, level: Level, message: string) =
    { logName = logName; level = level; message = message }

type Target = 
  abstract member Write: Message -> unit

type Logger = { name: string; mutable targets: Target list } with
  member private this.write (level: Level, message: string) =
    let m = Message.create(this.name, level, message)
    this.targets |> List.map(fun t -> t.Write m) |> ignore
  member this.Log (message: string) =
    this.write(Log, message)
  member this.Warn (message: string) =
    this.write(Warn, message)
  member this.Error (message: string) =
    this.write(Error, message)
  member this.addTarget (target: Target) =
    this.targets <- target :: this.targets

let create (name: string, targets: Target list) : Logger =
  { name = name; targets = targets }

let createChild (logger:Logger) (name:string) : Logger =
  create(logger.name + ":" + name, logger.targets)

type ConsoleWriter() =
  interface Target with
    member this.Write ({ logName = logName; level = level; message = message; }) =
      let color = match level with
                  | Log -> Color.Cyan
                  | Warn -> Color.Orange
                  | Error -> Color.Red

      let formatted = sprintf("[%s] {%s}: %s")(logName)(level.ToString().ToUpper())(message)
      Console.WriteLine(formatted, color)

type FileWriter(directory: string, combinedLog: bool) =
  // TODO: Is it better to just keep 1 stream open always and write to it?
  do()
    Directory.CreateDirectory(directory) |> ignore
  let getFileName level =
    Path.Combine(directory, level.ToString().ToLower()) + ".log"
  // separate these so we don't have to do the if check on every log message
  let writeStandard ({ logName = logName; level = level; message = message; }) =
    File.AppendAllLines(getFileName(level), [|sprintf "[%s] {%s}" logName message|])
  let writeCombined message =
    writeStandard message
    File.AppendAllLines(
      getFileName("combined"),
      [|sprintf("%A -- [%s] %s: %s")(DateTime.Now)(message.logName)(message.level.ToString().ToUpper())(message.message)|]
    )
  
  let writer = if combinedLog then writeCombined else writeStandard
  interface Target with
    member this.Write (message) = writer(message)
  new(directory: string) = FileWriter(directory, true)

let cw = ConsoleWriter();
// let fw = FileWriter("/Users/david/Logs/GenbankTypeProvider")
let logger = create("GenbankTypeProvider", [cw])