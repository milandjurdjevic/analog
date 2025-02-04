open System
open System.IO
open System.Text.Json
open Analog
open Argu
open Microsoft.FSharp.Core
open Spectre.Console
open Spectre.Console.Json

[<RequireQualifiedAccess>]
type Command =
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
    |> Option.map Log.createGrok
    |> Option.defaultValue (Log.defaultGrok |> Result.Ok)
    |> Result.bind (Log.parseGrok text)

let filter expression entries =
    expression
    |> Option.map Filter.parse
    |> Option.map (fun res ->
        res
        |> Result.bind (fun filter -> entries |> Result.map (fun entries -> filter, entries)))
    |> Option.map (fun result ->
        result
        |> Result.map (fun (filter, entries) ->
            entries |> List.filter (fun entry -> Filter.evaluate entry filter)))
    |> Option.defaultValue entries

let handle (args: ParseResults<Command>) =
    import (args.GetResults Command.File)
    |> parse (args.TryGetResult Command.Pattern)
    |> filter (args.TryGetResult Command.Filter)

let normalize (entry: Log.Entry) =
    entry
    |> Map.map (fun _ value ->
        match value with
        | Log.StringLiteral value -> box value
        | Log.NumberLiteral value -> box value
        | Log.BooleanLiteral value -> box value
        | Log.TimestampLiteral value -> box value)

let print entries =
    entries
    |> List.map normalize
    |> JsonSerializer.Serialize
    |> JsonText
    |> AnsiConsole.Write

let args =
    try
        ArgumentParser
            .Create<Command>()
            .ParseCommandLine(Environment.GetCommandLineArgs() |> Array.skip 1)
        |> Result.Ok
    with err ->
        Result.Error err.Message

match args |> Result.bind handle with
| Ok entries -> print entries
| Error error -> error |> eprintf "%s"
