open System
open System.IO
open System.Text.Json
open System.Threading
open Analog.Console
open FSharp.Control
open Spectre.Console
open Spectre.Console.Json
open Spectre.Console.Rendering

let scan token stream = Scanner.Scan stream token

let writeLine (element: IRenderable) =
    AnsiConsole.Write element
    AnsiConsole.WriteLine()

let path = Environment.GetCommandLineArgs() |> Array.tryItem 1

match path with
| None -> FileStream.Null
| Some value -> File.OpenRead value
|> scan CancellationToken.None
|> TaskSeq.map JsonSerializer.Serialize
|> TaskSeq.map JsonText
|> TaskSeq.iter writeLine
|> ignore
