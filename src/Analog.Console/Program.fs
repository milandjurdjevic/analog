open System
open System.IO
open System.Text.Json
open System.Threading
open Analog.Console
open FSharp.Control
open Spectre.Console
open Spectre.Console.Json

match (Environment.GetCommandLineArgs() |> Array.tryItem 1) with
| None -> FileStream.Null
| Some value -> File.OpenRead value
|> Log.ofStream CancellationToken.None
|> TaskSeq.map JsonSerializer.Serialize
|> TaskSeq.map JsonText
|> TaskSeq.iter (fun json ->
    AnsiConsole.Write json
    AnsiConsole.WriteLine())
|> ignore
