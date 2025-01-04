open System
open System.IO
open Analog
open Argu

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

let handle (args: ParseResults<Argument>) =
    let text =
        args.GetResults Argument.File
        |> List.map File.ReadAllText
        |> List.reduce (fun all next -> all + next)

    let entries =
        args.TryGetResult Argument.Pattern
        |> Option.map EntryParser.create
        |> Option.defaultValue (EntryParser.value |> Result.Ok)
        |> Result.map (EntryParser.parse text)

    args.TryGetResult Argument.Filter
    |> Option.map (ParserRunner.run FilterParser.expression)
    |> Option.map (fun res ->
        res
        |> Result.bind (fun filter -> entries |> Result.map (fun entries -> filter, entries)))
    |> Option.map (fun result ->
        result
        |> Result.map (fun (filter, entries) ->
            entries |> List.filter (fun entry -> FilterEvaluator.evaluate entry filter)))
    |> Option.defaultValue entries

let args =
    try
        ArgumentParser
            .Create<Argument>()
            .ParseCommandLine(Environment.GetCommandLineArgs() |> Array.skip 1)
        |> Result.Ok
    with err ->
        Result.Error err.Message

match args |> Result.bind handle with
| Ok entries -> entries.Length |> printf "%i"
| Error error -> error |> eprintf "%s"
