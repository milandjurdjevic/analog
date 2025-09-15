open System
open System.IO
open System.Text
open System.Text.Json

open Analog.Logs
open Analog.Filters
open Analog.Args

match Environment.GetCommandLineArgs() |> Array.skip 1 |> parseArgs with
| Ok value ->
    let filter =
        value.Filter
        |> Option.bind tryCreateFilterExpression
        |> Option.map applyFilter
        |> Option.defaultValue (fun _ -> true)

    let extract _ = List.empty<LogEntry>

    use output =
        value.Output
        |> Option.defaultValue (Path.Combine(Directory.GetCurrentDirectory(), "analog.json"))
        |> File.Create

    value.Inputs
    |> Seq.map (File.ReadAllText >> extract)
    |> Seq.collect (List.filter filter)
    |> Seq.map convertLogEntryToObject
    |> Seq.toArray
    |> JsonSerializer.Serialize
    |> Encoding.UTF8.GetBytes
    |> output.Write

| Error error -> failwith error
