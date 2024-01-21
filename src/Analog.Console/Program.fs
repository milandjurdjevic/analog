open System
open System.IO
open System.Text.Json
open System.Threading
open Analog.Console
open FSharp.Control
open Spectre.Console
open Spectre.Console.Json
open Spectre.Console.Rendering

let writeLine (element: IRenderable) =
    AnsiConsole.Write element
    AnsiConsole.WriteLine()

match (Environment.GetCommandLineArgs() |> Array.tryItem 1) with
| None -> FileStream.Null
| Some value -> File.OpenRead value
|> Log.ofStream CancellationToken.None
|> TaskSeq.map JsonSerializer.Serialize
|> TaskSeq.map JsonText
|> TaskSeq.iter writeLine
|> ignore
