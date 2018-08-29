module GenbankTypeProvider.Logger

open System.IO
open System
open Colorful
open System.Drawing

type Level = | Log | Warn | Error

type Message = { logName: string; level: Level; message: string } with
  static member create(logName: string, level: Level, message: string) =
    { logName = logName; level = level; message = message }

type Target = 
  abstract member Write: string -> Level -> string -> unit

type Logger = { name: string; mutable targets: Target list } with
  member private this.write (level: Level, message) =
    let m = Message.create(this.name, level, message)
    this.targets |> List.map(fun t -> t.Write(this.name)(level)(message)) |> ignore
  member this.Log message =
    Printf.ksprintf(fun res -> this.write(Log, res)) message
  member this.Warn message =
    Printf.ksprintf(fun res -> this.write(Warn, res)) message
  member this.Error message =
    Printf.ksprintf(fun res -> this.write(Error, res)) message
  member this.addTarget (target: Target) =
    this.targets <- target :: this.targets

let create (name: string, targets: Target list) : Logger =
  { name = name; targets = targets }

let createChild (logger:Logger) (name:string) : Logger =
  create(logger.name + ":" + name, logger.targets)

let colorOptions = [
  Color.Chartreuse;
  Color.Fuchsia;
  Color.DodgerBlue;
  Color.ForestGreen;
  Color.MediumSlateBlue;
  Color.MediumVioletRed;
  Color.SpringGreen;
  Color.Yellow;
  Color.HotPink;
  Color.DarkViolet;
]

let mutable usedIndex: int = 0;
let mutable colormap: Map<string, Color> = Map.empty;

let genColorForLog str =
  let color = colorOptions.[usedIndex]
  usedIndex <- (usedIndex + 1) % colorOptions.Length
  colormap <- colormap.Add(str, color)
  color

type ConsoleWriter() =
  interface Target with
    member this.Write logName level message =
      let logColor = match colormap.TryFind(logName) with
                     | Some(color) -> color
                     | None -> genColorForLog(logName)
      let color = match level with
                  | Log -> Color.Cyan
                  | Warn -> Color.Orange
                  | Error -> Color.Red
      
      let formatter = [|
        new Formatter(sprintf("[%s]")(logName), logColor);
        new Formatter(sprintf("{%s}: %s")(level.ToString().ToUpper())(message), color);
      |]

      Console.WriteLineFormatted("{0} {1}", Color.Gray, formatter);

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
  interface Target with
    member this.Write a b c = writer(a)(b)(c)
  new(directory: string) = FileWriter(directory, true)

let cw = ConsoleWriter();
let fw = FileWriter("C:/Users/david/Logs/GenbankTypeProvider")

let logger = create("GenbankTypeProvider", [cw; fw])