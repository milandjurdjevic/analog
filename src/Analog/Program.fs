open System
open System.IO
open System.Text.Json
open Analog
open Argu
open Spectre.Console
open Spectre.Console.Json

type Argument =
    | [<MainCommand; Mandatory; First>] File of string
    | [<AltCommandLine("-p"); Unique>] Pattern of string
    | [<AltCommandLine("-f")>] Filter of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | File _ -> "Log file path."
            | Pattern _ -> "GROK pattern."
            | Filter _ -> "Filter expression."

let import files =
    files |> List.map File.ReadAllText |> List.reduce (fun all next -> all + next)

let parse pattern text =
    pattern
    |> Option.map EntryParser.create
    |> Option.defaultValue (EntryParser.value |> Result.Ok)
    |> Result.map (EntryParser.parse text)

let filter filter entries =
    filter
    |> Option.map (ParserRunner.run FilterParser.expression)
    |> Option.map (fun res ->
        res
        |> Result.bind (fun filter -> entries |> Result.map (fun entries -> filter, entries)))
    |> Option.map (fun result ->
        result
        |> Result.map (fun (filter, entries) ->
            entries |> List.filter (fun entry -> FilterEvaluator.evaluate entry filter)))
    |> Option.defaultValue entries

let handle (args: ParseResults<Argument>) =
    import (args.GetResults Argument.File)
    |> parse (args.TryGetResult Argument.Pattern)
    |> filter (args.TryGetResult Argument.Filter)

let normalize (entry: Entry) =
    entry
    |> Map.map (fun _ value ->
        match value with
        | String value -> box value
        | Number value -> box value
        | Boolean value -> box value
        | Timestamp value -> box value)

let print entries =
    entries
    |> List.map normalize
    |> JsonSerializer.Serialize
    |> JsonText
    |> AnsiConsole.Write

let args =
    try
        ArgumentParser
            .Create<Argument>()
            .ParseCommandLine(Environment.GetCommandLineArgs() |> Array.skip 1)
        |> Result.Ok
    with err ->
        Result.Error err.Message

match args |> Result.bind handle with
| Ok entries -> print entries
| Error error -> error |> eprintf "%s"
