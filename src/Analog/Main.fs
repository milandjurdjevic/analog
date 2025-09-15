open System
open System.IO
open System.Text
open System.Text.Json

open Analog.Logs
open Analog.Filters
open Analog.Args

match Environment.GetCommandLineArgs() |> Array.skip 1 |> Arg.parse with
| Ok arg ->
    let filter =
        arg.Filter
        |> Option.bind FilterParser.tryCreateFilterExpression
        |> Option.map FilterEvaluator.eval
        |> Option.defaultValue (fun _ -> true)

    let extract =
        match arg.Pattern with
        | Some pattern -> LogEntryParser.list pattern
        | None -> LogEntryParser.list LogPattern.simple

    use output =
        arg.Output
        |> Option.defaultValue (Path.Combine(Directory.GetCurrentDirectory(), "analog.json"))
        |> File.Create

    arg.Inputs
    |> Seq.map (File.ReadAllText >> extract)
    |> Seq.collect (List.filter filter)
    |> Seq.map LogEntry.toObj
    |> Seq.toArray
    |> JsonSerializer.Serialize
    |> Encoding.UTF8.GetBytes
    |> output.Write

| Error error -> failwith error
